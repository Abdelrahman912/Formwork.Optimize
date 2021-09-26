using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitElementSelectionViewModel : ViewModelBase
    {

        #region Private Fields

        private bool _isFloorSelected;

        private bool _isColumnsSelected;

        private bool _isBeamsSelected;

        private RevitFormworkSystem _selectedFloorFormwork;

        private RevitFormworkSystem _selectedColumnsFormwork;

        private RevitFormworkSystem _selectedBeamsFormwork;

        #endregion

        #region Properties

        public bool IsFloorSelected
        {
            get => _isFloorSelected;
            set => NotifyPropertyChanged(ref _isFloorSelected, value);
        }

        public bool IsColumnsSelected
        {
            get => _isColumnsSelected;
            set => NotifyPropertyChanged(ref _isColumnsSelected, value);
        }

        public bool IsBeamsSelected
        {
            get => _isBeamsSelected;
            set => NotifyPropertyChanged(ref _isBeamsSelected, value);
        }

        public RevitFormworkSystem SelectedFloorFormwork
        {
            get => _selectedFloorFormwork;
            set
            {
                NotifyPropertyChanged(ref _selectedFloorFormwork, value);
                Mediator.Instance.NotifyColleagues(_selectedFloorFormwork, Context.FLOOR_SHORING_SYSTEM);
            }
        }

        public RevitFormworkSystem SelectedColumnsFormwork
        {
            get => _selectedColumnsFormwork;
            set
            {
                NotifyPropertyChanged(ref _selectedColumnsFormwork, value);
                Mediator.Instance.NotifyColleagues(_selectedColumnsFormwork, Context.COLUMNS_SHORING_SYSTEM);
            }
        }

        public RevitFormworkSystem SelectedBeamsFormwork
        {
            get => _selectedBeamsFormwork;
            set
            {
                NotifyPropertyChanged(ref _selectedBeamsFormwork, value);
                Mediator.Instance.NotifyColleagues(_selectedBeamsFormwork, Context.BEAMS_SHORING_SYSTEM);
            }
        }

        public List<RevitFormworkSystem> AvailableFloorFormworkSystems { get; set; }

        public List<RevitFormworkSystem> AvailableColumnsFormworkSystems { get; set; }

        public List<RevitFormworkSystem> AvailableBeamsFormworkSystems { get; set; }

        #endregion

        #region Constructors

        public RevitElementSelectionViewModel()
        {
            AvailableFloorFormworkSystems = new List<RevitFormworkSystem>() { RevitFormworkSystem.CUPLOCK_SYSTEM, RevitFormworkSystem.PROPS_SYSTEM, RevitFormworkSystem.SHORE_SYSTEM };
            AvailableBeamsFormworkSystems = new List<RevitFormworkSystem>() { RevitFormworkSystem.CUPLOCK_SYSTEM, RevitFormworkSystem.PROPS_SYSTEM };
            AvailableColumnsFormworkSystems = new List<RevitFormworkSystem>() { RevitFormworkSystem.CUPLOCK_SYSTEM, RevitFormworkSystem.PROPS_SYSTEM };
            SelectedFloorFormwork = AvailableFloorFormworkSystems.First();
            SelectedColumnsFormwork = AvailableColumnsFormworkSystems.First();
            SelectedBeamsFormwork = AvailableBeamsFormworkSystems.First();
            IsFloorSelected = true;
            IsColumnsSelected = true;
            IsBeamsSelected = true;
        }

        #endregion

    }
}
