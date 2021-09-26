using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public  class InvalidOpeningError:Error
    {

        public override string Message =>
            "Opening is not inside the Detail lines (floor)";

    }
}
