using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class ElementQuantificationCost
    {
        #region Properties

        public string Name { get; }

        public int Count { get; }

        public double TotalCost { get; }

        public double UnitCost { get;}

        public UnitCostMeasure UnitCostMeasure { get; }

        public CostType CostType { get;  }

        #endregion

        #region Constructors

        public ElementQuantificationCost(string name, 
                                         int count, 
                                         double totalCost, 
                                         double unitCost, 
                                         UnitCostMeasure unitCostMeasure,
                                         CostType costType)
        {
            Name = name;
            Count = count;
            TotalCost = totalCost;
            UnitCost = unitCost;
            UnitCostMeasure = unitCostMeasure;
            CostType = costType;
        }

        #endregion

    }
}
