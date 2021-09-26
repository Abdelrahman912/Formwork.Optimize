using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum SteelType
    {
        [Description("Steel 52")]
        STEEL_52,
        [Description("Steel 37")]
        STEEL_37
    }
}
