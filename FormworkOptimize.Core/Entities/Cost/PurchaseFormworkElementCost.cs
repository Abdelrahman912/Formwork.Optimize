using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class PurchaseFormworkElementCost:FormworkElementCost
    {
        public int NumberOfUses { get; set; }

        public override CostType GetCostType() =>
            CostType.PURCHASE;

        public override double GetInitialCost()
        {
            return Price;
        }

        public override double GetOptimizationCost()
        {
            if (NumberOfUses == 0)
                return Price;
            else
               return Price / NumberOfUses;
        }
    }
}
