using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public class ShortPropTypeError : Error
    {
        public override string Message =>
            "This Prop Type is short to be used";
    }
}
