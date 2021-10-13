using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticIncludedPlywoodsViewModel:GeneticIncludedBaseViewModel
    {

        #region Private Fields

        private bool _isSelectAllRows;

        #endregion

        #region Properties


        public List<PlywoodSelectionModel> Plywoods { get; }

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
               var ischanged =  NotifyPropertyChanged(ref _isSelectAllRows, value);
                if(ischanged)
                    Plywoods.ForEach(p=>p.IsSelected = _isSelectAllRows);
            }
        }

        #endregion

        #region Constructors

        public GeneticIncludedPlywoodsViewModel()
            :base("Plywood")
        {
            Plywoods = new List<PlywoodSelectionModel>()
            {
                new PlywoodSelectionModel(true,PlywoodSectionName.BETOFILM_18MM),
                new PlywoodSelectionModel(true,PlywoodSectionName.COFIFORM_PLUS_1705MM),
                new PlywoodSelectionModel(true,PlywoodSectionName.DOUGLAS_FIR_19MM),
                new PlywoodSelectionModel(true,PlywoodSectionName.WISAFORM_BIRCH_18MM)
            };
            IsSelectAllRows = true;
        }

        #endregion

    }
}
