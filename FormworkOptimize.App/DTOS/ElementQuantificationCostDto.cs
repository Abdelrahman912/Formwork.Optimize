using CsvHelper.Configuration.Attributes;

namespace FormworkOptimize.App.DTOS
{
    public class ElementQuantificationCostDto
    {
        [Name("Element Name")]
        public string Name { get; set; }

        public int Count { get; set; }

        [Name("Cost Type")]
        public string CostType { get; set; }

        [Name("Optimize Unit Cost")]
        public double OptimizeUnitCost { get; set; }

        [Name("Optimize Total Cost")]
        public double OptimizeTotalCost { get; set; }

        [Name("Intial Total Cost")]
        public double InitialUnitCost { get; set; }

        [Name("Intial Total Cost")]
        public double InitialTotalCost { get; set; }

        [Name("Unit Cost Measure")]
        public string UnitCostMeasure { get; set; }
    }
}
