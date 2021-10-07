using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.DTOS.Revit.Input;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.Entities.Cost.Interfaces;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.CostHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Helpers.RevitHelper.CuplockShoringHelper;
using Unit = System.ValueTuple;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class BeamCuplockCost : IEvaluateCost
    {

        #region Private Fields

        private readonly Lazy<Validation<List<RevitCuplock>>> _cuplock;

        #endregion

        #region Properties

        public RevitBeamInput RevitInput { get; }

        public RevitBeamCuplockInput CuplockInput { get; }

        #endregion

        #region Constructors

        public BeamCuplockCost(RevitBeamInput revitInput, RevitBeamCuplockInput cuplockInput)
        {
            RevitInput = revitInput;
            CuplockInput = cuplockInput;
            _cuplock = new Lazy<Validation<List<RevitCuplock>>>(() => BeamsToCuplock(RevitInput, CuplockInput));
        }

        #endregion

        #region Methods

        public List<ElementQuantificationCost> EvaluateCost(Func<FormworkCostElements, FormworkElementCost> costFunc) =>
            _cuplock.Value.Match(errs => new List<ElementQuantificationCost>(),
                                                          cuplocks => cuplocks.SelectMany(cuplock => cuplock.ToCost(costFunc)).ToList());

        public Validation<Unit> Draw(Document doc)
        {
            Action< List<RevitCuplock>> draw = (cuplock) =>
            {
                doc.UsingTransaction(_ => doc.LoadCuplockFamilies(), "Load Cuplock Families");
                doc.UsingTransaction(_ => doc.LoadDeckingFamilies(), "Load Decking Families");
                doc.UsingTransaction(_ => cuplock.Draw(doc), "Model Floor Cuplock Shoring");
            };
          return  _cuplock.Value.Map(cuplock =>draw.ToFunc()(cuplock));
        }


        #endregion

    }
}
