using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FormworkSystem
    {
        [Description("Cuplock System")]
        CUPLOCK_SYSTEM,
        [Description("European Props System")]
        EUROPEAN_PROPS_SYSTEM,
        [Description("Shorebrace System")]
        SHORE_SYSTEM,
        [Description("Frame System")]
        FRAME_SYSTEM,
        [Description("Aluminum Props System")]
        ALUMINUM_PROPS_SYSTEM

    }
}
