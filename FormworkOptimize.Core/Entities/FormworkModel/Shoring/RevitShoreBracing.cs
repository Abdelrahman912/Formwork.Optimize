using Autodesk.Revit.DB;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    public class RevitShoreBracing
    {
        #region Properties

        public XYZ LocationPoint { get; }

        public XYZ Direction { get; }

        public double Width { get; }

        public double Offset { get; }

        public int NoOfMains { get; }

        public Level HostLevel { get; }

        #endregion

        #region Constructors

        public RevitShoreBracing(XYZ startPoint,
                                   XYZ dir,
                                   double width,
                                   double offset,
                                   int noOfMains,
                                   Level hostLevel)

        {
            LocationPoint = startPoint;
            HostLevel = hostLevel;
            Direction = dir;
            Width = width;
            Offset = offset;
            NoOfMains = noOfMains;
        }

        #endregion

    }
}
