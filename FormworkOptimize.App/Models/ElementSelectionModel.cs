using Autodesk.Revit.DB;
using FormworkOptimize.App.ViewModels.Base;

namespace FormworkOptimize.App.Models
{
    public class ElementSelectionModel:ViewModelBase
    {

        #region Private Fields

        private bool _isSelected;

        #endregion

        #region Properties

        public Element Element { get;}

        public string Name { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => NotifyPropertyChanged(ref _isSelected,value);
        }

        public string Axes { get; }

        #endregion

        #region Constructors

        public ElementSelectionModel(Element ele,string axes,bool isSelected = false)
        {
            Element = ele;
            Name = ele.Name;
            IsSelected = isSelected;
            Axes = axes;
        }

        #endregion

    }
}
