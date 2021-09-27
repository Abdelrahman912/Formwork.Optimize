using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticParameters
{
    public class GeneticIncludedElements 
    {

        #region Properties

        public List<PlywoodSectionName> IncludedPlywoods { get; }

        public List<BeamSectionName> IncludedBeamSections { get; }

        #endregion

        #region Constructors

        public GeneticIncludedElements(List<PlywoodSectionName> includedPlywoods, List<BeamSectionName> includedBeamSections)
        {
            IncludedPlywoods = includedPlywoods;
            IncludedBeamSections = includedBeamSections;
        }

        #endregion

    }
}
