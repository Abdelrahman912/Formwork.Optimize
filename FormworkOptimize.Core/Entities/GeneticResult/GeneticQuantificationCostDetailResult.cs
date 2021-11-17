using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Constants.Database;

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
            ElementsCost = elesCost.Select(AsMonthCost).ToList();
        }

        private ElementQuantificationCost AsMonthCost(ElementQuantificationCost old)
        {
            var oUnitCost = old.CostType == Enums.CostType.RENT ? old.OptimizeUnitCost * NO_DAYS_PER_MONTH : old.OptimizeUnitCost;
            var iUnitCost = old.CostType == Enums.CostType.RENT ? old.InitialUnitCost * NO_DAYS_PER_MONTH : old.InitialUnitCost;
            return new ElementQuantificationCost(
                old.Name,
                old.Count,
                old.OptimizeTotalCost,
                oUnitCost,
                old.InitialTotalCost,
                iUnitCost,
                old.UnitCostMeasure,
                old.CostType
                );
        }

        public IEnumerable<GeneticReport> AsReport()
        {
            return new List<GeneticReport>();
        }
    }
}
