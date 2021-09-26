using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    public class RevitCuplock
    {
        #region Properties

        public List<RevitLedger> Ledgers { get; }

        public List<RevitCuplockVertical> Verticals { get; }

        public List<RevitCuplockBracing> Bracings { get; }

        public List<RevitBeam> MainBeams { get; }

        public List<RevitBeam> SecondaryBeams { get; }

        #endregion

        #region Constructor

        public RevitCuplock(List<RevitCuplockVertical> verticals, 
                            List<RevitLedger> ledgers, 
                            List<RevitCuplockBracing> bracings,
                            List<RevitBeam> mainBeams,
                            List<RevitBeam> secBeams)
                          
        {
            Ledgers = ledgers;
            Verticals = verticals;
            Bracings = bracings;
            MainBeams = mainBeams;
            SecondaryBeams = secBeams;
        }

        #endregion

    }
}
