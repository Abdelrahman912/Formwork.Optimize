using FormworkOptimize.App.ViewModels;
using FormworkOptimize.App.ViewModels.Mediators;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace FormworkOptimize.App.UI.Views
{
    /// <summary>
    /// Interaction logic for GeneticOptionsView.xaml
    /// </summary>
    public partial class GeneticOptionsView : UserControl
    {
        private readonly Action<int, int, string> _progressFunc;
        public GeneticOptionsView()
        {
            InitializeComponent();
            Action<int, int, string> progressFunc = (current, max, text) =>
            {
                progressBar.Dispatcher.Invoke(() =>
                {
                    progressBar.Value = current;
                    progressBar.Maximum = max;
                    txtProgress.Text = text;
                }, System.Windows.Threading.DispatcherPriority.Background);
            };
            _progressFunc = progressFunc;
            Mediator.Instance.Subscribe<GeneticOptionsViewModel>(this, OnNotified, ViewModels.Enums.Context.PROGRESS_FUNC);
        }

        private void OnNotified(GeneticOptionsViewModel obj)
        {
            obj.ProgressFunc = _progressFunc;
        }
    }
}
