using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Constants;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticIncludedTubesViewModel: GeneticIncludedBaseViewModel
    {
        #region Private Fields

        private bool _isSelectAllRows;

        #endregion

        #region Properties


        public List<LengthElementSelectionModel> Tubes { get; }

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
                var ischanged = NotifyPropertyChanged(ref _isSelectAllRows, value);
                if (ischanged)
                    Tubes.ForEach(p => p.IsSelected = _isSelectAllRows);
            }
        }

        #endregion

        #region Constructors

        public GeneticIncludedTubesViewModel()
            : base("Cuplock Cross Braces")
        {
            Tubes = Database.CuplockCrossBraceLengths.Select(l => new LengthElementSelectionModel(true, l, "Scaffolding Tube")).ToList();

            IsSelectAllRows = true;
        }

        #endregion
    }
}
