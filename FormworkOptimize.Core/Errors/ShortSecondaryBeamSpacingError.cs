using CSharp.Functional.Errors;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.Core.Errors
{
    public class ShortSecondaryBeamSpacingError:Error
    {

        public override string Message =>
            $"Selected Secondary Beam Spacing Violates the Minimum Spacing = {MIN_SEC_BEAM_SPACING} cm";

    }
}
