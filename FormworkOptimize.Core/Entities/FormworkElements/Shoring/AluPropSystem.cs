namespace FormworkOptimize.Core.Entities
{
    public class AluPropSystem:PropShoring
    {

        #region Properties

        /// <summary>
        /// Capacity of aluminum prop (ton).
        /// </summary>
        /// <value>
        /// Unit (ton).
        /// </value>
        public double Capacity => 6.0;

        #endregion

        #region Constructors

        public AluPropSystem(double mainSpacing, double secondarySpacing)
            : base(mainSpacing, secondarySpacing)
        {
           
        }

        #endregion

    }
}
