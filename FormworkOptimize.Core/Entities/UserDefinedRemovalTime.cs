using FormworkOptimize.Core.Entities.CostParameters.Interfaces;

namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class UserDefinedRemovalTime : ICalculateRemovalTime
    {
        #region Properties

        public int RemovalTime { get; }

        #endregion

        #region Constructors

        public UserDefinedRemovalTime(int removalTime)
        {
            RemovalTime = removalTime;
        }

        #endregion

        #region Methods

        public int CalculateRemovalTime() =>
            RemovalTime;
        

        #endregion

    }
}
