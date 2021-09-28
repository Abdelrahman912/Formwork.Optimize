namespace FormworkOptimize.Core.Entities.GeneticResult
{
    public class GeneticReport
    {
        #region Properties

        public string Name { get;  }

        public string Value { get;  }

        #endregion

        #region Constructors

        public GeneticReport(string name, string value)
        {
            Name = name;
            Value = value;
        }

        #endregion

    }
}
