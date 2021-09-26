using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Helpers.RevitHelper;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.Revit
{
    public class RevitConcreteFloor : RevitFloor
    {
        #region Properties

        public double Thickness { get;  }

        #endregion


        #region Constructors

        public RevitConcreteFloor(List<Line> boundary, List<List<Line>> openings , double thickness)
            :base(boundary, openings)
        {
            Thickness = thickness;
        }

        #endregion


    }
}
