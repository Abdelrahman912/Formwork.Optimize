using FormworkOptimize.App.ViewModels;
using FormworkOptimize.App.ViewModels.Mediators;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FormworkOptimize.App.UI.Windows
{
    /// <summary>
    /// Interaction logic for RevitFormworkWindow.xaml
    /// </summary>
    public partial class RevitFormworkWindow : Window
    {
        #region Private Fields

        private static RevitFormworkWindow _instance = null;

        #endregion

        #region Properties
        public static RevitFormworkWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RevitFormworkWindow();
                }
                else
                {
                    _instance.WindowState = WindowState.Normal;
                    _instance.Focus();
                }

                return _instance;
            }
        }

        #endregion


        #region Constructors
        private RevitFormworkWindow()
        {

            // Load Material Design Library for navisworks
            #region MaterialDesign

            ColorZoneAssist.SetMode(new GroupBox(), ColorZoneMode.Standard);
            Hue hue = new Hue("name", System.Windows.Media.Color.FromArgb(1, 2, 3, 4), System.Windows.Media.Color.FromArgb(1, 5, 6, 7));

            #endregion

            InitializeComponent();
            DataContext = new RevitFormworkViewModel();
        }

        #endregion

        #region Methods

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _instance = null;
            Mediator.Instance.Reset();
        }

        #endregion

    }
}
