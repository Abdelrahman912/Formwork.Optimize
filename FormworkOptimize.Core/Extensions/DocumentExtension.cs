using Autodesk.Revit.DB;
using CSharp.Functional;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Entities.Quantification;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.Helpers.RevitHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static CSharp.Functional.Extensions.ExceptionalExtension;
using static CSharp.Functional.Extensions.ValidationExtension;
using static FormworkOptimize.Core.Constants.RevitBase;
using static FormworkOptimize.Core.Errors.Errors;

namespace FormworkOptimize.Core.Extensions
{
    public static class DocumentExtension
    {

        public static Validation<View3D> GetDefault3DView(this Document doc)
        {
            var view = new FilteredElementCollector(doc).OfClass(typeof(View3D))
                                                        .OfType<View3D>()
                                                        .Where(v => v.Name == DEFAULT_3D_VIEW)
                                                        .FirstOrDefault();
            if (view == null)
                return Default3DViewNotFound;
            return view;
        }

        /// <summary>
        /// a Wrapper Method to Wrap the Transaction Group in a declartive way.
        /// </summary>
        /// <param name="doc">Revit model documnet.</param>
        /// <param name="action">Function to be invoked in the transaction.</param>
        /// <param name="transName">Transaction Name.</param>
        public static void UsingTransactionGroup(this Document doc, Action<TransactionGroup> action, string transName = "")
        {
            using (var transGroup = new TransactionGroup(doc, transName))
            {
                try
                {
                    transGroup.Start();
                    action?.Invoke(transGroup);
                    transGroup.Assimilate();
                }
                catch (Exception)
                {
                    transGroup.RollBack();
                    throw;
                }
            }
        }

        /// <summary>
        /// a Wrapper Method to Wrap the Transaction in a declartive way.
        /// </summary>
        /// <param name="doc">Revit model documnet.</param>
        /// <param name="action">Function to be invoked in the transaction.</param>
        /// <param name="transName">Transaction Name.</param>
        public static void UsingTransaction(this Document doc, Action<Transaction> action, string transName = "")
        {
            using (var trans = new Transaction(doc, transName))
            {
                try
                {
                    trans.Start();
                    action?.Invoke(trans);
                    trans.Commit();
                }
                catch (Exception e)
                {
                    var message = e.Message;
                    trans.RollBack();
                }
            }
        }

        /// <summary>
        /// a Wrapper Method to Wrap the Transaction in a declartive way.
        /// </summary>
        /// <param name="doc">Revit model documnet.</param>
        /// <param name="func">Function to be invoked in the transaction.</param>
        /// <param name="transName">Transaction Name.</param>
        public static T UsingTransaction<T>(this Document doc, Func<Transaction, T> func, string transName = "")
        {
            using (var trans = new Transaction(doc, transName))
            {
                try
                {
                    trans.Start();
                    var result = func.Invoke(trans);
                    trans.Commit();
                    return result;
                }
                catch (Exception e)
                {
                    var message = e.Message;
                    trans.RollBack();
                    return default(T);
                }
            }
        }


        public static Validation<T> CreatePlywoodElementType<T>(this Document doc, string typeName, double thickness)
            where T : HostObjAttributes
        {
            var eleTypes = new FilteredElementCollector(doc).OfClass(typeof(T))
                                                              .Cast<T>();

            var eleType = eleTypes.First() is WallType ? eleTypes.OfType<WallType>().Where(wt => wt.Kind == WallKind.Basic).First() as T : eleTypes.First();

            var material = new FilteredElementCollector(doc).OfClass(typeof(Material))
                                                            .FirstOrDefault(m => m.Name == RevitBase.PLYWOOD_MATERIAL);
            if (material == null)
                return MaterialNotFound;
            var newEleType = eleType.Duplicate(typeName) as T;
            var compundStructure = CompoundStructure.CreateSingleLayerCompoundStructure(MaterialFunctionAssignment.Structure, thickness, material.Id);
            compundStructure.EndCap = EndCapCondition.NoEndCap;
            newEleType.SetCompoundStructure(compundStructure);
            return newEleType;
        }


        /// <summary>
        /// Load required families if they are not in
        /// the revit document.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Name of families that are not exist in the revit document & whether they are loaded successfully or not</returns>
        private static List<Tuple<string, bool>> LoadFamilies(this Document doc, List<string> families)
        {
            var familiesFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Families";

            var allRevitFamilies = new FilteredElementCollector(doc).OfClass(typeof(Family)).ToElements();
            var result = families.Where(cf => !allRevitFamilies.Any(rf => rf.Name == cf))
                                  .Select(family => Tuple.Create(family, doc.LoadFamily($"{familiesFolder}\\{family}.rfa")))
                                  .ToList();
            return result;
        }

        public static List<Tuple<string, bool>> LoadCuplockFamilies(this Document doc) =>
            doc.LoadFamilies(RevitBase.CUPLOCK_FAMILIES);

        public static List<Tuple<string, bool>> LoadPropFamilies(this Document doc) =>
            doc.LoadFamilies(RevitBase.PROP_FAMILIES);

        public static List<Tuple<string, bool>> LoadShoreBraceFamilies(this Document doc) =>
            doc.LoadFamilies(RevitBase.SHORE_BRACE_FAMILIES);

        public static List<Tuple<string, bool>> LoadDeckingFamilies(this Document doc) =>
            doc.LoadFamilies(RevitBase.DECKING_FAMILIES);

        public static Validation<List<T>> AddPlywoodElementTypesIfNotExist<T>(this Document doc)
            where T : HostObjAttributes
        {
            var allElementTypes = new FilteredElementCollector(doc).OfClass(typeof(T))
                                                                 .ToElements()
                                                                 .Cast<T>();

            return Database.PlywoodFloorTypes.Aggregate(new List<Validation<T>>(), (soFar, current) =>
            {
                var tuple = current.Value;
                var plywoodType = allElementTypes.FirstOrDefault(ft => ft.Name == tuple.Item1);
                if (plywoodType != null)
                {
                    soFar.Add(plywoodType);
                }
                else
                {
                    var newElementType = doc.CreatePlywoodElementType<T>(tuple.Item1, tuple.Item2.MmToFeet());
                    soFar.Add(newElementType);
                }
                return soFar;
            }).PopOutValidation();
        }


        /// <summary>
        /// Get All Columns In Level.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="baseLevel">Level that host the supporting floor.</param>
        /// <param name="baseFloorOffset">Offset of the supporting floor from level.</param>
        /// <returns></returns>
        public static List<Element> GetAllColumnsInLevel(this Document doc, Level baseLevel, double baseFloorOffset)
        {

            var levelEle = baseLevel.Elevation;
            var zmin = levelEle + baseFloorOffset + 50.0.CmToFeet();
            var zmax = levelEle + baseFloorOffset + 100.0.CmToFeet();

            var allColumns = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns)
                                                              .WhereElementIsNotElementType()
                                                              .Where(ele => ele.Name != RevitBase.CUPLOCK_VERTICAL)
                                                              .ToList();

            if (allColumns.Count == 0)
                return new List<Element>();

            var minMax = allColumns.Select(col => (col.Location as LocationPoint).Point)
                                   .CreateXYMinMax(zmin, zmax);

            var outline = new Outline(minMax.Min, minMax.Max);

            var bbFilter = new BoundingBoxIntersectsFilter(outline);

            var columnsInLevel = new FilteredElementCollector(doc, allColumns.Select(col => col.Id).ToList()).WherePasses(bbFilter)
                                                                                                             .ToElements()
                                                                                                             .ToList();
            return columnsInLevel;
        }

        public static List<Element> GetAllBeamsInLevel(this Document doc, Level referenceLevel)
        {
            var beams = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming)
                                                          .WhereElementIsNotElementType()
                                                          .Where(ele => ele.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM).AsElementId().IntegerValue == referenceLevel.Id.IntegerValue)
                                                          .ToList();
            return beams;
        }

        public static List<Element> GetBeamsInLevelWithinPolygon(this Document doc, Level level, List<Line> polygon) =>
            doc.GetAllBeamsInLevel(level)
               .FilterBeamsInPolygon(polygon);

        public static List<Element> GetColumnsInLevelWithinPolygon(this Document doc, Level hostLevel, double hostFloorOffset, List<Line> polygon) =>
            doc.GetAllColumnsInLevel(hostLevel, hostFloorOffset)
               .FilterColumnsInPolygon(polygon);


        public static List<FormworkRectangle> GetDropPanels(this List<Line> floorBoundary, Floor hostFloor, Floor supportedFloor, Document doc, Options options)
        {
            var hostFloorTopEle = hostFloor.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble();
            var boundaryPoints = floorBoundary.ToPoints();
            var drops = new FilteredElementCollector(doc).OfClass(typeof(Floor))
                                                        .Cast<Floor>()
                                                        .Where(f => f.Id.IntegerValue != supportedFloor.Id.IntegerValue && f.Id.IntegerValue != hostFloor.Id.IntegerValue && f.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble() > hostFloorTopEle && (f.LevelId.IntegerValue == hostFloor.LevelId.IntegerValue || f.LevelId.IntegerValue == supportedFloor.LevelId.IntegerValue))
                                                        .OrderBy(f => f.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble())
                                                        .Select(f => f.GetGeometry(options))
                                                        .Where(f => f.Boundary.ToPoints().IsPolygonInside(boundaryPoints) && f.Boundary.Count == 4)
                                                        .Select(drop => drop.Boundary.ToRectangle())
                                                        .ToList();
            return drops;
        }

        public static Validation<RevitColumnInput> GetRevitColumnInput(this Document doc, List<Element> columnsWDrop, List<Tuple<Element, double>> columnsWNoDrop, Floor hostFloor, Floor supportedFloor)
        {
            return doc.GetDefault3DView()
                      .Map(view =>
                      {
                          var options = new Options();
                          options.ComputeReferences = true;
                          options.View = view;
                          return GetRevitColumnInput(doc, columnsWDrop, columnsWNoDrop, hostFloor, supportedFloor, options);
                      });
        }

        private static RevitColumnInput GetRevitColumnInput(this Document doc, List<Element> columnsWDrop, List<Tuple<Element, double>> columnsWNoDrop, Floor hostFloor, Floor supportedFloor, Options options)
        {
            var hostFloorTopEle = hostFloor.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble();
            var floors = new FilteredElementCollector(doc).OfClass(typeof(Floor))
                                                          .Cast<Floor>()
                                                          .Where(f => f.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble() > hostFloorTopEle && (f.LevelId.IntegerValue == hostFloor.LevelId.IntegerValue || f.LevelId.IntegerValue == supportedFloor.LevelId.IntegerValue))
                                                          .OrderBy(f => f.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble())
                                                          .Select(f => Tuple.Create(f, f.GetGeometry(options)));

            var hostLevel = doc.GetElement(hostFloor.LevelId) as Level;
            var hostFloorOffset = hostFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
            var columnsWDropPanels = columnsWDrop.Where(e => e.IsRectColumn(doc)).Select(c => c.ToConcreteColumn(doc))
                     .Aggregate(new List<Tuple<ConcreteColumnWDropPanel, double>>(), (soFar, current) =>
                     {
                         var result = floors.FirstOrDefault(f => f.Item1 != supportedFloor && current.Center.IsInPolygon(f.Item2.Boundary.ToPoints()));
                         if (result != null)
                             soFar.Add(Tuple.Create(new ConcreteColumnWDropPanel(current, new FormworkRectangle(result.Item2.Boundary.ToPoints())), hostFloor.GetClearHeight(result.Item1)));
                         return soFar;
                     }).ToList();
            var floorClearHeight = hostFloor.GetClearHeight(supportedFloor);
            var concreteColumnsWNoDrop = columnsWNoDrop.Where(tuple => tuple.Item1.IsRectColumn(doc)).Select(e => Tuple.Create(e.Item1.ToConcreteColumn(doc), e.Item2)).ToList();
            var concreteColumnsWDrop = columnsWDropPanels.Select(tuple => tuple.Item1).ToList();
            var dropClearHeight = columnsWDropPanels.Count == 0 ? 0 : columnsWDropPanels.First().Item2;
            return new RevitColumnInput(concreteColumnsWDrop, concreteColumnsWNoDrop, hostLevel, hostFloorOffset, floorClearHeight, dropClearHeight);
        }

        public static RevitBeamInput GetRevitBeamInput(this Document doc, List<Element> beams, Floor hostFloor)
        {
            var hostLevel = doc.GetElement(hostFloor.LevelId) as Level;
            var hostFloorOffset = hostFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
            var beamBoundingRect = beams.GetBoundingRectFromBeams();
            var columns = doc.GetColumnsInLevelWithinPolygon(hostLevel, hostFloorOffset, beamBoundingRect.Lines);
            return new RevitBeamInput(beams.Select(beam => beam.ToConcreteBeam(doc, hostFloor.GetClearHeight(beam))).ToList(),
                                     columns.Where(e => e.IsRectColumn(doc)).Select(c => c.ToConcreteColumn(doc)).ToList(),
                                     hostLevel,
                                     hostFloorOffset);
        }

        public static RevitFloorInput GetRevitFloorInput(this Document doc,
                                                         RevitFloor concreteFloor,
                                                         List<Tuple<double, Element>> columns,
                                                         Floor hostFloor,
                                                         Floor supportedFloor,
                                                         XYZ mainBeamDir)
        {
            var hostLevel = doc.GetElement(hostFloor.LevelId) as Level;
            var supportedLevel = doc.GetElement(supportedFloor.LevelId) as Level;
            var hostFloorOffset = hostFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
            var beams = doc.GetBeamsInLevelWithinPolygon(supportedLevel, concreteFloor.Boundary);
            var floorClearHeight = hostFloor.GetClearHeight(supportedFloor);

            Func<List<RevitBeam>, double, Validation<List<RevitBeam>>> adjustLayout = (concreteFloor is RevitLineFloor) ? DeckingHelper.AdjustLayout(doc, hostLevel)
                                                                                                          : DeckingHelper.AdjustLayout;

            return new RevitFloorInput(concreteFloor,
                                      columns.Where(tuple => tuple.Item2.IsRectColumn(doc)).Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2.ToConcreteColumn(doc))).ToList(),
                                     beams.Select(b => b.ToConcreteBeam(doc, 0/*we don't need clear height in floor*/)).ToList(),
                                     hostLevel,
                                     hostFloorOffset,
                                     floorClearHeight,
                                     mainBeamDir,
                                     adjustLayout);
        }

        public static List<ElementQuantification> Query(this Document doc, Level level)
        {
            var levelFilter = new ElementLevelFilter(level.Id);
            var multiCategoryFilter = new ElementMulticategoryFilter(new List<BuiltInCategory>() { BuiltInCategory.OST_StructuralStiffener, BuiltInCategory.OST_StructuralColumns });
            var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                                                             .OfClass(typeof(FamilyInstance))
                                                             .WherePasses(multiCategoryFilter)
                                                             .WherePasses(levelFilter)
                                                             .ToElements();
            return collector.QueryCuplocks(level)
                            .Concat(collector.QueryProps(level))
                            .Concat(collector.QueryShores(level))
                            .Concat(collector.QueryHeads(level))
                            .Concat(collector.QueryDeckingBeams(level))
                            .ToList();

        }

        /// <summary>
        /// Get All Axes in the revit document and divide them into two
        /// groups (X & Y).
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static (IEnumerable<Tuple<string, Line>> XAxes, IEnumerable<Tuple<string, Line>> YAxes) GetXYAxes(this Document doc)
        {
            var zeroZAxesLines = new FilteredElementCollector(doc).OfClass(typeof(Grid))
                                                           .Cast<Grid>()
                                                           .Select(g => Tuple.Create(g.Name, (g.Curve as Line).CopyWithNewZ(0)));
            var xAxes = zeroZAxesLines.Where(tuple => tuple.Item2.Direction.IsParallelTo(XYZ.BasisX)).ToList();
            var yAxes = zeroZAxesLines.Where(tuple => tuple.Item2.Direction.IsParallelTo(XYZ.BasisY)).ToList();
            return (XAxes: xAxes, YAxes: yAxes);
        }

        public static IEnumerable<FormworkBeamInstance> GetFormworkBeamInstances(this Document doc, Level level)
        {
            var levelFilter = new ElementLevelFilter(level.Id);


            var beams = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                                                            .OfCategory(BuiltInCategory.OST_StructuralStiffener)
                                                            .WherePasses(levelFilter)
                                                            .Where(fi => RevitBase.DECKING_SYMBOLS.Any(ds => ds == fi.Name))
                                                            .Select(fi => new FormworkBeamInstance(fi.Location as LocationPoint));

            return beams;
        }


        /// <summary>
        /// Suppresses the warning.
        /// </summary>
        /// <param name="trans">The trans.</param>
        /// <param name="failObj">The fail object.</param>
        public static void SuppressWarning(this Transaction trans, IFailuresPreprocessor failObj)
        {
            var failOpt = trans.GetFailureHandlingOptions();
            failOpt.SetFailuresPreprocessor(failObj);
            trans.SetFailureHandlingOptions(failOpt);
        }

    }
}
