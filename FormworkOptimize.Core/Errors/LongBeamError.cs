using CSharp.Functional.Errors;
using static FormworkOptimize.Core.Constants.RevitBase;

namespace FormworkOptimize.Core.Errors
{
    public class LongBeamError:Error
    {
        public override string Message { get; }

        public LongBeamError(double totalLengthCm , double spanCm )
        {
            Message = $"Beam with total length: {totalLengthCm} cm and span: {spanCm} cm will violates maximum cantilever length = {MAX_CANTILEVER_LENGTH} cm " ;
        }

    }
}
