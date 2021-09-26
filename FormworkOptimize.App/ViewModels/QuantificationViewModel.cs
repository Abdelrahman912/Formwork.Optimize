using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CSharp.Functional.Constructs;
using CSharp.Functional.Errors;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.UI.Services;
using FormworkOptimize.App.Utils;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.Quantification;
using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static FormworkOptimize.App.Utils.Memoization;
using Unit = System.ValueTuple;
using static CSharp.Functional.Functional;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.App.ViewModels
{
    public class QuantificationViewModel : ViewModelBase
    {

        #region Private Fields

        private readonly Document _doc;

        private readonly UIDocument _uiDoc;

        private ElementQuantification _selectedRow;

        private readonly Func<LevelKey, List<ElementQuantification>> _cachedQuery;

        private readonly Func<Func<string, Task<List<Exceptional<string>>>>,  Option<Task<List<Exceptional<string>>>>> _folderDialogService;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        #endregion

        #region Properties

        public ICommand SelectQuantElementCommand { get; }

        public ICommand ExportCommand { get; }

        public ICommand CostCommand { get;}

        public ElementQuantification SelectedRow
        {
            get => _selectedRow;
            set => NotifyPropertyChanged(ref _selectedRow, value);
        }

        public List<LevelViewModel> Levels { get; }

        public ObservableCollection<ElementQuantification> Table { get; }

        #endregion

        #region Constructors

        public QuantificationViewModel(Func<Func<string, Task<List<Exceptional<string>>>>, Option<Task<List<Exceptional<string>>>>> folderDialogService,
                                       Func<List<ResultMessage>,Unit> notificationService)
        {
            _doc = RevitBase.Document;
            _uiDoc = RevitBase.UIDocument;
            _folderDialogService = folderDialogService;
            _notificationService = notificationService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());

            Table = new ObservableCollection<ElementQuantification>();
            Mediator.Instance.Subscribe<LevelViewModel>(this, OnLevelSelected, Context.LEVEL_SELECTED);
            Levels = new FilteredElementCollector(_doc).OfClass(typeof(Level))
                                                       .OfType<Level>()
                                                       .Select(l => new LevelViewModel(false, l.Name, l))
                                                       .ToList();
            SelectQuantElementCommand = new RelayCommand(OnSelectQuantElement, CanSelectQuantElement);
            ExportCommand = new RelayCommand(OnExport, CanExport);
            CostCommand = new RelayCommand(OnCost);
            _cachedQuery = Memoization.MemoizeWeak<LevelKey, List<ElementQuantification>>(lkey => _doc.Query(lkey.Level), TimeSpan.FromMinutes(2));
        }


        #endregion

        #region Methods

        private void OnCost()
        {
            var dir = @"C:\ProgramData\Autodesk\Revit\Addins\2020\FormworkOptimize\Cost Database\";
            var fileName = $"{dir}{FORMWORK_ELEMENT_COST_FILE}";
            var result = fileName.ReadAsCsv<FormworkElementCost>()
                                 .Match(_showErrors,s=>Unit() );
        }

        private bool CanExport() =>
            Levels.Any(lvm => lvm.IsSelected);


        private  void OnExport()
        {
            Func<string, Task<List<Exceptional<string>>>> exportFunc = async (dir) =>
              {
                  var tasks = await Task.Run(() =>
                  {
                      return Table.ToLookup(r => r.Level.Id.IntegerValue)
                                   .Select(kvp => Tuple.Create(kvp.ToList().First().Level.Name, kvp.ToList().Select(row => new ElementQuantificationOutput(row.Name, row.Count))))
                                   .AsParallel()
                                   .Select(os => os.Item2.WriteAsCsv(dir, $"{_doc.Title} - {os.Item1}"))
                                   .ToList();
                  });
                  var results = await Task.WhenAll(tasks);
                  return results.ToList();
              };

            Func<Task<List<Exceptional<string>>>, Task> showResult =async  (task) =>
             {
                 var results = await task;
                 var messages = results.Select(fileName =>fileName.ToResult() )
                                      .ToList();      
                 _notificationService(messages);
             };

             _folderDialogService(exportFunc)
               .Map(showResult);
        }


        private bool CanSelectQuantElement() =>
           SelectedRow != null;


        private void OnSelectQuantElement() =>
            _uiDoc.Selection.SetElementIds(SelectedRow.Elements.ToList());


        private void OnLevelSelected(LevelViewModel level)
        {
            if (level.IsSelected)
            {
                _cachedQuery(new LevelKey(level.Level))
                    .ForEach(eq => Table.Add(eq));
            }
            else
            {
                Table.Where(eq => eq.Level.Id.IntegerValue == level.Level.Id.IntegerValue)
                     .ToList()
                     .ForEach(eq => Table.Remove(eq));
            }
        }


        #endregion



    }
}
