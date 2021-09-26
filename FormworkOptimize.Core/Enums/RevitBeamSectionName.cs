using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum RevitBeamSectionName
    {
        [Description("Acrow Beam S12")]
        ACROW_BEAM_S12 = 0,
        [Description("Double Acrow Beam S12")]
        DOUBLE_ACROW_BEAM_S12 = 1,
        [Description("Aluminum Beam")]
        ALUMINUM_BEAM = 2,
        [Description("Double Aluminum Beam")]
        ALUMINUM_BEAM_DOUBLE = 3,
        [Description("S Beam 12")]
        S_BEAM_12 = 4,
        [Description("Timber H20")]
        TIMBER_H20 = 5,
        [Description("Timber 2x4")]
        TIMBER_2X4 = 6,
        [Description("Timber 2x5")]
        TIMBER_2X5 = 7,
        [Description("Timber 2x6")]
        TIMBER_2X6 = 8,
        [Description("Timber 2x8")]
        TIMBER_2X8 = 9,
        [Description("Timber 3x3")]
        TIMBER_3X3 = 10,
        [Description("Timber 3x5")]
        TIMBER_3X5 = 11,
        [Description("Timber 3x6")]
        TIMBER_3X6 = 12,
        [Description("Timber 4x4")]
        TIMBER_4X4 = 13,
        [Description("Double Timber H20")]
        DOUBLE_TIMBER_H20 = 14,
        [Description("Double Timber 2x5")]
        DOUBLE_TIMBER_2X5 = 15,
        [Description("Double Timber Timber 2x6")]
        DOUBLE_TIMBER_2X6 = 16,
        [Description("Double timber 3x6")]
        DOUBLE_TIMBER_3X6 = 17,
        [Description("Double Timber 2x8")]
        DOUBLE_TIMBER_2X8 = 18,
        //[Description("S Beam 16")] //Not Exist in the Designer
        //S_BEAM_16 = 19
        //[Description("Soldier 10")]
        //SOLDIER_10,
        //[Description("Soldier 12")]
        //SOLDIER_12,
        //[Description("Soldier 14")]
        //SOLDIER_14,
        //[Description("Soldier 16")]
        //SOLDIER_16,
        //[Description("Double Soldier 10")]
        //DOUBLE_SOLDIER_10
    }
}
