using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CSharp.Functional.Constructs;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.CostParameters;
using FormworkOptimize.Core.Entities.GeneticParameters;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unit = System.ValueTuple;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticRunFormworkViewModel:ViewModelBase
    {

        #region Private Fileds

        private object  _geneticResultVM;


        #endregion

        #region Properties

        public object GeneticResultVM
        {
            get => _geneticResultVM;
            set => NotifyPropertyChanged(ref _geneticResultVM, value);
        }

        public GeneticOptionsViewModel  GeneticOptionsVM { get;  }

        public RevitFloorsViewModel FloorsVM { get;}

        #endregion

        #region Constructors

        public GeneticRunFormworkViewModel(UIDocument uiDoc,
                                        Func<List<ResultMessage>, Unit> notificationService,
                                        Func<double,double,Validation<CostParameter>> costParameterService,
                                        Func<FormworkSystem,Validation<GeneticIncludedElements>> includedElemntsService,
                                        Func<Func<string, Task<List<Exceptional<string>>>>, Option<Task<List<Exceptional<string>>>>> folderDialogService)
                                       
        {
            FloorsVM = new RevitFloorsViewModel();
            GeneticOptionsVM = new GeneticOptionsViewModel(uiDoc,notificationService, costParameterService, includedElemntsService, folderDialogService);
            FloorsVM.SelectedHostFloor = FloorsVM.SelectedHostFloor;
            FloorsVM.SelectedSupportedFloor = FloorsVM.SelectedSupportedFloor;
            Mediator.Instance.Subscribe<GeneticDetailResultViewModel>(this,(detailVM)=>GeneticResultVM=detailVM,Context.GENETIC_DETAIL_RESULT);
            Mediator.Instance.Subscribe<GeneticHistoryViewModel>(this, (detailVM) => GeneticResultVM = detailVM, Context.GENETIC_DETAIL_RESULT);

        }

        #endregion

    }
}
