using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities
{
    public class SectionDesignOutput
    {
        #region Properties

        public string SectionName { get; private set; }

        public List<DesignReport> DesignReports { get; private set; }

        #endregion

        #region Constructors

        public SectionDesignOutput(string sectionName, List<DesignReport> designReports)
        {
            SectionName = sectionName;
            DesignReports = designReports;
        }

        #endregion
    }
}
