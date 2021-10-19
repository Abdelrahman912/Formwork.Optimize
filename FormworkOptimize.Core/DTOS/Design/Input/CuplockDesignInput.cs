using FormworkOptimize.Core.Entities.Geometry;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.DTOS
{
    public class CuplockDesignInput
    {
        #region Properties

        public SecondaryBeamSpacing SecondaryBeamSpacing { get;}

        /// <summary>
        /// Thickness of slab (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double SlabThickness { get; }

        /// <summary>
        /// Thickness of beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double BeamThickness { get; }

        /// <summary>
        /// Width of beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double BeamWidth { get; }

        public PlywoodSectionName PlywoodSection { get; }

        public BeamSectionName SecondaryBeamSection { get; }

        public BeamSectionName MainBeamSection { get; }

        /// <summary>
        /// Gets the total length of the secondary beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double SecondaryBeamTotalLength { get; }

        /// <summary>
        /// Gets the total length of the main beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double MainBeamTotalLength { get; }

        /// <summary>
        /// Ledger length in direction of main beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double LedgersMainDir { get; }

        /// <summary>
        /// Ledger length in direction of secondary beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double LedgersSecondaryDir { get; }

        public SteelType SteelType { get;}

        #endregion

        #region Constructors

        public CuplockDesignInput(PlywoodSectionName plywoodSection, BeamSectionName secondaryBeamSection, BeamSectionName mainBeamSection,
                                  SteelType steelType, double ledgersMainDir, double ledgersSecondaryDir, double secondaryBeamTotalLength,
                                  double mainBeamTotalLength, double slabThickness,SecondaryBeamSpacing secondaryBeamSpacing, double beamThickness = 0, double beamWidth = 0)
        {
            PlywoodSection = plywoodSection;
            SecondaryBeamSection = secondaryBeamSection;
            MainBeamSection = mainBeamSection;
            SteelType = steelType;
            LedgersMainDir = ledgersMainDir;
            LedgersSecondaryDir = ledgersSecondaryDir;
            SecondaryBeamTotalLength = secondaryBeamTotalLength;
            MainBeamTotalLength = mainBeamTotalLength;
            SecondaryBeamSpacing = secondaryBeamSpacing;
            SlabThickness = slabThickness;
            BeamThickness = beamThickness;
            BeamWidth = beamWidth;
        }

        #endregion

    }
}
