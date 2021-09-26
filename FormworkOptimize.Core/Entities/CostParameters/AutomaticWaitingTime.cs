using FormworkOptimize.Core.Entities.CostParameters.Interfaces;
using System;

namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class AutomaticWaitingTime : ICalculateWaitingTime
    {

        #region Properties

        /// <summary>
        /// Small Length of the floor in meter.
        /// </summary>
        public double SmallerFloorLength { get;}

        #endregion

        #region Constructors

        public AutomaticWaitingTime(double smallerFloorLength)
        {
            SmallerFloorLength = smallerFloorLength;
        }

        #endregion

        #region Methods

        public int CalculateWaitingTime()
        {
            return (int)Math.Ceiling(2*SmallerFloorLength + 2);
        }

        #endregion

    }
}
