namespace FormworkOptimize.Core.Entities
{
    public class StrainingActions
    {

        #region Properties


        /// <summary>
        /// Maximum actual moment (t.m).
        /// </summary>
        /// <value>
        /// Unit (t.m).
        /// </value>
        public double MaxMoment { get; }

        /// <summary>
        /// Maximum actual shear (ton).
        /// </summary>
        /// <value>
        /// Unit (ton).
        /// </value>
        public double MaxShear { get; }

        /// <summary>
        /// Maximum actual normal (ton).
        /// </summary>
        /// <value>
        /// Unit (ton).
        /// </value>
        public double MaxNormal { get; }

        /// <summary>
        /// Maximum actual deflection (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double MaxDeflection { get; }

        #endregion

        #region Constructors

        internal StrainingActions( double maxMoment, double maxShear, double maxNormal, double maxDeflection)
        {
            MaxMoment = maxMoment;
            MaxShear = maxShear;
            MaxNormal = maxNormal;
            MaxDeflection = maxDeflection;
        }

        #endregion

    }
}
