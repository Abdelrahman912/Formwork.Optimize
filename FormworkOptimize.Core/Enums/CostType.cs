using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CostType
    {
        [Description("Rent")]
        RENT,
        [Description("Purchase")]
        PURCHASE
    }
}
