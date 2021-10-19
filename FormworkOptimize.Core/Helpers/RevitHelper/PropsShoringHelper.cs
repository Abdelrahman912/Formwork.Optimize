using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using CSharp.Functional.Constructs;
using FormworkOptimize.Core.Comparers;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.DTOS.Revit.Input.Props;
using FormworkOptimize.Core.DTOS.Revit.Internal;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static CSharp.Functional.Extensions.ValidationExtension;
using static CSharp.Functional.Functional;
using static FormworkOptimize.Core.Errors.Errors;
using Unit = System.ValueTuple;
using static FormworkOptimize.Core.Constants.RevitBase;
using static FormworkOptimize.Core.Comparers.Comparers;


namespace FormworkOptimize.Core.Helpers.RevitHelper
{
    public static class PropsShoringHelper
    {
        private static RevitPropsVertical Draw(this RevitPropsVertical props, Document doc)
        {
            var symbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                                                           .Cast<FamilySymbol>();

            var propsSymbol = symbols.First(sym => sym.Name == Database.PropsTypes[props.PropType]).ActivateIfNot();
            var uHeadSymbol = symbols.First(sym => sym.Name == PROPS_U_HEAD).ActivateIfNot();

            var propsInst = doc.Create.NewFamilyInstance(props.Position, propsSymbol, props.HostLevel, StructuralType.NonStructural);
            propsInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(props.OffsetFromLevel);
            propsInst.LookupParameter("H").Set(props.Height);

            var uHeadInst = doc.Create.NewFamilyInstance(props.Position, uHeadSymbol, props.HostLevel, StructuralType.NonStructural);
            uHeadInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(props.OffsetFromLevel + props.Height + props.ULockDistance);
            uHeadInst.LookupParameter("Lock_Distance").Set(props.ULockDistance);
            ElementTransformUtils.RotateElement(doc, uHeadInst.Id, Line.CreateBound(props.Position, props.Position + XYZ.BasisZ), Math.Atan2(props.UVector.Y, props.UVector.X));

            return props;
        }

        private static RevitPropsLeg Draw(this RevitPropsLeg leg, Document doc)
        {
            var legSymbol = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                                                                 .Cast<FamilySymbol>()
                                                                 .First(sym => sym.Name == PROP_LEG)
                                                                 .ActivateIfNot();

            var legInst = doc.Create.NewFamilyInstance(leg.LocationPoint, legSymbol, leg.HostLevel, StructuralType.NonStructural);
            legInst.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(leg.OffsetFromLevel);

            return leg;
        }

        public static RevitProps Draw(this RevitProps props, Document doc)
        {
            props.Verticals.ForEach(v => v.Draw(doc));
            props.Legs.ForEach(leg => leg.Draw(doc));
            props.MainBeams.ForEach(mb => mb.Draw(doc));
            props.SecondaryBeams.ForEach(sb => sb.Draw(doc));
            return props;
        }

        public static List<RevitProps> Draw(this List<RevitProps> props, Document doc) =>
           props.Select(p => p.Draw(doc)).ToList();


        /// <summary>
        /// Encapsulating transformation functions for props creation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="hostLevel"></param>
        /// <param name="hostFloorOffset"></param>
        /// <param name="clearHeight"></param>
        /// <returns></returns>
        private static Validation<PropsCreation> GetPropsCreationFuncs(this RevitPropsInput input, Level hostLevel, double hostFloorOffset, double clearHeight,bool isColumn = false)
        {
            var mainBeamSection = Database.GetRevitBeamSection(input.MainBeamSection);

            var secBeamSection = Database.GetRevitBeamSection(input.SecondaryBeamSection);

            var plywoodThickness = Database.PlywoodFloorTypes[input.PlywoodSection].Item2;

            var overallVerticalClearHeight = clearHeight + 7.3.CmToFeet() - mainBeamSection.Height.CmToFeet() - secBeamSection.Height.CmToFeet() - plywoodThickness.MmToFeet();


            Func<double, double, PropsCreation> toPropsCreation = (vLength, uLockDist) =>
            {
                var mainBeamOffset = vLength + uLockDist - 7.3.CmToFeet() + hostFloorOffset;
                var plywoodOffset = mainBeamOffset + mainBeamSection.Height.CmToFeet() + secBeamSection.Height.CmToFeet() + plywoodThickness.MmToFeet();

                Func<XYZ, XYZ, RevitPropsVertical> verticalFunc = (point, uVector) =>
                     new RevitPropsVertical(input.PropType, vLength, uLockDist, point, hostLevel, hostFloorOffset, uVector);

                Func<DeckingRectangle, Tuple<List<RevitBeam>, List<RevitBeam>>> mainSecBeamsFunc = (rect) =>
                isColumn ? rect.ToBeams(hostLevel, mainBeamOffset, input.SecondaryBeamSpacing, mainBeamSection, secBeamSection):
                rect.ToBeamsExact(hostLevel, mainBeamOffset, input.SecondaryBeamSpacing, mainBeamSection, secBeamSection);

                Func<XYZ, RevitPropsLeg> legFunc = (point) => new RevitPropsLeg(point, hostLevel, hostFloorOffset);

                return new PropsCreation(verticalFunc, legFunc, mainSecBeamsFunc);
            };

            var propsCreation = from tuple in input.PropType.GetPropsVerticalLayout(overallVerticalClearHeight)
                                select toPropsCreation(tuple.VLength, tuple.ULockDist);

            return propsCreation;
        }

        /// <summary>
        /// Determining the vertical lengths of Uhead & Prop
        /// </summary>
        /// <param name="propType"></param>
        /// <param name="clearVerticalLength"></param>
        /// <returns></returns>
        public static Validation<(double VLength, double ULockDist)> GetPropsVerticalLayout(this EuropeanPropTypeName propType,
                                                                                double clearVerticalLength)
        {
            (double minExtension, double maxExtension) = Database.PropsMinMaxExtensions[propType];

            double clearVerticalLengthInCm = clearVerticalLength.FeetToCm();
            double minVLengthWithU = minExtension + PROPS_ULOCK_DISTANCE;
            double maxVLengthWithU = maxExtension + PROPS_ULOCK_DISTANCE;

            double uLockDist = PROPS_ULOCK_DISTANCE;

            if (minVLengthWithU <= clearVerticalLengthInCm &&
                maxVLengthWithU >= clearVerticalLengthInCm)
            {
                var vLength = clearVerticalLengthInCm - uLockDist;
                // vLength = Math.Round(clearVerticalLengthInCm - uLockDist);
                //uLockDist = clearVerticalLengthInCm - vLength;

                if (vLength >= maxExtension)
                {
                    return ShortPropType;
                    //uLockDist = clearVerticalLengthInCm - maxExtension;
                    //vLength = clearVerticalLengthInCm - uLockDist;
                }
                else
                {
                    return (VLength: vLength.CmToFeet(), ULockDist: uLockDist.CmToFeet());
                }
                
            }
            else
                return ShortPropType;

        }
        public static Validation<RevitProps> FloorToProps(RevitFloorInput revitInput, RevitFloorPropsInput floorPropsInput)
        {
            var beamDB = MIN_PROPS_SPACING.CmToFeet().GetAtInterval(floorPropsInput.SpacingMain, PROPS_INTERVAL.CmToFeet());
            var database = MIN_PROPS_SPACING.CmToFeet().GetAtInterval(Math.Max(floorPropsInput.SpacingMain,floorPropsInput.SpacingSecondary), PROPS_INTERVAL.CmToFeet());

            Func<PropsCreation, Validation<RevitProps>> toProps = propsCreation =>
           {
               var props = revitInput.ConcreteFloor.Boundary.OffsetInsideBy(floorPropsInput.BoundaryLinesOffset)
                                                                .DivideOptimized(revitInput.MainBeamDir,floorPropsInput.SpacingMain, floorPropsInput.SpacingSecondary,database)
                                                                 .FilterBeamsOptimized(revitInput.Beams, floorPropsInput.BeamOffset, beamDB)
                                                                .FilterOpenings(revitInput.ConcreteFloor.Openings)
                                                                .Filter(revitInput.Columns.Select(c => c.Item2.CornerPoints.Offset(c.Item1)).ToList())
                                                                .ToProps(propsCreation, floorPropsInput.MainBeamTotalLength, floorPropsInput.SecondaryBeamTotalLength, revitInput.AdjustLayout, true);
               return props;
           };

            var floorProps = from floorPropsCreation in floorPropsInput.GetPropsCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.FloorClearHeight)
                             from validProps in toProps(floorPropsCreation)
                             select validProps;
            return floorProps;
        }

        public static Validation<List<RevitProps>> BeamsToProps(RevitBeamInput revitInput, RevitBeamPropsInput beamPropsInput)
        {
            Func<PropsCreation, Tuple<double, double>, Validation<List<RevitProps>>> beamsToProps = (propsCreation, mainSecTuple) =>
             {
                 (var mainLength, var secLength) = mainSecTuple;
                 var database = MIN_PROPS_SPACING.CmToFeet().GetAtInterval(beamPropsInput.SpacingMain, PROPS_INTERVAL.CmToFeet());
                 var beamsProps = revitInput.Beams.GetBeamsWithClearSpan(revitInput.Columns)
                                                  .Divide(beamPropsInput.SpacingMain, beamPropsInput.SpacingSecondary,database)
                                                  .Select(tuple => tuple.Item2.ToProps(propsCreation, mainLength, secLength,DeckingHelper.AdjustLayout, false, tuple.Item1))
                                                  .ToList()
                                                  .PopOutValidation();
                 return beamsProps;
             };

            var props = from beamsPropsCreation in beamPropsInput.GetPropsCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.Beams.First().ClearHeight)
                        from valid in beamsToProps(beamsPropsCreation, Tuple.Create(beamPropsInput.MainBeamTotalLength, beamPropsInput.SecondaryBeamTotalLength))
                        select valid;

            return props;
        }
        
        public static Validation<List<RevitProps>> ColumnsToProps(RevitColumnInput revitInput, RevitColumnPropsInput columnPropsInput)
        {
            var finalProps = new List<Validation<RevitProps>>();
            if (revitInput.ColumnsWNoDrop.Count > 0)
            {
                Func<PropsCreation, Unit> action = propsCreation =>
                 {
                     var floorProps = revitInput.ColumnsWNoDrop.Select(c => c.Item1.ToProps(propsCreation, c.Item1.CornerPoints.Offset(c.Item2)));
                     finalProps.AddRange(floorProps);
                     return Unit();
                 };

                var result = from floorPropsCreation in columnPropsInput.GetPropsCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.FloorClearHeight,true)
                             select action(floorPropsCreation);

                result.Match(errs => finalProps.Add(Invalid(errs)), _ => { });
            }

            if (revitInput.ColumnsWDrop.Count > 0)
            {
                Func<PropsCreation, Unit> action = propsCreation =>
               {
                   var dropcuplocks = revitInput.ColumnsWDrop.Select(c => c.ToProps(propsCreation, c.Drop));
                   finalProps.AddRange(dropcuplocks);
                   return Unit();
               };

                var result = from dropPropsCreation in columnPropsInput.GetPropsCreationFuncs(revitInput.HostLevel, revitInput.HostFloorOffset, revitInput.DropClearHeight)
                             select action(dropPropsCreation);

                result.Match(errs => finalProps.Add(Invalid(errs)), _ => { });
            }

            return finalProps.PopOutValidation();
            //(var props, var errors) = finalProps.Aggregate(Tuple.Create(new List<RevitProps>(), new List<Error>()), (soFar, current) =>
            //{
            //    current.Match(errs => { soFar.Item2.AddRange(errs); Unit(); }, prop => { soFar.Item1.Add(prop); Unit(); });
            //    return soFar;
            //});

            //if (errors.Count > 0)
            //    return Invalid(errors);
            //else
            //    return Valid(props);
        }

        private static Validation<RevitProps> ToProps(this List<DeckingRectangle> rectangles,
                                                      PropsCreation propsCreation,
                                                      double mainBeamTotalLength,
                                                      double secBeamTotalLength,
                                                      Func<List<RevitBeam>, double, Validation<List<RevitBeam>>> beamLayoutAdjustFunc,
                                                      bool isDrawingFloor,
                                                      Line concBeamLine = null)
        {
            var verticals = rectangles.SelectMany(rect => rect.Points.Select(p => Tuple.Create(p, rect.MainBeamDir)))
                                                     .Distinct(GenericComparer.Create<Tuple<XYZ, XYZ>>((p1, p2) => p1.Item1.IsEqual(p2.Item1),tuple=>tuple.Item1.GetHash()))
                                                     .ToList()
                                                     .Select(tuple => propsCreation.VerticalFunc(tuple.Item1, tuple.Item2))
                                                     .ToList();

            (var mBeams, var sBeams) = rectangles.Aggregate(Tuple.Create(new List<RevitBeam>(), new List<RevitBeam>()), (soFar, current) =>
           {
               var beamsTuple = propsCreation.MainSecBeamsFunc(current);
               soFar.Item1.AddRange(beamsTuple.Item1);
               soFar.Item2.AddRange(beamsTuple.Item2);
               return soFar;
           });

            var maxLengthMainBeam = mBeams.OrderByDescending(b => b.Length).First();
            var maxLengthSecBeam = sBeams.OrderByDescending(b => b.Length).First();

            if (mainBeamTotalLength < maxLengthMainBeam.Length + 2 * MIN_CANTILEVER_LENGTH.CmToFeet() || secBeamTotalLength < maxLengthSecBeam.Length + 2 * MIN_CANTILEVER_LENGTH.CmToFeet())
                return ShortBeam;

            var mainBeams = beamLayoutAdjustFunc(mBeams, mainBeamTotalLength);

            var newSBeams = mBeams.Distinct(RevitBeamComparer).ToColinears()
                                   .Select(g => g.ReduceColinearBeams(g.Sum(m => m.Length)))
                                   .ToList()
                                   .PopOutValidation().Map(bs=>bs
                                   .MatchMainBeams()
                                   .SelectMany(rect => propsCreation.MainSecBeamsFunc(rect).Item2)
                                   .ToList());

            var secBeams = newSBeams = newSBeams.Bind(bs=> beamLayoutAdjustFunc(bs, secBeamTotalLength));


            var legsPoints = isDrawingFloor ? rectangles.Arrange() : rectangles.Arrange(concBeamLine);


            var legs = legsPoints.Select(p => propsCreation.LegFunc(p))
                                 .ToList();
            var props = from validMainBeams in mainBeams
                        from validSecBeams in secBeams
                        select new RevitProps(verticals, legs, validMainBeams, validSecBeams);
            return props;
        }

        private static Validation<RevitProps> ToProps(this ConcreteColumn column,
                                              PropsCreation propsCreation,
                                              FormworkRectangle propsRect)
        {
            var verticals = propsRect.Points.Select(p => propsCreation.VerticalFunc(p, XYZ.BasisX))
                                            .ToList();

            (var mainBeams, var secBeams) = propsCreation.MainSecBeamsFunc(new DeckingRectangle(propsRect.Points, XYZ.BasisX));
            //var secBeamsB = beamsTuple.Item2.First().Section.Breadth.CmToFeet();
            var secBeamsWoColumnInt = secBeams.Where(b => !b.IsBeamIntersectWithRectangle(column.CornerPoints)).ToList();
            var mainBeamSec = mainBeams.First().Section.SectionName;
            var secBeamSec = secBeams.First().Section.SectionName;

            Func<double, double, Validation<RevitProps>> toProps = (mainLength, secLength) =>
            {
                var mergedMainBeams = mainBeams.ToColinears()
                                             .SelectMany(group => group.MergeColinearBeams(mainLength))
                                             .ToList()
                                             .PopOutValidation();

                var mergedSecBeams = secBeamsWoColumnInt.ToColinears()
                                                          .SelectMany(group => group.MergeColinearBeams(secLength))
                                                          .ToList()
                                                          .PopOutValidation();

                var legs = propsRect.Points
                               .Select(p => propsCreation.LegFunc(p))
                               .ToList();
                var result = from validMainBeams in mergedMainBeams
                             from validSecBeams in mergedSecBeams
                             select new RevitProps(verticals, legs, validMainBeams, validSecBeams);
                return result;
            };

            var props = from mainLength in mainBeamSec.GetNearestDbLength(propsRect.LengthX)
                        from secLength in secBeamSec.GetNearestDbLength(propsRect.LengthY)
                        from validProps in toProps(mainLength, secLength)
                        select validProps;

            return props;
        }

        private static List<XYZ> Arrange(this List<DeckingRectangle> allFloorDeckingRects)
        {
            var points = allFloorDeckingRects.SelectMany(rect => rect.Lines.Where(l => l.Direction.IsParallelTo(XYZ.BasisX)))
                                             .GroupBy(l => (int)(l.GetEndPoint(0).Y * FORMWORK_NUMBER))
                                             .OrderBy(kvp => kvp.Key)
                                             .Select(kvp => kvp.ToList())
                                             .Where((ls, index) => index % 2 == 0)
                                             .SelectMany(ls => ls)
                                             .SelectMany(l => new List<XYZ>() { l.GetEndPoint(0), l.GetEndPoint(1) })
                                             .Distinct(XYZComparer)
                                             .ToList();

            return points;
        }

        private static List<XYZ> Arrange(this List<DeckingRectangle> beamDeckingRects, Line concBeamLine)
        {
            var points = beamDeckingRects.SelectMany(rect => rect.Lines.Where(l => l.Direction.IsPrepTo(concBeamLine.Direction)))
                                             .OrderBy(l => l.MidPoint().DistanceTo(concBeamLine.GetEndPoint(0)))
                                             .Distinct(LineComparer)
                                             .Where((ls, index) => index % 2 == 0)
                                             .SelectMany(l => new List<XYZ>() { l.GetEndPoint(0), l.GetEndPoint(1) })
                                             .ToList();

            return points;
        }

        public static List<RevitProps> FilterFromExtesnionBoundries(this List<RevitProps> props, List<Line> extensionBoundries)
        {
            var zeroZLines = extensionBoundries.Select(l => l.CopyWithNewZ(0))
                                               .ToList();
            return props.Select(prop => prop.FilterFromExtesnionBoundries(zeroZLines))
                           .ToList();
        }


        public static RevitProps FilterFromExtesnionBoundries(this RevitProps props, List<Line> extensionBoundries)
        {
            var zeroZLines = extensionBoundries.Select(l => l.CopyWithNewZ(0)).ToList();
            Func<XYZ, List<Line>, bool> isPointOnAnyLine = (p, lines) => lines.Any(l => p.IsOnLine(l));
            var filteredVerticals = props.Verticals.Where(v => !isPointOnAnyLine(v.Position.CopyWithNewZ(0), zeroZLines))
                                                     .ToList();

            var filteredLegs = props.Legs.Where(l => !isPointOnAnyLine(l.LocationPoint.CopyWithNewZ(0), zeroZLines))
                                                     .ToList();

            var filteredMainBeams = props.MainBeams.Where(b => !isPointOnAnyLine(b.StartPoint.CopyWithNewZ(0), zeroZLines.Where(bl => bl.Direction.IsParallelTo(b.Direction)).ToList()))
                                                     .ToList();

            var filteredSecBeams = props.SecondaryBeams.Where(b => !isPointOnAnyLine(b.StartPoint.CopyWithNewZ(0), zeroZLines.Where(bl => bl.Direction.IsParallelTo(b.Direction)).ToList()))
                                                         .ToList();

            return new RevitProps(filteredVerticals, filteredLegs, filteredMainBeams, filteredSecBeams);
        }
    }
}
