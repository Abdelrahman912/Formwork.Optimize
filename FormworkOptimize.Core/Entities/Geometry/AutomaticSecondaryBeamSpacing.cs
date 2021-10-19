using CSharp.Functional.Constructs;

namespace FormworkOptimize.Core.Entities.Geometry
{
    public class AutomaticSecondaryBeamSpacing:SecondaryBeamSpacing
    {

        public override Validation<Plywood> AsNewPlywood(Plywood old)
        {
            return new Plywood(old.Section,old.Span);
        }

    }
}
