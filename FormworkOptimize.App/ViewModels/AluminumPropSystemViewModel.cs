using Autodesk.Revit.DB;
using CSharp.Functional.Errors;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.Utils;
using FormworkOptimize.App.ViewModels.Base;
using Autodesk.Revit.UI;
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
using System.Text;
using System.Threading.Tasks;
using Unit = System.ValueTuple;
using static CSharp.Functional.Functional;
using static FormworkOptimize.Core.Comparers.Comparers;
using GeneticSharp.Domain.Chromosomes;
using FormworkOptimize.Core.Entities.Geometry;

namespace FormworkOptimize.App.ViewModels
{
    public class AluminumPropSystemViewModel : SubstructureViewModel
    {
        #region Private Fields

        private double mainSpacing;

        private double secSpacing;

        private AluPropDesignInput designInput;

        private AluPropDesigner designer;

        private PropDesignOutput designOutput;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        // GAs
        private ObservableCollection<AluminumPropChromosome> bestChromosomes;

        private AluminumPropChromosome selectedChromosome;
        #endregion

        #region Properties

        public ObservableCollection<double> MainSpacings { get; private set; }

        public ObservableCollection<double> SecSpacings { get; private set; }

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
        public ObservableCollection<AluminumPropChromosome> BestChromosomes
        {
            get => bestChromosomes;
            set => NotifyPropertyChanged(ref bestChromosomes, value);
        }

        public AluminumPropChromosome SelectedChromosome
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

        public AluminumPropSystemViewModel(Func<List<ResultMessage>, Unit> notificationService)
        {
            _notificationService = notificationService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());
            BestChromosomes = new ObservableCollection<AluminumPropChromosome>();
        }

        #endregion

        #region Methods
        protected override void DesignElement()
        {
            var secBeamsSpacing = SuperstructureViewModel.IsUserDefinedSecSpacing ? new UserDefinedSecondaryBeamSpacing(SuperstructureViewModel.SecondaryBeamSpacing)
                                                                                  : new AutomaticSecondaryBeamSpacing() as SecondaryBeamSpacing;
            if (SuperstructureViewModel.SelectedElement.GetCategory() == BuiltInCategory.OST_Floors)
            {
                double slabThicknessCm = SuperstructureViewModel.SelectedElement.get_Parameter(BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS)
                                                                                .AsDouble()
                                                                                .FeetToCm();

                designInput = new AluPropDesignInput(
                     SuperstructureViewModel.SelectedPlywoodSection,
                     SuperstructureViewModel.SelectedSecondaryBeamSection,
                    SuperstructureViewModel.SelectedMainBeamSection,
                    MainSpacing,
                    SecSpacing,
                    SuperstructureViewModel.SelectedSecondaryBeamLength,
                    SuperstructureViewModel.SelectedMainBeamLength,
                    slabThicknessCm,
                    secBeamsSpacing);
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

                designInput = new AluPropDesignInput(
                   SuperstructureViewModel.SelectedPlywoodSection,
                    SuperstructureViewModel.SelectedSecondaryBeamSection,
                   SuperstructureViewModel.SelectedMainBeamSection,
                   MainSpacing,
                   SecSpacing,
                   SuperstructureViewModel.SelectedSecondaryBeamLength,
                   SuperstructureViewModel.SelectedMainBeamLength,
                   20,
                   secBeamsSpacing,
                    beamThicknessCm,
                    beamWidthCm);
            }

            designer = AluPropDesigner.Instance;//Single ton class.

            Func<PropDesignOutput, Unit> updateOutput = output =>
              {
                  DesignOutput = output;

                  DesignResultViewModel = new DesignResultViewModel()
                  {
                      PlywoodDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.Plywood.Item1.Section.SectionName.GetDescription()},Selected Span: {DesignOutput.Plywood.Item2.Span} cm, Max Span: {DesignOutput.Plywood.Item1.Span} cm", DesignOutput.Plywood.Item3.ToList()),
                      SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.SecondaryBeam.Item2.ToList()),
                      MainBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.MainBeam.Item2.ToList()),
                      ShoringSystemDesignOutput = new ShoringDesignOutput("Aluminum Prop System", new List<DesignReport>() { DesignOutput.Shoring.Item2 })
                  };
                 return Unit();
              };

             designer.Design(designInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction)
                     .Match(_showErrors,updateOutput);
          
        }

        protected override bool CanDesign()
        {
            return SuperstructureViewModel.SelectedElement != null
                    && SuperstructureViewModel.SelectedSecondaryBeamLength != 0.0
                    && SuperstructureViewModel.SelectedMainBeamLength != 0.0
                    && MainSpacing != 0.0
                    && SecSpacing != 0.0;
        }

        //protected override void OnDesign()
        //{
        //    DesignElement();
        //    IsDesignResultViewVisible = true;
        //}

        #endregion

        #region GAs

        //protected override void DesignGenetic()
        //{
        //    //! Creating an chromosome
        //    var chromosome = ChromosomeHelper.GenerateChromosomeAlumuinumProp();

        //    //! Creating the population
        //    var population = new Population(50, 100, chromosome);

        //    //! Creating the fitness function
        //    (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = SuperstructureViewModel.SelectedElement.GetFloorAndBeamDimensions();

        //    var fitness = new FuncFitness((c) =>
        //    {
        //        var dc = c as AluminumPropChromosome;

        //        return dc.EvaluateFitnessAluminumPropDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
        //    });

        //    //! Creating the selection
        //    var selection = new EliteSelection();

        //    //! Creating the crossover
        //    var crossover = new UniformCrossover();

        //    //! Creating the mutation
        //    var mutation = new FlipBitMutation();

        //    //! Creating the termination
        //    var termination = new GenerationNumberTermination(10000);

        //    //! Running the GA
        //    var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
        //    ga.Termination = termination;

        //    ga.Start();

        //    BestChromosomes = new ObservableCollection<AluminumPropChromosome>(ga.Population.CurrentGeneration.Chromosomes.Cast<AluminumPropChromosome>()
        //                                                              .Where(ChromosomeExtension.IsValid)
        //                                                              .Cast<BinaryChromosomeBase>()
        //                                                              .Distinct(Comparers.DesignChromosomeComparer)
        //                                                              .Cast<AluminumPropChromosome>()
        //                                                              .OrderByDescending(c => c.Fitness)
        //                                                              .Take(5)
        //                                                              .ToList());
        //}

        //protected override bool CanDesignGenetic()
        //{
        //    return SuperstructureViewModel.SelectedElement != null;
        //}



        #endregion

    }
}
