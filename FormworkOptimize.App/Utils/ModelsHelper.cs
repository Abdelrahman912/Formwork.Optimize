using CSharp.Functional.Constructs;
using CSharp.Functional.Errors;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.Models.Enums;
using FormworkOptimize.App.UI.Services;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Utils
{
    public static  class ModelsHelper
    {

        public static ResultMessage ToResult(this Exceptional<string> message)=>
            message.Match(ex=>new ResultMessage(ex.Message,ResultMessageType.ERROR),
                          value=>new ResultMessage(value,ResultMessageType.DONE));

        public static ResultMessage ToResult(this Error err) =>
            new ResultMessage(err.Message, ResultMessageType.ERROR);

        public static FormworkElementCostModel AsModel(this FormworkElementCost eleCost)
        {
            return eleCost is RentFormworkElementCost ? (eleCost as RentFormworkElementCost).AsModel()
                                                      : (eleCost as PurchaseFormworkElementCost).AsModel();
        }

        public static FormworkElementCostModel AsModel(this RentFormworkElementCost eleCost)
        {
           var model = new FormworkElementCostModel(eleCost.Name,eleCost.Price,eleCost.UnitCost,CostType.RENT,1);
            model.Status = ModelStatus.UPTODATE;
            return model;
        }

        public static FormworkElementCostModel AsModel(this PurchaseFormworkElementCost eleCost)
        {
            var model = new FormworkElementCostModel(eleCost.Name, eleCost.Price, eleCost.UnitCost, CostType.PURCHASE, eleCost.NumberOfUses);
            model.Status = ModelStatus.UPTODATE;
            return model;
        }

        public static FormworkElementCost AsElementCost(this FormworkElementCostModel model)
        {
            if(model.CostType == CostType.RENT)
            {
                return new RentFormworkElementCost()
                {
                    Name = model.Name,
                    Price = model.Price,
                    UnitCost = model.UnitCostMeasure
                };
            }
            else
            {
                return new PurchaseFormworkElementCost()
                {
                    Name = model.Name,
                    Price = model.Price,
                    UnitCost = model.UnitCostMeasure,
                    NumberOfUses = model.NumberOfUses
                };
            }
        }

    }
}
