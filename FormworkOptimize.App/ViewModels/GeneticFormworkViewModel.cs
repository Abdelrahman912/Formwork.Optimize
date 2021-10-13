using Autodesk.Revit.UI;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.Models;
using FormworkOptimize.Core.Entities.CostParameters;
using FormworkOptimize.Core.Entities.GeneticParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FormworkOptimize.Core.Errors.Errors;
using Unit = System.ValueTuple;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticFormworkViewModel
    {

        #region Private Fields

        #endregion

        #region Properties

        public GeneticSettingsViewModel GeneticSettingsVM { get; }

        public CostDbViewModel CostDbVM { get; }

        public GeneticRunFormworkViewModel GeneticRunVM { get; }

        public FormworkElementsIncludedInGeneticViewModel FormworkElementsIncludedVM { get;  }

        #endregion

        #region Constructors

        public GeneticFormworkViewModel(UIDocument uiDoc,
                                         Func<List<ResultMessage>, Unit> notificationService,
                                         Func<Func<string, Task<List<Exceptional<string>>>>, Option<Task<List<Exceptional<string>>>>> folderDialogService)
        {
            GeneticSettingsVM = new GeneticSettingsViewModel();
            CostDbVM = new CostDbViewModel(notificationService);
            FormworkElementsIncludedVM = new FormworkElementsIncludedInGeneticViewModel();
            GeneticRunVM = new GeneticRunFormworkViewModel(uiDoc, notificationService, CostParameterService, FormworkElementsIncludedVM.IncludedElementsService, folderDialogService);
        }


        #endregion

        #region Methods


       

        private Validation<CostParameter> CostParameterService(double floorArea, double smallerLength)
        {
            return GeneticSettingsVM.Validate().Map(_ =>
            {
                var manPower = GeneticSettingsVM.ManPowerVM.GetManPower(floorArea);

                return new CostParameter(
                    manPower: manPower,
                    equipments: GeneticSettingsVM.EquimentsVM.GetEquipments(),
                    time: GeneticSettingsVM.TimeParametersVM.GetTime(smallerLength, manPower),
                    transportation: GeneticSettingsVM.TransportationVM.GetTransportation()
                    );
            });
        }

        #endregion

    }
}
