using FormworkOptimize.Core.Enums;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.Core.Entities
{
    public class EuropeanPropSystem:PropShoring
    {

        #region Properties

        public EuropeanPropType PropType { get;  }

        #endregion

        #region Constructors

        public EuropeanPropSystem(EuropeanPropTypeName propType,double mainSpacing,double secondarySpacing)
            :base(mainSpacing,secondarySpacing)
        {
            PropType = GetEuropeanPropType(propType);
        }

        #endregion

    }
}
