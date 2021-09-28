using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticResult.Interfaces
{
    public interface IGeneticDetailResult
    {
        string Name { get;  }

        IEnumerable<GeneticReport> AsReport();
    }
}
