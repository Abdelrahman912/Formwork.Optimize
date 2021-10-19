using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FormworkOptimize.Core.Comparers;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS;
using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Entities.Designer;
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
    public class ShoreBraceSystemViewModel : SubstructureViewModel
    {

        #region Private Fields

        private double selectedSpacing;

        private ShoreBraceDesignInput designInput;

        private ShoreBraceDesigner designer;

        private FrameDesignOutput designOutput;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        // GAs
        private ObservableCollection<ShorBraceChromosome> bestChromosomes;

        private ShorBraceChromosome selectedChromosome;
        #endregion

        #region Properties

        public ObservableCollection<double> Spacings { get; private set; }

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
        public ObservableCollection<ShorBraceChromosome> BestChromosomes
        {
            get => bestChromosomes;
            set => NotifyPropertyChanged(ref bestChromosomes, value);
        }

        public ShorBraceChromosome SelectedChromosome
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

        public ShoreBraceSystemViewModel(Func<List<ResultMessage>, Unit> notificationService)
        {
            _notificationService = notificationService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());
            Spacings = new ObservableCollection<double>(Database.ShoreBraceSystemCrossBraces);
            SelectedSpacing = Spacings.First();
            BestChromosomes = new ObservableCollection<ShorBraceChromosome>();
        }

        #endregion

        #region Methods

        protected override void DesignElement()
        {
            var secBeamsSpacing = SuperstructureViewModel.IsUserDefinedSecSpacing ? new UserDefinedSecondaryBeamSpacing(SuperstructureViewModel.SecondaryBeamSpacing)
                                                                                  : new AutomaticSecondaryBeamSpacing() as SecondaryBeamSpacing;
            if (SuperstructureViewModel.SelectedElement.GetCategory() == BuiltInCategory.OST_Floors)
            {
                var slabThicknessCm = SuperstructureViewModel.SelectedElement.get_Parameter(BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS)
                                                                             .AsDouble()
                                                                             .FeetToCm();

                designInput = new ShoreBraceDesignInput(
                    SuperstructureViewModel.SelectedPlywoodSection,
                    SuperstructureViewModel.SelectedSecondaryBeamSection,
                    SuperstructureViewModel.SelectedMainBeamSection,
                    SelectedSpacing,
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

                double beamThicknessCm = elementType.LookupParameter("h")
                                                    .AsDouble()
                                                    .FeetToCm();

                designInput = new ShoreBraceDesignInput(
                    SuperstructureViewModel.SelectedPlywoodSection,
                    SuperstructureViewModel.SelectedSecondaryBeamSection,
                    SuperstructureViewModel.SelectedMainBeamSection,
                    SelectedSpacing,
                    SuperstructureViewModel.SelectedSecondaryBeamLength,
                    SuperstructureViewModel.SelectedMainBeamLength,
                    20,
                    secBeamsSpacing,
                    beamThicknessCm,
                    beamWidthCm);
            }

            designer = ShoreBraceDesigner.Instance;//Single ton class.

            Func<FrameDesignOutput, Unit> updateOutput = output =>
             {
                 DesignOutput = output;

                 DesignResultViewModel = new DesignResultViewModel()
                 {
                     PlywoodDesignOutput = DesignOutput.Plywood.AsSelectedMaxDesignOutput(),
                     SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.SecondaryBeam.Item2.ToList()),
                     MainBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.MainBeam.Item2.ToList()),
                     ShoringSystemDesignOutput = new ShoringDesignOutput("Shore Brace System", new List<DesignReport>() { DesignOutput.Shoring.Item2 })
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
                    && SelectedSpacing != 0.0;
        }

        #endregion

        #region GAs
        //protected override void DesignGenetic()
        //{
        //    //! Creating an chromosome
        //    var chromosome = ChromosomeHelper.GenerateChromosomeShorBrace();

        //    //! Creating the population
        //    var population = new Population(50, 100, chromosome);

        //    //! Creating the fitness function
        //    (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = SuperstructureViewModel.SelectedElement.GetFloorAndBeamDimensions();

        //    var fitness = new FuncFitness((c) =>
        //    {
        //        var dc = c as ShorBraceChromosome;
        //        return dc.EvaluateFitnessShorBraceDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
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

        //    BestChromosomes = new ObservableCollection<ShorBraceChromosome>(ga.Population.CurrentGeneration.Chromosomes.Cast<ShorBraceChromosome>()
        //                                                              .Where(ChromosomeExtension.IsValid)
        //                                                              .Cast<BinaryChromosomeBase>()
        //                                                              .Distinct(Comparers.DesignChromosomeComparer)
        //                                                              .Cast<ShorBraceChromosome>()
        //                                                              .OrderByDescending(c => c.Fitness)
        //                                                              .Take(5)
        //                                                              .ToList());

        //    #region comments
        //    //var bestChromosome = ga.BestChromosome as DesignChromosome;

        //    //var bestFitness = bestChromosome.Fitness.Value;

        //    //var phenotype = bestChromosome.ToFloatingPoints();

        //    //var plywoodSection = (PlywoodSectionName)phenotype[0];
        //    //var secondaryBeamSection = (BeamSectionName)phenotype[1];
        //    //var mainBeamSection = (BeamSectionName)phenotype[2];

        //    //TaskDialog.Show("GAs Result",
        //    //           $"Plywood: {plywoodSection.ToString()}{Environment.NewLine}" +
        //    //           $"Secondary Beam: {secondaryBeamSection.ToString()}{Environment.NewLine}" +
        //    //           $"Main Beam: {mainBeamSection.ToString()}{Environment.NewLine}" +
        //    //           $"No. of Generations: {ga.GenerationsNumber}");
        //    #endregion
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
