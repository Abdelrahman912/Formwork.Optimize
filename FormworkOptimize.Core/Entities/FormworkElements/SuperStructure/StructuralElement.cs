namespace FormworkOptimize.Core.Entities
{
    public abstract class StructuralElement
    {

        #region Properties

        /// <summary>
        /// Sapn Length.
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double Span { get; }

        public virtual int NumberOfSpans { get; }

        #endregion

        #region Constructors

        public StructuralElement( double span)
        {
            Span = span;
        }

        #endregion

    }
}
