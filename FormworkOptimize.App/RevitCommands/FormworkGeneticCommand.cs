using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FormworkOptimize.App.RevitExternalEvents;
using FormworkOptimize.App.UI.Windows;
using FormworkOptimize.Core.Constants;
using System;
using System.IO;
using System.Reflection;

namespace FormworkOptimize.App.RevitCommands
{
    [Transaction(TransactionMode.Manual)]
    class FormworkGeneticCommand : IExternalCommand
    {

        #region Constructors

        public FormworkGeneticCommand()
        {
            ExternalEventHandler.Init();
        }
       
        #endregion

        #region Methods
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Assign the UIDocument, Document and Result to the RevitBase properties
            RevitBase.UIDocument = commandData.Application.ActiveUIDocument;
            RevitBase.Document = commandData.Application.ActiveUIDocument.Document;
            RevitBase.Result = Result.Succeeded;

            var _doc = RevitBase.Document;
            var _uiDoc = RevitBase.UIDocument;

            var revitWnd = GeneticFormworkWindow.Instance(_uiDoc);
            revitWnd.Show();


            //CuplockShoringHelper.CuplockFromModelLinesCommand(input);
            // Return the RevitBase result property
            return RevitBase.Result;
        }

        #endregion

    }
}
