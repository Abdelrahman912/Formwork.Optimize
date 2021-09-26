using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.Revit
{
    public abstract class RevitFloor
    {
        #region Properties

        public List<Line> Boundary { get; }

        public List<List<Line>> Openings { get; }

        #endregion


        #region Constructors

        public RevitFloor(List<Line> boundary, List<List<Line>> openings )
        {
            Boundary = boundary;
            Openings = openings;
        }

        #endregion

    }
}
