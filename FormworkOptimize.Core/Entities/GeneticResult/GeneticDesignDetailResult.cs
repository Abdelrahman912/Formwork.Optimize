using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;

namespace FormworkOptimize.Core.Entities.GeneticResult
{
    public class GeneticDesignDetailResult:IGeneticDetailResult
    {

        #region Properties

        public SectionDesignOutput PlywoodDesignOutput { get ;  }

        public SectionDesignOutput SecondaryBeamDesignOutput { get; }

        public SectionDesignOutput MainBeamDesignOutput { get; }

        public ShoringDesignOutput ShoringSystemDesignOutput { get; }

        public string ShoringHeader { get;}

        public string MainBeamHeader { get;  }

        public string SecondaryBeamHeader { get; }

        public string PlywoodHeader { get;  }

        #endregion

        #region Constructors

        public GeneticDesignDetailResult(SectionDesignOutput plywoodDesignOutput,
                                         SectionDesignOutput secondaryBeamDesignOutput,
                                         SectionDesignOutput mainBeamDesignOutput,
                                         ShoringDesignOutput shoringSystemDesignOutput)
                                         
        {
            PlywoodDesignOutput = plywoodDesignOutput;
            SecondaryBeamDesignOutput = secondaryBeamDesignOutput;
            MainBeamDesignOutput = mainBeamDesignOutput;
            ShoringSystemDesignOutput = shoringSystemDesignOutput;
            PlywoodHeader = $"Plywood: (Section: {plywoodDesignOutput.SectionName})";
            SecondaryBeamHeader = $"Secondary Beam: (Section: {secondaryBeamDesignOutput.SectionName})";
            MainBeamHeader = $"Main Beam: (Section: {mainBeamDesignOutput.SectionName})";
            ShoringHeader = $"Shoring System: ({shoringSystemDesignOutput.ShoringSystemName})";
        }

        #endregion


    }
}
