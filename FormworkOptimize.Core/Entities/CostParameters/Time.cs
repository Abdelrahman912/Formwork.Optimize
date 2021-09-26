using FormworkOptimize.Core.Entities.CostParameters.Interfaces;

namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class Time
    {
        #region Properties

        public ICalculateInstallationTime InstallationTime { get; }

        public int SmitheryTime { get;}

        public ICalculateWaitingTime WaitingTime { get; }

        public ICalculateRemovalTime RemovalTime { get;}

        #endregion

        #region Constrcutors

        public Time(int smitheryTime, 
                    ICalculateWaitingTime waitingTime, 
                    ICalculateRemovalTime removalTime,
                    ICalculateInstallationTime installationTime)
        {
            SmitheryTime = smitheryTime;
            WaitingTime = waitingTime;
            RemovalTime = removalTime;
            InstallationTime = installationTime;
        }

        #endregion

    }
}
