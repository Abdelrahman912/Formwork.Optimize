using FormworkOptimize.Core.DTOS;
using static FormworkOptimize.Core.Helpers.DesignHelper.DesignHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormworkOptimize.Core.Helpers.DesignHelper;
using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;

namespace FormworkOptimize.Core.Entities
{
    public class AluPropDesigner : IAluPropDesigner
    {

        #region Private Fields

        private static readonly Lazy<AluPropDesigner> _instance = new Lazy<AluPropDesigner>(() => new AluPropDesigner(), true);

        #endregion

        #region Properties

        public static AluPropDesigner Instance = _instance.Value;

        #endregion

        #region Constructors

        private AluPropDesigner() { }

        #endregion

        #region Methods

        public Validation<PropDesignOutput> Design(AluPropDesignInput input, Func<Beam, double, double, double, StrainingActions> beamSolver, Func<Beam, double, double, double, double> beamReactionFunc)
        {
            var designDataValid = GetDesignData(input.SlabThickness, input.BeamThickness, input.BeamWidth,
                                           input.PlywoodSection, input.SecondaryBeamSection, input.MainBeamSection,
                                           input.MainSpacing, input.SecondarySpacing, input.MainBeamTotalLength,
                                           input.SecondaryBeamTotalLength, beamSolver, beamReactionFunc);

            Func<DesignDataDto, Validation<PropDesignOutput>> design = designData =>
            {
                var maxPlywood = designData.Plywood;
                var secBeam = designData.SecondaryBeam;
                var mainBeam = designData.MainBeam;
                var prop = new AluPropSystem(input.MainSpacing, input.SecondarySpacing);

                //Output.
                var chosenPlywoodValid = input.SecondaryBeamSpacing.AsNewPlywood(maxPlywood);

               return chosenPlywoodValid.Map(chosenPlywood =>
                {
                    var plywoodReports = chosenPlywood.GetStrainingActions(Math.Max(designData.WeightPerAreaSlab, designData.WeightPerAreaBeam))
                                                .CreateReports(chosenPlywood);

                    var secondaryBeamReports = designData.SecBeamSolver(secBeam, chosenPlywood)
                                                         .CreateReports(secBeam);
                    var secReaction = designData.SecReactionFunc(secBeam);
                    var mainBeamReports = designData.MainBeamSolver(mainBeam, secReaction)
                                                    .CreateReports(mainBeam);
                    var mainReaction = designData.MainReactionFunc(mainBeam, secReaction);

                    var propReport = new DesignReport(Enums.DesignCheckName.NORMAL, prop.Capacity, mainReaction);


                    return new PropDesignOutput(Tuple.Create(maxPlywood, chosenPlywood, plywoodReports),
                                                 Tuple.Create(secBeam, secondaryBeamReports),
                                                 Tuple.Create(mainBeam, mainBeamReports),
                                                 Tuple.Create(prop as PropShoring, propReport));
                });

              
            };

            return designDataValid.Bind(design);
           
        }

        #endregion

    }
}
