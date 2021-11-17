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



        public static PlywoodCost AsPlywoodCost(this RevitFloorPlywood floorPlywood, double floorArea, Func<FormworkCostElements, FormworkElementCost> costFunc,FormworkTimeLine timeLine)
        {
            var name = floorPlywood.SectionName.AsElementCost();
            var plywoodElement = costFunc(name);
            var optimizationCostPerArea = plywoodElement.GetOptimizationCost();
            var initalCostPerArea = plywoodElement.GetInitialCost();

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
           
            return new PlywoodCost(optimizationCostPerArea, initalCostPerArea, totalArea,plywoodElement.GetCostType(), loadAndDraw,timeLine.TotalDuration);
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

     

        public static FormworkElementsCost AsFormworkElementsCost(this IEnumerable<ElementQuantificationCost> elements, FormworkTimeLine timeLine)
        {
            var lookup = elements.ToLookup(ele => ele.CostType);
            var rentElements = lookup[CostType.RENT].ToList();
            var purchaseElements = lookup[CostType.PURCHASE].ToList();
            var purchaseCost = purchaseElements.Aggregate(Tuple.Create(0.0,0.0), (soFar, current) => Tuple.Create(soFar.Item1+current.OptimizeTotalCost, soFar.Item2 + current.InitialTotalCost));
            var rentCost = rentElements.Aggregate(0.0,(soFar,current)=>soFar+=current.InitialTotalCost);

            return new FormworkElementsCost(rentCost, timeLine.TotalDuration, purchaseCost.Item1,purchaseCost.Item2);
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
                  var optimizeUnitCost = eleCost.GetOptimizationCost();
                  var optimizeTotalCost = optimizeUnitCost * count;
                  var initialUnitCost = eleCost.GetInitialCost();
                  var initialTotalCost = initialUnitCost * count;
                  return new ElementQuantificationCost(name.GetDescription(), count, optimizeTotalCost, optimizeUnitCost, initialTotalCost, initialUnitCost, eleCost.UnitCost, eleCost.GetCostType());
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
                  var count = kvp.Count();
                  var optimizeUnitCost = eleCost.GetOptimizationCost();
                  var optimizeTotalCost = optimizeUnitCost * count;
                  var initialUnitCost = eleCost.GetInitialCost();
                  var initialTotalCost = initialUnitCost * count;
                  return new ElementQuantificationCost(name.GetDescription(), count, optimizeTotalCost, optimizeUnitCost,initialTotalCost,initialUnitCost, eleCost.UnitCost, eleCost.GetCostType());
              };

            var uHeadName = FormworkCostElements.U_HEAD_JACK_SOLID;
            var uHeadCount = verticals.Count();
            var uHeadEleCost = costFunc(uHeadName);
            var uHeadOptimizationUnitCost = uHeadEleCost.GetOptimizationCost();
            var uOptimizationTotalCost = uHeadOptimizationUnitCost * uHeadCount;
            var uHeadInitialUnitCost = uHeadEleCost.GetInitialCost();
            var uInitialTotalCost = uHeadInitialUnitCost * uHeadCount;

            var uHeadElementCost = new ElementQuantificationCost(uHeadName.GetDescription(), uHeadCount, uOptimizationTotalCost, uHeadOptimizationUnitCost, uInitialTotalCost, uHeadInitialUnitCost, uHeadEleCost.UnitCost, uHeadEleCost.GetCostType());

            var postHeadName = FormworkCostElements.POST_HEAD_JACK_SOLID;
            var postCount = verticals.Count();
            var postUnitEleCost = costFunc(postHeadName);
            var postOptimizeUnitCost = postUnitEleCost.GetOptimizationCost();
            var postOptimizeTotalCost = postOptimizeUnitCost * postCount;
            var postInitialUnitCost = postUnitEleCost.GetInitialCost();
            var postInitialTotalCost = postInitialUnitCost * postCount;


            var postElementCost = new ElementQuantificationCost(postHeadName.GetDescription(), postCount, postOptimizeTotalCost, postOptimizeUnitCost, postInitialTotalCost, postInitialUnitCost, postUnitEleCost.UnitCost, postUnitEleCost.GetCostType());

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

                var spigotOptimizeCost = spigotEleCost.GetOptimizationCost();
                var spigotOptimizeTotalCost = spigotOptimizeCost * roundSpigotCount;

                var spigotICost = spigotEleCost.GetInitialCost();
                var spigotITotalCost = spigotICost * roundSpigotCount;


                var spigotElementCost = new ElementQuantificationCost(spigotName.GetDescription(), roundSpigotCount, spigotOptimizeTotalCost, spigotOptimizeCost, spigotITotalCost, spigotICost, spigotEleCost.UnitCost, spigotEleCost.GetCostType());

                var pinName = FormworkCostElements.RIVET_PIN_16MM_L9CM;
                var pinEleCost = costFunc(pinName);

                var pinOCost = pinEleCost.GetOptimizationCost();
                var pinOTotalCost = pinOCost * rivetPinAndSpringClipCount;

                var pinICost = pinEleCost.GetInitialCost();
                var pinITotalCost = pinICost * rivetPinAndSpringClipCount;

                var pinElementCost = new ElementQuantificationCost(pinName.GetDescription(), rivetPinAndSpringClipCount, pinOTotalCost, pinOCost, pinITotalCost, pinICost, pinEleCost.UnitCost, pinEleCost.GetCostType());

                var clipName = FormworkCostElements.SPRING_CLIP;

                var clipEleCost = costFunc(clipName);

                var clipOCost = clipEleCost.GetOptimizationCost();
                var clipOTotalCost = clipOCost * rivetPinAndSpringClipCount;

                var clipICost = clipEleCost.GetInitialCost();
                var clipITotalCost = clipICost * rivetPinAndSpringClipCount;

                var clipElementCost = new ElementQuantificationCost(clipName.GetDescription(), rivetPinAndSpringClipCount, clipOTotalCost, clipOCost, clipITotalCost, clipICost, clipEleCost.UnitCost, clipEleCost.GetCostType());

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

                  var unitOCost = unitEleCost.GetOptimizationCost();
                  var totalOCost = unitOCost * count;

                  var unitICost = unitEleCost.GetInitialCost();
                  var totalICost = unitICost * count;

                  return new ElementQuantificationCost(name.GetDescription(), count, totalOCost, unitOCost, totalICost, unitICost, unitEleCost.UnitCost, unitEleCost.GetCostType());
              };
            var elementsCost = bracings.SelectMany(bracing => bracing.Lengths)
                                       .ToLookup(num => (int)(num.FeetToMeter().Round(2) * 100))
                                       .Select(toCost)
                                       .ToList();
            var couplerName = FormworkCostElements.PRESSED_PROP_SWIVEL_COUPLER;
            var couplerEleCost = costFunc(couplerName);
            var couplerCount = bracings.Sum(bracing => bracing.WidthHeightOffsets.Count) * 2;

            var couplerOCost = couplerEleCost.GetOptimizationCost();
            var couplerOTotalCost = couplerOCost * couplerCount;

            var couplerICost = couplerEleCost.GetInitialCost();
            var couplerITotalCost = couplerICost * couplerCount;

            var couplerElementCost = new ElementQuantificationCost(couplerName.GetDescription(), couplerCount, couplerITotalCost, couplerICost, couplerITotalCost, couplerICost, couplerEleCost.UnitCost, couplerEleCost.GetCostType());
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

            var count = propsVerticals.Count();

            var costO = eleCost.GetOptimizationCost();
            var totalOCost = count * costO;

            var costI = eleCost.GetInitialCost();
            var totalICost = count * costI;

            var propElementCost = new ElementQuantificationCost(propName.GetDescription(), count, totalOCost, costO, totalICost, costI, eleCost.UnitCost, eleCost.GetCostType());
            elementsCost.Add(propElementCost);

            var uName = FormworkCostElements.U_HEAD_FOR_PROPS;
            var uHeadEleCost = costFunc(uName); ;
            var uCount = propsVerticals.Count();

            var uOCost = uHeadEleCost.GetOptimizationCost();
            var uOTotalCost = uCount * uOCost;

            var uICost = uHeadEleCost.GetInitialCost();
            var uITotalCost = uCount * uICost;

            var uElementCost = new ElementQuantificationCost(uName.GetDescription(), uCount, uOTotalCost, uOCost, uITotalCost, uICost, uHeadEleCost.UnitCost, uHeadEleCost.GetCostType());
            elementsCost.Add(uElementCost);

            return elementsCost;
        }

        private static List<ElementQuantificationCost> ToCost(this IEnumerable<RevitPropsLeg> propsLegs,
                                                                 Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            var name = FormworkCostElements.PROP_LEG;
            var eleCost = costFunc(name);
            var count = propsLegs.Count();

            var costO = eleCost.GetOptimizationCost();
            var totalOCost = count * costO;

            var costI = eleCost.GetInitialCost();
            var totalICost = count * costI;

            return new List<ElementQuantificationCost>()
            {
                new ElementQuantificationCost(name.GetDescription(),count,totalOCost,costO,totalICost,costI,eleCost.UnitCost, eleCost.GetCostType())
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
            var mainFrameCount = shoreMains.Sum(main => main.NoOfMains);

            var mainFrameOCost = mainFrameEleCost.GetOptimizationCost();
            var mainFrameOTotalCost = mainFrameOCost * mainFrameCount;

            var mainFrameICost = mainFrameEleCost.GetInitialCost();
            var mainFrameITotalCost = mainFrameICost * mainFrameCount;

            var mainFrameElementCost = new ElementQuantificationCost(mainFrameName.GetDescription(), mainFrameCount, mainFrameOTotalCost, mainFrameOCost, mainFrameITotalCost, mainFrameICost, mainFrameEleCost.UnitCost, mainFrameEleCost.GetCostType());
            elementsCost.Add(mainFrameElementCost);

            var teleName = FormworkCostElements.SHOREBRACE_FRAME;
            var teleEleCost = costFunc(teleName);
            var teleCount = shoreMains.Count();

            var teleOCost = teleEleCost.GetOptimizationCost();
            var teleOTotalCost = teleOCost * teleCount;

            var teleICost = teleEleCost.GetInitialCost();
            var teleITotalCost = teleICost * teleCount;

            var teleElementCost = new ElementQuantificationCost(teleName.GetDescription(), teleCount, teleOTotalCost, teleOCost, teleITotalCost, teleICost, teleEleCost.UnitCost, teleEleCost.GetCostType());
            elementsCost.Add(teleElementCost);

            var uName = FormworkCostElements.U_HEAD_JACK_SOLID;
            var uEleCost = costFunc(uName);
            var uCount = shoreMains.Count();

            var uOCost = uEleCost.GetOptimizationCost();
            var uOTotalCost = uOCost * uCount;

            var uICost = uEleCost.GetInitialCost();
            var uITotalCost = uICost * uCount;

            var uElementCost = new ElementQuantificationCost(uName.GetDescription(), uCount, uOTotalCost, uOCost, uITotalCost, uICost, uEleCost.UnitCost, uEleCost.GetCostType());
            elementsCost.Add(uElementCost);

            var postName = FormworkCostElements.POST_HEAD_JACK_SOLID;
            var postEleCost = costFunc(postName);
            var postCount = shoreMains.Count();

            var postOCost = postEleCost.GetOptimizationCost();
            var postOTotalCost = postOCost * postCount;

            var postICost = postEleCost.GetInitialCost();
            var postITotalCost = postICost * postCount;

            var postElementCost = new ElementQuantificationCost(postName.GetDescription(), postCount, postOTotalCost, postOCost, postITotalCost, postICost, postEleCost.UnitCost, postEleCost.GetCostType());
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

                var pinOCost = pinEleCost.GetOptimizationCost();
                var pinOTotalCost = totalCount * pinOCost;

                var pinICost = pinEleCost.GetInitialCost();
                var pinITotalCost = totalCount * pinICost;

                var pinElementCost = new ElementQuantificationCost(pinName.GetDescription(), totalCount, pinOTotalCost, pinOCost, pinITotalCost, pinICost, pinEleCost.UnitCost, pinEleCost.GetCostType());
                elementsCost.Add(pinElementCost);

                var clipName = FormworkCostElements.SPRING_CLIP;
                var clipEleCost = costFunc(clipName);

                var clipOCost = clipEleCost.GetOptimizationCost();
                var clipOTotalCost = totalCount * clipOCost;

                var clipICost = clipEleCost.GetInitialCost();
                var clipITotalCost = totalCount * clipICost;

                var clipElementCost = new ElementQuantificationCost(clipName.GetDescription(), totalCount, clipOTotalCost, clipOCost, clipITotalCost, clipICost, clipEleCost.UnitCost, clipEleCost.GetCostType());
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
                  var count = kvp.Sum(br => br.NoOfMains);

                  var costO = eleCost.GetOptimizationCost();
                  var totalOCost = costO * count;

                  var costI = eleCost.GetInitialCost();
                  var totalICost = costI * count;


                  return new ElementQuantificationCost(name.GetDescription(), count, totalOCost, costO, totalICost, costI, eleCost.UnitCost, eleCost.GetCostType());
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
            var beamEleCost = costFunc(beamName); 
            var count = beams.Count();

            var beamOUnitCost = beamEleCost.GetOptimizationCost();
           // var totalOCost = beams.Sum(b => b.Length.FeetToMeter() * beamOUnitCost);

            var beamIUnitCost = beamEleCost.GetInitialCost();
            //var totalICost = beams.Sum(b => b.Length.FeetToMeter() * beamIUnitCost);

            return beams.GroupBy(b => (int)b.Length.FeetToCm().Round(0))
                         .Select(kvp => new ElementQuantificationCost($"{beamName} L={kvp.Key} cm", kvp.Count(), kvp.Sum(b => b.Length.FeetToMeter() * beamOUnitCost), beamOUnitCost, kvp.Sum(b => b.Length.FeetToMeter() * beamIUnitCost), beamIUnitCost, beamEleCost.UnitCost, beamEleCost.GetCostType()))
                         .ToList();
        }

        #endregion



    }
}
