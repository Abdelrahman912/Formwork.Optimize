using Autodesk.Revit.DB;
using FormworkOptimize.Core.Extensions;

namespace FormworkOptimize.Core.Entities.Revit
{
    public class FormworkBeamInstance
    {

        #region Properties

        public XYZ LocationPoint { get;  }

        public XYZ Direction { get; }

        #endregion

        #region Constructors

        public FormworkBeamInstance(LocationPoint locp)
        {
           LocationPoint =  locp.Point.CopyWithNewZ(0);
            Direction = XYZ.BasisX.RotateAboutZ(locp.Rotation);
        }

        #endregion

    }
}
