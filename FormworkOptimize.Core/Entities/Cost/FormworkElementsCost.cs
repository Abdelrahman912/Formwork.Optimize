namespace FormworkOptimize.Core.Entities.Cost
{
    public class FormworkElementsCost
    {
        
        #region Properties

        /// <summary>
        /// Daily cost of all rent formwork elements (LE/Day).
        /// </summary>
        public double RentCost { get; }

        public double OptimizePurchaseCost { get;  }

        public double InitialPurchaseCost { get; }

        /// <summary>
        /// Total Duration.
        /// </summary>
        public int Duration { get; }

        public double OptimizeTotalCost { get; }

        public double InitialTotalCost { get; }

        #endregion


        #region Constructors

        public FormworkElementsCost(double rentCost, int duration,double optimizePurchaseCost,double initialPurchaseCost)
        {
            RentCost = rentCost;
            Duration = duration;
            OptimizePurchaseCost = optimizePurchaseCost;
            InitialPurchaseCost = initialPurchaseCost;
            OptimizeTotalCost = RentCost * Duration + OptimizePurchaseCost;
            InitialTotalCost = RentCost *Duration + InitialPurchaseCost;
        }

        #endregion




    }
}
