using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using CSharp.Functional.Constructs;
using FormworkOptimize.Core.Comparers;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS.Internal;
using FormworkOptimize.Core.DTOS.Revit.Input;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static CSharp.Functional.Extensions.ValidationExtension;
using static CSharp.Functional.Functional;
using static FormworkOptimize.Core.Errors.Errors;
using static FormworkOptimize.Core.Constants.RevitBase;
using static FormworkOptimize.Core.Comparers.Comparers;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Helpers.RevitHelper
{
    public static class CuplockShoringHelper
    {
        public static List<RevitCuplock> FilterFromExtesnionBoundries(this List<RevitCuplock> cuplocks, List<Line> extensionBoundries)
        {
            var zeroZLines = extensionBoundries.Select(l => l.CopyWithNewZ(0))
                                               .ToList();
            return cuplocks.Select(cup => cup.FilterFromExtesnionBoundries(zeroZLines))
                           .ToList();
        }

        /// <summary>
        /// Filter Cuplock shoring elements that is on extension boundry lines.
        /// </summary>
        /// <param name="cuplock"></param>
        /// <param name="extensionBoundries"></param>
        /// <returns></returns>
        public static RevitCuplock FilterFromExtesnionBoundries(this RevitCuplock cuplock, List<Line> extensionBoundries)
        {
            var zeroZLines = extensionBoundries.Select(l => l.CopyWithNewZ(0)).ToList();
            Func<XYZ, List<Line>, bool> isPointOnAnyLine = (p, lines) => lines.Any(l => p.IsOnLine(l));
            var filteredVerticals = cuplock.Verticals.Where(v => !isPointOnAnyLine(v.Position.CopyWithNewZ(0), zeroZLines))
                                                     .ToList();
            var filteredLedgers = cuplock.Ledgers.Where(l => !isPointOnAnyLine(l.StartPoint.CopyWithNewZ(0), zeroZLines.Where(bl => bl.Direction.IsParallelTo(l.Direction)).ToList()))
                                                 .ToList();
            var filteredBraces = cuplock.Bracings.Where(b => !isPointOnAnyLine(b.LocationPoint.CopyWithNewZ(0), zeroZLines.Where(bl => bl.Direction.IsParallelTo(b.Direction)).ToList()))
                                                 .ToList();
            var filteredMainBeams = cuplock.MainBeams.Where(b => !isPointOnAnyLine(b.StartPoint.CopyWithNewZ(0), zeroZLines.Where(bl => bl.Direction.IsParallelTo(b.Direction)).ToList()))
                                                     .ToList();
            var filteredSecBeams = cuplock.SecondaryBeams.Where(b => !isPointOnAnyLine(b.StartPoint.CopyWithNewZ(0), zeroZLines.Where(bl => bl.Direction.IsParallelTo(b.Direction)).ToList()))
                                                         .ToList();
            return new RevitCuplock(filteredVerticals, filteredLedgers, filteredBraces, filteredMainBeams, filteredSecBeams);
        }

        public static Validation<RevitCuplock> FloorToCuplock(RevitFloorInput revitInput, RevitFloorCuplockInput floorCuplockInput , List<double> availableLedgers , List<double> verticalDb , List<double> bracesDb)
        {
            var floorCuplockCreation = floorCuplockInput.GetCuplockCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.FloorClearHeight,verticalDb,bracesDb);

            //var database = Database.LedgerLengths.Select(l => l.CmToFeet()).ToList();

            var floorCuplock = revitInput.ConcreteFloor.Boundary.OffsetInsideBy(floorCuplockInput.BoundaryLinesOffset)
                                                                .DivideOptimized(revitInput.MainBeamDir, floorCuplockInput.LedgersMainDir, floorCuplockInput.LedgersSecondaryDir, availableLedgers)
                                                                .FilterBeamsOptimized(revitInput.Beams, floorCuplockInput.BeamOffset, availableLedgers)
                                                                .FilterOpenings(revitInput.ConcreteFloor.Openings)
                                                                .Filter(revitInput.Columns.Select(c => c.Item2.CornerPoints.Offset(c.Item1)).ToList())
                                                                .ToCuplock(floorCuplockCreation, floorCuplockInput.MainBeamTotalLength, floorCuplockInput.SecondaryBeamTotalLength, revitInput.AdjustLayout);

            return floorCuplock;
        }

        public static Validation<List<RevitCuplock>> ColumnsToCuplock(RevitColumnInput revitInput, RevitColumnCuplockInput columnCuplockInput)
        {
            var finalcuplocks = new List<Validation<RevitCuplock>>();
            if (revitInput.ColumnsWNoDrop.Count > 0)
            {
                var floorCuplockCreation = columnCuplockInput.GetCuplockCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.FloorClearHeight,Database.CuplockVerticalLengths,Database.CuplockCrossBraceLengths, true);
                var floorcuplocks = revitInput.ColumnsWNoDrop.Select(c => c.Item1.ToCuplock(floorCuplockCreation, c.Item1.CornerPoints.Offset(c.Item2)));
                finalcuplocks.AddRange(floorcuplocks);
            }

            if (revitInput.ColumnsWDrop.Count > 0)
            {
                var dropCuplockCreation = columnCuplockInput.GetCuplockCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.DropClearHeight, Database.CuplockVerticalLengths, Database.CuplockCrossBraceLengths);
                var dropcuplocks = revitInput.ColumnsWDrop.Select(c => c.ToCuplock(dropCuplockCreation, c.Drop));
                finalcuplocks.AddRange(dropcuplocks);
            }

            return finalcuplocks.PopOutValidation();
        }

        public static Validation<List<RevitCuplock>> BeamsToCuplock(RevitBeamInput revitInput, RevitBeamCuplockInput beamCuplockInput)
        {
            var beamsCuplockCreation = beamCuplockInput.GetCuplockCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.Beams.First().ClearHeight, Database.CuplockVerticalLengths, Database.CuplockCrossBraceLengths);
            var columns = revitInput.Columns.Select(c => c.CornerPoints).ToList();
            var beamsCuplock = revitInput.Beams.GetBeamsWithClearSpan(revitInput.Columns)
                                               .Divide(beamCuplockInput.LedgersMainDir, beamCuplockInput.LedgersSecondaryDir, Database.LedgerLengths.Select(l => l.CmToFeet()).ToList())
                                               .Select(tuple => tuple.Item2.ToCuplock(beamsCuplockCreation, beamCuplockInput.MainBeamTotalLength, beamCuplockInput.SecondaryBeamTotalLength, DeckingHelper.AdjustLayout))
                                               .ToList()
                                               .PopOutValidation();

            return beamsCuplock;
        }

        private static (List<ConcreteColumn> ColumnsFloor, List<ConcreteColumn> ColumnsBeam) SplitColumns(List<ConcreteColumn> columns, List<ConcreteBeam> beams, double offset)
        {
            var beamRects = beams.Select(b => b.ToRectangle(offset));
            var splittedColumns = columns.Aggregate(Tuple.Create(new List<ConcreteColumn>(), new List<ConcreteColumn>()), (soFar, current) =>
            {
                var columnRect = current.CornerPoints.Offset(offset);
                var isIntersect = beamRects.Any(beamRect => columnRect.IsBooleanIntersectWith(beamRect));
                if (isIntersect)
                    soFar.Item2.Add(current);
                else
                    soFar.Item1.Add(current);
                return soFar;
            });
            return (ColumnsFloor: splittedColumns.Item1, ColumnsBeam: splittedColumns.Item2);
        }

        /// <summary>
        /// Generic Filter that applies on Floor Rectangles.
        /// </summary>
        /// <param name="rectangles"></param>
        /// <param name="elements"></param>
        /// <param name="intersectionFunc"></param>
        /// <returns></returns>

        /// <summary>
        /// Get Vertical cuplock layout.
        /// </summary>
        /// <param name="verticalDBLengths">Database vertical lengths</param>
        /// <param name="clearVerticalLength">Clear vertical distance between floor and decking.</param>
        /// <param name="minLockDist"></param>
        /// <param name="maxLockDist"></param>
        /// <returns></returns>
        public static (double PostLockDist, List<double> OrderedVLengths, double ULockDist) GetCuplockVerticalLayout(this List<double> verticalDBLengths, double clearVerticalLength)
        {
            Func<double, bool> canSelect = dbLength =>
             {
                 return 2 * RevitBase.MIN_ULOCK_DISTANCE.CmToFeet() <= clearVerticalLength - dbLength && clearVerticalLength - dbLength <= 2 * RevitBase.MAX_ULOCK_DISTANCE.CmToFeet();
             };
            var available = verticalDBLengths.Where(canSelect).ToList();
            if (available.Count > 0)
            {
                var cuplockVLength = available.First();
                var lockDist = (clearVerticalLength - cuplockVLength) / 2;
                return (PostLockDist: lockDist, OrderedVLengths: new List<double>() { cuplockVLength }, ULockDist: lockDist);
            }
            else
            {
                var length = clearVerticalLength - 2 * RevitBase.MIN_ULOCK_DISTANCE.CmToFeet();
                var orderedVLengths = verticalDBLengths.OrderByDescending(v => v).Aggregate(new List<(double Length, int Amount)>(), (soFar, current) =>
                   {
                       if (length < RevitBase.TOLERANCE)
                           return soFar;
                       var amount = (int)(length / current);
                       if (amount == 0 || current - clearVerticalLength > 2 * RevitBase.MAX_ULOCK_DISTANCE.CmToFeet())
                           return soFar;
                       soFar.Add((Length: current, Amount: amount));
                       length = length - amount * current;
                       return soFar;
                   }).SelectMany(tuple => Enumerable.Range(0, tuple.Amount).Select(_ => tuple.Length)).ToList();
                var lockDist = (clearVerticalLength - orderedVLengths.Sum()) / 2;
                return (PostLockDist: lockDist, OrderedVLengths: orderedVLengths, ULockDist: lockDist);
            }
        }

        private static CuplockCreation GetCuplockCreationFuncs(this RevitCuplockInput input, Level hostLevel, double hostFloorOffset, double clearHeight,List<double> verticalDb,List<double> bracesDb, bool isColumn = false)
        {
            var mainBeamSection = Database.GetRevitBeamSection(input.MainBeamSection);
            var secBeamSection = Database.GetRevitBeamSection(input.SecondaryBeamSection);
            var plywoodThickness = Database.PlywoodFloorTypes[input.PlywoodSection].Item2;

            var overallClearHeight = clearHeight + 7.3.CmToFeet() - mainBeamSection.Height.CmToFeet() - secBeamSection.Height.CmToFeet() - plywoodThickness.MmToFeet();
            (var postLockDist, var vLengths, var uLockDist) = verticalDb.Select(vl => vl.CmToFeet())
                                                                                             .ToList()
                                                                                             .GetCuplockVerticalLayout(overallClearHeight);
            var mainBeamOffset = postLockDist + vLengths.Sum() + uLockDist - 7.3.CmToFeet() + hostFloorOffset;
            var plywoodOffset = mainBeamOffset + mainBeamSection.Height.CmToFeet() + secBeamSection.Height.CmToFeet() + plywoodThickness.MmToFeet();


            Func<XYZ, XYZ, RevitCuplockVertical> verticalFunc = (point, uVector) =>
                 new RevitCuplockVertical(vLengths, postLockDist, uLockDist, point, hostLevel, hostFloorOffset, uVector, input.SteelType);

            Func<DeckingRectangle, Tuple<List<RevitBeam>, List<RevitBeam>>> mainSecBeamsFunc = (rect) =>
            {
                return isColumn ? rect.ToBeams(hostLevel, mainBeamOffset, input.SecondaryBeamSpacing, mainBeamSection, secBeamSection)
                                         : rect.ToBeamsExact(hostLevel, mainBeamOffset, input.SecondaryBeamSpacing, mainBeamSection, secBeamSection);
            };



            Func<FormworkRectangle, List<RevitLedger>> ledgersFunc = rectangle =>
            {
                var socketIndicies = GetLedgersSocketIndicies(vLengths.Sum());
                var ledgers = rectangle.RectToLedgers(hostLevel, hostFloorOffset + postLockDist, socketIndicies, input.SteelType);
                return ledgers;
            };

            Func<RevitLedger, RevitCuplockBracing> bracingFunc = ledger =>
            {
                var bracingLayout = ledger.OffsetsFromLevel.Take(ledger.OffsetsFromLevel.Count - 1).Select((offset, index) =>
                {
                    var ledgerHeight = ledger.OffsetsFromLevel[index + 1] - offset;
                    var requiredLength = Math.Sqrt(ledgerHeight * ledgerHeight + ledger.Length * ledger.Length);
                    var choosenLength = bracesDb.Select(l => l.CmToFeet())
                                                                         .First(brace => brace >= requiredLength);

                    var theta = Math.Atan(ledgerHeight / ledger.Length);
                    var width = choosenLength * Math.Cos(theta);
                    var height = choosenLength * Math.Sin(theta);
                    return (Width: width, Height: height, OffsetFromLevel: offset);
                }).ToList();
                return new RevitCuplockBracing(ledger.StartPoint, (ledger.EndPoint - ledger.StartPoint).Normalize(), bracingLayout, ledger.HostLevel);
            };
            return new CuplockCreation(verticalFunc, ledgersFunc, bracingFunc, mainSecBeamsFunc);
        }

        private static FormworkRectangle ToRectWithDBLedgers(this FormworkRectangle rect)
        {
            var ledgersInFeet = Database.LedgerLengths.Select(l => l.CmToFeet())
                                                      .OrderByDescending(l => l)
                                                      .ToList();
            var xLedger = ledgersInFeet.FirstOrDefault(l => l <= rect.LengthX);
            var yLedger = ledgersInFeet.FirstOrDefault(l => l <= rect.LengthY);

            var xVec = (xLedger / 2) * XYZ.BasisX;
            var yVec = (yLedger / 2) * XYZ.BasisY;

            var p0 = rect.Center - xVec - yVec;
            var p1 = rect.Center - xVec + yVec;
            var p2 = rect.Center + xVec + yVec;
            var p3 = rect.Center + xVec - yVec;

            return new FormworkRectangle(p0, p1, p2, p3);
        }

        private static Validation<RevitCuplock> ToCuplock(this ConcreteColumn column,
                                               CuplockCreation cuplockCreation,
                                               FormworkRectangle cuplockBaseRect)
        {
            var cuplockRectangle = cuplockBaseRect.ToRectWithDBLedgers();

            var verticals = cuplockRectangle.Points.Select(p => cuplockCreation.VerticalFunc(p, XYZ.BasisX))
                                            .ToList();


            (var mainBeams, var secBeams) = cuplockCreation.MainSecBeamsFunc(new DeckingRectangle(cuplockRectangle.Points, XYZ.BasisX));
            var secBeamsB = secBeams.First().Section.Breadth.CmToFeet();
            var secBeamsWoColumnInt = secBeams.Where(b => !b.IsBeamIntersectWithRectangle(column.CornerPoints)).ToList();
            var mainBeamSec = mainBeams.First().Section.SectionName;
            var secBeamSec = secBeams.First().Section.SectionName;


            Func<double, double, Validation<RevitCuplock>> tocuplock = (mainLength, secLength) =>
               {
                   var mergedMainBeams = mainBeams.ToColinears()
                                             .SelectMany(group => group.MergeColinearBeams(mainLength))
                                             .ToList()
                                             .PopOutValidation();

                   var mergedSecBeams = secBeamsWoColumnInt.ToColinears()
                                                           .SelectMany(group => group.MergeColinearBeams(secLength))
                                                           .ToList()
                                                           .PopOutValidation();

                   var ledgers = cuplockCreation.LedgersFunc(cuplockRectangle);

                   var bracings = ledgers.Select(cuplockCreation.BracingFunc)
                                         .ToList();
                 var result =   from vm in mergedMainBeams
                                from vs in mergedSecBeams
                                select new RevitCuplock(verticals, ledgers, bracings, vm, vs);
                   return result;
               };


            var cuplock = from mainLength in mainBeamSec.GetNearestDbLength(cuplockRectangle.LengthX)
                          from secLength in secBeamSec.GetNearestDbLength(cuplockRectangle.LengthY)
                          from valid in tocuplock(mainLength, secLength)
                          select valid;

            return cuplock;
        }

        private static Validation<RevitCuplock> ToCuplock(this List<DeckingRectangle> rectangles,
                                              CuplockCreation cuplockCreation,
                                              double mainBeamTotalLength,
                                              double secBeamTotalLength,
                                              Func<List<RevitBeam>, double, Validation<List<RevitBeam>>> beamLayoutAdjustFunc)
                                             

        {
            var verticals = rectangles.SelectMany(rect => rect.Points.Select(p => Tuple.Create(p, rect.MainBeamDir)))
                                                     .Distinct(GenericComparer.Create<Tuple<XYZ, XYZ>>((p1, p2) => p1.Item1.IsEqual(p2.Item1), tuple => tuple.Item1.GetHash()))
                                                     .ToList()
                                                     .Select(tuple => cuplockCreation.VerticalFunc(tuple.Item1, tuple.Item2))
                                                     .ToList();
            (var mBeams, var sBeams) = rectangles.Aggregate(Tuple.Create(new List<RevitBeam>(), new List<RevitBeam>()), (soFar, current) =>
                 {
                     var beamsTuple = cuplockCreation.MainSecBeamsFunc(current);
                     soFar.Item1.AddRange(beamsTuple.Item1);
                     soFar.Item2.AddRange(beamsTuple.Item2);
                     return soFar;
                 });
            var maxLengthMainBeam = mBeams.OrderByDescending(b => b.Length).First();
            var maxLengthSecBeam = sBeams.OrderByDescending(b => b.Length).First();

            if (mainBeamTotalLength < maxLengthMainBeam.Length + 2 * MIN_CANTILEVER_LENGTH.CmToFeet() || secBeamTotalLength < maxLengthSecBeam.Length + 2 * MIN_CANTILEVER_LENGTH.CmToFeet())
                return ShortBeam;

            var mainBeamsMergedValid = beamLayoutAdjustFunc(mBeams, mainBeamTotalLength);

            var validNewSBeams = mBeams.Distinct(RevitBeamComparer).ToColinears()
                                   .Select(g => g.ReduceColinearBeams(g.Sum(m => m.Length)))
                                   .ToList()
                                   .PopOutValidation();
            var newSBeams = validNewSBeams.Map(bs => bs.MatchMainBeams()
                                   .SelectMany(rect => cuplockCreation.MainSecBeamsFunc(rect).Item2)
                                   .ToList());


            var secBeamsMergedValid = newSBeams.Bind(bs => beamLayoutAdjustFunc(bs, secBeamTotalLength));


            Func<List<RevitBeam>, List<RevitBeam>, RevitCuplock> toCup = (mainBeamsMerged, secBeamsMerged) =>
              {
                  var ledgers = rectangles.SelectMany(cuplockCreation.LedgersFunc)
                                      .Distinct(RevitLedgerComparer)
                                      .ToList();


                  var bracings = ledgers.GroupToParallelLines()
                                         .SelectMany(line => line.GetBracedLedgersOnLine())
                                         .Select(cuplockCreation.BracingFunc)
                                         .ToList();

                  return new RevitCuplock(verticals, ledgers, bracings, mainBeamsMerged, secBeamsMerged);
              };

            var result = from validMBeams in mainBeamsMergedValid
                         from validSBeams in secBeamsMergedValid
                         select toCup(validMBeams, validSBeams);
            return result;
        }




        /// <summary>
        /// Get Ledgers positions in cuplock vertical element.
        /// </summary>
        /// <param name="verticalBarLength"></param>
        /// <returns></returns>
        private static List<int> GetLedgersSocketIndicies(double verticalBarLength)
        {

            var noSockets = (int)((verticalBarLength - 50.0.CmToFeet() /*Start Distance + End Distance*/) / 50.0.CmToFeet()) + 1;
            var noLedgers = Math.Max((int)Math.Ceiling((noSockets - 1) * 50.0.CmToFeet() / 200.0.CmToFeet()) + 1, 2);

            var ledgerIncrement = (noSockets - 1) / (noLedgers - 1);
            var socketIndicies = Enumerable.Range(0, noLedgers - 1).Select(i => i * ledgerIncrement).ToList();
            socketIndicies.Add(noSockets - 1);
            return socketIndicies;

        }

        public static List<RevitCuplock> Draw(this List<RevitCuplock> cuplocks, Document doc) =>
            cuplocks.Select(c => c.Draw(doc)).ToList();

        public static RevitCuplock Draw(this RevitCuplock cuplock, Document doc)
        {
            cuplock.Verticals.ForEach(v => v.Draw(doc));
            cuplock.Ledgers.ForEach(ledger => ledger.Draw(doc));
            cuplock.Bracings.ForEach(brace => brace.Draw(doc));
            cuplock.MainBeams.ForEach(mb => mb.Draw(doc));
            cuplock.SecondaryBeams.ForEach(sb => sb.Draw(doc));
            return cuplock;
        }

        private static RevitCuplockVertical Draw(this RevitCuplockVertical cuplock, Document doc)
        {
            var symbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                                                           .Cast<FamilySymbol>();

            var postHeadSymbol = symbols.First(sym => sym.Name == RevitBase.POST_HEAD).ActivateIfNot();
            var cuplockSymbol = symbols.First(sym => sym.Name == RevitBase.CUPLOCK_VERTICAL).ActivateIfNot();
            var uHeadSymbol = symbols.First(sym => sym.Name == RevitBase.U_HEAD).ActivateIfNot();

            var postInst = doc.Create.NewFamilyInstance(cuplock.Position, postHeadSymbol, cuplock.HostLevel, StructuralType.NonStructural);
            postInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(cuplock.OffsetFromLevel);
            postInst.LookupParameter("Lock_Distance").Set(cuplock.PostLockDistance);

            cuplock.OrderedVerticalLengths.ForEach(l =>
            {
                var sumLengthsSoFar = cuplock.OrderedVerticalLengths.Take(cuplock.OrderedVerticalLengths.IndexOf(l)).Sum();
                var cuplockInst = doc.Create.NewFamilyInstance(cuplock.Position, cuplockSymbol, cuplock.HostLevel, StructuralType.NonStructural);
                cuplockInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(cuplock.OffsetFromLevel + sumLengthsSoFar + cuplock.PostLockDistance);
                cuplockInst.LookupParameter("H").Set(l);
            });

            var uHeadInst = doc.Create.NewFamilyInstance(cuplock.Position, uHeadSymbol, cuplock.HostLevel, StructuralType.NonStructural);
            uHeadInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(cuplock.OffsetFromLevel + cuplock.PostLockDistance + cuplock.OrderedVerticalLengths.Sum() + cuplock.ULockDistance);
            uHeadInst.LookupParameter("Lock_Distance").Set(cuplock.ULockDistance);
            ElementTransformUtils.RotateElement(doc, uHeadInst.Id, Line.CreateBound(cuplock.Position, cuplock.Position + XYZ.BasisZ), Math.Atan2(cuplock.UVector.Y, cuplock.UVector.X));

            return cuplock;
        }

        private static RevitLedger Draw(this RevitLedger ledger, Document doc)
        {
            var ledgerSymbol = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                                                                .Cast<FamilySymbol>()
                                                                .First(sym => sym.Name == RevitBase.CUPLOCK_LEDGER)
                                                                .ActivateIfNot();
            ledger.OffsetsFromLevel.ForEach(offset =>
            {
                var ledgerInst = doc.Create.NewFamilyInstance(ledger.StartPoint, ledgerSymbol, ledger.HostLevel, StructuralType.NonStructural);
                ledgerInst.LookupParameter("L").Set(ledger.Length);
                //var socketOffset = ledger.OffsetFromLevel + 10.0.CmToFeet() + (offset * 50.0).CmToFeet()/*+((int)(ledger.SocketNumbers.IndexOf(socket)+1)/ledger.SocketNumbers.Count)*2.0.CmToFeet()*/;
                ledgerInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(offset);
                var vec = ledger.EndPoint - ledger.StartPoint;
                ElementTransformUtils.RotateElement(doc, ledgerInst.Id, Line.CreateBound(ledger.StartPoint, ledger.StartPoint + XYZ.BasisZ), Math.Atan2(vec.Y, vec.X));
            });
            return ledger;
        }

        private static RevitCuplockBracing Draw(this RevitCuplockBracing bracing, Document doc)
        {
            var bracingSymbol = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                                                                 .Cast<FamilySymbol>()
                                                                 .First(sym => sym.Name == RevitBase.CUPLOCK_BRACING)
                                                                 .ActivateIfNot();
            bracing.WidthHeightOffsets.ForEach(tuple =>
                                {
                                    var bracingInst = doc.Create.NewFamilyInstance(bracing.LocationPoint, bracingSymbol, bracing.HostLevel, StructuralType.NonStructural);
                                    bracingInst.LookupParameter("H").Set(tuple.Height);
                                    bracingInst.LookupParameter("W").Set(tuple.Width);
                                    bracingInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(tuple.OffsetFromLevel);
                                    ElementTransformUtils.RotateElement(doc, bracingInst.Id, Line.CreateBound(bracing.LocationPoint, bracing.LocationPoint + XYZ.BasisZ), Math.Atan2(bracing.Direction.Y, bracing.Direction.X) + 3 * Math.PI / 2);
                                });
            return bracing;
        }

        private static List<RevitLedger> GetBracedLedgersOnLine(this List<RevitLedger> line)
        {
            var nLedgers = line.Count();
            var remainder = nLedgers % 4.0;

            if (remainder == 0)
            {
                var nBraced = nLedgers / 4;
                var braced = Enumerable.Range(0, nBraced).Select(i => line[4 * i])
                                                   .ToList();
                if (line.Count != 0)
                    braced.Add(line.Last());
                return braced;
            }
            else if (line.Count == 1)
                return new List<RevitLedger>() { line.First() };
            else if (line.Count <= 4)
                return new List<RevitLedger>() { line.First(), line.Last() };
            else
            {
                //More than 4 with remainder
                var nBraced = (int)Math.Ceiling(nLedgers / 4.0);
                var braced = Enumerable.Range(0, nBraced).Select(i => line[4 * i])
                                                   .ToList();
                return braced;

            }
        }

        private static List<List<RevitLedger>> GroupToParallelLines(this List<RevitLedger> ledgers)
        {
            var ledgersCopy = ledgers.Where(ledger => true).ToList();
            var parallelLedgers = new List<List<RevitLedger>>();
            while (ledgersCopy.Count != 0)
            {

                var origin = ledgersCopy.GetOrigin();

                var nearestLedger = origin.GetNearestLedger(ledgersCopy);
                var nearestLedgerOrigin = origin.DistanceTo(nearestLedger.StartPoint) < origin.DistanceTo(nearestLedger.EndPoint) ? nearestLedger.StartPoint : nearestLedger.EndPoint;
                var onLine = ledgersCopy.Where(led => nearestLedger.IsCollinearWith(led))
                                        .OrderBy(ledger => Math.Min(nearestLedgerOrigin.DistanceTo(ledger.StartPoint), nearestLedgerOrigin.DistanceTo(ledger.EndPoint)))
                                        .ToList();

                if (onLine.Count > 0)
                {
                    parallelLedgers.Add(onLine);
                    onLine.ForEach(l => ledgersCopy.Remove(l));
                }
            }
            return parallelLedgers;
        }

        /// <summary>
        /// Get Point that makes all the ledgers lie in the first Quadrant.
        /// </summary>
        /// <param name="ledgers"></param>
        /// <returns></returns>
        private static XYZ GetOrigin(this List<RevitLedger> ledgers)
        {
            var allPoints = ledgers.SelectMany(ledger => new List<XYZ>() { ledger.StartPoint, ledger.EndPoint })
                                   .ToList();
            var x = allPoints.Min(point => point.X);
            var y = allPoints.Min(point => point.Y);
            var z = allPoints.Min(point => point.Z);
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Check if any two ledgers are on the same straight line or not.
        /// </summary>
        /// <param name="baseLedger"></param>
        /// <param name="ledger"></param>
        /// <returns></returns>
        private static bool IsCollinearWith(this RevitLedger baseLedger, RevitLedger ledger)
        {
            var origin = baseLedger.StartPoint;
            var baseVec = baseLedger.EndPoint - origin;
            var ledgerVec = ledger.EndPoint.DistanceTo(origin) > RevitBase.TOLERANCE ? ledger.EndPoint - origin : ledger.StartPoint - origin;
            return baseLedger.Direction.IsParallelTo(ledger.Direction) && baseVec.IsParallelTo(ledgerVec);
        }

        private static List<RevitLedger> RectToLedgers(this FormworkRectangle rectangle, Level hostLevel, double offsetFromLevel, List<int> socketNumbers, SteelType steelType)
        {
            var offsets = socketNumbers.Select(socket =>
            {
                var socketOffset = offsetFromLevel + 10.0.CmToFeet() + (socket * 50.0).CmToFeet()/*+((int)(ledger.SocketNumbers.IndexOf(socket)+1)/ledger.SocketNumbers.Count)*2.0.CmToFeet()*/;
                return socketOffset;
                //return (SocketIndex: socket, OffsetFromLevel: socketOffset);
            }).ToList();


            var ledgers = rectangle.Points.Aggregate(new List<RevitLedger>(), (soFar, current) =>
             {
                 var i = rectangle.Points.IndexOf(current);
                 var j = (i + 1) % rectangle.Points.Count;
                 var nextPoint = rectangle.Points[j];
                 soFar.Add(new RevitLedger(rectangle.Points[i], rectangle.Points[j], hostLevel, offsets, steelType));
                 return soFar;
             }).ToList();
            return ledgers;
        }

        public static bool IsEqual(this RevitLedger ledger, RevitLedger other)
        {
            var result = (ledger.StartPoint.IsEqual(other.StartPoint) ||
                          ledger.StartPoint.IsEqual(other.EndPoint)) &&
                         (ledger.EndPoint.IsEqual(other.StartPoint) ||
                          ledger.EndPoint.IsEqual(other.EndPoint));
            return result;

        }

    }
}
