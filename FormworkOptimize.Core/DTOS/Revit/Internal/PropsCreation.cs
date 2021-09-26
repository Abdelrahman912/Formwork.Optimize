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
    public class PropsCreation
    {

        #region Properties

        public Func<XYZ, XYZ, RevitPropsVertical> VerticalFunc { get; }

        public Func<XYZ, RevitPropsLeg> LegFunc { get; }

        public Func<DeckingRectangle, Tuple<List<RevitBeam>, List<RevitBeam>>> MainSecBeamsFunc { get; }

        #endregion

        #region Constructors

        public PropsCreation(Func<XYZ, XYZ, RevitPropsVertical> verticalFunc,
                               Func<XYZ, RevitPropsLeg> legFunc,
                               Func<DeckingRectangle, Tuple<List<RevitBeam>, List<RevitBeam>>> mainSecBeamsFunc)
        {
            VerticalFunc = verticalFunc;
            LegFunc = legFunc;
            MainSecBeamsFunc = mainSecBeamsFunc;
        }

        #endregion

    }
}
