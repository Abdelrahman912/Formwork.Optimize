using Autodesk.Revit.UI;
using FormworkOptimize.App.RevitCommands;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace FormworkOptimize.App
{
    class Main : IExternalApplication
    {
        #region Fields

        // The assembly of the project.
        static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        // The name of the tab to be added to Revit Ribbon.
        static readonly string tabName = "Formwork Optimize";

        #endregion

        #region Methods

        public Result OnShutdown(UIControlledApplication application)
        {
            // Return succeeded result.
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Create the tab.
                application.CreateRibbonTab(tabName);

                // Create the "element" panel.
                RibbonPanel mainPanel = application.CreateRibbonPanel(tabName, "Formwork Optimize");

                // Create the "element" button.
                PushButton designPushButton = CreateButton("designButton", "Design Formwork", typeof(FormworkDesignerCommand).FullName, "FormworkOptimize.App.UI.Resources.design.png", mainPanel, "Design formwork elements to support a slab or beam.");
                PushButton modelPushbutton = CreateButton("modelButton", "Model Formwork", typeof(FormworkModelingCommand).FullName, "FormworkOptimize.App.UI.Resources.model.png", mainPanel, "Model formwork geometry for a slab or beam.");
                PushButton geneticPushbutton = CreateButton("geneticButton", "Genetic Formwork", typeof(FormworkGeneticCommand).FullName, "FormworkOptimize.App.UI.Resources.genetic.png", mainPanel, "Optimize formwork for design or cost using Genetic Algorithms.");


            }
            catch (Exception e)
            {
                // Show the user an error message through task dialog.
                TaskDialog.Show("Error", e.Message);

                // Retun failed result.
                return Result.Failed;
            }
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolve);
            // Return succeeded result.
            return Result.Succeeded;
        }


        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            int position = args.Name.IndexOf(",");
            if (position > -1)
            {
                try
                {
                    string assemblyName = args.Name.Substring(0, position);
                    string assemblyFullPath = string.Empty;

                    //look in main folder
                    assemblyFullPath =  Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location) + "\\" + assemblyName + ".dll";
                    if (File.Exists(assemblyFullPath))
                        return Assembly.LoadFrom(assemblyFullPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return null;
        }

        /// <summary>
        /// Method to create the button with its required information,
        /// and add it to a certain panel.
        /// </summary>
        /// <param name="buttonName">The name of the button to be created.</param>
        /// <param name="buttonText">The text to be shown on the button in Revit UI.</param>
        /// <param name="className">The name of the class of the external command to be executed when clicking the button.</param>
        /// <param name="resource">The path of the image resource for the button.</param>
        /// <param name="panel">The panel where the button to be added.</param>
        /// <param name="description">The additional description to be shown as a tool tip for the button.</param>
        /// <returns>The push button which has been created.</returns>
        public PushButton CreateButton(string buttonName, string buttonText, string className, string imageResource, RibbonPanel panel, string description = null)
        {
            // Create the main information "data" about the button.
            PushButtonData buttonData = new PushButtonData(buttonName, buttonText, assembly.Location, className);

            // Create and add the button to the panel.
            PushButton pushButton = panel.AddItem(buttonData) as PushButton;

            // Add a tool tip description if the user sent it.
            if (description != null) pushButton.ToolTip = description;

            // Initialize the image.
            BitmapImage img = new BitmapImage();

            // Initialize a stream and assign the embedded resource to it.
            Stream stream = assembly.GetManifestResourceStream(imageResource);

            // Assign the resource stream to the image stream source.
            try
            {
                img.BeginInit();
                img.StreamSource = stream;
                img.EndInit();
            }
            catch (Exception e)
            {
                // Show an error message through task dialog.
                TaskDialog.Show("Create Button", e.Message);
            }

            // Assign the image to the large image of the button.
            pushButton.LargeImage = img;

            // Return the button that hase been created.
            return pushButton;
        }

        #endregion
    }
}
