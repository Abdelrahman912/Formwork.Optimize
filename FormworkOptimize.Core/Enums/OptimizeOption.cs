using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum OptimizeOption
    {
        [Description("Formwork Design")]
        DESIGN=0,
        [Description("Formwork Cost")]
        COST=1
    }
}
