using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitFloorsViewModel:ViewModelBase
    {

        #region Private Fileds

        private FloorModel _selectedHostFloor;

        private FloorModel _selectedSupportedFloor;

        private readonly Document _doc;

        private readonly UIDocument _uiDoc;

        private List<FloorModel> _supportedFloors;

        #endregion

        #region Properties

        public List<FloorModel> Floors { get;}

        public FloorModel SelectedHostFloor
        {
            get=> _selectedHostFloor;
            set 
            { 
                NotifyPropertyChanged(ref _selectedHostFloor, value);
               SupportedFloors = Floors.Where(Fm=> CanShowSupportedFloor(Fm)).ToList();
                SelectedSupportedFloor = SupportedFloors.FirstOrDefault();
                Mediator.Instance.NotifyColleagues(_selectedHostFloor?.Floor, Context.HOST_FLOOR);
            }
        }

        public FloorModel SelectedSupportedFloor
        {
            get=> _selectedSupportedFloor;
            set 
            { 
                NotifyPropertyChanged(ref _selectedSupportedFloor, value);
                Mediator.Instance.NotifyColleagues(_selectedSupportedFloor?.Floor, Context.SUPPORTED_FLOOR);
            }
        }

        public ICommand SelectHostFloorCommand { get; }

        public ICommand SelectSupportedFloorCommand { get; }

        public List<FloorModel> SupportedFloors
        {
            get => _supportedFloors;
            set=>NotifyPropertyChanged(ref _supportedFloors, value);
        }

        #endregion

        #region Constructors

        public RevitFloorsViewModel()
        {
            _doc = RevitBase.Document;
            _uiDoc = RevitBase.UIDocument;
            Floors = new FilteredElementCollector(_doc).OfClass(typeof(Floor))
                                                       .Cast<Floor>()
                                                       .Where(f=>!RevitBase.PLYWOOD_FLOOR_TYPES.Any(ply=>f.Name==ply))
                                                       .Select(f=>new FloorModel(f))
                                                       .ToList();

            SelectedHostFloor = Floors.FirstOrDefault();
            Mediator.Instance.NotifyColleagues(_selectedSupportedFloor?.Floor, Context.SUPPORTED_FLOOR);
            Mediator.Instance.NotifyColleagues(_selectedHostFloor?.Floor, Context.HOST_FLOOR);
            SelectHostFloorCommand = new RelayCommand(OnSelectHostFloor);
            SelectSupportedFloorCommand = new RelayCommand(OnSelectSupportedFloor);
        }

        private void OnSelectSupportedFloor() =>
            _uiDoc.Selection.SetElementIds(new List<ElementId>() { SelectedSupportedFloor.Floor.Id });
       

        private void OnSelectHostFloor()=>
            _uiDoc.Selection.SetElementIds(new List<ElementId>() { SelectedHostFloor.Floor.Id });

        #endregion

        #region Method

        private bool CanShowSupportedFloor(FloorModel floor)
        {
            if (SelectedHostFloor == null)
                return false;

            var hostLevelElevation = (_doc.GetElement(SelectedHostFloor.Floor.LevelId) as Level).Elevation;
            var supportedLevelElevation = (_doc.GetElement(floor.Floor.LevelId) as Level).Elevation;
            return supportedLevelElevation > hostLevelElevation;
        }

        #endregion

    }
}
