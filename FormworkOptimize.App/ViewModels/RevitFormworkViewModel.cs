using FormworkOptimize.App.ViewModels.Base;
using System.Collections.Generic;
using static FormworkOptimize.App.UI.Services.Services;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitFormworkViewModel:ViewModelBase
    {

        public QuantificationViewModel QuantificationVM { get; }

        public RevitFloorFormworkViewModel FloorVM { get;  }

        public RevitBeamFormworkViewModel BeamVM { get;}

        public RevitColumnFormworkViewModel ColumnVM { get;}

        public RevitPlywoodViewModel PlywoodVM { get; }

        public RevitFormworkViewModel()
        {
            var floorsVM = new RevitFloorsViewModel();
            FloorVM = new RevitFloorFormworkViewModel(floorsVM, ResultMessagesService,DesignResultService);
            BeamVM = new RevitBeamFormworkViewModel(floorsVM, ResultMessagesService, DesignResultService);
            ColumnVM = new RevitColumnFormworkViewModel(floorsVM, ResultMessagesService);
            PlywoodVM = new RevitPlywoodViewModel(floorsVM, ResultMessagesService);
            QuantificationVM = new QuantificationViewModel(FolderDialogService,ResultMessagesService);
        }

    }
}
