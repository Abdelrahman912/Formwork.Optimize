using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.FormworkModel.SuperStructure
{
    public class RevitLinePlywood:RevitPlywood
    {

        #region Properties

        public List<List<Line>> ConcreteFloorOpenings { get; }


        #endregion

        #region Constructors

        public RevitLinePlywood(PlywoodSectionName sectionName,
                                RevitLineFloor revitFloor,
                                Level hostLevel,
                                double offsetFromLevel,
                                List<FormworkRectangle> plywoodOpenings)
            :base(sectionName, revitFloor.Boundary, hostLevel, offsetFromLevel, plywoodOpenings)
        {
            ConcreteFloorOpenings = revitFloor.Openings;
        }

        #endregion

    }
}
