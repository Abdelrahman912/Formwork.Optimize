using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.CostParameters;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.RevitHelper;
using FormworkOptimize.Core.SuppressWarnings;
using System;
using System.Collections.Generic;
using System.Linq;
using CSharp.Functional.Extensions;
using Unit = System.ValueTuple;
using CSharp.Functional.Constructs;

namespace FormworkOptimize.Core.Helpers.CostHelper
{
    public static class CostHelper
    {

        public static PlywoodCost AsPlywoodCost(this RevitFloorPlywood floorPlywood,double floorArea , Func<string,double> costFunc)
        {
            var name = floorPlywood.SectionName.GetDescription();
            var costPerArea = costFunc(name);

            var sidesArea = floorPlywood.ConcreteFloorOpenings.SelectMany(os => os.Select(l => l))
                                                              .Concat(floorPlywood.Boundary)
                                                              .Aggregate(0.0, (soFar, current) => soFar += current.Length * floorPlywood.ConcreteFloorThickness)
                                                              .FeetSquareToMeterSquare();
            var totalArea = floorArea + sidesArea;

            Action<Document> draw = (doc) =>
            {
                var openingsDrawFunc = doc.UsingTransaction(trans =>
                {
                    trans.SuppressWarning(SuppressWarningsHelper.FloorsOverlapSuppress);
                    return floorPlywood.DrawBoundary(doc);

                }, "Draw Plywood Boundary");
                doc.UsingTransaction(_ => openingsDrawFunc(), "Draw Plywood Openings");
            };

            Func<Document, Validation<Unit>> loadAndDraw = (doc) =>
             {
                 var result = from floorTypes in doc.UsingTransaction(_ => doc.AddPlywoodElementTypesIfNotExist<FloorType>(), "Add Plywood Floor Types")
                              from wallTypes in doc.UsingTransaction(_ => doc.AddPlywoodElementTypesIfNotExist<WallType>(), "Add Plywood Wall Types")
                              select draw.ToFunc()(doc);
                 return result;
             };
            return new PlywoodCost(costPerArea, totalArea, loadAndDraw);
        }

        public static FormworkTimeLine AsTimeLine(this Time time)
        {
            return new FormworkTimeLine(
                installationDuration: time.InstallationTime.CalculateInstallationTime(),
                smitheryDuration: time.SmitheryTime,
                waitingDuaration: time.WaitingTime.CalculateWaitingTime(),
                removalDuration: time.RemovalTime.CalculateRemovalTime());
        }

        public static TransportationCost AsTransportationCost(this Transportation transportation)
        {
            var cost = transportation.GetCost();
            return new TransportationCost(cost);
        }

        public static ManPowerCost AsManPowerCost(this ManPower manPower, FormworkTimeLine timeLine)
        {
            return new ManPowerCost(
                noWorkers: manPower.NoWorkers.CalculateNoWorkers(),
                laborCost: manPower.LaborCost,
                duration: timeLine.InstallationDuration + timeLine.RemovalDuration
                );
        }

        public static EquipmentsCost AsEquipmentsCost(this Equipments equipments, FormworkTimeLine timeLine)
        {
            return new EquipmentsCost(
                noEquipments: equipments.NoCranes,
                rent: equipments.CraneRent,
                duration: timeLine.InstallationDuration + timeLine.RemovalDuration
                );
        }

        public static FormworkElementsCost AsFormworkElemntsCost(this double dailyCost, FormworkTimeLine timeLine)
        {
            return new FormworkElementsCost(
                cost: dailyCost,
                duration: timeLine.TotalDuration
                );
        }

        public static double TotalCost(this IEnumerable<ElementQuantificationCost> element) =>
            element.Aggregate(0.0, (soFar, current) => soFar + current.TotalCost);

        #region Cuplock

        public static List<ElementQuantificationCost> ToCost(this RevitCuplock cuplock,
                                                                  Func<string, double> costFunc)
        {
            return cuplock.Ledgers.ToCost(costFunc)
                                  .Concat(cuplock.Verticals.ToCost(costFunc))
                                  .Concat(cuplock.Bracings.ToCost(costFunc))
                                  .Concat(cuplock.MainBeams.ToCost(costFunc))
                                  .Concat(cuplock.SecondaryBeams.ToCost(costFunc))
                                  .ToList();
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitLedger> ledgers,
                                                                   Func<string, double> costFunc)
        {
            Func<IGrouping<int, RevitLedger>, ElementQuantificationCost> toCost = kvp =>
              {
                  var firstLedger = kvp.First();
                  var name = firstLedger.ToCostString();
                  var count = kvp.Sum(ledger => ledger.Count);
                  var unitCost = costFunc(name);
                  var totalCost = unitCost * count;
                  return new ElementQuantificationCost(name, count, totalCost, unitCost, UnitCostMeasure.NUMBER,CostType.RENT);
              };

            return ledgers.ToLookup(ledger => (int)(ledger.Length.FeetToMeter().Round(2) * 100))
                          .Select(toCost)
                          .ToList();
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitCuplockVertical> verticals,
                                                         Func<string, double> costFunc)
        {
            var steelType = verticals.First().SteelType;
            Func<IGrouping<int, double>, ElementQuantificationCost> toCost = kvp =>
              {
                  var length = (kvp.Key / 100.0).Round(2).ToString("0.00");
                  var name = $"CupLock Vertical {length} m, ({steelType.GetDescription()})";
                  var unitCost = costFunc(name);
                  var count = kvp.Count();
                  var totalCost = unitCost * count;
                  return new ElementQuantificationCost(name, count, totalCost, unitCost, UnitCostMeasure.NUMBER,CostType.RENT);
              };

            var uHeadName = "U-Head Jack Solid";
            var uHeadCount = verticals.Count();
            var uHeadUnitCost = costFunc(uHeadName);
            var uTotalCost = uHeadUnitCost * uHeadCount;

            var uHeadElementCost = new ElementQuantificationCost(uHeadName, uHeadCount, uTotalCost, uHeadUnitCost, UnitCostMeasure.NUMBER,CostType.RENT);

            var postHeadName = "Post Head Jack Solid";
            var postCount = verticals.Count();
            var postUnitCost = costFunc(postHeadName);
            var postTotalCost = postUnitCost * postCount;

            var postElementCost = new ElementQuantificationCost(postHeadName, postCount, postTotalCost, postUnitCost, UnitCostMeasure.NUMBER,CostType.RENT);

            var verticalPoints = verticals.SelectMany(v => v.OrderedVerticalLengths.Select(l => v.Position.CopyWithNewZ(0)))
                                          .GroupXYZ()
                                          .Where(tuple => tuple.Item2 > 1)
                                          .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2 - 1))
                                          .ToList();

            var elementsCost = verticals.SelectMany(v => v.OrderedVerticalLengths)
                                        .ToLookup(num => (int)(num.FeetToMeter().Round(2) * 100))
                                        .Select(toCost)
                                        .ToList();

            if (verticalPoints.Count > 0)
            {
                var roundSpigotCount = verticalPoints.Sum(tuple => tuple.Item2 * 1);
                var rivetPinAndSpringClipCount = verticalPoints.Sum(tuple => tuple.Item2 * 2);

                var spigotName = "Round Spigot";
                var spigotCost = costFunc(spigotName);
                var spigotTotalCost = spigotCost * roundSpigotCount;
                var spigotElementCost = new ElementQuantificationCost(spigotName, roundSpigotCount, spigotTotalCost, spigotCost, UnitCostMeasure.NUMBER, CostType.RENT);

                var pinName = "Rivet Pin 16mm, L=9cm";
                var pinCost = costFunc(pinName);
                var pinTotalCost = pinCost * rivetPinAndSpringClipCount;
                var pinElementCost = new ElementQuantificationCost(pinName, rivetPinAndSpringClipCount, pinTotalCost, pinCost, UnitCostMeasure.NUMBER, CostType.RENT);

                var clipName = "Spring Clip";
                var clipCost = costFunc(clipName);
                var clipTotalCost = clipCost * rivetPinAndSpringClipCount;
                var clipElementCost = new ElementQuantificationCost(clipName, rivetPinAndSpringClipCount, clipTotalCost, clipCost, UnitCostMeasure.NUMBER, CostType.RENT);

                elementsCost.Add(spigotElementCost);
                elementsCost.Add(pinElementCost);
                elementsCost.Add(clipElementCost);

            }

            elementsCost.Add(uHeadElementCost);
            elementsCost.Add(postElementCost);
            return elementsCost;
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitCuplockBracing> bracings,
                                                        Func<string, double> costFunc)
        {
            Func<IGrouping<int, double>, ElementQuantificationCost> toCost = kvp =>
              {
                  var length = (kvp.Key / 100.0).Round(2).ToString("0.00");
                  var name = $"Scaffolding Tube {length} m";
                  var count = kvp.Count();
                  var unitCost = costFunc(name);
                  var totalCost = unitCost * count;
                  return new ElementQuantificationCost(name, count, totalCost, unitCost, UnitCostMeasure.NUMBER, CostType.RENT);
              };
            var elementsCost = bracings.SelectMany(bracing => bracing.Lengths)
                                       .ToLookup(num => (int)(num.FeetToMeter().Round(2) * 100))
                                       .Select(toCost)
                                       .ToList();
            var couplerName = "Pressed Prop Swivel Coupler";
            var couplerCost = costFunc(couplerName);
            var couplerCount = bracings.Sum(bracing => bracing.WidthHeightOffsets.Count) * 2;
            var couplerTotalCost = couplerCost * couplerCount;

            var couplerElementCost = new ElementQuantificationCost(couplerName, couplerCount, couplerTotalCost, couplerCost, UnitCostMeasure.NUMBER, CostType.RENT);
            elementsCost.Add(couplerElementCost);
            return elementsCost;
        }


        private static string ToCostString(this RevitLedger ledger)
        {
            var length = ledger.Length.FeetToMeter().Round(2).ToString("0.00");
            return $"CupLock Ledger {length} m, ({ledger.SteelType.GetDescription()})";
        }

        #endregion

        #region Props

        public static List<ElementQuantificationCost> ToCost(this RevitProps props,
                                                                  Func<string, double> costFunc)
        {
            return props.Verticals.ToCost(costFunc)
                                  .Concat(props.Legs.ToCost(costFunc))
                                  .Concat(props.SecondaryBeams.ToCost(costFunc))
                                  .Concat(props.MainBeams.ToCost(costFunc))
                                  .ToList();
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitPropsVertical> propsVerticals,
                                                                  Func<string, double> costFunc)
        {
            var elementsCost = new List<ElementQuantificationCost>();

            var propName = $"Acrow {propsVerticals.First().PropType}";
            var cost = costFunc(propName);
            var count = propsVerticals.Count();
            var totalCost = count * cost;

            var propElementCost = new ElementQuantificationCost(propName, count, totalCost, cost, UnitCostMeasure.NUMBER, CostType.RENT);
            elementsCost.Add(propElementCost);

            var uName = "U-Head For Props";
            var uCost = costFunc(uName);
            var uCount = propsVerticals.Count();
            var uTotalCost = uCount * cost;

            var uElementCost = new ElementQuantificationCost(uName, uCount, uTotalCost, uCost, UnitCostMeasure.NUMBER, CostType.RENT);
            elementsCost.Add(uElementCost);

            return elementsCost;
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitPropsLeg> propsLegs,
                                                                  Func<string, double> costFunc)
        {
            var name = "Prop Leg";
            var cost = costFunc(name);
            var count = propsLegs.Count();
            var totalCost = count * cost;
            return new List<ElementQuantificationCost>()
            {
                new ElementQuantificationCost(name,count,totalCost,cost,UnitCostMeasure.NUMBER, CostType.RENT)
            };
        }

        #endregion

        #region ShoreBrace

        public static List<ElementQuantificationCost> ToCost(this RevitShore shore,
                                                                  Func<string, double> costFunc)
        {
            return shore.Mains.ToCost(costFunc)
                              .Concat(shore.Bracings.ToCost(costFunc))
                              .Concat(shore.MainBeams.ToCost(costFunc))
                              .Concat(shore.SecondaryBeams.ToCost(costFunc))
                              .ToList();
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitShoreMain> shoreMains,
                                                               Func<string, double> costFunc)
        {

            //Telescopic, Mains, U-Head, Post Head.
            var elementsCost = new List<ElementQuantificationCost>();

            var mainFrameName = "Shorebrace Frame";
            var mainFrameCost = costFunc(mainFrameName);
            var mainFrameCount = shoreMains.Sum(main => main.NoOfMains);
            var mainFrameTotalCost = mainFrameCost * mainFrameCount;

            var mainFrameElementCost = new ElementQuantificationCost(mainFrameName, mainFrameCount, mainFrameTotalCost, mainFrameCost, UnitCostMeasure.NUMBER, CostType.RENT);
            elementsCost.Add(mainFrameElementCost);

            var teleName = "Shorebrace Telescopic Frame";
            var teleCost = costFunc(teleName);
            var teleCount = shoreMains.Count();
            var teleTotalCost = teleCost * teleCount;

            var teleElementCost = new ElementQuantificationCost(teleName, teleCount, teleTotalCost, teleCost, UnitCostMeasure.NUMBER, CostType.RENT);
            elementsCost.Add(teleElementCost);

            var uName = "U-Head Jack Solid";
            var uCost = costFunc(uName);
            var uCount = shoreMains.Count();
            var uTotalCost = uCost * uCount;

            var uElementCost = new ElementQuantificationCost(uName, uCount, uTotalCost, uCost, UnitCostMeasure.NUMBER, CostType.RENT);
            elementsCost.Add(uElementCost);

            var postName = "Post Head Jack Solid";
            var postCost = costFunc(postName);
            var postCount = shoreMains.Count();
            var postTotalCost = postCost * postCount;

            var postElementCost = new ElementQuantificationCost(postName, postCount, postTotalCost, postCost, UnitCostMeasure.NUMBER, CostType.RENT);
            elementsCost.Add(postElementCost);

            var framesPoints = shoreMains.Select(main => main.Position)
                                         .GroupXYZ()
                                         .Where(tuple => tuple.Item2 > 1)
                                         .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2 - 1))
                                         .ToList();
            if (framesPoints.Count > 0)
            {
                var totalCount = framesPoints.Sum(tuple => tuple.Item2 * 1);

                var pinName = "Revit Pin 16 mm, L=9cm";
                var pinCost = costFunc(pinName);
                var pinTotalCost = totalCount * pinCost;

                var pinElementCost = new ElementQuantificationCost(pinName, totalCount, pinTotalCost, pinCost, UnitCostMeasure.NUMBER, CostType.RENT);
                elementsCost.Add(pinElementCost);

                var clipName = "Spring Clip";
                var clipCost = costFunc(clipName);
                var clipTotalCost = totalCount * clipCost;

                var clipElementCost = new ElementQuantificationCost(clipName, totalCount, clipTotalCost, clipCost, UnitCostMeasure.NUMBER, CostType.RENT);
                elementsCost.Add(clipElementCost);
            }


            return elementsCost;
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitShoreBracing> bracings,
                                                                   Func<string, double> costFunc)
        {
            Func<IGrouping<int, RevitShoreBracing>, ElementQuantificationCost> toCost = kvp =>
              {
                  var width = (kvp.Key / 100.0).Round(2).ToString("0.00");
                  var name = $"Cross Brace {width} m";
                  var cost = costFunc(name);
                  var count = kvp.Sum(br => br.NoOfMains);
                  var totalCost = cost * count;

                  return new ElementQuantificationCost(name, count, totalCost, cost, UnitCostMeasure.NUMBER, CostType.RENT);
              };

            return bracings.ToLookup(bracing => (int)(bracing.Width.FeetToMeter().Round(2) * 100))
                           .Select(toCost)
                           .ToList();
        }

        #endregion

        #region Beams

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitBeam> beams,
                                                       Func<string, double> costFunc)
        {
            var beamSection = beams.First().Section.SectionName;
            var beamCostType = CostType.RENT;
            if(beamSection == RevitBeamSectionName.TIMBER_2X4 ||
               beamSection == RevitBeamSectionName.TIMBER_2X5 || 
               beamSection == RevitBeamSectionName.TIMBER_2X6 ||
               beamSection == RevitBeamSectionName.TIMBER_2X8||
               beamSection == RevitBeamSectionName.TIMBER_3X3 || 
               beamSection == RevitBeamSectionName.TIMBER_3X5 ||
               beamSection == RevitBeamSectionName.TIMBER_3X6 ||
               beamSection == RevitBeamSectionName.TIMBER_4X4 ||
               beamSection == RevitBeamSectionName.DOUBLE_TIMBER_2X5||
               beamSection == RevitBeamSectionName.DOUBLE_TIMBER_2X6||
               beamSection == RevitBeamSectionName.DOUBLE_TIMBER_2X8 ||
               beamSection == RevitBeamSectionName.DOUBLE_TIMBER_3X6)
            {
                beamCostType = CostType.PURCHASE;
            }
            var beamName = beamSection.GetDescription();
            var beamUnitCost = costFunc(beamName);
            var count = beams.Count();
            var totalCost = beams.Sum(b => b.Length.FeetToMeter() * beamUnitCost);
            return beams.GroupBy(b => (int)b.Length.FeetToCm().Round(0))
                         .Select(kvp => new ElementQuantificationCost($"{beamName} L={kvp.Key} cm", kvp.Count(), kvp.Sum(b => b.Length.FeetToMeter() * beamUnitCost), beamUnitCost, UnitCostMeasure.LENGTH, beamCostType))
                         .ToList();
        }

        #endregion



    }
}
