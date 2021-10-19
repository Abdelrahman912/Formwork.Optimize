using FormworkOptimize.Core.Entities.Genetic;
using FormworkOptimize.Core.Enums;
using System;
using System.Linq;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.Core.Helpers.GeneticHelper
{
    public static class ChromosomeHelper
    {

        private static readonly int _beamsCount = Enum.GetValues(typeof(BeamSectionName)).Length - 1;

        public static CuplockChromosome GenerateChromosomeCuplock(int plywoodMaxIndex, int beamSectionsMaxIndex, int steelTypeMaxIndex, int ledgersMaxIndex)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0), SteelType (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23), SteelType (1)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5), SteelType (1)
            // int[] fractionDigits: 0, 0, 0, 0Enum.GetValues(typeof(BeamSectionName)).Length;


            var numbers = new int[] { 1, 2, 3, 4, 5 };


            //Max Number = 2^bits-1
            var beamBits = numbers.First(num => Math.Pow(2, num) - 1 >= beamSectionsMaxIndex);
            var plyBits = numbers.First(num => Math.Pow(2, num) - 1 >= plywoodMaxIndex);
            var ledgersBits = numbers.First(num => Math.Pow(2, num) - 1 >= ledgersMaxIndex);

            return new CuplockChromosome(
                new double[] { 0, 0, 0, 0, 0, 0, 0, 0,0 },
                new double[] { plywoodMaxIndex, beamSectionsMaxIndex, beamSectionsMaxIndex, steelTypeMaxIndex, ledgersMaxIndex, ledgersMaxIndex, 15, 15,AvailableSecBeamSpacings.Count-1 },
                new int[] { plyBits, beamBits, beamBits, 1, ledgersBits, ledgersBits, 4, 4,3 },
                new int[] { 0, 0, 0, 0, 0, 0, 0, 0,0 });
        }

        public static EuropeanPropChromosome GenerateChromosomeEuropeanProp(int plywoodCount, int beamSectionsCount, int propsCount)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0), EuropeanPropType (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23), EuropeanPropType (3)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5), EuropeanPropType (2)
            // int[] fractionDigits: 0, 0, 0, 0
            //24 => Props Spacing Values From 60 to 300 cm

            var numbers = new int[] { 1, 2, 3, 4, 5 };


            //Max Number = 2^bits-1
            var beamBits = numbers.First(num => Math.Pow(2, num) - 1 >= beamSectionsCount);
            var plyBits = numbers.First(num => Math.Pow(2, num) - 1 >= plywoodCount);
            var propsBits = numbers.First(num => Math.Pow(2, num) - 1 >= propsCount);

            return new EuropeanPropChromosome(
                new double[] { 0, 0, 0, 0, 0, 0, 0, 0,0 },
                new double[] { plywoodCount, beamSectionsCount, beamSectionsCount, propsCount, 24, 24, 15, 15,AvailableSecBeamSpacings.Count-1 },
                new int[] { plyBits, beamBits, beamBits, propsBits, 5, 5, 4, 4,3 },
                new int[] { 0, 0, 0, 0, 0, 0, 0, 0,0 });
        }

        public static ShorBraceChromosome GenerateChromosomeShorBrace(int plywoodCount, int beamSectionsCount, int crossBracesCount)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5)
            // int[] fractionDigits: 0, 0, 0

            var numbers = new int[] { 1, 2, 3, 4, 5 };


            //Max Number = 2^bits-1
            var beamBits = numbers.First(num => Math.Pow(2, num) - 1 >= beamSectionsCount);
            var plyBits = numbers.First(num => Math.Pow(2, num) - 1 >= plywoodCount);
            var crossBits = numbers.First(num => Math.Pow(2, num) - 1 >= crossBracesCount);

            return new ShorBraceChromosome(
                new double[] { 0, 0, 0, 0, 0, 0, 0,0 },
                new double[] { plywoodCount, beamSectionsCount, beamSectionsCount, 14, crossBracesCount, 15, 15,AvailableSecBeamSpacings.Count-1 },
                new int[] { plyBits, beamBits, beamBits, 4, crossBits, 4, 4,3 },
                new int[] { 0, 0, 0, 0, 0, 0, 0,0 });
        }

        public static AluminumPropChromosome GenerateChromosomeAlumuinumProp(int plywoodCount, int beamSectionsCount)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5)
            // int[] fractionDigits: 0, 0, 0

            var numbers = new int[] { 1, 2, 3, 4, 5 };


            //Max Number = 2^bits-1
            var beamBits = numbers.First(num => Math.Pow(2, num) - 1 >= beamSectionsCount);
            var plyBits = numbers.First(num => Math.Pow(2, num) - 1 >= plywoodCount);

            return new AluminumPropChromosome(
                new double[] { 0, 0, 0, 0, 0, 0, 0 ,0},
                new double[] { plywoodCount, beamSectionsCount, beamSectionsCount, 24, 24, 15, 15,AvailableSecBeamSpacings.Count-1 },
                new int[] { plyBits, beamBits, beamBits, 5, 5, 4, 4 ,3},
                new int[] { 0, 0, 0, 0, 0, 0, 0,0 });
        }

        public static FrameChromosome GenerateChromosomeFrame(int plywoodCount, int beamSectionsCount, int framesCount, int crossBracesCount)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0), FrameType (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23), FrameType (2)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5), FrameType (2)
            // int[] fractionDigits: 0, 0, 0, 0

            var numbers = new int[] { 1, 2, 3, 4, 5 };


            //Max Number = 2^bits-1
            var beamBits = numbers.First(num => Math.Pow(2, num) - 1 >= beamSectionsCount);
            var plyBits = numbers.First(num => Math.Pow(2, num) - 1 >= plywoodCount);
            var frameBits = numbers.First(num => Math.Pow(2, num) - 1 >= beamSectionsCount);
            var crossBits = numbers.First(num => Math.Pow(2, num) - 1 >= crossBracesCount);

            return new FrameChromosome(
                new double[] { 0, 0, 0, 0,0,0,0,0 },
                new double[] { plywoodCount, beamSectionsCount, beamSectionsCount, framesCount, crossBracesCount, 15, 15,AvailableSecBeamSpacings.Count-1 },
                new int[] { plyBits, beamBits, beamBits, frameBits , crossBits, 4, 4,3 },
                new int[] { 0, 0, 0, 0,0,0,0,0 });
        }

    }
}
