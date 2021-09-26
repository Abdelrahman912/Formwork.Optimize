using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.DTOS;
using FormworkOptimize.Core.Helpers.DesignHelper;
using System;
using static FormworkOptimize.Core.Helpers.DesignHelper.DesignHelper;

namespace FormworkOptimize.Core.Entities.Designer
{
    public class ShoreBraceDesigner : IShoreBraceDesigner
    {

        #region Private Fields
        private static readonly Lazy<ShoreBraceDesigner> _instance = new Lazy<ShoreBraceDesigner>(() => new ShoreBraceDesigner(), true);
        #endregion

        #region Properties
        public static ShoreBraceDesigner Instance => _instance.Value;

        #endregion

        #region Constructors
        private ShoreBraceDesigner() { }

        #endregion

        #region Methods

        public Validation<FrameDesignOutput> Design(ShoreBraceDesignInput input, 
                                        Func<Beam, double, double, double, StrainingActions> beamSolver, 
                                        Func<Beam, double, double, double, double> beamReactionFunc)
        {
            var designDataValid = GetDesignData(input.SlabThickness, input.BeamThickness, input.BeamWidth,
                                           input.PlywoodSection, input.SecondaryBeamSection, input.MainBeamSection,
                                           input.Spacing, 120, input.MainBeamTotalLength,
                                           input.SecondaryBeamTotalLength, beamSolver, beamReactionFunc);

            Func<DesignDataDto, FrameDesignOutput> design = designData =>
             {
                 var plywood = designData.Plywood;
                 var secBeam = designData.SecondaryBeam;
                 var mainBeam = designData.MainBeam;
                 var frame = new ShoreBraceSystem(input.Spacing);

                 //Output.
                 var plywoodReports = plywood.GetStrainingActions(Math.Max(designData.WeightPerAreaSlab, designData.WeightPerAreaBeam))
                                             .CreateReports(plywood);
                 var secondaryBeamReports = designData.SecBeamSolver(secBeam, plywood)
                                                      .CreateReports(secBeam);
                 var secReaction = designData.SecReactionFunc(secBeam);
                 var mainBeamReports = designData.MainBeamSolver(mainBeam, secReaction)
                                                 .CreateReports(mainBeam);
                 var mainReaction = designData.MainReactionFunc(mainBeam, secReaction);

                 var frameReport = new DesignReport(Enums.DesignCheckName.NORMAL, frame.Capacity, mainReaction * 2);

                 return new FrameDesignOutput(Tuple.Create(plywood, plywoodReports),
                                              Tuple.Create(secBeam, secondaryBeamReports),
                                              Tuple.Create(mainBeam, mainBeamReports),
                                              Tuple.Create(frame as FrameShoring, frameReport));
             };

            return designDataValid.Map(design);
           
        }

        #endregion

    }
}
