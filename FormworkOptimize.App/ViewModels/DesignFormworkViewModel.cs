using FormworkOptimize.App.ViewModels.Base;
using static FormworkOptimize.App.UI.Services.Services;


namespace FormworkOptimize.App.ViewModels
{
    public class DesignFormworkViewModel : ViewModelBase
    {

        #region Properties

        public CuplockSystemViewModel CuplockSystemViewModel { get; }
        public FrameSystemViewModel FrameSystemViewModel { get; }
        public ShoreBraceSystemViewModel ShoreBraceSystemViewModel { get; }
        public EuropeanPropSystemViewModel EuropeanPropSystemViewModel { get; }
        public AluminumPropSystemViewModel AluminumPropSystemViewModel { get; }

        #endregion

        #region Constructors

        public DesignFormworkViewModel()
        {
            CuplockSystemViewModel = new CuplockSystemViewModel(ResultMessagesService);
            FrameSystemViewModel = new FrameSystemViewModel(ResultMessagesService);
            ShoreBraceSystemViewModel = new ShoreBraceSystemViewModel(ResultMessagesService);
            EuropeanPropSystemViewModel = new EuropeanPropSystemViewModel(ResultMessagesService);
            AluminumPropSystemViewModel = new AluminumPropSystemViewModel(ResultMessagesService);
        }

        #endregion
    }
}
