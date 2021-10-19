using CSharp.Functional.Constructs;

namespace FormworkOptimize.Core.Entities.Geometry
{
    public abstract class SecondaryBeamSpacing
    {

        abstract public Validation<Plywood> AsNewPlywood(Plywood old);

    }
}
