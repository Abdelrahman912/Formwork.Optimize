using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Helpers.DesignHelper;
using FormworkOptimize.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormworkOptimize.Core.Extensions;

namespace FormworkOptimize.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //var plywoodSection = DBHelper.GetPlywoodSection(PlywoodSectionName.BETOFILM_18MM);
            //var weight = DesignHelper.CalculateAreaWeight(50);
            //var beam = new Beam(DBHelper.GetBeamSection(BeamSectionName.TIMBER_2X4), 90, 160);
            //var maxReaction = EmpiricalBeamSolver.SolveForDeflection(beam, 0.265, 0.836, 0.3);
            var name = typeof(Test).FullName;
        }

    }

    public class Test
    {

    }
}
