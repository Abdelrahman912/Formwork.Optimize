using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using System.Collections.Generic;
using  FormworkOptimize.Core.Helpers.RevitHelper;
using System;

namespace FormworkOptimize.Core.Entities.Revit
{
    public class RevitLineFloor:RevitFloor
    {

        #region Constructors

        public RevitLineFloor(List<Line> boundary, List<List<Line>> openings)
            :base(boundary, openings)
        {
            
        }

        #endregion

    }
}
