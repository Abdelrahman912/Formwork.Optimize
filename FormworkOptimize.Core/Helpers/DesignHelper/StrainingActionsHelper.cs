using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Helpers.DesignHelper
{
    public static class StrainingActionsHelper
    {
        public static List<DesignReport> CreateReports(this StrainingActions sa, Beam beam)
        {
            var list = new List<DesignReport>()
            {
                new DesignReport(DesignCheckName.MOMENT,beam.Section.Mall,sa.MaxMoment),
                new DesignReport(DesignCheckName.SHEAR,beam.Section.Qall,sa.MaxShear),
                new DesignReport(DesignCheckName.DEFLECTION,beam.Section.DeltaAll,sa.MaxDeflection)
            };
            return list;
        }

        public static List<DesignReport> CreateReports(this StrainingActions sa, Plywood plywood)
        {
            var list = new List<DesignReport>()
            {
                new DesignReport(DesignCheckName.MOMENT,plywood.Section.Mall,sa.MaxMoment),
                new DesignReport(DesignCheckName.SHEAR,plywood.Section.Qall,sa.MaxShear),
                new DesignReport(DesignCheckName.DEFLECTION,plywood.DeltaAll,sa.MaxDeflection)
            };
            return list;
        }
    }
}
