using CSharp.Functional.Constructs;
using static FormworkOptimize.Core.Constants.Database;
using static FormworkOptimize.Core.Errors.Errors;

namespace FormworkOptimize.Core.Entities.Geometry
{
    public class UserDefinedSecondaryBeamSpacing:SecondaryBeamSpacing
    {

        #region Properties

        /// <summary>
        /// Spacing between Secondary beams in cm.
        /// </summary>
        public double Spacing { get; }

        #endregion

        #region Constructors

        public UserDefinedSecondaryBeamSpacing(double spacingInCm)
        {
            Spacing = spacingInCm;
        }

        #endregion

        #region Methods

        public override Validation<Plywood> AsNewPlywood(Plywood old)
        {
            if (Spacing < MIN_SEC_BEAM_SPACING)
                return ShortSecSpacing;
            return new Plywood(old.Section, Spacing);
        }

        #endregion

    }
}
