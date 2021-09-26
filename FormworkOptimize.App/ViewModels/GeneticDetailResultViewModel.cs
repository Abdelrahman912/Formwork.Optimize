using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticDetailResultViewModel:ViewModelBase
    {

        #region Private Fields

        private IGeneticDetailResult _currentDetailResult;

        #endregion

        #region Properties

        public bool IsNavigationVisible { get; }

        public ICommand NextCommand { get; }

        public ICommand PreviousCommand { get; }

        public IGeneticDetailResult CurrentDetailResult
        {
            get => _currentDetailResult;
            set => NotifyPropertyChanged(ref _currentDetailResult, value);
        }

        public List<IGeneticDetailResult> DetailResults { get; }

        #endregion

        #region Constructors

        public GeneticDetailResultViewModel(List<IGeneticDetailResult> detailResults)
        {
            DetailResults = detailResults;
            IsNavigationVisible = DetailResults.Count > 1 ? true : false;
            CurrentDetailResult = DetailResults.First();
            NextCommand = new RelayCommand(OnNext, CanNext);
            PreviousCommand = new RelayCommand(OnPrevious, CanPrevious);
        }





        #endregion

        #region Methods

        private bool CanPrevious()
        {
            return DetailResults.IndexOf(CurrentDetailResult) == 1;
        }

        private void OnPrevious()
        {
            CurrentDetailResult = DetailResults.FirstOrDefault();
        }

        private bool CanNext()
        {
            return DetailResults.IndexOf(CurrentDetailResult) == 0;
        }

        private void OnNext()
        {
            CurrentDetailResult = DetailResults.LastOrDefault();
        }

        #endregion


    }
}
