using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.DTOS
{
    public class EuropeanPropDesignInput
    {

        #region Properties

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
        /// Spacing between two consecutive props in main beam direction (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double MainSpacing { get; }

        /// <summary>
        /// Spacing between two consecutive props in secondary direction (cm).
        /// </summary>
        /// <value>
        /// Units (cm).
        /// </value>
        public double SecondarySpacing { get; }

        public EuropeanPropTypeName EuropeanPropType { get; }

        #endregion

        #region Constructors

        public EuropeanPropDesignInput(PlywoodSectionName plywoodSection, BeamSectionName secondaryBeamSection, BeamSectionName mainBeamSection,EuropeanPropTypeName europeanPropType,
                                       double mainSpacing, double secondarySpacing, double secondaryBeamTotalLength, double mainBeamTotalLength,
                                       double slabThickness, double beamThickness = 0, double beamWidth = 0)
        {
            PlywoodSection = plywoodSection;
            SecondaryBeamSection = secondaryBeamSection;
            MainBeamSection = mainBeamSection;
            EuropeanPropType = europeanPropType;
            MainSpacing = mainSpacing;
            SecondarySpacing = secondarySpacing;
            SecondaryBeamTotalLength = secondaryBeamTotalLength;
            MainBeamTotalLength = mainBeamTotalLength;
            SlabThickness = slabThickness;
            BeamThickness = beamThickness;
            BeamWidth = beamWidth;
        }

        #endregion

    }
}
