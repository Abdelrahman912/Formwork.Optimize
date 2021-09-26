using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;

namespace FormworkOptimize.App.ViewModels
{
    public class TitleBarViewModel : ViewModelBase
    {
        #region Properties

        public RelayCommand HomeCommand { get; private set; }
        public RelayCommand CuplockSystemCommand { get; private set; }
        public RelayCommand FrameSystemCommand { get; private set; }
        public RelayCommand ShoreBaseSystemCommand { get; private set; }
        public RelayCommand EuropeanPropSystemCommand { get; private set; }
        public RelayCommand AluminumPropSystemCommand { get; private set; }
        //public RelayCommand TableSystemCommand { get; private set; }

        #endregion

        #region Constructors

        public TitleBarViewModel()
        {
            HomeCommand = new RelayCommand(OnHome);
            CuplockSystemCommand = new RelayCommand(OnCuplock);
            FrameSystemCommand = new RelayCommand(OnFrame);
            ShoreBaseSystemCommand = new RelayCommand(OnShoreBase);
            EuropeanPropSystemCommand = new RelayCommand(OnEuropeanProp);
            AluminumPropSystemCommand = new RelayCommand(OnAluminumProp);
            //TableSystemCommand = new RelayCommand(OnTable);
        }

        #endregion

        #region Methods

        private void OnHome()
        {
            Mediator.Instance.NotifyColleagues(new object(), Context.HOME_VIEW);
        }
        private void OnCuplock()
        {
            Mediator.Instance.NotifyColleagues(new object(), Context.CUPLOCK_SYSTEM_VIEW);
        }
        private void OnFrame()
        {
            Mediator.Instance.NotifyColleagues(new object(), Context.FRAME_SYSTEM_VIEW);
        }
        private void OnShoreBase()
        {
            Mediator.Instance.NotifyColleagues(new object(), Context.SHORE_BASE_SYSTEM_VIEW);
        }
        private void OnEuropeanProp()
        {
            Mediator.Instance.NotifyColleagues(new object(), Context.EUROPEAN_PROP_SYSTEM_VIEW);
        }
        private void OnAluminumProp()
        {
            Mediator.Instance.NotifyColleagues(new object(), Context.ALUMINUM_PROP_SYSTEM_VIEW);
        }
        //private void OnTable()
        //{
        //    Mediator.Instance.NotifyColleagues(new object(), Context.TABLE_SYSTEM_VIEW);
        //}

        #endregion
    }
}
