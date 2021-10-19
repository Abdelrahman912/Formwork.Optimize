using Autodesk.Revit.DB;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS;
using FormworkOptimize.Core.DTOS.Genetic;
using FormworkOptimize.Core.DTOS.Revit.Input;
using FormworkOptimize.Core.DTOS.Revit.Input.Props;
using FormworkOptimize.Core.DTOS.Revit.Input.Shore;
using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.Designer;
using FormworkOptimize.Core.Entities.Genetic;
using FormworkOptimize.Core.Entities.GeneticParameters;
using FormworkOptimize.Core.Entities.GeneticResult;
using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Helpers.CostHelper;
using FormworkOptimize.Core.Helpers.DesignHelper;
using FormworkOptimize.Core.Helpers.RevitHelper;
using GeneticSharp.Domain.Chromosomes;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.Core.Extensions
{
    public static class ChromosomeExtension
    {
        #region General
        public static bool IsValid(this BinaryChromosomeBase designChromosome)
        {
            return (designChromosome.Fitness != null && designChromosome.Fitness.Value != -1.0 && designChromosome.Fitness.Value != -2.0);
        }
        #endregion

        #region Cuplock Chromosome

        public static CuplockDesignInput AsDesignInput(this CuplockChromosome chromosome, double slabThicknessCm, double beamThicknessCm, double beamWidthCm, CuplockGeneticIncludedElements includedElements)
        {
            // Genes
            var values = chromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var steelTypeVal = values[3];
            var mainLedgerIndex = (int)values[4];
            var secLedgersIndex = (int)values[5];
            var secBeamLengthIndex = (int)values[6];
            var mainBeamLengthIndex = (int)values[7];
            var secSpacinIndex = (int)values[8];

            var secSpacingVal = includedElements.IncludedLedgers[secLedgersIndex];
            var secLengths = Database.GetBeamLengths((BeamSectionName)(int)secondaryBeamSectionVal).ToList();
            if (secLengths.Count - 1 < secBeamLengthIndex)
                return null;
            var secTotalLength = secLengths[secBeamLengthIndex];

            var mainSpacingVal = includedElements.IncludedLedgers[mainLedgerIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).ToList();
            if (mainLengths.Count - 1 < mainBeamLengthIndex)
                return null;
            var mainTotalLength = mainLengths[mainBeamLengthIndex];
            return new CuplockDesignInput(
              includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
              includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
              includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
              includedElements.IncludedSteelTypes[((int)steelTypeVal)],
              mainSpacingVal,
              secSpacingVal,
              secTotalLength,
              mainTotalLength,
              slabThicknessCm,
              new UserDefinedSecondaryBeamSpacing(AvailableSecBeamSpacings[secSpacinIndex]),
              beamThicknessCm,
              beamWidthCm);


        }

        public static double EvaluateFitnessCuplockDesign(this CuplockChromosome designChromosome,
                                                               double slabThicknessCm,
                                                               double beamThicknessCm,
                                                               double beamWidthCm,
                                                              CuplockGeneticIncludedElements includedElements)
        {

            // Design Input
            var input = designChromosome.AsDesignInput(slabThicknessCm, beamThicknessCm, beamWidthCm, includedElements);
            if (input == null)
                return -1;
            designChromosome.CuplockDesignInput = input;
            var designer = CuplockDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.CuplockDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<CuplockDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item3.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item3.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;

                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()},Selected Span: {designOutput.Plywood.Item2.Span} cm, Max Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item3.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.SecondaryBeamTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.MainBeamTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Cuplock System, M.B. Direction: {input.LedgersMainDir} cm, S.B. Direction: {input.LedgersSecondaryDir} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);

                    designChromosome.SecondaryBeamSpacing = designOutput.Plywood.Item1.Span;

                    return (ratio1 + ratio2 + ratio3 + ratio4);
                }

                return -1.0;
            };

            return designOutputValidation.Match(errs => -2.0, calculateFitness);

        }

        public static double EvaluateFitnessCuplockCost(this CuplockChromosome designChromosome,
                                                             double slabThicknessCm,
                                                             double beamThicknessCm,
                                                             double beamWidthCm,
                                                             CostGeneticResultInput costInput,
                                                             CuplockGeneticIncludedElements includedElements)
        {

            // Design Input
            var input = designChromosome.AsDesignInput(slabThicknessCm, beamThicknessCm, beamWidthCm, includedElements);
            if (input == null)
                return -1;
            designChromosome.CuplockDesignInput = input;

            var designer = CuplockDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.CuplockDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<CuplockDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item3.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()},Selected Span: {designOutput.Plywood.Item2.Span} cm, ,Max Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item3.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.SecondaryBeamTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.MainBeamTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Cuplock System, M.B. Direction: {input.LedgersMainDir} cm, S.B. Direction: {input.LedgersSecondaryDir} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);

                    designChromosome.SecondaryBeamSpacing = designOutput.Plywood.Item1.Span;

                    var newCostInput = costInput.UpdateCostInputWithNewRevitInput(costInput.RevitInput.UpdateWithNewXYZ(costInput.RevitInput.MainBeamDir.CrossProduct(XYZ.BasisZ)));
                    var costInputs = new List<CostGeneticResultInput>() { costInput, newCostInput };

                    var result = costInputs.Select(inp => designChromosome.AsFloorCuplockCost(inp))
                                           .Select(floorCost => Tuple.Create(floorCost, floorCost.EvaluateCost(costInput.CostFunc).TotalCost()))
                                           .OrderBy(tuple => tuple.Item2)
                                           .First();
                    var revitFloorPlywood = costInput.PlywoodFunc(designChromosome.CuplockDesignInput.PlywoodSection);
                    var plywoodCost = revitFloorPlywood.AsPlywoodCost(costInput.FloorArea, costInput.CostFunc);

                    designChromosome.FloorCuplockCost = result.Item1;
                    designChromosome.PlywoodCost = plywoodCost;
                    designChromosome.Cost = result.Item2;

                    var cost = designChromosome.PlywoodCost.TotalCost + designChromosome.Cost;
                    if (cost > 0.0)
                    {
                        var costInverse = 100000.0 / cost;

                        return costInverse;
                    }

                    return -1.0;
                }

                return -1.0;
            };

            return designOutputValidation.Match(errs => -2.0, calculateFitness);

        }

        public static FloorCuplockCost AsFloorCuplockCost(this CuplockChromosome chromosome, CostGeneticResultInput costInput)
        {
            var cuplockInput = new RevitFloorCuplockInput(chromosome.CuplockDesignInput.PlywoodSection,
                                                          chromosome.CuplockDesignInput.SecondaryBeamSection.ToRevitBeamSectionName(),
                                                          chromosome.CuplockDesignInput.MainBeamSection.ToRevitBeamSectionName(),
                                                          chromosome.CuplockDesignInput.SteelType,
                                                          chromosome.CuplockDesignInput.SecondaryBeamTotalLength.CmToFeet(),
                                                          chromosome.SecondaryBeamSpacing.CmToFeet(),
                                                          chromosome.CuplockDesignInput.MainBeamTotalLength.CmToFeet(),
                                                          chromosome.CuplockDesignInput.LedgersMainDir.CmToFeet(),
                                                          chromosome.CuplockDesignInput.LedgersSecondaryDir.CmToFeet(),
                                                          costInput.BoundaryLinesOffest.CmToFeet(),
                                                          costInput.BeamsOffset.CmToFeet());
            return new FloorCuplockCost(costInput.RevitInput, cuplockInput);
        }
        public static CostGeneticResult AsCostGeneticResult(this CuplockChromosome chromosome, CostGeneticResultInput costInput, int rank)
        {
            var formElesCost = chromosome.Cost.AsFormworkElemntsCost(costInput.TimeLine);
            chromosome.Cost += chromosome.PlywoodCost.TotalCost;
            var totalCost = formElesCost.TotalCost + costInput.ManPowerCost.TotalCost + costInput.EquipmentCost.TotalCost + costInput.TransportationCost.Cost + chromosome.PlywoodCost.TotalCost;
            var costDetailResult = new GeneticCostDetailResult(costInput.TimeLine, costInput.ManPowerCost, costInput.EquipmentCost, formElesCost, chromosome.PlywoodCost, costInput.TransportationCost);
            var detailResults = new List<IGeneticDetailResult>() { chromosome.DetailResult, costDetailResult };
            return new CostGeneticResult(rank, chromosome.Fitness.Value, $"Option {rank}", detailResults, totalCost, chromosome.FloorCuplockCost, chromosome.PlywoodCost);
        }
        public static CostGeneticResult AsGeneticResult(this CuplockChromosome chromosome, CostGeneticResultInput costInput, int rank = 0)
        {
            chromosome.FloorCuplockCost = chromosome.AsFloorCuplockCost(costInput);
            chromosome.Cost = chromosome.FloorCuplockCost.EvaluateCost(costInput.CostFunc).TotalCost();
            var revitFloorPlywood = costInput.PlywoodFunc(chromosome.CuplockDesignInput.PlywoodSection);
            var plywoodCost = revitFloorPlywood.AsPlywoodCost(costInput.FloorArea, costInput.CostFunc);
            chromosome.PlywoodCost = plywoodCost;
            return chromosome.AsCostGeneticResult(costInput, rank);
        }

        #endregion

        #region European Props Chromosome


        public static EuropeanPropDesignInput AsDesignInput(this EuropeanPropChromosome chromosome, double slabThicknessCm, double beamThicknessCm, double beamWidthCm, EuropeanPropsGeneticInludedElements includedElements)
        {
            // Genes
            var values = chromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var propTypeVal = values[3];
            var secPropsSpacingIndex = (int)values[4];
            var mainPropsSpacingIndex = (int)values[5];
            var secBeamLengthIndex = (int)values[6];
            var mainBeamLengthIndex = (int)values[7];
            var secSpacingIndex =(int) values[8];

            var propsSpacingVals = new List<double>() { 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250, 260, 270, 280, 290, 300 };//25.

            var secSpacingVal = propsSpacingVals[secPropsSpacingIndex];
            var secLengths = Database.GetBeamLengths((BeamSectionName)(int)secondaryBeamSectionVal).ToList();
            if (secLengths.Count - 1 < secBeamLengthIndex)
                return null;
            var secTotalLength = secLengths[secBeamLengthIndex];

            var mainSpacingVal = propsSpacingVals[mainPropsSpacingIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).ToList();
            if (mainLengths.Count - 1 < mainBeamLengthIndex)
                return null;
            var mainTotalLength = mainLengths[mainBeamLengthIndex];

            return new EuropeanPropDesignInput(
                includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
                includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
                includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
                includedElements.IncludedProps[((int)propTypeVal)],
                 mainSpacingVal,
                 secSpacingVal,
                 secTotalLength,
                 mainTotalLength,
                 slabThicknessCm,
                 new UserDefinedSecondaryBeamSpacing(AvailableSecBeamSpacings[ secSpacingIndex]),
                 beamThicknessCm,
                 beamWidthCm);

        }

        public static double EvaluateFitnessEuropeanPropDesign(this EuropeanPropChromosome designChromosome,
                                                                    double slabThicknessCm,
                                                                    double beamThicknessCm,
                                                                    double beamWidthCm,
                                                                    EuropeanPropsGeneticInludedElements includedElements)
        {

            var input = designChromosome.AsDesignInput(slabThicknessCm, beamThicknessCm, beamWidthCm, includedElements);
            if (input == null)
                return -1;
            // Design Input
            designChromosome.EuropeanPropDesignInput = input;

            var designer = EuropeanPropDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.EuropeanPropDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<PropDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item3.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item3.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;

                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item3.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.SecondaryBeamTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.MainBeamTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"European Prop System, M.B. Direction: {input.MainSpacing} cm, S.B. Direction: {input.SecondarySpacing} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);
                    designChromosome.SecondaryBeamSpacing = designOutput.Plywood.Item1.Span;

                    return (ratio1 + ratio2 + ratio3 + ratio4);
                }

                return -1.0;
            };

            return designOutputValidation.Match(errs => -2.0, calculateFitness);

        }
        public static double EvaluateFitnessEuropeanPropCost(this EuropeanPropChromosome designChromosome,
                                                                  double slabThicknessCm,
                                                                  double beamThicknessCm,
                                                                  double beamWidthCm,
                                                                  CostGeneticResultInput costInput,
                                                                 EuropeanPropsGeneticInludedElements includedElements)
        {
            var input = designChromosome.AsDesignInput(slabThicknessCm, beamThicknessCm, beamWidthCm, includedElements);
            if (input == null)
                return -1;
            // Design Input
            designChromosome.EuropeanPropDesignInput = input;

            var designer = EuropeanPropDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.EuropeanPropDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<PropDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item3.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item3.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.SecondaryBeamTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.MainBeamTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"European Prop System, M.B. Direction: {input.MainSpacing} cm, S.B. Direction: {input.SecondarySpacing} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);
                    designChromosome.SecondaryBeamSpacing = designOutput.Plywood.Item1.Span;




                    var newCostInput = costInput.UpdateCostInputWithNewRevitInput(costInput.RevitInput.UpdateWithNewXYZ(costInput.RevitInput.MainBeamDir.CrossProduct(XYZ.BasisZ)));
                    var costInputs = new List<CostGeneticResultInput>() { costInput, newCostInput };

                    var result = costInputs.Select(inp => designChromosome.AsFloorEuropeanPropCost(inp))
                                           .Select(floorCost => Tuple.Create(floorCost, floorCost.EvaluateCost(costInput.CostFunc).TotalCost()))
                                           .OrderBy(tuple => tuple.Item2)
                                           .First();

                    var revitFloorPlywood = costInput.PlywoodFunc(designChromosome.EuropeanPropDesignInput.PlywoodSection);
                    var plywoodCost = revitFloorPlywood.AsPlywoodCost(costInput.FloorArea, costInput.CostFunc);

                    designChromosome.FloorPropsCost = result.Item1;
                    designChromosome.PlywoodCost = plywoodCost;
                    designChromosome.Cost = result.Item2;

                    var totalCost = designChromosome.PlywoodCost.TotalCost + designChromosome.Cost;

                    if (totalCost > 0.0)
                    {
                        var costInverse = 100000.0 / totalCost;

                        return costInverse;
                    }

                    return -1.0;
                }

                return -1.0;
            };

            return designOutputValidation.Match(errs => -2.0, calculateFitness);

        }
        public static FloorPropsCost AsFloorEuropeanPropCost(this EuropeanPropChromosome chromosome, CostGeneticResultInput costInput)
        {
            var europeanInput = new RevitFloorPropsInput(chromosome.EuropeanPropDesignInput.EuropeanPropType,
                                             chromosome.EuropeanPropDesignInput.PlywoodSection,
                                              chromosome.EuropeanPropDesignInput.SecondaryBeamSection.ToRevitBeamSectionName(),
                                              chromosome.EuropeanPropDesignInput.MainBeamSection.ToRevitBeamSectionName(),
                                              chromosome.EuropeanPropDesignInput.SecondaryBeamTotalLength.CmToFeet(),
                                              chromosome.SecondaryBeamSpacing.CmToFeet(),
                                              chromosome.EuropeanPropDesignInput.MainBeamTotalLength.CmToFeet(),
                                              chromosome.EuropeanPropDesignInput.MainSpacing.CmToFeet(),
                                              chromosome.EuropeanPropDesignInput.SecondarySpacing.CmToFeet(),
                                              costInput.BoundaryLinesOffest.CmToFeet(),
                                              costInput.BeamsOffset.CmToFeet());

            return new FloorPropsCost(costInput.RevitInput, europeanInput);
        }

        public static CostGeneticResult AsCostGeneticResult(this EuropeanPropChromosome chromosome, CostGeneticResultInput costInput, int rank)
        {
            var formElesCost = chromosome.Cost.AsFormworkElemntsCost(costInput.TimeLine);
            chromosome.Cost += chromosome.PlywoodCost.TotalCost;
            var totalCost = formElesCost.TotalCost + costInput.ManPowerCost.TotalCost + costInput.EquipmentCost.TotalCost + costInput.TransportationCost.Cost + chromosome.PlywoodCost.TotalCost;
            var costDetailResult = new GeneticCostDetailResult(costInput.TimeLine, costInput.ManPowerCost, costInput.EquipmentCost, formElesCost, chromosome.PlywoodCost, costInput.TransportationCost);
            var detailResults = new List<IGeneticDetailResult>() { chromosome.DetailResult, costDetailResult };
            return new CostGeneticResult(rank, chromosome.Fitness.Value, $"Option {rank}", detailResults, totalCost, chromosome.FloorPropsCost, chromosome.PlywoodCost);
        }

        public static CostGeneticResult AsGeneticResult(this EuropeanPropChromosome chromosome, CostGeneticResultInput costInput, int rank = 0)
        {
            chromosome.FloorPropsCost = chromosome.AsFloorEuropeanPropCost(costInput);
            chromosome.Cost = chromosome.FloorPropsCost.EvaluateCost(costInput.CostFunc).TotalCost();
            var revitFloorPlywood = costInput.PlywoodFunc(chromosome.EuropeanPropDesignInput.PlywoodSection);
            var plywoodCost = revitFloorPlywood.AsPlywoodCost(costInput.FloorArea, costInput.CostFunc);
            chromosome.PlywoodCost = plywoodCost;
            return chromosome.AsCostGeneticResult(costInput, rank);
        }

        #endregion

        #region ShoreBrace Chromosome

        public static Tuple<ShoreBraceDesignInput, double> AsDesignInput(this ShorBraceChromosome chromosome, double slabThicknessCm, double beamThicknessCm, double beamWidthCm, ShoreBraceGeneticIncludedElements includedElements)
        {
            // Genes
            var values = chromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var shoreSpaceIndex = (int)values[3];
            var mainSpacingIndex = (int)values[4];
            var secBeamLengthIndex = (int)values[5];
            var mainBeamLengthIndex = (int)values[6];
            var secSpacingIndex =(int) values[7];

            var shoreSpaces = new List<double>() { 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200 };//15
            var shoreSpace = shoreSpaces[shoreSpaceIndex];
            var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal);
            if (secLengths.Count - 1 < secBeamLengthIndex)
                return null;
            var secTotalLength = secLengths[secBeamLengthIndex];

            var mainSpacingVal = includedElements.IncludedShoreBracing[mainSpacingIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal);
            if (mainLengths.Count - 1 < mainBeamLengthIndex)
                return null;
            var mainTotalLength = mainLengths[mainBeamLengthIndex];

            return Tuple.Create(new ShoreBraceDesignInput(
              includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
              includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
              includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
               mainSpacingVal,
               secTotalLength,
               mainTotalLength,
               slabThicknessCm,
               new UserDefinedSecondaryBeamSpacing(AvailableSecBeamSpacings[ secSpacingIndex]),
               beamThicknessCm,
               beamWidthCm), shoreSpace);
        }

        public static double EvaluateFitnessShorBraceDesign(this ShorBraceChromosome designChromosome,
                                                                 double slabThicknessCm,
                                                                 double beamThicknessCm,
                                                                 double beamWidthCm,
                                                                 ShoreBraceGeneticIncludedElements includedElements)

        {
            //// Genes
            //var values = designChromosome.ToFloatingPoints();
            //var plywoodSectionVal = values[0];
            //var secondaryBeamSectionVal = values[1];
            //var mainBeamSectionVal = values[2];

            //// Create an instance of Random
            //var random = new Random();

            //var maxShoreSpace = 200.0;
            //var minShoreSpace = 60.0;


            //var shoreSpaces = minShoreSpace.GetAtInterval(maxShoreSpace, 10);
            //var shoreSpaceIndex = random.Next(0, shoreSpaces.Count);
            //var shoreSpace = shoreSpaces[shoreSpaceIndex];
            //// SecondaryBeam Length
            //var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal)
            //                         .Where(bl => bl >= (Database.SHORE_MAIN_HALF_WIDTH * 2.0) + shoreSpace + (2 * RevitBase.MIN_CANTILEVER_LENGTH))
            //                         .ToList();
            //int secIndex = random.Next(secLengths.Count);
            //var secTotalLength = secLengths[secIndex];

            //// MainBeam Length & Main Spacing
            //var spacings = includedElements.IncludedShoreBracing.ToList();
            //int spacingIndex = random.Next(spacings.Count);
            //var mainSpacingVal = spacings[spacingIndex];
            //var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).Where(bl => bl >= mainSpacingVal + (2 * RevitBase.MIN_CANTILEVER_LENGTH)).ToList();
            //int mainIndex = random.Next(mainLengths.Count);
            //var mainTotalLength = mainLengths[mainIndex];

            var tuple = designChromosome.AsDesignInput(slabThicknessCm, beamThicknessCm, beamWidthCm, includedElements);
            if (tuple == null)
                return -1;

            (var input, var shoreSpace) = tuple;

            // Design Input
            designChromosome.ShorBraceDesignInput = input;

            var designer = ShoreBraceDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.ShorBraceDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<FrameDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item3.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item3.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;

                    var plywoodDesignOutput = designOutput.Plywood.AsSelectedMaxDesignOutput();

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.SecondaryBeamTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.MainBeamTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Shore Brace System, M.B. Direction: {input.Spacing} cm, S.B. Direction: {SHORE_MAIN_HALF_WIDTH * 2}", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);

                    designChromosome.SecondaryBeamSpacing = designOutput.Plywood.Item1.Span;
                    designChromosome.ShoreBraceSpacing = shoreSpace;

                    return (ratio1 + ratio2 + ratio3 + ratio4);
                }

                return -1.0;
            };

            return designOutputValidation.Match(errs => -2.0, calculateFitness);
        }

        public static double EvaluateFitnessShorBraceCost(this ShorBraceChromosome designChromosome,
                                                               double slabThicknessCm,
                                                               double beamThicknessCm,
                                                               double beamWidthCm,
                                                               CostGeneticResultInput costInput,
                                                               ShoreBraceGeneticIncludedElements includedElements)
        {
            var tuple = designChromosome.AsDesignInput(slabThicknessCm, beamThicknessCm, beamWidthCm, includedElements);
            if (tuple == null)
                return -1;

            (var input, var shoreSpace) = tuple;

            // Design Input
            designChromosome.ShorBraceDesignInput = input;

            var designer = ShoreBraceDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.ShorBraceDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<FrameDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item3.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var plywoodDesignOutput = designOutput.Plywood.AsSelectedMaxDesignOutput();

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.SecondaryBeamTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.MainBeamTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Shore Brace System, M.B. Direction: {input.Spacing} cm, S.B. Direction: {SHORE_MAIN_HALF_WIDTH * 2}", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);

                    designChromosome.SecondaryBeamSpacing = designOutput.Plywood.Item1.Span;
                    designChromosome.ShoreBraceSpacing = shoreSpace;


                    var newCostInput = costInput.UpdateCostInputWithNewRevitInput(costInput.RevitInput.UpdateWithNewXYZ(costInput.RevitInput.MainBeamDir.CrossProduct(XYZ.BasisZ)));
                    var costInputs = new List<CostGeneticResultInput>() { costInput, newCostInput };

                    var result = costInputs.Select(inp => designChromosome.AsFloorShorBraceCost(inp))
                                           .Select(floorCost => Tuple.Create(floorCost, floorCost.EvaluateCost(costInput.CostFunc).TotalCost()))
                                           .OrderBy(tup => tup.Item2)
                                           .First();

                    var revitFloorPlywood = costInput.PlywoodFunc(designChromosome.ShorBraceDesignInput.PlywoodSection);
                    var plywoodCost = revitFloorPlywood.AsPlywoodCost(costInput.FloorArea, costInput.CostFunc);
                    designChromosome.PlywoodCost = plywoodCost;

                    designChromosome.FloorShoreBraceCost = result.Item1;

                    designChromosome.Cost = result.Item2;

                    var totalCost = designChromosome.Cost + designChromosome.PlywoodCost.TotalCost;

                    if (totalCost > 0.0)
                    {
                        var costInverse = 100000.0 / totalCost;

                        return costInverse;
                    }

                    return -1.0;
                }

                return -1.0;
            };

            return designOutputValidation.Match(errs => -2.0, calculateFitness);
        }

        public static FloorShoreBraceCost AsFloorShorBraceCost(this ShorBraceChromosome chromosome, CostGeneticResultInput costInput)
        {
            var shoreInput = new RevitFloorShoreInput(chromosome.ShorBraceDesignInput.PlywoodSection,
                                                          chromosome.ShorBraceDesignInput.SecondaryBeamSection.ToRevitBeamSectionName(),
                                                          chromosome.ShorBraceDesignInput.MainBeamSection.ToRevitBeamSectionName(),
                                                          chromosome.ShorBraceDesignInput.SecondaryBeamTotalLength.CmToFeet(),
                                                          chromosome.SecondaryBeamSpacing.CmToFeet(),
                                                          chromosome.ShorBraceDesignInput.MainBeamTotalLength.CmToFeet(),
                                                          chromosome.ShorBraceDesignInput.Spacing.CmToFeet(),
                                                          chromosome.ShoreBraceSpacing.CmToFeet(),
                                                          costInput.BoundaryLinesOffest.CmToFeet(),
                                                          costInput.BeamsOffset.CmToFeet());

            return new FloorShoreBraceCost(costInput.RevitInput, shoreInput);
        }
        public static CostGeneticResult AsCostGeneticResult(this ShorBraceChromosome chromosome, CostGeneticResultInput costInput, int rank)
        {
            var formElesCost = chromosome.Cost.AsFormworkElemntsCost(costInput.TimeLine);
            chromosome.Cost += chromosome.PlywoodCost.TotalCost;
            var totalCost = formElesCost.TotalCost + costInput.ManPowerCost.TotalCost + costInput.EquipmentCost.TotalCost + costInput.TransportationCost.Cost + chromosome.PlywoodCost.TotalCost;
            var costDetailResult = new GeneticCostDetailResult(costInput.TimeLine, costInput.ManPowerCost, costInput.EquipmentCost, formElesCost, chromosome.PlywoodCost, costInput.TransportationCost);
            var detailResults = new List<IGeneticDetailResult>() { chromosome.DetailResult, costDetailResult };
            return new CostGeneticResult(rank, chromosome.Fitness.Value, $"Option {rank}", detailResults, totalCost, chromosome.FloorShoreBraceCost, chromosome.PlywoodCost);
        }
        public static CostGeneticResult AsGeneticResult(this ShorBraceChromosome chromosome, CostGeneticResultInput costInput, int rank = 0)
        {
            chromosome.FloorShoreBraceCost = chromosome.AsFloorShorBraceCost(costInput);
            chromosome.Cost = chromosome.FloorShoreBraceCost.EvaluateCost(costInput.CostFunc).TotalCost();
            var revitFloorPlywood = costInput.PlywoodFunc(chromosome.ShorBraceDesignInput.PlywoodSection);
            var plywoodCost = revitFloorPlywood.AsPlywoodCost(costInput.FloorArea, costInput.CostFunc);
            chromosome.PlywoodCost = plywoodCost;
            return chromosome.AsCostGeneticResult(costInput, rank);
        }

        #endregion

        #region Aluminum Props Chromosome

        public static AluPropDesignInput AsDesignInput(this AluminumPropChromosome chromosome, double slabThicknessCm, double beamThicknessCm, double beamWidthCm, AluPropsGeneticIncludedElements includedElements)
        {
            // Genes
            var values = chromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var secPropsSpacingIndex = (int)values[3];
            var mainPropsSpacingIndex = (int)values[4];
            var secBeamLengthIndex = (int)values[5];
            var mainBeamLengthIndex = (int)values[6];
            var secSpacingIndex =(int) values[7];

            var propsSpacingVals = new List<double>() { 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230, 240, 250, 260, 270, 280, 290, 300 };//25.

            var secSpacingVal = propsSpacingVals[secPropsSpacingIndex];
            var secLengths = Database.GetBeamLengths((BeamSectionName)(int)secondaryBeamSectionVal).ToList();
            if (secLengths.Count - 1 < secBeamLengthIndex)
                return null;
            var secTotalLength = secLengths[secBeamLengthIndex];

            var mainSpacingVal = propsSpacingVals[mainPropsSpacingIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).ToList();
            if (mainLengths.Count - 1 < mainBeamLengthIndex)
                return null;
            var mainTotalLength = mainLengths[mainBeamLengthIndex];

            return new AluPropDesignInput(
               includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
               includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
               includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
                mainSpacingVal,
                secSpacingVal,
                secTotalLength,
                mainTotalLength,
                slabThicknessCm,
                new UserDefinedSecondaryBeamSpacing(AvailableSecBeamSpacings[ secSpacingIndex]),
                beamThicknessCm,
                beamWidthCm);
        }

        public static double EvaluateFitnessAluminumPropDesign(this AluminumPropChromosome designChromosome,
                                                                    double slabThicknessCm,
                                                                    double beamThicknessCm,
                                                                    double beamWidthCm,
                                                                    AluPropsGeneticIncludedElements includedElements)
        {
            //// Genes
            //var values = designChromosome.ToFloatingPoints();
            //var plywoodSectionVal = values[0];
            //var secondaryBeamSectionVal = values[1];
            //var mainBeamSectionVal = values[2];

            //// Create an instance of Random
            //var random = new Random();

            //// SecondaryBeam Length & Secondary Spacing
            //var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal);
            //int secIndex = random.Next(secLengths.Count);
            //var secTotalLength = secLengths[secIndex];
            //var secSpacingVal = random.Next(50, (int)secTotalLength - (2 * (int)RevitBase.MIN_CANTILEVER_LENGTH));

            //// MainBeam Length & Main Spacing
            //var mainLengths = Database.GetBeamLengths((BeamSectionName)mainBeamSectionVal);
            //int mainIndex = random.Next(mainLengths.Count);
            //var mainTotalLength = mainLengths[mainIndex];
            //var mainSpacingVal = random.Next(50, (int)mainTotalLength - (2 * (int)RevitBase.MIN_CANTILEVER_LENGTH));

            var input = designChromosome.AsDesignInput(slabThicknessCm, beamThicknessCm, beamWidthCm, includedElements);
            if (input is null)
                return -1;
            // Design Input
            designChromosome.AluminumPropDesignInput = input;

            var designer = AluPropDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.AluminumPropDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<PropDesignOutput, double> calculateFitness = designOutput =>
            {

                var flag1 = designOutput.Plywood.Item3.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item3.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;



                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()},Selected Span: {designOutput.Plywood.Item2.Span} cm, Max Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item3.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.SecondaryBeamTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.MainBeamTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Aluminum Prop System, M.B. Direction {input.MainSpacing} cm, S.B. Direction: {input.SecondarySpacing} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);


                    return (ratio1 + ratio2 + ratio3 + ratio4);
                }

                return -1.0;
            };

            return designOutputValidation.Match(errs => -2.0, calculateFitness);
        }
        public static NoCostGeneticResult AsGeneticResult(this AluminumPropChromosome chromosome, int rank = 0)
        {
            var detailResults = new List<IGeneticDetailResult>() { chromosome.DetailResult };
            return new NoCostGeneticResult(rank, chromosome.Fitness.Value, $"Option {rank}", detailResults);
        }

        #endregion

        #region Frame Chromosome

        public static FrameDesignInput AsDesignInput(this FrameChromosome designChromosome, double slabThicknessCm, double beamThicknessCm, double beamWidthCm, FrameGeneticIncludedElements includedElements)
        {
            // Genes
            var values = designChromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var frameTypeVal = values[3];
            var mainSpacingIndex = (int)values[4];
            var secBeamLengthIndex = (int)values[5];
            var mainBeamLengthIndex = (int)values[6];
            var secSpacingIndex =(int)values[7];

            var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal);
            if (secLengths.Count - 1 < secBeamLengthIndex)
                return null;
            var secTotalLength = secLengths[secBeamLengthIndex];


            var mainSpacingVal = includedElements.IncludedShoreBracing[mainSpacingIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal);
            if (mainLengths.Count - 1 < mainBeamLengthIndex)
                return null;
            var mainTotalLength = mainLengths[mainBeamLengthIndex];
            return new FrameDesignInput(
              includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
              includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
              includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
               mainSpacingVal,
              includedElements.IncludedFrames[((int)frameTypeVal)],
               secTotalLength,
               mainTotalLength,
               slabThicknessCm,
               new UserDefinedSecondaryBeamSpacing(AvailableSecBeamSpacings[ secSpacingIndex]),
               beamThicknessCm,
               beamWidthCm);
        }

        public static double EvaluateFitnessFrameDesign(this FrameChromosome designChromosome,
                                                             double slabThicknessCm,
                                                             double beamThicknessCm,
                                                             double beamWidthCm,
                                                            FrameGeneticIncludedElements includedElements)
        {
            //// Genes
            //var values = designChromosome.ToFloatingPoints();
            //var plywoodSectionVal = values[0];
            //var secondaryBeamSectionVal = values[1];
            //var mainBeamSectionVal = values[2];
            //var frameTypeVal = values[3];

            //// Create an instance of Random
            //var random = new Random();

            //// SecondaryBeam Length
            //var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal)
            //                         .Where(bl => bl >= (Database.SHORE_MAIN_HALF_WIDTH * 2.0) + (2 * RevitBase.MIN_CANTILEVER_LENGTH))
            //                         .ToList();
            //int secIndex = random.Next(secLengths.Count);
            //var secTotalLength = secLengths[secIndex];

            //// MainBeam Length & Main Spacing

            //var spacings = includedElements.IncludedShoreBracing.ToList();
            //int spacingIndex = random.Next(spacings.Count);
            //var mainSpacingVal = spacings[spacingIndex];
            //var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).Where(bl => bl >= mainSpacingVal + (2 * RevitBase.MIN_CANTILEVER_LENGTH)).ToList();
            //int mainIndex = random.Next(mainLengths.Count);
            //var mainTotalLength = mainLengths[mainIndex];

            #region comments
            //var mainLengths = Database.GetBeamLengths((BeamSectionName)mainBeamSectionVal);
            //int mainIndex = random.Next(mainLengths.Count);
            //var mainTotalLength = mainLengths[mainIndex];
            //var spacings = Database.ShoreBraceSystemCrossBraces.Where(cb => cb + (2 * RevitBase.MIN_CANTILEVER_LENGTH) <= mainTotalLength)
            //                                                   .ToList();
            //int spacingIndex = random.Next(spacings.Count);
            //var mainSpacingVal = spacings[spacingIndex];
            #endregion

            var input = designChromosome.AsDesignInput(slabThicknessCm, beamThicknessCm, beamWidthCm, includedElements);
            if (input == null)
                return -1;

            // Design Inputs
            designChromosome.FrameDesignInput = input;

            var designer = FrameDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.FrameDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<FrameDesignOutput, double> calculateFitness = designOutput =>
            {

                var flag1 = designOutput.Plywood.Item3.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item3.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;


                    var plywoodDesignOutput = designOutput.Plywood.AsSelectedMaxDesignOutput();

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.SecondaryBeamTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {input.MainBeamTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Frame System, M.B. Direction: {input.Spacing} cm, S.B. Direction: {SHORE_MAIN_HALF_WIDTH * 2} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);

                    return (ratio1 + ratio2 + ratio3 + ratio4);
                }

                return -1.0;
            };

            return designOutputValidation.Match(errs => -2.0, calculateFitness);
        }
        public static NoCostGeneticResult AsGeneticResult(this FrameChromosome chromosome, int rank = 0)
        {
            var detailResults = new List<IGeneticDetailResult>() { chromosome.DetailResult };
            return new NoCostGeneticResult(rank, chromosome.Fitness.Value, $"Option {rank}", detailResults);
        }

        #endregion
    }
}
