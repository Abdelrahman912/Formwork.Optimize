using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities.Cost
{
    public abstract class FormworkElementCost
    {
        public FromworkCostElements Name { get; set; }

        public double Price { get; set; }

        public UnitCostMeasure UnitCost { get; set; }

    }
}
