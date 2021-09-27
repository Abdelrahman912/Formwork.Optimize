using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static CSharp.Functional.Extensions.OptionExtension;
using static FormworkOptimize.Core.Constants.RevitBase;
using static FormworkOptimize.Core.Comparers.Comparers;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.DTOS.Genetic;

namespace FormworkOptimize.Core.Helpers.RevitHelper
{
    public static class RevitHelper
    {

        public static CostGeneticResultInput UpdateCostInputWithNewRevitInput(this CostGeneticResultInput old , RevitFloorInput newRevitInput)
        {
            return new CostGeneticResultInput(old.CostFunc, newRevitInput, old.BoundaryLinesOffest, old.BeamsOffset, old.TimeLine, old.ManPowerCost, old.EquipmentCost, old.TransportationCost, old.FloorArea, old.PlywoodFunc);
        }

        public static RevitFloorInput UpdateWithNewXYZ(this RevitFloorInput old , XYZ newDir)
        {
          return  new RevitFloorInput(old.ConcreteFloor,
                old.Columns, old.Beams, old.HostLevel, old.HostFloorOffset, old.FloorClearHeight, newDir, old.AdjustLayout);
        }


        public static List<DeckingRectangle> ShiftAwayFromLine(this List<DeckingRectangle> rects, Line  line , List<double> database)
        {
            var firstRect = rects.First();
            var intersectingLine = firstRect.Lines.First(l => l.GetIntersectionPoints(line).Count() > 0);
            var intersectingLength = intersectingLine.Length;
            var candidateLengths = database.Where(len => len < intersectingLength)
                                           .OrderByDescending(len=>len)
                                           .ToList();
            if (candidateLengths.Count == 0)
                return new List<DeckingRectangle>();
            var lengthRectTuple = candidateLengths.Select(len => Tuple.Create(len, firstRect.AsNewRectangleAwayFromLine(line, len)))
                                                         .FirstOrDefault(tuple => !line.IsLineIntersectWithRectangle(tuple.Item2));

           if (lengthRectTuple is null)
                return new List<DeckingRectangle>();

           return rects.Select(rect=>rect.AsNewRectangleAwayFromLine(line,lengthRectTuple.Item1)).ToList();
        }


        public static (List<FormworkRectangle> InsidePolygon, 
                       List<FormworkRectangle> IntersectWithEdges) CategorizeWithRespectToPloygon(this IEnumerable<FormworkRectangle> allRects,
                                                                                                      List<Line> polygon)

        {
            var polyPoints = polygon.ToPoints();
            return allRects.Aggregate((new List<FormworkRectangle>(), new List<FormworkRectangle>()), (soFar, current) =>
            {
                (var inside, var intersect) = soFar;
                if (current.Points.IsPolygonInside(polyPoints))
                {
                    inside.Add(current);
                }
                else
                {
                  var points =   current.DoBooleanIntersectWith(polyPoints)
                                        .Distinct(XYZComparer);
                    if(points.Count() == 4 )
                        intersect.Add(current);
                }
                return soFar;
            });
        }

        public static CurveArray ToCurveArray(this FormworkRectangle rect)
        {
            return rect.Lines.Aggregate(new CurveArray(), (soFar, current) =>
             {
                 soFar.Append(current);
                 return soFar;
             });
        }

        /// <summary>
        /// Offset a rectangle by a given distance.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <returns></returns>
        public static FormworkRectangle Offset(this FormworkRectangle rect, double offsetX, double offsetY)
        {
            var totalOffsetXFromCenter = (rect.LengthX / 2) + offsetX;
            var totalOffsetYFromCenter = (rect.LengthY / 2) + offsetY;
            var center = rect.Center;
            var p0 = center - totalOffsetXFromCenter * XYZ.BasisX - totalOffsetYFromCenter * XYZ.BasisY;
            var p1 = center - totalOffsetXFromCenter * XYZ.BasisX + totalOffsetYFromCenter * XYZ.BasisY;
            var p2 = center + totalOffsetXFromCenter * XYZ.BasisX + totalOffsetYFromCenter * XYZ.BasisY;
            var p3 = center + totalOffsetXFromCenter * XYZ.BasisX - totalOffsetYFromCenter * XYZ.BasisY;
            return new FormworkRectangle(p0, p1, p2, p3);
        }

        public static FormworkRectangle Offset(this FormworkRectangle rect, double offset) =>
            rect.Offset(offset, offset);

        /// <summary>
        /// Convert ConcreteBeam to ForworkRectangle.
        /// </summary>
        /// <param name="beam"></param>
        /// <returns></returns>
        public static FormworkRectangle ToRectangle(this ConcreteBeam beam) =>
            beam.ToRectangle(beam.B);

        public static FormworkRectangle ToRectangle(this ConcreteBeam beam, double rectWidth)
        {
            var line = beam.BeamLine;
            var prepVec = line.Direction.CrossProduct(XYZ.BasisZ);
            var startPoint = line.GetEndPoint(0);
            var endPoint = line.GetEndPoint(1);
            var p0 = startPoint + (rectWidth / 2 * prepVec);
            var p1 = startPoint - (rectWidth / 2 * prepVec);
            var p2 = endPoint - (rectWidth / 2 * prepVec);
            var p3 = endPoint + (rectWidth / 2 * prepVec);
            return new FormworkRectangle(p0, p1, p2, p3);
        }

        public static ConcreteBeam UpdateWithNewLine(this ConcreteBeam beam, Line line) =>
             new ConcreteBeam(beam.B, beam.H, line, beam.ClearHeight);

        public static List<ConcreteBeam> GetBeamsWithClearSpan(this List<ConcreteBeam> beams, List<ConcreteColumn> columns)
        {
            var columnsRects = columns.Select(c => c.CornerPoints)
                                      .ToList();
            var offset = BEAM_LINE_OFFSET_FROM_COLUMN.CmToFeet();
            return beams.Select(b =>
             {
                 var sp = b.BeamLine.GetEndPoint(0).CopyWithNewZ(0);
                 var ep = b.BeamLine.GetEndPoint(1).CopyWithNewZ(0);
                 var uVec = (ep - sp).Normalize();
                 var startColumn = columnsRects.FirstOrDefault(c => c.Center.IsEqual(sp));
                 var endColumn = columnsRects.FirstOrDefault(c => c.Center.IsEqual(ep));
                 var newSp = startColumn == null ? sp : sp + uVec * (offset + startColumn.Lines.First(l => l.Direction.IsParallelTo(b.BeamLine.Direction)).Length / 2);
                 var newEp = endColumn == null ? ep : ep - uVec * (offset + endColumn.Lines.First(l => l.Direction.IsParallelTo(b.BeamLine.Direction)).Length / 2);
                 var newBeamLine = Line.CreateBound(newSp, newEp);
                 return b.UpdateWithNewLine(newBeamLine);
             }).ToList();
        }

        public static Option<DeckingRectangle> GetFittestIfNot(this DeckingRectangle rect, int index, Line beamLine, XYZ divVec, List<DeckingRectangle> fitRects, double mainDirDist, double secDirDist, List<double> database)
        {
            Func<double, DeckingRectangle> toRect = len =>
           {
               var origin = beamLine.Origin;
               var prepVec = XYZ.BasisZ.CrossProduct(beamLine.Direction);
               var startPoint = origin + index * mainDirDist * beamLine.Direction;
               var endPoint = startPoint + len * divVec;
               var p0 = startPoint + (secDirDist / 2) * prepVec;
               var p1 = startPoint - (secDirDist / 2) * prepVec;
               var p2 = endPoint - (secDirDist / 2) * prepVec;
               var p3 = endPoint + (secDirDist / 2) * prepVec;
               return new DeckingRectangle(new List<XYZ>() { p0, p1, p2, p3 }, beamLine.Direction);
           };
            var isIntersect = fitRects.Any(fRec => rect.Offset(8.0.CmToFeet()).IsBooleanIntersectWith(fRec));
            if (isIntersect)
            {
                var fittestRect = database.Where(len => len < rect.Lines.Where(l => l.Direction.IsParallelTo(beamLine.Direction)).First().Length)
                                          .OrderByDescending(len => len)
                                          .Select(toRect)
                                          .FirstOrDefault(candRect => fitRects.TrueForAll(fRec => !candRect.Offset(8.0.CmToFeet()).IsBooleanIntersectWith(fRec)));
                if (fittestRect == null)
                    return None;
                return fittestRect;
            }
            else
            {
                return rect;
            }
        }

        public static List<Tuple<Line, List<DeckingRectangle>>> Divide(this List<ConcreteBeam> beams, double mainDirDist, double secDirDist, List<double> database)
        {
            var distinctRects = beams.Select(beam => Tuple.Create(beam.BeamLine, beam.BeamLine.Divide(mainDirDist, secDirDist, database)))
                                     .Aggregate(new List<Tuple<Line, List<DeckingRectangle>>>(), (soFar, current) =>
                                     {
                                         (var beamLine, var beamRects) = current;
                                         var soFarDistinctRects = soFar.SelectMany(tuple => tuple.Item2).ToList();

                                         var firstRect = beamRects.First()
                                                                  .GetFittestIfNot(1, beamLine, -beamLine.Direction, soFarDistinctRects, mainDirDist, secDirDist, database);
                                         var lastRect = beamRects.Last()
                                                                 .GetFittestIfNot(beamRects.Count - 1, beamLine, beamLine.Direction, soFarDistinctRects, mainDirDist, secDirDist, database);

                                         var addedRects = new List<DeckingRectangle>();
                                         firstRect.Map(rect => { addedRects.Add(rect); return addedRects; });
                                         addedRects.AddRange(beamRects.Skip(1).Take(beamRects.Count - 2));
                                         lastRect.Map(rect => { addedRects.Add(rect); return addedRects; });

                                         soFar.Add(Tuple.Create(beamLine, addedRects));
                                         return soFar;
                                     }).ToList();
            return distinctRects;
        }

        public static List<DeckingRectangle> FilterOpeningsOptimized(this List<DeckingRectangle> rectangles, List<List<Line>> openings)
        {
            var openingsAsPoints = openings.Where(o => o.Count > 0)
                                           .Select(o => o.Select(l => l.GetEndPoint(0)).ToList())
                                           .ToList();
            var filteredRects = rectangles.Where(rect => openingsAsPoints.TrueForAll(o => !(rect.IsBooleanIntersectWith(o) || rect.Points.IsPolygonInside(o))))
                                          .ToList();
            return filteredRects;
        }

        public static List<DeckingRectangle> FilterOpenings(this List<DeckingRectangle> rectangles, List<List<Line>> openings)
        {
            var openingsAsPoints = openings.Where(o => o.Count > 0)
                                           .Select(o => o.Select(l => l.GetEndPoint(0)).ToList())
                                           .ToList();
            var filteredRects = rectangles.Where(rect => openingsAsPoints.TrueForAll(o => !(rect.IsBooleanIntersectWith(o) || rect.Points.IsPolygonInside(o))))
                                          .ToList();
            return filteredRects;
        }

        /// <summary>
        /// Get Prependicular vector from rectangle to line.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="beam"></param>
        /// <returns></returns>
        public static XYZ RectToLinePrepVector(this FormworkRectangle rect, Line line)
        {
            var prepVec = line.Direction.CrossProduct(XYZ.BasisZ).Abs();
            var rectToBeamVec = line.GetEndPoint(0) - rect.Center;
            if (rectToBeamVec.IsZeroLength() || rectToBeamVec.IsPrepTo(prepVec))
                return prepVec;
            return rectToBeamVec.VectorProjOnto(prepVec).Normalize();
        }

        /// <summary>
        /// Create new rectangle with new length in the prependicular direction
        /// to line.
        /// </summary>
        /// <param name="oldRectangle"></param>
        /// <param name="line"></param>
        /// <param name="newLength"></param>
        /// <returns></returns>
        public static DeckingRectangle AsNewRectangleAwayFromLine(this DeckingRectangle oldRectangle, Line line, double newLength)
        {
            var rectToBeamVec = oldRectangle.RectToLinePrepVector(line);
            var collidingLength = oldRectangle.Lines.First(l => l.Direction.IsPrepTo(line.Direction)).Length;

            var newCenterPoint = oldRectangle.Center - (collidingLength / 2) * rectToBeamVec + (newLength / 2) * rectToBeamVec;

            var parallelLine = oldRectangle.Lines.First(l => l.Direction.IsParallelTo(line.Direction));
            var parallelVec = parallelLine.Direction;
            var parallelLength = parallelLine.Length;

            var p0 = newCenterPoint - (rectToBeamVec) * (newLength / 2) - (parallelVec) * (parallelLength / 2);
            var p1 = newCenterPoint - (rectToBeamVec) * (newLength / 2) + (parallelVec) * (parallelLength / 2);
            var p2 = newCenterPoint + (rectToBeamVec) * (newLength / 2) + (parallelVec) * (parallelLength / 2);
            var p3 = newCenterPoint + (rectToBeamVec) * (newLength / 2) - (parallelVec) * (parallelLength / 2);


            return new DeckingRectangle(new List<XYZ> { p0, p1, p2, p3 }, oldRectangle.MainBeamDir);
        }

        public static List<DeckingRectangle> AsNewFitRectangles(this IEnumerable<DeckingRectangle> notFitRects,
                                                                     ConcreteBeam beam,
                                                                     FormworkRectangle beamRect , 
                                                                     List<double> candidateLengths)
        {
            return notFitRects.Aggregate(new List<DeckingRectangle>(), (finalRects, tor) =>
            {
                var ncor = candidateLengths.Select(l => tor.AsNewRectangleAwayFromLine(beam.BeamLine, l))
                                                    .FirstOrDefault(candidate => !candidate.IsBooleanIntersectWith(beamRect));
                if (ncor != null)
                    finalRects.Add(ncor);
                return finalRects;
            });
        }

        /// <summary>
        /// Filter Shoring recatngles that collide with beams 
        /// with optimization logic.
        /// </summary>
        /// <param name="shoringRects"></param>
        /// <param name="beams"></param>
        /// <param name="beamOffset"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static List<DeckingRectangle> FilterBeamsOptimized(this IEnumerable<DeckingRectangle> shoringRects,
                                                                  IEnumerable<ConcreteBeam> beams,
                                                                  double beamOffset,
                                                                  List<double> database)
        {
            var newRectsCopy = shoringRects.Select(r => r).ToList();

            var filteredRects = beams.Aggregate(newRectsCopy, (soFar, current) =>
            {
                var beamRect = current.ToRectangle(2 * beamOffset);

                var toBeOptimizedRects = soFar.Where(formRect => beamRect.IsBooleanIntersectWith(formRect))
                                              .ToList();
                if (toBeOptimizedRects.Count > 0)
                {
                    var collidingLength = toBeOptimizedRects.First().Lines.First(l => l.Direction.IsPrepTo(current.BeamLine.Direction)).Length;
                    var candidateLengths = database.Where(l => l < collidingLength)
                                                   .OrderByDescending(l => l)
                                                   .ToList();
                    if (candidateLengths.Count > 0)
                    {
                      var fitRects=  toBeOptimizedRects.AsNewFitRectangles(current,beamRect,candidateLengths);
                        if (fitRects.Count>0)
                        {
                            toBeOptimizedRects.ForEach(rect => soFar.Remove(rect));
                            soFar.AddRange(fitRects);
                            return soFar;
                        }
                    }
                    toBeOptimizedRects.ForEach(rect => soFar.Remove(rect));
                }
                return soFar;
            });

            return filteredRects;
        }


        /// <summary>
        /// Filter Shoring Rectangles that collide with columns or beams with no
        /// optimization logic.
        /// </summary>
        /// <param name="rectangles"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static List<DeckingRectangle> Filter(this List<DeckingRectangle> rectangles,
                                                     List<FormworkRectangle> elements)
        {

            var newRectsCopy = rectangles.Select(r => r).ToList(); //To take a seperate copy in order not to violate immutability.
            var withNoColumns = elements.Aggregate(newRectsCopy, (soFar, current) =>
            {
                //soFar: Floor Rectangles
                var rectsContainColumn = soFar.Where(formRect => current.IsBooleanIntersectWith(formRect))
                                              .ToList();
                if (rectsContainColumn.Count > 0)
                {
                    rectsContainColumn.ForEach(rect => soFar.Remove(rect));
                }
                return soFar;
            });

            return withNoColumns;
        }


    }
}
