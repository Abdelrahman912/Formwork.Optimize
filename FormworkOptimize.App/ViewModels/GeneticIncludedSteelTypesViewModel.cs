using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticIncludedSteelTypesViewModel: GeneticIncludedBaseViewModel
    {
        #region Private Fields

        private bool _isSelectAllRows;

        #endregion

        #region Properties


        public List<SteelTypeSelectionModel> SteelTypes { get; }

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
                var ischanged = NotifyPropertyChanged(ref _isSelectAllRows, value);
                if (ischanged)
                    SteelTypes.ForEach(p => p.IsSelected = _isSelectAllRows);
            }
        }

        #endregion

        #region Constructors

        public GeneticIncludedSteelTypesViewModel()
            : base("Steel Type")
        {
            SteelTypes = Enum.GetValues(typeof(SteelType)).Cast<SteelType>().Select(type => new SteelTypeSelectionModel(true,type)).ToList();
            IsSelectAllRows = true;
        }

        #endregion
    }
}
