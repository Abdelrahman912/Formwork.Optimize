using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.Entities
{
    public abstract class FrameShoring:Shoring
    {
        #region Properties

        /// <summary>
        /// Width of the frame (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double Width { get; }

        /// <summary>
        /// Spacing between two consecutive frames (cm).
        /// </summary>
        /// <value>
        /// Unit (cm).
        /// </value>
        public double Spacing { get; }

        #endregion

        #region Constructors

        protected FrameShoring(double width,  double spacing)
        {
            Width = width;
            Spacing = spacing;
        }

        #endregion

    }
}
