using FormworkOptimize.Core.Entities.CostParameters.Interfaces;

namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class UserDefinedNoWorkers : ICalculateNoWorkers
    {

        #region Properties

        public int NoWorkers { get; }

        #endregion

        #region Constructors

        public UserDefinedNoWorkers(int noWorkers)
        {
            NoWorkers = noWorkers;
        }

        #endregion

        #region Methods

        public int CalculateNoWorkers() =>
            NoWorkers;

        #endregion

    }
}
