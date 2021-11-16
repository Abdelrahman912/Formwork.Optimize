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

        private static readonly List<FormworkCostElements> _costEles =
            Enum.GetValues(typeof(FormworkCostElements))
               .Cast<FormworkCostElements>()
               .ToList();



        public static PlywoodCost AsPlywoodCost(this RevitFloorPlywood floorPlywood, double floorArea, Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            var name = floorPlywood.SectionName.AsElementCost();
            var plywoodElement = costFunc(name);
            var optimizationCostPerArea = plywoodElement.GetDailyPrice();

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
            return new PlywoodCost(optimizationCostPerArea, plywoodElement.Price, totalArea, loadAndDraw);
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

        //public static FormworkElementsCost AsFormworkElementsCost(this double dailyCost, FormworkTimeLine timeLine)
        //{
        //    //TODO:Wrong => Cost (Rent + purchase (dont multiply by duration))
        //    return new FormworkElementsCost(
        //        cost: dailyCost,
        //        duration: timeLine.TotalDuration
        //        );
        //}

        public static FormworkElementsCost TotalCost(this IEnumerable<ElementQuantificationCost> elements, FormworkTimeLine timeLine)
        {
            var lookup = elements.ToLookup(ele => ele.CostType);
            var rentElements = lookup[CostType.RENT].ToList();
            var purchaseElements = lookup[CostType.PURCHASE].ToList();
            var purchaseCost = purchaseElements.Aggregate(0.0, (soFar, current) => soFar += current.TotalCost);
            var rentCost = rentElements.Aggregate(0.0,(soFar,current)=>soFar+=current.TotalCost);

            return new FormworkElementsCost(rentCost, timeLine.TotalDuration, purchaseCost);
        }

        #region Cuplock

        public static List<ElementQuantificationCost> ToCost(this RevitCuplock cuplock,
                                                                  Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            return cuplock.Ledgers.ToCost(costFunc)
                                  .Concat(cuplock.Verticals.ToCost(costFunc))
                                  .Concat(cuplock.Bracings.ToCost(costFunc))
                                  .Concat(cuplock.MainBeams.ToCost(costFunc))
                                  .Concat(cuplock.SecondaryBeams.ToCost(costFunc))
                                  .ToList();
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitLedger> ledgers,
                                                                   Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            Func<IGrouping<int, RevitLedger>, ElementQuantificationCost> toCost = kvp =>
              {
                  var firstLedger = kvp.First();
                  var name = firstLedger.AsElementCost();
                  var count = kvp.Sum(ledger => ledger.Count);
                  var eleCost = costFunc(name);
                  var unitCost = eleCost.GetDailyPrice();
                  var totalCost = unitCost * count;
                  return new ElementQuantificationCost(name.GetDescription(), count, totalCost, unitCost, eleCost.UnitCost, eleCost.GetCostType());
              };

            return ledgers.ToLookup(ledger => (int)(ledger.Length.FeetToMeter().Round(2) * 100))
                          .Select(toCost)
                          .ToList();
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitCuplockVertical> verticals,
                                                         Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            var steelType = verticals.First().SteelType;
            Func<IGrouping<int, double>, ElementQuantificationCost> toCost = kvp =>
              {
                  var length = (kvp.Key / 100.0).Round(2).ToString("0.00");
                  var formattedString = $"CupLock Vertical {length} m, ({steelType.GetDescription()})";
                  var name = _costEles.First(en => en.GetDescription() == formattedString);
                  var eleCost = costFunc(name);
                  var unitCost = eleCost.GetDailyPrice();
                  var count = kvp.Count();
                  var totalCost = unitCost * count;
                  return new ElementQuantificationCost(name.GetDescription(), count, totalCost, unitCost, eleCost.UnitCost, eleCost.GetCostType());
              };

            var uHeadName = FormworkCostElements.U_HEAD_JACK_SOLID;
            var uHeadCount = verticals.Count();
            var uHeadEleCost = costFunc(uHeadName);
            var uHeadUnitCost = uHeadEleCost.GetDailyPrice();
            var uTotalCost = uHeadUnitCost * uHeadCount;

            var uHeadElementCost = new ElementQuantificationCost(uHeadName.GetDescription(), uHeadCount, uTotalCost, uHeadUnitCost, uHeadEleCost.UnitCost, uHeadEleCost.GetCostType());

            var postHeadName = FormworkCostElements.POST_HEAD_JACK_SOLID;
            var postCount = verticals.Count();
            var postUnitEleCost = costFunc(postHeadName);
            var postUnitCost = postUnitEleCost.GetDailyPrice();
            var postTotalCost = postUnitCost * postCount;

            var postElementCost = new ElementQuantificationCost(postHeadName.GetDescription(), postCount, postTotalCost, postUnitCost, postUnitEleCost.UnitCost, postUnitEleCost.GetCostType());

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

                var spigotName = FormworkCostElements.ROUND_SPIGOT;
                var spigotEleCost = costFunc(spigotName);
                var spigotCost = spigotEleCost.GetDailyPrice();
                var spigotTotalCost = spigotCost * roundSpigotCount;
                var spigotElementCost = new ElementQuantificationCost(spigotName.GetDescription(), roundSpigotCount, spigotTotalCost, spigotCost, spigotEleCost.UnitCost, spigotEleCost.GetCostType());

                var pinName = FormworkCostElements.RIVET_PIN_16MM_L9CM;
                var pinEleCost = costFunc(pinName);
                var pinCost = pinEleCost.GetDailyPrice();
                var pinTotalCost = pinCost * rivetPinAndSpringClipCount;
                var pinElementCost = new ElementQuantificationCost(pinName.GetDescription(), rivetPinAndSpringClipCount, pinTotalCost, pinCost, pinEleCost.UnitCost, pinEleCost.GetCostType());

                var clipName = FormworkCostElements.SPRING_CLIP;
                var clipEleCost = costFunc(clipName);
                var clipCost = clipEleCost.GetDailyPrice();
                var clipTotalCost = clipCost * rivetPinAndSpringClipCount;
                var clipElementCost = new ElementQuantificationCost(clipName.GetDescription(), rivetPinAndSpringClipCount, clipTotalCost, clipCost, clipEleCost.UnitCost, clipEleCost.GetCostType());

                elementsCost.Add(spigotElementCost);
                elementsCost.Add(pinElementCost);
                elementsCost.Add(clipElementCost);

            }

            elementsCost.Add(uHeadElementCost);
            elementsCost.Add(postElementCost);
            return elementsCost;
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitCuplockBracing> bracings,
                                                        Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            Func<IGrouping<int, double>, ElementQuantificationCost> toCost = kvp =>
              {
                  var length = (kvp.Key / 100.0).Round(2).ToString("0.00");
                  var formattedString = $"Scaffolding Tube {length} m";
                  var count = kvp.Count();
                  var name = _costEles.First(en => en.GetDescription() == formattedString);
                  var unitEleCost = costFunc(name);
                  var unitCost = unitEleCost.GetDailyPrice();
                  var totalCost = unitCost * count;
                  return new ElementQuantificationCost(name.GetDescription(), count, totalCost, unitCost, unitEleCost.UnitCost, unitEleCost.GetCostType());
              };
            var elementsCost = bracings.SelectMany(bracing => bracing.Lengths)
                                       .ToLookup(num => (int)(num.FeetToMeter().Round(2) * 100))
                                       .Select(toCost)
                                       .ToList();
            var couplerName = FormworkCostElements.PRESSED_PROP_SWIVEL_COUPLER;
            var couplerEleCost = costFunc(couplerName);
            var couplerCost = couplerEleCost.GetDailyPrice();
            var couplerCount = bracings.Sum(bracing => bracing.WidthHeightOffsets.Count) * 2;
            var couplerTotalCost = couplerCost * couplerCount;

            var couplerElementCost = new ElementQuantificationCost(couplerName.GetDescription(), couplerCount, couplerTotalCost, couplerCost, couplerEleCost.UnitCost, couplerEleCost.GetCostType());
            elementsCost.Add(couplerElementCost);
            return elementsCost;
        }


        private static FormworkCostElements AsElementCost(this RevitLedger ledger)
        {
            var length = ledger.Length.FeetToMeter().Round(2).ToString("0.00");
            var formattedString = $"CupLock Ledger {length} m, ({ledger.SteelType.GetDescription()})";
            return _costEles
                   .First(en => en.GetDescription() == formattedString);
        }

        #endregion

        #region Props

        public static List<ElementQuantificationCost> ToCost(this RevitProps props,
                                                                  Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            return props.Verticals.ToCost(costFunc)
                                  .Concat(props.Legs.ToCost(costFunc))
                                  .Concat(props.SecondaryBeams.ToCost(costFunc))
                                  .Concat(props.MainBeams.ToCost(costFunc))
                                  .ToList();
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitPropsVertical> propsVerticals,
                                                                 Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            var elementsCost = new List<ElementQuantificationCost>();

            var propName = propsVerticals.First().PropType.AsElementCost();
            var eleCost = costFunc(propName);
            var cost = eleCost.GetDailyPrice();
            var count = propsVerticals.Count();
            var totalCost = count * cost;

            var propElementCost = new ElementQuantificationCost(propName.GetDescription(), count, totalCost, cost, eleCost.UnitCost, eleCost.GetCostType());
            elementsCost.Add(propElementCost);

            var uName = FormworkCostElements.U_HEAD_FOR_PROPS;
            var uHeadEleCost = costFunc(uName); ;
            var uCost = uHeadEleCost.GetDailyPrice();
            var uCount = propsVerticals.Count();
            var uTotalCost = uCount * cost;

            var uElementCost = new ElementQuantificationCost(uName.GetDescription(), uCount, uTotalCost, uCost, uHeadEleCost.UnitCost, uHeadEleCost.GetCostType());
            elementsCost.Add(uElementCost);

            return elementsCost;
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitPropsLeg> propsLegs,
                                                                 Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            var name = FormworkCostElements.PROP_LEG;
            var eleCost = costFunc(name);
            var cost = eleCost.GetDailyPrice();
            var count = propsLegs.Count();
            var totalCost = count * cost;
            return new List<ElementQuantificationCost>()
            {
                new ElementQuantificationCost(name.GetDescription(),count,totalCost,cost,eleCost.UnitCost, eleCost.GetCostType())
            };
        }

        #endregion

        #region ShoreBrace

        public static List<ElementQuantificationCost> ToCost(this RevitShore shore,
                                                                  Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            return shore.Mains.ToCost(costFunc)
                              .Concat(shore.Bracings.ToCost(costFunc))
                              .Concat(shore.MainBeams.ToCost(costFunc))
                              .Concat(shore.SecondaryBeams.ToCost(costFunc))
                              .ToList();
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitShoreMain> shoreMains,
                                                              Func<FormworkCostElements, FormworkElementCost> costFunc)
        {

            //Telescopic, Mains, U-Head, Post Head.
            var elementsCost = new List<ElementQuantificationCost>();

            var mainFrameName = FormworkCostElements.SHOREBRACE_FRAME;
            var mainFrameEleCost = costFunc(mainFrameName);
            var mainFrameCost = mainFrameEleCost.GetDailyPrice();
            var mainFrameCount = shoreMains.Sum(main => main.NoOfMains);
            var mainFrameTotalCost = mainFrameCost * mainFrameCount;

            var mainFrameElementCost = new ElementQuantificationCost(mainFrameName.GetDescription(), mainFrameCount, mainFrameTotalCost, mainFrameCost, mainFrameEleCost.UnitCost, mainFrameEleCost.GetCostType());
            elementsCost.Add(mainFrameElementCost);

            var teleName = FormworkCostElements.SHOREBRACE_FRAME;
            var teleEleCost = costFunc(teleName);
            var teleCost = teleEleCost.GetDailyPrice();
            var teleCount = shoreMains.Count();
            var teleTotalCost = teleCost * teleCount;

            var teleElementCost = new ElementQuantificationCost(teleName.GetDescription(), teleCount, teleTotalCost, teleCost, teleEleCost.UnitCost, teleEleCost.GetCostType());
            elementsCost.Add(teleElementCost);

            var uName = FormworkCostElements.U_HEAD_JACK_SOLID;
            var uEleCost = costFunc(uName);
            var uCost = uEleCost.GetDailyPrice();
            var uCount = shoreMains.Count();
            var uTotalCost = uCost * uCount;

            var uElementCost = new ElementQuantificationCost(uName.GetDescription(), uCount, uTotalCost, uCost, uEleCost.UnitCost, uEleCost.GetCostType());
            elementsCost.Add(uElementCost);

            var postName = FormworkCostElements.POST_HEAD_JACK_SOLID;
            var postEleCost = costFunc(postName);
            var postCost = postEleCost.GetDailyPrice();
            var postCount = shoreMains.Count();
            var postTotalCost = postCost * postCount;

            var postElementCost = new ElementQuantificationCost(postName.GetDescription(), postCount, postTotalCost, postCost, postEleCost.UnitCost, postEleCost.GetCostType());
            elementsCost.Add(postElementCost);

            var framesPoints = shoreMains.Select(main => main.Position)
                                         .GroupXYZ()
                                         .Where(tuple => tuple.Item2 > 1)
                                         .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2 - 1))
                                         .ToList();
            if (framesPoints.Count > 0)
            {
                var totalCount = framesPoints.Sum(tuple => tuple.Item2 * 1);

                var pinName = FormworkCostElements.RIVET_PIN_16MM_L9CM;
                var pinEleCost = costFunc(pinName);
                var pinCost = pinEleCost.GetDailyPrice();
                var pinTotalCost = totalCount * pinCost;

                var pinElementCost = new ElementQuantificationCost(pinName.GetDescription(), totalCount, pinTotalCost, pinCost, pinEleCost.UnitCost, pinEleCost.GetCostType());
                elementsCost.Add(pinElementCost);

                var clipName = FormworkCostElements.SPRING_CLIP;
                var clipEleCost = costFunc(clipName);
                var clipCost = clipEleCost.GetDailyPrice();
                var clipTotalCost = totalCount * clipCost;

                var clipElementCost = new ElementQuantificationCost(clipName.GetDescription(), totalCount, clipTotalCost, clipCost, clipEleCost.UnitCost, clipEleCost.GetCostType());
                elementsCost.Add(clipElementCost);
            }


            return elementsCost;
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitShoreBracing> bracings,
                                                                   Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            Func<IGrouping<int, RevitShoreBracing>, ElementQuantificationCost> toCost = kvp =>
              {
                  var width = (kvp.Key / 100.0).Round(2).ToString("0.00");
                  var formattedString = $"Cross Brace {width} m";
                  var name = _costEles.First(en => en.GetDescription() == formattedString);
                  var eleCost = costFunc(name);
                  var cost = eleCost.GetDailyPrice();
                  var count = kvp.Sum(br => br.NoOfMains);
                  var totalCost = cost * count;

                  return new ElementQuantificationCost(name.GetDescription(), count, totalCost, cost, eleCost.UnitCost, eleCost.GetCostType());
              };

            return bracings.ToLookup(bracing => (int)(bracing.Width.FeetToMeter().Round(2) * 100))
                           .Select(toCost)
                           .ToList();
        }

        #endregion

        #region Beams

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitBeam> beams,
                                                       Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            var beamSection = beams.First().Section.SectionName;

            var beamName = beamSection.AsElementCost();
            var beamEleCost = costFunc(beamName); ;
            var beamUnitCost = beamEleCost.GetDailyPrice();
            var count = beams.Count();
            var totalCost = beams.Sum(b => b.Length.FeetToMeter() * beamUnitCost);
            return beams.GroupBy(b => (int)b.Length.FeetToCm().Round(0))
                         .Select(kvp => new ElementQuantificationCost($"{beamName} L={kvp.Key} cm", kvp.Count(), kvp.Sum(b => b.Length.FeetToMeter() * beamUnitCost), beamUnitCost, beamEleCost.UnitCost, beamEleCost.GetCostType()))
                         .ToList();
        }

        #endregion



    }
}
