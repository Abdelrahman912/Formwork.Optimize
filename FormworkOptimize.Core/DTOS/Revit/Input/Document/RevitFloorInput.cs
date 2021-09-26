using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Revit;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Document
{
    public class RevitFloorInput:RevitInput
    {
        #region Properties

        public  List<Tuple<double,ConcreteColumn>> Columns { get;  }

        public List<ConcreteBeam> Beams { get; }

        public RevitFloor ConcreteFloor { get; }

        public double  FloorClearHeight { get;  }

        public XYZ MainBeamDir { get; }

        public Func<List<RevitBeam>, double,List<RevitBeam>> AdjustLayout { get;  }

        #endregion

        #region Constructors

        public RevitFloorInput(RevitFloor concreteFloor,
                               List<Tuple<double,
                               ConcreteColumn>> columns,
                               List<ConcreteBeam> beams,
                               Level hostLevel,
                               double hostFloorOffset,
                               double floorClearHeight,
                               XYZ mainBeamDir,
                               Func<List<RevitBeam>, double, List<RevitBeam>> adjustLayout)
            :base(hostLevel,hostFloorOffset)
        {
            ConcreteFloor = concreteFloor;
            Columns = columns;
            Beams = beams;
            FloorClearHeight = floorClearHeight;
            MainBeamDir = mainBeamDir;
            AdjustLayout = adjustLayout;
        }

        #endregion

    }
}
