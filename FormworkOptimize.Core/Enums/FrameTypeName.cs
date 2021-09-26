using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FrameTypeName
    {
        [Description("H 2.9mm")]
        H209MM,
        [Description("M 2.5mm")]
        M205MM,
        [Description("L 2.0mm")]
        L200MM
    }
}
