using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public class ShortBeamError : Error
    {

        override public string Message =>
            "Beam Length is short to fit between vertical spacing.";

    }
}
