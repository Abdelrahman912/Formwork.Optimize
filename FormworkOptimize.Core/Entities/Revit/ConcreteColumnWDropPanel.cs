using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.Geometry;

namespace FormworkOptimize.Core.Entities.Revit
{
    public class ConcreteColumnWDropPanel:ConcreteColumn
    {

        #region Properties

        public FormworkRectangle Drop { get;  }

        #endregion

        #region Constructors

        public ConcreteColumnWDropPanel(double b, double h, XYZ center, FormworkRectangle cornerPoints,FormworkRectangle drop)
            :base(b, h, center, cornerPoints)
        {
            Drop = drop;
        }

        public ConcreteColumnWDropPanel(ConcreteColumn column, FormworkRectangle drop)
           : this(column.B, column.H, column.Center, column.CornerPoints,drop)
        {
            
        }

        #endregion

    }
}
