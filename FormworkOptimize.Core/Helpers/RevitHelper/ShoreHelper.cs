using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using FormworkOptimize.Core.Comparers;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.DTOS.Revit.Input.Shore;
using FormworkOptimize.Core.DTOS.Revit.Internal;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static CSharp.Functional.Extensions.ValidationExtension;
using static FormworkOptimize.Core.Errors.Errors;
using static CSharp.Functional.Functional;
using CSharp.Functional.Constructs;
using FormworkOptimize.Core.Entities.Revit;
using static FormworkOptimize.Core.Constants.RevitBase;
using static FormworkOptimize.Core.Comparers.Comparers;

namespace FormworkOptimize.Core.Helpers.RevitHelper
{
    public static class ShoreHelper
    {
        private static RevitShoreMain Draw(this RevitShoreMain shoreMain, Document doc)
        {
            var symbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                                                           .Cast<FamilySymbol>();

            var postHeadSymbol = symbols.First(sym => sym.Name == RevitBase.POST_HEAD).ActivateIfNot();
            var shoreMainSymbol = symbols.First(sym => sym.Name == RevitBase.SHORE_BRACE_FRAME).ActivateIfNot();
            var shoreTelescopicSymbol = symbols.First(sym => sym.Name == RevitBase.TELESCOPIC_FRAME).ActivateIfNot();
            var uHeadSymbol = symbols.First(sym => sym.Name == RevitBase.U_HEAD).ActivateIfNot();

            var rotationVector = shoreMain.UVector.CrossProduct(XYZ.BasisZ);
            var headPosition1 = shoreMain.Position - rotationVector.Normalize() * Database.SHORE_MAIN_HALF_WIDTH.CmToFeet();
            var headPosition2 = shoreMain.Position + rotationVector.Normalize() * Database.SHORE_MAIN_HALF_WIDTH.CmToFeet();

            var postInst1 = doc.Create.NewFamilyInstance(headPosition1, postHeadSymbol, shoreMain.HostLevel, StructuralType.NonStructural);
            postInst1.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(shoreMain.OffsetFromLevel);
            postInst1.LookupParameter("Lock_Distance").Set(shoreMain.PostLockDistance);
           // ElementTransformUtils.RotateElement(doc, postInst1.Id, Line.CreateBound(headPosition1, headPosition2 + XYZ.BasisZ), Math.Atan2(shoreMain.UVector.Y, shoreMain.UVector.X));

            var postInst2 = doc.Create.NewFamilyInstance(headPosition2, postHeadSymbol, shoreMain.HostLevel, StructuralType.NonStructural);
            postInst2.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(shoreMain.OffsetFromLevel);
            postInst2.LookupParameter("Lock_Distance").Set(shoreMain.PostLockDistance);
            //ElementTransformUtils.RotateElement(doc, postInst2.Id, Line.CreateBound(headPosition2, headPosition2 + XYZ.BasisZ), Math.Atan2(shoreMain.UVector.Y, shoreMain.UVector.X));

            for (int i = 0; i < shoreMain.NoOfMains; i++)
            {
                var reqOffset = shoreMain.OffsetFromLevel + Database.SHORE_MAIN_HEIGHT.CmToFeet() * i + shoreMain.PostLockDistance;
                var shoreMainInst = doc.Create.NewFamilyInstance(shoreMain.Position, shoreMainSymbol, shoreMain.HostLevel, StructuralType.NonStructural);
                shoreMainInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(reqOffset);
                ElementTransformUtils.RotateElement(doc, shoreMainInst.Id, Line.CreateBound(shoreMain.Position, shoreMain.Position + XYZ.BasisZ), Math.Atan2(rotationVector.Y, rotationVector.X));
            }

            var shoreTelescopicInst = doc.Create.NewFamilyInstance(shoreMain.Position, shoreTelescopicSymbol, shoreMain.HostLevel, StructuralType.NonStructural);
            shoreTelescopicInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(shoreMain.TelescopicOffset);
            ElementTransformUtils.RotateElement(doc, shoreTelescopicInst.Id, Line.CreateBound(shoreMain.Position, shoreMain.Position + XYZ.BasisZ), Math.Atan2(rotationVector.Y, rotationVector.X));

            var uHeadInst1 = doc.Create.NewFamilyInstance(headPosition1, uHeadSymbol, shoreMain.HostLevel, StructuralType.NonStructural);
            uHeadInst1.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(shoreMain.TelescopicOffset + Database.SHORE_TELESCOPIC_HEIGHT.CmToFeet() + shoreMain.ULockDistance);
            uHeadInst1.LookupParameter("Lock_Distance").Set(shoreMain.ULockDistance);
            ElementTransformUtils.RotateElement(doc, uHeadInst1.Id, Line.CreateBound(headPosition1, headPosition1 + XYZ.BasisZ), Math.Atan2(shoreMain.UVector.Y, shoreMain.UVector.X));

            var uHeadInst2 = doc.Create.NewFamilyInstance(headPosition2, uHeadSymbol, shoreMain.HostLevel, StructuralType.NonStructural);
            uHeadInst2.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(shoreMain.TelescopicOffset + +Database.SHORE_TELESCOPIC_HEIGHT.CmToFeet() + shoreMain.ULockDistance);
            uHeadInst2.LookupParameter("Lock_Distance").Set(shoreMain.ULockDistance);
            ElementTransformUtils.RotateElement(doc, uHeadInst2.Id, Line.CreateBound(headPosition2, headPosition2 + XYZ.BasisZ), Math.Atan2(shoreMain.UVector.Y, shoreMain.UVector.X));

            return shoreMain;
        }

        private static RevitShoreBracing Draw(this RevitShoreBracing bracing, Document doc)
        {
            var bracingSymbol = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                                                                 .Cast<FamilySymbol>()
                                                                 .First(sym => sym.Name == RevitBase.CROSS_BRACE)
                                                                 .ActivateIfNot();

            for (int i = 0; i < bracing.NoOfMains; i++)
            {
                var reqOffset = bracing.Offset + Database.SHORE_MAIN_HEIGHT.CmToFeet() * i;

                var bracingInst = doc.Create.NewFamilyInstance(bracing.LocationPoint, bracingSymbol, bracing.HostLevel, StructuralType.NonStructural);
                bracingInst.LookupParameter("W").Set(bracing.Width);
                bracingInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(reqOffset);

                var rotationVector = bracing.Direction.CrossProduct(XYZ.BasisZ);

                ElementTransformUtils.RotateElement(doc, bracingInst.Id, Line.CreateBound(bracing.LocationPoint, bracing.LocationPoint + XYZ.BasisZ), Math.Atan2(rotationVector.Y, rotationVector.X));

                //ElementTransformUtils.RotateElement(doc, bracingInst.Id, Line.CreateBound(bracing.LocationPoint, bracing.LocationPoint + XYZ.BasisZ), Math.Atan2(bracing.Direction.Y, bracing.Direction.X) + 3 * Math.PI / 2);
            }


            return bracing;
        }

        public static RevitShore Draw(this RevitShore shore, Document doc)
        {
            shore.Mains.ForEach(v => v.Draw(doc));
            shore.Bracings.ForEach(brace => brace.Draw(doc));
            shore.MainBeams.ForEach(mb => mb.Draw(doc));
            shore.SecondaryBeams.ForEach(sb => sb.Draw(doc));
            return shore;
        }

        public static List<RevitShore> Draw(this List<RevitShore> shores, Document doc) => shores.Select(c => c.Draw(doc)).ToList();

        private static ShoreCreation GetShoreCreationFuncs(this RevitShoreInput input, Level hostLevel, double hostFloorOffset, double clearHeight)
        {
            var mainBeamSection = Database.GetRevitBeamSection(input.MainBeamSection);
            var secBeamSection = Database.GetRevitBeamSection(input.SecondaryBeamSection);
            var plywoodThickness = Database.PlywoodFloorTypes[input.PlywoodSection].Item2;

            var overallClearHeight = clearHeight + 7.3.CmToFeet() - mainBeamSection.Height.CmToFeet() - secBeamSection.Height.CmToFeet() - plywoodThickness.MmToFeet();

            (var postLockDist, var noOfMains, var telescopicOffset, var uLockDist) = Database.SHORE_MAIN_HEIGHT.CmToFeet()
                                                                                             .GetShoreMainLayout(overallClearHeight);

            var mainBeamOffset = telescopicOffset + Database.SHORE_TELESCOPIC_HEIGHT.CmToFeet() + uLockDist - 7.3.CmToFeet() + hostFloorOffset;

            var plywoodOffset = mainBeamOffset + mainBeamSection.Height.CmToFeet() + secBeamSection.Height.CmToFeet() + plywoodThickness.MmToFeet();


            Func<Line, XYZ, RevitShoreMain> mainFunc = (line, uVector) =>
                 new RevitShoreMain(noOfMains, telescopicOffset + hostFloorOffset, postLockDist, uLockDist, line.MidPoint(), hostLevel, hostFloorOffset, uVector);

            Func<DeckingRectangle, Tuple<List<RevitBeam>, List<RevitBeam>>> mainSecBeamsFunc = (rect) =>
            {

                (var mBeams, var sBeams) =  rect.ToBeamsExact(hostLevel, mainBeamOffset, input.SecondaryBeamSpacing, mainBeamSection, secBeamSection);
                return Tuple.Create(mBeams.OffsetMainBeamsForShoreBrace(), sBeams);
            };


            Func<RevitShoreMain, double, List<RevitShoreBracing>> bracingFunc = (main, bracingWidth) =>
             {
                 var bracingLayout = (Width: bracingWidth, OffsetFromLevel: hostFloorOffset + postLockDist + 20.1.CmToFeet());

                 var list = new List<RevitShoreBracing>();

                #region comments
                //var bracing1 = new RevitShoreBracing(main.Position + new XYZ(Database.SHORE_MAIN_HALF_WIDTH.CmToFeet(), 0.0, 0.0), main.UVector.RotateAboutZ(90.0 * Math.PI / 180.0), bracingLayout.Width, bracingLayout.OffsetFromLevel, noOfMains, main.HostLevel);

                //var bracing2 = new RevitShoreBracing(main.Position - new XYZ(Database.SHORE_MAIN_HALF_WIDTH.CmToFeet() * 2.0 - 10.0.CmToFeet(), 0.0, 0.0) + new XYZ(Database.SHORE_MAIN_HALF_WIDTH.CmToFeet(), 0.0, 0.0), main.UVector.RotateAboutZ(90.0 * Math.PI / 180.0), bracingLayout.Width, bracingLayout.OffsetFromLevel, noOfMains, main.HostLevel);
                #endregion

                var rotationVector = main.UVector.CrossProduct(XYZ.BasisZ);
                 var bracingPosition1 = main.Position + rotationVector.Normalize() * Database.SHORE_MAIN_HALF_WIDTH.CmToFeet();
                 var bracingPosition2 = main.Position - rotationVector.Normalize() * (Database.SHORE_MAIN_HALF_WIDTH.CmToFeet() - 10.0.CmToFeet());

                 var bracing1 = new RevitShoreBracing(bracingPosition1, main.UVector, bracingLayout.Width, bracingLayout.OffsetFromLevel, noOfMains, main.HostLevel);

                 var bracing2 = new RevitShoreBracing(bracingPosition2, main.UVector, bracingLayout.Width, bracingLayout.OffsetFromLevel, noOfMains, main.HostLevel);

                 return new List<RevitShoreBracing> { bracing1, bracing2 };
             };

            return new ShoreCreation(mainFunc, bracingFunc, mainSecBeamsFunc);
        }

        public static (double PostLockDist, int NoOfMains, double TelescopicOffset, double ULockDist) GetShoreMainLayout(this double mainHeight, double clearVerticalLength)
        {
            var lockDist = RevitBase.MIN_ULOCK_DISTANCE.CmToFeet();

            var clearMainLength = clearVerticalLength - (lockDist * 2.0); // 4500 //// 4905 - 100 = 4805

            var noOfMains = 0;
            var shoreTelescopicOffset = 0.0;
            var shoreMainTotalHeight = 0.0;
            var shoreTelescopicOverlap = 0.0;

            while (shoreTelescopicOverlap <= 0.0)
            {
                noOfMains++;

                shoreMainTotalHeight = mainHeight * noOfMains; // 1800 * 2 = 3600

                shoreTelescopicOverlap = shoreMainTotalHeight + Database.SHORE_TELESCOPIC_HEIGHT.CmToFeet() - clearMainLength; // 3600 + 1650 - 4500 = 750 //// 3600 + 1650 - 4805 = 445

                shoreTelescopicOffset = shoreMainTotalHeight - shoreTelescopicOverlap; // 3600 - 750 = 2850 //// 3600 - 445 = 3155
            }

            return (PostLockDist: lockDist, NoOfMains: noOfMains, TelescopicOffset: shoreTelescopicOffset + lockDist, ULockDist: lockDist);
        }

        public static Validation<RevitShore> FloorToShore(RevitFloorInput revitInput, RevitFloorShoreInput floorShoreInput)
        {
            var bracingWidth = floorShoreInput.SpacingMain;
            var floorShoreCreation = floorShoreInput.GetShoreCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.FloorClearHeight);

            var floorShore = revitInput.ConcreteFloor.Boundary.OffsetInsideBy(floorShoreInput.BoundaryLinesOffset)
                                                                .Divide(revitInput.MainBeamDir, floorShoreInput.SpacingMain, floorShoreInput.SpacingSecondary + Database.SHORE_MAIN_HALF_WIDTH.CmToFeet() * 2.0)
                                                                .FilterOpenings(revitInput.ConcreteFloor.Openings)
                                                                .Filter(revitInput.Columns.Select(c => c.Item2.CornerPoints.Offset(c.Item1)).ToList())
                                                                .Filter(revitInput.Beams.Select(b => b.ToRectangle(2 * floorShoreInput.BeamOffset)).ToList())
                                                                .ToShore(floorShoreCreation, floorShoreInput.SpacingMain, floorShoreInput.MainBeamTotalLength, floorShoreInput.SecondaryBeamTotalLength, revitInput.AdjustLayout, true, XYZ.Zero);

            return floorShore;
        }

        public static Validation<List<RevitShore>> BeamsToShore(RevitBeamInput revitInput, RevitBeamShoreInput beamShoreInput)
        {
            var bracingWidth = beamShoreInput.SpacingMain;
            var beamsShoreCreation = beamShoreInput.GetShoreCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.Beams.First().ClearHeight);


            var columns = revitInput.Columns.Select(c => c.CornerPoints).ToList();

            var beamsShore = revitInput.Beams.GetBeamsWithClearSpan(revitInput.Columns)
                                             .Divide(beamShoreInput.SpacingMain, Database.SHORE_MAIN_HALF_WIDTH.CmToFeet() * 2.0, Database.ShoreBraceSystemCrossBraces.Select(s => s.CmToFeet()).ToList())
                                             .Select(tuple => tuple.Item2.ToShore(beamsShoreCreation, beamShoreInput.SpacingMain, beamShoreInput.MainBeamTotalLength, beamShoreInput.SecondaryBeamTotalLength, DeckingHelper.AdjustLayout, false, tuple.Item1.GetEndPoint(0)))
                                             .ToList()
                                             .PopOutValidation();
            return beamsShore;
        }

        private static Validation<RevitShore> ToShore(this List<DeckingRectangle> rectangles,
                                                      ShoreCreation shoreCreation,
                                                      double spacingMain,
                                                      double mainBeamTotalLength,
                                                      double secBeamTotalLength,
                                                      Func<List<RevitBeam>, double, Validation<List<RevitBeam>>> beamLayoutAdjustFunc,
                                                      bool isDrawingFloor,
                                                      XYZ refPoint)
        {
            var mains = rectangles.SelectMany(rect => rect.Lines.Where(l => l.Direction.IsParallelTo(rect.SecBeamDir))
                                                    .Select(l => Tuple.Create(l, rect.MainBeamDir)))
                                                    .Distinct(GenericComparer.Create<Tuple<Line, XYZ>>((l1, l2) => l1.Item1.IsEqual(l2.Item1),tuple=>tuple.Item1.GetHash()))
                                                    .ToList()
                                                    .Select(tuple => shoreCreation.MainFunc(tuple.Item1, tuple.Item2))
                                                    .ToList();
            (var mBeams, var sBeams) = rectangles.Aggregate(Tuple.Create(new List<RevitBeam>(), new List<RevitBeam>()), (soFar, current) =>
            {
                var beamsTuple = shoreCreation.MainSecBeamsFunc(current);
                soFar.Item1.AddRange(beamsTuple.Item1);
                soFar.Item2.AddRange(beamsTuple.Item2);
                return soFar;
            });
            var maxLengthMainBeam = mBeams.OrderByDescending(b => b.Length).First();
            var maxLengthSecBeam = sBeams.OrderByDescending(b => b.Length).First();
            if (mainBeamTotalLength < maxLengthMainBeam.Length + 2 * MIN_CANTILEVER_LENGTH.CmToFeet() || secBeamTotalLength < maxLengthSecBeam.Length + 2 * MIN_CANTILEVER_LENGTH.CmToFeet())
                return ShortBeam;

            var mainBeamsMerged = beamLayoutAdjustFunc(mBeams, mainBeamTotalLength);

            var newSBeams = mBeams.Distinct(RevitBeamComparer).ToColinears()
                                  .Select(g => g.ReduceColinearBeams(g.Sum(m => m.Length)))
                                  .ToList()
                                  .PopOutValidation().Map(bs=>bs
                                  .MatchMainBeams()
                                  .SelectMany(rect => shoreCreation.MainSecBeamsFunc(rect).Item2)
                                  .ToList());

            var secBeamsMerged = newSBeams.Bind(bs=> beamLayoutAdjustFunc(bs, secBeamTotalLength));

            #region comments
            //var bracings000 = mains.GroupBy(m => (int)(m.Position.Y * RevitBase.FORMWORK_NUMBER))
            //                          .OrderBy(kvp => kvp.Key)
            //                          .Select(kvp => kvp.OrderBy(m => m.Position.X).ToList())
            //                          .Select(ms => ms.FilterFirstBracingInGroup())
            //                          .Select(ms => ms.FilterUnNecessaryBracings(spacingMain))
            //                          .SelectMany(ms => ms)
            //                          .SelectMany(shoreCreation.BracingFunc)
            //                          .ToList();


            //var bracings = mains.Where(m => m.UVector.Normalize().IsParallelTo(XYZ.BasisX))
            //                     .GroupBy(m => (int)(m.Position.Y * RevitBase.FORMWORK_NUMBER))
            //                     .OrderBy(kvp => kvp.Key)
            //                     .Select(kvp => kvp.OrderBy(m => m.Position.X).ToList())
            //                     .Select(ms => ms.FilterFirstBracingInGroup())
            //                     .Select(ms => ms.FilterUnNecessaryBracings(spacingMain))
            //                     .SelectMany(ms => ms)
            //                     .SelectMany(shoreCreation.BracingFunc)
            //                     .ToList();
            #endregion


            if (isDrawingFloor)
            {
                var bracingsMains = mains.FilterForBracings(spacingMain, rectangles.First().MainBeamDir);
                var bracings = bracingsMains.SelectMany(main => shoreCreation.BracingFunc(main, spacingMain))
                                        .ToList();
                var result  = from validMainBeams in mainBeamsMerged
                              from validSecBeams in secBeamsMerged
                              select new RevitShore(mains, bracings, validMainBeams, validSecBeams);
                return result;
            }
            else
            {
                var bracingsMains = mains.FilterForBracings(refPoint);
                var bracings = bracingsMains.SelectMany(tuple => shoreCreation.BracingFunc(tuple.Item1, tuple.Item2))
                                        .ToList();

                var result = from validMainBeams in mainBeamsMerged
                             from validSecBeams in secBeamsMerged
                             select new RevitShore(mains, bracings, validMainBeams, validSecBeams);
                return result;
            }
            //var bracingsMains = isDrawingFloor ? mains.FilterForBracings(spacingMain) : mains.FilterForBracings(refPoint);
            //var bracings = bracingsMains.SelectMany(main => shoreCreation.BracingFunc(main ,spacingMain))
            //                            .ToList();
            #region comments
            //var bracings = mains
            //.GroupBy(m => (int)(m.Position.X * RevitBase.FORMWORK_NUMBER))
            //.OrderBy(kvp => kvp.Key)
            //.Select(kvp => kvp.OrderBy(m => m.Position.Y).ToList())
            //.Select(ms => ms.FilterFirstBracingInGroup())
            //.Select(ms => ms.FilterUnNecessaryBracings(spacingMain))
            //.SelectMany(ms => ms)
            //.SelectMany(shoreCreation.BracingFunc)
            //.ToList();
            #endregion

            //return new RevitShore(mains, bracings, mainBeamsMerged, secBeamsMerged);

            #region OldCode

            //var bracingGroupsX = mains.GroupBy(m => (int)(m.Position.Y * RevitBase.FORMWORK_NUMBER))
            //                     .OrderBy(kvp => kvp.Key)
            //                     .Select(kvp => kvp.ToList())
            //                     .Skip(1)
            //                     .SelectMany(ms => ms)
            //                     .GroupBy(m => (int)(m.Position.X * RevitBase.FORMWORK_NUMBER))
            //                     .OrderBy(kvp => kvp.Key)
            //                     .Select(kvp => kvp.ToList())
            //                     .ToList();

            //foreach (var brGroup in bracingGroupsX)
            //{
            //    brGroup.Remove(brGroup[0]);

            //    for (int i = 1; i < brGroup.Count; i++)
            //    {
            //        if (brGroup[i].Position.Y - brGroup[i - 1].Position.Y > spacingSecondary)
            //            brGroup.Remove(brGroup[i]);
            //    }
            //}
            //var bracings = bracingGroupsX.SelectMany(br => br)
            //                 .SelectMany(shoreCreation.BracingFunc)
            //                 .ToList();

            //var bracings = mains.GroupBy(m => (int)(m.Position.Y * RevitBase.FORMWORK_NUMBER))
            //                     .OrderBy(kvp => kvp.Key)
            //                     .Select(kvp => kvp.ToList())
            //                     .Skip(1)
            //                     .SelectMany(ms => ms)
            //                     .GroupBy(m => (int)(m.Position.X * RevitBase.FORMWORK_NUMBER))
            //                     .OrderBy(kvp => kvp.Key)
            //                     .Select(kvp => kvp.ToList())
            //                     .ToList();
            //.SelectMany(shoreCreation.BracingFunc)
            //.ToList();

            //var bracings = mains.GroupBy(m => (int)(m.Position.Y * RevitBase.FORMWORK_NUMBER))
            //                     .OrderBy(kvp => kvp.Key)
            //                     .Select(kvp => kvp.ToList())
            //                     .Skip(1)
            //                     .Aggregate((Tuple.Create(new List<List<RevitShoreMain>>(), new XYZ())), (soFar, current) =>
            //                     {
            //                         var currentPos = current.FirstOrDefault().Position;
            //                         soFar.Item1
            //                          soFar.Item1.Add(current);
            //                         soFar.Item2.Add(currentPos);
            //                         return soFar;
            //                     }
            //).ToList();

            #endregion
        }


        public static List<RevitShore> FilterFromExtesnionBoundries(this List<RevitShore> shores, List<Line> extensionBoundries)
        {
            var zeroZLines = extensionBoundries.Select(l => l.CopyWithNewZ(0))
                                               .ToList();
            return shores.Select(shore => shore.FilterFromExtesnionBoundries(zeroZLines))
                           .ToList();
        }

        public static RevitShore FilterFromExtesnionBoundries(this RevitShore shore, List<Line> extensionBoundries)
        {
            var zeroZLines = extensionBoundries.Select(l => l.CopyWithNewZ(0)).ToList();
            Func<XYZ, List<Line>, bool> isPointOnAnyLine = (p, lines) => lines.Any(l => p.IsOnLine(l));
            var filteredMains = shore.Mains.Where(v => !isPointOnAnyLine(v.Position.CopyWithNewZ(0), zeroZLines))
                                                     .ToList();

            var filteredBraces = shore.Bracings.Where(b => !isPointOnAnyLine(b.LocationPoint.CopyWithNewZ(0), zeroZLines.Where(bl => bl.Direction.IsParallelTo(b.Direction)).ToList()))
                                                 .ToList();

            var filteredMainBeams = shore.MainBeams.Where(b => !isPointOnAnyLine(b.StartPoint.CopyWithNewZ(0), zeroZLines.Where(bl => bl.Direction.IsParallelTo(b.Direction)).ToList()))
                                                     .ToList();
            var filteredSecBeams = shore.SecondaryBeams.Where(b => !isPointOnAnyLine(b.StartPoint.CopyWithNewZ(0), zeroZLines.Where(bl => bl.Direction.IsParallelTo(b.Direction)).ToList()))
                                                         .ToList();
            return new RevitShore(filteredMains, filteredBraces, filteredMainBeams, filteredSecBeams);
        }

        private static List<RevitShoreMain> FilterUnNecessaryBracings(this List<RevitShoreMain> bracingGroup, double filterDistance)
        {
            var tolerance = 0.164042; // 5cm to feet

            //for (int i = 1; i < bracingGroup.Count; i++)
            //{
            //    if ((bracingGroup[i].Position.Y - bracingGroup[i - 1].Position.Y) > (filterDistance + tolerance))
            //        bracingGroup.Remove(bracingGroup[i]);
            //}

            //for (int i = 1; i < bracingGroup.Count; i++)
            //{
            //    if ((bracingGroup[i].UVector.Y - bracingGroup[i - 1].UVector.Y) > (filterDistance + tolerance))
            //        bracingGroup.Remove(bracingGroup[i]);
            //}

            for (int i = 1; i < bracingGroup.Count; i++)
            {
                //if (isY)
                //{
                if ((bracingGroup[i].Position.X - bracingGroup[i - 1].Position.X) > (filterDistance + tolerance))
                    bracingGroup.Remove(bracingGroup[i]);
                //}
                //else
                //{
                //    if ((bracingGroup[i].Position.Y - bracingGroup[i - 1].Position.Y) > (filterDistance + tolerance))
                //        bracingGroup.Remove(bracingGroup[i]);
                //}

                //else if (isY && !isDrawingFloor)
                //{
                //    if ((bracingGroup[i].Position.X - bracingGroup[i - 1].Position.X) > (filterDistance + tolerance))
                //        bracingGroup.Remove(bracingGroup[i - 1]);
                //}
                //else
                //{
                //    if ((bracingGroup[i].Position.Y - bracingGroup[i - 1].Position.Y) > (filterDistance + tolerance))
                //        bracingGroup.Remove(bracingGroup[i - 1]);
                //}
            }

            return bracingGroup;
        }

        private static List<RevitShoreMain> FilterFirstBracingInGroup(this List<RevitShoreMain> bracingGroup)
        {
            if (bracingGroup.Count > 1)
            {
                //if (isDrawingFloor)
                bracingGroup.Remove(bracingGroup[0]);
                //else
                //    bracingGroup.Remove(bracingGroup[bracingGroup.Count - 1]);
            }

            //if (bracingGroup.Count > 1)
            //    bracingGroup.Remove(bracingGroup[bracingGroup.Count - 1]);

            return bracingGroup;
        }

        private static List<RevitShoreMain> FilterForBracings(this List<RevitShoreMain> allFloorMains, double filterDistance, XYZ mainBeamDir)
        {
            var theta = Math.Atan2(mainBeamDir.Y, mainBeamDir.X);

            var mains = allFloorMains.Where(m => m.UVector.Normalize().IsParallelTo(mainBeamDir))
                                 .Select(main => Tuple.Create(main, main.Position.RotateAboutZ(-theta)))
                                 .GroupBy(m => (int)(m.Item2.Y * FORMWORK_NUMBER))
                                 .OrderBy(kvp => kvp.Key)
                                 .Select(kvp => kvp.OrderBy(m => m.Item2.X).ToList())
                                 .Select(ms => ms.Select(tuple => tuple.Item1).ToList().FilterFirstBracingInGroup())
                                 .Select(ms => ms.FilterUnNecessaryBracings(filterDistance))
                                 .SelectMany(ms => ms)
                                 .ToList();

            return mains;

            #region comments
            //var mainsX = allFloorMains.Where(m => m.UVector.Normalize().IsParallelTo(XYZ.BasisY))
            //                     .GroupBy(m => (int)(m.Position.X * RevitBase.FORMWORK_NUMBER))
            //                     .OrderBy(kvp => kvp.Key)
            //                     .Select(kvp => kvp.OrderBy(m => m.Position.Y).ToList())
            //                     .Select(ms => ms.FilterFirstBracingInGroup())
            //                     .Select(ms => ms.FilterUnNecessaryBracings(filterDistance, false))
            //                     .SelectMany(ms => ms)
            //                     .ToList();

            //return mainsY.Concat(mainsX).ToList();
            #endregion
        }

        private static List<Tuple<RevitShoreMain, double>> FilterForBracings(this List<RevitShoreMain> beamMains, XYZ beamStartPoint)
        {
            var mains = beamMains.OrderBy(m => m.Position.DistanceTo(beamStartPoint))
                                 .Skip(1)
                                 .Select((m, i) => Tuple.Create(m, m.Position.DistanceTo(beamMains[i].Position)))
                                 //.FilterFirstBracingInGroup()
                                 .ToList();
            return mains;

            #region comments
            /*.Where(m => m.UVector.Normalize().IsParallelTo(XYZ.BasisX))*/
            //.GroupBy(m => (int)(m.Position.Y * RevitBase.FORMWORK_NUMBER))
            //.OrderBy(kvp => kvp.Key)

            //var mainsX = beamMains.Where(m => m.UVector.Normalize().IsParallelTo(XYZ.BasisY))
            //                     .GroupBy(m => (int)(m.Position.X * RevitBase.FORMWORK_NUMBER))
            //                     .OrderBy(kvp => kvp.Key)
            //                     .Select(kvp => kvp.OrderBy(m => m.Position.Y).ToList())
            //                     .Select(ms => ms.FilterFirstBracingInGroup())
            //                     .SelectMany(ms => ms)
            //                     .ToList();
            #endregion
        }
    }
}
