using CSharp.Functional.Constructs;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Entities.GeneticParameters;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.App.Errors.Errors;


namespace FormworkOptimize.App.ViewModels
{
    public class FormworkElementsIncludedInGeneticViewModel:ViewModelBase
    {

        #region Private Fields

        private readonly GeneticIncludedPlywoodsViewModel _includedPlywoodsVM;

        private readonly GeneticIncludedBeamSectionsViewModel _includedBeamSectionsVM;

        private readonly GeneticIncludedLedgersViewModel _includedLedgersVM;

        private readonly GeneticIncludedCuplockVerticalsViewModel _includedVerticalsVM;

        private readonly GeneticIncludedTubesViewModel _includedTubesVM;

        private readonly GeneticIncludedSteelTypesViewModel _includedSteelTypesVM;

        private readonly GeneticIncludedPropsViewModel _includedPropsVM;

        private readonly GeneticIncludedShoreBracingViewModel _includedShoreBracingVM;

        private readonly GeneticIncludedFrameTypesViewModel _includedFrameTypeVM;

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
            _includedLedgersVM = new GeneticIncludedLedgersViewModel();
            _includedVerticalsVM = new GeneticIncludedCuplockVerticalsViewModel();
            _includedTubesVM = new GeneticIncludedTubesViewModel();
            _includedSteelTypesVM = new GeneticIncludedSteelTypesViewModel();
            _includedPropsVM = new GeneticIncludedPropsViewModel();
            _includedShoreBracingVM = new GeneticIncludedShoreBracingViewModel();
            _includedFrameTypeVM = new GeneticIncludedFrameTypesViewModel();
            IncludedElements = new List<GeneticIncludedBaseViewModel>()
            {
                _includedPlywoodsVM,
                _includedBeamSectionsVM,
                _includedLedgersVM,
                _includedVerticalsVM,
                _includedTubesVM,
                _includedSteelTypesVM,
                _includedPropsVM,
                _includedShoreBracingVM,
                _includedFrameTypeVM
            };
        }

        #endregion

        #region Methods

        public Validation<GeneticIncludedElements> IncludedElementsService(FormworkSystem formwork)
        {
            GeneticIncludedElements result = new GeneticIncludedElements(new List<PlywoodSectionName>(), new List<BeamSectionName>(), new List<double>(), new List<double>(), new List<double>(), new List<SteelType>(), new List<EuropeanPropTypeName>(), new List<double>(), new List<FrameTypeName>());

            var includedPlywoods = _includedPlywoodsVM.Plywoods.Where(p => p.IsSelected).Select(p => p.Plywood).ToList();
            if (includedPlywoods.Count <= 1)
                return LessThanTwoPlywood;
            var includedBeamSections = _includedBeamSectionsVM.BeamSections.Where(bs => bs.IsSelected).Select(bs => bs.BeamSection).ToList();
            if (includedBeamSections.Count <= 1)
                return LessThanTwoBeamSection;
            switch (formwork)
            {
                case FormworkSystem.CUPLOCK_SYSTEM:
                    var includedLedgers = _includedLedgersVM.Ledgers.Where(bs => bs.IsSelected).Select(bs => bs.Length).ToList();
                    if (includedLedgers.Count <= 1)
                        return LessThanTwoLedgers;
                    var includedVerticals = _includedVerticalsVM.Verticals.Where(bs => bs.IsSelected).Select(bs => bs.Length).ToList();
                    if (includedVerticals.Count <= 1)
                        return LessThanTwoVerticals;
                    var includedSteelTypes = _includedSteelTypesVM.SteelTypes.Where(type => type.IsSelected).Select(ts => ts.SteelType).ToList();
                    if (includedSteelTypes.Count < 1)
                        return ZeroSteelTypes;
                    var includedTubes = _includedTubesVM.Tubes.Where(t => t.IsSelected).Select(ts => ts.Length).ToList();
                    if (includedTubes.Count <= 1)
                        return LessThanTwoTubes;
                    result =  new GeneticIncludedElements(includedPlywoods, includedBeamSections, includedLedgers, includedVerticals, includedTubes, includedSteelTypes, new List<EuropeanPropTypeName>(), new List<double>(), new List<FrameTypeName>());

                    break;
                case FormworkSystem.EUROPEAN_PROPS_SYSTEM:
                    var includedProps = _includedPropsVM.Props.Where(ps => ps.IsSelected).Select(ps => ps.PropsType).ToList();
                    if (includedProps.Count <= 1)
                        return LessThanTwoProps;
                    result = new GeneticIncludedElements(includedPlywoods, includedBeamSections, new List<double>(), new List<double>(), new List<double>(), new List<SteelType>(), includedProps, new List<double>(), new List<FrameTypeName>());
                    break;
                case FormworkSystem.SHORE_SYSTEM:
                    var includedShoreBracing = _includedShoreBracingVM.Braces.Where(bs => bs.IsSelected).Select(bs => bs.Length).ToList();
                    if (includedShoreBracing.Count <= 1)
                        return LessThanTwoShoreBracing;
                    var includedFrameTypes = _includedFrameTypeVM.Frames.Where(fs => fs.IsSelected).Select(fs => fs.FrameType).ToList();
                    if (includedFrameTypes.Count <= 1)
                        return LessThanTwoFrameTypes;
                    result = new GeneticIncludedElements(includedPlywoods, includedBeamSections, new List<double>(), new List<double>(), new List<double>(), new List<SteelType>(), new List<EuropeanPropTypeName>(), includedShoreBracing, includedFrameTypes);
                    break;
            }

            return result;
           
        }

        #endregion

    }
}
