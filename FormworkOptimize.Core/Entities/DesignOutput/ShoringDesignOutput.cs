using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities
{
    public class ShoringDesignOutput
    {
        #region Properties

        public string ShoringSystemName { get; private set; }

        public List<DesignReport> DesignReports { get; private set; }

        #endregion

        #region Constructors

        public ShoringDesignOutput(string shoringSystemName, List<DesignReport> designReports)
        {
            ShoringSystemName = shoringSystemName;
            DesignReports = designReports;
        }

        #endregion
    }
}
