using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormworkOptimize.Core.Errors;
using static FormworkOptimize.Core.Errors.Errors;

namespace FormworkOptimize.App.Errors
{
    public static class Errors
    {
        public static GenericError LessThanTwoPlywood =>
            GenericError("There must be more than one Plywood Selected to be included in Genetic Algorithm");

        public static GenericError LessThanTwoBeamSection =>
            GenericError("There must be more than one Beam Section Selected to be included in Genetic Algorithm");

        public static GenericError LessThanTwoLedgers =>
            GenericError("There must be more than one Ledger Selected to be included in Genetic Algorithm");

        public static GenericError LessThanTwoVerticals =>
           GenericError("There must be more than one Cuplock Vertical Selected to be included in Genetic Algorithm");

        public static GenericError LessThanTwoTubes =>
           GenericError("There must be more than one Cuplock Cross Brace Selected to be included in Genetic Algorithm");

        public static GenericError ZeroSteelTypes =>
           GenericError("There must be at least one steel type Selected to be included in Genetic Algorithm");


        public static GenericError LessThanTwoProps =>
          GenericError("There must be more than one Props Type Selected to be included in Genetic Algorithm");

        public static GenericError LessThanTwoShoreBracing =>
         GenericError("There must be more than one Shore Type Selected to be included in Genetic Algorithm");

        public static GenericError LessThanTwoFrameTypes =>
         GenericError("There must be more than one Frame Type Selected to be included in Genetic Algorithm");

    }
}
