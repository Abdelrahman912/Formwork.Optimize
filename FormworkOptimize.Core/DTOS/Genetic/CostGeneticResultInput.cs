using Autodesk.Revit.DB;
using FormworkOptimize.Core.DTOS.Revit.Input.Document;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Enums;
using System;

namespace FormworkOptimize.Core.DTOS.Genetic
{
    public class CostGeneticResultInput
    {
        #region Properties

        public Func<FormworkCostElements, FormworkElementCost> CostFunc { get; }

        public RevitFloorInput RevitInput { get;  }

        public double BoundaryLinesOffest { get;}

        public double BeamsOffset { get; }

        public FormworkTimeLine TimeLine { get;  }

        public ManPowerCost ManPowerCost { get; }

        public EquipmentsCost EquipmentCost { get;  }

        public TransportationCost TransportationCost { get; }

        /// <summary>
        /// Area of the floor in meter square.
        /// </summary>
        public double FloorArea { get; }

        public Func<PlywoodSectionName,RevitFloorPlywood> PlywoodFunc { get;  }

        #endregion

        #region Constructors

        public CostGeneticResultInput(Func<FormworkCostElements, FormworkElementCost> costFunc, 
                                      RevitFloorInput revitInput, 
                                      double boundaryLinesOffest, 
                                      double beamsOffset,
                                      FormworkTimeLine timeLine,
                                      ManPowerCost manPowerCost,
                                      EquipmentsCost equipmentCost,
                                      TransportationCost transportationCost,
                                      double floorArea,
                                      Func<PlywoodSectionName, RevitFloorPlywood> plywoodFunc)
        {
            CostFunc = costFunc;
            RevitInput = revitInput;
            BoundaryLinesOffest = boundaryLinesOffest;
            BeamsOffset = beamsOffset;
            TimeLine = timeLine;
            ManPowerCost = manPowerCost;
            EquipmentCost = equipmentCost;
            TransportationCost = transportationCost;
            FloorArea = floorArea;
            PlywoodFunc = plywoodFunc;
        }

        #endregion

    }
}
