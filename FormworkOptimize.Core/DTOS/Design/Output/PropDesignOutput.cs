using FormworkOptimize.Core.Entities;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.DTOS
{
    public class PropDesignOutput
    {

        #region Properties

        public Tuple<Plywood, List<DesignReport>> Plywood { get; }

        public Tuple<Beam, List<DesignReport>> SecondaryBeam { get; }

        public Tuple<Beam, List<DesignReport>> MainBeam { get; }

        public Tuple<PropShoring, DesignReport> Shoring { get;}

        #endregion

        #region Constructors

        public PropDesignOutput(Tuple<Plywood, List<DesignReport>> plywood, Tuple<Beam, List<DesignReport>> secondaryBeam, Tuple<Beam,
           List<DesignReport>> mainBeam,Tuple<PropShoring, DesignReport> shoring)
        {
            Plywood = plywood;
            SecondaryBeam = secondaryBeam;
            MainBeam = mainBeam;
            Shoring = shoring;
        }

        #endregion

    }
}
