using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.RevitExternalEvents;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FormworkOptimize.App.ViewModels
{
    public abstract class RevitModelFormworkViewModel<T> : ViewModelBase
        where T : ElementSelectionModel
    {

        #region Private Fields

        protected readonly Document _doc;

        protected readonly UIDocument _uiDoc;

        protected PlywoodSectionName _selectedPlywoodSection;

        protected RevitBeamSectionName _selectedMainBeamSection;

        protected RevitBeamSectionName _selectedSecondaryBeamSection;

        protected Floor _selectedHostFloor;

        protected Floor _selectedSupportedFloor;

        protected double _secondaryBeamsSpacing;

        protected Action _modelAction;

        protected RevitFormworkSystem _selectedFormworkSystem;

        protected readonly IEnumerable<Tuple<string, Line>> _xAxes;

        protected readonly IEnumerable<Tuple<string, Line>> _yAxes;

        private List<T> _table;

        private T _selectedRow;

        private bool _isSelectAllRows;

        #endregion

        #region Properties

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
                NotifyPropertyChanged(ref _isSelectAllRows, value);
                Table.ForEach(row => row.IsSelected = _isSelectAllRows);
            }
        }

        public List<T> Table
        {
            get => _table;
            set => NotifyPropertyChanged(ref _table, value);
        }


        public T SelectedRow
        {
            get => _selectedRow;
            set => NotifyPropertyChanged(ref _selectedRow, value);
        }

        public RevitFloorsViewModel FloorsVM { get; }

        public virtual RevitBeamSectionName SelectedMainBeamSection
        {
            get => _selectedMainBeamSection;
            set => NotifyPropertyChanged(ref _selectedMainBeamSection, value);
        }

        public virtual RevitBeamSectionName SelectedSecondaryBeamSection
        {
            get => _selectedSecondaryBeamSection;
            set => NotifyPropertyChanged(ref _selectedSecondaryBeamSection, value);
        }

        public double SecondaryBeamsSpacing
        {
            get => _secondaryBeamsSpacing;
            set => NotifyPropertyChanged(ref _secondaryBeamsSpacing, value);
        }

        public ICommand ModelCommand { get; }


        public RevitFormworkSystem SelectedFormworkSystem
        {
            get => _selectedFormworkSystem;
            set
            {
                NotifyPropertyChanged(ref _selectedFormworkSystem, value);
                ChangeShoring(_selectedFormworkSystem);
            }
        }

        public List<RevitFormworkSystem> AvailableFormworkSystems { get; }

        #endregion


        #region Constructors

        public RevitModelFormworkViewModel(RevitFloorsViewModel floorsVM, List<RevitFormworkSystem> availableFormworkSystems)
        {
            _doc = RevitBase.Document;
            _uiDoc = RevitBase.UIDocument;
            FloorsVM = floorsVM;
            _selectedHostFloor = FloorsVM.SelectedHostFloor?.Floor;
            _selectedSupportedFloor = FloorsVM.SelectedSupportedFloor?.Floor;

            Table = new List<T>();

            SelectedSecondaryBeamSection = RevitBeamSectionName.TIMBER_H20;
            SelectedMainBeamSection = RevitBeamSectionName.TIMBER_H20;
            SecondaryBeamsSpacing = 50; //Cm

            AvailableFormworkSystems = availableFormworkSystems;
            SelectedFormworkSystem = AvailableFormworkSystems.First();
            ModelCommand = new RelayCommand(OnModel, CanModel);

            (_xAxes, _yAxes) = _doc.GetXYAxes();

            IsSelectAllRows = true;

        }


        #endregion


        #region Methods

        protected virtual bool CanModel() =>
            true;


        private void OnModel()
        {
            ExternalEventHandler.Instance.Raise(_ => _doc.UsingTransactionGroup(tg => _modelAction(), "Model Shoring"),
                                               () => { },
                                               err => TaskDialog.Show("Error", err));
        }

        protected abstract void ChangeShoring(RevitFormworkSystem selectedFormworkSystem);

        protected abstract void ModelCuplockFormwork(RevitInput revitInput);

        protected abstract void ModelPropsFormwork(RevitInput revitInput);

        protected abstract void ModelShoreFormwork(RevitInput revitInput);

        #endregion

    }
}
