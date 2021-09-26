using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.Core.Helpers.DesignHelper
{
    public static class PlywoodHelper
    {

        /// <summary>
        /// The get plywood span from moment (3 spans or more)
        /// </summary>
        /// <value>Unit (m)</value>
        private static Func<PlywoodSection, double, double> GetPlywoodSpanFromMoment = (section, w) =>
                Math.Sqrt(section.Mall / (0.095 * w));

        /// <summary>
        /// The get plywood span from shear (3 spans or more)
        /// </summary>
        /// <value>Unit (m)</value>
        private static Func<PlywoodSection, double, double> GetPlywoodSpanFromShear = (section, w) =>
        {
            //Timber width (m)
            var B = 50.0 / 1000;
            var t = section.Thickness / 1000;
            return (section.Qall + 0.55 * w * (B + t)) / (0.55 * w);
        };

        /// <summary>
        /// The get plywood span from deflection (3 spans or more)
        /// </summary>
        /// <value>Unit (m)</value>
        private static Func<PlywoodSection, double, double> GetPlywoodSpanFromDeflection = (section, w) =>
        {
            //(cm^3)
            var spanCube = (section.E * section.I * 100) / (270 * 0.0066 * w);
            return Math.Pow(spanCube, (1.0 / 3.0)) / 100;
        };

        private static List<Func<PlywoodSection, double, double>> plywoodSpanFuncs = new List<Func<PlywoodSection, double, double>>()
        {
            GetPlywoodSpanFromMoment,
            GetPlywoodSpanFromDeflection,
            GetPlywoodSpanFromShear
        };

        /// <summary>
        /// Get the spacing between  secondary beams that makes Plywood section
        /// safe in (moment, shear, deflection).
        /// </summary>
        /// <param name="plywoodSection"></param>
        /// <param name="weight">Weight per unit area in (t/m^2)</param>
        /// <returns></returns>
        public static Plywood SolveForSpan(this PlywoodSection plywoodSection, double weight)
        {
            var spacing = plywoodSpanFuncs.Select(f => f(plywoodSection, weight) * 100)
                                          .Min()
                                          .ToNearest5Cm();
            
            return new Plywood(plywoodSection, spacing);
        }

        /// <summary>
        /// Get straining actions on a plywood due to given load.
        /// </summary>
        /// <param name="plywood">The plywood.</param>
        /// <param name="w">Weight per unit area (t/m^2).</param>
        /// <returns></returns>
        public static StrainingActions GetStrainingActions(this Plywood plywood, double w)
        {
            var spanInMeter = plywood.Span / 100;
            var moment = 0.095 * w * spanInMeter * spanInMeter;
            var shear  = 0.525 * (spanInMeter - 0.05 - 0.018) * w;
            //var reaction = w * plywood.Span / 100;
            var deflection = (0.0066*w * Math.Pow(plywood.Span,4)) / (100*plywood.Section.E * plywood.Section.I);
            return new StrainingActions( moment, shear, 0,  deflection);
        }

    }
}
