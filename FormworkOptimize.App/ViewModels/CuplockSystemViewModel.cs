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
using FormworkOptimize.Core.Entities.Geometry;

namespace FormworkOptimize.App.ViewModels
{
    public class CuplockSystemViewModel : SubstructureViewModel
    {
        #region Private Fields

        private SteelType selectedSteelType;

        private double selectedLedgerMain;

        private double selectedLedgerSec;

        private CuplockDesignInput designInput;

        private CuplockDesigner designer;

        private CuplockDesignOutput designOutput;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        // GAs
        private ObservableCollection<CuplockChromosome> bestChromosomes;

        private CuplockChromosome selectedChromosome;
        #endregion

        #region Properties

        public ObservableCollection<double> LedgerMainDirSections { get; private set; }

        public ObservableCollection<double> LedgerSecDirSections { get; private set; }

        public SteelType SelectedSteelType
        {
            get => selectedSteelType;
            set => NotifyPropertyChanged(ref selectedSteelType, value);
        }

        public double SelectedLedgerMain
        {
            get => selectedLedgerMain;
            set => NotifyPropertyChanged(ref selectedLedgerMain, value);
        }

        public double SelectedLedgerSec
        {
            get => selectedLedgerSec;
            set => NotifyPropertyChanged(ref selectedLedgerSec, value);
        }


        public CuplockDesignOutput DesignOutput
        {
            get => designOutput;
            set => NotifyPropertyChanged(ref designOutput, value);
        }

        // GAs
        public ObservableCollection<CuplockChromosome> BestChromosomes
        {
            get => bestChromosomes;
            set => NotifyPropertyChanged(ref bestChromosomes, value);
        }

        public CuplockChromosome SelectedChromosome
        {
            get => selectedChromosome;
            set
            {
                NotifyPropertyChanged(ref selectedChromosome, value);

                //DesignResultViewModel.PlywoodDesignOutput = SelectedChromosome.PlywoodDesignOutput;
                //DesignResultViewModel.SecondaryBeamDesignOutput = SelectedChromosome.SecondaryBeamDesignOutput;
                //DesignResultViewModel.MainBeamDesignOutput = SelectedChromosome.MainBeamDesignOutput;
                //DesignResultViewModel.ShoringSystemDesignOutput = SelectedChromosome.ShoringSystemDesignOutput;
                //BestChromosomes = new ObservableCollection<CuplockChromosome>();
            }
        }
        #endregion

        #region Constructors

        public CuplockSystemViewModel(Func<List<ResultMessage>, Unit> notificationService)
        {
            _notificationService = notificationService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());
            LedgerMainDirSections = new ObservableCollection<double>(Database.LedgerLengths);
            LedgerSecDirSections = new ObservableCollection<double>(Database.LedgerLengths);
            SelectedLedgerMain = LedgerMainDirSections.First();
            SelectedLedgerSec = LedgerSecDirSections.First();
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
                designInput = new CuplockDesignInput(
                   SuperstructureViewModel.SelectedPlywoodSection,
                   SuperstructureViewModel.SelectedSecondaryBeamSection,
                    SuperstructureViewModel.SelectedMainBeamSection,
                    SelectedSteelType,
                   SelectedLedgerMain,
                   SelectedLedgerSec,
                   SuperstructureViewModel.SelectedSecondaryBeamLength,
                   SuperstructureViewModel.SelectedMainBeamLength,
                   20,
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

                designInput = new CuplockDesignInput(
                    SuperstructureViewModel.SelectedPlywoodSection,
                    SuperstructureViewModel.SelectedSecondaryBeamSection,
                    SuperstructureViewModel.SelectedMainBeamSection,
                    SelectedSteelType,
                    SelectedLedgerMain,
                    SelectedLedgerSec,
                    SuperstructureViewModel.SelectedSecondaryBeamLength,
                    SuperstructureViewModel.SelectedMainBeamLength,
                    20,
                    secBeamsSpacing,
                    beamThicknessCm,
                    beamWidthCm);
            }

            designer = CuplockDesigner.Instance;//Single ton class.

            Func<CuplockDesignOutput, Unit> updateOutput = output =>
             {
                 DesignOutput = output;

                 DesignResultViewModel = new DesignResultViewModel()
                 {
                     PlywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()},Selected Span: {designOutput.Plywood.Item2.Span} cm, ,Max Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item3.ToList()),
                     SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.SecondaryBeam.Item2.ToList()),
                     MainBeamDesignOutput = new SectionDesignOutput($"Section: {DesignOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", DesignOutput.MainBeam.Item2.ToList()),
                     ShoringSystemDesignOutput = new ShoringDesignOutput("Cuplock System", new List<DesignReport>() { DesignOutput.Shoring.Item2 })
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
                    && SelectedLedgerMain != 0.0
                    && SelectedLedgerSec != 0.0;
        }

        #endregion

        #region GAs

        //protected override void DesignGenetic()
        //{
        //    //! Creating an chromosome
        //    var chromosome = ChromosomeHelper.GenerateChromosomeCuplock();

        //    //! Creating the population
        //    var population = new Population(50, 100, chromosome);

        //    //! Creating the fitness function
        //    (var slabThicknessCm, var beamWidthCm, var beamThicknessCm) = SuperstructureViewModel.SelectedElement.GetFloorAndBeamDimensions();

        //    var fitness = new FuncFitness((c) =>
        //    {
        //        var dc = c as CuplockChromosome;

        //        return dc.EvaluateFitnessCuplockDesign(slabThicknessCm, beamThicknessCm, beamWidthCm);
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
           
        //    BestChromosomes = new ObservableCollection<CuplockChromosome>(ga.Population.CurrentGeneration.Chromosomes.Cast<CuplockChromosome>()
        //                                                              .Where(ChromosomeExtension.IsValid)
        //                                                              .Cast<BinaryChromosomeBase>()
        //                                                              .Distinct(Comparers.DesignChromosomeComparer)
        //                                                              .Cast<CuplockChromosome>()
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
        //    //var steelType = (SteelType)phenotype[3];

        //    //TaskDialog.Show("GAs Result",
        //    //           $"Plywood: {plywoodSection.ToString()}{Environment.NewLine}" +
        //    //           $"Secondary Beam: {secondaryBeamSection.ToString()}{Environment.NewLine}" +
        //    //           $"Main Beam: {mainBeamSection.ToString()}{Environment.NewLine}" +
        //    //           $"Steel Type: {steelType.ToString()}{Environment.NewLine}" +
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