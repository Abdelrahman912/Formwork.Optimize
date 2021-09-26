using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities
{
    public class EuropeanPropType
    {
        #region Properties

        public EuropeanPropTypeName Name { get;}

        /// <summary>
        /// Capacity of Prop (ton).
        /// </summary>
        /// <value>
        /// Unit (ton).
        /// </value>
        public double Capacity { get;}

        #endregion

        #region Constructors

        public EuropeanPropType(EuropeanPropTypeName name, double capacity)
        {
            Name = name;
            Capacity = capacity;
        }

        #endregion
    }
}
