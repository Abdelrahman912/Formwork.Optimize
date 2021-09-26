using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities
{
    public class FrameSystemType
    {

        #region Properties

        public FrameTypeName Name { get;}

        /// <summary>
        /// Capacity of frame (ton).
        /// </summary>
        /// <value>
        /// Unit (ton).
        /// </value>
        public double Capacity { get;}

        #endregion

        #region Constructors

        internal FrameSystemType(FrameTypeName frameType , double capacity)
        {
            Name = frameType;
            Capacity = capacity;
        }

        #endregion

    }
}
