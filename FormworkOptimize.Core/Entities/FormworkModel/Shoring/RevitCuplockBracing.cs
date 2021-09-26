using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    public class RevitCuplockBracing
    {
       
        #region Properties

        public XYZ LocationPoint { get; }

        public XYZ Direction { get;  }

        public List<(double Width , double Height,double OffsetFromLevel)> WidthHeightOffsets { get;}

        public Level HostLevel { get; }

        public List<double> Lengths => WidthHeightOffsets.Select(tuple => Math.Sqrt(tuple.Height * tuple.Height + tuple.Width * tuple.Width))
                                                         .ToList();

        #endregion

        #region Constructors

        public RevitCuplockBracing(XYZ startPoint, 
                                   XYZ dir,
                                   List<(double Width,double Height,double OffsetFromLevel)> widthHeightOffsets ,
                                   Level hostLevel)
                                  
        {
            LocationPoint = startPoint;
            HostLevel = hostLevel;
            Direction = dir;
            WidthHeightOffsets = widthHeightOffsets;
        }

        #endregion

    }
}
