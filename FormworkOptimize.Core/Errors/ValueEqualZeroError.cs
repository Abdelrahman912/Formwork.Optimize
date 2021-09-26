using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public class ValueEqualZeroError:Error
    {
        override public string Message {  get;  }

        public ValueEqualZeroError(string valueName)
        {
            Message = $"{valueName} cannot be equal to zero or less than zero, It must be greater than zero.";
        }

    }
}
