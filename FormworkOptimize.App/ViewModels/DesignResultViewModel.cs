using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Entities;

namespace FormworkOptimize.App.ViewModels
{
    public class DesignResultViewModel : ViewModelBase
    {
        #region Private Fields

        private SectionDesignOutput plywoodDesignOutput;

        private SectionDesignOutput secondaryBeamDesignOutput;

        private SectionDesignOutput mainBeamDesignOutput;

        private ShoringDesignOutput shoringSystemDesignOutput;

        private string _plywoodHeader;

        private string _secondaryBeamHeader;

        private string _mainBeamHeader;

        private string _shoringHeader;

        #endregion

        #region Properties

        public string ShoringHeader
        {
            get => _shoringHeader;
            set => NotifyPropertyChanged(ref _shoringHeader,value);
        }

        public string MainBeamHeader
        {
            get => _mainBeamHeader;
            set => NotifyPropertyChanged(ref _mainBeamHeader, value);
        }

        public string SecondaryBeamHeader
        {
            get => _secondaryBeamHeader;
            set => NotifyPropertyChanged(ref _secondaryBeamHeader, value);
        }

        public string PlywoodHeader
        {
            get => _plywoodHeader;
            set => NotifyPropertyChanged(ref _plywoodHeader, value);
        }

        public SectionDesignOutput PlywoodDesignOutput
        {
            get => plywoodDesignOutput;
            set
            {
                NotifyPropertyChanged(ref plywoodDesignOutput, value);
                if (plywoodDesignOutput != null)
                    PlywoodHeader = $"Plywood Check Result: ({plywoodDesignOutput.SectionName})";
            }
        }

        public SectionDesignOutput SecondaryBeamDesignOutput
        {
            get => secondaryBeamDesignOutput;
            set
            {
                NotifyPropertyChanged(ref secondaryBeamDesignOutput, value);
                if (secondaryBeamDesignOutput != null)
                    SecondaryBeamHeader = $"Secondary Beam Check Result: ({secondaryBeamDesignOutput.SectionName})";
            }
        }

        public SectionDesignOutput MainBeamDesignOutput
        {
            get => mainBeamDesignOutput;
            set
            {
                NotifyPropertyChanged(ref mainBeamDesignOutput, value);
                if (mainBeamDesignOutput != null)
                    MainBeamHeader = $"Main Beam Check Result: ({mainBeamDesignOutput.SectionName})";
            }
        }

        public ShoringDesignOutput ShoringSystemDesignOutput
        {
            get => shoringSystemDesignOutput;
            set
            {
                NotifyPropertyChanged(ref shoringSystemDesignOutput, value);
                if(shoringSystemDesignOutput != null)
                    ShoringHeader = $"Shoring System Check Result: ({shoringSystemDesignOutput.ShoringSystemName})";
            }
        }

        #endregion

        #region Constructors

        public DesignResultViewModel()
        {
            PlywoodHeader = "Plywood Check Result: ";
            SecondaryBeamHeader = "Secondary Beam Check Result: ";
            MainBeamHeader = "Main Beam Check Result: ";
            ShoringHeader = "Shoring System Check Result: ";
        }

        #endregion

    }
}
