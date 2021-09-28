using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Comparers.Comparers;
using static FormworkOptimize.Core.Constants.RevitBase;
using static FormworkOptimize.Core.Errors.Errors;

namespace FormworkOptimize.Core.Helpers.RevitHelper
{
    public static class DeckingHelper
    {
        private static Tuple<Floor, Action> DrawBoundary(this RevitPlywood revitPlywood, Document doc, double offset = 0)
        {
            var plywoodFloorType = Database.PlywoodFloorTypes[revitPlywood.SectionName].Item1;

            var plywoodType = new FilteredElementCollector(doc).OfClass(typeof(FloorType))
                                                                .ToElements()
                                                                .Cast<FloorType>()
                                                                .First(type => type.Name == plywoodFloorType);

            var boundary = revitPlywood.Boundary
                                       .OffsetBy(offset)
                                       .Aggregate(new CurveArray(), (soFar, current) => { soFar.Append(current); return soFar; });


            var plywood = doc.Create.NewFloor(boundary, plywoodType, revitPlywood.HostLevel, true);
            plywood.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(revitPlywood.OffsetFromLevel);

            Action plywoodOpeningsAction = () =>
            {
                revitPlywood.PlywoodOpenings.Select(rect =>
                {
                    var curveArray = rect.ToCurveArray();
                    return doc.Create.NewOpening(plywood, curveArray, true);
                }).ToList();
            };

            return Tuple.Create(plywood, plywoodOpeningsAction);
        }

        public static Action DrawBoundary(this RevitFloorPlywood revitPlywood, Document doc)
        {
            (var plywoodFloorType, var plywoodThickness) = Database.PlywoodFloorTypes[revitPlywood.SectionName];

            var thicknessFeet = plywoodThickness.MmToFeet();

            (var plywood, var plywoodOpeningsAction) = (revitPlywood as RevitPlywood).DrawBoundary(doc, thicknessFeet);

            var plywoodType = new FilteredElementCollector(doc).OfClass(typeof(WallType))
                                                               .ToElements()
                                                               .Cast<WallType>()
                                                               .First(type => type.Name == plywoodFloorType);

            revitPlywood.Boundary.OffsetBy(thicknessFeet / 2).Select(l => Wall.Create(doc, l, plywoodType.Id, revitPlywood.HostLevel.Id, revitPlywood.ConcreteFloorThickness, revitPlywood.OffsetFromLevel, false, false))
                                 .ToList();

            return () =>
            {
                plywoodOpeningsAction();
                revitPlywood.ConcreteFloorOpenings.ForEach(os =>
                {
                    var o = os.OffsetBy(thicknessFeet)
                              .Aggregate(new CurveArray(), (soFar, current) => { soFar.Append(current); return soFar; });
                    doc.Create.NewOpening(plywood, o, true);
                    os.OffsetBy(thicknessFeet / 2)
                      .Select(l => Wall.Create(doc, l, plywoodType.Id, revitPlywood.HostLevel.Id, revitPlywood.ConcreteFloorThickness, revitPlywood.OffsetFromLevel, false, false))
                      .ToList();
                });

            };
        }

        public static Action DrawBoundary(this RevitLinePlywood revitPlywood, Document doc)
        {
            (var plywood, var plywoodOpeningsAction) = (revitPlywood as RevitPlywood).DrawBoundary(doc);

            return () =>
            {
                plywoodOpeningsAction();
                revitPlywood.ConcreteFloorOpenings.ForEach(os =>
                {
                    var o = os.Aggregate(new CurveArray(), (soFar, current) => { soFar.Append(current); return soFar; });
                    doc.Create.NewOpening(plywood, o, true);
                });

            };
        }

        public static Action DrawBoundary(this RevitBeamPlywood revitPlywood, Document doc)
        {
            (var plywoodFloorType, var plywoodThickness) = Database.PlywoodFloorTypes[revitPlywood.SectionName];
            var plywoodType = new FilteredElementCollector(doc).OfClass(typeof(WallType))
                                                               .ToElements()
                                                               .Cast<WallType>()
                                                               .First(type => type.Name == plywoodFloorType);

            var beamClearDepth = revitPlywood.ConcreteBeam.H - revitPlywood.ConcreteFloorThickness - plywoodThickness.MmToFeet();

            (var plywood, var plywoodOpeningsAction) = (revitPlywood as RevitPlywood).DrawBoundary(doc);
            revitPlywood.ConcreteBeam.ToRectangle(revitPlywood.ConcreteBeam.B + plywoodThickness.MmToFeet())
                                     .Lines.Where(l => l.Direction.IsParallelTo(revitPlywood.ConcreteBeam.BeamLine.Direction))
                                     .Select(l => Wall.Create(doc, l, plywoodType.Id, revitPlywood.HostLevel.Id, beamClearDepth, revitPlywood.OffsetFromLevel, false, false))
                                     .ToList();
            return plywoodOpeningsAction;
        }

        /// <summary>
        /// Create Plywood under beams with the correct geometry.
        /// </summary>
        /// <param name="beams"></param>
        /// <param name="plywoodFunc"></param>
        /// <param name="plywoodWidth"></param>
        /// <returns></returns>
        public static List<FormworkRectangle> ToRectangles(this IEnumerable<ConcreteBeam> beams, double plywoodWidth)
        {
            var beamsLine = beams.Select(beam => beam.BeamLine);
            Func<Line, FormworkRectangle> lineToRectangle = line =>
            {
                var prepVec = line.Direction.CrossProduct(XYZ.BasisZ);
                var startPoint = line.GetEndPoint(0);
                var endPoint = line.GetEndPoint(1);
                var p0 = startPoint + (plywoodWidth / 2) * prepVec;
                var p1 = startPoint - (plywoodWidth / 2) * prepVec;
                var p2 = endPoint - (plywoodWidth / 2) * prepVec;
                var p3 = endPoint + (plywoodWidth / 2) * prepVec;
                return new FormworkRectangle(p0, p1, p2, p3);
            };
            var group = beamsLine.GroupBy(l => l.Direction.IsParallelTo(XYZ.BasisX))
                                 .ToList();
            var parallelX = group.FirstOrDefault(g => g.Key == true)?.ToList() ?? new List<Line>();
            var parallelY = group.FirstOrDefault(g => g.Key == false)?.ToList() ?? new List<Line>();
            if (parallelX.Count == 0 || parallelY.Count == 0)
            {
                var plywoods = parallelX.Concat(parallelY)
                                        .Select(l => lineToRectangle.Invoke(l))
                                        .ToList();
                return plywoods;

            }
            else
            {
                //Clips
                var rectsFromParallelX = parallelX.Select(l => lineToRectangle.Invoke(l)).ToList();
                //Subjs
                var rectsFromParallelY = parallelY.Select(l => lineToRectangle.Invoke(l)).ToList();
                //var plywoods = rectsFromParallelY.SelectMany(rect => rect.DoBooleanDifferenceWith(rectsFromParallelX))
                //                                 .Concat(rectsFromParallelX)
                //                                 .Select(rect => rect)
                //                                 .ToList();
                // return plywoods;
                return rectsFromParallelX.Concat(rectsFromParallelY).ToList();
            }
        }

        public static Line ToLine(this RevitBeam beam) =>
            Line.CreateBound(beam.StartPoint, beam.EndPoint);

        public static List<Line> ToLines(this IEnumerable<RevitBeam> beams) =>
            beams.Select(ToLine).ToList();

        public static List<XYZ> ToPoints(this RevitBeam beam) =>
            new List<XYZ>() { beam.StartPoint, beam.EndPoint };

        public static List<XYZ> ToPoints(this IEnumerable<RevitBeam> beams) =>
            beams.SelectMany(ToPoints).ToList();

        public static RevitFloorPlywood GetPlywood(this RevitConcreteFloor revitFloor,
                                                        Floor hostFloor,
                                                        Floor supportedFloor,
                                                        PlywoodSectionName plywoodSection,
                                                        Document doc,
                                                        Options options)
        {
            var hostLevel = doc.GetElement(hostFloor.LevelId) as Level;
            var hostFloorOffset = hostFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();

            var clearHeight = hostFloor.GetClearHeight(supportedFloor);

            var supportedLevel = doc.GetElement(supportedFloor.LevelId) as Level;

            var columns = doc.GetColumnsInLevelWithinPolygon(hostLevel, hostFloorOffset, revitFloor.Boundary)
                             .Where(e => e.IsRectColumn(doc))
                              .Select(e => e.ToConcreteColumn(doc).CornerPoints);
            var beams = doc.GetBeamsInLevelWithinPolygon(supportedLevel, revitFloor.Boundary)
                            .Select(e => e.ToConcreteBeam(doc, 0).ToRectangle());

            var drops = revitFloor.Boundary.GetDropPanels(hostFloor, supportedFloor, doc, options);

            var plywoodOffset = hostFloorOffset + clearHeight;

            var plywoodOpenings = columns.Concat(beams).Concat(drops).ToList();

            return new RevitFloorPlywood(plywoodSection, revitFloor, hostLevel, plywoodOffset, plywoodOpenings);


        }

        public static Validation<RevitLinePlywood> GetPlywood(this RevitLineFloor revitFloor,
                                               Floor hostFloor,
                                               Floor supportedFloor,
                                               PlywoodSectionName plywoodSection,
                                               Document doc,
                                               Options options)
        {
            var hostLevel = doc.GetElement(hostFloor.LevelId) as Level;
            var hostFloorOffset = hostFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
            var supportedLevel = doc.GetElement(supportedFloor.LevelId) as Level;
            var clearHeight = hostFloor.GetClearHeight(supportedFloor);
            var columns = doc.GetColumnsInLevelWithinPolygon(hostLevel, hostFloorOffset, revitFloor.Boundary)
                             .Where(e => e.IsRectColumn(doc))
                             .Select(e => e.ToConcreteColumn(doc).CornerPoints);
            var beams = doc.GetBeamsInLevelWithinPolygon(supportedLevel, revitFloor.Boundary)
                            .Select(e => e.ToConcreteBeam(doc, 0).ToRectangle());

            var drops = revitFloor.Boundary.GetDropPanels(hostFloor, supportedFloor, doc, options);

            var plywoodOpenings = columns.Concat(beams).Concat(drops).ToList();

            var plywoodOffset = hostFloorOffset + clearHeight;
            return new RevitLinePlywood(plywoodSection, revitFloor, hostLevel, plywoodOffset, plywoodOpenings);
        }

        public static RevitBeamPlywood GetPlywood(this ConcreteBeam beam,
                                                   List<Line> boundary,
                                                   Floor hostFloor,
                                                   double clearHeight,
                                                   PlywoodSectionName plywoodSection,
                                                   double floorThickness,
                                                   Document doc)
        {
            var hostLevel = doc.GetElement(hostFloor.LevelId) as Level;
            var hostFloorOffset = hostFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();

            var columns = doc.GetColumnsInLevelWithinPolygon(hostLevel, hostFloorOffset, boundary.OffsetBy(-2))
                              .Where(e => e.IsRectColumn(doc))
                              .Select(e => e.ToConcreteColumn(doc).CornerPoints)
                              .Where(c => c.IsBooleanIntersectWith(boundary.ToPoints()))
                              .ToList();
            var plywoodOffset = hostFloorOffset + clearHeight;
            return new RevitBeamPlywood(plywoodSection, boundary, hostLevel, plywoodOffset, beam, floorThickness, columns);
        }

        private static Dictionary<RevitBeamSectionName, Tuple<string, Action<FamilyInstance, RevitBeam>>> _revitBeamSectionSymbols = new Dictionary<RevitBeamSectionName, Tuple<string, Action<FamilyInstance, RevitBeam>>>()
        {
            //TODO:Check Final list of beam sections because there is a conflict between designer and revit.
            [RevitBeamSectionName.ACROW_BEAM_S12] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_ACROW_BEAM_S12_SYMBOL, (x, y) => { }),
            [RevitBeamSectionName.DOUBLE_ACROW_BEAM_S12] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(DOUBLE_ACROW_BEAM_S12_SYMBOL, (x, y) => { }),
            [RevitBeamSectionName.ALUMINUM_BEAM] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_ALUMINUM_BEAM_SYMBOL, (x, y) => { }),
            [RevitBeamSectionName.ALUMINUM_BEAM_DOUBLE] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(Double_ALUMINUM_BEAM_SYMBOL, (x, y) => { }),
            [RevitBeamSectionName.S_BEAM_12] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_S_BEAM_12CM_SYMBOL, (x, y) => { }),
            //[RevitBeamSectionName.S_BEAM_16] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_S_BEAM_16CM_SYMBOL, (x, y) => { }),
            [RevitBeamSectionName.TIMBER_H20] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_TIMBER_H20_SYMBOL, (x, y) => { }),
            [RevitBeamSectionName.DOUBLE_TIMBER_H20] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(DOUBLE_TIMBER_H20_SYMBOL, (x, y) => { }),
            [RevitBeamSectionName.TIMBER_2X4] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.TIMBER_2X5] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.DOUBLE_TIMBER_2X5] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(DOUBLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.TIMBER_2X6] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.DOUBLE_TIMBER_2X6] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(DOUBLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.TIMBER_2X8] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.DOUBLE_TIMBER_2X8] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(DOUBLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.TIMBER_3X3] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.TIMBER_3X5] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.TIMBER_3X6] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.DOUBLE_TIMBER_3X6] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(DOUBLE_TIMBER_SYMBOL, AddParametersForTimber),
            [RevitBeamSectionName.TIMBER_4X4] = Tuple.Create<string, Action<FamilyInstance, RevitBeam>>(SINGLE_TIMBER_SYMBOL, AddParametersForTimber)
        };

        public static List<RevitBeam> OffsetMainBeamsForShoreBrace(this List<RevitBeam> mainBeams)
        {

            Func<RevitBeam, XYZ> midPoint = beam => (beam.StartPoint + beam.EndPoint) / 2;

            Func<RevitBeam, double> cantLength = beam => (beam.Length - beam.StartPoint.DistanceTo(beam.EndPoint)) / 2;

            var fBeam = mainBeams[0];
            var sBeam = mainBeams[1];
            var midFBeam = midPoint(fBeam);
            var midSBeam = midPoint(sBeam);
            var length = midFBeam.DistanceTo(midSBeam);
            var offsetDist = length / 2.0 - Database.SHORE_MAIN_HALF_WIDTH.CmToFeet();
            var fToSVec = (midSBeam - midFBeam).Normalize();
            var newFBeam = new RevitBeam(fBeam.Section, fBeam.StartPoint + offsetDist * fToSVec, fBeam.EndPoint + offsetDist * fToSVec, fBeam.HostLevel, fBeam.OffsetFromLevel - fBeam.Section.Height.CmToFeet(), cantLength(fBeam));
            var newSBeam = new RevitBeam(sBeam.Section, sBeam.StartPoint - offsetDist * fToSVec, sBeam.EndPoint - offsetDist * fToSVec, sBeam.HostLevel, sBeam.OffsetFromLevel - sBeam.Section.Height.CmToFeet(), cantLength(sBeam));
            return new List<RevitBeam>() { newFBeam, newSBeam };
        }

        public static List<RevitBeam> AdjustLayout(this List<RevitBeam> beams, double beamTotalLength)
        {
            var adjustedBeams = beams.Distinct(RevitBeamComparer)
                                      .ToColinears()
                                      .SelectMany(cl => cl.MergeColinearBeams(beamTotalLength))
                                      .ToColinears()
                                      .SelectMany(g => g.OffsetColinear())
                                      .ToList();
            return adjustedBeams;
        }


        public static Func<List<RevitBeam>, double, List<RevitBeam>> AdjustLayout(Document doc, Level level)
        {
            var allExistingBeams = doc.GetFormworkBeamInstances(level)
                                      .ToList();


            return (beams, beamTotalLength) =>
            {

                var adjustedBeams = beams.Distinct(RevitBeamComparer)
                                     .ToColinears()
                                     .SelectMany(cl => cl.MergeColinearBeams(beamTotalLength))
                                     .ToColinears()
                                     .OffsetGroupFromExisting(allExistingBeams);

                return adjustedBeams;
            };

        }



        private static void AddParametersForTimber(FamilyInstance beamInst, RevitBeam beam)
        {
            beamInst.LookupParameter("b").Set(beam.Section.Breadth.CmToFeet());
            beamInst.LookupParameter("h").Set(beam.Section.Height.CmToFeet());
        }

        public static RevitBeam Draw(this RevitBeam beam, Document doc)
        {
            (var symbolName, var addParameterAction) = _revitBeamSectionSymbols[beam.Section.SectionName];

            var beamSymbol = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                                                              .Cast<FamilySymbol>()
                                                              .First(sym => sym.Name == symbolName)
                                                              .ActivateIfNot();

            var locp = (beam.StartPoint + beam.EndPoint) / 2 + beam.OffsetVector * (beam.Section.Breadth.CmToFeet() / 2);
            var beamInst = doc.Create.NewFamilyInstance(locp, beamSymbol, beam.HostLevel, StructuralType.NonStructural);
            beamInst.LookupParameter("L").Set(beam.Length);
            beamInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(beam.OffsetFromLevel);
            var vec = beam.EndPoint - beam.StartPoint;
            ElementTransformUtils.RotateElement(doc, beamInst.Id, Line.CreateBound(locp, locp + XYZ.BasisZ), Math.Atan2(vec.Y, vec.X));
            addParameterAction(beamInst, beam);
            return beam;
        }

        /// <summary>
        /// Transform collection of revit beams to groups of colinear beams.
        /// </summary>
        /// <param name="beams"></param>
        /// <returns></returns>
        public static List<List<RevitBeam>> ToColinears(this IEnumerable<RevitBeam> beams)
        {

            var dir = beams.First().Direction;
            var theta = Math.Atan2(dir.Y, dir.X);
            Func<RevitBeam, double> rotateAndGetMinX = beam =>
                 beam.ToPoints().Select(p => p.RotateAboutZ(-theta)).Min(p => p.X);

           var sortedBeams =   beams.OrderBy(rotateAndGetMinX).ToList();

            //var sortedBeams = beams.OrderBy(b => Math.Min(b.StartPoint.X, b.EndPoint.X))
            //                       .ThenBy(b => Math.Min(b.StartPoint.Y, b.EndPoint.Y));
            return sortedBeams.Aggregate(new List<List<RevitBeam>>(), (soFar, current) =>
             {
                 var coLinearGroup = soFar.FirstOrDefault(group => current.ToLine().GetCollinears(group.ToLines()).Count > 0);
                 if (coLinearGroup is null)
                     soFar.Add(new List<RevitBeam>() { current });
                 else
                     coLinearGroup.Add(current);
                 return soFar;
             }).Where(g => g.Count > 0).ToList();
        }

        public static List<RevitBeam> OffsetColinear(this IEnumerable<RevitBeam> beams)
        {
            if (beams.Count() == 1)
                return beams.ToList();

            var dir = beams.First().Direction.CrossProduct(XYZ.BasisZ);
            return beams.Select(b =>
            {
                var newBeam = new RevitBeam(b, dir);
                dir = -1 * dir;
                return newBeam;
            }).ToList();
        }

        public static List<RevitBeam> OffsetGroupFromExisting(this List<List<RevitBeam>> groups, IEnumerable<FormworkBeamInstance> existingBeams)
        {
            Func<RevitBeam, XYZ> midPoint = beam => (beam.StartPoint.CopyWithNewZ(0) + beam.EndPoint.CopyWithNewZ(0)) / 2;
            Func<FormworkBeamInstance, Line> prepLine = (inst) =>
           {
               var prepVector = inst.Direction.CrossProduct(XYZ.BasisZ);
               var p1 = inst.LocationPoint + 40 * prepVector;
               var p2 = inst.LocationPoint - 40 * prepVector;
               return Line.CreateBound(p1, p2);
           };
            Func<Line, Line, XYZ, XYZ> intersectionVec = (shorter, longer, pVec) =>
           {
               var p1 = shorter.Origin + 20 * pVec;
               var p2 = shorter.Origin - 20 * pVec;
               var intLine = Line.CreateBound(p1, p2);

               var intPoint = intLine.GetIntersectionPoints(longer).First();
               return (intPoint - shorter.Origin).Normalize();
           };
            var firstBeam = groups.First().First();
            var midPointFirst = midPoint(firstBeam);
            var nearestExistingBeam = existingBeams.Where(b => b.Direction.IsParallelTo(firstBeam.Direction))
                                                   .Where(b => prepLine(b).GetIntersectionPoints(firstBeam.ToLine()).Count() == 0)
                                                   .OrderBy(b => b.LocationPoint.DistanceTo(midPointFirst))
                                                   .FirstOrDefault();
            if (nearestExistingBeam != null)
            {
                var l1 = Line.CreateBound(firstBeam.StartPoint, nearestExistingBeam.LocationPoint);
                var l2 = Line.CreateBound(firstBeam.EndPoint, nearestExistingBeam.LocationPoint);


                var prepVec = firstBeam.Direction.CrossProduct(XYZ.BasisZ).Normalize();
                var offsetVec = l1.Length > l2.Length ? intersectionVec(l2, l1, prepVec) : intersectionVec(l1, l2, prepVec);
                return groups.SelectMany(g => g.OffsetColinear(-offsetVec)).ToList();
            }
            return groups.SelectMany(g => g.OffsetColinear()).ToList();
        }

        private static List<RevitBeam> OffsetColinear(this IEnumerable<RevitBeam> beams, XYZ vector)
        {
            var dir = vector.Normalize();
            return beams.Select(b =>
            {
                var newBeam = new RevitBeam(b, dir);
                dir = -1 * dir;
                return newBeam;
            }).ToList();
        }

        public static List<RevitBeam> SortBeams(this IEnumerable<RevitBeam> beams)
        {
            var dir = beams.First().Direction;
            var theta = Math.Atan2(dir.Y, dir.X);
            Func<RevitBeam, double> rotateAndGetMinX = beam =>
                 beam.ToPoints().Select(p => p.RotateAboutZ(-theta)).Min(p => p.X);

            return beams.OrderBy(rotateAndGetMinX).ToList();
        }

        public static List<DeckingRectangle> MatchMainBeams(this IEnumerable<RevitBeam> beams)
        {
            var dir = beams.First().Direction;
            var theta = Math.Atan2(dir.Y, dir.X);
            Func<RevitBeam, double> rotateAndGetMinX = beam =>
                 beam.ToPoints().Select(p => p.RotateAboutZ(-theta)).Min(p => p.Y);

            var groups = beams.Select(b => Tuple.Create((int)(rotateAndGetMinX(b) * 1000), b)).GroupBy(tuple => tuple.Item1).OrderBy(kvp => kvp.Key).ToList();
            return groups.Skip(1).Aggregate(Tuple.Create(0, new List<DeckingRectangle>()), (soFar, current) =>
              {
                  (var index, var rects) = soFar;
                  var previousGroup = groups[index].Select(tuple => tuple.Item2).ToList();
                  var currentGroup = current.Select(tuple => tuple.Item2).ToList();
                  previousGroup.ForEach(beam =>
                  {
                      var withinBeam = currentGroup.Where(cb => beam.IsWithin(cb)).ToList();
                      var rs = withinBeam.Select(b => b.AsRectangle(beam));
                      rects.AddRange(rs);
                  });
                  return Tuple.Create(index + 1, rects);
              }).Item2;
        }

        public static DeckingRectangle AsRectangle(this RevitBeam a, RevitBeam b)
        {
            (var shorter, var longer) = a.Length < b.Length ? Tuple.Create(a, b) : Tuple.Create(b, a);
            var dir = shorter.Direction;
            var length = shorter.Length;
            var centerPoint = (shorter.StartPoint + shorter.EndPoint) / 2;
            var otherPoint = longer.ToLine().Project(centerPoint).XYZPoint;
            var p0 = centerPoint + (length / 2) * dir;
            var p1 = otherPoint + (length / 2) * dir;
            var p2 = otherPoint - (length / 2) * dir;
            var p3 = centerPoint - (length / 2) * dir;
            var points = new List<XYZ>() { p0, p1, p2, p3 };
            (var min , var max) = points.CreateXYMinMax();
            var newPoints = new List<XYZ>()
            {
                min,
                new XYZ(min.X,max.Y,0),
                max,
                new XYZ(max.X,min.Y,0),
            };
            var rect = new DeckingRectangle(newPoints, dir);
            return rect;
        }

        public static bool IsWithin(this RevitBeam a, RevitBeam b)
        {
            (var shorter, var longer) = a.Length < b.Length ? Tuple.Create(a, b) : Tuple.Create(b, a);
            var prepDir = shorter.Direction.CrossProduct(XYZ.BasisZ);
            var length = 1000.0;
            var line1 = Line.CreateBound(shorter.StartPoint + prepDir * length, shorter.StartPoint - prepDir * length);
            var line2 = Line.CreateBound(shorter.EndPoint + prepDir * length, shorter.EndPoint - prepDir * length);
            var longerLine = longer.ToLine();
            var pts1 = line1.GetIntersectionPoints(longerLine);
            var pts2 = line2.GetIntersectionPoints(longerLine);
            return pts1.Count() > 0 && pts2.Count() > 0;
        }

        public static RevitBeam ReduceColinearBeams(this IEnumerable<RevitBeam> beams, double totalBeamLength)
        {
            var beam = beams.Count() == 1 ? beams.First() : beams.Skip(1).First();
            var dir = beam.Direction;
            var theta = Math.Atan2(dir.Y, dir.X);
            //Inverse Rotation.
            var xPoints = beams.ToPoints().Select(p => p.RotateAboutZ(-theta)).OrderBy(p => p.X).ToList();
            var start = xPoints.First().RotateAboutZ(theta);
            var end = xPoints.Last().RotateAboutZ(theta);
            var cantLength = (totalBeamLength - beams.Sum(b => b.Length)/* beams.Count() * beam.Length*/) / 2;
            return new RevitBeam(beams.First().Section, start, end, beam.HostLevel, beams.First().OffsetFromLevel - beams.First().Section.Height.CmToFeet(), cantLength);
        }

        public static List<RevitBeam> MergeColinearBeams(this List<RevitBeam> colinearBeams, double totalBeamLength)
        {
            var sortedColinearBeams = colinearBeams.SortBeams();
            var minCantLength = MIN_CANTILEVER_LENGTH.CmToFeet();
            //var maxCantLength = 100.0.CmToFeet();
            var firstBeam = sortedColinearBeams.Count() == 1 ? sortedColinearBeams.First() : sortedColinearBeams.Skip(1).First();

            var noSegemnts = (int)Math.Floor((totalBeamLength - 2 * minCantLength) / firstBeam.Length);
            var nGroups = sortedColinearBeams.Count / noSegemnts;

            var adjustedBeams = Enumerable.Range(0, nGroups).Select(i => sortedColinearBeams.Skip(i * noSegemnts).Take(noSegemnts))
                                          //.Concat(new[] { colinearBeams.Skip(nGroups * noSegemnts) })
                                          .Aggregate(new List<RevitBeam>(), (soFar, current) =>
                                           {
                                               soFar.Add(current.ReduceColinearBeams(totalBeamLength));
                                               return soFar;
                                           });
            var lastGroup = sortedColinearBeams.Skip(nGroups * noSegemnts);
            if (lastGroup.Count() > 0)
            {
                var fBeam = lastGroup.First();
                var lBeam = lastGroup.Last();
                var beamsLength = lastGroup.Sum(b => b.Length);
                var lastGroupDbLength = Database.GetBeamLengths(lastGroup.First().Section.SectionName)
                                                .Select(l => l.CmToFeet())
                                                .FirstOrDefault(l => l - beamsLength > 2 * minCantLength);
                adjustedBeams.Add(lastGroup.ReduceColinearBeams(lastGroupDbLength));
            }
            return adjustedBeams;
        }

        public static Tuple<List<RevitBeam>, List<RevitBeam>> ToBeams(this DeckingRectangle rect,
                                                                      Level hostLevel,
                                                                      double offsetfromLevel,
                                                                      double maxSecSpacing,
                                                                      RevitBeamSection mainSection,
                                                                      RevitBeamSection secSection)
        {
            var mainBeams = rect.Lines.Where(l => l.Direction.IsParallelTo(rect.MainBeamDir))
                                      .Select(l => new RevitBeam(mainSection, l.GetEndPoint(0), l.GetEndPoint(1), hostLevel, offsetfromLevel))
                                      .ToList();
            var firstMainBeam = mainBeams.First();
            var secBeamLine = rect.Lines.First(l => l.Direction.IsPrepTo(rect.MainBeamDir));
            var secDir = firstMainBeam.StartPoint.IsEqual(secBeamLine.GetEndPoint(0)) || firstMainBeam.EndPoint.IsEqual(secBeamLine.GetEndPoint(0)) ?
                                                                                 (secBeamLine.GetEndPoint(1) - secBeamLine.GetEndPoint(0)).Normalize() :
                                                                                 (secBeamLine.GetEndPoint(0) - secBeamLine.GetEndPoint(1)).Normalize();
            var noSpaces = (int)Math.Ceiling(Math.Round(firstMainBeam.Length / maxSecSpacing, 4));
            var secBeamSpaing = firstMainBeam.Length / noSpaces;
            var secBeams = Enumerable.Range(0, noSpaces + 1).Select(i =>
            {
                var startPoint = firstMainBeam.StartPoint + firstMainBeam.Direction * secBeamSpaing * i;
                var endPoint = startPoint + secDir * secBeamLine.Length;
                return new RevitBeam(secSection, startPoint, endPoint, hostLevel, firstMainBeam.OffsetFromLevel);
            }).ToList();
            return Tuple.Create(mainBeams, secBeams);
        }

        public static Tuple<List<RevitBeam>, List<RevitBeam>> ToBeamsExact(this DeckingRectangle rect,
                                                                     Level hostLevel,
                                                                     double offsetfromLevel,
                                                                     double maxSecSpacing,
                                                                     RevitBeamSection mainSection,
                                                                     RevitBeamSection secSection)
        {
            var mainBeams = rect.Lines.Where(l => l.Direction.IsParallelTo(rect.MainBeamDir))
                                      .Select(l => new RevitBeam(mainSection, l.GetEndPoint(0), l.GetEndPoint(1), hostLevel, offsetfromLevel))
                                      .ToList();
            var firstMainBeam = mainBeams.First();
            var secBeamLine = rect.Lines.First(l => l.Direction.IsPrepTo(rect.MainBeamDir));
            var secDir = firstMainBeam.StartPoint.IsEqual(secBeamLine.GetEndPoint(0)) || firstMainBeam.EndPoint.IsEqual(secBeamLine.GetEndPoint(0)) ?
                                                                                 (secBeamLine.GetEndPoint(1) - secBeamLine.GetEndPoint(0)).Normalize() :
                                                                                 (secBeamLine.GetEndPoint(0) - secBeamLine.GetEndPoint(1)).Normalize();
            var noSpaces = (int)Math.Ceiling(Math.Round(firstMainBeam.Length / maxSecSpacing, 4));
            var remainingLength = noSpaces * maxSecSpacing - firstMainBeam.Length;
            var actualNoSpaces = remainingLength > 0.75 * maxSecSpacing ? noSpaces-1 : noSpaces;
            var secBeams = Enumerable.Range(0, actualNoSpaces + 1).Select(i =>
            {
                var startPoint = firstMainBeam.StartPoint + firstMainBeam.Direction * maxSecSpacing * i - (i / noSpaces) * remainingLength * firstMainBeam.Direction;
                var endPoint = startPoint + secDir * secBeamLine.Length;
                return new RevitBeam(secSection, startPoint, endPoint, hostLevel, firstMainBeam.OffsetFromLevel);
            }).ToList();
            return Tuple.Create(mainBeams, secBeams);
        }

        public static bool IsEqual(this RevitBeam beam, RevitBeam other)
        {
            var result = (beam.StartPoint.IsEqual(other.StartPoint) ||
                          beam.StartPoint.IsEqual(other.EndPoint)) &&
                         (beam.EndPoint.IsEqual(other.StartPoint) ||
                          beam.EndPoint.IsEqual(other.EndPoint));
            return result;

        }

        public static bool IsBeamIntersectWithRectangle(this RevitBeam beam, FormworkRectangle rect)
        {
            var z = beam.StartPoint.Z;
            var prepLine = rect.Lines.Where(l => l.Direction.IsPrepTo(beam.Direction)).FirstOrDefault();
            if (prepLine is null)
                return false;

            var newPrepLine = Line.CreateBound(prepLine.GetEndPoint(0).CopyWithNewZ(z), prepLine.GetEndPoint(1).CopyWithNewZ(z));
            var beamLine = Line.CreateBound(beam.StartPoint, beam.EndPoint);
            return beamLine.GetIntersectionPoints(newPrepLine).Count() > 0;
        }

        /// <summary>
        /// Gets Nearest Length in the database to the spacing.
        /// </summary>
        /// <param name="beamSection"></param>
        /// <param name="spacing">distance between vertical elements.</param>
        /// <returns></returns>
        public static Validation<double> GetNearestDbLength(this RevitBeamSectionName beamSection, double spacing)
        {

            var candidateDbLengths = Database.GetBeamLengths(beamSection)
                                              .Select(l => l.CmToFeet())
                                              .Where(l => (l - spacing) >= 2 * MIN_CANTILEVER_LENGTH.CmToFeet())
                                              .OrderBy(l => l);
            if (candidateDbLengths.Count() == 0)
                return ShortBeam;
            else
                return candidateDbLengths.First();
        }

    }
}
