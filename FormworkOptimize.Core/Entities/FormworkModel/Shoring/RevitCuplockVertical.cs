using Autodesk.Revit.DB;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    public class RevitCuplockVertical
    {

        #region Properties

        /// <summary>
        /// Collection of vertical elements lengths in order from bottom to top.
        /// </summary>
        public List<double> OrderedVerticalLengths { get; set; }

        public double PostLockDistance { get; }

        public double ULockDistance { get; }

        /// <summary>
        /// Vector in which U-Head points.
        /// </summary>
        public XYZ UVector { get; }

        public XYZ Position { get; }

        public Level HostLevel { get; }

        public double OffsetFromLevel { get; }

        public SteelType SteelType { get;  }

        #endregion

        #region Constructors

        public RevitCuplockVertical(List<double> orderedVerticalLengths , double postLockDistance , double uLockDistance ,
                            XYZ position,Level hostLevel,double offsetFromLevel,XYZ uVector,SteelType steelType)
        {
            OrderedVerticalLengths = orderedVerticalLengths;
            PostLockDistance = postLockDistance;
            ULockDistance = uLockDistance;
            Position = position;
            HostLevel = hostLevel;
            OffsetFromLevel = offsetFromLevel;
            UVector = uVector;
            SteelType = steelType;
        }

        #endregion



    }
}
