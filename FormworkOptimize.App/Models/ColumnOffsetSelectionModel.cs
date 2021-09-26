using Autodesk.Revit.DB;

namespace FormworkOptimize.App.Models
{
    public class ColumnOffsetSelectionModel:ElementSelectionModel
    {

        #region Private Fields

        private double _offset;

        #endregion

        #region Properties

        public double Offset
        {
            get => _offset;
            set=>NotifyPropertyChanged(ref _offset, value); 
        }


        #endregion

        #region Constructors

        public ColumnOffsetSelectionModel(Element e,string axes,bool isSelected = false)
            :base(e,axes,isSelected)
        {
            Offset = 0;
        }

        #endregion

    }
}
