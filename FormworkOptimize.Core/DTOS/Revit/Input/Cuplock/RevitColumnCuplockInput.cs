using FormworkOptimize.Core.DTOS.Internal;
using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.DTOS.Revit.Input
{
    public class RevitColumnCuplockInput:RevitCuplockInput
    {

        #region Properties


        #endregion

        #region Constructors

        public RevitColumnCuplockInput(PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection,
                                      RevitBeamSectionName mainBeamSection,
                                      double secondaryBeamSpacing) :
            base(plywoodSection,secondaryBeamSection,mainBeamSection,SteelType.STEEL_37, secondaryBeamSpacing)
        {
        }

        #endregion

    }
}
