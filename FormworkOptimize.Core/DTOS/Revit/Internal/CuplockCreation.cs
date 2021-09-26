using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Geometry;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.DTOS.Internal
{
    public class CuplockCreation
    {
       

        #region Properties

        public Func<XYZ,XYZ,RevitCuplockVertical> VerticalFunc { get; }

        public Func<FormworkRectangle,List<RevitLedger>> LedgersFunc { get;}

        public Func<RevitLedger, RevitCuplockBracing> BracingFunc { get; }

        public Func<DeckingRectangle, Tuple<List<RevitBeam>, List<RevitBeam>>> MainSecBeamsFunc { get; }

        #endregion

        #region Constructors

        public CuplockCreation(Func<XYZ,XYZ, RevitCuplockVertical> verticalFunc, 
                               Func<FormworkRectangle, List<RevitLedger>> ledgersFunc, 
                               Func<RevitLedger, RevitCuplockBracing> bracingFunc, 
                               Func<DeckingRectangle, Tuple<List<RevitBeam>, List<RevitBeam>>> mainSecBeamsFunc)
        {
            VerticalFunc = verticalFunc;
            LedgersFunc = ledgersFunc;
            BracingFunc = bracingFunc;
            MainSecBeamsFunc = mainSecBeamsFunc;
        }

        #endregion

    }
}
