namespace FormworkOptimize.Core.Entities.Cost
{
    public class EquipmentsCost
    {

        #region Properties

        public int NoEquipments { get; }

        /// <summary>
        /// Daily Rent (LE/Day).
        /// </summary>
        public double Rent { get; }

        /// <summary>
        /// Total Duration.
        /// </summary>
        public int Duration { get; }

        public double TotalCost =>
            NoEquipments * Rent * Duration;

        #endregion

        #region Constructors

        public EquipmentsCost(int noEquipments, double rent, int duration)
        {
            NoEquipments = noEquipments;
            Rent = rent;
            Duration = duration;
        }

        #endregion

    }
}
