using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.FormworkModel.Shoring
{
    public class RevitProps
    {
        #region Properties

        public List<RevitPropsVertical> Verticals { get; }

        public List<RevitPropsLeg> Legs { get; }

        public List<RevitBeam> MainBeams { get; }

        public List<RevitBeam> SecondaryBeams { get; }

        #endregion

        #region Constructor

        public RevitProps(List<RevitPropsVertical> verticals,
                          List<RevitPropsLeg> legs,
                          List<RevitBeam> mainBeams,
                          List<RevitBeam> secBeams)

        {
            Verticals = verticals;
            Legs = legs;
            MainBeams = mainBeams;
            SecondaryBeams = secBeams;
        }

        #endregion

    }
}
