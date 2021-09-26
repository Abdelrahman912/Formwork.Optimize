using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PlywoodSectionName
    {
        [Description("Betofilm 18mm")]
        BETOFILM_18MM,
        [Description("Cofiform Plus 17.5mm")]
        COFIFORM_PLUS_1705MM,
        [Description("Douglas Fir 19mm")]
        DOUGLAS_FIR_19MM,
        [Description("Wisaform Birch 18mm")]
        WISAFORM_BIRCH_18MM
    }
}
