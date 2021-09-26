namespace FormworkOptimize.Core.Entities
{
    public class Plywood :StructuralElement
    {

        #region Properties

        public PlywoodSection Section { get; }

        public override int NumberOfSpans { get; }

        public double DeltaAll { get;}

        #endregion

        #region Constructors

        public Plywood(PlywoodSection section, double span)
            :base(span)
        {
            NumberOfSpans = 3; //Default value as in case of Plywood.
            Section = section;
            DeltaAll = span / 270;
        }

        #endregion

    }
}
