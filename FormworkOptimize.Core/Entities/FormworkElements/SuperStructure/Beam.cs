using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Exceptions;

namespace FormworkOptimize.Core.Entities
{
    public class Beam : StructuralElement
    {

        #region Properties

        public BeamSection Section { get; }

        /// <summary>
        /// Total length of the beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double BeamLength { get; }

        public override int NumberOfSpans { get; }

        /// <summary>
        /// Cantilever length of the beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double CantileverLength { get; }

        public BeamType BeamType { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Beam"/> class.
        /// </summary>
        /// <param name="section">Beam section.</param>
        /// <param name="span">Span (cm).</param>
        /// <param name="beamLength">Length of the beam (cm).</param>
        /// <exception cref="LayoutException">Total Length of the beam: {BeamLength} is less than its span length: {span}</exception>
        internal Beam(BeamSection section, double span, double beamLength,int nSpans,double cantLength)
            : base(span)
        {
            BeamLength = beamLength;
            NumberOfSpans = nSpans;
            CantileverLength = cantLength;
            Section = section;
        }

        #endregion
    }
}
