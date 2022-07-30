using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FormworkOptimize.App.UI.Windows;
using FormworkOptimize.Core.Constants;
using System;

namespace FormworkOptimize.App.RevitCommands
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class FormworkDesignerCommand : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Assign the UIDocument, Document and Result to the RevitBase properties
            RevitBase.UIDocument = commandData.Application.ActiveUIDocument;
            RevitBase.Document = commandData.Application.ActiveUIDocument.Document;
            RevitBase.Result = Result.Succeeded;

           
            // Initialize the MainWindow "View" and MainViewModel "View model"
            DesignFormworkWindow mainWindow = DesignFormworkWindow.Instance;

            // Initialize the FormworkDesignWindow "View" and FormworkDesignVM "View model"
            //DesignFormworkWindow designFormworkWindow = DesignFormworkWindow.Instance;
            //DesignFormworkVM designFormworkVM = new DesignFormworkVM();

            // Register the event that is responsible for closing the window to select the walls
            //designFormworkVM.SelectButtonClicked += (sender, args) =>
            //{
            //    designFormworkWindow.Close();
            //};

            // Assign the FormworkDesignVM "View model" to the FormworkDesignWindow "View"
            //designFormworkWindow.DataContext = designFormworkVM;

            // Show the MainWindow"view"
            mainWindow.Show();

            // Show the FloorType window "view"
            //designFormworkWindow.Show();

            // Assign the RevitBase message property to the ref error message
            message = RevitBase.Message;

            // Return the RevitBase result property
            return RevitBase.Result;
        }

        private void importReplacement(object sender, Autodesk.Revit.UI.Events.ExecutedEventArgs arg)
        {
            TaskDialog.Show("Stop!", "Do not import!");
        }
    }

}
