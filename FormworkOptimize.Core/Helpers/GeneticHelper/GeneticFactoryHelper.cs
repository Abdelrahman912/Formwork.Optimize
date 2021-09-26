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
using static FormworkOptimize.Core.Comparers.Comparers;

namespace FormworkOptimize.Core.Helpers.GeneticHelper
{
    public static class GeneticFactoryHelper
    {
        #region General
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
            return ga;
        }
        #endregion

        #region Cuplock
        public static List<CuplockChromosome> DesignCuplockGenetic(GeneticDesignInput geneticInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeCuplock();

            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as CuplockChromosome;

                return dc.EvaluateFitnessCuplockDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
            });

            var ga = CreateGenetic(geneticInput,chromosome,fitness);

            ga.Start();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<CuplockChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<CuplockChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return bestChromosomes;
        }

        public static List<CuplockChromosome> CostCuplockGenetic(GeneticDesignInput geneticInput, CostGeneticResultInput costInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeCuplock();

            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as CuplockChromosome;

                return dc.EvaluateFitnessCuplockCost(slabThicknessCm, beamThicknessCm, beamWidthCm, costInput);
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            ga.Start();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<CuplockChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<CuplockChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return bestChromosomes;
        }
        #endregion

        #region European Prop
        public static  List<EuropeanPropChromosome> DesignEurpopeanPropGenetic(GeneticDesignInput geneticInput)
        {
            var chromosome = ChromosomeHelper.GenerateChromosomeEuropeanProp();

           
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as EuropeanPropChromosome;
                return dc.EvaluateFitnessEuropeanPropDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
            });

            var ga = CreateGenetic(geneticInput,chromosome,fitness);

            ga.Start();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<EuropeanPropChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<EuropeanPropChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return bestChromosomes;
        }
        public static List<EuropeanPropChromosome> CostEurpopeanPropGenetic(GeneticDesignInput geneticInput, CostGeneticResultInput costInput)
        {
            var chromosome = ChromosomeHelper.GenerateChromosomeEuropeanProp();


            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as EuropeanPropChromosome;
                return dc.EvaluateFitnessEuropeanPropCost(slabThicknessCm, beamThicknessCm, beamWidthCm, costInput);
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            ga.Start();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<EuropeanPropChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<EuropeanPropChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return bestChromosomes;
        }
        #endregion

        #region ShorBrace
        public static List<ShorBraceChromosome> DesignShorGenetic(GeneticDesignInput geneticInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeShorBrace();


            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as ShorBraceChromosome;
                return dc.EvaluateFitnessShorBraceDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            ga.Start();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<ShorBraceChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<ShorBraceChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();

            return bestChromosomes;
        }
        public static List<ShorBraceChromosome> CostShorGenetic(GeneticDesignInput geneticInput, CostGeneticResultInput costInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeShorBrace();


            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as ShorBraceChromosome;
                return dc.EvaluateFitnessShorBraceCost(slabThicknessCm, beamThicknessCm, beamWidthCm, costInput);
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            ga.Start();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<ShorBraceChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<ShorBraceChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();

            return bestChromosomes;
        }
        #endregion

        #region Aluminum Prop
        public static List<AluminumPropChromosome> DesignAluminumPropGenetic(GeneticDesignInput geneticInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeAlumuinumProp();

            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as AluminumPropChromosome;

                return dc.EvaluateFitnessAluminumPropDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
            });


            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            ga.Start();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<AluminumPropChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<AluminumPropChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return bestChromosomes;
        }
        #endregion

        #region Frame
        public static List<FrameChromosome> DesignFrameGenetic(GeneticDesignInput geneticInput)
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeFrame();

            //! Creating the fitness function
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = geneticInput.SupportedFloor.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
            {
                var dc = c as FrameChromosome;

                return dc.EvaluateFitnessFrameDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
            });

            var ga = CreateGenetic(geneticInput, chromosome, fitness);

            ga.Start();

            var bestChromosomes = ga.Population.CurrentGeneration.Chromosomes.Cast<FrameChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Distinct(DesignChromosomeComparer)
                                                                      .Cast<FrameChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList();
            return bestChromosomes;
        }
        #endregion
    }
}
