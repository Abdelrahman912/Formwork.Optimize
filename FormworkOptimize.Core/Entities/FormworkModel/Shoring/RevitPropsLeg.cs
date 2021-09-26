using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    public class RevitPropsLeg
    {
        #region Properties

        public XYZ LocationPoint { get; }

        public double OffsetFromLevel { get; }

        public Level HostLevel { get; }

        #endregion

        #region Constructors

        public RevitPropsLeg(XYZ locationPoint,
                             Level hostLevel,
                             double offsetFromLevel)

        {
            LocationPoint = locationPoint;
            HostLevel = hostLevel;
            OffsetFromLevel = offsetFromLevel;
        }

        #endregion

    }
}
