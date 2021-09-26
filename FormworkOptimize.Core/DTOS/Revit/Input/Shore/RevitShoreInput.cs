using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Shore
{
    public class RevitShoreInput
    {
        #region Properties

        public PlywoodSectionName PlywoodSection { get; }

        public RevitBeamSectionName SecondaryBeamSection { get; }

        public RevitBeamSectionName MainBeamSection { get; }

        public double SecondaryBeamSpacing { get; }

        #endregion

        #region Constructors

        public RevitShoreInput(PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection,
                                 RevitBeamSectionName mainBeamSection,
                                 double secondaryBeamSpacing)
        {
            PlywoodSection = plywoodSection;
            SecondaryBeamSection = secondaryBeamSection;
            MainBeamSection = mainBeamSection;
            SecondaryBeamSpacing = secondaryBeamSpacing;
        }

        #endregion

    }
}
