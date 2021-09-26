using CSharp.Functional.Constructs;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Interfaces;
using FormworkOptimize.Core.Entities.CostParameters;
using Unit = System.ValueTuple;
using static CSharp.Functional.Functional;
using static FormworkOptimize.Core.Errors.Errors;

namespace FormworkOptimize.App.ViewModels
{
    public class EquipmentsViewModel : ViewModelBase,IValidateViewModel
    {

        #region Private Fields

        private int _noCranes;

        private double _craneRent;

        #endregion

        #region Properties

        public int NoCranes
        {
            get => _noCranes;
            set => NotifyPropertyChanged(ref _noCranes, value);
        }

        public double CraneRent
        {
            get => _craneRent;
            set => NotifyPropertyChanged(ref _craneRent, value);
        }

        #endregion

        #region Constructors

        public EquipmentsViewModel()
        {
            NoCranes = 1;
            CraneRent = 4000;
        }

        #endregion

        #region Methods

        public Equipments GetEquipments()=>
             new Equipments(NoCranes, CraneRent);

        public Validation<Unit> Validate()
        {
            if (NoCranes < 0)
                return LessThanZeroError("Number of Cranes");
            if (CraneRent < 0)
                return LessThanZeroError("Crane Rent");
            return Unit();
        }

        #endregion

    }
}
