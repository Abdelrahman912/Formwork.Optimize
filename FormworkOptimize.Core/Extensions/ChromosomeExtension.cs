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

        public static double EvaluateFitnessCuplockDesign(this CuplockChromosome designChromosome, 
                                                               double slabThicknessCm, 
                                                               double beamThicknessCm, 
                                                               double beamWidthCm,
                                                              CuplockGeneticIncludedElements includedElements)
        {
            // Genes
            var values = designChromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var steelTypeVal = values[3];

            // Create an instance of Random
            var random = new Random();

            // SecondaryBeam Length & Secondary Spacing
            var secSpacings = includedElements.IncludedLedgers.ToList();
            int secSpacingIndex = random.Next(secSpacings.Count);
            var secSpacingVal = secSpacings[secSpacingIndex];
            var secLengths = Database.GetBeamLengths((BeamSectionName)(int)secondaryBeamSectionVal).Where(bl => bl >= secSpacingVal + (2 * RevitBase.MIN_CANTILEVER_LENGTH)).ToList();
            int secIndex = random.Next(secLengths.Count);
            var secTotalLength = secLengths[secIndex];

            // MainBeam Length & Main Spacing
            var mainSpacings = includedElements.IncludedLedgers.ToList();
            int mainSpacingIndex = random.Next(mainSpacings.Count);
            var mainSpacingVal = mainSpacings[mainSpacingIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).Where(bl => bl >= mainSpacingVal + (2 * RevitBase.MIN_CANTILEVER_LENGTH)).ToList();
            int mainIndex = random.Next(mainLengths.Count);
            var mainTotalLength = mainLengths[mainIndex];

            // Design Input
            designChromosome.CuplockDesignInput = new CuplockDesignInput(
             includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
             includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
             includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
             includedElements.IncludedSteelTypes[((int)steelTypeVal)],
             mainSpacingVal,
             secSpacingVal,
             secTotalLength,
             mainTotalLength,
             slabThicknessCm,
             beamThicknessCm,
             beamWidthCm);

            var designer = CuplockDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.CuplockDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<CuplockDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;

                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {secTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {mainTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Cuplock System, M.B. Direction: {mainSpacingVal} cm, S.B. Direction: {secSpacingVal} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

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
            // Genes
            var values = designChromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var steelTypeVal = values[3];

            // Create an instance of Random
            var random = new Random();

            // SecondaryBeam Length & Secondary Spacing
            var secSpacings = includedElements.IncludedLedgers.ToList();
            int secSpacingIndex = random.Next(secSpacings.Count);
            var secSpacingVal = secSpacings[secSpacingIndex];
            var secLengths = Database.GetBeamLengths((BeamSectionName)(int)secondaryBeamSectionVal).Where(bl => bl >= secSpacingVal + (2 * RevitBase.MIN_CANTILEVER_LENGTH)).ToList();
            int secIndex = random.Next(secLengths.Count);
            var secTotalLength = secLengths[secIndex];

            // MainBeam Length & Main Spacing
            var mainSpacings = includedElements.IncludedLedgers.ToList();
            int mainSpacingIndex = random.Next(mainSpacings.Count);
            var mainSpacingVal = mainSpacings[mainSpacingIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).Where(bl => bl >= mainSpacingVal + (2 * RevitBase.MIN_CANTILEVER_LENGTH)).ToList();
            int mainIndex = random.Next(mainLengths.Count);
            var mainTotalLength = mainLengths[mainIndex];

            // Design Input
            designChromosome.CuplockDesignInput = new CuplockDesignInput(
             includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
             includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
             includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
             includedElements.IncludedSteelTypes[((int)steelTypeVal)],
             mainSpacingVal,
             secSpacingVal,
             secTotalLength,
             mainTotalLength,
             slabThicknessCm,
             beamThicknessCm,
             beamWidthCm);

            var designer = CuplockDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.CuplockDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<CuplockDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {secTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {mainTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Cuplock System, M.B. Direction: {mainSpacingVal} cm, S.B. Direction: {secSpacingVal} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);

                    designChromosome.SecondaryBeamSpacing = designOutput.Plywood.Item1.Span;

                    var newCostInput = costInput.UpdateCostInputWithNewRevitInput(costInput.RevitInput.UpdateWithNewXYZ(costInput.RevitInput.MainBeamDir.CrossProduct(XYZ.BasisZ)));
                    var costInputs = new List<CostGeneticResultInput>() { costInput, newCostInput };

                    var result = costInputs.Select(input => designChromosome.AsFloorCuplockCost(input))
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
        public static CostGeneticResult AsCostGeneticResult (this CuplockChromosome chromosome, CostGeneticResultInput costInput, int rank)
        {
            var formElesCost = chromosome.Cost.AsFormworkElemntsCost(costInput.TimeLine);
            chromosome.Cost += chromosome.PlywoodCost.TotalCost;
            var totalCost = formElesCost.TotalCost + costInput.ManPowerCost.TotalCost + costInput.EquipmentCost.TotalCost+costInput.TransportationCost.Cost+ chromosome.PlywoodCost.TotalCost;
            var costDetailResult = new GeneticCostDetailResult(costInput.TimeLine, costInput.ManPowerCost, costInput.EquipmentCost, formElesCost, chromosome.PlywoodCost, costInput.TransportationCost);
            var detailResults = new List<IGeneticDetailResult>() { chromosome.DetailResult,costDetailResult };
            return new CostGeneticResult(rank, chromosome.Fitness.Value, $"Option {rank}",detailResults , totalCost, chromosome.FloorCuplockCost, chromosome.PlywoodCost);
        }
        public static CostGeneticResult AsGeneticResult(this CuplockChromosome chromosome, CostGeneticResultInput costInput, int rank = 0)
        {
            chromosome.FloorCuplockCost = chromosome.AsFloorCuplockCost(costInput);
            chromosome.Cost = chromosome.FloorCuplockCost.EvaluateCost(costInput.CostFunc).TotalCost();
            var revitFloorPlywood = costInput.PlywoodFunc(chromosome.CuplockDesignInput.PlywoodSection);
            var plywoodCost = revitFloorPlywood.AsPlywoodCost(costInput.FloorArea, costInput.CostFunc);
            chromosome.PlywoodCost = plywoodCost;
            return chromosome.AsCostGeneticResult(costInput,rank);
        }

        #endregion

        #region European Props Chromosome

        public static double EvaluateFitnessEuropeanPropDesign(this EuropeanPropChromosome designChromosome, 
                                                                    double slabThicknessCm, 
                                                                    double beamThicknessCm, 
                                                                    double beamWidthCm,
                                                                    EuropeanPropsGeneticInludedElements includedElements)
        {
            // Genes
            var values = designChromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var propTypeVal = values[3];

            // Create an instance of Random
            var random = new Random();

            // SecondaryBeam Length & Secondary Spacing
            var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal);
            int secIndex = random.Next(secLengths.Count);
            var secTotalLength = secLengths[secIndex];
            var secSpacingVal = random.Next(50, (int)secTotalLength - (2 * (int)RevitBase.MIN_CANTILEVER_LENGTH));

            // MainBeam Length & Main Spacing
            var mainLengths = Database.GetBeamLengths((BeamSectionName)mainBeamSectionVal);
            int mainIndex = random.Next(mainLengths.Count);
            var mainTotalLength = mainLengths[mainIndex];
            var mainSpacingVal = random.Next(50, (int)mainTotalLength - (2 * (int)RevitBase.MIN_CANTILEVER_LENGTH));

            // Design Input
            designChromosome.EuropeanPropDesignInput = new EuropeanPropDesignInput(
               includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
               includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
               includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
               includedElements.IncludedProps[((int)propTypeVal)],
                mainSpacingVal,
                secSpacingVal,
                secTotalLength,
                mainTotalLength,
                slabThicknessCm,
                beamThicknessCm,
                beamWidthCm);

            var designer = EuropeanPropDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.EuropeanPropDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<PropDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;

                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {secTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {mainTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"European Prop System, M.B. Direction: {mainSpacingVal} cm, S.B. Direction: {secSpacingVal} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

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
            // Genes
            var values = designChromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var propTypeVal = values[3];

            // Create an instance of Random
            var random = new Random();

            // SecondaryBeam Length & Secondary Spacing
            var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal);
            int secIndex = random.Next(secLengths.Count);
            var secTotalLength = secLengths[secIndex];
            var secSpacingVal = random.Next(50, (int)secTotalLength - (2 * (int)RevitBase.MIN_CANTILEVER_LENGTH));

            // MainBeam Length & Main Spacing
            var mainLengths = Database.GetBeamLengths((BeamSectionName)mainBeamSectionVal);
            int mainIndex = random.Next(mainLengths.Count);
            var mainTotalLength = mainLengths[mainIndex];
            var mainSpacingVal = random.Next(50, (int)mainTotalLength - (2 * (int)RevitBase.MIN_CANTILEVER_LENGTH));

            // Design Input
            designChromosome.EuropeanPropDesignInput = new EuropeanPropDesignInput(
                includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
                includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
                includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
                includedElements.IncludedProps[((int)propTypeVal)],
                 mainSpacingVal,
                 secSpacingVal,
                 secTotalLength,
                 mainTotalLength,
                 slabThicknessCm,
                 beamThicknessCm,
                 beamWidthCm);

            var designer = EuropeanPropDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.EuropeanPropDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<PropDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {secTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {mainTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"European Prop System, M.B. Direction: {mainSpacingVal} cm, S.B. Direction: {secSpacingVal} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);
                    designChromosome.SecondaryBeamSpacing = designOutput.Plywood.Item1.Span;

                  


                    var newCostInput = costInput.UpdateCostInputWithNewRevitInput(costInput.RevitInput.UpdateWithNewXYZ(costInput.RevitInput.MainBeamDir.CrossProduct(XYZ.BasisZ)));
                    var costInputs = new List<CostGeneticResultInput>() { costInput, newCostInput };

                    var result = costInputs.Select(input => designChromosome.AsFloorEuropeanPropCost(input))
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
            var totalCost = formElesCost.TotalCost + costInput.ManPowerCost.TotalCost + costInput.EquipmentCost.TotalCost+costInput.TransportationCost.Cost+ chromosome.PlywoodCost.TotalCost;
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
            return chromosome.AsCostGeneticResult(costInput,rank);
        }

        #endregion

        #region ShoreBrace Chromosome

        public static double EvaluateFitnessShorBraceDesign(this ShorBraceChromosome designChromosome, 
                                                                 double slabThicknessCm, 
                                                                 double beamThicknessCm, 
                                                                 double beamWidthCm,
                                                                 ShoreBraceGeneticIncludedElements includedElements)
                                                              
        {
            // Genes
            var values = designChromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];

            // Create an instance of Random
            var random = new Random();

            var maxShoreSpace = 200.0;
            var minShoreSpace = 60.0;


            var shoreSpaces = minShoreSpace.GetAtInterval(maxShoreSpace, 10);
            var shoreSpaceIndex = random.Next(0, shoreSpaces.Count);
            var shoreSpace = shoreSpaces[shoreSpaceIndex];
            // SecondaryBeam Length
            var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal)
                                     .Where(bl => bl >= (Database.SHORE_MAIN_HALF_WIDTH * 2.0) + shoreSpace + (2 * RevitBase.MIN_CANTILEVER_LENGTH))
                                     .ToList();
            int secIndex = random.Next(secLengths.Count);
            var secTotalLength = secLengths[secIndex];

            // MainBeam Length & Main Spacing
            var spacings = includedElements.IncludedShoreBracing.ToList();
            int spacingIndex = random.Next(spacings.Count);
            var mainSpacingVal = spacings[spacingIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).Where(bl => bl >= mainSpacingVal + (2 * RevitBase.MIN_CANTILEVER_LENGTH)).ToList();
            int mainIndex = random.Next(mainLengths.Count);
            var mainTotalLength = mainLengths[mainIndex];

            // Design Input
            designChromosome.ShorBraceDesignInput = new ShoreBraceDesignInput(
              includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
              includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
              includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
               mainSpacingVal,
               secTotalLength,
               mainTotalLength,
               slabThicknessCm,
               beamThicknessCm,
               beamWidthCm);

            var designer = ShoreBraceDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.ShorBraceDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<FrameDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;

                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {secTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {mainTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Shore Brace System, M.B. Direction: {mainSpacingVal} cm, S.B. Direction: {SHORE_MAIN_HALF_WIDTH * 2}", new List<DesignReport>() { designOutput.Shoring.Item2 });

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
            // Genes
            var values = designChromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];

            // Create an instance of Random
            var random = new Random();

            var maxShoreSpace = 200.0;
            var minShoreSpace = 60.0;


            var shoreSpaces = minShoreSpace.GetAtInterval(maxShoreSpace, 10);
            var shoreSpaceIndex = random.Next(0, shoreSpaces.Count);
            var shoreSpace = shoreSpaces[shoreSpaceIndex];
            // SecondaryBeam Length
            var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal)
                                     .Where(bl => bl >= (Database.SHORE_MAIN_HALF_WIDTH * 2.0) + shoreSpace + (2 * RevitBase.MIN_CANTILEVER_LENGTH))
                                     .ToList();
            int secIndex = random.Next(secLengths.Count);
            var secTotalLength = secLengths[secIndex];

            // MainBeam Length & Main Spacing
            var spacings = includedElements.IncludedShoreBracing.ToList();
            int spacingIndex = random.Next(spacings.Count);
            var mainSpacingVal = spacings[spacingIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).Where(bl => bl >= mainSpacingVal + (2 * RevitBase.MIN_CANTILEVER_LENGTH)).ToList();
            int mainIndex = random.Next(mainLengths.Count);
            var mainTotalLength = mainLengths[mainIndex];

            // Design Input
            designChromosome.ShorBraceDesignInput = new ShoreBraceDesignInput(
              includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
              includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
              includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
               mainSpacingVal,
               secTotalLength,
               mainTotalLength,
               slabThicknessCm,
               beamThicknessCm,
               beamWidthCm);

            var designer = ShoreBraceDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.ShorBraceDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<FrameDesignOutput, double> calculateFitness = designOutput =>
            {
                var flag1 = designOutput.Plywood.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {secTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {mainTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Shore Brace System, M.B. Direction: {mainSpacingVal} cm, S.B. Direction: {SHORE_MAIN_HALF_WIDTH * 2}", new List<DesignReport>() { designOutput.Shoring.Item2 });

                    designChromosome.DetailResult = new GeneticDesignDetailResult(plywoodDesignOutput,
                                                                                  secondaryBeamDesignOutput,
                                                                                  mainBeamDesignOutput,
                                                                                  shoringSystemDesignOutput);

                    designChromosome.SecondaryBeamSpacing = designOutput.Plywood.Item1.Span;
                    designChromosome.ShoreBraceSpacing = shoreSpace;


                    var newCostInput = costInput.UpdateCostInputWithNewRevitInput(costInput.RevitInput.UpdateWithNewXYZ(costInput.RevitInput.MainBeamDir.CrossProduct(XYZ.BasisZ)));
                    var costInputs = new List<CostGeneticResultInput>() { costInput, newCostInput };

                    var result = costInputs.Select(input => designChromosome.AsFloorShorBraceCost(input))
                                           .Select(floorCost => Tuple.Create(floorCost, floorCost.EvaluateCost(costInput.CostFunc).TotalCost()))
                                           .OrderBy(tuple => tuple.Item2)
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
            var totalCost = formElesCost.TotalCost + costInput.ManPowerCost.TotalCost + costInput.EquipmentCost.TotalCost+costInput.TransportationCost.Cost+ chromosome.PlywoodCost.TotalCost;
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
            return chromosome.AsCostGeneticResult(costInput,rank);
        }

        #endregion

        #region Aluminum Props Chromosome

        public static double EvaluateFitnessAluminumPropDesign(this AluminumPropChromosome designChromosome, 
                                                                    double slabThicknessCm, 
                                                                    double beamThicknessCm, 
                                                                    double beamWidthCm,
                                                                    AluPropsGeneticIncludedElements includedElements)
        {
            // Genes
            var values = designChromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];

            // Create an instance of Random
            var random = new Random();

            // SecondaryBeam Length & Secondary Spacing
            var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal);
            int secIndex = random.Next(secLengths.Count);
            var secTotalLength = secLengths[secIndex];
            var secSpacingVal = random.Next(50, (int)secTotalLength - (2 * (int)RevitBase.MIN_CANTILEVER_LENGTH));

            // MainBeam Length & Main Spacing
            var mainLengths = Database.GetBeamLengths((BeamSectionName)mainBeamSectionVal);
            int mainIndex = random.Next(mainLengths.Count);
            var mainTotalLength = mainLengths[mainIndex];
            var mainSpacingVal = random.Next(50, (int)mainTotalLength - (2 * (int)RevitBase.MIN_CANTILEVER_LENGTH));

            // Design Input
            designChromosome.AluminumPropDesignInput = new AluPropDesignInput(
               includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
               includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
               includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
                mainSpacingVal,
                secSpacingVal,
                secTotalLength,
                mainTotalLength,
                slabThicknessCm,
                beamThicknessCm,
                beamWidthCm);

            var designer = AluPropDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.AluminumPropDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<PropDesignOutput, double> calculateFitness = designOutput =>
            {

                var flag1 = designOutput.Plywood.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;



                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {secTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {mainTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Aluminum Prop System, M.B. Direction {mainSpacingVal} cm, S.B. Direction: {secSpacingVal} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

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
            return new NoCostGeneticResult(rank, chromosome.Fitness.Value, $"Option {rank}",detailResults );
        }

        #endregion

        #region Frame Chromosome

        public static double EvaluateFitnessFrameDesign(this FrameChromosome designChromosome, 
                                                             double slabThicknessCm, 
                                                             double beamThicknessCm, 
                                                             double beamWidthCm,
                                                            FrameGeneticIncludedElements includedElements)
        {
            // Genes
            var values = designChromosome.ToFloatingPoints();
            var plywoodSectionVal = values[0];
            var secondaryBeamSectionVal = values[1];
            var mainBeamSectionVal = values[2];
            var frameTypeVal = values[3];

            // Create an instance of Random
            var random = new Random();

            // SecondaryBeam Length
            var secLengths = Database.GetBeamLengths((BeamSectionName)secondaryBeamSectionVal)
                                     .Where(bl => bl >= (Database.SHORE_MAIN_HALF_WIDTH * 2.0) + (2 * RevitBase.MIN_CANTILEVER_LENGTH))
                                     .ToList();
            int secIndex = random.Next(secLengths.Count);
            var secTotalLength = secLengths[secIndex];

            // MainBeam Length & Main Spacing

            var spacings = includedElements.IncludedShoreBracing.ToList();
            int spacingIndex = random.Next(spacings.Count);
            var mainSpacingVal = spacings[spacingIndex];
            var mainLengths = Database.GetBeamLengths((BeamSectionName)(int)mainBeamSectionVal).Where(bl => bl >= mainSpacingVal + (2 * RevitBase.MIN_CANTILEVER_LENGTH)).ToList();
            int mainIndex = random.Next(mainLengths.Count);
            var mainTotalLength = mainLengths[mainIndex];

            #region comments
            //var mainLengths = Database.GetBeamLengths((BeamSectionName)mainBeamSectionVal);
            //int mainIndex = random.Next(mainLengths.Count);
            //var mainTotalLength = mainLengths[mainIndex];
            //var spacings = Database.ShoreBraceSystemCrossBraces.Where(cb => cb + (2 * RevitBase.MIN_CANTILEVER_LENGTH) <= mainTotalLength)
            //                                                   .ToList();
            //int spacingIndex = random.Next(spacings.Count);
            //var mainSpacingVal = spacings[spacingIndex];
            #endregion

            // Design Inputs
            designChromosome.FrameDesignInput = new FrameDesignInput(
              includedElements.IncludedPlywoods[((int)plywoodSectionVal)],
              includedElements.IncludedBeamSections[((int)secondaryBeamSectionVal)],
              includedElements.IncludedBeamSections[((int)mainBeamSectionVal)],
               mainSpacingVal,
              includedElements.IncludedFrames[((int)frameTypeVal)],
               secTotalLength,
               mainTotalLength,
               slabThicknessCm,
               beamThicknessCm,
               beamWidthCm);

            var designer = FrameDesigner.Instance;

            var designOutputValidation = designer.Design(designChromosome.FrameDesignInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction);

            Func<FrameDesignOutput, double> calculateFitness = designOutput =>
            {

                var flag1 = designOutput.Plywood.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag2 = designOutput.SecondaryBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag3 = designOutput.MainBeam.Item2.TrueForAll(dr => dr.Status == DesignStatus.SAFE);
                var flag4 = designOutput.Shoring.Item2.Status == DesignStatus.SAFE;

                var isCandidate = flag1 && flag2 && flag3 && flag4;

                if (isCandidate)
                {
                    var ratio1 = designOutput.Plywood.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio2 = designOutput.SecondaryBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio3 = designOutput.MainBeam.Item2.Select(dr => dr.DesignRatio).Sum();
                    var ratio4 = designOutput.Shoring.Item2.DesignRatio;


                    var plywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList());

                    var secondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}, Length: {secTotalLength} cm", designOutput.SecondaryBeam.Item2.ToList());

                    var mainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}, Length: {mainTotalLength} cm", designOutput.MainBeam.Item2.ToList());

                    var shoringSystemDesignOutput = new ShoringDesignOutput($"Frame System, M.B. Direction: {mainSpacingVal} cm, S.B. Direction: {SHORE_MAIN_HALF_WIDTH * 2} cm", new List<DesignReport>() { designOutput.Shoring.Item2 });

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
            return new NoCostGeneticResult(rank, chromosome.Fitness.Value, $"Option {rank}",detailResults );
        }

        #endregion
    }
}
