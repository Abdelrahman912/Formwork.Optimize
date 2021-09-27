using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Models
{
    public class PlywoodSelectionModel:ViewModelBase
    {

        #region Private Fields

        private bool _isSelected;

        #endregion

        #region Properties

        public bool IsSelected
        {
            get=> _isSelected;
            set=>NotifyPropertyChanged(ref _isSelected,value);
        }

        public PlywoodSectionName Plywood { get;}

        #endregion

        #region Constructors

        public PlywoodSelectionModel(bool isSelected, PlywoodSectionName plywood)
        {
            IsSelected = isSelected;
            Plywood = plywood;
        }

        #endregion

    }
}
