using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Entities.GeneticResult;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticHistoryViewModel:ViewModelBase
    {

        #region Private Fields

        private List<int> _lables;

        private List<ChromosomeHistory> _history;

        #endregion

        #region Properties

       

        public List<int> Labels
        {
            get => _lables;
            set=>NotifyPropertyChanged(ref _lables, value);
        }

        public SeriesCollection SeriesCollection { get; set; }

        public Func<int, string> XFormatter { get; set; }

        public Func<double, string> YFormatter { get; set; }

        public ICommand ExportChartDataCommand { get; }

        #endregion

        #region constructors

        public GeneticHistoryViewModel(List<ChromosomeHistory> history)
        {
            _history = history;
            SeriesCollection = new SeriesCollection()
            {
                new LineSeries
                {
                    Title = "Fitness",
                    LineSmoothness = 1,
                    Values = new ChartValues<double> (history.Select(chm=>Math.Round(chm.Fitness,2)))
                }
            };

            var nGenerations = history.Count;
            var intervals =(int)Math.Ceiling(nGenerations / 10.0);
            Labels = Enumerable.Range(0, 10).Select(i => i * 10).ToList();
            XFormatter =(num)=>num.ToString();
            YFormatter =(num)=> Math.Round(num, 2).ToString();
            ExportChartDataCommand = new RelayCommand(OnExport, CanExport);
        }



        #endregion

        #region Methods

        private bool CanExport() => 
            _history != null &&
            _history.Count > 0;
        

        private void OnExport()
        {
            Mediator.Instance.NotifyColleagues(_history, Context.EXPORT_GA_HISTORY_DATA);
        }

        #endregion

    }
}
