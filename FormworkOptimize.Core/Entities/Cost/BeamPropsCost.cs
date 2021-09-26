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
using System.Linq;
using static FormworkOptimize.Core.Helpers.RevitHelper.PropsShoringHelper;
using Unit = System.ValueTuple;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class BeamPropsCost : IEvaluateCost
    {

        #region Private Fields

        private readonly Lazy<Validation<List<RevitProps>>> _props;

        #endregion

        #region Properties

        public RevitBeamInput RevitInput { get; }

        public RevitBeamPropsInput BeamInput { get; }

        #endregion

        #region Constructors

        public BeamPropsCost(RevitBeamInput revitInput, RevitBeamPropsInput beamInput)
        {
            RevitInput = revitInput;
            BeamInput = beamInput;
            _props = new Lazy<Validation<List<RevitProps>>>(() => BeamsToProps(RevitInput, BeamInput));
        }

        #endregion

        #region Methods

        public List<ElementQuantificationCost> EvaluateCost(Func<string, double> costFunc) =>
            _props.Value.Match(errs => new List<ElementQuantificationCost>(),
                                                     props => props.SelectMany(prop => prop.ToCost(costFunc)).ToList());

        public Validation<Unit> Draw(Document doc)
        {
            Action< List<RevitProps>> draw = ( props) =>
            {
                doc.UsingTransaction(_ => doc.LoadPropFamilies(), "Load Props Families");
                doc.UsingTransaction(_ => doc.LoadDeckingFamilies(), "Load Decking Families");
                doc.UsingTransaction(_ => props.Draw(doc), "Model Floor Props Shoring");
            };

          return  _props.Value.Map(prop => draw.ToFunc()(prop));
        }


        #endregion

    }
}
