using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticSettingsViewModel:ViewModelBase,IValidateViewModel
    {

        #region Properties

        public ManPowerViewModel ManPowerVM { get; }

        public EquipmentsViewModel EquimentsVM { get; }

        public TransportationViewModel TransportationVM { get;  }

        public TimeParametersViewModel TimeParametersVM { get;}

        public ExcelCostFileViewModel ExcelCostFileVM { get; }

        public GeneticIncludedPlywoodsViewModel IncludedPlywoodsVM { get;  }

        public GeneticIncludedBeamSectionsViewModel IncludedBeamSectionsVM { get;  }

        #endregion

        #region Constructors

        public GeneticSettingsViewModel()
        {
            ManPowerVM = new ManPowerViewModel();
            EquimentsVM = new EquipmentsViewModel();
            TransportationVM = new TransportationViewModel();
            TimeParametersVM = new TimeParametersViewModel();
            ExcelCostFileVM = new ExcelCostFileViewModel();
            IncludedPlywoodsVM = new GeneticIncludedPlywoodsViewModel();
            IncludedBeamSectionsVM = new GeneticIncludedBeamSectionsViewModel();
        }

        public Validation<ValueTuple> Validate()
        {
          return  ManPowerVM.Validate()
                            .Bind(_ => EquimentsVM.Validate())
                            .Bind(_ => TransportationVM.Validate())
                            .Bind(_ => TimeParametersVM.Validate());
        }

        #endregion

    }
}
