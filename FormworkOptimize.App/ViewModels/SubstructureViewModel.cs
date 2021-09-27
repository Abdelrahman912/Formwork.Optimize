using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Entities.Genetic;
using System.Collections.ObjectModel;

namespace FormworkOptimize.App.ViewModels
{
    /// <summary>
    /// Abstract class that conatins common propertis for all substructure view models (Cuplock, Frame, ...etc)
    /// </summary>
    public abstract class SubstructureViewModel : ViewModelBase
    {
        #region Private Fields

        private SuperstructureViewModel superstructureViewModel;

        private DesignResultViewModel designResultViewModel;

        //private bool isDesignResultViewVisible;

        #endregion

        #region Properties

        public SuperstructureViewModel SuperstructureViewModel
        {
            get => superstructureViewModel;
            private set => NotifyPropertyChanged(ref superstructureViewModel, value);
        }

        public DesignResultViewModel DesignResultViewModel
        {
            get => designResultViewModel;
            set => NotifyPropertyChanged(ref designResultViewModel, value);
        }

        //public bool IsDesignResultViewVisible
        //{
        //    get => isDesignResultViewVisible;
        //    set => NotifyPropertyChanged(ref isDesignResultViewVisible, value);
        //}

        public RelayCommand DesignElementCommand { get; private set; }

        public RelayCommand DesignGeneticCommand { get; private set; }

        #endregion

        #region Constructors

        public SubstructureViewModel()
        {
            SuperstructureViewModel = new SuperstructureViewModel();

            DesignResultViewModel = new DesignResultViewModel();

            DesignElementCommand = new RelayCommand(DesignElement, CanDesign);

            //DesignGeneticCommand = new RelayCommand(DesignGenetic, CanDesignGenetic);

            //IsDesignResultViewVisible = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The method that allows the user to desgin the selected element.
        /// </summary>
        protected abstract void DesignElement();

        /// <summary>
        /// The method that checks for the design command
        /// </summary>
        /// <returns></returns>
        protected abstract bool CanDesign();

        //protected abstract void OnDesign();

        //protected abstract void DesignGenetic();

        //protected abstract bool CanDesignGenetic();
        #endregion
    }
}
