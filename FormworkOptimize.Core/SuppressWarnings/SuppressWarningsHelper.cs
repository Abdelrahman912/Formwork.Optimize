using Autodesk.Revit.DB;
using System.Linq;

namespace FormworkOptimize.Core.SuppressWarnings
{
    public static class SuppressWarningsHelper
    {
        public static FailureProcessingResult RemoveDuplicateInstancesWarning( FailuresAccessor failuresAccessor)
        {
            var failures = failuresAccessor.GetFailureMessages();

            failures.ToList().ForEach(f =>
            {
                var id = f.GetFailureDefinitionId();
                if (BuiltInFailures.OverlapFailures.DuplicateInstances == id)
                {
                    failuresAccessor.DeleteWarning(f);
                }
            });
           return FailureProcessingResult.Continue;
        }

        public static FailureProcessingResult RemoveFloorsOverlapWarning(FailuresAccessor failuresAccessor)
        {
            var failures = failuresAccessor.GetFailureMessages();

            failures.ToList().ForEach(f =>
            {
                var id = f.GetFailureDefinitionId();
                if (BuiltInFailures.OverlapFailures.FloorsOverlap == id)
                {
                    failuresAccessor.DeleteWarning(f);
                }
            });
            return FailureProcessingResult.Continue;
        }

        public static GenericSuppressWarning DuplicateInstanceSuppress =>
            new GenericSuppressWarning(RemoveDuplicateInstancesWarning);

        public static GenericSuppressWarning FloorsOverlapSuppress =>
           new GenericSuppressWarning(RemoveFloorsOverlapWarning);

    }
}
