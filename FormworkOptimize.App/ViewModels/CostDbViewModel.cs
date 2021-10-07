using CSharp.Functional.Errors;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.Models.Enums;
using FormworkOptimize.App.UI.Services;
using FormworkOptimize.App.Utils;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unit = System.ValueTuple;

namespace FormworkOptimize.App.ViewModels
{
    public class CostDbViewModel:ViewModelBase
    {

        #region Private Fields

        private bool _isChanged;

        private List<FormworkElementCostModel> _table;

        private bool _isDbVisible;

        private readonly string _costFilePath;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<Exception, Unit> _showErrors;

        #endregion

        #region Properties

        public bool IsDbVisible
        {
            get => _isDbVisible;
            set => NotifyPropertyChanged(ref _isDbVisible,value);
        }

        public List<FormworkElementCostModel> Table
        {
            get => _table;
            set=>NotifyPropertyChanged(ref _table,value);
        }

        public ICommand SaveCommand { get; }

        public ICommand LoadCommand { get;  }

        public bool IsChanged
        {
            get => _isChanged;
            set => NotifyPropertyChanged(ref _isChanged,value);
        }

        #endregion

        #region Constructors

        public CostDbViewModel(Func<List<ResultMessage>, Unit> notificationService)
        {
            _notificationService = notificationService;
            _showErrors = exp => _notificationService(new List<ResultMessage>() { new ResultMessage(exp.Message,ResultMessageType.ERROR)});
            _costFilePath = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Cost Database\Formwork Elements Cost.json";
            SaveCommand = new RelayCommand(OnSave, CanSave);
            LoadCommand = new RelayCommand(OnLoad);
            Table = new List<FormworkElementCostModel>();
            IsDbVisible = true;
        }

        private async void OnLoad()
        {
            var validEles = await _costFilePath.ReadAsJsonList<FormworkElementCost>();
            validEles.Map(eles =>
            {
                Table = eles.Select(ele => ele.AsModel()).ToList();
                return Table;
            });
        }

        #endregion

        #region Methods

        private bool CanSave()
        {
            var result = Table.Any(model => model.Status == ModelStatus.MODIFIED) &&
                        !(Table.Where(model=>model.CostType==CostType.PURCHASE)
                             .Any(model=>model.NumberOfUses == 0));
            IsChanged = result;
            return result;
        }

        private async void OnSave()
        {
            Action<Unit> showResult=(_)  =>
            {
                var messages = new List<ResultMessage>()
                {
                    new ResultMessage("Cost Databse is Saved Successfully.",ResultMessageType.DONE)
                };
                _notificationService(messages);
                Table.ForEach(model => model.Status = ModelStatus.UPTODATE);
                IsChanged = false;  
            };

            IsDbVisible = false;
           var exceptionalDb =  await Table.Select(model=>model.AsElementCost()).ToList().WriteAsJson(_costFilePath);
            exceptionalDb.Match(_showErrors, showResult.ToFunc());   
            IsDbVisible = true;
        }

        #endregion

    }
}
