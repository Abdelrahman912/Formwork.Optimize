namespace FormworkOptimize.Core.Entities.GeneticResult
{
    public class ChromosomeHistory
    {
       
        #region Properties

        public int GenerationNumber { get; }

        public double Fitness { get; }

        #endregion

        #region Constructors

        public ChromosomeHistory(int generationNumber, double fitness)
        {
            GenerationNumber = generationNumber;
            Fitness = fitness;
        }

        #endregion

        #region Methods

        #endregion

    }
}
