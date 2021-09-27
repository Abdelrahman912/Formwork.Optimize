using Autodesk.Revit.UI;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.Models;
using FormworkOptimize.Core.Entities.CostParameters;
using FormworkOptimize.Core.Entities.GeneticParameters;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public GeneticRunFormworkViewModel GeneticRunVM { get; }

        #endregion

        #region Constructors

        public GeneticFormworkViewModel(UIDocument uiDoc,
                                         Func<List<ResultMessage>, Unit> notificationService)
        {
            GeneticSettingsVM = new GeneticSettingsViewModel();
            GeneticRunVM = new GeneticRunFormworkViewModel(uiDoc, notificationService, CostParameterService, IncludedElementsService);
        }



        #endregion

        #region Methods


        private Validation<GeneticIncludedElements> IncludedElementsService()
        {
            var includedPlywoods = GeneticSettingsVM.IncludedPlywoodsVM.Plywoods.Where(p => p.IsSelected).Select(p=>p.Plywood).ToList();
            if (includedPlywoods.Count <= 1)
                return LessThanTwoPlywood;
            var includedBeamSections = GeneticSettingsVM.IncludedBeamSectionsVM.BeamSections.Where(bs=>bs.IsSelected).Select(bs=>bs.BeamSection).ToList();
            if(includedBeamSections.Count <= 1)
                return LessThanTwoBeamSection;
            return new GeneticIncludedElements(includedPlywoods, includedBeamSections);
        }

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
