using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Optimization Cost.
        /// </summary>
        public string FormworkElementsCost { get;}

        public string InitialFormworkElementsCost { get; }

        /// <summary>
        /// Optimization Cost.
        /// </summary>
        public string PlywoodCost { get;  }


        public string InitialPlywoodCost { get; }

        public string TransportationCost { get; }

        /// <summary>
        /// Optimization Cost.
        /// </summary>
        public string TotalCost { get;  }

        public string InitialTotalCost { get; }


        public string ManPowerInfo { get; }

        public string EquipmentsInfo { get; }

        /// <summary>
        /// Optimization formwork info.
        /// </summary>
        public string FormworkInfo { get; }

        /// <summary>
        /// Optimization formwork info.
        /// </summary>
        public string InitialFormworkInfo { get; }

        /// <summary>
        /// Optimization plywood cost info.
        /// </summary>
        public string OptimizePlywoodInfo { get; }

        public string InitialPlywoodInfo { get; }


        /// <summary>
        /// Optimization total cost info.
        /// </summary>
        public string TotalCostInfo { get; }

        public string TransportationCostInfo { get; }

        public bool IsTransportationIncluded { get;  }

        public string Name { get; }



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

            //Optimization cost.
            FormworkElementsCost = $"{formworkElemntsCost.RentCost.ToString("#,##0.00")} * {formworkElemntsCost.Duration} + {formworkElemntsCost.OptimizePurchaseCost.ToString("#,##0.00")} = {formworkElemntsCost.OptimizeTotalCost.ToString("#,##0.00")} LE";
            PlywoodCost = plywoodCost.OptimizePlywoodCostFormula;
            var totalCost = manPowerCost.TotalCost + equipmentCost.TotalCost + formworkElemntsCost.OptimizeTotalCost+transportationCost.Cost + plywoodCost.TotalCost;
            TotalCost = $"{ totalCost.ToString("#,##0.00")} LE";

            //Intial cost.
            InitialFormworkElementsCost = $"{formworkElemntsCost.RentCost.ToString("#,##0.00")} * {formworkElemntsCost.Duration} + {formworkElemntsCost.InitialPurchaseCost.ToString("#,##0.00")} = {formworkElemntsCost.InitialTotalCost.ToString("#,##0.00")} LE";
            InitialPlywoodCost = plywoodCost.InitialPlywoodCostFormula;
            var initialTotalCost = manPowerCost.TotalCost + equipmentCost.TotalCost + formworkElemntsCost.InitialTotalCost + transportationCost.Cost + plywoodCost.InitialTotalCost;

            InitialTotalCost = $"{ initialTotalCost.ToString("#,##0.00")} LE"; ;

            ManPowerInfo = "Labor Cost * No. Workers * (Install Time + Removal Time)";
            EquipmentsInfo = "Rent * No. Cranes * (Install Time + Removal Time)";
            FormworkInfo = "Rent * (Install Time + Smithery Time + Waiting Time + Removal Time) + Purchase / No. Uses";
            InitialFormworkInfo = "Rent * (Install Time + Smithery Time + Waiting Time + Removal Time) + Purchase";
            OptimizePlywoodInfo = plywoodCost.OptimizeInfo;
            InitialPlywoodInfo = plywoodCost.InitialInfo;
            Name = "Cost Result";
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

        #region Methods

        public IEnumerable<GeneticReport> AsReport()
        {
            return new List<GeneticReport>()
            {
                new GeneticReport("Formwork Installation Duration" , InstallationDuration),
                new GeneticReport("Steel Fixing Duration" , SmitheryDuration),
                new GeneticReport("Waiting Duration before Formwork Removal" , WaitingDuration),
                new GeneticReport("Formwork Removal Duration" , RemovalDuration),
                new GeneticReport("Manpower Cost" , ManPowerCost),
                new GeneticReport("Equipments Cost" , EquipmentsCost),
                new GeneticReport("Transportation Cost" , TransportationCost),
                new GeneticReport("Formwork Elements Cost" , FormworkElementsCost),
                new GeneticReport("Plywood Cost" , PlywoodCost),
                new GeneticReport("Total Cost" , TotalCost),
            };
        }

        #endregion


    }
}
