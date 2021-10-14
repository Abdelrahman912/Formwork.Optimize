using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticIncludedFrameTypesViewModel: GeneticIncludedBaseViewModel
    {
        #region Private Fields

        private bool _isSelectAllRows;

        #endregion

        #region Properties


        public List<FrameSelectionModel> Frames { get; }

        public bool IsSelectAllRows
        {
            get => _isSelectAllRows;
            set
            {
                var ischanged = NotifyPropertyChanged(ref _isSelectAllRows, value);
                if (ischanged)
                    Frames.ForEach(p => p.IsSelected = _isSelectAllRows);
            }
        }

        #endregion

        #region Constructors

        public GeneticIncludedFrameTypesViewModel()
            : base("Frame types")
        {
            Frames = Enum.GetValues(typeof(FrameTypeName)).Cast<FrameTypeName>().Select(e => new FrameSelectionModel(true,e)).ToList();
            IsSelectAllRows = true;
        }

        #endregion
    }
}
