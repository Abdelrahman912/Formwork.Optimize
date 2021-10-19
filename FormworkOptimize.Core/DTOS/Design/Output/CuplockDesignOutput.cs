using FormworkOptimize.Core.Entities;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.DTOS
{
    public class CuplockDesignOutput
    {

        #region Properties

        /// <summary>
        /// MaxPlywood , ChosenPlywood
        /// </summary>
        public Tuple<Plywood,Plywood, List<DesignReport>> Plywood { get; }

        public Tuple<Beam, List<DesignReport>> SecondaryBeam { get; }

        public Tuple<Beam, List<DesignReport>> MainBeam { get; }

        public Tuple<CuplockShoring, DesignReport> Shoring { get; }

        #endregion

        #region Constructors

        public CuplockDesignOutput(Tuple<Plywood,Plywood, List<DesignReport>> plywood, Tuple<Beam, List<DesignReport>> secondaryBeam, Tuple<Beam,
                                   List<DesignReport>> mainBeam, Tuple<CuplockShoring, DesignReport> shoring)

        {
            Plywood = plywood;
            SecondaryBeam = secondaryBeam;
            MainBeam = mainBeam;
            Shoring = shoring;
        }

        #endregion

    }
}
