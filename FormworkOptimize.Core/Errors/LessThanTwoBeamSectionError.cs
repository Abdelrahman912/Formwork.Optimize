using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public class LessThanTwoBeamSectionError:Error
    {

        override public string Message =>
            "There must be more than one Beam Section Selected to be included in Genetic Algorithm";

    }
}
