using FormworkOptimize.App.Models.Base;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.App.Models
{
    public class PlywoodSelectionModel:SelectionModelBase
    {

        #region Properties

        public PlywoodSectionName Plywood { get;}

        #endregion

        #region Constructors

        public PlywoodSelectionModel(bool isSelected, PlywoodSectionName plywood)
            :base(isSelected)
        {
            Plywood = plywood;
        }

        #endregion

    }
}
