using System.ComponentModel;
namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum EuropeanPropTypeName
    {
        [Description("Prop E30")]
        E30,
        [Description("Prop E35")]
        E35,
        [Description("Prop D40")]
        D40,
        [Description("Prop D45")]
        D45
    }
}
