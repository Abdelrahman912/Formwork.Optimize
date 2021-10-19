using CSharp.Functional.Constructs;
using CSharp.Functional.Extensions;
using FormworkOptimize.Core.DTOS;
using FormworkOptimize.Core.Helpers.DesignHelper;
using System;
using static FormworkOptimize.Core.Helpers.DesignHelper.DesignHelper;


namespace FormworkOptimize.Core.Entities
{
    public class CuplockDesigner : ICuplockDesigner
    {

        #region Private Fields

        private static readonly Lazy<CuplockDesigner> _instance = new Lazy<CuplockDesigner>(() => new CuplockDesigner(), true);

        #endregion

        #region Properties

        public static CuplockDesigner Instance => _instance.Value;

        #endregion

        #region Constructors

        private CuplockDesigner()
        {
        }

        #endregion

        #region Methods

        public Validation<CuplockDesignOutput> Design(CuplockDesignInput input, 
                                   Func<Beam, double, double, double, StrainingActions> beamSAFunc ,
                                   Func<Beam , double,double,double,double> beamReactionFunc)
        {

            var designDataValid = GetDesignData(input.SlabThickness, input.BeamThickness, input.BeamWidth,
                                           input.PlywoodSection, input.SecondaryBeamSection, input.MainBeamSection,
                                           input.LedgersMainDir, input.LedgersSecondaryDir, input.MainBeamTotalLength,
                                           input.SecondaryBeamTotalLength, beamSAFunc, beamReactionFunc);


            Func<DesignDataDto, Validation<CuplockDesignOutput>> design = designData =>
            {
                //var secDesignFuncs = new List<Func<Beam, Plywood, Func<Beam, Plywood, StrainingActions>, Tuple<Beam, Plywood, double>>>()
                //{
                //    DesignSecondaryBeamForSpacing,
                //    DesignSecondaryBeamForSpan
                //};

                //var mainDesignFuncs = new List<Func<Beam, Beam, Func<Beam, double, StrainingActions>, Func<Beam, double>, Tuple<Beam, Beam, double>>>()
                //{
                //    DesignMainBeamForSpacing,
                //    DesignMainBeamForSpan
                //};

                var maxPlywood = designData.Plywood;
                var secBeam = designData.SecondaryBeam;
                var mainBeam = designData.MainBeam;
                var cuplock = new CuplockShoring(200, input.LedgersMainDir, input.LedgersSecondaryDir, input.SteelType);

                //var secondaryBeamDesign = secBeam.DesignAsSecondary(plywood, designData.SecBeamSolver, secDesignFuncs);
                //secBeam = secondaryBeamDesign.Item1;
                //plywood = secondaryBeamDesign.Item2;


                //var mainBeamDesign = designData.MainBeam.DesignAsMain(secBeam, designData.MainBeamSolver, designData.SecReactionFunc, mainDesignFuncs);
                //mainBeam = mainBeamDesign.Item1;
                //secBeam = mainBeamDesign.Item2;



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
                    var cuplockReport = new DesignReport(Enums.DesignCheckName.NORMAL, cuplock.Capacity, mainReaction);


                    return new CuplockDesignOutput(Tuple.Create(maxPlywood, chosenPlywood, plywoodReports),
                                            Tuple.Create(secBeam, secondaryBeamReports),
                                            Tuple.Create(mainBeam, mainBeamReports),
                                            Tuple.Create(cuplock, cuplockReport));
                });
                
            };
            return designDataValid.Bind(design);
        }

        #endregion

    }
}
