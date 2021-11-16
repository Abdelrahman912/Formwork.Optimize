using FormworkOptimize.Core.DTOS.Genetic;
using FormworkOptimize.Core.Entities.Genetic;
using FormworkOptimize.Core.Extensions;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using System.Collections.Generic;
using System.Linq;
using FormworkOptimize.Core.Entities.GeneticParameters;
using static FormworkOptimize.Core.Comparers.Comparers;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Infrastructure.Framework.Threading;
using FormworkOptimize.Core.Entities.GeneticResult;
using System;

namespace FormworkOptimize.Core.Helpers.GeneticHelper
{
    public static class GeneticFactoryHelper
    {
        #region General

        public static List<ChromosomeHistory> StartGA(this GeneticAlgorithm ga)
        {
            var history = new List<ChromosomeHistory>();
            ga.GenerationRan += (s, e) =>
            {
                var newHist = new ChromosomeHistory(ga.GenerationsNumber, ga.BestChromosome.Fitness.Value);
                history.Add(newHist);
            };
            ga.Start();
            return history;
        }

        private static GeneticAlgorithm CreateGenetic(GeneticDesignInput geneticInput,
                                                      BinaryChromosomeBase chromosome,
                                                      IFitness fitness)
        {
            var population = new Population(geneticInput.NoPopulation, geneticInput.NoPopulation * 2, chromosome);
            //! Creating the selection
            var selection = new EliteSelection();

            //! Creating the crossover
            var crossover = new UniformCrossover();

            //! Creating the mutation
            var mutation = new FlipBitMutation();

            //! Creating the termination
            var termination = new GenerationNumberTermination(geneticInput.NoGenerations);

            //! Running the GA
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            ga.Termination = termination;
            ga.CrossoverProbability = (float)geneticInput.CrossOverProbability;
            ga.MutationProbability = (float)geneticInput.MutationProbability;

            //ga.Reinsertion = new UniformReinsertion();
            //ga.TaskExecutor = new TplTaskExecutor();
            return ga;
        }


        #endregion

        #region Cuplock

        public static CuplockGeneticIncludedElements AsCuplockIncludedElements(this GeneticIncludedElements includedEles)
        {
            return new CuplockGeneticIncludedElements(includedEles.IncludedLedgers, includedEles.IncludedVerticals, includedEles.IncludedCuplockBraces, includedEles.IncludedPlywoods, includedEles.IncludedBeamSections, includedEles.IncludedSteelTypes);
        }

        public static Tuple<List<CuplockChromosome>, List<ChromosomeHistory>> DesignCuplockGenetic(GeneticDesignInput geneticInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeCuplock(geneticInput.IncludedElements.IncludedPlywoods.Count - 1, geneticInput.IncludedElements.IncludedBeamSections.Count - 1, geneticInput.IncludedElements.IncludedSteelTypes.Count - 1, geneticInput.IncludedElements.IncludedLedgers.Count - 1);

            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as CuplockChromosome;

                return dc.EvaluateFitnessCuplockDesign(slabThicknessCm, beamThicknessCm, beamWidthCm, geneticInput.IncludedElements.AsCuplockIncludedElements());
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            var hsitory = ga.StartGA();



            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<CuplockChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<CuplockChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return Tuple.Create(bestChromosomes, hsitory);
        }

        public static Tuple<List<CuplockChromosome>, List<ChromosomeHistory>> CostCuplockGenetic(GeneticDesignInput geneticInput, CostGeneticResultInput costInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeCuplock(geneticInput.IncludedElements.IncludedPlywoods.Count - 1, geneticInput.IncludedElements.IncludedBeamSections.Count - 1, geneticInput.IncludedElements.IncludedSteelTypes.Count - 1, geneticInput.IncludedElements.IncludedLedgers.Count - 1);

            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as CuplockChromosome;

                return dc.EvaluateFitnessCuplockCost(slabThicknessCm, beamThicknessCm, beamWidthCm, costInput, geneticInput.IncludedElements.AsCuplockIncludedElements());
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

           var history = ga.StartGA();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<CuplockChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<CuplockChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return Tuple.Create( bestChromosomes,history);
        }

        #endregion

        #region European Prop

        public static EuropeanPropsGeneticInludedElements AsEuroPropsIncludedElements(this GeneticIncludedElements includedElements)
        {
            return new EuropeanPropsGeneticInludedElements(includedElements.IncludedPlywoods, includedElements.IncludedBeamSections, includedElements.IncludedProps);
        }

        public static Tuple<List<EuropeanPropChromosome>, List<ChromosomeHistory>> DesignEurpopeanPropGenetic(GeneticDesignInput geneticInput)
        {
            var chromosome = ChromosomeHelper.GenerateChromosomeEuropeanProp(geneticInput.IncludedElements.IncludedPlywoods.Count - 1, geneticInput.IncludedElements.IncludedBeamSections.Count - 1, geneticInput.IncludedElements.IncludedProps.Count - 1);


            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as EuropeanPropChromosome;
                return dc.EvaluateFitnessEuropeanPropDesign(slabThicknessCm, beamThicknessCm, beamWidthCm, geneticInput.IncludedElements.AsEuroPropsIncludedElements());
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            var history = ga.StartGA();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<EuropeanPropChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<EuropeanPropChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return Tuple.Create(bestChromosomes, history);
        }

        public static Tuple<List<EuropeanPropChromosome>, List<ChromosomeHistory>> CostEurpopeanPropGenetic(GeneticDesignInput geneticInput, CostGeneticResultInput costInput)
        {
            var chromosome = ChromosomeHelper.GenerateChromosomeEuropeanProp(geneticInput.IncludedElements.IncludedPlywoods.Count - 1, geneticInput.IncludedElements.IncludedBeamSections.Count - 1, geneticInput.IncludedElements.IncludedProps.Count - 1);


            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as EuropeanPropChromosome;
                return dc.EvaluateFitnessEuropeanPropCost(slabThicknessCm, beamThicknessCm, beamWidthCm, costInput, geneticInput.IncludedElements.AsEuroPropsIncludedElements());
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            var history = ga.StartGA();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<EuropeanPropChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<EuropeanPropChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return Tuple.Create( bestChromosomes,history);
        }
        #endregion

        #region ShorBrace


        public static ShoreBraceGeneticIncludedElements AsShoreIncludedElements(this GeneticIncludedElements includedElements)
        {
            return new ShoreBraceGeneticIncludedElements(includedElements.IncludedPlywoods, includedElements.IncludedBeamSections, includedElements.IncludedShoreBracing);
        }

        public static Tuple<List<ShorBraceChromosome>, List<ChromosomeHistory>> DesignShorGenetic(GeneticDesignInput geneticInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeShorBrace(geneticInput.IncludedElements.IncludedPlywoods.Count - 1, geneticInput.IncludedElements.IncludedBeamSections.Count - 1, geneticInput.IncludedElements.IncludedShoreBracing.Count - 1);


            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as ShorBraceChromosome;
                return dc.EvaluateFitnessShorBraceDesign(slabThicknessCm, beamThicknessCm, beamWidthCm, geneticInput.IncludedElements.AsShoreIncludedElements());
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            var history = ga.StartGA();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<ShorBraceChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<ShorBraceChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();

            return Tuple.Create(bestChromosomes, history);
        }

        public static Tuple<List<ShorBraceChromosome>, List<ChromosomeHistory>> CostShorGenetic(GeneticDesignInput geneticInput, CostGeneticResultInput costInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeShorBrace(geneticInput.IncludedElements.IncludedPlywoods.Count - 1, geneticInput.IncludedElements.IncludedBeamSections.Count - 1, geneticInput.IncludedElements.IncludedShoreBracing.Count - 1);


            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as ShorBraceChromosome;
                return dc.EvaluateFitnessShorBraceCost(slabThicknessCm, beamThicknessCm, beamWidthCm, costInput, geneticInput.IncludedElements.AsShoreIncludedElements());
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            var hsitory = ga.StartGA();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<ShorBraceChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<ShorBraceChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();

            return Tuple.Create(bestChromosomes,hsitory);
        }
        #endregion

        #region Aluminum Prop


        public static AluPropsGeneticIncludedElements AsAluPropsIncludedElements(this GeneticIncludedElements includedElements)
        {
            return new AluPropsGeneticIncludedElements(includedElements.IncludedPlywoods, includedElements.IncludedBeamSections);
        }

        public static Tuple<List<AluminumPropChromosome>, List<ChromosomeHistory>> DesignAluminumPropGenetic(GeneticDesignInput geneticInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeAlumuinumProp(geneticInput.IncludedElements.IncludedPlywoods.Count - 1, geneticInput.IncludedElements.IncludedBeamSections.Count - 1);

            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as AluminumPropChromosome;

                return dc.EvaluateFitnessAluminumPropDesign(slabThicknessCm, beamThicknessCm, beamWidthCm, geneticInput.IncludedElements.AsAluPropsIncludedElements());
            });


            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            var history = ga.StartGA();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<AluminumPropChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<AluminumPropChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return Tuple.Create(bestChromosomes, history);
        }

        #endregion

        #region Frame

        public static FrameGeneticIncludedElements AsFrameIncludedElements(this GeneticIncludedElements includedElements)
        {
            return new FrameGeneticIncludedElements(includedElements.IncludedPlywoods, includedElements.IncludedBeamSections, includedElements.IncludedFrames, includedElements.IncludedShoreBracing);
        }

        public static Tuple<List<FrameChromosome>,List<ChromosomeHistory>> DesignFrameGenetic(GeneticDesignInput geneticInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeFrame(geneticInput.IncludedElements.IncludedPlywoods.Count - 1, geneticInput.IncludedElements.IncludedBeamSections.Count - 1, geneticInput.IncludedElements.IncludedFrames.Count - 1, geneticInput.IncludedElements.IncludedShoreBracing.Count - 1);

            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as FrameChromosome;

                return dc.EvaluateFitnessFrameDesign(slabThicknessCm, beamThicknessCm, beamWidthCm, geneticInput.IncludedElements.AsFrameIncludedElements());
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            var history = ga.StartGA();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<FrameChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<FrameChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return Tuple.Create(bestChromosomes, history);
        }
        #endregion
    }
}
