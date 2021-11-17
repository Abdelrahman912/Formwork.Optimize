using FormworkOptimize.App.DTOS;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.App.Extensions
{
    public static class DtoExtension
    {
        public static ElementQuantificationCostDto AsDto(this ElementQuantificationCost eleCost)
        {
            return new ElementQuantificationCostDto()
            {
                Name = eleCost.Name,
                Count = eleCost.Count,
                CostType = eleCost.CostType.GetDescription(),
                OptimizeUnitCost = eleCost.CostType == CostType.PURCHASE ? eleCost.OptimizeUnitCost : eleCost.OptimizeUnitCost * NO_DAYS_PER_MONTH,
                OptimizeTotalCost = eleCost.OptimizeTotalCost,
                InitialUnitCost = eleCost.CostType == CostType.PURCHASE ? eleCost.InitialUnitCost : eleCost.InitialUnitCost * NO_DAYS_PER_MONTH,
                InitialTotalCost = eleCost.InitialTotalCost,
                UnitCostMeasure = eleCost.UnitCostMeasure.GetDescription(),
            };
        }

        public static List<ElementQuantificationCostDto> AsDtos(this IEnumerable<ElementQuantificationCost> eleCosts) =>
            eleCosts.Select(ele => ele.AsDto()).ToList();

    }
}
