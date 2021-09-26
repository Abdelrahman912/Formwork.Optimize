using Autodesk.Revit.DB;
using CSharp.Functional.Constructs;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Interfaces;
using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Entities.CostParameters;
using FormworkOptimize.Core.Entities.CostParameters.Interfaces;
using Unit = System.ValueTuple;
using static CSharp.Functional.Functional;
using static FormworkOptimize.Core.Errors.Errors;

namespace FormworkOptimize.App.ViewModels
{
    public class TimeParametersViewModel : ViewModelBase,IValidateViewModel
    {

        #region Private Fields

        private int _smitheryTime;

        private bool _isAutomaticWaitingTime;

        private int _userDefinedWaitingTime;

        private bool _isAutomaticRemovalTime;

        private int _userDefinedRemovalTime;

        #endregion

        #region Properties

        public int UserDefinedRemovalTime
        {
            get => _userDefinedRemovalTime;
            set => NotifyPropertyChanged(ref _userDefinedRemovalTime, value);
        }

        public bool IsAutomaticRemovalTime
        {
            get => _isAutomaticRemovalTime;
            set => NotifyPropertyChanged(ref _isAutomaticRemovalTime, value);
        }

        public int UserDefinedWaitingTime
        {
            get => _userDefinedWaitingTime;
            set => NotifyPropertyChanged(ref _userDefinedWaitingTime, value);
        }

        public bool IsAutomaticWaitingTime
        {
            get => _isAutomaticWaitingTime;
            set => NotifyPropertyChanged(ref _isAutomaticWaitingTime, value);
        }

        public int SmitheryTime
        {
            get => _smitheryTime;
            set => NotifyPropertyChanged(ref _smitheryTime, value);
        }

        public string WaitingTimeInfo { get;  }

        public string RemovalTimeInfo { get;  }

        #endregion

        #region Constructors

        public TimeParametersViewModel()
        {
            SmitheryTime = 8;
            IsAutomaticWaitingTime = true;
            UserDefinedWaitingTime = 2;
            IsAutomaticRemovalTime = true;
            UserDefinedRemovalTime = 2;
            WaitingTimeInfo = "Automatic Option: (2*L+2) , L is the Shortest Span.";
            RemovalTimeInfo = "Automatic Option: 0.5 * Installation Time.";
        }

        #endregion

        #region Methods

        public Time GetTime(double smallerLength,ICalculateInstallationTime installTime)
        {
            var waitingTime = IsAutomaticWaitingTime ? new AutomaticWaitingTime(smallerLength) as ICalculateWaitingTime : new UserDefinedWaitingTime(UserDefinedWaitingTime);
            var removalTime = IsAutomaticRemovalTime ? new AutomaticRemovalTime(installTime) as ICalculateRemovalTime :new UserDefinedRemovalTime(UserDefinedRemovalTime);

            return new Time(SmitheryTime, waitingTime, removalTime, installTime);
        }

        public Validation<Unit> Validate()
        {
            if (SmitheryTime < 0)
                return LessThanZeroError("Smithery Time");
            if (!IsAutomaticWaitingTime && UserDefinedWaitingTime < 0)
                return LessThanZeroError("Waiting Time before Formwork Removal");
            if (!IsAutomaticRemovalTime && UserDefinedRemovalTime < 0)
                return LessThanZeroError("Formwork Removal Time");
            return Unit();
        }

        #endregion

    }
}
