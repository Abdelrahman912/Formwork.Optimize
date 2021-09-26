using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.DTOS.Revit.Internal
{
    public class ShoreCreation
    {

        #region Properties

        public Func<Line, XYZ, RevitShoreMain> MainFunc { get; }

        public Func<RevitShoreMain,double, List<RevitShoreBracing>> BracingFunc { get; }

        public Func<DeckingRectangle, Tuple<List<RevitBeam>, List<RevitBeam>>> MainSecBeamsFunc { get; }

        #endregion

        #region Constructors

        public ShoreCreation(Func<Line, XYZ, RevitShoreMain> mainFunc,
                               Func<RevitShoreMain,double, List<RevitShoreBracing>> bracingFunc,
                               Func<DeckingRectangle, Tuple<List<RevitBeam>, List<RevitBeam>>> mainSecBeamsFunc)
        {
            MainFunc = mainFunc;
            BracingFunc = bracingFunc;
            MainSecBeamsFunc = mainSecBeamsFunc;
        }

        #endregion

    }
}
