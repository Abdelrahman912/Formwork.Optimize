using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.Entities
{
    public class BeamLengths
    {
        #region Properties

        public BeamSectionName SectionName { get; }

        public List<double> BeamLengthsList { get; }

        #endregion

        #region Constructors

        public BeamLengths(BeamSectionName sectionName, List<double> beamLengthList)
        {
            SectionName = sectionName;
            BeamLengthsList = beamLengthList;
        }

        #endregion
    }
}
