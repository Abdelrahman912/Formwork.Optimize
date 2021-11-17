using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using FormworkOptimize.Core.Enums;
using System;
using Unit = System.ValueTuple;

namespace FormworkOptimize.Core.Entities.Cost
{
    public class PlywoodCost
    {

        #region Private Fields

        private readonly Func<Document, Validation<Unit>> _drawFunc;

        #endregion

        #region Properties

        public double InitialCostPerArea { get; }

        /// <summary>
        /// Optimization Cost.
        /// </summary>
        public double CostPerArea { get;  }

        /// <summary>
        /// Total Area in meter Square.
        /// </summary>
        public double Area { get; }

        /// <summary>
        /// Optimization total cost.
        /// </summary>
        public double TotalCost { get; }

        public double InitialTotalCost { get;}

        public CostType CostType { get;}

        public int Duration { get;  }

        public string OptimizePlywoodCostFormula { get; }
        public string InitialPlywoodCostFormula { get; set; }

        public string OptimizeInfo { get; }

        public string InitialInfo { get; }

        #endregion

        #region Constructors

        public PlywoodCost(double costPerArea ,
                           double initialCostPerArea,
                           double area,
                           CostType costType,
                           Func<Document,Validation<Unit>> drawFunc,
                           int duration)
        {
            _drawFunc = drawFunc;
            Area = area;
            CostPerArea = costPerArea;
            InitialCostPerArea = initialCostPerArea;
            CostType = costType;
            Duration = duration;
            switch (CostType)
            {
                case CostType.RENT:
                    TotalCost = CostPerArea*Duration * Area;
                    InitialCostPerArea = InitialCostPerArea* Duration * Area;
                    OptimizePlywoodCostFormula = $"{CostPerArea.ToString("#,##0.00")} * {Math.Round(Area, 2)} * {duration} = {TotalCost.ToString("#,##0.00")} LE";
                    InitialPlywoodCostFormula = $"{InitialCostPerArea.ToString("#,##0.00")} * {Math.Round(Area, 2)} * {duration} = {InitialTotalCost.ToString("#,##0.00")} LE";
                    OptimizeInfo = "Rent(Cost Per Square Meter) * Total Area * Total Duration";
                    InitialInfo = "Rent(Cost Per Square Meter) * Total Area * Total Duration";

                    break;
                case CostType.PURCHASE:
                    TotalCost = CostPerArea * Area;
                    InitialTotalCost = InitialCostPerArea * Area;
                    OptimizePlywoodCostFormula = $"{CostPerArea.ToString("#,##0.00")} * {Math.Round(Area, 2)}  = {TotalCost.ToString("#,##0.00")} LE";
                    InitialPlywoodCostFormula = $"{InitialCostPerArea.ToString("#,##0.00")} * {Math.Round(Area, 2)}  = {InitialTotalCost.ToString("#,##0.00")} LE";
                    OptimizeInfo = "(Cost Per Square Meter / No. Uses) * Total Area";
                    InitialInfo = "Cost Per Square Meter * Total Area";

                    break;
            }
        }

        #endregion

        #region Methods

        public Validation<Unit> Draw(Document doc) =>
            _drawFunc(doc);
       

        #endregion

    }
}
