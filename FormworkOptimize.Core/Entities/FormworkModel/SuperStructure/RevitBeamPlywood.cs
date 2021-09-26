using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.FormworkModel.SuperStructure
{
    public class RevitBeamPlywood:RevitPlywood
    {

        #region Properties

        public ConcreteBeam ConcreteBeam { get; }

        public double ConcreteFloorThickness { get; }

        #endregion

        #region Constructors

        public RevitBeamPlywood(PlywoodSectionName sectionName,
                                List<Line> boundary,
                                Level hostLevel,
                                double offsetFromLevel,
                                ConcreteBeam concreteBeam,
                                double concreteFloorThickness,
                                List<FormworkRectangle> plywoodOpenings)
                                
            :base(sectionName, boundary, hostLevel, offsetFromLevel,plywoodOpenings)
        {

            ConcreteBeam = concreteBeam;
            ConcreteFloorThickness = concreteFloorThickness;

        }

        #endregion

    }
}
