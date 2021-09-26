using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using static FormworkOptimize.Core.SelectionFilters.Filters;

namespace FormworkOptimize.App.ViewModels
{
    public class SuperstructureViewModel : ViewModelBase
    {
        #region Private Fields

        private readonly UIDocument _uiDoc;

        private readonly Document _doc;

        private PlywoodSectionName selectedPlywoodSection;

        private BeamSectionName selectedSecondaryBeamSection;

        private ObservableCollection<double> secondaryBeamLengths;

        private double selectedSecondaryBeamLength;

        private BeamSectionName selectedMainBeamSection;

        private ObservableCollection<double> mainBeamLengths;

        private double selectedMainBeamLength;

        private Element selectedElement;

        #endregion

        #region Properties

        public ObservableCollection<double> SecondaryBeamLengths
        {
            get => secondaryBeamLengths;
            set 
            { 
                NotifyPropertyChanged(ref secondaryBeamLengths, value);
                if (secondaryBeamLengths != null && secondaryBeamLengths.Count > 0)
                    SelectedSecondaryBeamLength = secondaryBeamLengths.First();
            }
        }

        public ObservableCollection<double> MainBeamLengths
        {
            get => mainBeamLengths;
            set
            {
                NotifyPropertyChanged(ref mainBeamLengths, value);
                if(mainBeamLengths != null && mainBeamLengths.Count > 0)
                    SelectedMainBeamLength = mainBeamLengths.First();
            }
        }

        public PlywoodSectionName SelectedPlywoodSection
        {
            get => selectedPlywoodSection;
            set => NotifyPropertyChanged(ref selectedPlywoodSection, value);
        }

        public BeamSectionName SelectedSecondaryBeamSection
        {
            get => selectedSecondaryBeamSection;
            set
            {
                NotifyPropertyChanged(ref selectedSecondaryBeamSection, value);
                SecondaryBeamLengths = new ObservableCollection<double>(Database.GetBeamLengths( SelectedSecondaryBeamSection));
            }
        }

        public double SelectedSecondaryBeamLength
        {
            get => selectedSecondaryBeamLength;
            set => NotifyPropertyChanged(ref selectedSecondaryBeamLength, value);
        }

        public BeamSectionName SelectedMainBeamSection
        {
            get => selectedMainBeamSection;
            set
            {
                NotifyPropertyChanged(ref selectedMainBeamSection, value);
                MainBeamLengths = new ObservableCollection<double>(Database.GetBeamLengths(SelectedMainBeamSection));
            }
        }

        public double SelectedMainBeamLength
        {
            get => selectedMainBeamLength;
            set => NotifyPropertyChanged(ref selectedMainBeamLength, value);
        }

        public Element SelectedElement
        {
            get => selectedElement;
            set => NotifyPropertyChanged(ref selectedElement, value);
        }

        public RelayCommand SelectElementCommand { get; private set; }

        #endregion

        #region Constructors

        public SuperstructureViewModel()
        {
            _uiDoc = RevitBase.UIDocument;
            _doc = _uiDoc.Document;
            SelectElementCommand = new RelayCommand(SelectElement);
            SelectedSecondaryBeamSection = BeamSectionName.ACROW_BEAM_S12;
            SelectedMainBeamSection = BeamSectionName.ACROW_BEAM_S12;
        }

        #endregion

        #region Methods

        private void SelectElement()
        {
            _uiDoc.PickElement(FloorAndBeamFilter, "Please Select a concrete floor or concrete beam.")
                  .Map(ele => SelectedElement = ele);
        }

        #endregion
    }
}
