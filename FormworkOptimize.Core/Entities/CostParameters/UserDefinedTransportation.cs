namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class UserDefinedTransportation:Transportation
    {
        #region Properties

        /// <summary>
        /// Transportation Cost (LE).
        /// </summary>
        public double Cost { get; }

        #endregion

        #region Constructors

        public UserDefinedTransportation(double cost)
        {
            Cost = cost;
        }

        public override double GetCost() =>
            Cost;
        

        #endregion

    }
}
