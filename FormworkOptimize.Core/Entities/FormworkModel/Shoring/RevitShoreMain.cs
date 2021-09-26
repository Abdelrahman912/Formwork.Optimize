using Autodesk.Revit.DB;
using FormworkOptimize.Core.Extensions;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    public class RevitShoreMain
    {
        #region Properties

        public int NoOfMains { get; set; }

        public double TelescopicOffset { get; }

        public double PostLockDistance { get; }

        public double ULockDistance { get; }

        /// <summary>
        /// Vector in which U-Head points.
        /// </summary>
        public XYZ UVector { get; }

        public XYZ Position { get; }

        public Level HostLevel { get; }

        public double OffsetFromLevel { get; }

        #endregion

        #region Constructors

        public RevitShoreMain(int noOfMains, double telescopicOffset, double postLockDistance, double uLockDistance,
                            XYZ position, Level hostLevel, double offsetFromLevel, XYZ uVector)
        {
            NoOfMains = noOfMains;
            TelescopicOffset = telescopicOffset;
            PostLockDistance = postLockDistance;
            ULockDistance = uLockDistance;
            Position = position.CopyWithNewZ(0);
            HostLevel = hostLevel;
            OffsetFromLevel = offsetFromLevel;
            UVector = uVector;
        }

        #endregion

    }
}
