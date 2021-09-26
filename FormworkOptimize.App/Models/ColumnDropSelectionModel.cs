using Autodesk.Revit.DB;

namespace FormworkOptimize.App.Models
{
    public class ColumnDropSelectionModel:ElementSelectionModel
    {

        #region Private Fields

        private bool _isDrop;

        #endregion

        #region Properties

        public bool IsDrop
        {
            get => _isDrop;
            set => NotifyPropertyChanged(ref _isDrop ,value);
        }

        /// <summary>
        /// Minimum Offset from column edge in cm.
        /// </summary>
        public double MaxOffset { get; set; }

        #endregion

        #region Constructors

        public ColumnDropSelectionModel(Element ele,string axes,bool isDrop=false , double maxOffset=50, bool isSelected = false)
            :base(ele,axes,isSelected)
        {
            IsDrop = isDrop;
            MaxOffset = maxOffset;
        }

        #endregion

    }
}
