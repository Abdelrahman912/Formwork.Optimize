using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.Revit;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.Core.Extensions
{
    public static class FloorExtension
    {
        /// <summary>
        /// Gets all faces of floor.
        /// </summary>
        /// <param name="floor">The floor.</param>
        /// <param name="doc">The Revit Document.</param>
        /// <returns></returns>
        public static IEnumerable<Face> GetFaces(this Floor floor ,Options options)
        {
            var geoEle = floor.get_Geometry(options);
            return geoEle.OfType<Solid>()
                         .FirstOrDefault()
                         .Faces.Cast<Face>();
        }

        /// <summary>
        /// Gets the lower face of floor.
        /// </summary>
        /// <param name="floor">The floor.</param>
        /// <param name="doc">The options.</param>
        /// <returns></returns>
        public static PlanarFace GetLowerFace(this Floor floor,Options options)
        {
            return floor.GetFaces(options)
                        .OfType<PlanarFace>()
                        .Where(pf => pf.FaceNormal.IsParallelTo(XYZ.BasisZ))
                        .OrderBy(pf => pf.Origin.Z)
                        .First();
        }

        /// <summary>
        /// Gets the upper face of floor.
        /// </summary>
        /// <param name="floor">The floor.</param>
        /// <param name="doc">The options.</param>
        /// <returns></returns>
        public static PlanarFace GetUpperFace(this Floor floor,Options options)
        {
            return floor.GetFaces(options)
                        .OfType<PlanarFace>()
                        .Where(pf => pf.FaceNormal.IsParallelTo(XYZ.BasisZ))
                        .OrderByDescending(pf => pf.Origin.Z)
                        .First();
        }
       
        public static Floor GetFloorAbove(this Floor floor,Document doc, Options options)
        {
            var floorTopEle = floor.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble();
            var floorOrigin = floor.GetUpperFace(options).Origin;
            var floorRay = Line.CreateBound(floorOrigin, floorOrigin + 20 * XYZ.BasisZ);
            var aboveFloor = new FilteredElementCollector(doc).OfClass(typeof(Floor))
                                                              .Cast<Floor>()
                                                              .Where(f => f.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble() > floorTopEle)
                                                              .OrderBy(f => f.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble())
                                                              .FirstOrDefault(f => floorRay.GetIntersectionPoints(f.GetLowerFace(options)).Count() > 0);
            return aboveFloor;
        }

        public static double GetClearHeight(this Floor hostFloor , Element supportedElement)
        {
            var aboveEleAtBottom = supportedElement.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_BOTTOM).AsDouble();
            var eleAtTop = hostFloor.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble();
            return aboveEleAtBottom - eleAtTop;
        }

        /// <summary>
        /// Get the outer Upper polygon of floor.
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static RevitConcreteFloor GetGeometry(this Floor floor,Options options)
        {
            var edges = floor.GetUpperFace(options)
                                    .GetEdgesAsCurveLoops()
                                    .OrderByDescending(cl=>cl.OfType<Line>().Sum(l=>l.Length))
                                    .ToList();
            var boundary = edges.First()
                                .OfType<Line>()
                                .ToList();
            var openings = edges.Skip(1)
                                .Select(cl => cl.OfType<Line>().ToList())
                                .ToList();

            var thickness = floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();

            return new RevitConcreteFloor(boundary,openings,thickness);
        }

    }
}
