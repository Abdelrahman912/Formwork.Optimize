using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;
using System;

namespace FormworkOptimize.Core.Entities.GeneticResult
{
    public class GeneticCostDetailResult:IGeneticDetailResult
    {

        #region Properties

        public string InstallationDuration { get;}

        public string SmitheryDuration { get;  }

        public string WaitingDuration { get; }

        public string RemovalDuration { get; }

        public string ManPowerCost { get; }

        public string EquipmentsCost { get; set; }

        public string FormworkElementsCost { get;}

        public string PlywoodCost { get;  }

        public string ManPowerInfo { get; }

        public string EquipmentsInfo { get; }

        public string FormworkInfo { get; }

        public string PlywoodInfo { get; }

        #endregion

        #region Constructors

        public GeneticCostDetailResult(FormworkTimeLine timeLine,
                                       ManPowerCost manPowerCost,
                                       EquipmentsCost equipmentCost,
                                       FormworkElementsCost formworkElemntsCost,
                                       PlywoodCost plywoodCost)
        {
            InstallationDuration = $"{timeLine.InstallationDuration} Days";
            SmitheryDuration = $"{timeLine.SmitheryDuration} Days";
            WaitingDuration = $"{timeLine.WaitingDuaration} Days";
            RemovalDuration = $"{timeLine.RemovalDuration} Days";
            ManPowerCost = $"{manPowerCost.LaborCost} * {manPowerCost.NoWorkers} * {manPowerCost.Duration} = {manPowerCost.TotalCost} LE";
            EquipmentsCost = $"{equipmentCost.Rent} * {equipmentCost.NoEquipments} * {equipmentCost.Duration} = {equipmentCost.TotalCost} LE";
            FormworkElementsCost = $"{Math.Round(formworkElemntsCost.Cost,2)} * {formworkElemntsCost.Duration} = {Math.Round(formworkElemntsCost.TotalCost,2)} LE";
            PlywoodCost = $"{Math.Round(plywoodCost.CostPerArea)} * {Math.Round(plywoodCost.Area, 2)} = {Math.Round(plywoodCost.TotalCost, 2)} LE";
            ManPowerInfo = "Labor Cost * No. Workers * (Install Time + Removal Time)";
            EquipmentsInfo = "Rent * No. Cranes * (Install Time + Removal Time)";
            FormworkInfo = "Rent * (Install Time + Smithery Time + Waiting Time + Removal Time)";
            PlywoodInfo = "Cost Per Square Meter * Total Area";
        }

        #endregion

    }
}
