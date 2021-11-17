using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CostType
    {
        [Description("Rent (Monthly)")]
        RENT,
        [Description("Purchase")]
        PURCHASE
    }
}
