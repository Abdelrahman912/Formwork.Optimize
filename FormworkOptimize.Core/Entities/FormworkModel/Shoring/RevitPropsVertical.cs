using Autodesk.Revit.DB;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    public class RevitPropsVertical
    {
        #region Properties

        public double Height { get; }
        public double ULockDistance { get; }

        public EuropeanPropTypeName PropType { get; }

        /// <summary>
        /// Vector in which U-Head points.
        /// </summary>
        public XYZ UVector { get; }

        public XYZ Position { get; }

        public Level HostLevel { get; }

        public double OffsetFromLevel { get; }

        #endregion

        #region Constructors

        public RevitPropsVertical(EuropeanPropTypeName propType ,double height, double uLockDistance,
                            XYZ position, Level hostLevel, double offsetFromLevel, XYZ uVector)
        {
            PropType = propType;
            Height = height;
            ULockDistance = uLockDistance;
            Position = position;
            HostLevel = hostLevel;
            OffsetFromLevel = offsetFromLevel;
            UVector = uVector;
        }

        #endregion

    }
}
