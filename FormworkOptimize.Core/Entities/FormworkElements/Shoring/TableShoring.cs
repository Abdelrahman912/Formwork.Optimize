using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Helpers.DesignHelper;
using static FormworkOptimize.Core.Constants.Database;
namespace FormworkOptimize.Core.Entities
{
    public class TableShoring:Shoring
    {
        #region Properties

        public TableLayout Layout { get;}

        public Beam MainBeam { get; }

        public Beam SecondaryBeam { get;}

        #endregion

        #region Constructors

        public TableShoring(TableSystemType tableType)
        {
            //Layout = GetTableLayout(tableType);
            //MainBeam = GetBeamSection(BeamSectionName.DOUBLE_TIMBER_H20).AsBeam( Layout.MainSpan, Layout.MainTotalLength));
            //SecondaryBeam = GetBeamSection(BeamSectionName.TIMBER_H20).AsBeam( Layout.SecondarySpan, Layout.SecondaryTotalLength));
            //TODO: Sheeting.
        }

        #endregion

    }
}
