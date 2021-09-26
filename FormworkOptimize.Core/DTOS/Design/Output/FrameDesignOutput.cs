using FormworkOptimize.Core.Entities;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.DTOS
{
    public class FrameDesignOutput
    {

        #region Properties

        public Tuple<Plywood, List<DesignReport>> Plywood { get; }

        public Tuple<Beam, List<DesignReport>> SecondaryBeam { get; }

        public Tuple<Beam, List<DesignReport>> MainBeam { get; }

        public Tuple<FrameShoring, DesignReport> Shoring { get; }

        #endregion

        #region Constructors

        public FrameDesignOutput(Tuple<Plywood, List<DesignReport>> plywood, Tuple<Beam, List<DesignReport>> secondaryBeam, Tuple<Beam,
           List<DesignReport>> mainBeam, Tuple<FrameShoring, DesignReport> shoring)
        {
            Plywood = plywood;
            SecondaryBeam = secondaryBeam;
            MainBeam = mainBeam;
            Shoring = shoring;
        }

        #endregion

    }
}
