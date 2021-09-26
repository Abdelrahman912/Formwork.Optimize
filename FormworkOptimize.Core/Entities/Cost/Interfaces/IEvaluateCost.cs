using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using System;
using System.Collections.Generic;
using Unit = System.ValueTuple;

namespace FormworkOptimize.Core.Entities.Cost.Interfaces
{
    public interface IEvaluateCost
    {
        List<ElementQuantificationCost> EvaluateCost(Func<string,double> costFunc);

        Validation<Unit> Draw(Document doc);

    }
}
