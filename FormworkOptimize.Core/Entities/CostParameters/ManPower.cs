using FormworkOptimize.Core.Entities.CostParameters.Interfaces;
using System;

namespace FormworkOptimize.Core.Entities.CostParameters
{
    public class ManPower:ICalculateInstallationTime
    {

        #region Private Fields

        private readonly Lazy<int> _installDuration;

        #endregion

        #region Properties

        /// <summary>
        /// Area of Floor in (m^2).
        /// </summary>
        public double FloorArea { get;  }

        /// <summary>
        /// Productivity for the chosen system (m^2/day)
        /// </summary>
        public double Productivity { get; }

        /// <summary>
        /// Daily Cost of one labor (LE/Day)
        /// </summary>
        public double LaborCost { get; }

        public ICalculateNoWorkers NoWorkers { get; }

        #endregion

        #region Constructors

        public ManPower(double productivity, double laborCost,double floorArea, ICalculateNoWorkers noWorkers)
        {
            Productivity = productivity;
            LaborCost = laborCost;
            NoWorkers = noWorkers;
            FloorArea = floorArea;
            _installDuration = new Lazy<int>(() => (int)Math.Ceiling((FloorArea / (NoWorkers.CalculateNoWorkers() * Productivity))) + 1);
        }

        #endregion


        #region Methods

        public int CalculateInstallationTime() => 
            _installDuration.Value;
      

        #endregion

    }
}
