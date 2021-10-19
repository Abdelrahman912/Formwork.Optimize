using Autodesk.Revit.DB;
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
using FormworkOptimize.App.Utils;
using CSharp.Functional.Errors;
using FormworkOptimize.App.Models;
using static FormworkOptimize.Core.Comparers.Comparers;
using GeneticSharp.Domain.Chromosomes;
using FormworkOptimize.Core.Entities.Geometry;

namespace FormworkOptimize.App.ViewModels
{
    public class FrameSystemViewModel : SubstructureViewModel
    {

        #region Private Fields

        private FrameTypeName selectedFrameType;

        private double selectedSpacing;

        private FrameDesignInput designInput;

        private FrameDesigner designer;

        private FrameDesignOutput designOutput;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        // GAs
        private ObservableCollection<FrameChromosome> bestChromosomes;

        private FrameChromosome selectedChromosome;
        #endregion

        #region Properties

        public ObservableCollection<double> Spacings { get; private set; }

        public FrameTypeName SelectedFrameType
        {
            get => selectedFrameType;
            set => NotifyPropertyChanged(ref selectedFrameType, value);
        }

        public double SelectedSpacing
        {
            get => selectedSpacing;
            set => NotifyPropertyChanged(ref selectedSpacing, value);
        }

        public FrameDesignOutput DesignOutput
        {
            get => designOutput;
            set => NotifyPropertyChanged(ref designOutput, value);
        }

        // GAs
        public ObservableCollection<FrameChromosome> BestChromosomes
        {
            get => bestChromosomes;
            set => NotifyPropertyChanged(ref bestChromosomes, value);
        }

        public FrameChromosome SelectedChromosome
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

        public FrameSystemViewModel(Func<List<ResultMessage>, Unit> notificationService)
        {
            _notificationService = notificationService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());
            Spacings = new ObservableCollection<double>(Database.FrameSystemCrossBraces);
            SelectedSpacing = Spacings.First();
            BestChromosomes = new ObservableCollection<FrameChromosome>();
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
                designInput = new FrameDesignInput(
                    SuperstructureViewModel.SelectedPlywoodSection,
                    SuperstructureViewModel.SelectedSecondaryBeamSection,
                    SuperstructureViewModel.SelectedMainBeamSection,
                    SelectedSpacing,
                    SelectedFrameType,
                    SuperstructureViewModel.SelectedSecondaryBeamLength,
                    SuperstructureViewModel.SelectedMainBeamLength,
                    slabThicknessCm,
                   secBeamsSpacing);
            }
            else if (SuperstructureViewModel.SelectedElement.GetCategory() == BuiltInCategory.OST_StructuralFraming)
            {
                var elementType = RevitBase.Document.GetElement(SuperstructureViewModel.SelectedElement.GetTypeId()) as ElementType;

                double beamWidthCm = elementType.LookupParameter("b")
                                                .AsDouble()
                                                .FeetToCm();

                double beamThicknessCm = elementType.LookupParameter("h")
                                                    .AsDouble()
                                                    .FeetToCm();


                designInput = new FrameDesignInput(
                    SuperstructureViewModel.SelectedPlywoodSection,
                    SuperstructureViewModel.SelectedSecondaryBeamSection,
                    SuperstructureViewModel.SelectedMainBeamSection,
                    SelectedSpacing,
                    SelectedFrameType,
                    SuperstructureViewModel.SelectedSecondaryBeamLength,
                    SuperstructureViewModel.SelectedMainBeamLength,
                    20,
                    secBeamsSpacing,
                    beamThicknessCm,
                    beamWidthCm);
            }

            designer = FrameDesigner.Instance;//Single ton class.

            Func<FrameDesignOutput, Unit> updateOutput = output =>
             {
                 DesignOutput = output;

                 DesignResultViewModel = new DesignResultViewModel()
                 {
                     PlywoodDesignOutput = DesignOutput.Plywood.AsSelectedMaxDesignOutput(),
                     SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.SecondaryBeam.Item2.ToList()),
                     MainBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.MainBeam.Item2.ToList()),
                     ShoringSystemDesignOutput = new ShoringDesignOutput("Frame System", new List<DesignReport>() { DesignOutput.Shoring.Item2 })
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
                    && SelectedSpacing != 0.0;
        }

        #endregion

        #region GAs
        //protected override void DesignGenetic()
        //{
        //    //! Creating an chromosome
        //    var chromosome = ChromosomeHelper.GenerateChromosomeFrame();

        //    //! Creating the population
        //    var population = new Population(50, 100, chromosome);

        //    //! Creating the fitness function
        //    (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = SuperstructureViewModel.SelectedElement.GetFloorAndBeamDimensions();

        //    var fitness = new FuncFitness((c) =>
        //    {
        //        var dc = c as FrameChromosome;

        //        return dc.EvaluateFitnessFrameDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
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

        //    BestChromosomes = new ObservableCollection<FrameChromosome>(ga.Population.CurrentGeneration.Chromosomes.Cast<FrameChromosome>()
        //                                                              .Where(ChromosomeExtension.IsValid)
        //                                                              .Cast<BinaryChromosomeBase>()
        //                                                              .Distinct(Comparers.DesignChromosomeComparer)
        //                                                              .Cast<FrameChromosome>()
        //                                                              .OrderByDescending(c => c.Fitness)
        //                                                              .Take(5)
        //                                                              .ToList());

        //}

        //protected override bool CanDesignGenetic()
        //{
        //    return SuperstructureViewModel.SelectedElement != null;
        //}

        //protected override void OnDesign()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

    }
}
