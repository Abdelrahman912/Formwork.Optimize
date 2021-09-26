namespace FormworkOptimize.Core.Entities.Cost
{
    public class ManPowerCost
    {
        #region Properties

        public int NoWorkers { get; }

        /// <summary>
        /// Labor daily cost (LE/Day).
        /// </summary>
        public double LaborCost { get;}

        /// <summary>
        /// Install + Removal.
        /// </summary>
        public int Duration { get;}

        public double TotalCost =>
            NoWorkers * Duration * LaborCost;

        #endregion

        #region Constructors

        public ManPowerCost(int noWorkers, double laborCost, int duration)
        {
            NoWorkers = noWorkers;
            LaborCost = laborCost;
            Duration = duration;
        }

        #endregion

    }
}
