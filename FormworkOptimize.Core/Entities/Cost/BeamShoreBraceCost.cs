using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.DTOS.Revit.Input.Shore;
using FormworkOptimize.Core.Entities.Cost.Interfaces;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.CostHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Helpers.RevitHelper.ShoreHelper;
using Unit = System.ValueTuple;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class BeamShoreBraceCost : IEvaluateCost
    {

        #region Private Fields

        private readonly Lazy<Validation<List<RevitShore>>> _shore;

        #endregion

        #region Properties

        public RevitBeamInput RevitInput { get;}

        public RevitBeamShoreInput ShoreInput { get; }

        #endregion

        #region Constructors

        public BeamShoreBraceCost(RevitBeamInput revitInput, RevitBeamShoreInput shoreInput)
        {
            RevitInput = revitInput;
            ShoreInput = shoreInput;
            _shore = new Lazy<Validation<List<RevitShore>>>(() => BeamsToShore(RevitInput, ShoreInput));
        }

        #endregion

        #region Methods

        public List<ElementQuantificationCost> EvaluateCost(Func<string, double> costFunc) =>
            _shore.Value.Match(errs => new List<ElementQuantificationCost>(),
                                                      shores => shores.SelectMany(shore => shore.ToCost(costFunc)).ToList());

        public Validation<Unit> Draw(Document doc)
        {
            Action< List<RevitShore>> draw = ( shore) =>
            {
                doc.UsingTransaction(_ => doc.LoadShoreBraceFamilies(), "Load Shore Brace Families");
                doc.UsingTransaction(_ => doc.LoadDeckingFamilies(), "Load Decking Families");
                doc.UsingTransaction(_ => shore.Draw(doc), "Model Floor Shore Brace Shoring");


            };
           return _shore.Value.Map(shore => draw.ToFunc()(shore));
        }


        #endregion
    }
}
