using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public class ValueLessThanZeroError:Error
    {
        override public string Message {  get;  }
        public ValueLessThanZeroError(string valueName)
        {
            Message = $"{valueName} cannot be less than zero, It must be greater than or equal zero.";
        }

    }
}
