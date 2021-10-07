using FormworkOptimize.Core.Enums;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class RentFormworkElementCost : FormworkElementCost
    {
        public override CostType GetCostType() =>
            CostType.RENT;
       

        public override double GetDailyPrice()
        {
            return Price / NO_DAYS_PER_MONTH; 
        }


    }
}
