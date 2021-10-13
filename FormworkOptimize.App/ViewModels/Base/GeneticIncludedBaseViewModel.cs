namespace FormworkOptimize.App.ViewModels.Base
{
   public abstract class GeneticIncludedBaseViewModel : ViewModelBase
    {

        #region Properties

        public string Name { get;  }

        #endregion

        #region Constructors

        public GeneticIncludedBaseViewModel(string name)
        {
            Name = name;
        }

        #endregion

    }
}
