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

        public double CostPerArea { get;  }

        /// <summary>
        /// Total Area in meter Square.
        /// </summary>
        public double Area { get; }

        public double TotalCost { get; }

        #endregion

        #region Constructors

        public PlywoodCost(double costPerArea ,
                           double area,
                           Func<Document,Validation<Unit>> drawFunc)
        {
            _drawFunc = drawFunc;
            Area = area;
            CostPerArea = costPerArea;
            TotalCost = CostPerArea * Area;
        }

        #endregion

        #region Methods

        public Validation<Unit> Draw(Document doc) =>
            _drawFunc(doc);
       

        #endregion

    }
}
