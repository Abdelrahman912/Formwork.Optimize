using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities
{
    public class BeamSection : Section
    {

        #region Properties

        /// <summary>
        /// Allowable deflection.
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double DeltaAll { get; }

        public BeamSectionName SectionName { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BeamSection"/> class.
        /// </summary>
        /// <param name="a">Cross sectional area (cm^2).</param>
        /// <param name="i">Moment of inertia (cm^4).</param>
        /// <param name="z">Elastic modulus of section (cm^3).</param>
        /// <param name="deltaAll">Allowable deflection (cm).</param>
        /// <param name="fall">Allowable normal stress (t/cm^2).</param>
        /// <param name="qall">Allowable shear (ton).</param>
        /// <param name="mall">Allowable moment (ton.m).</param>
        /// <param name="e">Modulus of elasticity (t/cm^2).</param>
        /// <param name="sectionName">Name of the section.</param>
        internal BeamSection(double a, double i, double z, double deltaAll, double fall, double qall, double mall, double e,BeamSectionName sectionName)
            :base(a,i,z,fall,qall,mall,e)
        {
            SectionName = sectionName;
            DeltaAll = deltaAll;
        }

        #endregion
    }
}
