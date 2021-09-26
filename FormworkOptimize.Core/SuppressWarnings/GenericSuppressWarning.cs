using Autodesk.Revit.DB;
using System;

namespace FormworkOptimize.Core.SuppressWarnings
{
    public class GenericSuppressWarning : IFailuresPreprocessor
    {
        private readonly Func< FailuresAccessor, FailureProcessingResult> _executeFunc;

        public GenericSuppressWarning(Func<FailuresAccessor, FailureProcessingResult> executeFunc)
        {
            _executeFunc = executeFunc;
        }

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)=>
                _executeFunc( failuresAccessor);
       
    }
}
