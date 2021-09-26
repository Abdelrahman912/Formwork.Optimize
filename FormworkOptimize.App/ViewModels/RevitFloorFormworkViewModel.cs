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
using FormworkOptimize.Core.Entities.Revit;
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
using static CSharp.Functional.Functional;
using FormworkOptimize.Core.DTOS;
using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Helpers.DesignHelper;
using FormworkOptimize.Core.Entities.Designer;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitFloorFormworkViewModel : RevitModelFormworkViewModel<ColumnOffsetSelectionModel>
    {

        #region Private Fields

        private bool _isModelFromDetailLines;

        private RevitFloor _revitFloor;

        private bool _isColumnsWithConstantOffset;

        private double _constantColumnOffset;

        private double _selectedSecondaryBeamLength;

        private double _selectedMainBeamLength;

        private double _floorLinesOffset;

        private double _beamOffset;

        private List<double> _secondaryBeamLengths;

        private List<double> _mainBeamLengths;

        private readonly RevitCuplockViewModel _cuplockVM;

        private readonly RevitPropViewModel _propVM;

        private readonly RevitShoreViewModel _shoreVM;

        private object _currentShoringVM;

        private readonly Func<FloorsKey, Validation<Tuple<List<ColumnOffsetSelectionModel>, RevitConcreteFloor>>> _cachedColumnsFromFloorFunc;

        private readonly Func<ElementKey, string> _nearestAxisFunc;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        private Func<Validation<Tuple<double, DesignResultViewModel>>> _designResultFunc;

        private readonly Func<DesignResultViewModel, DesignResultDialog> _designResultService;

        #endregion

        #region Properties

        public bool IsColumnsWithConstantOffset
        {
            get => _isColumnsWithConstantOffset;
            set => NotifyPropertyChanged(ref _isColumnsWithConstantOffset, value);
        }


        public double ConstantColumnOffset
        {
            get => _constantColumnOffset;
            set
            {
                NotifyPropertyChanged(ref _constantColumnOffset, value);
                Table.ForEach(row => row.Offset = _constantColumnOffset);
            }
        }

        public bool IsModelFromDetailLines
        {
            get => _isModelFromDetailLines;
            set
            {
                NotifyPropertyChanged(ref _isModelFromDetailLines, value);
                if (_isModelFromDetailLines)
                {
                    _revitFloor = new RevitLineFloor(new List<Line>(), new List<List<Line>>());
                    Table = new List<ColumnOffsetSelectionModel>();
                    FloorLinesOffset = 0;
                }
                else
                    GetColumnsFromSupportedFloorCached();
            }
        }


        public object CurrentShoringVM
        {
            get => _currentShoringVM;
            set => NotifyPropertyChanged(ref _currentShoringVM, value);
        }

        public override RevitBeamSectionName SelectedMainBeamSection
        {
            get => _selectedMainBeamSection;
            set
            {
                NotifyPropertyChanged(ref _selectedMainBeamSection, value);
                MainBeamLengths = Database.GetBeamLengths(_selectedMainBeamSection);
                SelectedMainBeamLength = MainBeamLengths.FirstOrDefault();
            }
        }

        public override RevitBeamSectionName SelectedSecondaryBeamSection
        {
            get => _selectedSecondaryBeamSection;
            set
            {
                NotifyPropertyChanged(ref _selectedSecondaryBeamSection, value);
                SecondaryBeamLengths = Database.GetBeamLengths(_selectedSecondaryBeamSection);
                SelectedSecondaryBeamLength = SecondaryBeamLengths.FirstOrDefault();
            }
        }

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

        public double FloorLinesOffset
        {
            get => _floorLinesOffset;
            set => NotifyPropertyChanged(ref _floorLinesOffset, value);
        }

        public double BeamOffset
        {
            get => _beamOffset;
            set => NotifyPropertyChanged(ref _beamOffset, value);
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

        public RevitCuplockViewModel CuplockVM { get; set; }

        public ICommand SelectDetailLinesCommand { get; }

        public ICommand SelectColumnRowCommand { get; }

        public ICommand ColumnsFromUserSelectionCommand { get; }

        public ICommand DesignCommand { get; }

        #endregion

        #region Constructors

        public RevitFloorFormworkViewModel(RevitFloorsViewModel floorsVM,
                                          Func<List<ResultMessage>, Unit> notificationService,
                                          Func<DesignResultViewModel, DesignResultDialog> designResultService)
            : base(floorsVM, new List<RevitFormworkSystem>() { RevitFormworkSystem.CUPLOCK_SYSTEM, RevitFormworkSystem.PROPS_SYSTEM, RevitFormworkSystem.SHORE_SYSTEM })
        {
            _notificationService = notificationService;
            _designResultService = designResultService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());

            _cachedColumnsFromFloorFunc = Memoization.MemoizeWeak<FloorsKey, Tuple<List<ColumnOffsetSelectionModel>, RevitConcreteFloor>>(key => GetColumnsFromFloor(key), TimeSpan.FromMinutes(2));

            Mediator.Instance.Subscribe<PlywoodSectionName>(this, (section) => _selectedPlywoodSection = section, Context.PLYWOOD_SECTION);
            Mediator.Instance.Subscribe<Floor>(this, (hostFloor) => { _selectedHostFloor = hostFloor; if (!IsModelFromDetailLines) GetColumnsFromSupportedFloorCached(); }, Context.HOST_FLOOR);
            Mediator.Instance.Subscribe<Floor>(this, (supportedFloor) => { _selectedSupportedFloor = supportedFloor; if (!IsModelFromDetailLines) GetColumnsFromSupportedFloorCached(); }, Context.SUPPORTED_FLOOR);
            Mediator.Instance.Subscribe<double>(this, OnMainBeamMinLengthChanged, Context.MAIN_BEAM_MIN_LENGTH);
            Mediator.Instance.Subscribe<double>(this, OnSecBeamMinLengthChanged, Context.SEC_BEAM_MIN_LENGTH);


            SelectDetailLinesCommand = new RelayCommand(OnSelectDetailLines, CanSelectDetailLines);
            SelectColumnRowCommand = new RelayCommand(OnSelectSelectedColumnRow, CanSelectSelectedColumnRow);
            ColumnsFromUserSelectionCommand = new RelayCommand(OnColumnsFromUserSelection);
            DesignCommand = new RelayCommand(OnDesign, CanDesign);

            _revitFloor = new RevitLineFloor(new List<Line>(), new List<List<Line>>());

            FloorLinesOffset = 0;//cm
            BeamOffset = 50;//cm


            _cuplockVM = new RevitCuplockViewModel();
            _propVM = new RevitPropViewModel();
            _shoreVM = new RevitShoreViewModel();
            CurrentShoringVM = _cuplockVM;

            Func<Element, string> nearestAxisFunc = (e) => e.GetColumnNearestAxes(_xAxes, _yAxes);
            _nearestAxisFunc = Memoization.MemoizeWeak<ElementKey, string>(key => nearestAxisFunc(key.Element), TimeSpan.FromMinutes(2));

            IsColumnsWithConstantOffset = true;
            IsModelFromDetailLines = false;
        }




        #endregion

        #region Methods

        private bool CanDesign() =>
            _selectedSupportedFloor != null &&
            MainBeamLengths.Count > 0 &&
            SecondaryBeamLengths.Count > 0 &&
            _designResultFunc != null;


        private void OnDesign()
        {
           _designResultFunc().Match(_showErrors,tuple=> 
           {
               (var secBeamspacing, var designResult) = tuple;
               var dialogResult = _designResultService(designResult);
               if (dialogResult == DesignResultDialog.ACCEPT)
               {
                   SecondaryBeamsSpacing = secBeamspacing;
               }
               return Unit();
           });
        }


        private Validation<Tuple<double, DesignResultViewModel>> DesignShoreBrace()
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
                                                    slabThicknessCm);

            var designer = ShoreBraceDesigner.Instance;

            Func<FrameDesignOutput, Tuple<double, DesignResultViewModel>> getOutput = designOutput =>
            {
                var designResult = new DesignResultViewModel()
                {
                    PlywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList()),
                    SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", designOutput.SecondaryBeam.Item2.ToList()),
                    MainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", designOutput.MainBeam.Item2.ToList()),
                    ShoringSystemDesignOutput = new ShoringDesignOutput("Shore Brace System", new List<DesignReport>() { designOutput.Shoring.Item2 })
                };
                return Tuple.Create(designOutput.Plywood.Item1.Span, designResult);
            };

            return designer.Design(designInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction)
                           .Map(getOutput);
        }

        private Validation<Tuple<double, DesignResultViewModel>> DesignProps()
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
                                                    slabThicknessCm);
            var designer = EuropeanPropDesigner.Instance;
            Func<PropDesignOutput, Tuple<double, DesignResultViewModel>> getOutput = designOutput =>
            {
                var designResult = new DesignResultViewModel()
                {
                    PlywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList()),
                    SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", designOutput.SecondaryBeam.Item2.ToList()),
                    MainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", designOutput.MainBeam.Item2.ToList()),
                    ShoringSystemDesignOutput = new ShoringDesignOutput("Props System", new List<DesignReport>() { designOutput.Shoring.Item2 })
                };
                return Tuple.Create(designOutput.Plywood.Item1.Span, designResult);
            };

            return designer.Design(designInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction)
                           .Map(getOutput);

        }

        private Validation<Tuple<double, DesignResultViewModel>> DesignCuplock()
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
                                                     slabThicknessCm);
            var designer = CuplockDesigner.Instance;
            Func<CuplockDesignOutput, Tuple<double, DesignResultViewModel>> getOutput = designOutput =>
            {
                var designResult = new DesignResultViewModel()
                {
                    PlywoodDesignOutput = new SectionDesignOutput($"Section: {designOutput.Plywood.Item1.Section.SectionName.GetDescription()}, Span: {designOutput.Plywood.Item1.Span} cm", designOutput.Plywood.Item2.ToList()),
                    SecondaryBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.SecondaryBeam.Item1.Section.SectionName.GetDescription()}", designOutput.SecondaryBeam.Item2.ToList()),
                    MainBeamDesignOutput = new SectionDesignOutput($"Section: {designOutput.MainBeam.Item1.Section.SectionName.GetDescription()}", designOutput.MainBeam.Item2.ToList()),
                    ShoringSystemDesignOutput = new ShoringDesignOutput("Cuplock System", new List<DesignReport>() { designOutput.Shoring.Item2 })
                };
                return Tuple.Create(designOutput.Plywood.Item1.Span, designResult);
            };

            return designer.Design(designInput, EmpiricalBeamSolver.GetStrainingActions, EmpiricalBeamSolver.GetReaction)
                           .Map(getOutput);
        }


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

        protected override bool CanModel() =>
            _selectedSupportedFloor != null &&
            _selectedHostFloor != null &&
            _revitFloor.Boundary.Count > 0 &&
            MainBeamLengths.Count > 0 &&
            SecondaryBeamLengths.Count > 0&&
            BeamOffset >=0 && 
            FloorLinesOffset >=0;


        private void OnColumnsFromUserSelection()
        {
            Action<List<Element>> updateTableInLevelAndBoundary = cols =>
            {
                Table = cols.Where(e => e.LevelId.IntegerValue == _selectedHostFloor.LevelId.IntegerValue)
                            .FilterColumnsInPolygon(_revitFloor.Boundary)
                            .Select(e => new ColumnOffsetSelectionModel(e, _nearestAxisFunc(new ElementKey(e)), isSelected: true))
                            .ToList();
            };

            _uiDoc.PickElements(Filters.ColumnFilter, "Select Columns In the same Host Floor Level.")
                  .ForEach(updateTableInLevelAndBoundary);

        }

        private bool CanSelectSelectedColumnRow() =>
            SelectedRow != null;


        private void OnSelectSelectedColumnRow() =>
             _uiDoc.Selection.SetElementIds(new List<ElementId>() { SelectedRow.Element.Id });

        private Validation<Tuple<List<ColumnOffsetSelectionModel>, RevitConcreteFloor>> GetColumnsFromFloor(FloorsKey key)
        {
            Func<View3D, Tuple<List<ColumnOffsetSelectionModel>, RevitConcreteFloor>> getColumns = view =>
            {
                var options = view.Options();
                var hostLevel = _doc.GetElement(key.HostFloor.LevelId) as Level;
                var hostFloorOffset = key.HostFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                var revitFloor = key.SupportedFloor.GetGeometry(options);
                var columns = _doc.GetColumnsInLevelWithinPolygon(hostLevel, hostFloorOffset, revitFloor.Boundary)
                                   .Select(e => new ColumnOffsetSelectionModel(e, _nearestAxisFunc(new ElementKey(e)), isSelected: IsSelectAllRows))
                                   .ToList();
                return Tuple.Create(columns, revitFloor);
            };

            return _doc.GetDefault3DView()
                       .Map(getColumns);
        }

        private void GetColumnsFromSupportedFloorCached()
        {
            Action<Tuple<List<ColumnOffsetSelectionModel>, RevitConcreteFloor>> update = tuple =>
             {
                 (var columns, var revitFloor) = tuple;
                 Table = columns;
                 _revitFloor = revitFloor;
             };

            if (_selectedSupportedFloor != null)
            {
                var floorsKey = new FloorsKey(_selectedHostFloor, _selectedSupportedFloor);
                _cachedColumnsFromFloorFunc(floorsKey)
                   .Match(_showErrors, update.ToFunc());
            }
        }

        private bool CanSelectDetailLines() =>
            IsModelFromDetailLines;


        private void OnSelectDetailLines()
        {
            Action<RevitFloor> updateTable = revitFloor =>
            {
                _revitFloor = revitFloor;
                var hostLevel = _doc.GetElement(_selectedHostFloor.LevelId) as Level;
                var hostFloorOffset = _selectedHostFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                Table = _doc.GetColumnsInLevelWithinPolygon(hostLevel, hostFloorOffset, _revitFloor.Boundary)
                                                   .Select(e => new ColumnOffsetSelectionModel(e, _nearestAxisFunc(new ElementKey(e))))
                                                   .ToList();
            };



            var result = from floorValid in _uiDoc.GetRevitFloorFromDetailLines()
                         select floorValid.Match(_showErrors, updateTable.ToFunc());

        }

        protected override void ChangeShoring(RevitFormworkSystem formworkSystem)
        {
            switch (formworkSystem)
            {
                case RevitFormworkSystem.CUPLOCK_SYSTEM:
                    CurrentShoringVM = _cuplockVM;
                    _designResultFunc = DesignCuplock;
                    if (_cuplockVM != null)
                    {
                        _cuplockVM.SelectedLedgerMain = _cuplockVM.SelectedLedgerMain;
                        _cuplockVM.SelectedLedgerSecondary = _cuplockVM.SelectedLedgerSecondary;
                    }
                    _modelAction = () => ModelFloor(ModelCuplockFormwork);
                    break;
                case RevitFormworkSystem.PROPS_SYSTEM:
                    CurrentShoringVM = _propVM;
                    _designResultFunc = DesignProps;
                    if (_propVM != null)
                    {
                        _propVM.SpacingMain = _propVM.SpacingMain;
                        _propVM.SpacingSecondary = _propVM.SpacingSecondary;
                    }
                    _modelAction = () => ModelFloor(ModelPropsFormwork);
                    break;
                case RevitFormworkSystem.SHORE_SYSTEM:
                    CurrentShoringVM = _shoreVM;
                    _designResultFunc = DesignShoreBrace;
                    if (_shoreVM != null)
                    {
                        _shoreVM.SelectedSpacingMain = _shoreVM.SelectedSpacingMain;
                        _shoreVM.SpacingSec = _shoreVM.SpacingSec;
                    }
                    _modelAction = () => ModelFloor(ModelShoreFormwork);
                    break;
            }
        }

        private void ModelFloor(Action<RevitInput> modelAction)
        {
            _uiDoc.PickLine("Please Select Main Beam Direction").Map(l =>
            {
                var columns = Table.Where(row => row.IsSelected).Select(row => Tuple.Create(row.Offset.CmToFeet(), row.Element)).ToList();
                return _doc.GetRevitFloorInput(_revitFloor, columns, _selectedHostFloor, _selectedSupportedFloor, l.Direction);
            }).Match(exp => Unit(), ri => modelAction.ToFunc()(ri));
        }

        protected override void ModelCuplockFormwork(RevitInput revitInput)
        {
            var supportedLevel = _doc.GetElement(_selectedSupportedFloor.LevelId) as Level;
            var revitFloorInput = revitInput as RevitFloorInput;
            var floorCuplockInput = new RevitFloorCuplockInput(_selectedPlywoodSection,
                                                              SelectedMainBeamSection,
                                                              SelectedMainBeamSection,
                                                              _cuplockVM.SelectedSteelType,
                                                              SelectedSecondaryBeamLength.CmToFeet(),
                                                              SecondaryBeamsSpacing.CmToFeet(),
                                                              SelectedMainBeamLength.CmToFeet(),
                                                              _cuplockVM.SelectedLedgerMain.CmToFeet(),
                                                              _cuplockVM.SelectedLedgerSecondary.CmToFeet(),
                                                              FloorLinesOffset.CmToFeet(),
                                                              BeamOffset.CmToFeet());

            Action<List<Line>, RevitCuplock> draw = (extensionBoundries, cuplock) =>
             {
                 _doc.UsingTransaction(_ => _doc.LoadCuplockFamilies(), "Load Cuplock Families");
                 _doc.UsingTransaction(_ => _doc.LoadDeckingFamilies(), "Load Decking Families");
                 _doc.UsingTransaction(_ => cuplock.FilterFromExtesnionBoundries(extensionBoundries).Draw(_doc), "Model Floor Cuplock Shoring");
             };


            var result = from extensionBoundries in IsModelFromDetailLines ? _uiDoc.GetBoundryLinesFromDetailLines() : new List<Line>()
                         select CuplockShoringHelper.FloorToCuplock(revitFloorInput, floorCuplockInput).Match(_showErrors, cuplock => draw.ToFunc()(extensionBoundries, cuplock));

        }

        protected override void ModelPropsFormwork(RevitInput revitInput)
        {
            var revitFloorInput = revitInput as RevitFloorInput;

            var floorPropsInput = new RevitFloorPropsInput(_propVM.SelectedPropType,
                                                              _selectedPlywoodSection,
                                                              SelectedMainBeamSection,
                                                              SelectedMainBeamSection,
                                                              SelectedSecondaryBeamLength.CmToFeet(),
                                                              SecondaryBeamsSpacing.CmToFeet(),
                                                              SelectedMainBeamLength.CmToFeet(),
                                                              _propVM.SpacingMain.CmToFeet(),
                                                              _propVM.SpacingSecondary.CmToFeet(),
                                                              FloorLinesOffset.CmToFeet(),
                                                              BeamOffset.CmToFeet());

            Action<List<Line>, RevitProps> draw = (extensionBoundries, props) =>
            {
                _doc.UsingTransaction(_ => _doc.LoadPropFamilies(), "Load Props Families");
                _doc.UsingTransaction(_ => _doc.LoadDeckingFamilies(), "Load Decking Families");
                _doc.UsingTransaction(_ => props.FilterFromExtesnionBoundries(extensionBoundries).Draw(_doc), "Model Floor Props Shoring");
            };



            var result = from extensionBoundries in IsModelFromDetailLines ? _uiDoc.GetBoundryLinesFromDetailLines() : new List<Line>()
                         select PropsShoringHelper.FloorToProps(revitFloorInput, floorPropsInput).Match(_showErrors, props => draw.ToFunc()(extensionBoundries, props));
        }

        protected override void ModelShoreFormwork(RevitInput revitInput)
        {
            var supportedLevel = _doc.GetElement(_selectedSupportedFloor.LevelId) as Level;
            var revitFloorInput = revitInput as RevitFloorInput;
            var floorShoreInput = new RevitFloorShoreInput(_selectedPlywoodSection,
                                                              SelectedMainBeamSection,
                                                              SelectedMainBeamSection,
                                                              SelectedSecondaryBeamLength.CmToFeet(),
                                                              SecondaryBeamsSpacing.CmToFeet(),
                                                              SelectedMainBeamLength.CmToFeet(),
                                                              _shoreVM.SelectedSpacingMain.CmToFeet(),
                                                              _shoreVM.SpacingSec.CmToFeet(),
                                                              FloorLinesOffset.CmToFeet(),
                                                              BeamOffset.CmToFeet());

            Action<List<Line>, RevitShore> draw = (extensionBoundries, shore) =>
            {
                _doc.UsingTransaction(_ => _doc.LoadShoreBraceFamilies(), "Load Shore Brace Families");
                _doc.UsingTransaction(_ => _doc.LoadDeckingFamilies(), "Load Decking Families");
                _doc.UsingTransaction(_ => shore.FilterFromExtesnionBoundries(extensionBoundries).Draw(_doc), "Model Floor Shore Brace Shoring");


            };


            var result = from extensionBoundries in IsModelFromDetailLines ? _uiDoc.GetBoundryLinesFromDetailLines() : new List<Line>()
                         select ShoreHelper.FloorToShore(revitFloorInput, floorShoreInput).Match(_showErrors, shore => draw.ToFunc()(extensionBoundries, shore));

        }


        #endregion
    }
}
