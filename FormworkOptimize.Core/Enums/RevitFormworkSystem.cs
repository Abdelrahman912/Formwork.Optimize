using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum RevitFormworkSystem
    {
        [Description("Cuplock System")]
        CUPLOCK_SYSTEM,
        [Description("European Props System")]
        PROPS_SYSTEM,
        [Description("Shorebrace System")]
        SHORE_SYSTEM
    }
}
