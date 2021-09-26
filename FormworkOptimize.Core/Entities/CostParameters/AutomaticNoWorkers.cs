using FormworkOptimize.Core.Entities.CostParameters.Interfaces;
using System;

namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class AutomaticNoWorkers : ICalculateNoWorkers
    {

        #region Properties

        /// <summary>
        /// Area of the floor in m^2
        /// </summary>
        public double FloorArea { get; }

        #endregion

        #region Constructors

        public AutomaticNoWorkers(double floorArea)
        {
            FloorArea = floorArea;
        }

        #endregion

        #region Methods

        public int CalculateNoWorkers()
        {
            //for 1000 meter square there is 20 workers / day
            var nWorkers = Math.Max((int)Math.Ceiling((FloorArea / 1000.0) * 20),4);
            return nWorkers;
        }

        #endregion

    }
}
