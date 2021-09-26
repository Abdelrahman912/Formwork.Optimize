using FormworkOptimize.Core.Entities;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.DTOS
{
    public class TableDesignOutput
    {
        //TODO
        #region Properties

        public Tuple<Plywood, List<DesignReport>> Plywood { get; }

        public Tuple<Beam, List<DesignReport>> SecondaryBeam { get; }

        public Tuple<Beam, List<DesignReport>> MainBeam { get; }

        #endregion

        #region Constructors

        public TableDesignOutput(Tuple<Plywood, List<DesignReport>> plywood, 
                                 Tuple<Beam, List<DesignReport>> secondaryBeam,
                                 Tuple<Beam, List<DesignReport>> mainBeam)
        {
            Plywood = plywood;
            SecondaryBeam = secondaryBeam;
            MainBeam = mainBeam;
        }

        #endregion

    }
}
