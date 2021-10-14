using FormworkOptimize.App.Models.Base;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Models
{
    public class SteelTypeSelectionModel:SelectionModelBase
    {
        #region Properties

        public SteelType SteelType { get; }

        #endregion

        #region Constructors

        public SteelTypeSelectionModel(bool isSelected, SteelType steelType)
            : base(isSelected)
        {
            SteelType = steelType;
        }

        #endregion
    }
}
