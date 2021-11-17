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
using FormworkOptimize.Core.Constants;
using static CSharp.Functional.Functional;
using FormworkOptimize.App.Extensions;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticOptionsViewModel : ViewModelBase
    {

        #region Private Fields

        private readonly Dictionary<GeneticResultKey, Tuple<List<NoCostGeneticResult>,List<ChromosomeHistory>>> _resultCash;

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

        private double _crossOverPropability;

        private double _mutationPropability;

        private List<ChromosomeHistory> _gaHistory;

        #endregion

        #region Properties

        public List<ChromosomeHistory> GaHistory
        {
            get => _gaHistory;
            set => NotifyPropertyChanged(ref _gaHistory,value);
        }

        public double CrossOverProbability
        {
            get => _crossOverPropability;
            set => NotifyPropertyChanged(ref _crossOverPropability, value);
        }

        public double MutationProbability
        {
            get => _mutationPropability;
            set=>NotifyPropertyChanged(ref _mutationPropability, value);
        }

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

        public ICommand GraphCommand { get;}

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
                    Tuple<List<NoCostGeneticResult>,List<ChromosomeHistory>> geneticResults = null;
                    var isExist = _resultCash.TryGetValue(key, out geneticResults);
                    if (isExist)
                    {
                        IsGeneticResultsVisible = true;
                        GeneticResults = geneticResults.Item1;
                        GaHistory = geneticResults.Item2;
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
                    Tuple<List<NoCostGeneticResult>,List<ChromosomeHistory>> geneticResults = null;
                    var isExist = _resultCash.TryGetValue(key, out geneticResults);
                    if (isExist)
                    {
                        IsGeneticResultsVisible = true;
                        GeneticResults = geneticResults.Item1;
                        GaHistory = geneticResults.Item2;
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
            _resultCash = new Dictionary<GeneticResultKey, Tuple<List<NoCostGeneticResult>,List<ChromosomeHistory>>>();
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
            NoGenerations = 100;
            NoPopulation = 1000;
            GeneticCommand = new RelayCommand(OnGenetic, CanGenetic);
            ShowDetailResultCommand = new RelayCommand(OnShowDetailResult, CanShowDetailResult);
            DrawCommand = new RelayCommand(OnDraw, CanDraw);
            ExportCommand = new RelayCommand(OnExport, CanExport);
            GraphCommand = new RelayCommand(OnGraph, CanGraph);
            IsGeneticResultsVisible = false;
            Mediator.Instance.Subscribe<Floor>(this, (hostFloor) => _selectedHostFloor = hostFloor, Context.HOST_FLOOR);
            Mediator.Instance.Subscribe<Floor>(this, (supportedFloor) => _selectedSupportedFloor = supportedFloor, Context.SUPPORTED_FLOOR);
            IsLoading = false;
            BoundaryLinesOffset = 0;//cm
            BeamsOffset = 50;//cm
            CrossOverProbability = 0.8; //0.6 -> 1
            MutationProbability = 0.05; //0 -> 0.1
            _costFilePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Cost Database\Formwork Elements Cost.json";

        }


        #endregion

        #region Methods

        private bool CanGraph() =>
           GaHistory != null;


        private void OnGraph()
        {
            var history = new GeneticHistoryViewModel(GaHistory);
            Mediator.Instance.NotifyColleagues(history, Context.GENETIC_DETAIL_RESULT);
        }

        private bool CanExport() =>
            SelectedGeneticResult != null;


        private async void OnExport()
        {
            Func<string, Func<FormworkCostElements, FormworkElementCost>, Task<List<Exceptional<string>>>> exportFunc = async (dir, costFunc) =>
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
                                                                              .AsDtos()
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

            var validCostfunc = await GetCostFunc();
            validCostfunc.Map(costfunc => _folderDialogService(dir => exportFunc(dir, costfunc)).Map(showResult));

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
                        var res = from x in costGenetic.SystemModel.Draw(_doc)
                                  from y in costGenetic.Plywood.Draw(_doc)
                                  select Unit();
                        res.Match(_showErrors, unit => unit);
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
            BeamsOffset >= 0 &&
            CrossOverProbability <=1 &&
            CrossOverProbability > 0 &&
            MutationProbability > 0 && 
            MutationProbability <= 1;

        private async  void OnGenetic()
        {
            var validFunc = await GetCostFunc();
            Action<IEnumerable<Error>> invalid = (errs) =>
            {
                IsLoading = false;
                _showErrors(errs);
            };
            Action<Task<Tuple<List<NoCostGeneticResult>, List<ChromosomeHistory>>>> valid = async (results) =>
            {
                var key = new GeneticResultKey(SelectedOptimizeOption, SelectedSystem);
                var tuple = await results;
                GeneticResults = tuple.Item1;
                GaHistory = tuple.Item2;
                Tuple<List<NoCostGeneticResult>,List<ChromosomeHistory>> value = null;
                var result = _resultCash.TryGetValue(key, out value);
                if (result)
                {
                    _resultCash.Remove(key);
                    _resultCash.Add(key, tuple);
                }
                else
                {
                    _resultCash.Add(key, tuple);
                }
                IsLoading = false;
                IsGeneticResultsVisible = true;
            };

            IsLoading = true;

            if (SelectedOptimizeOption == OptimizeOption.DESIGN)
                validFunc.Bind(GetDesignGeneticResults).Match(invalid.ToFunc(), valid.ToFunc());
            else
               validFunc.Bind(GetCostGeneticResults).Match(invalid.ToFunc(),  valid.ToFunc());
        }

        private async Task<Validation<Func<FormworkCostElements, FormworkElementCost>>> GetCostFunc()
        {
            var validEles = await _costFilePath.ReadAsJsonList<FormworkElementCost>();
            return validEles.Map(db =>
                          {
                              Func<FormworkCostElements, FormworkElementCost> costFunc = name =>
                               {
                                   return db.First(ele => ele.Name == name);
                               };
                              return costFunc;
                          });
        }

        private Validation<CostGeneticResultInput> GetCostResultInput(Func<FormworkCostElements, FormworkElementCost> costFunc)
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


                Func<CostParameter, CostGeneticResultInput> asCostInput =  costParameters =>
                {
                    var timeLine = costParameters.Time.AsTimeLine();
                    var manPowerCost = costParameters.ManPower.AsManPowerCost(timeLine);
                    var equipmentsCost = costParameters.Equipments.AsEquipmentsCost(timeLine);
                    var transportationCost = costParameters.Transportation.AsTransportationCost();


                    
                    return  new CostGeneticResultInput(costFunc,
                                                                                revitInput,
                                                                                BoundaryLinesOffset,
                                                                                BeamsOffset,
                                                                                timeLine,
                                                                                manPowerCost,
                                                                                equipmentsCost,
                                                                                transportationCost,
                                                                                floorArea,
                                                                                asFloorPlywood);
                };

                return _costParameterService(floorArea, smallerLength).Map(asCostInput);
            };
            return _doc.GetDefault3DView()
                       .Bind(getCostInput);
        }

        public Validation<Task<Tuple<List<NoCostGeneticResult>, List<ChromosomeHistory>>>> GetDesignGeneticResults(Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            Func<CostGeneticResultInput, GeneticIncludedElements, Task<Tuple<List<NoCostGeneticResult>,List<ChromosomeHistory>>>> getResults = (costInput, includedElements) =>
              {
                  return Task.Run(() =>
                  {
                      var newCostInput =costInput is null ? null: costInput.UpdateCostInputWithNewRevitInput(costInput.RevitInput.UpdateWithNewXYZ(costInput.RevitInput.MainBeamDir.CrossProduct(XYZ.BasisZ)));
                      var costInputs = new List<CostGeneticResultInput>() { costInput, newCostInput };
                      var geneticInput = new GeneticDesignInput(_selectedSupportedFloor, NoGenerations, NoPopulation,CrossOverProbability,MutationProbability, includedElements);
                      switch (SelectedSystem)
                      {
                          case FormworkSystem.CUPLOCK_SYSTEM:
                              (var chms, var history) = GeneticFactoryHelper.DesignCuplockGenetic(geneticInput);
                              var cupResult =  chms.AsParallel()
                                         .Select((chm, i) => costInputs.Select(input => chm.AsGeneticResult(input, i + 1)).OrderBy(resul => resul.Cost).First())
                                         .Cast<NoCostGeneticResult>()
                                         .ToList();
                              return Tuple.Create(cupResult, history);
                          case FormworkSystem.EUROPEAN_PROPS_SYSTEM:
                              (var euroChms,var euroHistory) = GeneticFactoryHelper.DesignEurpopeanPropGenetic(geneticInput);
                                var euroResult =  euroChms.AsParallel()
                                               .Select((chm, i) => costInputs.Select(input => chm.AsGeneticResult(input, i + 1)).OrderBy(resul => resul.Cost).First())
                                               .Cast<NoCostGeneticResult>()
                                               .ToList();
                              return Tuple.Create(euroResult, euroHistory);
                          case FormworkSystem.SHORE_SYSTEM:
                              (var shoreChms, var shorHistory) = GeneticFactoryHelper.DesignShorGenetic(geneticInput);
                               var shoreResult =  shoreChms.AsParallel()
                                               .Select((chm, i) => costInputs.Select(input => chm.AsGeneticResult(input, i + 1)).OrderBy(resul => resul.Cost).First())
                                               .Cast<NoCostGeneticResult>()
                                               .ToList();
                              return Tuple.Create(shoreResult, shorHistory);
                          case FormworkSystem.FRAME_SYSTEM:
                              (var frameChms, var frameHistory) = GeneticFactoryHelper.DesignFrameGenetic(geneticInput);
                               var frameResult =frameChms.AsParallel()
                                                   .Select((chm, i) => chm.AsGeneticResult(i + 1))
                                                   .ToList();
                              return Tuple.Create(frameResult,frameHistory);
                          case FormworkSystem.ALUMINUM_PROPS_SYSTEM:
                              (var aluChms, var aluHistory) = GeneticFactoryHelper.DesignAluminumPropGenetic(geneticInput);
                               var aluResult =  aluChms.AsParallel()
                                             .Select((chm, i) => chm.AsGeneticResult(i + 1))
                                             .ToList();
                              return Tuple.Create(aluResult,aluHistory);
                          default:
                              return Tuple.Create(new List<NoCostGeneticResult>(),new List<ChromosomeHistory>());
                      }
                  });
              };

            if (SelectedSystem == FormworkSystem.CUPLOCK_SYSTEM || SelectedSystem == FormworkSystem.EUROPEAN_PROPS_SYSTEM || SelectedSystem == FormworkSystem.SHORE_SYSTEM)
            {
                return from includedEles in _includedElementsService()
                       from costInput in GetCostResultInput(costFunc)
                       select getResults(costInput, includedEles);
            }
            else
            {
                return _includedElementsService().Map(includedEles => getResults(null, includedEles));
            }
        }

        public Validation<Task<Tuple< List<NoCostGeneticResult>,List<ChromosomeHistory>>>> GetCostGeneticResults(Func<FormworkCostElements, FormworkElementCost> costFunc)
        {
            Func<CostGeneticResultInput, GeneticIncludedElements, Task<Tuple<List<NoCostGeneticResult>, List<ChromosomeHistory>>>> getResults = (costInput, includedElements) =>
             {
                 return Task.Run(() =>
                 {
                     var geneticInput = new GeneticDesignInput(_selectedSupportedFloor, NoGenerations, NoPopulation,CrossOverProbability,MutationProbability, includedElements);
                     switch (SelectedSystem)
                     {
                         case FormworkSystem.CUPLOCK_SYSTEM:
                             (var cuplockChms, var cuplockHistory) = GeneticFactoryHelper.CostCuplockGenetic(geneticInput, costInput);
                             var cuplockResult = cuplockChms.Select((chm, i) => chm.AsCostGeneticResult(costInput, i + 1))
                                                             .Cast<NoCostGeneticResult>()
                                                             .ToList();
                             return Tuple.Create(cuplockResult, cuplockHistory);
                         case FormworkSystem.EUROPEAN_PROPS_SYSTEM:
                             (var euroChms, var euroHistory) = GeneticFactoryHelper.CostEurpopeanPropGenetic(geneticInput, costInput);
                             var euroResult =euroChms.Select((chm, i) => chm.AsCostGeneticResult(costInput, i + 1))
                                                      .Cast<NoCostGeneticResult>()
                                                      .ToList();
                             return Tuple.Create(euroResult, euroHistory);
                         case FormworkSystem.SHORE_SYSTEM:
                             (var shoreChms, var shoreHistory) = GeneticFactoryHelper.CostShorGenetic(geneticInput, costInput);
                              var shoreResult =shoreChms.Select((chm, i) => chm.AsCostGeneticResult(costInput, i + 1))
                                                        .Cast<NoCostGeneticResult>()
                                                        .ToList();
                             return Tuple.Create(shoreResult, shoreHistory);
                         default:
                             return Tuple.Create( new List<NoCostGeneticResult>(),new List<ChromosomeHistory>());
                     }
                 });
             };
            return from includedEles in _includedElementsService()
                   from costInput in GetCostResultInput(costFunc)
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
