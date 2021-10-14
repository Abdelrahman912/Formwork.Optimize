using FormworkOptimize.App.Models.Base;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Models
{
    public class BeamSectionSelectionModel : SelectionModelBase
    {
        #region Properties

        public BeamSectionName BeamSection { get; }

        #endregion

        #region Constructors

        public BeamSectionSelectionModel(bool isSelected, BeamSectionName beamSection)
            :base(isSelected)
        {
            BeamSection = beamSection;
        }

        #endregion

    }
}
