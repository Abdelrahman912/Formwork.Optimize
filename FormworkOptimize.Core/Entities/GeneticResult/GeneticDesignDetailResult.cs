using FormworkOptimize.Core.Entities.GeneticResult.Interfaces;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.GeneticResult
{
    public class GeneticDesignDetailResult:IGeneticDetailResult
    {

        #region Private Fields

        private readonly string _plywoodSectionName;

        private readonly string _secSectionName;

        private readonly string _mainSectionName;

        private readonly string _shoringsystemName;

        #endregion

        #region Properties

        public SectionDesignOutput PlywoodDesignOutput { get ;  }

        public SectionDesignOutput SecondaryBeamDesignOutput { get; }

        public SectionDesignOutput MainBeamDesignOutput { get; }

        public ShoringDesignOutput ShoringSystemDesignOutput { get; }

        public string ShoringHeader { get;}

        public string MainBeamHeader { get;  }

        public string SecondaryBeamHeader { get; }

        public string PlywoodHeader { get;  }

        public string Name { get; }

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
            _plywoodSectionName = plywoodDesignOutput.SectionName;
            _secSectionName = secondaryBeamDesignOutput.SectionName;
            _mainSectionName = mainBeamDesignOutput.SectionName;
            _shoringsystemName = shoringSystemDesignOutput.ShoringSystemName;
            PlywoodHeader = $"Plywood: ({plywoodDesignOutput.SectionName})";
            SecondaryBeamHeader = $"Secondary Beam: ({secondaryBeamDesignOutput.SectionName})";
            MainBeamHeader = $"Main Beam: ({mainBeamDesignOutput.SectionName})";
            ShoringHeader = $"Shoring System: ({shoringSystemDesignOutput.ShoringSystemName})";
            Name = "Design Result";
        }



        #endregion

        #region Methods

        public IEnumerable<GeneticReport> AsReport()
        {
            return new List<GeneticReport>()
            {
                new GeneticReport("Plywood",_plywoodSectionName),
                new GeneticReport("Secondary Beam",_secSectionName),
                new GeneticReport("Main Beam",_mainSectionName),
                new GeneticReport("Shoring System",_shoringsystemName)
            };
        }

        #endregion

    }
}
