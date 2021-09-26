using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.Entities.FormworkModel.SuperStructure
{
    public class RevitBeamSection
    {
        #region Properties

        public RevitBeamSectionName SectionName { get;}

        /// <summary>
        /// Breadth of section in cm.
        /// </summary>
        public double  Breadth { get;  }

        /// <summary>
        /// Height of section in cm.
        /// </summary>
        public double Height { get;}

        #endregion

        #region constructors

        public RevitBeamSection(RevitBeamSectionName sectionName, double breadth, double height)
        {
            SectionName = sectionName;
            Breadth = breadth;
            Height = height;
        }

        #endregion

    }
}
