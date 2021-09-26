using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.Core.Extensions
{
    public static class DoubleExtension
    {
        /// <summary>
        /// Get values between start value and end value by interval.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static List<double> GetAtInterval(this double start ,double end , double interval)
        {
            if (end < start)
                return new List<double>();
            var n =(int) ((end - start) / interval);
            return Enumerable.Range(1, n + 1)
                             .Select(index => start + index * interval)
                             .ToList();
        }

        public static double Round(this double number, int digits) =>
            Math.Round(number, digits);

        public static double FeetToMeter(this double number) =>
            UnitUtils.ConvertFromInternalUnits(number, DisplayUnitType.DUT_METERS);

        public static double MmToFeet(this double numberInMm) =>
          UnitUtils.ConvertToInternalUnits(numberInMm, DisplayUnitType.DUT_MILLIMETERS);

        public static double CmToFeet(this double numberInCm) =>
            UnitUtils.ConvertToInternalUnits(numberInCm, DisplayUnitType.DUT_CENTIMETERS);

        public static double FeetToCm(this double numberInFeet) =>
            UnitUtils.ConvertFromInternalUnits(numberInFeet, DisplayUnitType.DUT_CENTIMETERS);

        public static double FeetSquareToMeterSquare(this double squareFeet) =>
            UnitUtils.Convert(squareFeet,DisplayUnitType.DUT_SQUARE_FEET, DisplayUnitType.DUT_SQUARE_METERS);

        /// <summary>
        /// Rounded the input to the nearest 5 cm by less.
        /// </summary>
        /// <param name="unRounded">The un rounded value in cm.</param>
        /// <returns>Rounded value in cm</returns>
        public static double ToNearest5Cm(this double unRounded)
        {
            var div = (int)(unRounded / 5);
            return div * 5;
        }

    }
}
