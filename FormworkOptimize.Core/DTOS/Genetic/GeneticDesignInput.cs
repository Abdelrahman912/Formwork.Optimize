using Autodesk.Revit.DB;

namespace FormworkOptimize.Core.DTOS.Genetic
{
    public class GeneticDesignInput
    {
        #region Properties

        public Floor SupportedFloor { get; }

        public int NoGenerations { get; }

        public int NoPopulation { get; }

        #endregion

        #region Constructors

        public GeneticDesignInput(Floor supportedFloor, int noGenerations, int noPopulation)
        {
            SupportedFloor = supportedFloor;
            NoGenerations = noGenerations;
            NoPopulation = noPopulation;
        }

        #endregion

    }
}
