namespace FormworkOptimize.Core.Entities.Cost
{
    public class FormworkElementsCost
    {
        
        #region Properties

        /// <summary>
        /// Daily cost of all formwork elements (LE/Day).
        /// </summary>
        public double Cost { get; }

        /// <summary>
        /// Total Duration.
        /// </summary>
        public int Duration { get; }

        public double TotalCost =>
            Cost * Duration;

        #endregion


        #region Constructors

        public FormworkElementsCost(double cost, int duration)
        {
            Cost = cost;
            Duration = duration;
        }

        #endregion




    }
}
