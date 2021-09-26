using FormworkOptimize.Core.Enums;
using static FormworkOptimize.Core.Helpers.DesignHelper.ShoringHelper;

namespace FormworkOptimize.Core.Entities
{
    public class CuplockShoring : Shoring
    {

        #region Properties

        /// <summary>
        /// Critical un-braced length (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double Lcr { get; }

        /// <summary>
        /// Length of ledger in main beam direction (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double LedgerMainDir { get; }

        /// <summary>
        /// Length of ledger in secondary beam direction (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double LedgerSecDir { get; }

        public SteelType SteelType { get; }

        public double Capacity { get; }

        #endregion

        #region Constructors

        public CuplockShoring(double lcr, double ledgerX, double ledgerY, SteelType steelType)
        {
            Lcr = lcr;
            LedgerMainDir = ledgerX;
            LedgerSecDir = ledgerY;
            Capacity = GetCuplockCapacity(lcr, steelType);
        }

        #endregion
    }
}
