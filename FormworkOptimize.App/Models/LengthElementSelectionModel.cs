using FormworkOptimize.App.Models.Base;

namespace FormworkOptimize.App.Models
{
    public class LengthElementSelectionModel:SelectionModelBase
    {

        #region Properties

        /// <summary>
        /// Length of Ledger in cm.
        /// </summary>
        public double Length { get; }

        public string Name { get;}

        #endregion

        #region Constructors

        public LengthElementSelectionModel(bool isSelected,double lengthInCm , string elementCategoryName)
            :base(isSelected)
        {
            Length = lengthInCm;
            Name = $"{elementCategoryName} {Length / 100.0} m";
        }

        #endregion

    }
}
