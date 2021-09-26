using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Helpers.DesignHelper
{
    public static class EmpiricalBeamSolver
    {

        /// <summary>
        /// Solves the beam for shear.
        /// </summary>
        /// <param name="beam">The beam.</param>
        /// <param name="slabWeight">Distributed load from slab (t/m').</param>
        /// <param name="beamWeight">Distributed load from beam (t/m').</param>
        /// <param name="beamWidth">Distance on which beam is distributed (m).</param>
        /// <returns>Max. shear (ton).</returns>
        private static double SolveForShear(this Beam beam, double slabWeight, double beamWeight = 0, double beamWidth = 0)
        {
            var spanInMeter = beam.Span / 100;
            var cantileverLengthInMeter = beam.CantileverLength / 100;
            if (beam.NumberOfSpans == 1)
            {
                //Simple.
                return Math.Max(slabWeight * cantileverLengthInMeter, ((spanInMeter - beamWidth) * slabWeight + (beamWeight * beamWidth)) / 2);
            }
            else
            {
                //Continous.
                return Math.Max(slabWeight * cantileverLengthInMeter, 0.6 * slabWeight * spanInMeter + 0.5 * (beamWeight - slabWeight) * beamWidth);
            }
        }

        /// <summary>
        /// Solves the beam for reaction.
        /// </summary>
        /// <param name="beam">The beam.</param>
        /// <param name="slabWeight">Distributed load from slab (t/m').</param>
        /// <param name="beamWeight">Distributed load from beam (t/m').</param>
        /// <param name="beamWidth">Distance on which beam is distributed (m).</param>
        /// <returns>Max. reaction (ton).</returns>
        public static double GetReaction(this Beam beam, double slabWeight, double beamWeight = 0, double beamWidth = 0)
        {
            var beamLengthInMeter = beam.BeamLength / 100;
            var spanInMeter = beam.Span / 100;
            if (beam.NumberOfSpans == 1)
            {
                //Simple.
                return (beamWeight * beamWidth + slabWeight * (beamLengthInMeter - beamWidth)) / 2;
            }
            else
            {
                //Continous.
                return 1.1 * slabWeight * spanInMeter + 0.5 * (beamWeight - slabWeight) * beamWidth;
            }
        }

        /// <summary>
        /// Solves the beam for moment.
        /// </summary>
        /// <param name="beam">The beam.</param>
        /// <param name="slabWeight">Distributed load from slab (t/m').</param>
        /// <param name="beamWeight">Distributed load from beam (t/m').</param>
        /// <param name="beamWidth">Distance on which beam is distributed (m).</param>
        /// <returns>Max. moment (ton.m).</returns>
        private static double SolveForMoment(this Beam beam, double slabWeight, double beamWeight = 0, double beamWidth = 0)
        {
            var spanInMeter = beam.Span / 100;
            var cantileverLengthInMeter = beam.CantileverLength / 100;
            switch (beam.NumberOfSpans)
            {
                case 1:
                    var addedWeight = beamWeight - slabWeight;
                    var mCantilever = (slabWeight * cantileverLengthInMeter * cantileverLengthInMeter) / 2;
                    var mMidSpan = Math.Abs(mCantilever - (slabWeight * spanInMeter * spanInMeter) / 8) + ((addedWeight * beamWidth * spanInMeter) / 4 - (addedWeight * beamWidth * beamWidth) / 8);
                    return Math.Max(mCantilever, mMidSpan);
                case 2:
                    var weightedAverageWeight = (beamWeight * beamWidth + slabWeight * (spanInMeter - beamWidth)) / spanInMeter;
                    return Math.Max((slabWeight * cantileverLengthInMeter * cantileverLengthInMeter) / 2, (weightedAverageWeight * spanInMeter * spanInMeter) / 9);
                case 3:
                default:
                    weightedAverageWeight = (beamWeight * beamWidth + slabWeight * (spanInMeter - beamWidth)) / spanInMeter;
                    return Math.Max((slabWeight * cantileverLengthInMeter * cantileverLengthInMeter) / 2, (weightedAverageWeight * spanInMeter * spanInMeter) / 10);
            }
        }

        /// <summary>
        /// Get Deflection of a simple beam with a distributed load in the middle.
        /// </summary>
        /// <param name="span">Beam span (cm).</param>
        /// <param name="w">Distributed load (t/cm).</param>
        /// <param name="a">Distributed length (cm).</param>
        /// <param name="E">Modulus of elasticity (t/cm^2).</param>
        /// <param name="I">Moment of Inertia.</param>
        /// <returns>Deflection at Beam midspan (cm).</returns>
        private static double GetDeflectionUsingConjugateBeam(double span, double w, double a, double E, double I)
        {
            //moment (t.cm) at point just before w.
            var y1 = w * a * (span - a) / 4;
            //moment at beam midspan (t.cm).
            var y2 = (w * a * span / 4) - (w * a * a / 8);
            //moment at midspan of a (t.cm).
            var y3 = w * a * a / 32;
            //distance of unloaded area.
            var c = (span - a) / 2;
            var A1 = 0.5 * c * y1;
            var A2 = y1 * a / 2;
            var A3 = y2 * a / 4;
            var A4 = y3 * a / 3;
            var Re = A1 + A2 + A3 + A4;
            return (Re * span / 2 - A1 * ((c / 3) + (a / 2)) - A2 * (a / 4) - A3 * (a / 6) - A4 * (a / 4)) / (E * I);
        }

        /// <summary>
        /// Solves the beam for deflection.
        /// </summary>
        /// <param name="beam">The beam.</param>
        /// <param name="slabWeight">Distributed load from slab (t/m').</param>
        /// <param name="beamWeight">Distributed load from beam (t/m').</param>
        /// <param name="beamWidth">Distance on which beam is distributed (m).</param>
        /// <returns>Max. deflection (cm).</returns>
        private static double SolveForDeflection(this Beam beam, double slabWeight, double beamWeight = 0, double beamWidth = 0)
        {
            if (beam.NumberOfSpans == 1)
            {
                //Simple.
                var deltaSimple = (5 * slabWeight * Math.Pow(beam.Span, 4)) / (100 * 384 * beam.Section.E * beam.Section.I);
                return deltaSimple + GetDeflectionUsingConjugateBeam(beam.Span, (beamWeight - slabWeight) / 100, beamWidth, beam.Section.E, beam.Section.I);
            }
            else
            {
                //Continous.
                var weightedAverageWeight = (beamWeight * beamWidth * 100 + slabWeight * (beam.Span - beamWidth * 100)) / beam.Span;
                var deltaSimple = (5 * weightedAverageWeight * Math.Pow(beam.Span, 4)) / (100 * 384 * beam.Section.E * beam.Section.I);
                return 0.8 * deltaSimple;
            }
        }

        /// <summary>
        /// Get beam straining actions associated with given loads.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="slabWeight">Distributed load from slab section (t/m').</param>
        /// <param name="beamWeight">Distributed load from beam section (t/m').</param>
        /// <param name="beamWidth">Length over which load from beam section is distributed (m).</param>
        /// <returns></returns>
        public static StrainingActions GetStrainingActions(Beam beam, double slabWeight, double beamWeight = 0, double beamWidth = 0)
        {
            var moment = beam.SolveForMoment(slabWeight, beamWeight, beamWidth);
            var shear = beam.SolveForShear(slabWeight, beamWeight, beamWidth);
            //var reaction = beam.GetReaction(slabWeight, beamWeight, beamWidth);
            var deflection = beam.SolveForDeflection(slabWeight, beamWeight, beamWidth);
            return new StrainingActions(moment, shear, 0,deflection);
        }

    }
}
