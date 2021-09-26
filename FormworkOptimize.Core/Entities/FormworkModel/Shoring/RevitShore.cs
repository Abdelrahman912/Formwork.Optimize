using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    public class RevitShore
    {
        #region Properties

        public List<RevitShoreMain> Mains { get; }

        public List<RevitShoreBracing> Bracings { get; }

        public List<RevitBeam> MainBeams { get; }

        public List<RevitBeam> SecondaryBeams { get; }

        #endregion

        #region Constructor

        public RevitShore(List<RevitShoreMain> mains,
                            List<RevitShoreBracing> bracings,
                            List<RevitBeam> mainBeams,
                            List<RevitBeam> secBeams)

        {
            Mains = mains;
            Bracings = bracings;
            MainBeams = mainBeams;
            SecondaryBeams = secBeams;
        }

        #endregion

    }
}
