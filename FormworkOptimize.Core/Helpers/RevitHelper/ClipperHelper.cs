using Autodesk.Revit.DB;
using ClipperLib;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Entities.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace FormworkOptimize.Core.Helpers.RevitHelper
{
    public static class ClipperHelper
    {

        public static long FeetToLong(this double val) => (long)(val * RevitBase.FORMWORK_NUMBER);

        public static double LongToFeet(this long val) => (double)val / RevitBase.FORMWORK_NUMBER;

        /// <summary>
        /// Gets Clipper Point from Revit point.
        /// </summary>
        /// <param name="p">Revit point.</param>
        /// <returns></returns>
        public static IntPoint ToIntPoint(this XYZ p) => new IntPoint(p.X.FeetToLong(), p.Y.FeetToLong());

        /// <summary>
        /// Gets Revit point from Clipper point.
        /// </summary>
        /// <param name="p">Clipper point.</param>
        /// <param name="z">Point's Z value.</param>
        /// <returns></returns>
        public static XYZ GetXYZ(this IntPoint p, double z = 0) => new XYZ(p.X.LongToFeet(), p.Y.LongToFeet(), z);

        private static Polygon  ToPolygon(this List<XYZ> polygon) =>
                polygon.Aggregate(new Polygon(), (soFar, current) => { soFar.Add(current.ToIntPoint()); return soFar; }).ToList();

        /// <summary>
        /// Check whether a rectangle intersects with another rectangle.
        /// </summary>
        /// <param name="subjRect"></param>
        /// <param name="clipRect"></param>
        /// <returns></returns>
        public static bool IsBooleanIntersectWith(this FormworkRectangle subjRect, FormworkRectangle clipRect) =>
            subjRect.IsBooleanIntersectWith(clipRect.Points);
        //{
        //    //Note: Subject rectangle is inside Clip Rectangle => No Intersection.
        //    var subj = subjRect.Points.ToPolygon();
        //    var clip = clipRect.Points.ToPolygon();

        //    var clipper = new Clipper();
        //    clipper.AddPolygon(subj, PolyType.ptSubject);
        //    clipper.AddPolygon(clip, PolyType.ptClip);
        //    var solution = new Polygons();
        //    clipper.Execute(ClipType.ctIntersection, solution);
        //    return solution.Count !=0 ;
        //}


        /// <summary>
        /// Check whether a rectangle intersects with another rectangle.
        /// </summary>
        /// <param name="subjRect"></param>
        /// <param name="clipPolygon"></param>
        /// <returns></returns>
        public static bool IsBooleanIntersectWith(this FormworkRectangle subjRect, List<XYZ> clipPolygon)
        {
            //Note: Subject rectangle is inside Clip Rectangle => No Intersection.
            var subj = subjRect.Points.ToPolygon();
            var clip = clipPolygon.ToPolygon();

            var clipper = new Clipper();
            clipper.AddPolygon(subj, PolyType.ptSubject);
            clipper.AddPolygon(clip, PolyType.ptClip);
            var solution = new Polygons();
            clipper.Execute(ClipType.ctIntersection, solution);
            return solution.Count != 0;
        }

        public static List<XYZ> DoBooleanIntersectWith(this FormworkRectangle subjRect, List<XYZ> clipPolygon)
        {
            //Note: Subject rectangle is inside Clip Rectangle => No Intersection.
            var subj = subjRect.Points.ToPolygon();
            var clip = clipPolygon.ToPolygon();

            var clipper = new Clipper();
            clipper.AddPolygon(subj, PolyType.ptSubject);
            clipper.AddPolygon(clip, PolyType.ptClip);
            var solution = new Polygons();
            clipper.Execute(ClipType.ctIntersection, solution);
            if (solution.Count > 0)
            {
                return solution.First()
                               .Select(intPoint => intPoint.GetXYZ())
                               .ToList();
            }
            else
            {
                return new List<XYZ>();
            }
        }


        /// <summary>
        /// Do boolean difference between rectangles.
        /// </summary>
        /// <param name="subjRect">Recatangle that will be subtracted.</param>
        /// <param name="clipRects"></param>
        /// <returns></returns>
        public static List<FormworkRectangle> DoBooleanDifferenceWith(this FormworkRectangle subjRect ,List<FormworkRectangle> clipRects)
        {
            var subj = subjRect.Points.ToPolygon();
            var clips = clipRects.Aggregate(new Polygons(), (soFar, current) =>
            {
                var rect = current.Points.ToPolygon();
                soFar.Add(rect);
                return soFar;
            });
            var solution = new Polygons();
            Clipper c = new Clipper();
            c.AddPolygon(subj, PolyType.ptSubject);
            c.AddPolygons(clips, PolyType.ptClip);

            c.Execute(ClipType.ctDifference, solution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
           var rectangles =  solution.Where(points=>points.Count==4)
                                     .Select(poly => new FormworkRectangle(poly.Select(intPoint => intPoint.GetXYZ()).ToList()))
                                     .ToList();
            if (rectangles.Count == 0 )
                return new List<FormworkRectangle>() { subjRect };
            return rectangles;
        }

    }
}
