using Autodesk.Revit.DB;
using FormworkOptimize.Core.DTOS.Internal;
using System;

namespace FormworkOptimize.Core.Entities.Revit
{
    public class ConcreteBeam:ConcreteElement
    {

        #region Properties

        /// <summary>
        /// Line representing the underlying geometry of concrete beam.
        /// </summary>
        public Line BeamLine { get;}

       /// <summary>
       /// Clear height between beam and floor below.
       /// </summary>
        public double ClearHeight { get;}

        #endregion

        #region Constructor

        public ConcreteBeam(double b , double h , Line beamLine,double clearHeight):
            base(b,h)
        {
            BeamLine = beamLine;
            ClearHeight = clearHeight;
        }

        #endregion
    }
}
