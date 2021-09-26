using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitShoreViewModel : ViewModelBase
    {
        #region Private Fields

        private double _selectedSpacingMain;

        private double _spacingSec;

        private bool _isSecSpacingVisible;

        #endregion

        #region Properties
        public double SelectedSpacingMain
        {
            get => _selectedSpacingMain;
            set 
            { 
                NotifyPropertyChanged(ref _selectedSpacingMain, value);
                Mediator.Instance.NotifyColleagues(_selectedSpacingMain, Context.MAIN_BEAM_MIN_LENGTH);
            }
        }

        public double SpacingSec
        {
            get => _spacingSec;
            set
            {
                NotifyPropertyChanged(ref _spacingSec, value);
                Mediator.Instance.NotifyColleagues(_spacingSec + Database.SHORE_MAIN_HALF_WIDTH * 2, Context.SEC_BEAM_MIN_LENGTH);
            }
        }

        public bool IsSecSpacingVisible
        {
            get => _isSecSpacingVisible;
            set => NotifyPropertyChanged(ref _isSecSpacingVisible, value);
        }


        public List<double> MainSpacings { get; set; }
        #endregion

        #region Constructors

        public RevitShoreViewModel(bool isSecSpacingVisible = true)
        {
            MainSpacings = Database.ShoreBraceSystemCrossBraces;
            SelectedSpacingMain = MainSpacings.First();
            SpacingSec = 100;
            IsSecSpacingVisible = isSecSpacingVisible;
        }

        #endregion

    }
}
