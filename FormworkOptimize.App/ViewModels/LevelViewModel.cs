using Autodesk.Revit.DB;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;

namespace FormworkOptimize.App.ViewModels
{
    public class LevelViewModel:ViewModelBase
    {

        #region Private Fields

        private bool _isSelected;

        #endregion

        #region Properties

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                NotifyPropertyChanged(ref _isSelected,value);
                Mediator.Instance.NotifyColleagues(this, Context.LEVEL_SELECTED);
            }
        }

        public string Name { get;}

        public Level Level { get; }

        #endregion

        #region Constructors

        public LevelViewModel(bool isSelected, string name, Level level)
        {
            IsSelected = isSelected;
            Name = name;
            Level = level;
        }

        #endregion

    }
}
