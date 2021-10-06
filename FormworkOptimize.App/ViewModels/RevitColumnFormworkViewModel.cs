using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.Utils;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.DTOS.Revit.Input;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.DTOS.Revit.Input.Props;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.RevitHelper;
using FormworkOptimize.Core.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using static FormworkOptimize.App.Utils.Memoization;
using static CSharp.Functional.Extensions.ValidationExtension;
using Unit = System.ValueTuple;
using static CSharp.Functional.Functional;
using CSharp.Functional.Errors;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitColumnFormworkViewModel : RevitModelFormworkViewModel<ColumnDropSelectionModel>
    {

        #region Private Fields

        private object _currentShoringVM;

        private double _columnEdgeOffset;

        private bool _isModelFromColumns;

        private readonly Func<FloorsKey, Validation<List<ColumnDropSelectionModel>>> _cachedColumnsFromFloorFunc;

        private readonly Func<ElementKey, string> _nearestAxisFunc;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        private RevitPropViewModel _propVM;

        #endregion

        #region Properties

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



        public bool IsModelFromColumns
        {
            get => _isModelFromColumns;
            set
            {
                NotifyPropertyChanged(ref _isModelFromColumns, value);
                if (!_isModelFromColumns)
                    GetColumnsFromFloorCached();
                else
                    Table = new List<ColumnDropSelectionModel>();
            }
        }


        public ICommand SelectColumnsCommand { get; }

        public ICommand SelectColumnRowCommand { get; }


        #endregion

        #region Constructors

        public RevitColumnFormworkViewModel(RevitFloorsViewModel floorsVM,
                                            Func<List<ResultMessage>, Unit> notificationService)
            : base(floorsVM, new List<RevitFormworkSystem>() { RevitFormworkSystem.CUPLOCK_SYSTEM, RevitFormworkSystem.PROPS_SYSTEM })
        {

            _notificationService = notificationService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());

            Mediator.Instance.Subscribe<PlywoodSectionName>(this, (section) => _selectedPlywoodSection = section, Context.PLYWOOD_SECTION);
            Mediator.Instance.Subscribe<Floor>(this, (hostFloor) => { _selectedHostFloor = hostFloor; if (!IsModelFromColumns) GetColumnsFromFloorCached(); }, Context.HOST_FLOOR);
            Mediator.Instance.Subscribe<Floor>(this, (supportedFloor) => { _selectedSupportedFloor = supportedFloor; if (!IsModelFromColumns) GetColumnsFromFloorCached(); }, Context.SUPPORTED_FLOOR);

            SelectColumnsCommand = new RelayCommand(OnSelectColumns, CanSelectColumns);
            SelectColumnRowCommand = new RelayCommand(OnSelectColumnRow, CanSelectColumnRow);

            _cachedColumnsFromFloorFunc = Memoization.MemoizeWeak<FloorsKey, List<ColumnDropSelectionModel>>(key => GetColumnsFromFloor(key), TimeSpan.FromMinutes(2));

            _propVM = new RevitPropViewModel(false);

            Func<Element, string> nearestAxisFunc = (e) => e.GetColumnNearestAxes(_xAxes, _yAxes);
            _nearestAxisFunc = Memoization.MemoizeWeak<ElementKey, string>(key => nearestAxisFunc(key.Element), TimeSpan.FromMinutes(2));

            IsModelFromColumns = false;
        }


        #endregion

        #region Methods

        protected override bool CanModel() =>
            _selectedHostFloor != null && _selectedHostFloor != null && Table.Any(c => c.IsSelected);

        /// <summary>
        /// Get Columns In the Host floor Level and within geometry of Supported Floor
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        private Validation<List<ColumnDropSelectionModel>> GetColumnsFromFloor(FloorsKey key)
        {
            Func<View3D, List<ColumnDropSelectionModel>> getColumns = view =>
            {
                var options = view.Options();
                var hostLevel = _doc.GetElement(key.HostFloor.LevelId) as Level;
                var hostFloorOffset = key.HostFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                var revitFloor = key.SupportedFloor.GetGeometry(options);
                return _doc.GetColumnsInLevelWithinPolygon(hostLevel, hostFloorOffset, revitFloor.Boundary)
                                   .Select(e => new ColumnDropSelectionModel(e, _nearestAxisFunc(new ElementKey(e)), isSelected: IsSelectAllRows))
                                   .ToList();
            };

          return  _doc.GetDefault3DView()
                      .Map(getColumns);

        }
        
        private void GetColumnsFromFloorCached()
        {
            if (_selectedSupportedFloor is null || _selectedHostFloor == null)
                return;
            var floorKey = new FloorsKey(_selectedHostFloor,_selectedSupportedFloor);
            Action<List<ColumnDropSelectionModel>> updateTable=columns =>
                        Table = columns; 
             _cachedColumnsFromFloorFunc(floorKey)
                .Match(_showErrors,updateTable.ToFunc());
        }

        private bool CanSelectColumnRow() =>
            SelectedRow != null;


        private void OnSelectColumnRow()
        {
            _uiDoc.Selection.SetElementIds(new List<ElementId>() { SelectedRow.Element.Id });
        }

        private bool CanSelectColumns() =>
            IsModelFromColumns;


        private void OnSelectColumns()
        {
            Action<List<Element>> updateTableInSameLevel = cols =>
            {
                Table = cols.Where(e => e.LevelId.IntegerValue == _selectedHostFloor.LevelId.IntegerValue)
                            .Select(e => new ColumnDropSelectionModel(e, _nearestAxisFunc(new ElementKey(e)), isSelected: false))
                            .ToList();
            };

            _uiDoc.PickElements(Filters.ColumnFilter, "Select Columns in the same Host Floor Level.")
                  .ForEach(updateTableInSameLevel);
        }


        protected override void ChangeShoring(RevitFormworkSystem formworkSystem)
        {
            switch (formworkSystem)
            {
                case RevitFormworkSystem.CUPLOCK_SYSTEM:
                    CurrentShoringVM = null;
                    _modelAction =()=>ModelColumn( ModelCuplockFormwork);
                    break;
                case RevitFormworkSystem.PROPS_SYSTEM:
                    CurrentShoringVM = _propVM;
                    _modelAction =()=>ModelColumn( ModelPropsFormwork);
                    break;
                case RevitFormworkSystem.SHORE_SYSTEM:
                    CurrentShoringVM = null;
                    throw new Exception("Not Supported For Columns");
            }
        }


        private void ModelColumn(Action<RevitInput> modelAction)
        {
            var columnsWDrop = Table.Where(c => c.IsSelected && c.IsDrop).Select(row => row.Element).ToList();
            var columnsWNoDrop = Table.Where(c => c.IsSelected && !c.IsDrop).Select(row => Tuple.Create(row.Element, row.MaxOffset.CmToFeet())).ToList();

            var columnInput = _doc.GetRevitColumnInput(columnsWDrop, columnsWNoDrop, _selectedHostFloor, _selectedSupportedFloor);
            columnInput.Map(modelAction.ToFunc());
        }

        protected override void ModelCuplockFormwork(RevitInput revitInput)
        {
            var revitColumnInput = revitInput as RevitColumnInput;
            var columnCuplockInput = new RevitColumnCuplockInput(_selectedPlywoodSection,
                                                               SelectedMainBeamSection,
                                                               SelectedSecondaryBeamSection,
                                                               SecondaryBeamsSpacing.CmToFeet());
            Action<List<RevitCuplock>> draw = revitCuplocks =>
            {
                _doc.UsingTransaction(_ => _doc.LoadCuplockFamilies(), "Load Cuplock Families");
                _doc.UsingTransaction(_ => _doc.LoadDeckingFamilies(), "Load Decking Families");
                _doc.UsingTransaction(_ => revitCuplocks.Draw(_doc), "Model Columns Cuplock Shoring");
            };


         
            _doc.UsingTransaction(_ => _doc.LoadDeckingFamilies(), "Load Decking Families");

           CuplockShoringHelper.ColumnsToCuplock(revitColumnInput, columnCuplockInput)
                               .Match(_showErrors, draw.ToFunc());
            
        }

        protected override void ModelPropsFormwork(RevitInput revitInput)
        {
           var revitColumnInput = revitInput as RevitColumnInput;
            var beamPropsInput = new RevitColumnPropsInput(_propVM.SelectedPropType,
                                                           _selectedPlywoodSection,
                                                           SelectedMainBeamSection,
                                                           SelectedSecondaryBeamSection,
                                                           SecondaryBeamsSpacing.CmToFeet());

            Action<List<RevitProps>> draw = revitProps =>
            {
                _doc.UsingTransaction(_ => _doc.LoadPropFamilies(), "Load Props Families");
                _doc.UsingTransaction(_ => _doc.LoadDeckingFamilies(), "Load Decking Families");
                _doc.UsingTransaction(_ => revitProps.Draw(_doc), "Model Columns Props Shoring");

            };

            PropsShoringHelper.ColumnsToProps(revitColumnInput, beamPropsInput)
                      .Match(_showErrors, draw.ToFunc());

        }

        protected override void ModelShoreFormwork(RevitInput revitinput)
        {
            throw new NotImplementedException();
        }


        #endregion


    }
}
