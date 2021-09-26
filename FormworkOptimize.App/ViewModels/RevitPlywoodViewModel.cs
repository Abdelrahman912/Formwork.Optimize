using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CSharp.Functional.Errors;
using CSharp.Functional.Extensions;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.RevitExternalEvents;
using FormworkOptimize.App.Utils;
using FormworkOptimize.App.ViewModels.Base;
using FormworkOptimize.App.ViewModels.Enums;
using FormworkOptimize.App.ViewModels.Mediators;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Revit;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.RevitHelper;
using FormworkOptimize.Core.SelectionFilters;
using FormworkOptimize.Core.SuppressWarnings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using static CSharp.Functional.Functional;
using Unit = System.ValueTuple;

namespace FormworkOptimize.App.ViewModels
{
    public class RevitPlywoodViewModel : ViewModelBase
    {

        #region Private Fields

        private readonly Document _doc;

        private readonly UIDocument _uiDoc;

        private PlywoodSectionName _selectedPlywoodSection;

        private double _plywoodWidth;

        private Floor _selectedHostFloor;

        private Floor _selectedSupportedFloor;

        private bool _isModelFromFloor;

        private readonly Func<List<ResultMessage>, Unit> _notificationService;

        private readonly Func<IEnumerable<Error>, Unit> _showErrors;

        #endregion

        #region Properties

        public bool IsModelFromFloor
        {
            get => _isModelFromFloor;
            set => NotifyPropertyChanged(ref _isModelFromFloor, value);
        }

        public ICommand ModelPlywoodForFloorCommand { get; }

        public ICommand ModelPlywoodForBeamCommand { get; }

        public double PlywoodWidth
        {
            get => _plywoodWidth;
            set => NotifyPropertyChanged(ref _plywoodWidth, value);
        }

        public PlywoodSectionName SelectedPlywoodSection
        {
            get => _selectedPlywoodSection;
            set
            {
                NotifyPropertyChanged(ref _selectedPlywoodSection, value);
                Mediator.Instance.NotifyColleagues(_selectedPlywoodSection, Context.PLYWOOD_SECTION);
            }
        }

        public RevitFloorsViewModel FloorsVM { get; }

        #endregion

        #region Constructors

        public RevitPlywoodViewModel(RevitFloorsViewModel floorsVM,
                                    Func<List<ResultMessage>, Unit> notificationService)
        {
            _doc = RevitBase.Document;
            _uiDoc = RevitBase.UIDocument;
            _notificationService = notificationService;
            _showErrors = errors => _notificationService(errors.Select(err => err.ToResult()).ToList());
            FloorsVM = floorsVM;
            _selectedHostFloor = FloorsVM.SelectedHostFloor?.Floor;
            _selectedSupportedFloor = FloorsVM.SelectedSupportedFloor?.Floor;

            IsModelFromFloor = true;

            SelectedPlywoodSection = PlywoodSectionName.BETOFILM_18MM;
            PlywoodWidth = 50;
            Mediator.Instance.Subscribe<Floor>(this, (host) => _selectedHostFloor = host, Context.HOST_FLOOR);
            Mediator.Instance.Subscribe<Floor>(this, (supported) => _selectedSupportedFloor = supported, Context.SUPPORTED_FLOOR);
            ModelPlywoodForFloorCommand = new RelayCommand(OnModelForFloor, CanPlywoodFromFloor);
            ModelPlywoodForBeamCommand = new RelayCommand(() => OnModelingPlywood(OnPlywoodFromBeams), CanPlywoodFromBeams);
        }

        #endregion


        #region Methods

        private bool CanPlywoodFromFloor() =>
          _selectedSupportedFloor != null && _selectedHostFloor != null;


        private bool CanPlywoodFromBeams() =>
            PlywoodWidth > 0;


        private void OnModelForFloor()
        {
            if (IsModelFromFloor)
                OnModelingPlywood(OnPlywoodFromFloor);
            else
                OnModelingPlywood(OnPlywoodFromLines);
        }

        private void OnModelingPlywood(Action modelingFunc)
        {
            Action executeEvent = () =>
            {
                var result = from floorTypes in _doc.UsingTransaction(_ => _doc.AddPlywoodElementTypesIfNotExist<FloorType>(), "Add Plywood Floor Types")
                             from wallTypes in _doc.UsingTransaction(_ => _doc.AddPlywoodElementTypesIfNotExist<WallType>(), "Add Plywood Wall Types")
                             select modelingFunc.ToFunc()();
                result.Match(_showErrors, _ => Unit());

            };
            ExternalEventHandler.Instance.Raise(_ => _doc.UsingTransactionGroup(transGroup => executeEvent(), "Plywood Model"),
                                                    () => { },
                                                    (err) => TaskDialog.Show("Error", err));
        }


        private void OnPlywoodFromBeams()
        {

            Func<IEnumerable<Element>, Unit> modelingFunc = beams =>
            {
                if (beams.Count() == 0)
                    return Unit();
                var clearHeight = _selectedHostFloor.GetClearHeight(beams.First());
                var concreteBeams = beams.Select(beam => beam.ToConcreteBeam(_doc, _selectedHostFloor.GetClearHeight(beam)))
                                           .ToList();
                var rectangles = concreteBeams.ToRectangles(PlywoodWidth.CmToFeet());

                var floorThickness = _selectedSupportedFloor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();

                concreteBeams.Zip(rectangles, (b, rec) => Tuple.Create(b, rec))
                             .ToList()
                             .ForEach(tuple =>
                             {
                                 (var beam, var rect) = tuple;
                                 var plywoodOpeningsAction = _doc.UsingTransaction(trans =>
                                 {
                                     trans.SuppressWarning(SuppressWarningsHelper.FloorsOverlapSuppress);
                                     return beam.GetPlywood(rect.Lines, _selectedHostFloor, clearHeight, SelectedPlywoodSection, floorThickness, _doc)
                                                  .DrawBoundary(_doc);
                                 }, "Draw Plywood Boundary");
                                 _doc.UsingTransaction(_ => plywoodOpeningsAction(), "Plywood Openings");

                             });
                return Unit();
            };
            Func<IEnumerable<Element>, Unit> executeEvent = (beams) =>
            {
                var result = from floorTypes in _doc.UsingTransaction(_ => _doc.AddPlywoodElementTypesIfNotExist<FloorType>(), "Add Plywood Floor Types")
                             from wallTypes in _doc.UsingTransaction(_ => _doc.AddPlywoodElementTypesIfNotExist<WallType>(), "Add Plywood Wall Types")
                             select modelingFunc(beams);
                result.Match(_showErrors, _ => Unit());
                return Unit();
            };

            var res = _uiDoc.PickElements(Filters.BeamFilter, "Select Concrete Beams")
                                .Map(beams => executeEvent(beams));
        }

        private void OnPlywoodFromLines()
        {
            Action<RevitLinePlywood> draw = floorPlywood =>
            {
                var openingsDrawFunc = _doc.UsingTransaction(trans =>
                {
                    trans.SuppressWarning(SuppressWarningsHelper.FloorsOverlapSuppress);
                    return floorPlywood.DrawBoundary(_doc);
                }, "Draw Plywood Boundary");
                _doc.UsingTransaction(_ => openingsDrawFunc(), "Draw Plywood Openings");
            };

            var validation = from valid in _uiDoc.GetRevitFloorFromDetailLines()
                             select valid.Bind(lineFloor=>_doc.GetDefault3DView()
                                         .Bind(view => DeckingHelper.GetPlywood(lineFloor, _selectedHostFloor, _selectedSupportedFloor, SelectedPlywoodSection, _doc, view.Options())))
                                         .Match(_showErrors, draw.ToFunc());
        }

        private void OnPlywoodFromFloor()
        {

            Action<RevitFloorPlywood> draw = floorPlywood =>
            {
                var openingsDrawFunc = _doc.UsingTransaction(trans =>
                {
                    trans.SuppressWarning(SuppressWarningsHelper.FloorsOverlapSuppress);
                    return floorPlywood.DrawBoundary(_doc);

                }, "Draw Plywood Boundary");
                _doc.UsingTransaction(_ => openingsDrawFunc(), "Draw Plywood Openings");
            };

            var plywoodValidation = from view in _doc.GetDefault3DView()
                                    select DeckingHelper.GetPlywood(_selectedSupportedFloor.GetGeometry(view.Options()), _selectedHostFloor, _selectedSupportedFloor, SelectedPlywoodSection, _doc, view.Options());

            plywoodValidation.Match(_showErrors, draw.ToFunc());

        }

        #endregion

    }
}
