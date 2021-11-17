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

        private int _currentIndex;

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
            _currentIndex = 0;
        }





        #endregion

        #region Methods

        private bool CanPrevious()
        {
            return DetailResults.IndexOf(CurrentDetailResult) >0 ;
        }

        private void OnPrevious()
        {
            CurrentDetailResult = DetailResults[_currentIndex-1];
            _currentIndex = DetailResults.IndexOf(CurrentDetailResult);
        }

        private bool CanNext()
        {
            return DetailResults.IndexOf(CurrentDetailResult) < DetailResults.Count-1;
        }

        private void OnNext()
        {
            CurrentDetailResult = DetailResults[_currentIndex+1];
            _currentIndex = DetailResults.IndexOf(CurrentDetailResult);
        }

        #endregion


    }
}
