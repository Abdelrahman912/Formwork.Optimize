using FormworkOptimize.Core.DTOS;
using static FormworkOptimize.Core.Helpers.DesignHelper.DesignHelper;
using System;
using FormworkOptimize.Core.Helpers.DesignHelper;
using CSharp.Functional.Extensions;
using CSharp.Functional.Constructs;

namespace FormworkOptimize.Core.Entities
{
    public class EuropeanPropDesigner : IEuropeanPropDesigner
    {

        #region Private Fields

        private static readonly Lazy<EuropeanPropDesigner> _instance = new Lazy<EuropeanPropDesigner>(() => new EuropeanPropDesigner(), true);

        #endregion

        #region Properties

        public static EuropeanPropDesigner Instance => _instance.Value;

        #endregion

        #region Constructors

        private EuropeanPropDesigner() { }

        #endregion

        #region Methods

        public Validation<PropDesignOutput> Design(EuropeanPropDesignInput input, 
                                       Func<Beam, double, double, double, StrainingActions> beamSolver, 
                                       Func<Beam, double, double, double, double> beamReactionFunc)
        {
            var designDataValid = GetDesignData(input.SlabThickness, input.BeamThickness, input.BeamWidth,
                                           input.PlywoodSection, input.SecondaryBeamSection, input.MainBeamSection,
                                           input.MainSpacing, input.SecondarySpacing, input.MainBeamTotalLength,
                                           input.SecondaryBeamTotalLength, beamSolver, beamReactionFunc);


            Func<DesignDataDto, Validation<PropDesignOutput>> design = designData  =>
            {
                var maxPlywood = designData.Plywood;
                var secBeam = designData.SecondaryBeam;
                var mainBeam = designData.MainBeam;
                var prop = new EuropeanPropSystem(input.EuropeanPropType, input.MainSpacing, input.SecondarySpacing);

                //Output.
                var chosenPlywoodValid = input.SecondaryBeamSpacing.AsNewPlywood(maxPlywood);
              return  chosenPlywoodValid.Map(chosenPlywood =>
                {
                    var plywoodReports = maxPlywood.GetStrainingActions(Math.Max(designData.WeightPerAreaSlab, designData.WeightPerAreaBeam))
                                           .CreateReports(maxPlywood);
                    var secondaryBeamReports = designData.SecBeamSolver(secBeam, maxPlywood)
                                                         .CreateReports(secBeam);
                    var secReaction = designData.SecReactionFunc(secBeam);
                    var mainBeamReports = designData.MainBeamSolver(mainBeam, secReaction)
                                                    .CreateReports(mainBeam);
                    var mainReaction = designData.MainReactionFunc(mainBeam, secReaction);

                    var propReport = new DesignReport(Enums.DesignCheckName.NORMAL, prop.PropType.Capacity, mainReaction);


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
