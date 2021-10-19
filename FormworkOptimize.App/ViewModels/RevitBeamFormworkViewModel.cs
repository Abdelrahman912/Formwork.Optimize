using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using CSharp.Functional.Errors;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.Utils;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS.Revit.Input;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.DTOS.Revit.Input.Props;
using FormworkOptimize.Core.DTOS.Revit.Input.Shore;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.RevitHelper;
using FormworkOptimize.Core.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using static FormworkOptimize.App.Utils.Memoization;
using Unit = System.ValueTuple;
using static FormworkOptimize.Core.SelectionFilters.Filters;
using FormworkOptimize.Core.DTOS;
using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Helpers.DesignHelper;
using FormworkOptimize.Core.Entities.Designer;
using static CSharp.Functional.Functional;
using FormworkOptimize.Core.Entities.Geometry;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitBeamFormworkViewModel : RevitModelFormworkViewModel<ElementSelectionModel>
    {

        #region Private Fields

        private double _columnEdgeOffset;

        private readonly RevitCuplockViewModel _cuplockVM;

        private readonly RevitPropViewModel _propVM;

        private readonly RevitShoreViewModel _shoreVM;

        private object _currentShoringVM;

        private bool _isModelFromBeams;

        private readonly Func<FloorKey, Validation<List<ElementSelectionModel>>> _cachedBeamsFromFloorFunc;

        private readonly Func<ElementKey, string> _nearestAxisFunc;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        private Func<double, double, Validation<Tuple<double, DesignResultViewModel>>> _designResultFunc;

        private readonly Func<DesignResultViewModel, DesignResultDialog> _designResultService;

        private List<double> _secondaryBeamLengths;

        private List<double> _mainBeamLengths;

        private double _selectedSecondaryBeamLength;

        private double _selectedMainBeamLength;

        #endregion

        #region Properties

        public double SelectedSecondaryBeamLength
        {
            get => _selectedSecondaryBeamLength;
            set => NotifyPropertyChanged(ref _selectedSecondaryBeamLength, value);
        }

        public double SelectedMainBeamLength
        {
            get => _selectedMainBeamLength;
            set => NotifyPropertyChanged(ref _selectedMainBeamLength, value);
        }

        public bool IsModelFromBeams
        {
            get => _isModelFromBeams;
            set
            {
                NotifyPropertyChanged(ref _isModelFromBeams, value);
                if (!_isModelFromBeams)
                    GetBeamsFromSupportedFloorCached();
                else
                    Table = new List<ElementSelectionModel>();
            }
        }

        public object CurrentShoringVM
        {
            get => _currentShoringVM;
            set => NotifyPropertyChanged(ref _currentShoringVM, value);
        }

        public double ColumnEdgeOffset
        {
            get => _columnEdgeOffset;
            set => NotifyPropertyChanged(ref _columnEdgeOffset, value);
        }

        public List<double> SecondaryBeamLengths
        {
            get => _secondaryBeamLengths;
            set => NotifyPropertyChanged(ref _secondaryBeamLengths, value);
        }

        public List<double> MainBeamLengths
        {
            get => _mainBeamLengths;
            set => NotifyPropertyChanged(ref _mainBeamLengths, value);
        }

        public ICommand SelectBeamsCommand { get; }

        public ICommand SelectBeamRowCommand { get; }

        public ICommand DesignCommand { get; }

        #endregion

        #region Constructors

        public RevitBeamFormworkViewModel(RevitFloorsViewModel floorsVM,
                                          Func<List<ResultMessage>, Unit> notificationService,
                                          Func<DesignResultViewModel, DesignResultDialog> designResultService)
            : base(floorsVM, new List<RevitFormworkSystem>() { RevitFormworkSystem.CUPLOCK_SYSTEM, RevitFormworkSystem.PROPS_SYSTEM, RevitFormworkSystem.SHORE_SYSTEM })
        {
            _notificationService = notificationService;
            _designResultService = designResultService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());

            Mediator.Instance.Subscribe<PlywoodSectionName>(this, (section) => _selectedPlywoodSection = section, Context.PLYWOOD_SECTION);
            Mediator.Instance.Subscribe<Floor>(this, (hostFloor) => _selectedHostFloor = hostFloor, Context.HOST_FLOOR);
            Mediator.Instance.Subscribe<Floor>(this, (supportedFloor) => { _selectedSupportedFloor = supportedFloor; if (!IsModelFromBeams) GetBeamsFromSupportedFloorCached(); }, Context.SUPPORTED_FLOOR);
            Mediator.Instance.Subscribe<double>(this, OnMainBeamMinLengthChanged, Context.MAIN_BEAM_MIN_LENGTH);
            Mediator.Instance.Subscribe<double>(this, OnSecBeamMinLengthChanged, Context.SEC_BEAM_MIN_LENGTH);

            SelectBeamsCommand = new RelayCommand(OnSelectBeams, CanSelectBeams);
            SelectBeamRowCommand = new RelayCommand(OnSelectBeamRow, CanSelectBeamRow);
            DesignCommand = new RelayCommand(OnDesign, CanDesign);


            _cuplockVM = new RevitCuplockViewModel();
            _propVM = new RevitPropViewModel();
            _shoreVM = new RevitShoreViewModel(false);
            CurrentShoringVM = _cuplockVM;

            _cachedBeamsFromFloorFunc = Memoization.MemoizeWeak<FloorKey, List<ElementSelectionModel>>(key => GetBeamsFromFloor(key.Floor), TimeSpan.FromMinutes(2));
            Func<Element, string> nearestAxisFunc = (e) => e.GetBeamNearestAxes(_xAxes.Concat(_yAxes));
            _nearestAxisFunc = Memoization.MemoizeWeak<ElementKey, string>(key => nearestAxisFunc(key.Element), TimeSpan.FromMinutes(2));

            IsModelFromBeams = false;
        }


        #endregion

        #region Methods

        private void OnSecBeamMinLengthChanged(double minLength)
        {
            SecondaryBeamLengths = Database.GetBeamLengths(_selectedSecondaryBeamSection)
                                           .Where(l => l > minLength)
                                           .ToList();
            SelectedSecondaryBeamLength = SecondaryBeamLengths.FirstOrDefault();
        }

        private void OnMainBeamMinLengthChanged(double minLength)
        {
            MainBeamLengths = Database.GetBeamLengths(_selectedMainBeamSection)
                                      .Where(l => l > minLength)
                                      .ToList();
            SelectedMainBeamLength = MainBeamLengths.FirstOrDefault();
        }

        private bool CanDesign()
        {
            return true;
        }

        private void OnDesign()
        {
            Func<Element, Validation<Tuple<double, DesignResultViewModel>>> getTuple = (ele) =>
            {
                var eleType = _doc.GetElement(ele.GetTypeId());
                var beamWidthCm = eleType.LookupParameter("b")
                                       .AsDouble()
                                       .FeetToCm();
                var beamThicknessCm = eleType.LookupParameter("h")
                                         .AsDouble()
                                         .FeetToCm();
                return _designResultFunc(beamWidthCm, beamThicknessCm);
            };

            Func<Tuple<double, DesignResultViewModel>, Unit> action = tuple =>
             {
                 (var secBeamspacing, var designResult) = tuple;
                 var dialogResult = _designResultService(designResult);
                 if (dialogResult == DesignResultDialog.ACCEPT)
                 {
                     SecondaryBeamsSpacing = secBeamspacing;
                 }
                 return Unit();
             };

            _uiDoc.PickElement(BeamFilter, "Please Select a concrete Beam.")
                  .Map(getTuple)
                  .Map(valid => valid.Match(_showErrors,action));
                  
        }

        private Validation<Tuple<double, DesignResultViewModel>> DesignShoreBrace(double beamWidthCm, double beamThicknessCm)
        {
            var slabThicknessCm = _selectedSupportedFloor.get_Parameter(BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS)
                                                        .AsDouble()
                                                        .FeetToCm();
            var designInput = new ShoreBraceDesignInput(_selectedPlywoodSection,
                                                    SelectedSecondaryBeamSection.ToBeamSectionName(),
                                                    SelectedMainBeamSection.ToBeamSectionName(),
                                                    _shoreVM.SelectedSpacingMain,
                                                    SelectedSecondaryBeamLength,
                                                    SelectedMainBeamLength,
                                                    slabThicknessCm,
                                                    new AutomaticSecondaryBeamSpacing(),
                                                    beamThicknessCm,
                                                    beamWidthCm);

            var designer = ShoreBraceDesigner.Instance;

            Func<FrameDesignOutput, Tuple<double, DesignResultViewModel>> getOutput = designOutput =>
            {
                var designResult =  new DesignResultViewModel()
                {
                    PlywoodDesignOutput = designOutput.Plywood.AsDesignOutput(),
                    SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", designOutput.SecondaryBeam.Item2.ToList()),
                    MainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", designOutput.MainBeam.Item2.ToList()),
                    ShoringSystemDesignOutput = new ShoringDesignOutput("Shore Brace System", new List<DesignReport>() { designOutput.Shoring.Item2 })
                };
                return Tuple.Create(designOutput.Plywood.Item1.Span, designResult);
            };
            
            return designer.Design(designInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction)
                           .Map(getOutput);
            
        }

        private Validation<Tuple<double, DesignResultViewModel>> DesignProps(double beamWidthCm , double beamThicknessCm)
        {
            var slabThicknessCm = _selectedSupportedFloor.get_Parameter(BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS)
                                                         .AsDouble()
                                                         .FeetToCm();
            var designInput = new EuropeanPropDesignInput(_selectedPlywoodSection,
                                                    SelectedSecondaryBeamSection.ToBeamSectionName(),
                                                    SelectedMainBeamSection.ToBeamSectionName(),
                                                    _propVM.SelectedPropType,
                                                    _propVM.SpacingMain,
                                                    _propVM.SpacingSecondary,
                                                    SelectedSecondaryBeamLength,
                                                    SelectedMainBeamLength,
                                                    slabThicknessCm,
                                                    new AutomaticSecondaryBeamSpacing(),
                                                    beamThicknessCm,
                                                    beamWidthCm);
            var designer = EuropeanPropDesigner.Instance;


            Func<PropDesignOutput, Tuple<double, DesignResultViewModel>> getOutput = designOutput =>
            {
                var designResult = new DesignResultViewModel()
                {
                    PlywoodDesignOutput = designOutput.Plywood.AsDesignOutput(),
                    SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", designOutput.SecondaryBeam.Item2.ToList()),
                    MainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", designOutput.MainBeam.Item2.ToList()),
                    ShoringSystemDesignOutput = new ShoringDesignOutput("Props System", new List<DesignReport>() { designOutput.Shoring.Item2 })
                };
                return Tuple.Create(designOutput.Plywood.Item1.Span, designResult);
            };

            return designer.Design(designInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction)
                           .Map(getOutput);

        }

        private Validation<Tuple<double, DesignResultViewModel>> DesignCuplock(double beamWidthCm, double beamThicknessCm)
        {
            var slabThicknessCm = _selectedSupportedFloor.get_Parameter(BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS)
                                                         .AsDouble()
                                                         .FeetToCm();
            var designInput = new CuplockDesignInput(_selectedPlywoodSection,
                                                     SelectedSecondaryBeamSection.ToBeamSectionName(),
                                                     SelectedMainBeamSection.ToBeamSectionName(),
                                                     _cuplockVM.SelectedSteelType,
                                                     _cuplockVM.SelectedLedgerMain,
                                                     _cuplockVM.SelectedLedgerSecondary,
                                                     SelectedSecondaryBeamLength,
                                                     SelectedMainBeamLength,
                                                     slabThicknessCm,
                                                     new AutomaticSecondaryBeamSpacing(),
                                                     beamThicknessCm,
                                                     beamWidthCm);
            var designer = CuplockDesigner.Instance;
            Func<CuplockDesignOutput, Tuple<double, DesignResultViewModel>> getOutput = designOutput =>
            {
                var designResult = new DesignResultViewModel()
                {
                    PlywoodDesignOutput = designOutput.Plywood.AsDesignOutput(),
                    SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", designOutput.SecondaryBeam.Item2.ToList()),
                    MainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", designOutput.MainBeam.Item2.ToList()),
                    ShoringSystemDesignOutput = new ShoringDesignOutput("Cuplock System", new List<DesignReport>() { designOutput.Shoring.Item2 })
                };
                return Tuple.Create(designOutput.Plywood.Item1.Span, designResult);
            };

            return designer.Design(designInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction)
                           .Map(getOutput);
        }

        protected override bool CanModel() =>
            _selectedHostFloor != null && _selectedSupportedFloor != null && Table.Any(beam => beam.IsSelected);


        private bool CanSelectBeamRow() =>
            SelectedRow != null;


        private void OnSelectBeamRow()
        {
            _uiDoc.Selection.SetElementIds(new List<ElementId>() { SelectedRow.Element.Id });
        }

        private bool CanSelectBeams() =>
            IsModelFromBeams;


        private Validation<List<ElementSelectionModel>> GetBeamsFromFloor(Floor floor)
        {
            Func<View3D, List<ElementSelectionModel>> getBeams = view =>
            {
                var options = view.Options();
                var supportedLevel = _doc.GetElement(floor.LevelId) as Level;
                var revitFloor = floor.GetGeometry(options);
                return _doc.GetBeamsInLevelWithinPolygon(supportedLevel, revitFloor.Boundary)
                                   .Select(e => new ElementSelectionModel(e, _nearestAxisFunc(new ElementKey(e)), IsSelectAllRows))
                                   .ToList();
            };

            return _doc.GetDefault3DView()
                       .Map(getBeams);
        }

        private void GetBeamsFromSupportedFloorCached()
        {
            if (_selectedSupportedFloor is null)
                return;
            var floorKey = new FloorKey(_selectedSupportedFloor);
            Action<List<ElementSelectionModel>> updateTable = elementSelections =>
                Table = elementSelections;

            _cachedBeamsFromFloorFunc(floorKey).Match(_showErrors, updateTable.ToFunc());
        }

        private void OnSelectBeams()
        {
            Action<List<Element>> updateTableOfSameLevel = beams =>
            {
                Table = beams.Where(e => e.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM).AsElementId().IntegerValue == _selectedSupportedFloor.LevelId.IntegerValue)
                             .Select(e => new ElementSelectionModel(e, _nearestAxisFunc(new ElementKey(e)), true))
                             .ToList();
            };

            _uiDoc.PickElements(BeamFilter, "Select Beams in the same Supported Floor Level.")
                  .ForEach(updateTableOfSameLevel);
        }


        protected override void ChangeShoring(RevitFormworkSystem formworkSystem)
        {
            switch (formworkSystem)
            {
                case RevitFormworkSystem.CUPLOCK_SYSTEM:
                    CurrentShoringVM = _cuplockVM;
                    _designResultFunc = DesignCuplock;
                    _modelAction = () => ModelBeam(ModelCuplockFormwork);
                    break;
                case RevitFormworkSystem.PROPS_SYSTEM:
                    CurrentShoringVM = _propVM;
                    _designResultFunc = DesignProps;
                    _modelAction = () => ModelBeam(ModelPropsFormwork);
                    break;
                case RevitFormworkSystem.SHORE_SYSTEM:
                    CurrentShoringVM = _shoreVM;
                    _designResultFunc = DesignShoreBrace;
                    _modelAction = () => ModelBeam(ModelShoreFormwork);
                    break;
            }
        }

        private void ModelBeam(Action<RevitInput> modelAction)
        {
            var beamInput = _doc.GetRevitBeamInput(Table.Where(row => row.IsSelected).Select(row => row.Element).ToList(), _selectedHostFloor);
            modelAction(beamInput);
        }

        protected override void ModelCuplockFormwork(RevitInput revitInput)
        {
            var revitBeamInput = revitInput as RevitBeamInput;
            var beamCuplockInput = new RevitBeamCuplockInput(_selectedPlywoodSection,
                                                             SelectedMainBeamSection,
                                                             SelectedSecondaryBeamSection,
                                                             _cuplockVM.SelectedSteelType,
                                                             SelectedSecondaryBeamLength.CmToFeet(),
                                                             SelectedMainBeamLength.CmToFeet(),
                                                             SecondaryBeamsSpacing.CmToFeet(),
                                                             _cuplockVM.SelectedLedgerMain.CmToFeet(),
                                                             _cuplockVM.SelectedLedgerSecondary.CmToFeet());

            Action<List<RevitCuplock>> draw = cuplock =>
            {
                _doc.UsingTransaction(_ => _doc.LoadCuplockFamilies(), "Load Cuplock Families");
                _doc.UsingTransaction(_ => _doc.LoadDeckingFamilies(), "Load Decking Families");
                _doc.UsingTransaction(_ => cuplock.Draw(_doc), "Model Beams Cuplock shoring");
            };

            CuplockShoringHelper.BeamsToCuplock(revitBeamInput, beamCuplockInput)
                                 .Match(_showErrors, draw.ToFunc());

        }

        protected override void ModelPropsFormwork(RevitInput revitInput)
        {
            var revitBeamInput = revitInput as RevitBeamInput;
            var beamPropsInput = new RevitBeamPropsInput(_propVM.SelectedPropType,
                                                         _selectedPlywoodSection,
                                                         SelectedMainBeamSection,
                                                         SelectedSecondaryBeamSection,
                                                         SelectedSecondaryBeamLength.CmToFeet(),
                                                         SelectedMainBeamLength.CmToFeet(),
                                                         SecondaryBeamsSpacing.CmToFeet(),
                                                         _propVM.SpacingMain.CmToFeet(),
                                                         _propVM.SpacingSecondary.CmToFeet());

            Action<List<RevitProps>> draw = props =>
            {
                _doc.UsingTransaction(_ => _doc.LoadPropFamilies(), "Load Props Families");
                _doc.UsingTransaction(_ => _doc.LoadDeckingFamilies(), "Load Decking Families");
                _doc.UsingTransaction(_ => props.Draw(_doc), "Model Beams Props shoring");
            };

            PropsShoringHelper.BeamsToProps(revitBeamInput, beamPropsInput)
                              .Match(_showErrors, draw.ToFunc());

        }

        protected override void ModelShoreFormwork(RevitInput revitInput)
        {
            var revitBeamInput = revitInput as RevitBeamInput;
            var beamShoreInput = new RevitBeamShoreInput(_selectedPlywoodSection,
                                                             SelectedMainBeamSection,
                                                             SelectedSecondaryBeamSection,
                                                             SelectedSecondaryBeamLength.CmToFeet(),
                                                             SelectedMainBeamLength.CmToFeet(),
                                                             SecondaryBeamsSpacing.CmToFeet(),
                                                              _shoreVM.SelectedSpacingMain.CmToFeet(),
                                                              _shoreVM.SpacingSec.CmToFeet());


            Action<List<RevitShore>> draw = shores =>
             {
                 _doc.UsingTransaction(_ => _doc.LoadShoreBraceFamilies(), "Load Shore Brace Families");
                 _doc.UsingTransaction(_ => _doc.LoadDeckingFamilies(), "Load Decking Families");
                 _doc.UsingTransaction(_ => shores.Draw(_doc), "Model Beams Shore Brace shoring");
             };

            ShoreHelper.BeamsToShore(revitBeamInput, beamShoreInput)
                .Match(_showErrors, draw.ToFunc());
        }

        #endregion

    }
}
