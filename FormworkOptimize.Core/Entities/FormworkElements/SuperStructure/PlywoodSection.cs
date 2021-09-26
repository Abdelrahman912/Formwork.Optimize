using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities
{
    public class PlywoodSection : Section
    {

        #region Properties

        /// <summary>
        /// Thickness of plywood.
        /// </summary>
        /// <value>
        /// Unit (mm).
        /// </value>
        public double Thickness { get;  }

        public PlywoodSectionName SectionName { get; }

        #endregion

        #region Constructors

        internal PlywoodSection(double a, double i, double z,  double fall, double qall, double mall, double e,double thickness,PlywoodSectionName sectionName)
            :base(a,i,z,fall,qall,mall,e)
        {
            SectionName = sectionName;
            Thickness = thickness;
        }

        #endregion

    }
}
