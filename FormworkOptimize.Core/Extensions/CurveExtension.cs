using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.Entities.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Errors.Errors;
using static FormworkOptimize.Core.Comparers.Comparers;
using FormworkOptimize.Core.Helpers.RevitHelper;

namespace FormworkOptimize.Core.Extensions
{
    public static class CurveExtension
    {

        public static int GetHash(this Line line)
        {
            var prime1 = 17;
            var prime2 = 23;
            unchecked
            {
                var hash = prime1 * prime2 * (line.GetEndPoint(0).GetHash() + line.GetEndPoint(1).GetHash());
                return hash;
            }
        }


        public static FormworkRectangle ToRectangle(this List<Line> lines)
        {
            var points = lines.Select(l => l.GetEndPoint(0)).ToList();
            return new FormworkRectangle(points);
        }

        public static bool IsEqual(this Line l1, Line l2)
        {
            return (l1.GetEndPoint(0).IsEqual(l2.GetEndPoint(0)) || l1.GetEndPoint(0).IsEqual(l2.GetEndPoint(1))) &&
                    (l1.GetEndPoint(1).IsEqual(l2.GetEndPoint(0)) || l1.GetEndPoint(1).IsEqual(l2.GetEndPoint(1)));
        }


        /// <summary>
        /// Gets the intersection points between curve and another curve.
        /// </summary>
        /// <param name="curve">The face.</param>
        /// <param name="other">The curve.</param>
        /// <returns></returns>
        public static IEnumerable<XYZ> GetIntersectionPoints(this Curve curve, Curve other)
        {
            IntersectionResultArray intResults;
            if (curve.Intersect(other, out intResults) == SetComparisonResult.Overlap)
            {
                foreach (var result in intResults.Cast<IntersectionResult>())
                    yield return result.XYZPoint;
            }
        }

        /// <summary>
        /// Get intersection points of a line with other lines.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="lines">The lines.</param>
        /// <returns></returns>
        public static List<XYZ> GetIntersections(this Line line, List<Line> lines)
        {
            var cndLines = lines.Where(l => !(l.Direction.IsParallelTo(line.Direction))).ToList();
            var intPoints = cndLines.SelectMany(l => line.GetIntersectionPoints(l)).ToList();
            return intPoints;
        }

        /// <summary>
        /// Gets the intersection points between curve and face.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="face">The face.</param>
        /// <returns></returns>
        public static IEnumerable<XYZ> GetIntersectionPoints(this Curve curve, Face face)
        {
            IntersectionResultArray intResults;
            if (face.Intersect(curve, out intResults) == SetComparisonResult.Overlap)
            {
                foreach (var result in intResults.Cast<IntersectionResult>())
                    yield return result.XYZPoint;
            }
        }

        /// <summary>
        /// Gets the verticies of a closed polygon.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <returns></returns>
        public static List<XYZ> ToPoints(this List<Line> polygon) =>
            polygon.Select(l => l.GetEndPoint(0)).ToList();


        /// <summary>
        /// Gets number of right angles.
        /// </summary>
        /// <param name="curves">The curves.</param>
        /// <returns></returns>
        public static int GetRightAngles(this List<Line> curves)
        {
            return curves.Aggregate(0, (soFar, current) =>
            {
                var i = curves.IndexOf(current);
                var vec1 = current.Direction;
                var vec2 = curves[(i + 1) % curves.Count].Direction;
                if (vec1.IsPrepTo(vec2))
                    soFar++;
                return soFar;
            });
        }

        /// <summary>
        /// Check if lines create a rectangle
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static bool IsRectangle(this List<Line> curves) =>
            curves.Count == 4 && curves.GetRightAngles() == 4 ? true : false;

        /// <summary>
        /// Divides Rectangle Shape into one specific direction uniformly.
        /// </summary>
        /// <param name="rectangle">The curves.</param>
        /// <param name="dividingVal">The dividing value.</param>
        /// <param name="divVec">The div vec.</param>
        /// <returns></returns>
        public static List<FormworkRectangle> DivideRectnagle(this List<Line> rectangle, double dividingVal, XYZ divVec)
        {
            if (!IsRectangle(rectangle))
                return null;
            var result = new List<FormworkRectangle>();
            //------------------Get Lines parallel to the dividing vector--------------------
            var divLines = rectangle.Where(l => l.Direction.IsParallelTo(divVec)).ToList();
            var divLine = divLines[0];
            var parallelLine = divLines[1];
            //-------------------------------------------------------------------------------

            //-------------------------Rectangle Ordered Points------------------------------
            var p0 = divLine.GetEndPoint(0);
            var p1 = divLine.GetEndPoint(1);
            var p2 = parallelLine.GetEndPoint(0);
            var p3 = parallelLine.GetEndPoint(1);
            //-------------------------------------------------------------------------------

            var length = divLine.Length;
            if (length > dividingVal)
            {
                var unitVec = (p1 - p0).Normalize();
                int n = (int)Math.Ceiling(Math.Round(length / dividingVal, 4));
                //var segment = length / n;

                var extendedParallelLine = Line.CreateBound(p2-parallelLine.Direction*100,p3+parallelLine.Direction*100);
                result = Enumerable.Range(0, n).Aggregate(new List<FormworkRectangle>(), (soFar, current) =>
                     {
                         var p0Hat = p0 + current * unitVec * dividingVal;
                         var p1Hat = p0Hat + unitVec * dividingVal;
                         var p2Hat = extendedParallelLine.Project(p1Hat).XYZPoint;
                         var p3Hat = p2Hat - unitVec * dividingVal;
                         var rect = new FormworkRectangle(p0Hat, p1Hat, p2Hat, p3Hat);
                         soFar.Add(rect);
                         return soFar;
                     });
            }
            else
            {
                result.Add(new FormworkRectangle(p0, p1, p2, p3));
            }
            return result;
        }

        public static bool IsCollinearWith(this Curve curve, Curve other)
        {
            var compResult = curve.Intersect(other);
            return compResult == SetComparisonResult.Subset || compResult == SetComparisonResult.Equal;
        }


        /// <summary>
        /// Gets the collinear lines in collection to a given line .
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="lines">The lines.</param>
        /// <returns></returns>
        public static List<Line> GetCollinears(this Line line, List<Line> lines)
        {
            var coLinears = lines.Where(l => l.Direction.IsParallelTo(line.Direction) && line.IsCollinearWith(l)).ToList();
            return coLinears;
            //var cndLines = lines.Where(l => l.Direction.IsParallelTo(line.Direction)).ToList();
            //var collinearLines = cndLines.Where(l => l != line && line.IsCollinearWith(l)).ToList();
            //if (collinearLines.Count > 0)
            //    collinearLines.Add(line);
            //return collinearLines;
        }

        public static Line ReduceToLine(this List<Line> coLinearLines)
        {
            if (coLinearLines.Count == 0)
                return null;
            var points = coLinearLines.SelectMany(l => new List<XYZ> { l.GetEndPoint(0), l.GetEndPoint(1) })
                                      .OrderBy(p => p.X)
                                      .ThenBy(p => p.Y)
                                      .ToList();
            var line = Line.CreateBound(points.First(), points.Last());
            return line;
        }


        public static bool IsLineIntersectWithRectangle(this Line line, FormworkRectangle rect) =>
             rect.Lines.Any(l => l.GetIntersectionPoints(line).Count() > 0);

        public static List<DeckingRectangle> DivideOptimized(this List<Line> polygonToDivide, 
                                                                  XYZ mainBeamDir, 
                                                                  double mainDirDist, 
                                                                  double secDirDist,
                                                                  List<double> database)
        {
            var newPolygon = polygonToDivide.Select(l => l.CopyWithNewZ(0)).ToList();
            var polygonToDividePoints = newPolygon.Select(l => l.GetEndPoint(0)).ToList();
            var secBeamDir = XYZ.BasisZ.CrossProduct(mainBeamDir);
            (var insidePolygons, var intersectAtEdges) = polygonToDivide.GetBoundingRectangle()
                                                                        .DivideRectnagle(mainDirDist, mainBeamDir)
                                                                        .SelectMany(rect => rect.Lines.DivideRectnagle(secDirDist, secBeamDir))
                                                                        .CategorizeWithRespectToPloygon(newPolygon);

            var fitRectsAtEdge = newPolygon.Select(l => Tuple.Create(l, intersectAtEdges.Where(rect => l.IsLineIntersectWithRectangle(rect)).ToList()))
                                                .Where(tuple => tuple.Item2.Count > 0)
                                                .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2.Select(rect => new DeckingRectangle(rect.Points, mainBeamDir)).ToList()))
                                                .SelectMany(tuple => tuple.Item2.ShiftAwayFromLine(tuple.Item1, database))
                                                .Where(rect=>rect.Points.IsPolygonInside(polygonToDividePoints))
                                                .ToList();
           return fitRectsAtEdge.Concat(insidePolygons.Select(rect => new DeckingRectangle(rect.Points, mainBeamDir))).ToList();
        }


        /// <summary>
        /// Divide Polygon to rectangles.
        /// </summary>
        /// <param name="polygonToDivide"></param>
        /// <param name="mainBeamDir"></param>
        /// <param name="mainDirDist"></param>
        /// <param name="secDirDist"></param>
        /// <returns></returns>
        public static List<DeckingRectangle> Divide(this List<Line> polygonToDivide, XYZ mainBeamDir, double mainDirDist, double secDirDist)
        {
            var polygonToDividePoints = polygonToDivide.Select(l => l.GetEndPoint(0)).ToList();
            var secBeamDir = XYZ.BasisZ.CrossProduct(mainBeamDir);
            return polygonToDivide.GetBoundingRectangle()
                                  .DivideRectnagle(mainDirDist, mainBeamDir)
                                  .SelectMany(rect => rect.Lines.DivideRectnagle(secDirDist, secBeamDir))
                                  .Where(rect => rect.Points.IsPolygonInside(polygonToDividePoints))
                                  .Select(rect => new DeckingRectangle(rect.Points, mainBeamDir))
                                  .ToList();
        }

        public static List<DeckingRectangle> Divide(this Line lineToDivide, double mainDirDist, double secDirDist, List<double> database)
        {
            var origin = lineToDivide.GetEndPoint(0);
            var length = lineToDivide.Length;
            var divVec = lineToDivide.Direction;
            var prepVec = XYZ.BasisZ.CrossProduct(divVec);
            var n = (int)(length / mainDirDist);
            var remaining = length - mainDirDist * n;

            Func<XYZ, XYZ, DeckingRectangle> toRect = (startPoint, endPoint) =>
              {
                  var p0 = startPoint + (secDirDist / 2) * prepVec;
                  var p1 = startPoint - (secDirDist / 2) * prepVec;
                  var p2 = endPoint - (secDirDist / 2) * prepVec;
                  var p3 = endPoint + (secDirDist / 2) * prepVec;
                  return new DeckingRectangle(new List<XYZ>() { p0, p1, p2, p3 }, divVec);
              };

            Func<int, DeckingRectangle> repeatedRectCreationFunc = (index) =>
             {
                 var startPoint = origin + index * mainDirDist * divVec;
                 var endPoint = startPoint + mainDirDist * divVec;
                 return toRect(startPoint, endPoint);
             };

            Func<int, double, DeckingRectangle> lastRectCreationFunc = (index, dist) =>
             {
                 var startPoint = origin + n * mainDirDist * divVec;
                 var endPoint = startPoint + dist * divVec;
                 return toRect(startPoint, endPoint);
             };


            var smallerThanMainDirDist = database.OrderByDescending(l => l)
                                                 .Where(l => l <= remaining)
                                                 .ToList();

            var common = Enumerable.Range(0, n)
                                    .Select(repeatedRectCreationFunc)
                                    .ToList();
            if (smallerThanMainDirDist.Count > 0)
            {
                common.Add(lastRectCreationFunc(n, smallerThanMainDirDist.First()));
                return common;
            }
            else
            {
                return common;
            }
        }

        public static List<Line> OffsetBy(this List<Line> loopLines, double offsetDist)
        {
            var loop = loopLines.Aggregate(new CurveLoop(), (soFar, current) => { soFar.Append(current); return soFar; });
            return CurveLoop.CreateViaOffset(loop, offsetDist, XYZ.BasisZ).Cast<Line>().ToList();
        }

        public static List<Line> OffsetInsideBy(this List<Line> loopLines, double offsetDist) =>
            loopLines.OffsetBy(-offsetDist);


        /// <summary>
        /// Get the lines that can form a closed polygon.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static Validation<List<Line>> GetClosedPolygon(this List<Line> lines)
        {
            Func<List<XYZ>, Validation<List<XYZ>>> handleCountWithException = points =>
             {
                 var count = points.Count;
                 if (count < 2 || count > 2)
                     return InvalidPolygon;
                 else
                     return points;
             };
            var polygon = lines.Select(l => l.GetCollinears(lines))
                                .Select(colines => colines.ReduceToLine())
                                .Where(l => l != null)
                                .Distinct(LineComparer)
                                .ToList();
            var validPoints = polygon.Select(l => handleCountWithException.Invoke(l.GetIntersections(polygon)));

            if (validPoints.Any(v => !v.IsValid))
                return InvalidPolygon;

            var closedLines = validPoints.SelectMany(v => v.AsEnumerable())
                                         .Select(points => Line.CreateBound(points.First(), points.Last()))
                                         .ToList();
            return closedLines;
        }

        /// <summary>
        /// Flips a line.
        /// </summary>
        /// <param name="line">The curve.</param>
        /// <returns></returns>
        public static Line FlipCurve(this Line line)
        {
            var StartPoint = line.GetEndPoint(0);
            var EndPoint = line.GetEndPoint(1);
            return Line.CreateBound(EndPoint, StartPoint);
        }

        /// <summary>
        /// Arrange curves to form curve loop.
        /// </summary>
        /// <param name="polygonCurves">The polygon curves.</param>
        /// <returns></returns>
        public static List<Line> Arrange(this List<Line> polygonCurves)
        {
            var newArrangedCurves = new List<Line>();
            newArrangedCurves.Add(polygonCurves[0]);
            polygonCurves.RemoveAt(0);
            int counter = 0;
            for (int i = 0; i < polygonCurves.Count; i++)
            {
                var tempCurve = newArrangedCurves[counter];

                var otherCurve = polygonCurves.FirstOrDefault(p => p.GetEndPoint(0).IsEqual(tempCurve.GetEndPoint(1)));
                if (otherCurve != null)
                {
                    newArrangedCurves.Add(otherCurve);
                    polygonCurves.Remove(otherCurve);
                    i--;
                    counter++;
                    continue;
                }

                otherCurve = polygonCurves.FirstOrDefault(p => p.GetEndPoint(1).IsEqual(tempCurve.GetEndPoint(1)));
                if (otherCurve != null)
                {
                    newArrangedCurves.Add(otherCurve.FlipCurve());
                    polygonCurves.Remove(otherCurve);
                    counter++;
                    i--;
                }


            }
            return newArrangedCurves;
        }

        /// <summary>
        /// Get bounding rectangle for set of othogonal lines that form a polygon.
        /// </summary>
        /// <param name="orthogonalLines"></param>
        /// <returns></returns>
        public static List<Line> GetBoundingRectangle(this List<Line> orthogonalLines)
        {
            if (orthogonalLines.Count == 4)
                return orthogonalLines;
            var minMax = orthogonalLines.Select(line => line.GetEndPoint(0))
                                         .CreateXYMinMax();
            var xMin = minMax.Min.X;
            var yMin = minMax.Min.Y;

            var xMax = minMax.Max.X;
            var yMax = minMax.Max.Y;

            //Point Loop from Left Bottom clock wise.
            var p0 = new XYZ(xMin, yMin, 0);
            var p1 = new XYZ(xMin, yMax, 0);
            var p2 = new XYZ(xMax, yMax, 0);
            var p3 = new XYZ(xMax, yMin, 0);

            return new List<Line>()
            {
                Line.CreateBound(p0,p1),
                Line.CreateBound(p1,p2),
                Line.CreateBound(p2,p3),
                Line.CreateBound(p3,p0)
            };
        }

        public static Line CopyWithNewZ(this Line line, double z) =>
            Line.CreateBound(line.GetEndPoint(0).CopyWithNewZ(z), line.GetEndPoint(1).CopyWithNewZ(z));


        public static Validation<List<Line>> ValidateClosedPolygon(this List<Line> polygon, Document doc)
        {
            if (polygon.Count < 4)
                return InvalidPolygon;
            var lines = polygon.GetClosedPolygon()
                               .Map(ls => ls.Arrange());
            return lines;
        }

        public static XYZ MidPoint(this Line line)
        {
            return (line.GetEndPoint(0) + line.GetEndPoint(1)) / 2.0;
        }
    }
}
