using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticParameters
{
    public class GeneticIncludedElements 
    {

        #region Properties

        public List<PlywoodSectionName> IncludedPlywoods { get; }

        public List<BeamSectionName> IncludedBeamSections { get; }

        public List<double> IncludedLedgers { get; }

        public List<double> IncludedVerticals { get; }

        public List<double> IncludedCuplockBraces { get;  }

        public List<SteelType> IncludedSteelTypes { get; }

        public List<EuropeanPropTypeName> IncludedProps { get;  }

        public List<double> IncludedShoreBracing { get;  }

        public List<FrameTypeName> IncludedFrames { get;}

        #endregion

        #region Constructors

        public GeneticIncludedElements(List<PlywoodSectionName> includedPlywoods, 
                                       List<BeamSectionName> includedBeamSections,
                                       List<double> includedLedgers,
                                       List<double> includedVerticals,
                                       List<double> includedCuplockBraces,
                                       List<SteelType> includedSteelTypes,
                                       List<EuropeanPropTypeName> includedProps,
                                       List<double> includedShoreBracing,
                                       List<FrameTypeName> includedFrames)
        {
            IncludedPlywoods = includedPlywoods;
            IncludedBeamSections = includedBeamSections;
            IncludedLedgers = includedLedgers;
            IncludedVerticals = includedVerticals;
            IncludedCuplockBraces = includedCuplockBraces;
            IncludedSteelTypes = includedSteelTypes;
            IncludedProps = includedProps;
            IncludedShoreBracing = includedShoreBracing;
            IncludedFrames = includedFrames;
        }

        #endregion

    }
}
