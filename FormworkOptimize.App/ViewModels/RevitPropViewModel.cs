using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitPropViewModel : ViewModelBase
    {
        #region Private Fields

        private double _spacingMain;

        private double _spacingSecondary;

        private EuropeanPropTypeName _selectedPropType;

        

        #endregion

        #region Properties

        public bool IsDisplaySpacing { get; }

        public double SpacingMain
        {
            get => _spacingMain;
            set 
            { 
                NotifyPropertyChanged(ref _spacingMain, value);
                Mediator.Instance.NotifyColleagues(_spacingMain, Context.MAIN_BEAM_MIN_LENGTH);
            }
        }

        public double SpacingSecondary
        {
            get => _spacingSecondary;
            set 
            { 
                NotifyPropertyChanged(ref _spacingSecondary, value);
                Mediator.Instance.NotifyColleagues(_spacingSecondary, Context.SEC_BEAM_MIN_LENGTH);

            }
        }

        public EuropeanPropTypeName SelectedPropType
        {
            get => _selectedPropType;
            set => NotifyPropertyChanged(ref _selectedPropType, value);
        }
        #endregion

        #region Constructors

        public RevitPropViewModel(bool isDisplaySpacing = true)
        {
            SpacingMain = 100;
            SpacingSecondary = 100;
            SelectedPropType = EuropeanPropTypeName.D40;
            IsDisplaySpacing = isDisplaySpacing;
        }

        #endregion

    }
}
