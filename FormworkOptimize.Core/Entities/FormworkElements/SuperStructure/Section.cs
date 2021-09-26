namespace FormworkOptimize.Core.Entities
{
    public abstract class Section
    {
        #region Properties

        /// <summary>
        /// Cross sectional area.
        /// </summary>
        /// <value>
        /// Unit (cm^2).
        /// </value>
        public double A { get; }

        /// <summary>
        /// Moment of inertia.
        /// </summary>
        /// <value>
        /// Unit (cm^4).
        /// </value>
        public double I { get; }

        /// <summary>
        /// Elastic modulus of section.
        /// </summary>
        /// <value>
        /// Unit (cm^3).
        /// </value>
        public double Z { get; }

        /// <summary>
        /// Allowable normal stress.
        /// </summary>
        /// <value>
        /// Unit (t/cm^2).
        /// </value>
        public double Fall { get; }

        /// <summary>
        /// Allowable shear force.
        /// </summary>
        /// <value>
        /// Unit (ton).
        /// </value>
        public double Qall { get; }

        /// <summary>
        /// Allowable moment.
        /// </summary>
        /// <value>
        /// Unit (t.m).
        /// </value>
        public double Mall { get; }

        /// <summary>
        /// Modulus of elasticity.
        /// </summary>
        /// <value>
        /// Unit (t/cm^2).
        /// </value>
        public double E { get; }

        #endregion

        #region Constructors

        internal Section(double a , double i , double z  , double fall , double qall , double mall , double e)
        {
            A = a;
            I = i;
            Z = z;
            Fall = fall;
            Qall = qall;
            Mall = mall;
            E = e;
        }

        #endregion

    }
}
