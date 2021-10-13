using CSharp.Functional.Constructs;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Entities.GeneticParameters;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Errors.Errors;


namespace FormworkOptimize.App.ViewModels
{
    public class FormworkElementsIncludedInGeneticViewModel:ViewModelBase
    {

        #region Private Fields

        private readonly GeneticIncludedPlywoodsViewModel _includedPlywoodsVM;

        private readonly GeneticIncludedBeamSectionsViewModel _includedBeamSectionsVM;

        private GeneticIncludedBaseViewModel _selectedInludedElement;

        #endregion

        #region Properties

        public List<GeneticIncludedBaseViewModel> IncludedElements { get;  }

        public GeneticIncludedBaseViewModel SelectedInludedElement
        {
            get => _selectedInludedElement;
            set => NotifyPropertyChanged(ref _selectedInludedElement , value);
        }

        #endregion


        #region Constructors

        public FormworkElementsIncludedInGeneticViewModel()
        {
             _includedPlywoodsVM = new GeneticIncludedPlywoodsViewModel();
            _includedBeamSectionsVM = new GeneticIncludedBeamSectionsViewModel();
            IncludedElements = new List<GeneticIncludedBaseViewModel>()
            {
                _includedPlywoodsVM,
                _includedBeamSectionsVM
            };
        }

        #endregion

        #region Methods

        public Validation<GeneticIncludedElements> IncludedElementsService()
        {
            var includedPlywoods = _includedPlywoodsVM.Plywoods.Where(p => p.IsSelected).Select(p => p.Plywood).ToList();
            if (includedPlywoods.Count <= 1)
                return LessThanTwoPlywood;
            var includedBeamSections = _includedBeamSectionsVM.BeamSections.Where(bs => bs.IsSelected).Select(bs => bs.BeamSection).ToList();
            if (includedBeamSections.Count <= 1)
                return LessThanTwoBeamSection;
            return new GeneticIncludedElements(includedPlywoods, includedBeamSections);
        }

        #endregion

    }
}
