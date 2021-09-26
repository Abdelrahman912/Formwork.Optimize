using FormworkOptimize.App.ViewModels;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FormworkOptimize.App.UI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DesignFormworkWindow : Window
    {
        private static DesignFormworkWindow instance = null;

        public static DesignFormworkWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DesignFormworkWindow();
                }
                else
                {
                    instance.WindowState = WindowState.Normal;
                    instance.Focus();
                }

                return instance;
            }
        }

        private DesignFormworkWindow()
        {
            // Load Material Design Library for revit
            #region MaterialDesign

            ColorZoneAssist.SetMode(new GroupBox(), ColorZoneMode.Standard);
            Hue hue = new Hue("name", System.Windows.Media.Color.FromArgb(1, 2, 3, 4), System.Windows.Media.Color.FromArgb(1, 5, 6, 7));

            #endregion

            DataContext = new DesignFormworkViewModel();
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            instance = null;
        }
    }
}
