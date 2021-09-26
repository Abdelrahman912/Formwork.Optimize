using Autodesk.Revit.UI;
using FormworkOptimize.App.ViewModels;
using FormworkOptimize.App.ViewModels.Mediators;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using static FormworkOptimize.App.UI.Services.Services;


namespace FormworkOptimize.App.UI.Windows
{
    /// <summary>
    /// Interaction logic for GeneticFormworkWindow.xaml
    /// </summary>
    public partial class GeneticFormworkWindow : Window
    {

        #region Private Fields

        private static GeneticFormworkWindow _instance;

        private readonly GeneticFormworkViewModel _geneticFormworkVM;

        #endregion

        #region Constructors

        private GeneticFormworkWindow(UIDocument uiDoc)
        {

            // Load Material Design Library for navisworks
            #region MaterialDesign

            //ColorZoneAssist.SetMode(new GroupBox(), ColorZoneMode.Standard);
            //var hue = new Hue("name", System.Windows.Media.Color.FromArgb(1, 2, 3, 4), System.Windows.Media.Color.FromArgb(1, 5, 6, 7));

            #endregion

            InitializeComponent();
            _geneticFormworkVM = new GeneticFormworkViewModel(uiDoc, ResultMessagesService);
            DataContext = _geneticFormworkVM;
        }

        #endregion

        #region Methods

        public static GeneticFormworkWindow Instance(UIDocument uiDoc)
        {
            if (_instance == null)
            {
                _instance = new GeneticFormworkWindow(uiDoc);
            }
            else
            {
                _instance.WindowState = WindowState.Normal;
                _instance.Focus();
            }

            return _instance;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _instance = null;
            Mediator.Instance.Reset();
        }

        #endregion

    }
}
