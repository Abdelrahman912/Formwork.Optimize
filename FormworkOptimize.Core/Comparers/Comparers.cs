using Autodesk.Revit.DB;
using FormworkOptimize.Core.Entities.FormworkModel.Shoring;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Entities.Genetic;
using FormworkOptimize.Core.Extensions;
using FormworkOptimize.Core.Helpers.RevitHelper;
using GeneticSharp.Domain.Chromosomes;
using System;
using static FormworkOptimize.Core.Constants.Database;

namespace FormworkOptimize.Core.Comparers
{
    public static class Comparers
    {
        public static GenericComparer<BinaryChromosomeBase> DesignChromosomeComparer =>
            GenericComparer.Create<BinaryChromosomeBase>((c1, c2) => Math.Abs(Convert.ToDouble(c1.Fitness) - Convert.ToDouble(c2.Fitness)) <= CHROMOSOME_TOLERANCE);

        public static GenericComparer<RevitLedger> RevitLedgerComparer =>
           GenericComparer.Create<RevitLedger>((b1, b2) => b1.IsEqual(b2), ledger => ledger.GetHashCode());

        public static GenericComparer<RevitBeam> RevitBeamComparer =>
           GenericComparer.Create<RevitBeam>((b1, b2) => b1.IsEqual(b2), beam => beam.GetHashCode());

        public static GenericComparer<Line> LineComparer =>
            GenericComparer.Create<Line>((l1, l2) => l1.IsEqual(l2), line => line.GetHash());

        public static GenericComparer<XYZ> XYZComparer =>
            new GenericComparer<XYZ>((p1, p2) => p1.IsEqual(p2), point => point.GetHash());

    }

}
