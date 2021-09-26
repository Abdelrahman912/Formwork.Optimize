namespace FormworkOptimize.Core.Entities
{
    public abstract class PropShoring : Shoring
    {
        #region Properties

        /// <summary>
        /// Spacing between props in direction of main beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double MainSpacing { get; }

        /// <summary>
        /// Spacing between props in direction of secondary beams (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double SecondarySpacing { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropShoring"/> class.
        /// </summary>
        /// <param name="mainSpacing">Spacing between props in direction of main beams (cm).</param>
        /// <param name="secondarySpacing">Spacing between props in direction of secondary beams (cm).</param>
        /// <param name="capacity">Capacity of prop (ton).</param>
        protected PropShoring(double mainSpacing, double secondarySpacing)
        {
            MainSpacing = mainSpacing;
            SecondarySpacing = secondarySpacing;
        }

        #endregion
    }
}
