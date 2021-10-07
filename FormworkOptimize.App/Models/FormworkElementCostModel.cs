using FormworkOptimize.App.Models.Base;
using FormworkOptimize.App.Models.Enums;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Models
{
    public class FormworkElementCostModel:ModelBase
    {

        #region Private Fields

        private double _price;

        private CostType _costType;

        private int _numberOfUses;

       

        #endregion

        #region Properties

        public FormworkCostElements Name { get;  }

        public double Price
        {
            get => _price;
            set
            {
               var isChanged =  NotifyPropertyChanged(ref _price,value);
                Status = isChanged ? ModelStatus.MODIFIED : ModelStatus.UPTODATE; 
            }
        }

        public UnitCostMeasure UnitCostMeasure { get;  }

        public CostType CostType
        {
            get => _costType;
            set
            {
                var isChanged = NotifyPropertyChanged(ref _costType,value);
                Status = isChanged ? ModelStatus.MODIFIED : ModelStatus.UPTODATE;
            }
        }

        public int NumberOfUses
        {
            get => _numberOfUses;
            set
            {
                var isChanged = NotifyPropertyChanged(ref _numberOfUses,value);
                Status = isChanged ? ModelStatus.MODIFIED :ModelStatus.UPTODATE;
            }
        }

        #endregion

        #region Cosntructors

        public FormworkElementCostModel(FormworkCostElements name, double price, UnitCostMeasure unitCostMeasure, CostType costType, int numberOfUses)
        {
            Name = name;
            Price = price;
            UnitCostMeasure = unitCostMeasure;
            CostType = costType;
            NumberOfUses = numberOfUses;
        }

        #endregion

    }
}
