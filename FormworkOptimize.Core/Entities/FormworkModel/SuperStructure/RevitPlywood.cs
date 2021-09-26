using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.FormworkModel.SuperStructure
{
    public abstract class RevitPlywood
    {
        #region Properties

        public PlywoodSectionName SectionName { get;}

        public List<Line> Boundary { get;}

        public List<FormworkRectangle> PlywoodOpenings { get;}

        public Level HostLevel { get; }

        public double OffsetFromLevel { get;}

        #endregion

        #region Constructors

        public RevitPlywood(PlywoodSectionName sectionName ,
                            List<Line> boundary ,
                            Level hostLevel ,
                            double offsetFromLevel,
                            List<FormworkRectangle> plywoodOpenings)
                           
        {
            SectionName = sectionName;
            Boundary = boundary;
            HostLevel = hostLevel;
            OffsetFromLevel = offsetFromLevel;
            PlywoodOpenings = plywoodOpenings;
        }

        #endregion


    }
}
