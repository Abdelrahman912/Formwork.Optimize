using FormworkOptimize.App.Utils;
using FormworkOptimize.App.ViewModels.Base;
using System;
using System.IO;
using System.Reflection;

namespace FormworkOptimize.App.ViewModels
{
    public class ExcelCostFileViewModel : ViewModelBase
    {
        #region Private Fields

        private string _excelFilePath;

        #endregion

        #region Properties

        public string ExcelFilePath
        {
            get => _excelFilePath;
            set => NotifyPropertyChanged(ref _excelFilePath, value);
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public RelayCommand OpenExcelFile { get; }

        #endregion

        #region Constructors

        public ExcelCostFileViewModel()
        {
            //Assembly.GetExecutingAssembly().FullName;
            //ExcelFilePath = @"C:\ProgramData\Autodesk\Revit\Addins\2020\FormworkOptimize\Cost Database\Formwork Elements Cost.csv";
            ExcelFilePath = string.Concat(AssemblyDirectory, @"\Cost Database\Formwork Elements Cost.csv");
            OpenExcelFile = new RelayCommand(OnOpen, CanOpen);
        }

        #endregion

        #region Methods
        private bool CanOpen()
        {
            return true;
        }

        private void OnOpen()
        {
            if (File.Exists(ExcelFilePath) && !new FileInfo(ExcelFilePath).IsFileinUse())
            {
                System.Diagnostics.Process.Start(ExcelFilePath);

                //var excel = new Excel.Application();
                //var wb = excel.Workbooks.Open(ExcelFilePath);
            }
        }

        #endregion

    }
}
