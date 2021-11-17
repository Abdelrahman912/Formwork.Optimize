using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticResult
{
    public class GeneticQuantificationCostDetailResult : IGeneticDetailResult
    {
        public string Name { get; }

        public string ShoringName { get;  }

        public List<ElementQuantificationCost>  ElementsCost { get; }

        public GeneticQuantificationCostDetailResult(List<ElementQuantificationCost> elesCost,string shoringName)
        {
            Name = $"Formwork Elements Quantification Detail Result ({shoringName}).";
            ShoringName = shoringName;
            ElementsCost = elesCost;
        }

        public IEnumerable<GeneticReport> AsReport()
        {
            return new List<GeneticReport>();
        }
    }
}
