using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.Models;
using FormworkOptimize.Core.Entities.CostParameters;
using System;
using System.Collections.Generic;
using Unit = System.ValueTuple;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticFormworkViewModel
    {

        #region Private Fields

        #endregion

        #region Properties

        public GeneticSettingsViewModel GeneticSettingsVM { get; }

        public GeneticRunFormworkViewModel GeneticRunVM { get; }

        #endregion

        #region Constructors

        public GeneticFormworkViewModel(UIDocument uiDoc,
                                         Func<List<ResultMessage>, Unit> notificationService)
        {
            GeneticSettingsVM = new GeneticSettingsViewModel();
            GeneticRunVM = new GeneticRunFormworkViewModel(uiDoc, notificationService, CostParameterService);
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
