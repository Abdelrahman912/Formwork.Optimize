using FormworkOptimize.Core.DTOS.Internal;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.DTOS.Revit.Input
{
    public class RevitBeamCuplockInput:RevitCuplockInput
    {

        #region Properties

        public double LedgersMainDir { get; }

       
        public double LedgersSecondaryDir { get; }

        public double SecondaryBeamTotalLength { get; }

        public double MainBeamTotalLength { get; }

        #endregion

        #region Constructors

        public RevitBeamCuplockInput(PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection,
                                      RevitBeamSectionName mainBeamSection,SteelType steelType,
                                       double secondaryBeamTotalLength,
                                       double mainBeamTotalLength,
                                      double secondaryBeamSpacing, double ledgersMainDir,
                                      double ledgersSecondaryDir):
            base(plywoodSection,secondaryBeamSection,mainBeamSection,steelType ,secondaryBeamSpacing)
        {
            LedgersMainDir = ledgersMainDir;
            LedgersSecondaryDir = ledgersSecondaryDir;
            SecondaryBeamTotalLength = secondaryBeamTotalLength;
            MainBeamTotalLength = mainBeamTotalLength;
        }

        #endregion

    }
}
