using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.DTOS.Revit.Input.Props;
using FormworkOptimize.Core.Entities.Cost.Interfaces;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.CostHelper;
using System;
using System.Collections.Generic;
using static FormworkOptimize.Core.Helpers.RevitHelper.PropsShoringHelper;
using Unit = System.ValueTuple;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class FloorPropsCost : IEvaluateCost
    {

        #region Private Fields

        private readonly Lazy<Validation<RevitProps>> _props;

        #endregion

        #region Properties

        public RevitFloorInput RevitInput { get; }

        public RevitFloorPropsInput PropsInput { get; }

        #endregion

        #region Constructors

        public FloorPropsCost(RevitFloorInput revitInput, RevitFloorPropsInput propsInput)
        {
            RevitInput = revitInput;
            PropsInput = propsInput;
            _props = new Lazy<Validation<RevitProps>>(() => FloorToProps(RevitInput, PropsInput));
        }

        #endregion

        #region Methods

        public List<ElementQuantificationCost> EvaluateCost(Func<string, double> costFunc) =>
            _props.Value.Match(errs => new List<ElementQuantificationCost>(),
                                                       props => props.ToCost(costFunc));

        public Validation<Unit> Draw(Document doc)
        {
            Action<RevitProps> draw = (props) =>
           {
               doc.UsingTransaction(_ => doc.LoadPropFamilies(), "Load Props Families");
               doc.UsingTransaction(_ => doc.LoadDeckingFamilies(), "Load Decking Families");
               doc.UsingTransaction(_ => props.Draw(doc), "Model Floor Props Shoring");
           };

            return _props.Value.Map(prop => draw.ToFunc()(prop));
        }

        #endregion

    }
}
