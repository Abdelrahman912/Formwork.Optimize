using FormworkOptimize.Core.Enums;
using System;
using System.ComponentModel;
using System.Reflection;

namespace FormworkOptimize.Core.Extensions
{
    public static class EnumExtension
    {

        public static FormworkCostElements AsElementCost(this PlywoodSectionName plywood)
        {
            switch (plywood)
            {
                case PlywoodSectionName.BETOFILM_18MM:
                    return FormworkCostElements.BETOFILM_18MM;
                case PlywoodSectionName.COFIFORM_PLUS_1705MM:
                   return FormworkCostElements.COFIFORM_PLUS_1705MM;
                case PlywoodSectionName.DOUGLAS_FIR_19MM:
                    return FormworkCostElements.DOUGLAS_FIR_19MM;
                case PlywoodSectionName.WISAFORM_BIRCH_18MM:
                    return FormworkCostElements.WISAFORM_BIRCH_18MM;
                default:
                    throw new ArgumentOutOfRangeException(nameof(plywood));
            }
        }

        public static FormworkCostElements AsElementCost(this EuropeanPropTypeName propType)
        {
            switch (propType)
            {
                case EuropeanPropTypeName.E30:
                    return FormworkCostElements.ACROW_PROP_E30;
                case EuropeanPropTypeName.E35:
                    return FormworkCostElements.ACROW_PROP_E35;
                case EuropeanPropTypeName.D40:
                    return FormworkCostElements.ACROW_PROP_D40;
                case EuropeanPropTypeName.D45:
                    return FormworkCostElements.ACROW_PROP_D45;
                default:
                    throw new ArgumentOutOfRangeException(nameof(propType));
            }
        }

        public static FormworkCostElements AsElementCost(this RevitBeamSectionName beamSection)
        {
            switch (beamSection)
            {
                case RevitBeamSectionName.ACROW_BEAM_S12:
                    return FormworkCostElements.ACROW_BEAM_S12;
                case RevitBeamSectionName.DOUBLE_ACROW_BEAM_S12:
                    return FormworkCostElements.DOUBLE_ACROW_BEAM_S12;
                case RevitBeamSectionName.ALUMINUM_BEAM:
                    return FormworkCostElements.ALUMINUM_BEAM;
                case RevitBeamSectionName.ALUMINUM_BEAM_DOUBLE:
                    return FormworkCostElements.DOUBLE_ALUMINUM_BEAM;
                case RevitBeamSectionName.S_BEAM_12:
                    return FormworkCostElements.S_BEAM_12;
                case RevitBeamSectionName.TIMBER_H20:
                    return FormworkCostElements.TIMBER_H20;
                case RevitBeamSectionName.TIMBER_2X4:
                    return FormworkCostElements.TIMBER_2X4; 
                case RevitBeamSectionName.TIMBER_2X5:
                    return FormworkCostElements.TIMBER_2X5;
                case RevitBeamSectionName.TIMBER_2X6:
                    return FormworkCostElements.TIMBER_2X6;
                case RevitBeamSectionName.TIMBER_2X8:
                    return FormworkCostElements.TIMBER_2X8;
                case RevitBeamSectionName.TIMBER_3X3:
                    return FormworkCostElements.TIMBER_3X3;
                case RevitBeamSectionName.TIMBER_3X5:
                    return FormworkCostElements.TIMBER_3X5;
                case RevitBeamSectionName.TIMBER_3X6:
                    return FormworkCostElements.TIMBER_3X6;
                case RevitBeamSectionName.TIMBER_4X4:
                    return FormworkCostElements.TIMBER_4X4;
                case RevitBeamSectionName.DOUBLE_TIMBER_H20:
                    return FormworkCostElements.DOUBLE_TIMBER_H20;
                case RevitBeamSectionName.DOUBLE_TIMBER_2X5:
                    return FormworkCostElements.DOUBLE_TIMBER_2X5;
                case RevitBeamSectionName.DOUBLE_TIMBER_2X6:
                    return FormworkCostElements.DOUBLE_TIMBER_2X6;
                case RevitBeamSectionName.DOUBLE_TIMBER_3X6:
                    return FormworkCostElements.DOUBLE_TIMBER_3X6;
                case RevitBeamSectionName.DOUBLE_TIMBER_2X8:
                    return FormworkCostElements.DOUBLE_TIMBER_2X8;
                default:
                    throw new ArgumentOutOfRangeException(nameof(beamSection));
            }
        }

        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return string.Empty;
        }
    }
}
