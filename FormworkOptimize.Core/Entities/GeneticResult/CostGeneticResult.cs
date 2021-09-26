using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.Cost.Interfaces;
using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticResult
{
    public class CostGeneticResult:NoCostGeneticResult
    {
        #region Properties

        public double Cost { get; }

        public IEvaluateCost SystemModel { get;  }


        public PlywoodCost Plywood { get;  }

        #endregion

        #region Constructors

        public CostGeneticResult(int rank, 
                                double fitness, 
                                string name, 
                                List<IGeneticDetailResult> detailResults,
                                double cost, 
                                IEvaluateCost systemModel,
                                PlywoodCost plywood)
            :base(rank, fitness, name, detailResults)
        {
            Cost =Math.Round(cost,2);
            SystemModel = systemModel;
            Plywood = plywood;
        }

        #endregion

    }
}
