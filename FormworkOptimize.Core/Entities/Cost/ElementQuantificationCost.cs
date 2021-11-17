using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class ElementQuantificationCost
    {
        #region Properties

        public string Name { get; }

        public int Count { get; }

        public CostType CostType { get; }

        public double OptimizeUnitCost { get; }

        public double OptimizeTotalCost { get; }

        public double InitialUnitCost { get; }

        public double InitialTotalCost { get; }

        public UnitCostMeasure UnitCostMeasure { get; }


        #endregion

        #region Constructors

        public ElementQuantificationCost(string name, 
                                         int count, 
                                         double optimizeTotalCost,
                                         double optimizeUnitCost,
                                         double initialTotalCost,
                                         double initialUnitCost,
                                         UnitCostMeasure unitCostMeasure,
                                         CostType costType)
        {
            Name = name;
            Count = count;
            OptimizeUnitCost = optimizeUnitCost;
            OptimizeTotalCost = optimizeTotalCost;
            UnitCostMeasure = unitCostMeasure;
            InitialTotalCost = initialTotalCost;
            InitialUnitCost = initialUnitCost;
            CostType = costType;
        }

        #endregion

    }
}
