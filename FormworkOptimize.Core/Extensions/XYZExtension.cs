using Autodesk.Revit.DB;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Entities.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Comparers.Comparers;
using static FormworkOptimize.Core.Constants.RevitBase;

namespace FormworkOptimize.Core.Extensions
{
    public static class XYZExtension
    {

        public static int GetHash(this XYZ point)
        {
            var xInt = (int)(point.X * FORMWORK_NUMBER);
            var yInt = (int)(point.Y * FORMWORK_NUMBER);
            var zInt = (int)(point.Z * FORMWORK_NUMBER);

            var prime1 = 17;
            var prime2 = 23;
            unchecked
            {
                var hash = prime1 * prime2 * (xInt.GetHashCode() + xInt.GetHashCode() + xInt.GetHashCode());
                return hash;
            }
        }

        /// <summary>
        /// Take List of repeated XYZ and returns un repated XYZ and number of repeatition for each XYZ.
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<XYZ, int>> GroupXYZ(this IEnumerable<XYZ> points)
        {
            var distinctPoints = points.Distinct(XYZComparer).ToList();
            return distinctPoints.Aggregate(new List<Tuple<XYZ, int>>(), (soFar, current) =>
              {
                  var count = points.Where(p => current.IsEqual(p)).Count();
                  soFar.Add(Tuple.Create(current, count));
                  return soFar;
              });
        }

        /// <summary>
        /// Determines whether vector is prependicular to another vector or not.
        /// </summary>
        /// <param name="vec1">First Vector.</param>
        /// <param name="vec2">Second Vector.</param>
        /// <returns>
        ///   <c>true</c> if [is prep to] [the specified vec2]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrepTo(this XYZ vec1, XYZ vec2) =>
            Math.Abs(vec1.DotProduct(vec2)) < RevitBase.TOLERANCE;

        /// <summary>
        /// Determines whether a vector is parallel to another vector or not.
        /// </summary>
        /// <param name="vec1">First vector.</param>
        /// <param name="vec2">Second vector.</param>
        /// <returns>
        ///   <c>true</c> if [is parallel to] [the specified vec2]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsParallelTo(this XYZ vec1, XYZ vec2) =>
            Math.Abs(Math.Abs(vec1.Normalize().DotProduct(vec2.Normalize())) - 1) < RevitBase.TOLERANCE;

        /// <summary>
        /// Projects polygon points to polygon lines.
        /// </summary>
        /// <param name="pts">The PTS.</param>
        /// <returns></returns>
        public static List<Line> ToLines(this List<XYZ> points)
        {
            return points.Aggregate(new List<Line>(), (soFar, current) =>
            {
                var i = points.IndexOf(current);
                var j = (i + 1) % points.Count;
                var nextPoint = points[j];
                if (current.DistanceTo(nextPoint) > TOLERANCE)
                    soFar.Add(Line.CreateBound(current, nextPoint));
                return soFar;
            }).ToList();
        }

       
        public static (XYZ Min, XYZ Max) CreateXYMinMax(this IEnumerable<XYZ> points, double zmin = 0, double zmax = 0)
        {
            var sortedByX = points.OrderBy(p => p.X);
            var minX = sortedByX.First().X;
            var maxX = sortedByX.Last().X;

            var sortedByY = points.OrderBy(p => p.Y);
            var minY = sortedByY.First().Y;
            var maxY = sortedByY.Last().Y;

            var min = new XYZ(minX, minY, zmin);
            var max = new XYZ(maxX, maxY, zmax);

            return (Min: min, Max: max);
        }

        public static (XYZ Min, XYZ Max) AddToleranceForColinearBeams(this (XYZ Min, XYZ Max) minMax, Line beamLine)
        {
            var minMaxVec = minMax.Max - minMax.Min;
            if (!minMaxVec.IsParallelTo(beamLine.Direction))
                return minMax;
            var prepVec = beamLine.Direction.CrossProduct(XYZ.BasisZ);
            var newMinPoint = minMax.Min - 1 * prepVec;
            var newMaxPoint = minMax.Max + 1 * prepVec;
            return (Min: newMinPoint, Max: newMaxPoint);
        }

        /// <summary>
        /// Determines whether p1 is equal to p2 or not.
        /// </summary>
        /// <param name="p1">Point p1.</param>
        /// <param name="p2">Point p2.</param>
        /// <returns>
        ///   <c>true</c> if the specified p2 is equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEqual(this XYZ p1, XYZ p2) =>
           p1.DistanceTo(p2) < RevitBase.TOLERANCE ? true : false;

        /// <summary>
        /// Determines a poin is on Line defined by start and end point.
        /// </summary>
        /// <param name="p">Point.</param>
        /// <param name="sp">Start Point.</param>
        /// <param name="ep">End Point.</param>
        /// <returns>
        ///   <c>true</c> if [is on line] [the specified sp]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOnLine(this XYZ p, XYZ sp, XYZ ep)
        {
            if (p.IsEqual(sp) || p.IsEqual(ep))
                return true;
            var psp = sp - p;
            var pep = ep - p;
            var lineLength = sp.DistanceTo(ep);
            return Math.Abs(psp.Normalize().DotProduct(pep.Normalize()) + 1) < RevitBase.TOLERANCE /*&& Math.Abs(psp.GetLength()+pep.GetLength() -lineLength) < RevitBase.TOLERANCE*/;
            //var spp = p - sp;
            //var spep = ep - sp;
            //return Math.Abs(spp.Normalize().DotProduct(spep.Normalize()) - 1) < RevitBase.TOLERANCE && spp.GetLength() < spep.GetLength();
        }

        /// <summary>
        /// Determines a poin is on Line defined by start and end point.
        /// </summary>
        /// <param name="p">Point.</param>
        /// <param name="sp">Start Point.</param>
        /// <param name="ep">End Point.</param>
        /// <returns>
        ///   <c>true</c> if [is on line] [the specified sp]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOnLine(this XYZ p, Line line) =>
            p.IsOnLine(line.GetEndPoint(0), line.GetEndPoint(1));


        public static RevitLedger GetNearestLedger(this XYZ point, List<RevitLedger> ledgers) =>
        ledgers.OrderBy(ledger => Math.Min(point.DistanceTo(ledger.StartPoint), point.DistanceTo(ledger.EndPoint)))
               .First();

        public static XYZ Abs(this XYZ vec) =>
            new XYZ(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));

        /// <summary>
        /// Rotate Vector around Z axis by radian angle
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="angle">The angle.</param>
        /// <returns></returns>
        public static XYZ RotateAboutZ(this XYZ vector, double angle)
        {
            return new XYZ(vector.X * Math.Cos(angle) - vector.Y * Math.Sin(angle), vector.X * Math.Sin(angle) + vector.Y * Math.Cos(angle), vector.Z);
        }


        /// <summary>
        /// Determines whether [is in polygon] [the specified test point].
        /// </summary>
        /// <param name="testPoint">The test point.</param>
        /// <param name="vertices">The vertices.</param>
        /// <returns>
        ///   <c>true</c> if [is in polygon] [the specified test point]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInPolygon(this XYZ testPoint, IList<XYZ> vertices)
        {

            Func<double, double, double, bool> isBetween = (x, a, b) => (x - a) * (x - b) < 0;

            if (vertices.Count < 3) return false;
            bool isInPolygon = false;
            var lastVertex = vertices[vertices.Count - 1];
            foreach (var vertex in vertices)
            {
                if (isBetween(testPoint.Y, lastVertex.Y, vertex.Y))
                {
                    double t = (testPoint.Y - lastVertex.Y) / (vertex.Y - lastVertex.Y);
                    double x = t * (vertex.X - lastVertex.X) + lastVertex.X;
                    if (Math.Abs(x - testPoint.X) < RevitBase.TOLERANCE || x - testPoint.X > RevitBase.TOLERANCE) isInPolygon = !isInPolygon;
                }
                else
                {
                    if (Math.Abs(testPoint.Y - lastVertex.Y) < RevitBase.TOLERANCE && testPoint.X < lastVertex.X && vertex.Y > testPoint.Y) isInPolygon = !isInPolygon;
                    if (Math.Abs(testPoint.Y - vertex.Y) < RevitBase.TOLERANCE && testPoint.X < vertex.X && lastVertex.Y > testPoint.Y) isInPolygon = !isInPolygon;
                }
                lastVertex = vertex;
            }

            return isInPolygon;
        }

        /// <summary>
        /// Determines whether [is all in polygon] [the specified points].
        /// </summary>
        /// <param name="small">The points.</param>
        /// <param name="big">The vertices.</param>
        /// <returns>
        ///   <c>true</c> if [is all in polygon] [the specified points]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPolygonInside(this List<XYZ> small, List<XYZ> big)
        {
            var z = big.First().Z;
            small = small.Select(p => p.CopyWithNewZ(z)).ToList();
            foreach (var point in small)
            {
                var isIn = point.IsInPolygon(big);
                if (!isIn)
                {
                    for (int i = 0; i < big.Count; i++)
                    {
                        int j = (i + 1) % big.Count;
                        var result = point.IsOnLine(big[i], big[j]);
                        if (result)
                        {
                            isIn = result;
                            break;
                        }
                    }
                    if (!isIn)
                        return false;
                }
            }
            return true;
        }

        public static XYZ CopyWithNewZ(this XYZ point, double z) =>
            new XYZ(point.X, point.Y, z);

        public static FormworkRectangle NewFormworkRectangle(this XYZ origin, XYZ lengthXVec, XYZ lengthYVec)
        {
            var p0 = origin;
            var p1 = origin + lengthYVec;
            var p2 = p1 + lengthXVec;
            var p3 = origin + lengthXVec;
            return new FormworkRectangle(p0, p1, p2, p3);
        }

        /// <summary>
        /// Get the scalar projection of some vector onto some direction.
        /// </summary>
        /// <param name="vec">Vector we want to get its scalar Projection.</param>
        /// <param name="dir">Direction in which the vector is going to be projected onto.</param>
        /// <returns></returns>
        public static double ScalarProjOnto(this XYZ vec, XYZ dir) => vec.DotProduct(dir) / dir.GetLength();

        /// <summary>
        /// Gets the vector projection of a vector onto another vector.
        /// </summary>
        /// <param name="vec">The vec.</param>
        /// <param name="dir">The dir.</param>
        /// <returns></returns>
        public static XYZ VectorProjOnto(this XYZ vec, XYZ dir) => (vec.ScalarProjOnto(dir) / dir.GetLength()) * dir;

    }
}
