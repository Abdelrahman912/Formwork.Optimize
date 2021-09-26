using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CSharp.Functional.Constructs;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.CostParameters;
using System;
using System.Collections.Generic;
using Unit = System.ValueTuple;

namespace FormworkOptimize.App.ViewModels
{
    public class GeneticRunFormworkViewModel:ViewModelBase
    {

        #region Private Fileds

        private GeneticDetailResultViewModel _geneticDetailResultVM;


        #endregion

        #region Properties

        public GeneticDetailResultViewModel GeneticDetailResultVM
        {
            get => _geneticDetailResultVM;
            set => NotifyPropertyChanged(ref _geneticDetailResultVM, value);
        }

        public GeneticOptionsViewModel  GeneticOptionsVM { get;  }

        public RevitFloorsViewModel FloorsVM { get;}

        #endregion

        #region Constructors

        public GeneticRunFormworkViewModel(UIDocument uiDoc,
                                        Func<List<ResultMessage>, Unit> notificationService,
                                        Func<double,double,Validation<CostParameter>> costParameterService)
                                       
        {
            FloorsVM = new RevitFloorsViewModel();
            GeneticOptionsVM = new GeneticOptionsViewModel(uiDoc,notificationService, costParameterService);
            FloorsVM.SelectedHostFloor = FloorsVM.SelectedHostFloor;
            FloorsVM.SelectedSupportedFloor = FloorsVM.SelectedSupportedFloor;
            Mediator.Instance.Subscribe<GeneticDetailResultViewModel>(this,(detailVM)=>GeneticDetailResultVM=detailVM,Context.GENETIC_DETAIL_RESULT);
        }

        #endregion

    }
}
