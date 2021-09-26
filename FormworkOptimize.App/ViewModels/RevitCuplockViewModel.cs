using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitCuplockViewModel:ViewModelBase
    {

        #region Private Fields

        private double _selectedLedgerMain;

        private double _selectedLedgerSecondary;

        private SteelType _selectedSteelType;

        #endregion

        #region Properties

        public SteelType SelectedSteelType
        {
            get => _selectedSteelType;
            set => NotifyPropertyChanged(ref  _selectedSteelType , value);
        }

        public double SelectedLedgerMain
        {
            get => _selectedLedgerMain;
            set 
            { 
                NotifyPropertyChanged(ref _selectedLedgerMain, value);
                Mediator.Instance.NotifyColleagues(_selectedLedgerMain, Context.MAIN_BEAM_MIN_LENGTH);
            }
        }

        public double SelectedLedgerSecondary
        {
            get => _selectedLedgerSecondary;
            set
            {
                NotifyPropertyChanged(ref _selectedLedgerSecondary, value);
                Mediator.Instance.NotifyColleagues(_selectedLedgerSecondary, Context.SEC_BEAM_MIN_LENGTH);
            }
        }

        public List<double> LedgerMainLengths { get; set; }

        public List<double> LedgerSecondaryLengths { get; set; }

        #endregion

        #region Constructors

        public RevitCuplockViewModel()
        {
            LedgerMainLengths = Database.LedgerLengths;
            LedgerSecondaryLengths = Database.LedgerLengths;
            SelectedLedgerMain = LedgerMainLengths.First();
            SelectedLedgerSecondary = LedgerSecondaryLengths.First();
            SelectedSteelType = SteelType.STEEL_37;
        }

        #endregion

    }
}
