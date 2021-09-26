using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.Revit;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Document
{
    public class RevitColumnInput:RevitInput
    {
      
        #region Properties

        public List<ConcreteColumnWDropPanel> ColumnsWDrop { get; }

        public List<Tuple<ConcreteColumn,double>> ColumnsWNoDrop { get; }


        public double FloorClearHeight { get;  }

        public double DropClearHeight { get;}

        #endregion

        #region Constructors

        public RevitColumnInput(List<ConcreteColumnWDropPanel> columnsWDrop,List<Tuple<ConcreteColumn,double>> columnsWNoDrop, Level hostLevel, double hostFloorOffset, double floorClearHeight,double dropClearHeight)
            :base(hostLevel, hostFloorOffset)
        {
            ColumnsWDrop = columnsWDrop;
            ColumnsWNoDrop = columnsWNoDrop;
            FloorClearHeight = floorClearHeight;
            DropClearHeight = dropClearHeight;
        }

        #endregion

    }
}
