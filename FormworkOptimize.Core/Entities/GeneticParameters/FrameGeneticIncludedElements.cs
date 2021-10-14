using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticParameters
{
    public class FrameGeneticIncludedElements
    {

        #region Properties

        public List<PlywoodSectionName> IncludedPlywoods { get; }

        public List<BeamSectionName> IncludedBeamSections { get; }

        public List<FrameTypeName> IncludedFrames { get; }

        public List<double> IncludedShoreBracing { get; }

        #endregion

        #region Constructors

        public FrameGeneticIncludedElements(List<PlywoodSectionName> includedPlywoods, List<BeamSectionName> includedBeamSections, List<FrameTypeName> includedFrames, List<double> includedShoreBracing)
        {
            IncludedPlywoods = includedPlywoods;
            IncludedBeamSections = includedBeamSections;
            IncludedFrames = includedFrames;
            IncludedShoreBracing = includedShoreBracing;
        }

        #endregion

    }
}
