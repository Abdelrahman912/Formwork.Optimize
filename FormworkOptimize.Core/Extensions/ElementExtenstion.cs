using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Entities.Quantification;
using FormworkOptimize.Core.Entities.Revit;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Constants.RevitBase;

namespace FormworkOptimize.Core.Extensions
{
    public static class ElementExtenstion
    {
        public static (double SlabThicknessCm, double BeamWidthCm, double BeamThicknessCm) GetFloorAndBeamDimensions(this Element element)
        {
            double slabThicknessCm = 20.0;
            double beamWidthCm = 0.0;
            double beamThicknessCm = 0.0;

            if (element.GetCategory() == BuiltInCategory.OST_Floors)
            {
                double slabThicknessFeet = Convert.ToDouble(element.get_Parameter(BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS).AsDouble());

                slabThicknessCm = UnitUtils.ConvertFromInternalUnits(slabThicknessFeet, DisplayUnitType.DUT_CENTIMETERS);
            }
            else if (element.GetCategory() == BuiltInCategory.OST_StructuralFraming)
            {
                ElementType elementType = RevitBase.Document.GetElement(element.GetTypeId()) as ElementType;

                double beamWidthFeet = Convert.ToDouble(elementType.LookupParameter("b").AsDouble());

                beamWidthCm = UnitUtils.ConvertFromInternalUnits(beamWidthFeet, DisplayUnitType.DUT_CENTIMETERS);

                double beamThicknessFeet = Convert.ToDouble(elementType.LookupParameter("h").AsDouble());

                beamThicknessCm = UnitUtils.ConvertFromInternalUnits(beamThicknessFeet, DisplayUnitType.DUT_CENTIMETERS);
            }

            return (SlabThicknessCm: slabThicknessCm, BeamWidthCm: beamWidthCm, BeamThicknessCm: beamThicknessCm);
        }

        /// <summary>
        /// Method to get the category of the element.
        /// </summary>
        /// <param name="element">A candidate element in selection operation.</param>
        /// <returns>The category.</returns>
        public static BuiltInCategory GetCategory(this Element element)
        {
            return (BuiltInCategory)element?.Category?.Id?.IntegerValue;
        }

        /// <summary>
        /// Method to get the category id as integer of the element.
        /// </summary>
        /// <param name="element">A candidate element in selection operation.</param>
        /// <returns>The category id as integer.</returns>
        public static int GetCategoryIdAsInteger(this Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }

        public static bool IsRectColumn(this Element col, Document doc)
        {
            if ((BuiltInCategory)col.Category.Id.IntegerValue != BuiltInCategory.OST_StructuralColumns)
                return false;
            try
            {
                var type = doc.GetElement(col.GetTypeId());

                var b = type.LookupParameter("b").AsDouble();
                var h = type.LookupParameter("h").AsDouble();
                return true;
            }
            catch (Exception)
            {

                return false;    
            }
        }

        public static ConcreteColumn ToConcreteColumn(this Element column, Document doc)
        {

            var type = doc.GetElement(column.GetTypeId());

            var b = type.LookupParameter("b").AsDouble();
            var h = type.LookupParameter("h").AsDouble();
            var centerPoint = (column.Location as LocationPoint).Point;
            var theta = (column.Location as LocationPoint).Rotation;

            var basisX = XYZ.BasisX.RotateAboutZ(theta);
            var basisY = XYZ.BasisY.RotateAboutZ(theta);

            var xDist = (b / 2);
            var yDist = (h / 2);

            var leftBottom = centerPoint - (xDist) * basisX - (yDist) * basisY;
            var rightBottom = centerPoint + (xDist) * basisX - (yDist) * basisY;
            var rightTop = centerPoint + (xDist) * basisX + (yDist) * basisY;
            var leftTop = centerPoint - (xDist) * basisX + (yDist) * basisY;
            var rect = new FormworkRectangle(leftBottom, rightBottom, rightTop, leftTop);
            return new ConcreteColumn(b, h, centerPoint.CopyWithNewZ(0), rect);
        }

        public static double GetSmallerLength(this IEnumerable<ConcreteColumn> columns)
        {
            return columns.SelectMany(col1 => columns.Select(col2 => col1.Center.DistanceTo(col2.Center)))
                          .Where(l=>l>TOLERANCE)
                          .OrderBy(l => l)
                          .First();
        }

        public static XYZ GetYOffsetVector(this Element beam,double width)
        {
            var beamLine = (beam.Location as LocationCurve).Curve as Line;
            var yJust = (YJustification)beam.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).AsInteger();
            var yOffset = beam.get_Parameter(BuiltInParameter.Y_OFFSET_VALUE).AsDouble();

            switch (yJust)
            {
                case YJustification.Left:
                    return -XYZ.BasisZ.CrossProduct(beamLine.Direction) * (yOffset + width / 2);
                case YJustification.Center:
                case YJustification.Origin:
                default:
                    return XYZ.BasisZ.CrossProduct(beamLine.Direction) * yOffset;
                case YJustification.Right:
                    return XYZ.BasisZ.CrossProduct(beamLine.Direction) * (yOffset+width/2);
            }

        }

        public static ConcreteBeam ToConcreteBeam(this Element beam, Document doc, double clearHeight)
        {
            if ((BuiltInCategory)beam.Category.Id.IntegerValue != BuiltInCategory.OST_StructuralFraming)
                throw new ArgumentException("Not Concrete Beam");
            var beamLine = (beam.Location as LocationCurve).Curve as Line;
            var type = doc.GetElement(beam.GetTypeId());
            var b = type.LookupParameter("b").AsDouble();
            var h = type.LookupParameter("h").AsDouble();
            var offsetVect = beam.GetYOffsetVector(b);
            var newBeamLine = Line.CreateBound(beamLine.GetEndPoint(0).CopyWithNewZ(0)+offsetVect, beamLine.GetEndPoint(1).CopyWithNewZ(0)+offsetVect);
            return new ConcreteBeam(b, h, newBeamLine, clearHeight);
        }

        public static FormworkRectangle GetBoundingRectFromColumns(this IEnumerable<Element> columns)
        {
            (var minPoint, var maxPoint) = columns.Select(c => (c.Location as LocationPoint).Point)
                                                   .CreateXYMinMax(0, 0);
            var xMin = minPoint.X;
            var yMin = minPoint.Y;

            var xMax = maxPoint.X;
            var yMax = maxPoint.Y;

            //Point Loop from Left Bottom clock wise.
            var p0 = new XYZ(xMin, yMin, 0);
            var p1 = new XYZ(xMin, yMax, 0);
            var p2 = new XYZ(xMax, yMax, 0);
            var p3 = new XYZ(xMax, yMin, 0);
            return new FormworkRectangle(p0, p1, p2, p3);
        }

        public static FormworkRectangle GetBoundingRectFromBeams(this IEnumerable<Element> beams)
        {
            //stretch each beam by tolerance 2ft, inorder to capture the supporting column.
            (var minPoint, var maxPoint) = beams.SelectMany(b => { var line = (b.Location as LocationCurve).Curve as Line; return new List<XYZ> { line.GetEndPoint(0) - 2 * line.Direction, line.GetEndPoint(1) + 2 * line.Direction }; })
                                                .CreateXYMinMax(0, 0)
                                                .AddToleranceForColinearBeams((beams.First().Location as LocationCurve).Curve as Line);
            var xMin = minPoint.X;
            var yMin = minPoint.Y;

            var xMax = maxPoint.X;
            var yMax = maxPoint.Y;

            //Point Loop from Left Bottom clock wise.
            var p0 = new XYZ(xMin, yMin, 0);
            var p1 = new XYZ(xMin, yMax, 0);
            var p2 = new XYZ(xMax, yMax, 0);
            var p3 = new XYZ(xMax, yMin, 0);
            return new FormworkRectangle(p0, p1, p2, p3);
        }

        public static List<Element> FilterColumnsInPolygon(this IEnumerable<Element> columns, List<Line> polygon)
        {
            var polygonPoints = polygon.ToPoints();
            return columns.Where(c => (c.Location as LocationPoint).Point.IsInPolygon(polygonPoints)).ToList();
        }

        public static List<Element> FilterBeamsInPolygon(this List<Element> beams, List<Line> polygon)
        {
            var z = polygon.First().GetEndPoint(0).Z;
            var polygonPoints = polygon.ToPoints();
            return beams.Where(b =>
            {
                var l = (b.Location as LocationCurve).Curve as Line;
                var line = Line.CreateBound(l.GetEndPoint(0).CopyWithNewZ(z), l.GetEndPoint(1).CopyWithNewZ(z));
                return line.GetIntersections(polygon).Count > 0 || (line.GetEndPoint(0).IsInPolygon(polygonPoints) && line.GetEndPoint(1).IsInPolygon(polygonPoints));
            }).ToList();
        }

        public static List<ElementQuantification> QueryDeckingBeams(this IList<Element> elements, Level level)
        {
            var result = RevitBase.DECKING_SYMBOLS.Select(typeName => Tuple.Create(typeName, elements.Where(e => e.Name == typeName)))
                                                    .Where(tuple => tuple.Item2.Count() > 0)
                                                    .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2.ToLookup(e => (int)(e.LookupParameter("L").AsDouble().FeetToMeter().Round(2) * 100))))
                                                    .SelectMany(tuple => tuple.Item2.Select(kvp => new ElementQuantification($"{tuple.Item1} {kvp.Key / 100.0} m", kvp.Count(), level, kvp.Select(e => e.Id))))
                                                    .ToList();
            return result;

        }

        public static List<ElementQuantification> QueryHeads(this IList<Element> elements, Level level)
        {
            var allHeadElements = new List<ElementQuantification>();

            var uHeadInsts = elements.Where(e => e.Name == RevitBase.U_HEAD).ToList();
            var postHeadInsts = elements.Where(e => e.Name == RevitBase.POST_HEAD).ToList();
            var uHeadPropsInts = elements.Where(e => e.Name == RevitBase.PROPS_U_HEAD).ToList();

            if (uHeadInsts.Count > 0)
            {
                var uHeads = new ElementQuantification("U-Head Jack Solid", uHeadInsts.Count, level, uHeadInsts.Select(e => e.Id));
                allHeadElements.Add(uHeads);
            }
            if (uHeadPropsInts.Count > 0)
            {
                var uHeads = new ElementQuantification("U-Head For Props", uHeadPropsInts.Count, level, uHeadPropsInts.Select(e => e.Id));
                allHeadElements.Add(uHeads);
            }
            if (postHeadInsts.Count > 0)
            {
                var postHead = new ElementQuantification("Post Head Jack Solid", postHeadInsts.Count, level, postHeadInsts.Select(e => e.Id));
                allHeadElements.Add(postHead);
            }
            return allHeadElements;
        }

        public static List<ElementQuantification> QueryCuplocks(this IList<Element> elements, Level level)
        {
            var allCuplockElements = new List<ElementQuantification>();

          var allVerticalElements =   elements.Where(e => e.Name == RevitBase.CUPLOCK_VERTICAL)
                                              .ToList();      

            var verticalPoints = allVerticalElements.Select(e => (e.Location as LocationPoint).Point.CopyWithNewZ(0))
                                                    .GroupXYZ()
                                                    .Where(tuple=>tuple.Item2 > 1)
                                                    .Select(tuple => Tuple.Create(tuple.Item1,tuple.Item2-1))
                                                    .ToList();

            var verticals = allVerticalElements.ToLookup(e => (int)(e.LookupParameter("H").AsDouble().FeetToMeter().Round(2) * 100))
                                               .Select(kvp => new ElementQuantification($"Cuplock Vertical {kvp.Key / 100.0} m", kvp.Count(), level, kvp.Select(e => e.Id)))
                                               .ToList();


            var ledgers = elements.Where(e => e.Name == RevitBase.CUPLOCK_LEDGER)
                                  .ToLookup(e => (int)(e.LookupParameter("L").AsDouble().FeetToMeter().Round(2) * 100))
                                  .Select(kvp => new ElementQuantification($"Cuplock Ledger {kvp.Key / 100.0} m", kvp.Count(), level, kvp.Select(e => e.Id)))
                                  .ToList();

            var braces = elements.Where(e => e.Name == RevitBase.CUPLOCK_BRACING)
                                 .ToLookup(e =>
                                 {
                                     var h = e.LookupParameter("H").AsDouble();
                                     var w = e.LookupParameter("W").AsDouble();
                                     return (int)(Math.Sqrt(h * h + w * w).FeetToMeter().Round(2) * 100);
                                 })
                                 .Select(kvp => new ElementQuantification($"Scaffolding Tube {kvp.Key / 100.0} m", kvp.Count(), level, kvp.Select(e => e.Id)))
                                 .ToList();

            allCuplockElements.AddRange(verticals);
            allCuplockElements.AddRange(ledgers);
            allCuplockElements.AddRange(braces);

            if(verticalPoints.Count > 0)
            {
                var roundSpigotCount = verticalPoints.Sum(tuple => tuple.Item2 * 1);
                var rivetPinAndSpringClipCount = verticalPoints.Sum(tuple => tuple.Item2 * 2);
                var cuplocksId = allVerticalElements.Select(e => e.Id);

                var roundSpigot = new ElementQuantification("Round Spigot", roundSpigotCount, level, cuplocksId);
                var rivetPin = new ElementQuantification("Rivet Pin 16mm, L=9cm", rivetPinAndSpringClipCount, level, cuplocksId);
                var springClip = new    ElementQuantification("Spring Clip" , rivetPinAndSpringClipCount,level, cuplocksId);
                allCuplockElements.Add(roundSpigot);
                allCuplockElements.Add(rivetPin);
                allCuplockElements.Add(springClip);
            }

            if (braces.Count > 0)
            {
                //Each brace has one coupler at each end.
                var allIds = braces.SelectMany(b => b.Elements);
                var couplers = new ElementQuantification("Pressed Prop Swivel Coupler", braces.SelectMany(e => e.Elements).Count()*2, level, allIds);
                allCuplockElements.Add(couplers);
            }
            return allCuplockElements;
        }

        public static List<ElementQuantification> QueryProps(this IList<Element> elements, Level level)
        {
            var allPropElements = new List<ElementQuantification>();

            var e30Insts = elements.Where(e => e.Name == RevitBase.PROP_E30).ToList();
            var e35Insts = elements.Where(e => e.Name == RevitBase.PROP_E35).ToList();
            var d40Insts = elements.Where(e => e.Name == RevitBase.PROP_D40).ToList();
            var d45Insts = elements.Where(e => e.Name == RevitBase.PROP_D45).ToList();
            var propLegInsts = elements.Where(e => e.Name == RevitBase.PROP_LEG).ToList();

            if (e30Insts.Count > 0)
            {
                var e30s = new ElementQuantification("Acrow Prop E30", e30Insts.Count, level, e30Insts.Select(e => e.Id));
                allPropElements.Add(e30s);
            }
            if (e35Insts.Count > 0)
            {
                var e35s = new ElementQuantification("Acrow Prop E35", e35Insts.Count, level, e35Insts.Select(e => e.Id));
                allPropElements.Add(e35s);
            }
            if (d40Insts.Count > 0)
            {
                var d40s = new ElementQuantification("Acrow Prop D40", d40Insts.Count, level, d40Insts.Select(e => e.Id));
                allPropElements.Add(d40s);
            }
            if (d45Insts.Count > 0)
            {
                var d45s = new ElementQuantification("Acrow Prop D45", d45Insts.Count, level, d45Insts.Select(e => e.Id));
                allPropElements.Add(d45s);
            }
            if (propLegInsts.Count > 0)
            {
                var legs = new ElementQuantification("Prop Leg", propLegInsts.Count, level, propLegInsts.Select(e => e.Id));
                allPropElements.Add(legs);
            }

            return allPropElements;
        }

        public static List<ElementQuantification> QueryShores(this IList<Element> elements, Level level)
        {
            var allShoreElements = new List<ElementQuantification>();

            var shoreMainFrameInsts = elements.Where(e => e.Name == RevitBase.SHORE_BRACE_FRAME).ToList();
            var shoreTelescopicFrameInsts = elements.Where(e => e.Name == RevitBase.TELESCOPIC_FRAME).ToList();

            var allFramesPoints = shoreMainFrameInsts.Concat(shoreTelescopicFrameInsts)
                                                     .Select(e => (e.Location as LocationPoint).Point.CopyWithNewZ(0))
                                                     .GroupXYZ()
                                                     .Where(tuple => tuple.Item2 > 1)
                                                     .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2 - 1))
                                                     .ToList(); 

            if (shoreMainFrameInsts.Count > 0)
            {
                var mains = new ElementQuantification("Shorbrace Frame 120 cm * 180 cm", shoreMainFrameInsts.Count, level, shoreMainFrameInsts.Select(e => e.Id));
                allShoreElements.Add(mains);
            }
            if (shoreTelescopicFrameInsts.Count > 0)
            {
                var telescopics = new ElementQuantification("Shorbrace Telescopic Frame 120 cm * 165 cm", shoreTelescopicFrameInsts.Count, level, shoreTelescopicFrameInsts.Select(e => e.Id));
                allShoreElements.Add(telescopics);
            }

            if (allFramesPoints.Count>0)
            {
                var totalCount = allFramesPoints.Sum(tuple => tuple.Item2 * 1);
                var allIds = shoreMainFrameInsts.Concat(shoreTelescopicFrameInsts)
                                                .Select(e => e.Id);
                var revitPin = new ElementQuantification("Revit Pin 16 mm, L=9cm", totalCount, level, allIds);
                var springClip = new ElementQuantification("Spring Clip",totalCount, level, allIds);
                allShoreElements.Add(springClip);
                allShoreElements.Add(revitPin);
            }

            var braces = elements.Where(e => e.Name == RevitBase.CROSS_BRACE)
                        .ToLookup(e => (int)(e.LookupParameter("W").AsDouble().FeetToMeter().Round(2) * 100))
                        .Select(kvp => new ElementQuantification($"Cross Brace ({kvp.Key / 100.0} x 1.20m)", kvp.Count(), level, kvp.Select(e => e.Id)))
                        .ToList();

            allShoreElements.AddRange(braces);

            return allShoreElements;
        }


        /// <summary>
        /// Get Nearest X & Y Grid to column.
        /// </summary>
        /// <param name="col"></param>
        /// <param name="xAxes"></param>
        /// <param name="yAxes"></param>
        /// <returns></returns>
        public static string GetColumnNearestAxes(this Element col, IEnumerable<Tuple<string, Line>> xAxes, IEnumerable<Tuple<string, Line>> yAxes)
        {
            var point = (col.Location as LocationPoint).Point.CopyWithNewZ(0);
            var nearestXGrid = xAxes.OrderBy(tuple => ((tuple.Item2.Project(point)?.XYZPoint ?? XYZ.Zero) - point).GetLength())
                                    .FirstOrDefault()?.Item1 ?? "NA";
            var nearestYGrid = yAxes.OrderBy(tuple => ((tuple.Item2.Project(point)?.XYZPoint ?? XYZ.Zero) - point).GetLength())
                                    .FirstOrDefault()?.Item1 ?? "NA";
            return $"{nearestXGrid}-{nearestYGrid}";
        }


        /// <summary>
        /// Get the Nearest Grid Parallel to Beam.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="axes"></param>
        /// <returns></returns>
        public static string GetBeamNearestAxes(this Element beam, IEnumerable<Tuple<string, Line>> axes)
        {
            var beamLine = ((beam.Location as LocationCurve).Curve as Line).CopyWithNewZ(0);
            var start = beamLine.GetEndPoint(0).CopyWithNewZ(0);
            var end = beamLine.GetEndPoint(1).CopyWithNewZ(0);
            var centerPoint = (start + end) / 2;
            var nearestGrid = axes.Where(axis => axis.Item2.Direction.IsParallelTo(beamLine.Direction))
                                   .OrderBy(tuple => ((tuple.Item2.Project(centerPoint)?.XYZPoint ?? XYZ.Zero) - centerPoint).GetLength())
                                   .FirstOrDefault()?.Item1 ?? "NA";
            var prepAxes = axes.Where(axis => axis.Item2.Direction.IsPrepTo(beamLine.Direction));
            var nearestStartGrid = prepAxes.OrderBy(tuple => ((tuple.Item2.Project(start)?.XYZPoint ?? XYZ.Zero) - start).GetLength())
                                           .FirstOrDefault()?.Item1 ?? "NA";
            var nearestEndGrid = prepAxes.OrderBy(tuple => ((tuple.Item2.Project(end)?.XYZPoint ?? XYZ.Zero) - end).GetLength())
                                         .FirstOrDefault()?.Item1 ?? "NA";
            return $"{nearestGrid} - {nearestStartGrid}:{nearestEndGrid}";
        }

    }
}
