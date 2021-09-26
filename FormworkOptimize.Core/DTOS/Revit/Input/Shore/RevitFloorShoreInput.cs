using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Shore
{
    public class RevitFloorShoreInput : RevitShoreInput
    {
        #region Properties

        public double BoundaryLinesOffset { get; }

        public double BeamOffset { get; }

        public double SpacingMain { get; }

        public double SpacingSecondary { get; }

        public double SecondaryBeamTotalLength { get; }

        public double MainBeamTotalLength { get; }

        #endregion

        #region Constructors

        public RevitFloorShoreInput(PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection,
                                      RevitBeamSectionName mainBeamSection, double secondaryBeamTotalLength,
                                      double secondaryBeamSpacing, double mainBeamTotalLength, double spacingMain,
                                      double spacingSecondary, double boundaryLinesOffset, double beamOffset) :
            base(plywoodSection, secondaryBeamSection, mainBeamSection, secondaryBeamSpacing)
        {
            SpacingMain = spacingMain;
            SpacingSecondary = spacingSecondary;
            BoundaryLinesOffset = boundaryLinesOffset;
            MainBeamTotalLength = mainBeamTotalLength;
            SecondaryBeamTotalLength = secondaryBeamTotalLength;
            BeamOffset = beamOffset;
        }

        #endregion

    }
}
