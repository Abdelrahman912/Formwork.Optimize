using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using System;
using System.Windows.Input;

namespace FormworkOptimize.App.ViewModels
{
    public class DesignFormworkResultViewModel : ViewModelBase
    {

        #region Private Fields

        private DesignResultDialog _resultDialog;

        #endregion

        #region Events

        public event Action CloseEvent = delegate {  };

        #endregion

        #region Properties

        public DesignResultDialog ResultDialog => _resultDialog;

        public DesignResultViewModel DesignResultVM { get; }

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        #endregion

        #region Constructors

        public DesignFormworkResultViewModel(DesignResultViewModel designResultVM)
        {
            DesignResultVM = designResultVM;
            AcceptCommand = new RelayCommand(OnAccept);
            CancelCommand = new RelayCommand(OnCancel);
            _resultDialog = DesignResultDialog.CANCEL;
        }

        #endregion

        #region Methods

        private void OnCancel()
        {
            _resultDialog = DesignResultDialog.CANCEL;
            CloseEvent();
        }

        private void OnAccept()
        {
            _resultDialog = DesignResultDialog.ACCEPT;
            CloseEvent();
        }
       


        #endregion

    }
}
