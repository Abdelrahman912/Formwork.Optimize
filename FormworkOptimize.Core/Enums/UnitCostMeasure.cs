using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum UnitCostMeasure
    {
        [Description("Number")]
        NUMBER,
        [Description("Length")]
        LENGTH,
        [Description("Area")]
        AREA,
        [Description("Volume")]
        VOLUME
    }
}
