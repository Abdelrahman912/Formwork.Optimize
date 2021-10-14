using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticParameters
{
    public class ShoreBraceGeneticIncludedElements
    {

        #region Properties

        public List<PlywoodSectionName> IncludedPlywoods { get; }

        public List<BeamSectionName> IncludedBeamSections { get; }

        public List<double> IncludedShoreBracing { get; }

        #endregion

        #region Constructors

        public ShoreBraceGeneticIncludedElements(List<PlywoodSectionName> includedPlywoods, List<BeamSectionName> includedBeamSections, List<double> includedShoreBracing)
        {
            IncludedPlywoods = includedPlywoods;
            IncludedBeamSections = includedBeamSections;
            IncludedShoreBracing = includedShoreBracing;
        }

        #endregion

    }
}
