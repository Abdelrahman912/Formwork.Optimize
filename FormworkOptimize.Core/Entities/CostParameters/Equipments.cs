namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class Equipments
    {
        #region Properties

        public int NoCranes { get;  }

        public double CraneRent { get;}

        #endregion

        #region Constructors

        public Equipments(int noCranes, double craneRent)
        {
            NoCranes = noCranes;
            CraneRent = craneRent;
        }

        #endregion

    }
}
