using FormworkOptimize.App.Models.Base;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Models
{
    public class EuropeanPropsSelectionModel: SelectionModelBase
    {
        #region Properties

        public EuropeanPropTypeName PropsType { get; }

        #endregion

        #region Constructors

        public EuropeanPropsSelectionModel(bool isSelected, EuropeanPropTypeName propsType)
            : base(isSelected)
        {
            PropsType = propsType;
        }

        #endregion
    }
}
