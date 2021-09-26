using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Errors.Errors;
using static CSharp.Functional.Extensions.ValidationExtension;
using static CSharp.Functional.Extensions.ExceptionalExtension;

namespace FormworkOptimize.Core.Extensions
{
    public static class UIDocumentExtension
    {

        public static Exceptional<List<Line>> GetLinesFromDetailLines(this UIDocument uiDoc, string promptMessage)
        {
            try
            {
                var doc = uiDoc.Document;
                var refs = uiDoc.Selection.PickObjects(ObjectType.Element,
                                                       Filters.DetailLineFilter,
                                                       promptMessage);
                var detailLines = refs.Select(r => doc.GetElement(r))
                                      .OfType<DetailLine>()
                                      .Select(dl => dl.GeometryCurve as Line)
                                      .ToList();
                return detailLines;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException e)
            {
                return e;
            }
        }

        public static Exceptional<Validation<List<Line>>> GetPolygonFromDetailLines(this UIDocument uiDoc) =>
            uiDoc.GetLinesFromDetailLines("Select Closed Shape Detail Lines")
                 .Map(lines => lines.ValidateClosedPolygon(uiDoc.Document));

        public static Exceptional<Validation<List<Line>>> GetOpeningFromDetailLines(this UIDocument uiDoc, List<Line> boundary)
        {
            return uiDoc.GetLinesFromDetailLines("Select Closed Shape Detail Lines that represents opening")
                   .Map(detailLines =>
                   {
                       if (detailLines.Count == 0)
                           return Valid(new List<Line>());
                       return detailLines.ValidateClosedPolygon(uiDoc.Document)
                                        .Bind(opening =>
                                        {
                                            if (!opening.Select(l => l.GetEndPoint(0).CopyWithNewZ(0)).ToList().IsPolygonInside(boundary.Select(l => l.GetEndPoint(0).CopyWithNewZ(0)).ToList()))
                                                return InvalidOpening;
                                            return Valid(opening);
                                        });

                   });
        }


        public static Exceptional<List<Line>> GetBoundryLinesFromDetailLines(this UIDocument uiDoc) =>
           uiDoc.GetLinesFromDetailLines("Select Extesnion Boundry Lines");

        /// <summary>
        /// Get RevitFloor(Boundary , opening) from Detail Lines selection.
        /// </summary>
        /// <returns></returns>
        public static Exceptional<Validation<RevitLineFloor>> GetRevitFloorFromDetailLines(this UIDocument uiDoc)
        {
            return uiDoc.GetPolygonFromDetailLines()
                    .Bind(boundaryValidtion =>
                    {
                        return boundaryValidtion.Match(errors => Exceptional(Invalid<RevitLineFloor>(errors)), boundary =>
                        {
                            return uiDoc.GetOpeningFromDetailLines(boundary)
                                      .Map(openingValidation => openingValidation.Map(o => new RevitLineFloor(boundary, new List<List<Line>>() { o })));
                        });

                    });
        }

        public static Exceptional<Line> PickLine(this UIDocument uiDoc,  string promptMessage)
        {
            try
            {
                var doc = uiDoc.Document;
                var refer = uiDoc.Selection.PickObject(ObjectType.Edge,promptMessage);
               var edge =  doc.GetElement(refer).GetGeometryObjectFromReference(refer) as Edge;
                return edge.AsCurve() as Line;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException e)
            {
                return e;
            }
        }

        public static Exceptional<Element> PickElement(this UIDocument uiDoc, ISelectionFilter filter, string promptMessage)
        {
            try
            {
                var doc = uiDoc.Document;
                return doc.GetElement(uiDoc.Selection.PickObject(ObjectType.Element, filter, promptMessage));
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException e)
            {
                return e;
            }
        }


        public static Exceptional<List<Element>> PickElements(this UIDocument uiDoc, ISelectionFilter filter, string promptMessage)
        {
            try
            {
                var doc = uiDoc.Document;
                return uiDoc.Selection.PickObjects(ObjectType.Element, filter, promptMessage)
                                      .Select(r => doc.GetElement(r))
                                      .ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException e)
            {
                return e;
            }
        }
    }
}
