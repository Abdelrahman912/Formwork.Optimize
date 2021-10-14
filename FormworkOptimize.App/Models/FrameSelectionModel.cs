using FormworkOptimize.App.Models.Base;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Models
{
    public class FrameSelectionModel:SelectionModelBase
    {
        #region Properties

        public FrameTypeName FrameType { get; }

        #endregion

        #region Constructors

        public FrameSelectionModel(bool isSelected, FrameTypeName frameType)
            : base(isSelected)
        {
            FrameType = frameType;
        }

        #endregion
    }
}
