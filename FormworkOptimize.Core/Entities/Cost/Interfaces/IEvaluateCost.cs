using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using Unit = System.ValueTuple;

namespace FormworkOptimize.Core.Entities.Cost.Interfaces
{
    public interface IEvaluateCost
    {
        List<ElementQuantificationCost> EvaluateCost(Func<FormworkCostElements, FormworkElementCost> costFunc);

        Validation<Unit> Draw(Document doc);

    }
}
