using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticResult
{
    public  class NoCostGeneticResult
    {
      
        #region Properties

        public int Rank { get; }

        public double Fitness { get; }

        public string Name { get; }

        public List<IGeneticDetailResult> DetailResults { get; }

        #endregion

        #region Constructors

        public NoCostGeneticResult(int rank, double fitness, string name, List<IGeneticDetailResult> detailResults)
        {
            Rank = rank;
            Fitness =Math.Round(fitness,3);
            Name = name;
            DetailResults = detailResults;
        }

        #endregion

       

    }
}
