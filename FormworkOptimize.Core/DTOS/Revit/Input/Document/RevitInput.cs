using Autodesk.Revit.DB;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Document
{
    public class RevitInput
    {

        #region Properties

        public Level HostLevel { get; }

        public double HostFloorOffset { get; }

        #endregion

        #region Constructors

        public RevitInput(Level hostLevel , double hostFloorOffset)
        {
            HostLevel = hostLevel;
            HostFloorOffset = hostFloorOffset;
        }

        #endregion

    }
}
