using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticIncludedPropsViewModel: GeneticIncludedBaseViewModel
    {
        #region Private Fields

        private bool _isSelectAllRows;

        #endregion

        #region Properties


        public List<EuropeanPropsSelectionModel> Props { get; }

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
                var ischanged = NotifyPropertyChanged(ref _isSelectAllRows, value);
                if (ischanged)
                    Props.ForEach(p => p.IsSelected = _isSelectAllRows);
            }
        }

        #endregion

        #region Constructors

        public GeneticIncludedPropsViewModel()
            : base("European Props")
        {
            Props = Enum.GetValues(typeof(EuropeanPropTypeName)).Cast<EuropeanPropTypeName>().Select(type => new EuropeanPropsSelectionModel(true, type)).ToList();
            IsSelectAllRows = true;
        }

        #endregion
    }
}
