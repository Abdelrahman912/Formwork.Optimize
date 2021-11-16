using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
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

        #endregion

        #region Constructors

        public PlywoodCost(double costPerArea ,
                           double initialCostPerArea,
                           double area,
                           Func<Document,Validation<Unit>> drawFunc)
        {
            _drawFunc = drawFunc;
            Area = area;
            CostPerArea = costPerArea;
            InitialCostPerArea = initialCostPerArea;
            TotalCost = CostPerArea * Area;
            InitialCostPerArea = InitialCostPerArea * Area;
        }

        #endregion

        #region Methods

        public Validation<Unit> Draw(Document doc) =>
            _drawFunc(doc);
       

        #endregion

    }
}
