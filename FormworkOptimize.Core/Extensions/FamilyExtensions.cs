using Autodesk.Revit.DB;

namespace FormworkOptimize.Core.Extensions
{
    public static class FamilyExtensions
    {

        /// <summary>
        /// Activate family if it is not active.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>Active Family Symbol</returns>
        public static FamilySymbol ActivateIfNot(this FamilySymbol symbol )
        {
            if (!symbol.IsActive)
                symbol.Activate();
            return symbol;
        }
            
    }
}
