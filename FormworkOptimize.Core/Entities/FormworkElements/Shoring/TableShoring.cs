using FormworkOptimize.Core.Enums;
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
            Layout = GetTableLayout(tableType);
            MainBeam = new Beam(GetBeamSection(BeamSectionName.DOUBLE_TIMBER_H20), Layout.MainSpan, Layout.MainTotalLength);
            SecondaryBeam = new Beam(GetBeamSection(BeamSectionName.TIMBER_H20), Layout.SecondarySpan, Layout.SecondaryTotalLength);
            //TODO: Sheeting.
        }

        #endregion

    }
}
