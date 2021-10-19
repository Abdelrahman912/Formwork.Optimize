using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.DTOS;
using FormworkOptimize.Core.Helpers.DesignHelper;
using System;
using static FormworkOptimize.Core.Helpers.DesignHelper.DesignHelper;

namespace FormworkOptimize.Core.Entities
{
    public class FrameDesigner : IFrameDesigner
    {

        #region Private Fields

        private static readonly Lazy<FrameDesigner> _instance = new Lazy<FrameDesigner>(() => new FrameDesigner(), true);

        #endregion

        #region Properties

        public static FrameDesigner Instance => _instance.Value;

        #endregion

        #region Constructors

        private FrameDesigner() { }
       

        #endregion

        #region Methods

        public Validation<FrameDesignOutput> Design(FrameDesignInput input, 
                                        Func<Beam, double, double, double, StrainingActions> beamSolver, 
                                        Func<Beam, double, double, double, double> beamReactionFunc)
        {
            var designDataValid = GetDesignData(input.SlabThickness, input.BeamThickness, input.BeamWidth,
                                          input.PlywoodSection, input.SecondaryBeamSection, input.MainBeamSection,
                                          input.Spacing, 100, input.MainBeamTotalLength,
                                          input.SecondaryBeamTotalLength, beamSolver, beamReactionFunc);

            Func<DesignDataDto, Validation<FrameDesignOutput>> design = designData =>
            {
                var maxPlywood = designData.Plywood;
                var secBeam = designData.SecondaryBeam;
                var mainBeam = designData.MainBeam;
                var frame = new FrameSystem(input.FrameType, input.Spacing);

                //Output.
                var chosenPlywoodValid = input.SecondaryBeamSpacing.AsNewPlywood(maxPlywood);
                return chosenPlywoodValid.Map(chosenPlywood =>
                {
                    var plywoodReports = chosenPlywood.GetStrainingActions(Math.Max(designData.WeightPerAreaSlab, designData.WeightPerAreaBeam))
                                                .CreateReports(maxPlywood);
                    var secondaryBeamReports = designData.SecBeamSolver(secBeam, maxPlywood)
                                                         .CreateReports(secBeam);
                    var secReaction = designData.SecReactionFunc(secBeam);
                    var mainBeamReports = designData.MainBeamSolver(mainBeam, secReaction)
                                                    .CreateReports(mainBeam);
                    var mainReaction = designData.MainReactionFunc(mainBeam, secReaction);

                    var frameReport = new DesignReport(Enums.DesignCheckName.NORMAL, frame.FrameType.Capacity, mainReaction * 2);


                    return new FrameDesignOutput(Tuple.Create(maxPlywood, chosenPlywood, plywoodReports),
                                                 Tuple.Create(secBeam, secondaryBeamReports),
                                                 Tuple.Create(mainBeam, mainBeamReports),
                                                 Tuple.Create(frame as FrameShoring, frameReport));
                });
               
            };

            return designDataValid.Bind(design);

        } 

        #endregion

    }
}
