using Autodesk.Revit.UI;
using System;

namespace FormworkOptimize.App.RevitExternalEvents
{
    public class ExternalEventHandler : IExternalEventHandler
    {

        #region Private Fields


        private static ExternalEvent _externalEvent;

        private Action<UIApplication> _execute;

        #endregion

        #region Properties

        public static ExternalEventHandler Instance;

        #endregion

        #region Constructors

        private ExternalEventHandler() { }
       

        #endregion

        #region Methods

        public static void Init()
        {
            if (Instance != null)
                return;
            Instance = new ExternalEventHandler();
            _externalEvent = ExternalEvent.Create(Instance);
        }

        public void Execute(UIApplication app) =>
            _execute?.Invoke(app);
        

        public string GetName() =>
            "External Event";
        


        public  void Raise(Action<UIApplication> execute,Action OnSuccess , Action<string> OnFail)
        {
            _execute = app =>
            {
                try
                {
                    execute?.Invoke(app);
                    OnSuccess.Invoke();
                }
                catch (Exception e)
                {
                    OnFail?.Invoke(e.Message);
                }
            };
            _externalEvent.Raise();
        }

        #endregion

    }
}
