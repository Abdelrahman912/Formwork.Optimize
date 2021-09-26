using Autodesk.Revit.DB;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    /// <summary>
    /// Represnts Group of Ledgers in one side of cuplock rectangle
    /// </summary>
    public class RevitLedger
    {
        #region Properties

        public XYZ StartPoint { get; }

        public XYZ EndPoint { get; }

        public double Length { get; }

        public Level HostLevel { get; }

        public XYZ Direction { get; }

        public List<double> OffsetsFromLevel { get; }

        public int Count => OffsetsFromLevel.Count;

        public SteelType SteelType { get; }

        #endregion

        #region Constructors

        public RevitLedger(XYZ startPoint, XYZ endPoint, Level hostLevel,List< double > offsetsFromLevel,SteelType steelType )
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Direction = (EndPoint - StartPoint).Normalize();
            Length = StartPoint.DistanceTo(EndPoint);
            HostLevel = hostLevel;
            OffsetsFromLevel = offsetsFromLevel;
            SteelType = steelType; 
        }

        #endregion

        #region Methods

        public override int GetHashCode()
        {
            var prime1 = 17;
            var prime2 = 23;
            unchecked
            {
                var hash = prime1 * prime2 * (StartPoint.GetHash() + EndPoint.GetHash());
                return hash;
            }
        }

        #endregion


    }
}
