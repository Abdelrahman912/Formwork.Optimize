using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;

namespace FormworkOptimize.Core.SelectionFilters
{
    public class GenericSelectionFilter : ISelectionFilter
    {

        #region Private Fields

        private readonly Func<Element, bool> _allowElement;

        #endregion

        #region Constructors

        public GenericSelectionFilter(Func<Element, bool> allowElement)
        {
            _allowElement = allowElement ?? ((_)=>true);
        }

        #endregion

        #region Methods

        #endregion
        public bool AllowElement(Element elem) =>
            _allowElement.Invoke(elem);


        public bool AllowReference(Reference reference, XYZ position) =>
            false;
       
    }
}
