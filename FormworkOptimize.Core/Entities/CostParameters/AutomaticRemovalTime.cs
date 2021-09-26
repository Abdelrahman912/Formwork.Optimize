using FormworkOptimize.Core.Entities.CostParameters.Interfaces;
using System;

namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class AutomaticRemovalTime : ICalculateRemovalTime
    {
        #region Properties

        public ICalculateInstallationTime InstallationTime { get; }

        #endregion

        #region Constructors

        public AutomaticRemovalTime(ICalculateInstallationTime installationTime)
        {
            InstallationTime = installationTime;
        }

        #endregion

        #region Methods

        public int CalculateRemovalTime()
        {
            return (int)Math.Ceiling(0.5 * InstallationTime.CalculateInstallationTime());
        }

        #endregion


    }
}
