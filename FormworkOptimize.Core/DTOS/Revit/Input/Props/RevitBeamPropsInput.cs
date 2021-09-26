using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Props
{
    public class RevitBeamPropsInput : RevitPropsInput
    {

        #region Properties

        public double SpacingMain { get; }

        public double SpacingSecondary { get; }

        public double SecondaryBeamTotalLength { get; }

        public double MainBeamTotalLength { get; }

        #endregion

        #region Constructors

        public RevitBeamPropsInput(EuropeanPropTypeName propType, PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection,
                                      RevitBeamSectionName mainBeamSection,
                                      double secondaryBeamTotalLength,
                                      double mainBeamTotalLength,
                                      double secondaryBeamSpacing, double spacingMain,
                                      double spacingSecondary) :
            base(propType, plywoodSection, secondaryBeamSection, mainBeamSection, secondaryBeamSpacing)
        {
            SpacingMain = spacingMain;
            SpacingSecondary = spacingSecondary;
            SecondaryBeamTotalLength = secondaryBeamTotalLength;
            MainBeamTotalLength = mainBeamTotalLength;
        }

        #endregion

    }
}
