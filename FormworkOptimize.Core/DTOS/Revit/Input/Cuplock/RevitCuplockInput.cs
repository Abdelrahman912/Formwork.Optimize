using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.DTOS.Internal
{
    public class RevitCuplockInput
    {
        #region Properties

        public PlywoodSectionName PlywoodSection { get; }

        public RevitBeamSectionName SecondaryBeamSection { get; }

        public RevitBeamSectionName MainBeamSection { get; }

        public SteelType SteelType { get; }

        public double SecondaryBeamSpacing { get;  }

        #endregion

        #region Constructors

        public RevitCuplockInput(PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection, 
                                 RevitBeamSectionName mainBeamSection,SteelType steelType,
                                 double secondaryBeamSpacing)
        {
            PlywoodSection = plywoodSection;
            SecondaryBeamSection = secondaryBeamSection;
            MainBeamSection = mainBeamSection;
            SecondaryBeamSpacing = secondaryBeamSpacing;
            SteelType = steelType;
        }

        #endregion

    }
}
