using FormworkOptimize.Core.Enums;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class RentFormworkElementCost : FormworkElementCost
    {
        public override CostType GetCostType() =>
            CostType.RENT;

        public override double GetInitialCost()
        {
            return Price / NO_DAYS_PER_MONTH;
        }

        public override double GetOptimizationCost()
        {
            return Price / NO_DAYS_PER_MONTH; 
        }


    }
}
