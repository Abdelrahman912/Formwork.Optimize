using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Constants;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticIncludedLedgersViewModel:GeneticIncludedBaseViewModel
    {
        #region Private Fields

        private bool _isSelectAllRows;

        #endregion

        #region Properties


        public List<LengthElementSelectionModel> Ledgers { get; }

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
                var ischanged = NotifyPropertyChanged(ref _isSelectAllRows, value);
                if (ischanged)
                    Ledgers.ForEach(p => p.IsSelected = _isSelectAllRows);
            }
        }

        #endregion

        #region Constructors

        public GeneticIncludedLedgersViewModel()
            : base("Cuplock Ledgers")
        {
            Ledgers = Database.LedgerLengths.Select(l => new LengthElementSelectionModel(true,l,"Cuplock Ledger")).ToList();
           
            IsSelectAllRows = true;
        }

        #endregion
    }
}
