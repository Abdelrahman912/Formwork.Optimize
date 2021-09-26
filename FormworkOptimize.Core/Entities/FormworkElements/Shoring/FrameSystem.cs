using FormworkOptimize.Core.Enums;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.Core.Entities
{
    public class FrameSystem:FrameShoring
    {

        #region Properties

        public FrameSystemType FrameType { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameSystem"/> class.
        /// </summary>
        /// <param name="frameType">Type of the frame.</param>
        /// <param name="spacing">Spacing between two consecutive frames (cm).</param>
        public FrameSystem(FrameTypeName frameType, double spacing)
            :base(100,spacing)
        {
            FrameType = GetFrameCapacity(frameType);
        }

        #endregion
    }
}
