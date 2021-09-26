namespace FormworkOptimize.Core.Entities.Revit
{
    public abstract class ConcreteElement
    {
        #region Properties

        /// <summary>
        /// Breadth of rectangular concrete element.
        /// </summary>
        public double B { get; }

        /// <summary>
        /// Height of rectangular concrete element.
        /// </summary>
        public double H { get; }

        #endregion

        #region Constructors

        public ConcreteElement(double b , double h)
        {
            B = b;
            H = h;
        }

        #endregion


    }
}
