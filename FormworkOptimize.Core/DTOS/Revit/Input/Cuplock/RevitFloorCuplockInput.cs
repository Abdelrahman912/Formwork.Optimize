using FormworkOptimize.Core.DTOS.Internal;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.DTOS.Revit.Input
{
    public class RevitFloorCuplockInput:RevitCuplockInput
    {

        #region Properties

        public double BoundaryLinesOffset { get; }

        public double BeamOffset { get; }

        public double LedgersMainDir { get; }

        public double LedgersSecondaryDir { get; }

        public double SecondaryBeamTotalLength { get; }

        public double MainBeamTotalLength { get; }

        #endregion

        #region Constructors

        public RevitFloorCuplockInput(PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection,
                                      RevitBeamSectionName mainBeamSection,SteelType steelType, double secondaryBeamTotalLength,
                                      double secondaryBeamSpacing, double mainBeamTotalLength, double ledgersMainDir,
                                      double ledgersSecondaryDir ,double boundaryLinesOffset , double beamOffset):
            base(plywoodSection,secondaryBeamSection,mainBeamSection,steelType,secondaryBeamSpacing)
        {
            LedgersMainDir = ledgersMainDir;
            LedgersSecondaryDir = ledgersSecondaryDir;
            BoundaryLinesOffset = boundaryLinesOffset;
            MainBeamTotalLength = mainBeamTotalLength;
            SecondaryBeamTotalLength = secondaryBeamTotalLength;
            BeamOffset = beamOffset;
        }

        #endregion

    }
}
