﻿using FormworkOptimize.Core.Entities.Cost.Interfaces;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Infrastructure.Framework.Commons;
using System;
using System.Globalization;
using System.Linq;

namespace FormworkOptimize.Core.Entities.Genetic
{
    /// <summary>
    /// Design chromosome with binary values (0 and 1).
    /// </summary>
    public class DesignChromosome : BinaryChromosomeBase
    {
        #region Private Fields

        private readonly double[] m_minValue;
        private readonly double[] m_maxValue;
        private readonly int[] m_totalBits;
        private readonly int[] m_fractionDigits;
        private readonly string m_originalValueStringRepresentation;

        #endregion

        #region Properties

        public IEvaluateCost FormworkSystem { get; set; }

        public SectionDesignOutput PlywoodDesignOutput { get; set; }

        public SectionDesignOutput SecondaryBeamDesignOutput { get ; set ; }

        public SectionDesignOutput MainBeamDesignOutput { get ; set ; }

        public ShoringDesignOutput ShoringSystemDesignOutput { get ; set ; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GeneticSharp.Domain.Chromosomes.FloatingPointChromosome"/> class.
        /// </summary>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Max value.</param>
        /// <param name="fractionDigits">Decimals.</param>
        public DesignChromosome(double minValue, double maxValue, int fractionDigits)
            : this(new double[] { minValue }, new double[] { maxValue }, new int[] { 32 }, new int[] { fractionDigits })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GeneticSharp.Domain.Chromosomes.FloatingPointChromosome"/> class.
        /// </summary>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Max value.</param>
        /// <param name="totalBits">Total bits.</param>
        /// <param name="fractionDigits">Decimals.</param>
        public DesignChromosome(double minValue, double maxValue, int totalBits, int fractionDigits)
            : this(new double[] { minValue }, new double[] { maxValue }, new int[] { totalBits }, new int[] { fractionDigits })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GeneticSharp.Domain.Chromosomes.FloatingPointChromosome"/> class.
        /// </summary>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Max value.</param>
        /// <param name="totalBits">Total bits.</param>
        /// <param name="fractionDigits">Fraction digits.</param>
        public DesignChromosome(double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits)
            : this(minValue, maxValue, totalBits, fractionDigits, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GeneticSharp.Domain.Chromosomes.FloatingPointChromosome"/> class.
        /// </summary>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Max value.</param>
        /// <param name="totalBits">Total bits.</param>
        /// <param name="fractionDigits">Fraction digits.</param>
        /// /// <param name="geneValues">Gene values.</param>
        public DesignChromosome(double[] minValue, double[] maxValue, int[] totalBits, int[] fractionDigits, double[] geneValues)
            : base(totalBits.Sum())
        {
            m_minValue = minValue;
            m_maxValue = maxValue;
            m_totalBits = totalBits;
            m_fractionDigits = fractionDigits;

            // If values are not supplied, create random values
            if (geneValues == null)
            {
                geneValues = new double[minValue.Length];
                var rnd = RandomizationProvider.Current;

                for (int i = 0; i < geneValues.Length; i++)
                {
                    geneValues[i] = rnd.GetDouble(minValue[i], maxValue[i]);
                }
            }

            m_originalValueStringRepresentation = String.Join(
                "",
                BinaryStringRepresentation.ToRepresentation(
                geneValues,
                totalBits,
                fractionDigits));

            CreateGenes();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the new.
        /// </summary>
        /// <returns>The new.</returns>
        public override IChromosome CreateNew()
        {
            return new DesignChromosome(m_minValue, m_maxValue, m_totalBits, m_fractionDigits);
        }

        /// <summary>
        /// Generates the gene.
        /// </summary>
        /// <returns>The gene.</returns>
        /// <param name="geneIndex">Gene index.</param>
        public override Gene GenerateGene(int geneIndex)
        {
            return new Gene(Convert.ToInt32(m_originalValueStringRepresentation[geneIndex].ToString(), CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Converts the chromosome to the floating point representation.
        /// </summary>
        /// <returns>The floating point.</returns>
        public double ToFloatingPoint()
        {
            return EnsureMinMax(
                BinaryStringRepresentation.ToDouble(ToString()),
                0);

        }

        /// <summary>
        /// Converts the chromosome to the floating points representation.
        /// </summary>
        /// <returns>The floating points.</returns>
        public double[] ToFloatingPoints()
        {
            return BinaryStringRepresentation
                .ToDouble(ToString(), m_totalBits, m_fractionDigits)
                .Select(EnsureMinMax)
                .ToArray();
        }

        private double EnsureMinMax(double value, int index)
        {
            if (value < m_minValue[index])
            {
                return m_minValue[index];
            }

            if (value > m_maxValue[index])
            {
                return m_maxValue[index];
            }

            return value;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:GeneticSharp.Domain.Chromosomes.FloatingPointChromosome"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:GeneticSharp.Domain.Chromosomes.FloatingPointChromosome"/>.</returns>
        public override string ToString()
        {
            return String.Join("", GetGenes().Select(g => g.Value.ToString()).ToArray());
        }

        public override int GetHashCode()
        {
            var fitnessInt =(int) (Fitness * 1000);
            return fitnessInt.GetHashCode();
        }

        #endregion
    }

}