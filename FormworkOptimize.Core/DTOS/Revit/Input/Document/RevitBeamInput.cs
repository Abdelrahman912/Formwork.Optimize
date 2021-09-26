using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.Revit;
using System.Collections.Generic;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Document
{
    public class RevitBeamInput:RevitInput
    {

        #region Properties

        public List<ConcreteBeam> Beams { get;}

        /// <summary>
        /// Columns within Beams Bounding Rectangle.
        /// </summary>
        public List<ConcreteColumn> Columns { get;  }

        #endregion

        #region Constructors

        public RevitBeamInput(List<ConcreteBeam> beams, List<ConcreteColumn> columns, Level hostLevel, double hostFloorOffset)
            :base(hostLevel, hostFloorOffset)
        {
            Beams = beams;
            Columns = columns;
        }

        #endregion

    }
}
