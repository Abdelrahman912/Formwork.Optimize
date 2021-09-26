using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.Geometry;

namespace FormworkOptimize.Core.Entities.Revit
{
    public class ConcreteColumn:ConcreteElement
    {

        #region Properties

        public XYZ Center { get; }

        public FormworkRectangle CornerPoints { get; }

        #endregion

        #region Constructors

        public ConcreteColumn(double b , double h , XYZ center,FormworkRectangle cornerPoints ):
            base(b,h)
        {
            Center = center;
            CornerPoints = cornerPoints;
        }

        #endregion

    }
}
