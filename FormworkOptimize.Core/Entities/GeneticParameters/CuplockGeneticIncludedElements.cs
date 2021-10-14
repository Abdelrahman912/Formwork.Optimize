using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticParameters
{
    public class CuplockGeneticIncludedElements
    {
      

        #region Properties

        public List<double> IncludedLedgers { get; }

        public List<double> IncludedVerticals { get; }

        public List<double> IncludedCuplockBraces { get; }

        public List<PlywoodSectionName> IncludedPlywoods { get; }

        public List<BeamSectionName> IncludedBeamSections { get; }

        public List<SteelType> IncludedSteelTypes { get; }

        #endregion

        #region Constructors

        public CuplockGeneticIncludedElements(List<double> includedLedgers, List<double> includedVerticals, List<double> includedCuplockBraces, List<PlywoodSectionName> includedPlywoods, List<BeamSectionName> includedBeamSections, List<SteelType> includedSteelTypes)
        {
            IncludedLedgers = includedLedgers;
            IncludedVerticals = includedVerticals;
            IncludedCuplockBraces = includedCuplockBraces;
            IncludedPlywoods = includedPlywoods;
            IncludedBeamSections = includedBeamSections;
            IncludedSteelTypes = includedSteelTypes;
        }

        #endregion

    }
}
