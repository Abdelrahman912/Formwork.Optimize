using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public class LessThanTwoPlywoodError:Error
    {
        public override string Message =>
            "There must be more than one Plywood Selected to be included in Genetic Algorithm";

    }
}
