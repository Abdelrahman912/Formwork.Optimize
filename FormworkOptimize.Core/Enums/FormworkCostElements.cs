using System.ComponentModel;

namespace FormworkOptimize.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FormworkCostElements
    {
        [Description("Betofilm 18mm")]
        BETOFILM_18MM,
        [Description("Cofiform Plus 17.5mm")]
        COFIFORM_PLUS_1705MM,
        [Description("Douglas Fir 19mm")]
        DOUGLAS_FIR_19MM,
        [Description("Douglas Fir 19mm")]
        WISAFORM_BIRCH_18MM,
        [Description("Acrow Beam S12")]
        ACROW_BEAM_S12,
        [Description("Double Acrow Beam S12")]
        DOUBLE_ACROW_BEAM_S12,
        [Description("Aluminum Beam")]
        ALUMINUM_BEAM,
        [Description("Double Aluminum Beam")]
        DOUBLE_ALUMINUM_BEAM,
        [Description("S Beam 12")]
        S_BEAM_12,
        [Description("Timber H20")]
        TIMBER_H20,
        [Description("Timber 2x4")]
        TIMBER_2X4,
        [Description("Timber 2x5")]
        TIMBER_2X5,
        [Description("Timber 2x6")]
        TIMBER_2X6,
        [Description("Timber 2x8")]
        TIMBER_2X8,
        [Description("Timber 3x3")]
        TIMBER_3X3,
        [Description("Timber 3x5")]
        TIMBER_3X5,
        [Description("Timber 3x6")]
        TIMBER_3X6,
        [Description("Timber 4x4")]
        TIMBER_4X4,
        [Description("Double Timber H20")]
        DOUBLE_TIMBER_H20,
        [Description("Double Timber 2x5")]
        DOUBLE_TIMBER_2X5,
        [Description("Double Timber 2x6")]
        DOUBLE_TIMBER_2X6,
        [Description("Double Timber 2x8")]
        DOUBLE_TIMBER_2X8,
        [Description("Double Timber 3x6")]
        DOUBLE_TIMBER_3X6,

        [Description("CupLock Vertical 0.50 m, (Steel 37)")]
        CUPLOCK_VERTICAL_0050M_STEEL37,
        [Description("CupLock Vertical 1.00 m, (Steel 37)")]
        CUPLOCK_VERTICAL_100M_STEEL37,
        [Description("CupLock Vertical 1.50 m, (Steel 37)")]
        CUPLOCK_VERTICAL_1050M_STEEL37,
        [Description("CupLock Vertical 2.00 m, (Steel 37)")]
        CUPLOCK_VERTICAL_200M_STEEL37,
        [Description("CupLock Vertical 2.50 m, (Steel 37)")]
        CUPLOCK_VERTICAL_2050M_STEEL37,
        [Description("CupLock Vertical 3.00 m, (Steel 37)")]
        CUPLOCK_VERTICAL_300M_STEEL37,

        [Description("CupLock Vertical 0.50 m, (Steel 52)")]
        CUPLOCK_VERTICAL_0050M_STEEL52,
        [Description("CupLock Vertical 1.00 m, (Steel 52)")]
        CUPLOCK_VERTICAL_100M_STEEL52,
        [Description("CupLock Vertical 1.50 m, (Steel 52)")]
        CUPLOCK_VERTICAL_1050M_STEEL52,
        [Description("CupLock Vertical 2.00 m, (Steel 52)")]
        CUPLOCK_VERTICAL_200M_STEEL52,
        [Description("CupLock Vertical 2.50 m, (Steel 52)")]
        CUPLOCK_VERTICAL_2050M_STEEL52,
        [Description("CupLock Vertical 3.00 m, (Steel 52)")]
        CUPLOCK_VERTICAL_300M_STEEL52,

        [Description("CupLock Ledger 0.60 m, (Steel 37)")]
        CUPLOCK_LEDGER_0060M_STEEL37,
        [Description("CupLock Ledger 0.90 m, (Steel 37)")]
        CUPLOCK_LEDGER_0090M_STEEL37,
        [Description("CupLock Ledger 1.20 m, (Steel 37)")]
        CUPLOCK_LEDGER_1020M_STEEL37,
        [Description("CupLock Ledger 1.50 m, (Steel 37)")]
        CUPLOCK_LEDGER_1050M_STEEL37,
        [Description("CupLock Ledger 1.80 m, (Steel 37)")]
        CUPLOCK_LEDGER_1080M_STEEL37,
        [Description("CupLock Ledger 2.10 m, (Steel 37)")]
        CUPLOCK_LEDGER_2010M_STEEL37,
        [Description("CupLock Ledger 2.40 m, (Steel 37)")]
        CUPLOCK_LEDGER_2040M_STEEL37,
        [Description("CupLock Ledger 2.70 m, (Steel 37)")]
        CUPLOCK_LEDGER_2070M_STEEL37,
        [Description("CupLock Ledger 3.00 m, (Steel 37)")]
        CUPLOCK_LEDGER_300M_STEEL37,

        [Description("CupLock Ledger 0.60 m, (Steel 52)")]
        CUPLOCK_LEDGER_0060M_STEEL52,
        [Description("CupLock Ledger 0.90 m, (Steel 52)")]
        CUPLOCK_LEDGER_0090M_STEEL52,
        [Description("CupLock Ledger 1.20 m, (Steel 52)")]
        CUPLOCK_LEDGER_1020M_STEEL52,
        [Description("CupLock Ledger 1.50 m, (Steel 52)")]
        CUPLOCK_LEDGER_1050M_STEEL52,
        [Description("CupLock Ledger 1.80 m, (Steel 52)")]
        CUPLOCK_LEDGER_1080M_STEEL52,
        [Description("CupLock Ledger 2.10 m, (Steel 52)")]
        CUPLOCK_LEDGER_2010M_STEEL52,
        [Description("CupLock Ledger 2.40 m, (Steel 52)")]
        CUPLOCK_LEDGER_2040M_STEEL52,
        [Description("CupLock Ledger 2.70 m, (Steel 52)")]
        CUPLOCK_LEDGER_2070M_STEEL52,
        [Description("CupLock Ledger 3.00 m, (Steel 52)")]
        CUPLOCK_LEDGER_300M_STEEL52,

        [Description("U-Head Jack Solid")]
        U_HEAD_JACK_SOLID,
        [Description("Post Head Jack Solid")]
        POST_HEAD_JACK_SOLID,
        [Description("Round Spigot")]
        ROUND_SPIGOT,
        [Description("Spring Clip")]
        SPRING_CLIP,
        [Description("Rivet Pin 16mm, L=9cm")]
        RIVET_PIN_16MM_L9CM,
        [Description("Pressed Prop Swivel Coupler")]
        PRESSED_PROP_SWIVEL_COUPLER,
        [Description("Prop Leg")]
        PROP_LEG,
        [Description("U-Head For Props")]
        U_HEAD_FOR_PROPS,
        [Description("Shore brace connector")]
        SHOREBRACE_CONNECTOR,

        [Description("Scaffolding Tube 1.00 m")]
        SCAFFOLDING_TUBE_100M,
        [Description("Scaffolding Tube 1.50 m")]
        SCAFFOLDING_TUBE_1050M,
        [Description("Scaffolding Tube 2.00 m")]
        SCAFFOLDING_TUBE_200M,
        [Description("Scaffolding Tube 2.50 m")]
        SCAFFOLDING_TUBE_2050M,
        [Description("Scaffolding Tube 3.00 m")]
        SCAFFOLDING_TUBE_300M,
        [Description("Scaffolding Tube 3.50 m")]
        SCAFFOLDING_TUBE_3050M,
        [Description("Scaffolding Tube 4.00 m")]
        SCAFFOLDING_TUBE_400M,
        [Description("Scaffolding Tube 4.50 m")]
        SCAFFOLDING_TUBE_4050M,
        [Description("Scaffolding Tube 5.00 m")]
        SCAFFOLDING_TUBE_500M,
        [Description("Scaffolding Tube 6.00 m")]
        SCAFFOLDING_TUBE_600M,

        [Description("Acrow Prop D40")]
        ACROW_PROP_D40,
        [Description("Acrow Prop D45")]
        ACROW_PROP_D45,
        [Description("Acrow Prop E30")]
        ACROW_PROP_E30,
        [Description("Acrow Prop E35")]
        ACROW_PROP_E35,

        [Description("Shorebrace Frame")]
        SHOREBRACE_FRAME,
        [Description("Shorebrace Telescopic Frame")]
        SHOREBRACE_TELESCOPIC_FRAME,

        [Description("Cross Brace 0.90 m")]
        CROSS_BRACE_0090M,
        [Description("Cross Brace 1.20 m")]
        CROSS_BRACE_1020,
        [Description("Cross Brace 1.50 m")]
        CROSS_BRACE_1050,
        [Description("Cross Brace 1.80 m")]
        CROSS_BRACE_1080,
        [Description("Cross Brace 2.10 m")]
        CROSS_BRACE_2010,
        [Description("Cross Brace 2.40 m")]
        CROSS_BRACE_2040,
        [Description("Cross Brace 2.70 m")]
        CROSS_BRACE_2070,
        [Description("Cross Brace 3.00 m")]
        CROSS_BRACE_300,



    }
}
