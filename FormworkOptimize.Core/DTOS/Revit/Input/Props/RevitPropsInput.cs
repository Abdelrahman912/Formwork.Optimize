using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Props
{
    public class RevitPropsInput
    {
        #region Properties

        public PlywoodSectionName PlywoodSection { get; }

        public RevitBeamSectionName SecondaryBeamSection { get; }

        public RevitBeamSectionName MainBeamSection { get; }

        public double SecondaryBeamSpacing { get; }

        public EuropeanPropTypeName PropType { get; }

        #endregion

        #region Constructors

        public RevitPropsInput(EuropeanPropTypeName propType, PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection,
                                 RevitBeamSectionName mainBeamSection,
                                 double secondaryBeamSpacing)
        {
            PropType = propType;
            PlywoodSection = plywoodSection;
            SecondaryBeamSection = secondaryBeamSection;
            MainBeamSection = mainBeamSection;
            SecondaryBeamSpacing = secondaryBeamSpacing;
        }

        #endregion
    }
}
