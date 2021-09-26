using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.Geometry
{
    public class DeckingRectangle:FormworkRectangle
    {

        #region Properties 

        public XYZ MainBeamDir { get;  }

        public XYZ SecBeamDir { get;  }

        #endregion

        #region Constructors

        public DeckingRectangle(List<XYZ> points , XYZ mainDir)
            :base(points)
        {
            MainBeamDir = mainDir.Normalize();
            SecBeamDir = mainDir.Normalize().CrossProduct(XYZ.BasisZ);
        }

        #endregion

    }
}
