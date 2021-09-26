using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Shore
{
    public class RevitBeamShoreInput : RevitShoreInput
    {

        #region Properties

        public double SpacingMain { get; }


        public double SpacingSecondary { get; }

        public double SecondaryBeamTotalLength { get; }

        public double MainBeamTotalLength { get; }

        #endregion

        #region Constructors

        public RevitBeamShoreInput(PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection,
                                      RevitBeamSectionName mainBeamSection,
                                      double secondaryBeamTotalLength,
                                      double mainBeamTotalLength,
                                      double secondaryBeamSpacing, double spacingMain,
                                      double spacingSecondary) :
            base(plywoodSection, secondaryBeamSection, mainBeamSection, secondaryBeamSpacing)
        {
            SpacingMain = spacingMain;
            SpacingSecondary = spacingSecondary;
            SecondaryBeamTotalLength = secondaryBeamTotalLength;
            MainBeamTotalLength = mainBeamTotalLength;
        }

        #endregion

    }
}
