using CSharp.Functional.Constructs;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Interfaces;
using FormworkOptimize.Core.Entities.CostParameters;
using FormworkOptimize.Core.Entities.CostParameters.Interfaces;
using FormworkOptimize.Core.Enums;
using System.Collections.Generic;
using System.Linq;
using Unit = System.ValueTuple;
using static FormworkOptimize.Core.Errors.Errors;
using CSharp.Functional;
using static CSharp.Functional.Functional;

namespace FormworkOptimize.App.ViewModels
{
    public class ManPowerViewModel : ViewModelBase , IValidateViewModel
    {
        #region Private Fields

        private SystemProductivityModel _selectedSystemProductivity;

        private double _laborCost;

        private bool _isAutomaticNoWorkers;

        private int _userDefinedNoWorkers;

        #endregion

        #region Properties

        public bool IsAutomaticNoWorkers
        {
            get => _isAutomaticNoWorkers;
            set => NotifyPropertyChanged(ref _isAutomaticNoWorkers, value);
        }

        public int UserDefinedNoWorkers
        {
            get => _userDefinedNoWorkers;
            set=>NotifyPropertyChanged(ref _userDefinedNoWorkers, value);
        }

        public double LaborCost
        {
            get => _laborCost;
            set => NotifyPropertyChanged(ref _laborCost, value);
        }

        public SystemProductivityModel SelectedSystemProductivity
        {
            get => _selectedSystemProductivity;
            set => NotifyPropertyChanged(ref _selectedSystemProductivity, value);
        }

        public List<SystemProductivityModel> SystemsProductivity { get; }


        public string NoWorkersInfo { get;  }

        #endregion

        #region Constructors

        public ManPowerViewModel()
        {
            SystemsProductivity = new List<SystemProductivityModel>()
            {
                new SystemProductivityModel(15,FormworkSystem.CUPLOCK_SYSTEM),
                new SystemProductivityModel(20,FormworkSystem.EUROPEAN_PROPS_SYSTEM),
                new SystemProductivityModel(10,FormworkSystem.SHORE_SYSTEM)
            };

            SelectedSystemProductivity = SystemsProductivity.First();
            LaborCost = 200;//(LE/Day)
            IsAutomaticNoWorkers = true;
            UserDefinedNoWorkers = 3;//min
            NoWorkersInfo = "Automatic Option: For every 1000 m^2 -> 20 workers and minimum are 4 workers.";
        }

        #endregion

        #region Methods

        public ManPower GetManPower(double floorArea)
        {
            var productivity = SelectedSystemProductivity.Productivity;
            var laborCost = LaborCost;
            var noWorkers = IsAutomaticNoWorkers ? new AutomaticNoWorkers(floorArea) as ICalculateNoWorkers :new UserDefinedNoWorkers(UserDefinedNoWorkers);
            return new ManPower(productivity, laborCost, floorArea, noWorkers);
        }

        public Validation<Unit> Validate()
        {
            if (SystemsProductivity.Any(s=>s.Productivity <= 0))
                return EqualZeroError("Manpower Productivity");
            if (LaborCost < 0)
                return LessThanZeroError("Labor Cost");
            if (!IsAutomaticNoWorkers && UserDefinedNoWorkers <= 0)
                return EqualZeroError("Number of Workers");
            return Unit();
        }

        #endregion

    }
}
