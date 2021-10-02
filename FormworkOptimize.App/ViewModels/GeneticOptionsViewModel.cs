using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CSharp.Functional.Constructs;
using CSharp.Functional.Errors;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.RevitExternalEvents;
using FormworkOptimize.App.Utils;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.DTOS.Genetic;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.CostParameters;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.GeneticParameters;
using FormworkOptimize.Core.Entities.GeneticResult;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.CostHelper;
using FormworkOptimize.Core.Helpers.GeneticHelper;
using FormworkOptimize.Core.Helpers.RevitHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Unit = System.ValueTuple;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticOptionsViewModel : ViewModelBase
    {

        #region Private Fields

        private readonly Dictionary<GeneticResultKey, List<NoCostGeneticResult>> _resultCash;

        private readonly UIDocument _uiDoc;

        private readonly Document _doc;

        private OptimizeOption _selectedOptimizeOption;

        private readonly List<FormworkSystem> _designSystems;

        private readonly List<FormworkSystem> _costSystems;

        private List<FormworkSystem> _systems;

        private FormworkSystem _selectedSystem;

        private int _noGenerations;

        private int _noPopulation;

        private bool _isGeneticResultsVisible;

        private List<NoCostGeneticResult> _geneticResults;

        private NoCostGeneticResult _selectedGeneticResult;

        private Floor _selectedHostFloor;

        private Floor _selectedSupportedFloor;

        private bool _isLoading;

        private double _boundaryLinesOffset;

        private double _beamsOffset;

        private bool _isCostVisible;

        private readonly string _costFilePath;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        private readonly Func<double, double, Validation<CostParameter>> _costParameterService;

        private readonly Func<Validation<GeneticIncludedElements>> _includedElementsService;

        private readonly Func<Func<string, Task<List<Exceptional<string>>>>, Option<Task<List<Exceptional<string>>>>> _folderDialogService;

        #endregion

        #region Properties

        public bool IsCostVisible
        {
            get => _isCostVisible;
            set => NotifyPropertyChanged(ref _isCostVisible, value);
        }

        public double BoundaryLinesOffset
        {
            get => _boundaryLinesOffset;
            set => NotifyPropertyChanged(ref _boundaryLinesOffset, value);
        }

        public double BeamsOffset
        {
            get => _beamsOffset;
            set => NotifyPropertyChanged(ref _beamsOffset, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => NotifyPropertyChanged(ref _isLoading, value);
        }

        public ICommand GeneticCommand { get; }

        public ICommand ShowDetailResultCommand { get; }

        public ICommand DrawCommand { get; }

        public ICommand ExportCommand { get; }

        public OptimizeOption SelectedOptimizeOption
        {
            get => _selectedOptimizeOption;
            set
            {
                var isChanged = NotifyPropertyChanged(ref _selectedOptimizeOption, value);
                if (isChanged)
                {
                    SelectedSystem = FormworkSystem.CUPLOCK_SYSTEM;
                    var key = new GeneticResultKey(_selectedOptimizeOption, SelectedSystem);
                    List<NoCostGeneticResult> geneticResults = null;
                    var isExist = _resultCash.TryGetValue(key, out geneticResults);
                    if (isExist)
                    {
                        IsGeneticResultsVisible = true;
                        GeneticResults = geneticResults;
                    }
                    else
                        IsGeneticResultsVisible = false;
                }
                switch (_selectedOptimizeOption)
                {
                    case OptimizeOption.DESIGN:
                        Systems = _designSystems;
                        break;
                    case OptimizeOption.COST:
                        Systems = _costSystems;
                        break;
                }
            }
        }

        public List<FormworkSystem> Systems
        {
            get => _systems;
            set => NotifyPropertyChanged(ref _systems, value);
        }

        public FormworkSystem SelectedSystem
        {
            get => _selectedSystem;
            set
            {
                var result = NotifyPropertyChanged(ref _selectedSystem, value);
                if (result)
                {
                    var key = new GeneticResultKey(SelectedOptimizeOption, _selectedSystem);
                    List<NoCostGeneticResult> geneticResults = null;
                    var isExist = _resultCash.TryGetValue(key, out geneticResults);
                    if (isExist)
                    {
                        IsGeneticResultsVisible = true;
                        GeneticResults = geneticResults;
                    }
                    else
                        IsGeneticResultsVisible = false;
                }
                switch (_selectedSystem)
                {
                    case FormworkSystem.CUPLOCK_SYSTEM:
                        IsCostVisible = true;
                        break;
                    case FormworkSystem.EUROPEAN_PROPS_SYSTEM:
                        IsCostVisible = true;
                        break;
                    case FormworkSystem.SHORE_SYSTEM:
                        IsCostVisible = true;
                        break;
                    case FormworkSystem.FRAME_SYSTEM:
                        IsCostVisible = false;
                        break;
                    case FormworkSystem.ALUMINUM_PROPS_SYSTEM:
                        IsCostVisible = false;
                        break;
                }
            }
        }

        public int NoGenerations
        {
            get => _noGenerations;
            set => NotifyPropertyChanged(ref _noGenerations, value);
        }

        public int NoPopulation
        {
            get => _noPopulation;
            set => NotifyPropertyChanged(ref _noPopulation, value);
        }

        public bool IsGeneticResultsVisible
        {
            get => _isGeneticResultsVisible;
            set => NotifyPropertyChanged(ref _isGeneticResultsVisible, value);
        }

        public List<NoCostGeneticResult> GeneticResults
        {
            get => _geneticResults;
            set => NotifyPropertyChanged(ref _geneticResults, value);
        }

        public NoCostGeneticResult SelectedGeneticResult
        {
            get => _selectedGeneticResult;
            set => NotifyPropertyChanged(ref _selectedGeneticResult, value);
        }

        #endregion

        #region Constructors

        public GeneticOptionsViewModel(UIDocument uiDoc,
                                       Func<List<ResultMessage>, Unit> notificationService,
                                         Func<double, double, Validation<CostParameter>> costParameterService,
                                         Func<Validation<GeneticIncludedElements>> includedElementsService,
                                         Func<Func<string, Task<List<Exceptional<string>>>>, Option<Task<List<Exceptional<string>>>>> folderDialogService)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _costParameterService = costParameterService;
            _notificationService = notificationService;
            _includedElementsService = includedElementsService;
            _folderDialogService = folderDialogService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());
            _resultCash = new Dictionary<GeneticResultKey, List<NoCostGeneticResult>>();
            _designSystems = new List<FormworkSystem>()
            {
                FormworkSystem.CUPLOCK_SYSTEM,
                FormworkSystem.EUROPEAN_PROPS_SYSTEM,
                FormworkSystem.SHORE_SYSTEM,
                FormworkSystem.FRAME_SYSTEM,
                FormworkSystem.ALUMINUM_PROPS_SYSTEM
            };
            _costSystems = new List<FormworkSystem>()
            {
                FormworkSystem.CUPLOCK_SYSTEM,
                FormworkSystem.EUROPEAN_PROPS_SYSTEM,
                FormworkSystem.SHORE_SYSTEM,
            };
            SelectedOptimizeOption = OptimizeOption.DESIGN;
            SelectedSystem = FormworkSystem.CUPLOCK_SYSTEM;
            NoGenerations = 1000;
            NoPopulation = 50;
            GeneticCommand = new RelayCommand(OnGenetic, CanGenetic);
            ShowDetailResultCommand = new RelayCommand(OnShowDetailResult, CanShowDetailResult);
            DrawCommand = new RelayCommand(OnDraw, CanDraw);
            ExportCommand = new RelayCommand(OnExport, CanExport);
            IsGeneticResultsVisible = false;
            Mediator.Instance.Subscribe<Floor>(this, (hostFloor) => _selectedHostFloor = hostFloor, Context.HOST_FLOOR);
            Mediator.Instance.Subscribe<Floor>(this, (supportedFloor) => _selectedSupportedFloor = supportedFloor, Context.SUPPORTED_FLOOR);
            IsLoading = false;
            BoundaryLinesOffset = 0;//cm
            BeamsOffset = 50;//cm
            _costFilePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Cost Database\Formwork Elements Cost.csv";
        }




        #endregion

        #region Methods

        private bool CanExport() =>
            SelectedGeneticResult != null;


        private void OnExport()
        {
            Func<string, Func<string, double>, Task<List<Exceptional<string>>>> exportFunc = async (dir, costFunc) =>
              {
                  var tasks = await Task.Run(() =>
                  {
                      var newDir = $"{dir}\\{_doc.Title} - Gentic Result - {SelectedGeneticResult.Rank}";
                      var dirInfo = Directory.CreateDirectory(newDir);
                      var tsks = SelectedGeneticResult.DetailResults.Select(r => Tuple.Create(r.Name, r.AsReport()))
                                   .AsParallel()
                                   .Select(os => os.Item2.WriteAsCsv(newDir, $"{_doc.Title} - {os.Item1}"))
                                   .ToList();
                      var task = (SelectedGeneticResult as CostGeneticResult).SystemModel.EvaluateCost(costFunc)
                                                                              .WriteAsCsv(newDir, $"{_doc.Title} - Quantification");
                      tsks.Add(task);
                      return tsks;
                  });
                  var results = await Task.WhenAll(tasks);
                  return results.ToList();
              };

            Func<Task<List<Exceptional<string>>>, Task> showResult = async (task) =>
            {
                var results = await task;
                var messages = results.Select(fileName => fileName.ToResult())
                                     .ToList();
                _notificationService(messages);
            };

            GetCostFunc().Map(costfunc => _folderDialogService(dir => exportFunc(dir, costfunc)).Map(showResult));

        }

        private bool CanDraw() =>
            SelectedGeneticResult != null;

        private void OnDraw()
        {
            var costGenetic = SelectedGeneticResult as CostGeneticResult;
            if (costGenetic != null)
            {
                Action draw = () =>
                {
                    _doc.UsingTransactionGroup(transGroup =>
                    {
                        costGenetic.SystemModel.Draw(_doc).Match(_showErrors, unit => unit);
                        costGenetic.Plywood.Draw(_doc).Match(_showErrors, unit => unit);
                    }, "Model shoring");
                };
                ExternalEventHandler.Instance.Raise(_ => draw(),
                                                    () => { },
                                                     err => TaskDialog.Show("Error", err));
            }
        }

        private bool CanShowDetailResult() =>
            _selectedGeneticResult != null;

        private void OnShowDetailResult()
        {
            var detailResult = new GeneticDetailResultViewModel(SelectedGeneticResult.DetailResults);
            Mediator.Instance.NotifyColleagues(detailResult, Context.GENETIC_DETAIL_RESULT);
        }

        private bool CanGenetic() =>
            _selectedSupportedFloor != null &&
            _selectedHostFloor != null &&
            NoGenerations >= 10 &&
            NoPopulation >= 10 &&
            BoundaryLinesOffset >= 0 &&
            BeamsOffset >= 0;

        private void OnGenetic()
        {

            Action<IEnumerable<Error>> invalid = (errs) =>
            {
                IsLoading = false;
                _showErrors(errs);
            };
            Action<Task<List<NoCostGeneticResult>>> valid = async (results) =>
            {
                var key = new GeneticResultKey(SelectedOptimizeOption, SelectedSystem);
                GeneticResults = await results;
                List<NoCostGeneticResult> value = null;
                var result = _resultCash.TryGetValue(key, out value);
                if (result)
                {
                    _resultCash.Remove(key);
                    _resultCash.Add(key, GeneticResults);
                }
                else
                {
                    _resultCash.Add(key, GeneticResults);
                }
                IsLoading = false;
                IsGeneticResultsVisible = true;
            };

            IsLoading = true;

            if (SelectedOptimizeOption == OptimizeOption.DESIGN)
                GetDesignGeneticResults().Match(invalid.ToFunc(), valid.ToFunc());
            else
                GetCostGeneticResults().Match(invalid.ToFunc(), valid.ToFunc());
        }

        private Validation<Func<string, double>> GetCostFunc()
        {
            Func<FormworkElementCost, FormworkElementCost> updateCost = (old) => {
                return new FormworkElementCost()
                {
                    Name = old.Name,
                    UnitCost = old.UnitCost,
                    Price = old.Price / NO_DAYS_PER_MONTH
                };
            };
            return _costFilePath.ReadAsCsv<FormworkElementCost>()
                          .Map(db=>db.Select(updateCost))
                          .Map(db =>
                          {
                              Func<string, double> costFunc = name =>
                               {
                                   var results = db.Where(ele => ele.Name == name);
                                   if (results.Count() > 0)
                                       return results.First().Price;
                                   else
                                       return 0.0; //TODO: Add Option Construct.
                               };
                              return costFunc;
                          });
        }

        private Validation<CostGeneticResultInput> GetCostResultInput()
        {



            Func<View3D, Validation<CostGeneticResultInput>> getCostInput = (view) =>
            {
                var options = view.Options();
                var revitFloor = _selectedSupportedFloor.GetGeometry(options);
                var hostLevel = _doc.GetElement(_selectedHostFloor.LevelId) as Level;
                var hostFloorOffset = _selectedSupportedFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                var columns = _doc.GetColumnsInLevelWithinPolygon(hostLevel, hostFloorOffset, revitFloor.Boundary)
                                  .Select(col => Tuple.Create(0.0, col))
                                  .ToList();
                var revitInput = _doc.GetRevitFloorInput(revitFloor,
                                                         columns,
                                                         _selectedHostFloor,
                                                         _selectedSupportedFloor,
                                                         XYZ.BasisX);

                var floorArea = _selectedSupportedFloor.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED)
                                                       .AsDouble()
                                                       .FeetSquareToMeterSquare();
                var smallerLength = columns.Select(tuple => tuple.Item2)
                                           .Where(e => e.IsRectColumn(_doc))
                                           .Select(e => e.ToConcreteColumn(_doc))
                                           .GetSmallerLength()
                                           .FeetToMeter();

                var defaultPlywood = DeckingHelper.GetPlywood(revitFloor, _selectedHostFloor, _selectedSupportedFloor, PlywoodSectionName.BETOFILM_18MM, _doc, options);
                Func<PlywoodSectionName, RevitFloorPlywood> asFloorPlywood = (plywood) =>
                      new RevitFloorPlywood(plywood, new RevitConcreteFloor(defaultPlywood.Boundary, defaultPlywood.ConcreteFloorOpenings, defaultPlywood.ConcreteFloorThickness), defaultPlywood.HostLevel, defaultPlywood.OffsetFromLevel, defaultPlywood.PlywoodOpenings);


                Func<CostParameter, Validation<CostGeneticResultInput>> asCostInput = costParameters =>
                {
                    var timeLine = costParameters.Time.AsTimeLine();
                    var manPowerCost = costParameters.ManPower.AsManPowerCost(timeLine);
                    var equipmentsCost = costParameters.Equipments.AsEquipmentsCost(timeLine);
                    var transportationCost = costParameters.Transportation.AsTransportationCost();


                    return GetCostFunc().Map(func => new CostGeneticResultInput(func,
                                                                                revitInput,
                                                                                BoundaryLinesOffset,
                                                                                BeamsOffset,
                                                                                timeLine,
                                                                                manPowerCost,
                                                                                equipmentsCost,
                                                                                transportationCost,
                                                                                floorArea,
                                                                                asFloorPlywood));
                };

                return _costParameterService(floorArea, smallerLength).Bind(asCostInput);
            };
            return _doc.GetDefault3DView()
                       .Bind(getCostInput);
        }

        public Validation<Task<List<NoCostGeneticResult>>> GetDesignGeneticResults()
        {
            Func<CostGeneticResultInput, GeneticIncludedElements, Task<List<NoCostGeneticResult>>> getResults = (costInput, includedElements) =>
              {
                  return Task.Run(() =>
                  {
                      var newCostInput = costInput.UpdateCostInputWithNewRevitInput(costInput.RevitInput.UpdateWithNewXYZ(costInput.RevitInput.MainBeamDir.CrossProduct(XYZ.BasisZ)));
                      var costInputs = new List<CostGeneticResultInput>() { costInput, newCostInput };
                      var geneticInput = new GeneticDesignInput(_selectedSupportedFloor, NoGenerations, NoPopulation, includedElements.IncludedPlywoods, includedElements.IncludedBeamSections);
                      switch (SelectedSystem)
                      {
                          case FormworkSystem.CUPLOCK_SYSTEM:
                              return GeneticFactoryHelper.DesignCuplockGenetic(geneticInput)
                                                                   .AsParallel()
                                                                   .Select((chm, i) => costInputs.Select(input => chm.AsGeneticResult(input, i + 1)).OrderBy(resul => resul.Cost).First())
                                                                   .Cast<NoCostGeneticResult>()
                                                                   .ToList();
                          case FormworkSystem.EUROPEAN_PROPS_SYSTEM:
                              return GeneticFactoryHelper.DesignEurpopeanPropGenetic(geneticInput)
                                                                  .AsParallel()
                                                                  .Select((chm, i) => costInputs.Select(input => chm.AsGeneticResult(input, i + 1)).OrderBy(resul => resul.Cost).First())
                                                                  .Cast<NoCostGeneticResult>()
                                                                  .ToList();
                          case FormworkSystem.SHORE_SYSTEM:
                              return GeneticFactoryHelper.DesignShorGenetic(geneticInput)
                                                                  .AsParallel()
                                                                  .Select((chm, i) => costInputs.Select(input => chm.AsGeneticResult(input, i + 1)).OrderBy(resul => resul.Cost).First())
                                                                  .Cast<NoCostGeneticResult>()
                                                                  .ToList();
                          case FormworkSystem.FRAME_SYSTEM:
                              return GeneticFactoryHelper.DesignFrameGenetic(geneticInput)
                                                                  .AsParallel()
                                                                  .Select((chm, i) => chm.AsGeneticResult(i + 1))
                                                                  .ToList();
                          case FormworkSystem.ALUMINUM_PROPS_SYSTEM:
                              return GeneticFactoryHelper.DesignAluminumPropGenetic(geneticInput)
                                                                  .AsParallel()
                                                                  .Select((chm, i) => chm.AsGeneticResult(i + 1))
                                                                  .ToList();
                          default:
                              return new List<NoCostGeneticResult>();
                      }
                  });
              };

            if (SelectedSystem == FormworkSystem.CUPLOCK_SYSTEM || SelectedSystem == FormworkSystem.EUROPEAN_PROPS_SYSTEM || SelectedSystem == FormworkSystem.SHORE_SYSTEM)
            {
                return from includedEles in _includedElementsService()
                       from costInput in GetCostResultInput()
                       select getResults(costInput, includedEles);
            }
            else
            {
                return _includedElementsService().Map(includedEles => getResults(null, includedEles));
            }
        }

        public Validation<Task<List<NoCostGeneticResult>>> GetCostGeneticResults()
        {
            Func<CostGeneticResultInput, GeneticIncludedElements, Task<List<NoCostGeneticResult>>> getResults = (costInput, includedElements) =>
             {
                 return Task.Run(() =>
                 {
                     var geneticInput = new GeneticDesignInput(_selectedSupportedFloor, NoGenerations, NoPopulation, includedElements.IncludedPlywoods, includedElements.IncludedBeamSections);
                     switch (SelectedSystem)
                     {
                         case FormworkSystem.CUPLOCK_SYSTEM:
                             return GeneticFactoryHelper.CostCuplockGenetic(geneticInput, costInput)
                                                                  .Select((chm, i) => chm.AsCostGeneticResult(costInput, i + 1))
                                                                  .Cast<NoCostGeneticResult>()
                                                                  .ToList();
                         case FormworkSystem.EUROPEAN_PROPS_SYSTEM:
                             return GeneticFactoryHelper.CostEurpopeanPropGenetic(geneticInput, costInput)
                                                                 .Select((chm, i) => chm.AsCostGeneticResult(costInput, i + 1))
                                                                 .Cast<NoCostGeneticResult>()
                                                                 .ToList();
                         case FormworkSystem.SHORE_SYSTEM:
                             return GeneticFactoryHelper.CostShorGenetic(geneticInput, costInput)
                                                                 .Select((chm, i) => chm.AsCostGeneticResult(costInput, i + 1))
                                                                 .Cast<NoCostGeneticResult>()
                                                                 .ToList();
                         default:
                             return new List<NoCostGeneticResult>();
                     }
                 });
             };
            return from includedEles in _includedElementsService()
                   from costInput in GetCostResultInput()
                   select getResults(costInput, includedEles);
        }

        #endregion

        #region Classes

        private struct GeneticResultKey
        {
            public GeneticResultKey(OptimizeOption optimizeOption, FormworkSystem system)
            {
                OptimizeOption = optimizeOption;
                System = system;
            }

            public OptimizeOption OptimizeOption { get; }

            public FormworkSystem System { get; }

        }

        #endregion

    }
}
