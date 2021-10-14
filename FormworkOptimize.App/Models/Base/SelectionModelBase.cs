namespace FormworkOptimize.App.Models.Base
{
    public abstract class SelectionModelBase:ModelBase
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

        #endregion

        #region Constructors

        public SelectionModelBase(bool isSelected)
        {
            IsSelected = isSelected;
        }

        #endregion
    }
}
