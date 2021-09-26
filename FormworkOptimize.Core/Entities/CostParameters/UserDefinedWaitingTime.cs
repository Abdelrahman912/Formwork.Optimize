using FormworkOptimize.Core.Entities.CostParameters.Interfaces;
using System;

namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class UserDefinedWaitingTime : ICalculateWaitingTime
    {

        #region Properties

        public int WaitingTime { get; }

        #endregion

        #region Constructors

        public UserDefinedWaitingTime(int waitingTime)
        {
            WaitingTime = waitingTime;
        }

        #endregion

        #region Methods

        public int CalculateWaitingTime() =>
            WaitingTime;
       

        #endregion


    }
}
