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

        public string TotalDuration { get; }

        public string ManPowerCost { get; }

        public string EquipmentsCost { get; set; }

        public string FormworkElementsCost { get;}

        public string PlywoodCost { get;  }

        public string TransportationCost { get; }

        public string TotalCost { get;  }

        public string ManPowerInfo { get; }

        public string EquipmentsInfo { get; }

        public string FormworkInfo { get; }

        public string PlywoodInfo { get; }

        public string TotalCostInfo { get; }

        public string TransportationCostInfo { get; }

        public bool IsTransportationIncluded { get;  }

        #endregion

        #region Constructors

        public GeneticCostDetailResult(FormworkTimeLine timeLine,
                                       ManPowerCost manPowerCost,
                                       EquipmentsCost equipmentCost,
                                       FormworkElementsCost formworkElemntsCost,
                                       PlywoodCost plywoodCost,
                                       TransportationCost transportationCost)
        {
            InstallationDuration = $"{timeLine.InstallationDuration} Days";
            SmitheryDuration = $"{timeLine.SmitheryDuration} Days";
            WaitingDuration = $"{timeLine.WaitingDuaration} Days";
            RemovalDuration = $"{timeLine.RemovalDuration} Days";
            TotalDuration = $"{timeLine.InstallationDuration + timeLine.SmitheryDuration + timeLine.WaitingDuaration + timeLine.RemovalDuration} Days";

            ManPowerCost = $"{manPowerCost.LaborCost.ToString("#,##0.00")} * {manPowerCost.NoWorkers} * {manPowerCost.Duration} = {manPowerCost.TotalCost.ToString("#,##0.00")} LE";
            EquipmentsCost = $"{equipmentCost.Rent.ToString("#,##0.00")} * {equipmentCost.NoEquipments} * {equipmentCost.Duration} = {equipmentCost.TotalCost.ToString("#,##0.00")} LE";
            FormworkElementsCost = $"{formworkElemntsCost.Cost.ToString("#,##0.00")} * {formworkElemntsCost.Duration} = {formworkElemntsCost.TotalCost.ToString("#,##0.00")} LE";
            PlywoodCost = $"{plywoodCost.CostPerArea.ToString("#,##0.00")} * {Math.Round(plywoodCost.Area, 2)} = {plywoodCost.TotalCost.ToString("#,##0.00")} LE";
            var totalCost = manPowerCost.TotalCost + equipmentCost.TotalCost + formworkElemntsCost.TotalCost+transportationCost.Cost + plywoodCost.TotalCost;
            TotalCost = $"{ totalCost.ToString("#,##0.00")} LE";
            ManPowerInfo = "Labor Cost * No. Workers * (Install Time + Removal Time)";
            EquipmentsInfo = "Rent * No. Cranes * (Install Time + Removal Time)";
            FormworkInfo = "Rent * (Install Time + Smithery Time + Waiting Time + Removal Time)";
            PlywoodInfo = "Cost Per Square Meter * Total Area";

            if(transportationCost.Cost == 0)
            {
                TotalCostInfo = "Manpower + Equipments + Formwork Elements + Plywood";
                IsTransportationIncluded = false;
                TransportationCost = $"{transportationCost.Cost.ToString("#,##0.00")} LE";
            }
            else
            {
                TotalCostInfo = "Manpower + Equipments + Transportation + Formwork Elements + Plywood";
                IsTransportationIncluded = true;
                TransportationCost = $"{transportationCost.Cost.ToString("#,##0.00")} LE";
                TransportationCostInfo = "User defined Transportation Cost";
            }
        }

        #endregion

    }
}
