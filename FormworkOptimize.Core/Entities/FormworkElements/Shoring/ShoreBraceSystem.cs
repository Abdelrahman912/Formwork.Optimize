namespace FormworkOptimize.Core.Entities
{
    public class ShoreBraceSystem:FrameShoring
    {

        #region Properties

        /// <summary>
        /// Capacity of frame (ton).
        /// </summary>
        /// <value>
        /// Unit (ton).
        /// </value>
        public double Capacity => 7;

        #endregion

        #region Constructors

        public ShoreBraceSystem(double spacing)
            : base(120, spacing) { }
        

        #endregion

    }
}
