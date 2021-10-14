using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Constants;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticIncludedCuplockVerticalsViewModel : GeneticIncludedBaseViewModel
    {
        #region Private Fields

        private bool _isSelectAllRows;

        #endregion

        #region Properties


        public List<LengthElementSelectionModel> Verticals { get; }

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
                var ischanged = NotifyPropertyChanged(ref _isSelectAllRows, value);
                if (ischanged)
                    Verticals.ForEach(p => p.IsSelected = _isSelectAllRows);
            }
        }

        #endregion

        #region Constructors

        public GeneticIncludedCuplockVerticalsViewModel()
            : base("Cuplock Verticals")
        {
            Verticals = Database.CuplockVerticalLengths.Select(l => new LengthElementSelectionModel(true, l,"Cuplock Vertical")).ToList();

            IsSelectAllRows = true;
        }

        #endregion
    }
}
