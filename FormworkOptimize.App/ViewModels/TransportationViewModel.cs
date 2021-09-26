using CSharp.Functional.Constructs;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Interfaces;
using FormworkOptimize.Core.Entities.CostParameters;
using Unit = System.ValueTuple;
using static CSharp.Functional.Functional;
using static FormworkOptimize.Core.Errors.Errors;


namespace FormworkOptimize.App.ViewModels
{
    public class TransportationViewModel:ViewModelBase,IValidateViewModel
    {

        #region Private Fileds

        private bool _isIncluded;

        private double _userDefinedTransportation;

        #endregion

        #region Properties

        public bool IsIncluded
        {
            get => _isIncluded;
            set => NotifyPropertyChanged(ref  _isIncluded,value);
        }

        public double UserDefinedTransportation
        {
            get => _userDefinedTransportation;
            set => NotifyPropertyChanged(ref _userDefinedTransportation,value);
        }

        public string TransportationCostInfo { get; }

        #endregion

        #region Constructors

        public TransportationViewModel()
        {
            IsIncluded = true;
            UserDefinedTransportation = 200;
            TransportationCostInfo = "Included Option: Transportation cost is already added to the cost of Formwork elements in the excel sheet.";
        }

        #endregion

        #region Methods

        public Transportation GetTransportation()
        {
            return IsIncluded ? new IncludedFormworkTransportation() : new UserDefinedTransportation(UserDefinedTransportation) as Transportation;
        }

        public Validation<Unit> Validate()
        {
            if (!IsIncluded && UserDefinedTransportation < 0)
                return LessThanZeroError("Transportation Cost");
            return Unit();
        }

        #endregion

    }
}
