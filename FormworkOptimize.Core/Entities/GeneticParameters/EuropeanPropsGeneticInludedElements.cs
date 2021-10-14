using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticParameters
{
    public class EuropeanPropsGeneticInludedElements
    {

        #region Properties

        public List<PlywoodSectionName> IncludedPlywoods { get; }

        public List<BeamSectionName> IncludedBeamSections { get; }

        public List<EuropeanPropTypeName> IncludedProps { get; }

        #endregion

        #region Contsrutcors

        public EuropeanPropsGeneticInludedElements(List<PlywoodSectionName> includedPlywoods, List<BeamSectionName> includedBeamSections, List<EuropeanPropTypeName> includedProps)
        {
            IncludedPlywoods = includedPlywoods;
            IncludedBeamSections = includedBeamSections;
            IncludedProps = includedProps;
        }

        #endregion

    }
}
