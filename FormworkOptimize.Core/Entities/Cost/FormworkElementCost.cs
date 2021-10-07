using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities.Cost
{
    public abstract class FormworkElementCost
    {
        public FormworkCostElements Name { get; set; }

        public double Price { get; set; }

        public UnitCostMeasure UnitCost { get; set; }

        public abstract double GetDailyPrice();

        public abstract CostType GetCostType();
    }
}
