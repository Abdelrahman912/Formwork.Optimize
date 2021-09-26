namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class CostParameter
    {
        #region Properties

        public ManPower ManPower { get; }

        public Equipments Equipments { get; }

        public Time Time { get;  }

        public Transportation Transportation { get; }

        #endregion

        #region Constructors

        public CostParameter(ManPower manPower, Equipments equipments, Time time,Transportation transportation)
        {
            ManPower = manPower;
            Equipments = equipments;
            Time = time;
            Transportation = transportation;
        }

        #endregion

    }
}
