using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticIncludedBeamSectionsViewModel:GeneticIncludedBaseViewModel
    {

        #region Private Fields

        private bool _isSelectAllRows;

        #endregion

        #region Properties

        public List<BeamSectionSelectionModel> BeamSections { get; }

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
                var ischanged = NotifyPropertyChanged(ref _isSelectAllRows, value);
                if (ischanged)
                    BeamSections.ForEach(p => p.IsSelected = _isSelectAllRows);
            }
        }

        #endregion

        #region Constructors

        public GeneticIncludedBeamSectionsViewModel()
            :base("Beams Sections")
        {
            BeamSections = Enum.GetValues(typeof(BeamSectionName))
                               .Cast<BeamSectionName>()
                               .Select(value=>new BeamSectionSelectionModel(true,value))
                               .ToList();
            IsSelectAllRows = true;
        }

        #endregion

    }
}
