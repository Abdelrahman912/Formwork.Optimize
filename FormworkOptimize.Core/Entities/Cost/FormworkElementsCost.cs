namespace FormworkOptimize.Core.Entities.Cost
{
    public class FormworkElementsCost
    {
        
        #region Properties

        /// <summary>
        /// Daily cost of all rent formwork elements (LE/Day).
        /// </summary>
        public double RentCost { get; }

        public double PurchaseCost { get;  }

        /// <summary>
        /// Total Duration.
        /// </summary>
        public int Duration { get; }

        public double TotalCost { get; }
           

        #endregion


        #region Constructors

        public FormworkElementsCost(double rentCost, int duration,double purchaseCost)
        {
            RentCost = rentCost;
            Duration = duration;
            PurchaseCost = purchaseCost;
            TotalCost = RentCost * Duration + PurchaseCost;
        }

        #endregion




    }
}
