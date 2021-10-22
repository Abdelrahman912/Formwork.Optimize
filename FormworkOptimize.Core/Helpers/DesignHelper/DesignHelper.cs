using CSharp.Functional.Constructs;
using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.DTOS;
using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static FormworkOptimize.Core.Constants.Database;
using static FormworkOptimize.Core.Errors.Errors;
using static FormworkOptimize.Core.Constants.RevitBase;
using CSharp.Functional.Extensions;

namespace FormworkOptimize.Core.Helpers.DesignHelper
{
    public static class DesignHelper
    {

        public static Validation<Beam> AsBeam(this BeamSection beamSection , double spanCm , double beamLengthCm)
        {
            var beamLengthWoMinCant = beamLengthCm - 2 * MIN_CANTILEVER_LENGTH;
            var nSpans = (int)(beamLengthWoMinCant / spanCm);
            var actualCantLength = (beamLengthWoMinCant - nSpans * spanCm) / 2 + MIN_CANTILEVER_LENGTH;
            if (actualCantLength > MAX_CANTILEVER_LENGTH)
                return LongBeam(beamLengthCm, spanCm);
            return new Beam(beamSection, spanCm, beamLengthCm, nSpans, actualCantLength);
        }

        /// <summary>
        /// Claculates the area weight.
        /// </summary>
        /// <param name="ts">Concrete Slab thickness (cm).</param>
        /// <returns>Weight (t/m^2)</returns>
        public static double CalculateAreaWeight(double ts)
        {
            //Concrete slab self weight (t/m^2).
            var wc = 2.5 * (ts / 100);
            //(t/m^2).
            var workingLL = 0.075;
            //(t/m^2).
            var boardSelfWeight = 0.0125;

            var minPouringLL = 0.075;
            var maxPouringLL = 0.175;
            var tenPercentSelfWeight = Math.Min(Math.Max(0.1 * wc, minPouringLL), maxPouringLL);
            var totalWeight = wc + workingLL + boardSelfWeight;
            if (ts <= 30)
            {
                totalWeight += minPouringLL;
            }
            else if (ts > 70)
            {
                totalWeight += maxPouringLL;
            }
            else
            {
                //  700  >= ts > 300
                totalWeight += tenPercentSelfWeight;
            }
            return totalWeight;
        }

        internal static Validation<DesignDataDto> GetDesignData(double slabThickness, double beamThickness, double beamWidth,
                                                  PlywoodSectionName plywoodSectionName,
                                                  BeamSectionName secondarySectionName, BeamSectionName mainsectionName, double mainSpan, double secSpan,
                                                  double mainTotalLength, double secTotalLength,
                                                  Func<Beam, double, double, double, StrainingActions> beamSAFunc,
                                                  Func<Beam, double, double, double, double> beamReactionFunc)
        {


            if (mainTotalLength <= mainSpan + 2 * MIN_CANTILEVER_LENGTH || secTotalLength < secSpan + 2 * MIN_CANTILEVER_LENGTH)
                return ShortBeam;

            //Weight per unit area (t/m^2).
            var weightPerAreaSlab = CalculateAreaWeight(slabThickness);
            var weightPerAreaBeam = CalculateAreaWeight(beamThickness);

            //Intializing new plywood.
            var plywoodSection = GetPlywoodSection(plywoodSectionName);
            var plywood = plywoodSection.SolveForSpan(Math.Max(weightPerAreaSlab, weightPerAreaBeam));

            //Intializing new secondary beam.
            var secBeam = GetBeamSection(secondarySectionName)
                         .AsBeam(secSpan,secTotalLength);

            //Intializing new main beam.
            var mainBeam = GetBeamSection(mainsectionName)
                           .AsBeam(mainSpan,mainTotalLength);

            Func<Beam, Plywood, StrainingActions> secSolver = (beam, ply) =>
            {
                var weightOnSecBeamTonPerMeterForSlab = weightPerAreaSlab * (ply.Span / 100);
                if (beamThickness == 0)
                {
                    return beamSAFunc(beam, weightOnSecBeamTonPerMeterForSlab, 0, 0);
                }
                else
                {
                    var weightOnSecBeamTonPerMeterForBeam = weightPerAreaBeam * (ply.Span / 100);
                    return beamSAFunc(beam, weightOnSecBeamTonPerMeterForSlab, weightOnSecBeamTonPerMeterForBeam, beamWidth / 100);
                }
            };

            Func<Beam, double> secReactionFunc = (beam) =>
            {
                var weightOnSecBeamTonPerMeterForSlab = weightPerAreaSlab * (plywood.Span / 100);
                if (beamThickness == 0)
                {
                    return beamReactionFunc(beam, weightOnSecBeamTonPerMeterForSlab, 0, 0);
                }
                else
                {
                    var weightOnSecBeamTonPerMeterForBeam = weightPerAreaBeam * (plywood.Span / 100);
                    return beamReactionFunc(beam, weightOnSecBeamTonPerMeterForSlab, weightOnSecBeamTonPerMeterForBeam, beamWidth / 100);
                }
            };

            Func<Beam,double, double> mainReactionFunc = (beam,reaction) =>
            {
                var weightOnMainBeamTonPerMeter = reaction / (plywood.Span / 100);
                return beamReactionFunc(beam, weightOnMainBeamTonPerMeter, 0, 0);
            };

            Func<Beam, double, StrainingActions> mainSolver = (main, reaction) =>
            {
                var weightOnMainBeamTonPerMeter = reaction / (plywood.Span / 100);
                return beamSAFunc(main, weightOnMainBeamTonPerMeter, 0, 0);
            };

            var dto = from m in mainBeam
                      from s in secBeam
                      select new DesignDataDto(weightPerAreaSlab, weightPerAreaBeam, plywood, m,
                                     s, secReactionFunc, mainReactionFunc, mainSolver, secSolver);
            return dto;
          
        }

        public static Tuple<bool, double> IsSafe(this List<DesignReport> reports) =>
            Tuple.Create(reports.TrueForAll(report => report.Status == DesignStatus.SAFE), reports.Max(r => r.DesignRatio));

        /// <summary>
        /// Reponsible for taking intial secondary beam and plywood and return 
        /// new beam and plywood.
        /// </summary>
        /// <param name="beam">Intial secondary beam.</param>
        /// <param name="plywood">Itial Plywood.</param>
        /// <param name="beamSolver">Function responsible for solving the beam.</param>
        /// <param name="designFuncs">Collection of functions that encapsulate selection criteria.</param>
        /// <returns></returns>
        public static Tuple<Beam, Plywood> DesignAsSecondary(this Beam beam, Plywood plywood,
                                                             Func<Beam, Plywood, StrainingActions> beamSolver,
                                                             List<Func<Beam, Plywood, Func<Beam, Plywood, StrainingActions>, Tuple<Beam, Plywood, double>>> designFuncs)
        {
            var isSafe = beamSolver(beam, plywood).CreateReports(beam)
                                                  .IsSafe();
            if (isSafe.Item1)
                return Tuple.Create(beam, plywood);
            else
            {
                var results = designFuncs.Select(f => f(beam, plywood, beamSolver))
                                         .OrderByDescending(t => t.Item3)
                                         .ToList();
                var first = results.FirstOrDefault(t => t.Item3 <= 1);
                var last = results.Last();

                return first != null ? Tuple.Create(first.Item1, first.Item2) : Tuple.Create(last.Item1, last.Item2);
            }
        }

        /// <summary>
        /// Responsible for selecting appropriate spacing between secondary beams.
        /// </summary>
        /// <param name="secBeam">Un-safe secondary beam.</param>
        /// <param name="plywood">Initial plywood.</param>
        /// <param name="secSolver">Function responsible for solving secondary beam.</param>
        /// <returns>new Secondary beam , new Plywood , Design ratio</returns>
        public static Tuple<Beam, Plywood, double> DesignSecondaryBeamForSpacing(Beam secBeam, Plywood plywood, Func<Beam, Plywood, StrainingActions> secSolver)
        {
            var result = secSolver(secBeam, plywood).CreateReports(secBeam)
                                                     .IsSafe();
            var designRatio = result.Item2;
            var isSafe = result.Item1;
            while (!isSafe && plywood.Span > 30)
            {
                plywood = new Plywood(plywood.Section, plywood.Span - 5);
                result = secSolver(secBeam, plywood).CreateReports(secBeam)
                                                    .IsSafe();
                isSafe = result.Item1;
                designRatio = result.Item2;
            }
            return Tuple.Create(secBeam, plywood, designRatio);
        }

        /// <summary>
        /// Resonsible for selecting appropriate secondary beam span.
        /// </summary>
        /// <param name="secBeam">Un-safe secondary beam.</param>
        /// <param name="plywood">Plywood section.</param>
        /// <param name="secSolver">Function responsible for solving secondary beam.</param>
        /// <returns>(new Secondary beam, new Plywood, Design ratio)</returns>
        public static Tuple<Beam, Plywood, double> DesignSecondaryBeamForSpan(Beam secBeam, Plywood plywood, Func<Beam, Plywood, StrainingActions> secSolver)
        {
            var result = secSolver(secBeam, plywood).CreateReports(secBeam)
                                                    .IsSafe();
            var designRatio = result.Item2;
            var isSafe = result.Item1;
            do
            {
                double? lessSpan = Database.LedgerLengths.Where(l => l < secBeam.Span)
                                                         .OrderByDescending(l => l)
                                                         .FirstOrDefault();
                if (lessSpan == null)
                {
                    return Tuple.Create(secBeam, plywood, designRatio);
                }
                else
                {
                    secBeam = secBeam.Section.AsBeam(lessSpan.Value, secBeam.BeamLength).Match(_=>null,b=>b);
                    result = secSolver(secBeam, plywood).CreateReports(secBeam)
                                                      .IsSafe();
                    isSafe = result.Item1;
                    designRatio = result.Item2;
                }
            } while (!isSafe);
            return Tuple.Create(secBeam, plywood, designRatio);
        }


        /// <summary>
        /// Reponsible for taking intial main beam and secondary beam 
        /// and return new main beam and new secondary beam.
        /// </summary>
        /// <param name="main">Intial main beam.</param>
        /// <param name="secondary">Intial Secondary beam.</param>
        /// <param name="mainSolver">Function responsible for solving main beam.</param>
        /// <param name="secReactionFunc">Function responsible calculating reaction of secondary beam (ton).</param>
        /// <param name="designFuncs">Collection of functions that encapsulate selection criteria.</param>
        /// <returns></returns>
        public static Tuple<Beam, Beam> DesignAsMain(this Beam main, Beam secondary,
                                                     Func<Beam, double, StrainingActions> mainSolver,
                                                     Func<Beam, double> secReactionFunc,
                                                     List<Func<Beam, Beam, Func<Beam, double, StrainingActions>, Func<Beam, double>, Tuple<Beam, Beam, double>>> designFuncs)
        {
            var secReaction = secReactionFunc(secondary);
            var isSafe = mainSolver(main, secReaction).CreateReports(main)
                                                      .IsSafe();
            if (isSafe.Item1)
                return Tuple.Create(main, secondary);
            else
            {
                var results = designFuncs.Select(f => f(main, secondary, mainSolver, secReactionFunc))
                                         .OrderByDescending(t => t.Item3)
                                         .ToList();
                var first = results.FirstOrDefault(t => t.Item3 <= 1);
                var last = results.Last();

                return first != null ? Tuple.Create(first.Item1, first.Item2) : Tuple.Create(last.Item1, last.Item2);
            }
        }

        /// <summary>
        /// Responsible for selecting appropriate spacing between main beams.
        /// </summary>
        /// <param name="main">Un-safe main beam.</param>
        /// <param name="secondary">Secondary beam.</param>
        /// <param name="mainSolver">Function responsible for solving main beam.</param>
        /// <param name="secReactionFunc">Function responsible for calcultaing reaction of secondary beam (ton).</param>
        /// <returns>new main beam, new secondary beam, Design ratio</returns>
        public static Tuple<Beam, Beam, double> DesignMainBeamForSpacing(Beam main, Beam secondary,
                                                                        Func<Beam, double, StrainingActions> mainSolver,
                                                                        Func<Beam, double> secReactionFunc)
        {
            var designRatio = 1.0;
            var isSafe = false;
            do
            {
                var lessSpan = Database.LedgerLengths.Where(l => l < secondary.Span)
                                                     .OrderByDescending(l => l)
                                                     .FirstOrDefault();
                if (lessSpan == default(double))
                {
                    return Tuple.Create(main, secondary, designRatio);
                }
                else
                {
                    secondary = secondary.Section.AsBeam( lessSpan, secondary.BeamLength).Match(_=>null,b=>b);
                    var secSA = secReactionFunc(secondary);
                    var result = mainSolver(main, secSA).CreateReports(main)
                                                        .IsSafe();

                    isSafe = result.Item1;
                    designRatio = result.Item2;
                }

            } while (!isSafe);
            return Tuple.Create(main, secondary, designRatio);
        }

        /// <summary>
        /// Responsible for selecting span for main beam.
        /// </summary>
        /// <param name="main">Un-safe main beam.</param>
        /// <param name="secondary">Secondary beam.</param>
        /// <param name="mainSolver">Function responsible for solving main beam.</param>
        /// <param name="secReactionFunc">Function responsible for calculating reaction of secondary beam.</param>
        /// <returns></returns>
        public static Tuple<Beam, Beam, double> DesignMainBeamForSpan(Beam main, Beam secondary,
                                                                      Func<Beam, double, StrainingActions> mainSolver,
                                                                      Func<Beam, double> secReactionFunc)
        {
            var designRatio = 1.0;
            var isSafe = false;
            var secSA = secReactionFunc(secondary);
            do
            {
                var lessSpan = Database.LedgerLengths.Where(l => l < main.Span)
                                                     .OrderByDescending(l => l)
                                                     .FirstOrDefault();
                if (lessSpan == default(double))
                {
                    return Tuple.Create(main, secondary, designRatio);
                }
                else
                {
                    main = main.Section.AsBeam( lessSpan, main.BeamLength).Match(_=>null,b=>b);
                    var result = mainSolver(main, secSA).CreateReports(main)
                                                       .IsSafe();
                    isSafe = result.Item1;
                    designRatio = result.Item2;
                }

            } while (!isSafe);
            return Tuple.Create(main, secondary, designRatio);
        }

    }
}
