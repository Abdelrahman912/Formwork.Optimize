using Autodesk.Revit.DB;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;

namespace FormworkOptimize.Core.Entities.FormworkModel.SuperStructure
{
    public class RevitBeam
    {
        #region Properties

        public RevitBeamSection Section { get; }

        public XYZ OffsetVector { get; }

        public XYZ StartPoint { get; }

        public XYZ EndPoint { get; }

        public double Length { get; }

        public Level HostLevel { get; }

        /// <summary>
        /// Disatnce between supporting floor and support.
        /// </summary>
        public double OffsetFromLevel { get; }

        /// <summary>
        /// Unit veacto that represents direction of the beam.
        /// </summary>
        public XYZ Direction { get; }

        #endregion

        #region Constructors


        public RevitBeam(RevitBeamSection section, XYZ startPoint, XYZ endPoint, Level hostLevel, double offsetFromLevel,double cantLength = 0)
        {
            Section = section;
            StartPoint = startPoint.CopyWithNewZ(0);
            EndPoint = endPoint.CopyWithNewZ(0);
            Direction = (EndPoint - StartPoint).Normalize();
            Length = StartPoint.DistanceTo(EndPoint) + 2* cantLength;
            HostLevel = hostLevel;
            OffsetFromLevel = offsetFromLevel+Section.Height.CmToFeet();
            OffsetVector = XYZ.Zero;
        }

        public RevitBeam(RevitBeam oldBeam , XYZ offsetVector)
        {
            Section = oldBeam.Section;
            StartPoint = oldBeam.StartPoint;
            EndPoint = oldBeam.EndPoint;
            Direction = (EndPoint - StartPoint).Normalize();
            Length = oldBeam.Length;
            HostLevel = oldBeam.HostLevel;
            OffsetFromLevel = oldBeam.OffsetFromLevel;
            OffsetVector = offsetVector;
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
