using Autodesk.Revit.DB;
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

        public List<PlywoodSectionName> IncludedPlywoods { get; }

        public List<BeamSectionName> IncludedBeamSections { get;}

        #endregion

        #region Constructors

        public GeneticDesignInput(Floor supportedFloor, 
                                  int noGenerations, 
                                  int noPopulation,
                                  List<PlywoodSectionName> includedPlywoods,
                                  List<BeamSectionName> includedBeamSections)
        {
            SupportedFloor = supportedFloor;
            NoGenerations = noGenerations;
            NoPopulation = noPopulation;
            IncludedPlywoods = includedPlywoods;
            IncludedBeamSections = includedBeamSections;
        }

        #endregion

    }
}
