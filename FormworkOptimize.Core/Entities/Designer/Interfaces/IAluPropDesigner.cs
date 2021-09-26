using CSharp.Functional.Constructs;
using FormworkOptimize.Core.DTOS;
using System;

namespace FormworkOptimize.Core.Entities
{
    public interface IAluPropDesigner
    {
        Validation<PropDesignOutput> Design(AluPropDesignInput input,
                          Func<Beam, double, double, double, StrainingActions> beamSolver,
                          Func<Beam, double, double, double, double> beamReactionFunc);
    }
}
