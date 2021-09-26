using Autodesk.Revit.DB;

namespace FormworkOptimize.Core.Extensions
{
    public static class ViewExtension
    {
        public static Options Options(this View view)
        {
            var options = new Options();
            options.ComputeReferences = true;
            options.View = view;
            return options;
        }
    }
}
