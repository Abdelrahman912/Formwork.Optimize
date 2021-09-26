using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities
{
    public class TableLayout
    {
        #region Properties

        public TableSystemType Name { get; }

        /// <summary>
        /// Span of main beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double MainSpan { get;}

        /// <summary>
        /// Span of secondary beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double SecondarySpan { get;}

        /// <summary>
        /// Spacing between main beams (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double MainSpacing { get;}

        /// <summary>
        /// Max. Spacing between secondary beams (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double SecondarySpacing { get;}

        /// <summary>
        /// Cantilever length of main beam (cm).
        /// </summary>
        /// <value>
        /// The main lc.
        /// </value>
        public double MainLc { get;}

        /// <summary>
        /// Cantilever length of secondary beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double SecondaryLc { get;}

        /// <summary>
        /// total length of the main beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double MainTotalLength { get; }

        /// <summary>
        /// total length of secondary beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double SecondaryTotalLength { get;}

        #endregion

        #region Constructors

        internal TableLayout(TableSystemType name, double mainSpan, double secondarySpan, double secondarySpacing, double mainLc, double secondaryLc)
        {
            Name = name;
            MainSpan = mainSpan;
            SecondarySpan = secondarySpan;
            MainSpacing = SecondarySpan;
            SecondarySpacing = secondarySpacing;
            MainLc = mainLc;
            SecondaryLc = secondaryLc;
            MainTotalLength = MainSpan + 2 * MainLc + 2 * 10;
            SecondaryTotalLength = SecondarySpan + 2 * SecondaryLc;
        }

        #endregion

    }
}
