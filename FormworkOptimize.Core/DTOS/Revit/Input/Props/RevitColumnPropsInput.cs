using FormworkOptimize.Core.Enums;

namespace FormworkOptimize.Core.DTOS.Revit.Input.Props
{
    public class RevitColumnPropsInput : RevitPropsInput
    {

        #region Properties


        #endregion

        #region Constructors

        public RevitColumnPropsInput(EuropeanPropTypeName propType, PlywoodSectionName plywoodSection, RevitBeamSectionName secondaryBeamSection,
                                      RevitBeamSectionName mainBeamSection,
                                      double secondaryBeamSpacing) :
            base(propType, plywoodSection, secondaryBeamSection, mainBeamSection, secondaryBeamSpacing)
        {
        }

        #endregion

    }
}
