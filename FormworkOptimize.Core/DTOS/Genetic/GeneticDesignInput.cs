using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.GeneticParameters;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.DTOS.Genetic
{
    public class GeneticDesignInput
    {
        #region Properties

        public Floor SupportedFloor { get; }

        public int NoGenerations { get; }

        public int NoPopulation { get; }

        public double CrossOverProbability { get; }

        public double MutationProbability { get; }

        public GeneticIncludedElements IncludedElements { get; }

        #endregion

        #region Constructors

        public GeneticDesignInput(Floor supportedFloor,
                                  int noGenerations,
                                  int noPopulation,
                                  double crossOverPropability,
                                  double mutationPropability,
                                  GeneticIncludedElements includedElements)
        {
            SupportedFloor = supportedFloor;
            NoGenerations = noGenerations;
            NoPopulation = noPopulation;
            CrossOverProbability = crossOverPropability;
            MutationProbability = mutationPropability;
            IncludedElements = includedElements;
        }

        #endregion

    }
}
