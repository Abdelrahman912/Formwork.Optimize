using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.DTOS
{
    public class TableDesignInput
    {

        #region Properties

        /// <summary>
        /// Thickness of slab (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double SlabThickness { get; }

        /// <summary>
        /// Thickness of beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double BeamThickness { get; }

        /// <summary>
        /// Width of beam (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double BeamWidth { get; }

        public TableSystemType TableType { get;}

        #endregion

        #region Constructors

        public TableDesignInput(TableSystemType tableType,double slabThickness, double beamThickness = 0, double beamWidth = 0)

        {
            SlabThickness = slabThickness;
            BeamThickness = beamThickness;
            BeamWidth = beamWidth;
            TableType = tableType;
        }

        #endregion
    }
}
