using FormworkOptimize.Core.Entities;
using System;

namespace FormworkOptimize.Core.DTOS
{
    internal class DesignDataDto
    {
        #region Properties

        /// <summary>
        /// Weight per unit area for slab section (t/m^2).
        /// </summary>
        /// <value>
        /// Unit (t/m^2).
        /// </value>
        public double WeightPerAreaSlab { get;}

        /// <summary>
        /// Weight per unit area for beam section (t/m^2).
        /// </summary>
        /// <value>
        /// Unit (t/m^2).
        /// </value>
        public double WeightPerAreaBeam { get;}

        public Plywood Plywood { get;}

        public Beam MainBeam { get;}

        public Beam SecondaryBeam { get;}

        public Func<Beam,double> SecReactionFunc { get;}

        public Func<Beam,double, double> MainReactionFunc { get; }

        public Func<Beam, double, StrainingActions> MainBeamSolver { get; }

        public Func<Beam, Plywood, StrainingActions> SecBeamSolver { get; }

        #endregion

        #region Constructors

        public DesignDataDto(double weightPerAreaSlab, double weightPerAreaBeam, Plywood plywood, 
                             Beam mainBeam, Beam secondaryBeam, Func<Beam, double> secReactionFunc, 
                             Func<Beam,double,double> mainReactionFunc,
                             Func<Beam, double, StrainingActions> mainBeamSolver, 
                             Func<Beam, Plywood, StrainingActions> secSolver)
        {
            WeightPerAreaSlab = weightPerAreaSlab;
            WeightPerAreaBeam = weightPerAreaBeam;
            Plywood = plywood;
            MainBeam = mainBeam;
            SecondaryBeam = secondaryBeam;
            SecReactionFunc = secReactionFunc;
            MainReactionFunc = mainReactionFunc;
            MainBeamSolver = mainBeamSolver;
            SecBeamSolver = secSolver;
        }

        #endregion

    }
}
