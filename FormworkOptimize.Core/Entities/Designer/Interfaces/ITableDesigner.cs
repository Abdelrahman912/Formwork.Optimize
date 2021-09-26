using FormworkOptimize.Core.DTOS;
using System;

namespace FormworkOptimize.Core.Entities
{
    public interface ITableDesigner
    {
        TableDesignOutput Design(TableDesignInput input,
                           Func<Beam, double, double, double, StrainingActions> beamSolver,
                           Func<Beam, double, double, double, double> beamReactionFunc);
    }
}
