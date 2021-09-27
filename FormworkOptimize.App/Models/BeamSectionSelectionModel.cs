using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Models
{
    public class BeamSectionSelectionModel : ViewModelBase
    {

        #region Private Fields

        private bool _isSelected;

        #endregion

        #region Properties

        public bool IsSelected
        {
            get => _isSelected;
            set => NotifyPropertyChanged(ref _isSelected, value);
        }

        public BeamSectionName BeamSection { get; }

        #endregion

        #region Constructors

        public BeamSectionSelectionModel(bool isSelected, BeamSectionName beamSection)
        {
            IsSelected = isSelected;
            BeamSection = beamSection;
        }

        #endregion

    }
}
