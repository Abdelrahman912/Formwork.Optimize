using FormworkOptimize.Core.Entities.Genetic;
using FormworkOptimize.Core.Enums;
using System;

namespace FormworkOptimize.Core.Helpers.GeneticHelper
{
    public static class ChromosomeHelper
    {

        private static readonly int _beamsCount = Enum.GetValues(typeof(BeamSectionName)).Length - 1;

        public static CuplockChromosome GenerateChromosomeCuplock(int plywoodCount,int beamSectionsCount,int steelTypeCount)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0), SteelType (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23), SteelType (1)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5), SteelType (1)
            // int[] fractionDigits: 0, 0, 0, 0Enum.GetValues(typeof(BeamSectionName)).Length;

            return new CuplockChromosome(
                new double[] { 0, 0, 0, 0 },
                new double[] { plywoodCount, beamSectionsCount, beamSectionsCount, steelTypeCount },
                new int[] { 2, 5, 5, 1 },
                new int[] { 0, 0, 0, 0 });
        }

        public static EuropeanPropChromosome GenerateChromosomeEuropeanProp(int plywoodCount, int beamSectionsCount,int propsCount)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0), EuropeanPropType (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23), EuropeanPropType (3)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5), EuropeanPropType (2)
            // int[] fractionDigits: 0, 0, 0, 0


            return new EuropeanPropChromosome(
                new double[] { 0, 0, 0, 0 },
                new double[] { plywoodCount, beamSectionsCount, beamSectionsCount, propsCount },
                new int[] { 2, 5, 5, 2 },
                new int[] { 0, 0, 0, 0 });
        }

        public static ShorBraceChromosome GenerateChromosomeShorBrace(int plywoodCount, int beamSectionsCount)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5)
            // int[] fractionDigits: 0, 0, 0

            return new ShorBraceChromosome(
                new double[] { 0, 0, 0 },
                new double[] { plywoodCount, beamSectionsCount, beamSectionsCount, },
                new int[] { 2, 5, 5 },
                new int[] { 0, 0, 0 });
        }

        public static AluminumPropChromosome GenerateChromosomeAlumuinumProp(int plywoodCount, int beamSectionsCount)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5)
            // int[] fractionDigits: 0, 0, 0

            return new AluminumPropChromosome(
                new double[] { 0, 0, 0 },
                new double[] { plywoodCount, beamSectionsCount, beamSectionsCount, },
                new int[] { 2, 5, 5 },
                new int[] { 0, 0, 0 });
        }

        public static FrameChromosome GenerateChromosomeFrame(int plywoodCount, int beamSectionsCount,int framesCount)
        {
            // double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits
            // double[] minValue: PlywoodSection (0), SecondaryBeamSection (0), MainBeamSection (0), FrameType (0)
            // double[] maxValue: PlywoodSection (3), SecondaryBeamSection (23), MainBeamSection (23), FrameType (2)
            // int[] totalBits: PlywoodSection (2), SecondaryBeamSection (5), MainBeamSection (5), FrameType (2)
            // int[] fractionDigits: 0, 0, 0, 0

            return new FrameChromosome(
                new double[] { 0, 0, 0, 0 },
                new double[] { plywoodCount, beamSectionsCount, beamSectionsCount, framesCount },
                new int[] { 2, 5, 5, 2 },
                new int[] { 0, 0, 0, 0 });
        }

    }
}
