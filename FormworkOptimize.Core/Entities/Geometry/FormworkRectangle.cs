using Autodesk.Revit.DB;
using FormworkOptimize.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.Core.Entities.Geometry
{
    public class FormworkRectangle
    {
        #region Properties

        public List<XYZ> Points { get; }

        public List<Line> Lines { get; }

        public double LengthX { get; }

        public double LengthY { get;}

        public XYZ Center { get; }

        #endregion

        #region Constructors

        public FormworkRectangle(XYZ p0, XYZ p1, XYZ p2, XYZ p3) :
            this(new List<XYZ>() { p0, p1, p2, p3 })
        { }

        public FormworkRectangle(List<XYZ> points)
        {
            if (points.Count != 4)
                throw new System.Exception("Not Valid Rectangle");
            Points = points.Select(p=>p.CopyWithNewZ(0)).ToList();
            Lines = Points.ToLines();
            LengthX = Lines.First(l => l.Direction.IsParallelTo(XYZ.BasisX)).Length;
            LengthY = Lines.First(l=>l.Direction.IsParallelTo(XYZ.BasisY)).Length;
            Center = (points[0] + points[2]) / 2;
        }

        #endregion
    }
}
