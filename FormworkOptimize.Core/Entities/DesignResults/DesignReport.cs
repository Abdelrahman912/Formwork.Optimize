using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities
{
    public class DesignReport
    {

        #region Properties

        public DesignCheckName CheckName { get; }

        public double Allowable { get; }

        public double Actual { get; }

        public DesignStatus Status { get; }

        public double DesignRatio { get;}

        #endregion

        #region Constructors

        internal DesignReport(DesignCheckName checkName, double allowable, double actual)
        {
            CheckName = checkName;
            Allowable = allowable;
            Actual = actual;
            DesignRatio = Actual / Allowable;
            Status = Allowable >= Actual ? DesignStatus.SAFE : DesignStatus.UNSAFE;
        }

        #endregion

    }
}
