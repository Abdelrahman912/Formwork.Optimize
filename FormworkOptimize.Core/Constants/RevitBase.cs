using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Constants
{
    /// <summary>
    /// Revit Base represents the revit model.
    /// </summary>
    public static class RevitBase
    {

        public const string BETOFILM_18MM_FLOOR_TYPE = "Betofilm 18mm";

        public const string COFIFORM_PLUS_1705MM_FLOOR_TYPE = "Cofiform Plus 17.5mm";

        public const string DOUGLAS_FIR_19MM_FLOOR_TYPE = "Douglas Fir 19mm";

        public const string WISAFORM_BIRCH_18MM_FLOOR_TYPE = "Wisaform Birch 18mm";

        public const string PLYWOOD_MATERIAL = "Plywood, Sheathing";

        public const string CUPLOCK_VERTICAL = "Cup-Lock_Vertical";

        public const string CUPLOCK_LEDGER = "Cup-Lock_Ledgers";

        public const string CUPLOCK_BRACING = "Cup-Lock_Bracing";

        public const string POST_HEAD = "Post-Head";

        public const string U_HEAD = "U-Head Jack Solid-34mm";

        public const string PROP_D40 = "Acrow_Prop_D40";

        public const string PROP_D45 = "Acrow_Prop_D45";

        public const string PROP_E30 = "Acrow_Prop_E30";

        public const string PROP_E35 = "Acrow_Prop_E35";

        public const string PROP_LEG = "Prop_Leg";

        public const string PROPS_U_HEAD = "U-Head for props";

        public const string CROSS_BRACE = "Cross_Brace";

        public const string CROSS_BRACE_UNIT = "Cross_Brace_Unit";

        public const string HORIZONTAL_TUBE = "Horizontal_Tube";

        public const string SHORE_BRACE_FRAME = "Shore_Brace_Frame";

        public const string SHORE_BRACE_FRAME_CONNECTOR = "Shorebrace_Frame_connector";

        public const string TELESCOPIC_FRAME = "Telescopic_Frame";

        public const string ACROW_BEAM_S12_FAMILY = "Acrow Beam_S12";

        public const string SINGLE_ACROW_BEAM_S12_SYMBOL = "Single_Acrow";

        public const string DOUBLE_ACROW_BEAM_S12_SYMBOL = "Double_Acrow";

        public const string ALUMINUM_BEAM_FAMILY = "Aluminum_Beam";

        public const string SINGLE_ALUMINUM_BEAM_SYMBOL = "Single_Timber_Aluminum";

        public const string Double_ALUMINUM_BEAM_SYMBOL = "Double_Timber_Aluminum";

        public const string S_BEAM_12CM_FAMILY = "S_Beam_12cm";

        public const string SINGLE_S_BEAM_12CM_SYMBOL = "S_Beam_12cm_Single";

        public const string DOUBLE_S_BEAM_12CM_SYMBOL = "S_Beam_12cm_Double";

        public const string S_BEAM_16CM_FAMILY = "S_Beam_16cm";

        public const string SINGLE_S_BEAM_16CM_SYMBOL = "S_Beam_16cm_Single";

        public const string DOUBLE_S_BEAM_16CM_SYMBOL = "S_Beam_16cm_Double";

        public const string TIMBER_FAMILY = "Timber";

        public const string SINGLE_TIMBER_SYMBOL = "Single_Timber";

        public const string DOUBLE_TIMBER_SYMBOL = "Double_Timber";

        public const string SINGLE_TIMBER_H20_SYMBOL = "Single_Timber_H20";

        public const string DOUBLE_TIMBER_H20_SYMBOL = "Double_Timber_H20";

        public const string TIMBER_H20_FAMILY = "Timber_H20";

        public static List<string> PLYWOOD_FLOOR_TYPES = new List<string>() { BETOFILM_18MM_FLOOR_TYPE, COFIFORM_PLUS_1705MM_FLOOR_TYPE, DOUGLAS_FIR_19MM_FLOOR_TYPE, WISAFORM_BIRCH_18MM_FLOOR_TYPE };

        public static List<string> CUPLOCK_FAMILIES = new List<string>() { CUPLOCK_VERTICAL, CUPLOCK_LEDGER, CUPLOCK_BRACING, POST_HEAD, U_HEAD };

        public static List<string> PROP_FAMILIES = new List<string>() { PROP_D40, PROP_D45, PROP_E30, PROP_E35, PROP_LEG, PROPS_U_HEAD };

        public static List<string> SHORE_BRACE_FAMILIES = new List<string>() { CROSS_BRACE, CROSS_BRACE_UNIT, HORIZONTAL_TUBE, SHORE_BRACE_FRAME, SHORE_BRACE_FRAME_CONNECTOR, TELESCOPIC_FRAME, POST_HEAD, U_HEAD };

        public static List<string> DECKING_FAMILIES = new List<string>() { TIMBER_H20_FAMILY, ACROW_BEAM_S12_FAMILY, ALUMINUM_BEAM_FAMILY, S_BEAM_12CM_FAMILY, S_BEAM_16CM_FAMILY, TIMBER_FAMILY };

        public static List<string> DECKING_SYMBOLS = new List<string>()
        {
            SINGLE_ACROW_BEAM_S12_SYMBOL,
            DOUBLE_ACROW_BEAM_S12_SYMBOL,
            SINGLE_ALUMINUM_BEAM_SYMBOL,
            Double_ALUMINUM_BEAM_SYMBOL,
            SINGLE_S_BEAM_12CM_SYMBOL,
            DOUBLE_S_BEAM_12CM_SYMBOL,
            SINGLE_S_BEAM_16CM_SYMBOL,
            DOUBLE_S_BEAM_16CM_SYMBOL,
            SINGLE_TIMBER_SYMBOL,
            DOUBLE_TIMBER_SYMBOL,
            SINGLE_TIMBER_H20_SYMBOL,
            DOUBLE_TIMBER_H20_SYMBOL,
        };

        /// <summary>
        /// General use tolerance.
        /// </summary>
        public const double TOLERANCE = 0.0002;

        public const long FORMWORK_NUMBER = 10_000_000;

        public const double PROPS_ULOCK_DISTANCE = 7.9;

        public const double MIN_ULOCK_DISTANCE = 15;

        public const double MAX_ULOCK_DISTANCE = 55;

        public const double MIN_CANTILEVER_LENGTH = 25;

        public const string DEFAULT_3D_VIEW = "{3D}";

        public const double BEAM_LINE_OFFSET_FROM_COLUMN = 10;

        public const double MIN_PROPS_SPACING = 60;

        /// <summary>
        /// Interval Used To Get Min spacing than the input spacing.
        /// </summary>
        public const double PROPS_INTERVAL = 10;

        /// <summary>
        /// Revit active ui document.
        /// </summary>
        public static UIDocument UIDocument { get; set; }

        /// <summary>
        /// Revit document.
        /// </summary>
        public static Document Document { get; set; }

        /// <summary>
        /// Revit result enum.
        /// </summary>
        public static Result Result { get; set; }

        /// <summary>
        /// Revit error message.
        /// </summary>
        public static string Message { get; set; }
    }
}
