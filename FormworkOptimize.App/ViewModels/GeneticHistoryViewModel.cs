using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Entities.GeneticResult;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticHistoryViewModel:ViewModelBase
    {

        #region Private Fields

        private List<int> _lables;

        #endregion

        #region Properties

       

        public List<int> Labels
        {
            get => _lables;
            set=>NotifyPropertyChanged(ref _lables, value);
        }

        public SeriesCollection SeriesCollection { get; set; }

        public Func<int, string> XFormatter { get; set; }

        #endregion

        #region constructors

        public GeneticHistoryViewModel(List<ChromosomeHistory> history)
        {
            SeriesCollection = new SeriesCollection()
            {
                new LineSeries
                {
                    Title = "Genetic History",
                    LineSmoothness = 1,
                    Values = new ChartValues<ObservablePoint> (history.Select(chm=>new ObservablePoint(chm.GenerationNumber, Math.Round(chm.Fitness,2))))
                }
            };

            var nGenerations = history.Count;
            var intervals =(int)Math.Ceiling(nGenerations / 10.0);
            Labels = Enumerable.Range(0, 10).Select(i => i * 10).ToList();
            XFormatter =(num)=>num.ToString();
        }

        #endregion

        #region Methods

        #endregion

    }
}
