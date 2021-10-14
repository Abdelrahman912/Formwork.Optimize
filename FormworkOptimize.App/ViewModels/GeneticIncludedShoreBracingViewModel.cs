using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Constants;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticIncludedShoreBracingViewModel:GeneticIncludedBaseViewModel
    {
        #region Private Fields

        private bool _isSelectAllRows;

        #endregion

        #region Properties


        public List<LengthElementSelectionModel> Braces { get; }

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
                var ischanged = NotifyPropertyChanged(ref _isSelectAllRows, value);
                if (ischanged)
                    Braces.ForEach(p => p.IsSelected = _isSelectAllRows);
            }
        }

        #endregion

        #region Constructors

        public GeneticIncludedShoreBracingViewModel()
            : base("shore Cross Braces")
        {
            Braces = Database.ShoreBraceSystemCrossBraces.Select(l => new LengthElementSelectionModel(true, l, "Cross Brace")).ToList();
            IsSelectAllRows = true;
        }

        #endregion
    }
}
