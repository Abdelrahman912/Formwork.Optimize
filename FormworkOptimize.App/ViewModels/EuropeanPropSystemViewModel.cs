using Autodesk.Revit.DB;
using CSharp.Functional.Errors;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.Utils;
using FormworkOptimize.Core.Comparers;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS;
using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Entities.Genetic;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.DesignHelper;
using FormworkOptimize.Core.Helpers.GeneticHelper;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static CSharp.Functional.Functional;
using Unit = System.ValueTuple;
using static FormworkOptimize.Core.Comparers.Comparers;
using GeneticSharp.Domain.Chromosomes;

namespace FormworkOptimize.App.ViewModels
{
    public class EuropeanPropSystemViewModel : SubstructureViewModel
    {

        #region Private Fields

        private EuropeanPropTypeName selectedEuropeanPropType;

        private double mainSpacing;

        private double secSpacing;

        private EuropeanPropDesignInput designInput;

        private EuropeanPropDesigner designer;

        private PropDesignOutput designOutput;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        // GAs
        private ObservableCollection<EuropeanPropChromosome> bestChromosomes;

        private EuropeanPropChromosome selectedChromosome;
        #endregion

        #region Properties

        public EuropeanPropTypeName SelectedEuropeanPropType
        {
            get => selectedEuropeanPropType;
            set => NotifyPropertyChanged(ref selectedEuropeanPropType, value);
        }

        public double MainSpacing
        {
            get => mainSpacing;
            set => NotifyPropertyChanged(ref mainSpacing, value);
        }

        public double SecSpacing
        {
            get => secSpacing;
            set => NotifyPropertyChanged(ref secSpacing, value);
        }

        public PropDesignOutput DesignOutput
        {
            get => designOutput;
            set => NotifyPropertyChanged(ref designOutput, value);
        }

        // GAs
        public ObservableCollection<EuropeanPropChromosome> BestChromosomes
        {
            get => bestChromosomes;
            set => NotifyPropertyChanged(ref bestChromosomes, value);
        }

        public EuropeanPropChromosome SelectedChromosome
        {
            get => selectedChromosome;
            set
            {
                NotifyPropertyChanged(ref selectedChromosome, value);

                //DesignResultViewModel.PlywoodDesignOutput = SelectedChromosome.PlywoodDesignOutput;
                //DesignResultViewModel.SecondaryBeamDesignOutput = SelectedChromosome.SecondaryBeamDesignOutput;
                //DesignResultViewModel.MainBeamDesignOutput = SelectedChromosome.MainBeamDesignOutput;
                //DesignResultViewModel.ShoringSystemDesignOutput = SelectedChromosome.ShoringSystemDesignOutput;
            }
        }
        #endregion

        #region Constructors

        public EuropeanPropSystemViewModel(Func<List<ResultMessage>, Unit> notificationService)
        {
            _notificationService = notificationService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());
            BestChromosomes = new ObservableCollection<EuropeanPropChromosome>();
        }

        #endregion

        #region Methods
        protected override void DesignElement()
        {
            if (SuperstructureViewModel.SelectedElement.GetCategory() == BuiltInCategory.OST_Floors)
            {
                var slabThicknessCm = SuperstructureViewModel.SelectedElement.get_Parameter(BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS)
                                                                             .AsDouble()
                                                                             .FeetToCm();

                designInput = new EuropeanPropDesignInput(
                    SuperstructureViewModel.SelectedPlywoodSection,
                    SuperstructureViewModel.SelectedSecondaryBeamSection,
                    SuperstructureViewModel.SelectedMainBeamSection,
                    SelectedEuropeanPropType,
                    MainSpacing,
                    SecSpacing,
                    SuperstructureViewModel.SelectedSecondaryBeamLength,
                    SuperstructureViewModel.SelectedMainBeamLength,
                    slabThicknessCm);
            }
            else if (SuperstructureViewModel.SelectedElement.GetCategory() == BuiltInCategory.OST_StructuralFraming)
            {
                var elementType = RevitBase.Document.GetElement(SuperstructureViewModel.SelectedElement.GetTypeId()) as ElementType;

                var beamWidthCm = elementType.LookupParameter("b")
                                             .AsDouble()
                                             .FeetToCm();

                var beamThicknessCm = elementType.LookupParameter("h")
                                                 .AsDouble()
                                                 .FeetToCm();

                designInput = new EuropeanPropDesignInput(
                    SuperstructureViewModel.SelectedPlywoodSection,
                    SuperstructureViewModel.SelectedSecondaryBeamSection,
                    SuperstructureViewModel.SelectedMainBeamSection,
                    SelectedEuropeanPropType,
                    MainSpacing,
                    SecSpacing,
                    SuperstructureViewModel.SelectedSecondaryBeamLength,
                    SuperstructureViewModel.SelectedMainBeamLength,
                    20,
                    beamThicknessCm,
                    beamWidthCm);
            }

            designer = EuropeanPropDesigner.Instance;//Single ton class.

            Func<PropDesignOutput, Unit> updateOutput = output =>
             {
                 DesignOutput = output;

                 DesignResultViewModel = new DesignResultViewModel()
                 {
                     PlywoodDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {DesignOutput.Plywood.Item1.Span} cm", DesignOutput.Plywood.Item2.ToList()),
                     SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.SecondaryBeam.Item2.ToList()),
                     MainBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.MainBeam.Item2.ToList()),
                     ShoringSystemDesignOutput = new ShoringDesignOutput("European Prop System", new List<DesignReport>() { DesignOutput.Shoring.Item2 })
                 };
                 return Unit();
             };
            designer.Design(designInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction)
                    .Match(_showErrors, updateOutput);

        }

        protected override bool CanDesign()
        {
            return SuperstructureViewModel.SelectedElement != null
                    && SuperstructureViewModel.SelectedSecondaryBeamLength != 0.0
                    && SuperstructureViewModel.SelectedMainBeamLength != 0.0
                    && MainSpacing != 0.0
                    && SecSpacing != 0.0;
        }

        #endregion

        #region GAs

        protected override void DesignGenetic()
        {
            //! Creating an chromosome
            var chromosome = ChromosomeHelper.GenerateChromosomeEuropeanProp();

            //! Creating the population
            // A population that will have a minimum and a maximum number of 4 and used our chromosome template as the “Adam chromosome”
            var population = new Population(50, 100, chromosome);

            //! Creating the fitness function
            // Return the value as the fitness value of the current chromosome
            (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = SuperstructureViewModel.SelectedElement.GetFloorAndBeamDimensions();

            var fitness = new FuncFitness((c) =>
        {
            var dc = c as EuropeanPropChromosome;
            return dc.EvaluateFitnessEuropeanPropDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
        });

            //! Creating the selection
            // You can use the already implemented classic selections: Elite, Roulete Wheel, Stochastic Universal Sampling and Tournament.
            var selection = new EliteSelection();

            //! Creating the crossover
            // You can use one of already available: Cut and Splice, Cycle (CX), One-Point (C1), Order-based (OX2), Ordered (OX1), Partially Mapped (PMX), Position-based (POS), Three parent, Two-Point (C2) and Uniform
            var crossover = new UniformCrossover();

            //! Creating the mutation
            // You can use some from the GeneticSharp menu: Flip-bit, Reverse Sequence (RSM), Twors and Uniform.
            // Flip-bit mutation is a mutation specific to chromosomes that implement IBinaryChromosome interface, as our FloatingPointChromosome does. It will randomly chose a gene and flip it bit, so a gene with value 0 will turn to 1 and vice-versa.
            var mutation = new FlipBitMutation();

            //! Creating the termination
            // For most of cases you just need to use some of the availables terminations: Generation number, Time evolving, Fitness stagnation, Fitness threshold, And e Or (allows combine others terminations).
            //var termination = new TimeEvolvingTermination();
            var termination = new GenerationNumberTermination(10000);

            //! Running the GA
            // Now that everything is set up, we just need to instantiate and start our genetic algorithm and watch it run.
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            ga.Termination = termination;

            // Better way to monitor the current best chromosome is use the GeneticAlgorithm.GenerationRan event. This event is raised right after a generation finish to run. Using this event you can see in realtime how the genetic algorithm is evolving.

            ga.Start();

            BestChromosomes = new ObservableCollection<EuropeanPropChromosome>(ga.Population.CurrentGeneration.Chromosomes.Cast<EuropeanPropChromosome>()
                                                                      .Where(ChromosomeExtension.IsValid)
                                                                      .Cast<BinaryChromosomeBase>()
                                                                      .Distinct(Comparers.DesignChromosomeComparer)
                                                                      .Cast<EuropeanPropChromosome>()
                                                                      .OrderByDescending(c => c.Fitness)
                                                                      .Take(5)
                                                                      .ToList());

            #region comments
            //var bestChromosome = ga.BestChromosome as DesignChromosome;

            //var bestFitness = bestChromosome.Fitness.Value;

            //var phenotype = bestChromosome.ToFloatingPoints();

            //var plywoodSection = (PlywoodSectionName)phenotype[0];
            //var secondaryBeamSection = (BeamSectionName)phenotype[1];
            //var mainBeamSection = (BeamSectionName)phenotype[2];
            //var propType = (EuropeanPropTypeName)phenotype[3];

            //TaskDialog.Show("GAs Result",
            //               $"Plywood: {plywoodSection.ToString()}{Environment.NewLine}" +
            //               $"Secondary Beam: {secondaryBeamSection.ToString()}{Environment.NewLine}" +
            //               $"Main Beam: {mainBeamSection.ToString()}{Environment.NewLine}" +
            //               $"Prop Type: {propType.ToString()}{Environment.NewLine}" +
            //               $"No. of Generations: {ga.GenerationsNumber}");
            #endregion
        }

        protected override bool CanDesignGenetic()
        {
            return SuperstructureViewModel.SelectedElement != null;
        }

        //protected override void OnDesign()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}
