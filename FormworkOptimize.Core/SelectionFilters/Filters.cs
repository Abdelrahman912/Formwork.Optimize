using Autodesk.Revit.DB;
using FormworkOptimize.Core.Extensions;

namespace FormworkOptimize.Core.SelectionFilters
{
    public static class Filters
    {
        public static GenericSelectionFilter FloorAndBeamFilter =>
            new GenericSelectionFilter(ele => ele.GetCategory() == BuiltInCategory.OST_Floors || ele.GetCategory() == BuiltInCategory.OST_StructuralFraming);

        public static GenericSelectionFilter DetailLineFilter =>
            new GenericSelectionFilter(ele => ele is DetailLine);

        public static GenericSelectionFilter BeamFilter =>
            new GenericSelectionFilter(ele => (BuiltInCategory)ele.Category.Id.IntegerValue == BuiltInCategory.OST_StructuralFraming);

        public static GenericSelectionFilter ColumnFilter =>
           new GenericSelectionFilter(ele => (BuiltInCategory)ele.Category.Id.IntegerValue == BuiltInCategory.OST_StructuralColumns);
    }
}
