using CSharp.Functional.Constructs;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.UI.Windows;
using FormworkOptimize.App.ViewModels;
using FormworkOptimize.App.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static CSharp.Functional.Extensions.OptionExtension;
using static CSharp.Functional.Functional;
using Unit = System.ValueTuple;

namespace FormworkOptimize.App.UI.Services
{
    public enum ResultMessageType
    {
        DONE,
        ERROR,
        WARNING
    }

    public static class Services
    {
        public static DesignResultDialog DesignResultService(DesignResultViewModel msg)
        {
            var wnd = new DesignFormworkResultWindow();
            var viewModel = new DesignFormworkResultViewModel(msg);
            viewModel.CloseEvent += () => wnd.Close();
            wnd.DataContext = viewModel;
            wnd.ShowDialog();
            return viewModel.ResultDialog;
        }

        public static Unit ResultMessagesService(List<ResultMessage> messages)
        {
            var wnd = new ResultMessagesWindow();
            wnd.DataContext = messages;
            wnd.ShowDialog();
            return Unit();
        }

        //public static Unit ResultMessageService(ResultMessage message)
        //{
        //    var wnd = new ResultMessageWindow();
        //    wnd.DataContext = message;
        //    wnd.ShowDialog();
        //    return Unit();
        //}

        public static Option<T> FolderDialogService<T>(Func<string,T> onOk)
        {
            var outputDialog = new FolderBrowserDialog();

            outputDialog.Description = "Choose Output Directory";
            if (outputDialog.ShowDialog() == DialogResult.OK)
              return Some(onOk(outputDialog.SelectedPath));
            else
                return None;
        }

    }
}
