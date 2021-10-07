using FormworkOptimize.Core.Entities;
using FormworkOptimize.Core.Entities.Cost;
using FormworkOptimize.Core.Entities.FormworkModel.SuperStructure;
using FormworkOptimize.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FormworkOptimize.Core.Constants
{
    public static class Database
    {

        #region Designer

        public const double CHROMOSOME_TOLERANCE = 0.0000001;

        private static readonly List<BeamSection> _beamSections = new List<BeamSection>()
        {
            new BeamSection(102,4642.13,464.21,0.3,0.085,1.1,0.5,85,BeamSectionName.TIMBER_H20),
            new BeamSection(7.95,141.5,24,0.3,2.1,6,0.5,2100,BeamSectionName.ACROW_BEAM_S12),
            new BeamSection(15.29,454.18,56.07,0.3,1.43,2.7,0.8,700,BeamSectionName.ALUMINUM_BEAM),
            //new BeamSection(12,412,82.4,0.3,1.39,10.08,1.15,2100,BeamSectionName.SOLDIER_10),
            new BeamSection(10.10,194.4,32.4,0.3,1.4,5.04,0.45,2100,BeamSectionName.S_BEAM_12),
            new BeamSection(204.8,9284.26,928.42,0.3,0.09,2.2,1,85,BeamSectionName.DOUBLE_TIMBER_H20),
            new BeamSection(15.9,283,48,0.3,2.1,12,1,2100,BeamSectionName.DOUBLE_ACROW_BEAM_S12),
            new BeamSection(30.58,908.36,112.14,0.3,1.43,5.4,1.6,700,BeamSectionName.ALUMINUM_BEAM_DOUBLE),
            //new BeamSection(24,2169,216.9,0.3,1.392,20.16,3.02,2100,BeamSectionName.DOUBLE_SOLDIER_10),
            new BeamSection(50,416.67,83.3,0.3,0.085,0.5,0.07,85,BeamSectionName.TIMBER_2X4),
            new BeamSection(62.5,813.8,130.2,0.3,0.085,0.63,0.11,85,BeamSectionName.TIMBER_2X5),
            new BeamSection(75,1406.25,187.5,0.3,0.085,0.75,0.16,85,BeamSectionName.TIMBER_2X6),
            new BeamSection(100,3333.33,333.33,0.3,0.085,1,0.28,85,BeamSectionName.TIMBER_2X8),
            //new BeamSection(16.8,728,121.4,0.3,1.39,14.11,1.69,2100,BeamSectionName.SOLDIER_12),
            new BeamSection(15,263.67,70.31,0.3,0.085,0.15,0.06,85,BeamSectionName.TIMBER_3X3),
            new BeamSection(93.75,1220.7,195.31,0.3,0.085,0.94,0.17,85,BeamSectionName.TIMBER_3X5),
            new BeamSection(112.5,2109.37,281.25,0.3,0.085,1.13,0.24,85,BeamSectionName.TIMBER_3X6),
            new BeamSection(100,833.33,166.67,0.3,0.085,1,0.14,85,BeamSectionName.TIMBER_4X4),
            //new BeamSection(19.6,1210,172.8,0.3,1.39,16.46,2.41,2100,BeamSectionName.SOLDIER_14),
            new BeamSection(125,1627.6,260.4,0.3,0.085,1.25,0.22,85,BeamSectionName.DOUBLE_TIMBER_2X5),
            new BeamSection(150,2812.5,375,0.3,0.085,1.5,0.32,85,BeamSectionName.DOUBLE_TIMBER_2X6),
            new BeamSection(225,4218.74,562.5,0.3,0.085,2.25,0.48,85,BeamSectionName.DOUBLE_TIMBER_3X6),
            new BeamSection(200,6666.66,666.66,0.3,0.085,2,0.57,85,BeamSectionName.DOUBLE_TIMBER_2X8),
            //new BeamSection(24,1850,232,0.3,1.392,20.16,3.23,2100,BeamSectionName.SOLDIER_16)
        };

        private static readonly List<PlywoodSection> _plywoodSections = new List<PlywoodSection>()
        {
            new PlywoodSection(180,48.6,54,0.082,0.662,0.044,42.8,18,PlywoodSectionName.BETOFILM_18MM),
            new PlywoodSection(180,48.6,54,0.082,0.662,0.044,42.8,17.5,PlywoodSectionName.COFIFORM_PLUS_1705MM),
            new PlywoodSection(190,57.2,60.2,0.061,0.393,0.0366,32,19,PlywoodSectionName.DOUGLAS_FIR_19MM),
            new PlywoodSection(180,48.6,54,0.108,1.545,0.0582,49.8,18,PlywoodSectionName.WISAFORM_BIRCH_18MM)
        };

        private static readonly List<BeamLengths> _beamLengths = new List<BeamLengths>()
        {
            new BeamLengths(BeamSectionName.ACROW_BEAM_S12,new List<double>(){200, 250, 300, 350, 400 }),
            new BeamLengths(BeamSectionName.DOUBLE_ACROW_BEAM_S12, new List<double>(){200, 250, 300, 350, 400 }),
            new BeamLengths(BeamSectionName.ALUMINUM_BEAM, new List<double>(){100, 150, 200, 250, 300, 350, 400, 450, 500, 600 }),
            new BeamLengths(BeamSectionName.ALUMINUM_BEAM_DOUBLE, new List<double>(){100, 150, 200, 250, 300, 350, 400, 450, 500, 600 }),
            new BeamLengths(BeamSectionName.S_BEAM_12, new List<double>(){125, 140, 150, 200, 225, 250, 275, 300, 325, 350, 380, 400, 450, 480, 500, 525 }),
            new BeamLengths(BeamSectionName.TIMBER_H20, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.TIMBER_2X4, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.TIMBER_2X5, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.TIMBER_2X6, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.TIMBER_2X8, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.TIMBER_3X3, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.TIMBER_3X5, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.TIMBER_3X6, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.TIMBER_4X4, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.DOUBLE_TIMBER_H20, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.DOUBLE_TIMBER_2X5, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.DOUBLE_TIMBER_2X6, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.DOUBLE_TIMBER_2X8, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            new BeamLengths(BeamSectionName.DOUBLE_TIMBER_3X6, new List<double>(){180, 190, 250, 290, 330, 360, 390, 450, 490, 590 }),
            //new BeamLengths(BeamSectionName.SOLDIER_10,new List<double>(){117,147,178,239,300,361,422,483,544,605}),
            //new BeamLengths(BeamSectionName.SOLDIER_12,new List<double>(){117,147,178,239,300,361,422,483,544,605}),
            //new BeamLengths(BeamSectionName.SOLDIER_14,new List<double>(){117,147,178,239,300,361,422,483,544,605}),
            //new BeamLengths(BeamSectionName.SOLDIER_16,new List<double>(){117,147,178,239,300,361,422,483,544,605}),
            //new BeamLengths(BeamSectionName.DOUBLE_SOLDIER_10,new List<double>(){117,147,178,239,300,361,422,483,544,605}),
        };





        private static readonly List<TableLayout> _tablesLayout = new List<TableLayout>()
        {
            //Dokaflex table 27mm.
            new TableLayout(TableSystemType.DOKA_FLEX_TABLE_27MM_205X4,225,150,56.3,77.6,47.5),
            new TableLayout(TableSystemType.DOKA_FLEX_TABLE_27MM_205X5,280,150,46.7,100,47.5),
            new TableLayout(TableSystemType.DOKA_FLEX_TABLE_27MM_2X4,225,145,56.3,77.6,25),
            new TableLayout(TableSystemType.DOKA_FLEX_TABLE_27MM_205X4,280,145,46.7,100,25),
            //Dokaflex table 21mm.
            new TableLayout(TableSystemType.DOKA_FLEX_TABLE_21MM_205X4,225,150,56.3,77.6,47.5),
            new TableLayout(TableSystemType.DOKA_FLEX_TABLE_21MM_205X5,300,150,50,90,47.5),
            new TableLayout(TableSystemType.DOKA_FLEX_TABLE_21MM_2X4,225,145,56.3,77.6,25),
            new TableLayout(TableSystemType.DOKA_FLEX_TABLE_21MM_205X4,300,145,50,90,25),
        };

        private static readonly List<FrameSystemType> _framesCapacity = new List<FrameSystemType>()
        {
            new FrameSystemType(FrameTypeName.H209MM,4.5),
            new FrameSystemType(FrameTypeName.M205MM,3.8),
            new FrameSystemType(FrameTypeName.L200MM,2.8),
        };

        private static readonly List<EuropeanPropType> _europeanPropTypes = new List<EuropeanPropType>()
        {
            new EuropeanPropType(EuropeanPropTypeName.E30,3),
            new EuropeanPropType(EuropeanPropTypeName.E35,3),
            new EuropeanPropType(EuropeanPropTypeName.D40,2),
            new EuropeanPropType(EuropeanPropTypeName.D45,2)
        };

        public static TableLayout GetTableLayout(TableSystemType tableType) =>
            _tablesLayout.First(table => table.Name == tableType);

        public static FrameSystemType GetFrameCapacity(FrameTypeName frameTypeName) =>
            _framesCapacity.First(frame => frame.Name == frameTypeName);

        public static EuropeanPropType GetEuropeanPropType(EuropeanPropTypeName propTypeName) =>
            _europeanPropTypes.First(prop => prop.Name == propTypeName);

        public static BeamSection GetBeamSection(BeamSectionName sectionName) =>
            _beamSections.First(s => s.SectionName == sectionName);

        public static PlywoodSection GetPlywoodSection(PlywoodSectionName sectionName) =>
            _plywoodSections.First(s => s.SectionName == sectionName);

        public static List<double> GetBeamLengths(BeamSectionName sectionName) =>
            _beamLengths.First(s => s.SectionName == sectionName).BeamLengthsList;

        public static List<double> GetBeamLengths(RevitBeamSectionName revitSectionName)
        {
            var sectionName = BeamSectionName.TIMBER_H20;
            switch (revitSectionName)
            {
                case RevitBeamSectionName.ACROW_BEAM_S12:
                    sectionName = BeamSectionName.ACROW_BEAM_S12;
                    break;
                case RevitBeamSectionName.DOUBLE_ACROW_BEAM_S12:
                    sectionName = BeamSectionName.DOUBLE_ACROW_BEAM_S12;
                    break;
                case RevitBeamSectionName.ALUMINUM_BEAM:
                    sectionName = BeamSectionName.ALUMINUM_BEAM;
                    break;
                case RevitBeamSectionName.ALUMINUM_BEAM_DOUBLE:
                    sectionName = BeamSectionName.ALUMINUM_BEAM_DOUBLE;
                    break;
                case RevitBeamSectionName.S_BEAM_12:
                    sectionName = BeamSectionName.S_BEAM_12;
                    break;
                //case RevitBeamSectionName.S_BEAM_16:
                //    sectionName = BeamSectionName.SOLDIER_16;
                //    break;
                case RevitBeamSectionName.TIMBER_H20:
                    sectionName = BeamSectionName.TIMBER_H20;
                    break;
                case RevitBeamSectionName.TIMBER_2X4:
                    sectionName = BeamSectionName.TIMBER_2X4;
                    break;
                case RevitBeamSectionName.TIMBER_2X5:
                    sectionName = BeamSectionName.TIMBER_2X5;
                    break;
                case RevitBeamSectionName.TIMBER_2X6:
                    sectionName = BeamSectionName.TIMBER_2X6;
                    break;
                case RevitBeamSectionName.TIMBER_2X8:
                    sectionName = BeamSectionName.TIMBER_2X8;
                    break;
                case RevitBeamSectionName.TIMBER_3X3:
                    sectionName = BeamSectionName.TIMBER_3X3;
                    break;
                case RevitBeamSectionName.TIMBER_3X5:
                    sectionName = BeamSectionName.TIMBER_3X5;
                    break;
                case RevitBeamSectionName.TIMBER_3X6:
                    sectionName = BeamSectionName.TIMBER_3X6;
                    break;
                case RevitBeamSectionName.TIMBER_4X4:
                    sectionName = BeamSectionName.TIMBER_4X4;
                    break;
                case RevitBeamSectionName.DOUBLE_TIMBER_H20:
                    sectionName = BeamSectionName.DOUBLE_TIMBER_H20;
                    break;
                case RevitBeamSectionName.DOUBLE_TIMBER_2X5:
                    sectionName = BeamSectionName.DOUBLE_TIMBER_2X5;
                    break;
                case RevitBeamSectionName.DOUBLE_TIMBER_2X6:
                    sectionName = BeamSectionName.DOUBLE_TIMBER_2X6;
                    break;
                case RevitBeamSectionName.DOUBLE_TIMBER_3X6:
                    sectionName = BeamSectionName.DOUBLE_TIMBER_3X6;
                    break;
                case RevitBeamSectionName.DOUBLE_TIMBER_2X8:
                    sectionName = BeamSectionName.DOUBLE_TIMBER_2X8;
                    break;
            }
            return _beamLengths.First(s => s.SectionName == sectionName).BeamLengthsList;
        }


        /// <summary>
        /// Collection of all Ledger lengths (cm) available in market.
        /// </summary>
        public static List<double> LedgerLengths = new List<double>() { 60, 90, 120, 150, 180, 210, 240, 270, 300 };

        /// <summary>
        /// Collection of All Cuplock vertiacl element lengths (cm) available in market.
        /// </summary>
        public static List<double> CuplockVerticalLengths = new List<double>() { 50, 100, 150, 200, 250, 300 };

        /// <summary>
        /// Collections of all Cross brace lengths (cm) available in market.
        /// </summary>
        public static List<double> CuplockCrossBraceLengths = new List<double>() { 100, 150, 200, 250, 300, 350, 400, 450, 500, 600 };

        /// <summary>
        /// List of tuples contains (Lcr (cm) , Allowable buckling load (ton)) for steel 52.
        /// </summary>
        private static List<Tuple<double, double>> steel52AllowableLoad = new List<Tuple<double, double>>()
        {
            Tuple.Create(100.0,7.054),
            Tuple.Create(150.0,4.713),
            Tuple.Create(200.0,3.061)
        };

        /// <summary>
        /// List of tuples contains (Lcr (cm) , Allowable buckling load (ton)) for steel 37.
        /// </summary>
        private static List<Tuple<double, double>> steel37AllowableLoad = new List<Tuple<double, double>>()
        {
            Tuple.Create(100.0,4.68),
            Tuple.Create(150.0,3.486),
            Tuple.Create(200.0,2.511)
        };

        public static Dictionary<SteelType, List<Tuple<double, double>>> SteelTypesAllowableLoad = new Dictionary<SteelType, List<Tuple<double, double>>>()
        {
            [SteelType.STEEL_52] = steel52AllowableLoad,
            [SteelType.STEEL_37] = steel37AllowableLoad
        };

        /// <summary>
        /// Width of Frame system cross braces availabe in market (cm).
        /// </summary>
        public static List<double> FrameSystemCrossBraces = new List<double>()
            { 90,120,150,180,210,240,270,300};

        /// <summary>
        /// Width of Shore-Brace system cross braces availabe in market.
        /// </summary>
        public static List<double> ShoreBraceSystemCrossBraces = new List<double>()
            {90,120,150,180,210,240,270,300 };

        #endregion


        #region Revit 

        public static BeamSectionName ToBeamSectionName(this RevitBeamSectionName revitName) =>
            (BeamSectionName)((int)revitName);

        public static RevitBeamSectionName ToRevitBeamSectionName(this BeamSectionName revitName) =>
           (RevitBeamSectionName)((int)revitName);

        private static Dictionary<RevitBeamSectionName, RevitBeamSection> _revitBeamSections = new Dictionary<RevitBeamSectionName, RevitBeamSection>()
        {
            //TODO:Check Final list of beam sections because there is a conflict between designer and revit.
            [RevitBeamSectionName.ACROW_BEAM_S12] = new RevitBeamSection(RevitBeamSectionName.ACROW_BEAM_S12, 8, 12),
            [RevitBeamSectionName.DOUBLE_ACROW_BEAM_S12] = new RevitBeamSection(RevitBeamSectionName.DOUBLE_ACROW_BEAM_S12, 16, 12),
            [RevitBeamSectionName.ALUMINUM_BEAM] = new RevitBeamSection(RevitBeamSectionName.ALUMINUM_BEAM, 8, 15),
            [RevitBeamSectionName.ALUMINUM_BEAM_DOUBLE] = new RevitBeamSection(RevitBeamSectionName.ALUMINUM_BEAM_DOUBLE, 8, 15),
            [RevitBeamSectionName.S_BEAM_12] = new RevitBeamSection(RevitBeamSectionName.S_BEAM_12, 11, 12),
            //[RevitBeamSectionName.S_BEAM_16] = new RevitBeamSection(RevitBeamSectionName.S_BEAM_16, 11, 16),
            [RevitBeamSectionName.TIMBER_H20] = new RevitBeamSection(RevitBeamSectionName.TIMBER_H20, 8, 20),
            [RevitBeamSectionName.DOUBLE_TIMBER_H20] = new RevitBeamSection(RevitBeamSectionName.DOUBLE_TIMBER_H20, 16, 20),
            [RevitBeamSectionName.TIMBER_2X4] = new RevitBeamSection(RevitBeamSectionName.TIMBER_2X4, 5, 10),
            [RevitBeamSectionName.TIMBER_2X5] = new RevitBeamSection(RevitBeamSectionName.TIMBER_2X5, 5, 12.5),
            [RevitBeamSectionName.DOUBLE_TIMBER_2X5] = new RevitBeamSection(RevitBeamSectionName.DOUBLE_TIMBER_2X5, 10, 12.5),
            [RevitBeamSectionName.TIMBER_2X6] = new RevitBeamSection(RevitBeamSectionName.TIMBER_2X6, 5, 15),
            [RevitBeamSectionName.DOUBLE_TIMBER_2X6] = new RevitBeamSection(RevitBeamSectionName.DOUBLE_TIMBER_2X6, 10, 15),
            [RevitBeamSectionName.TIMBER_2X8] = new RevitBeamSection(RevitBeamSectionName.TIMBER_2X8, 5, 20),
            [RevitBeamSectionName.DOUBLE_TIMBER_2X8] = new RevitBeamSection(RevitBeamSectionName.DOUBLE_TIMBER_2X8, 10, 20),
            [RevitBeamSectionName.TIMBER_3X3] = new RevitBeamSection(RevitBeamSectionName.TIMBER_3X3, 7.5, 7.5),
            [RevitBeamSectionName.TIMBER_3X5] = new RevitBeamSection(RevitBeamSectionName.TIMBER_3X5, 7.5, 12.5),
            [RevitBeamSectionName.TIMBER_3X6] = new RevitBeamSection(RevitBeamSectionName.TIMBER_3X6, 7.5, 15),
            [RevitBeamSectionName.DOUBLE_TIMBER_3X6] = new RevitBeamSection(RevitBeamSectionName.DOUBLE_TIMBER_3X6, 7.5, 15),
            [RevitBeamSectionName.TIMBER_4X4] = new RevitBeamSection(RevitBeamSectionName.TIMBER_4X4, 10, 10),
        };


        public static Dictionary<PlywoodSectionName, Tuple<string, double>> PlywoodFloorTypes = new Dictionary<PlywoodSectionName, Tuple<string, double>>()
        {
            [PlywoodSectionName.BETOFILM_18MM] = Tuple.Create(RevitBase.BETOFILM_18MM_FLOOR_TYPE, 18.0),
            [PlywoodSectionName.COFIFORM_PLUS_1705MM] = Tuple.Create(RevitBase.COFIFORM_PLUS_1705MM_FLOOR_TYPE, 17.5),
            [PlywoodSectionName.DOUGLAS_FIR_19MM] = Tuple.Create(RevitBase.DOUGLAS_FIR_19MM_FLOOR_TYPE, 19.0),
            [PlywoodSectionName.WISAFORM_BIRCH_18MM] = Tuple.Create(RevitBase.WISAFORM_BIRCH_18MM_FLOOR_TYPE, 18.0)
        };

        public static Dictionary<EuropeanPropTypeName, Tuple<double, double>> PropsMinMaxExtensions = new Dictionary<EuropeanPropTypeName, Tuple<double, double>>()
        {
            [EuropeanPropTypeName.E30] = Tuple.Create(174.0, 300.0),
            [EuropeanPropTypeName.E35] = Tuple.Create(199.0, 350.0),
            [EuropeanPropTypeName.D40] = Tuple.Create(224.0, 400.0),
            [EuropeanPropTypeName.D45] = Tuple.Create(251.0, 450.0)
        };

        public static RevitBeamSection GetRevitBeamSection(RevitBeamSectionName sectionName) =>
            _revitBeamSections[sectionName];


        public static Dictionary<EuropeanPropTypeName, string> PropsTypes = new Dictionary<EuropeanPropTypeName, string>()
        {
            [EuropeanPropTypeName.E30] = RevitBase.PROP_E30,
            [EuropeanPropTypeName.E35] = RevitBase.PROP_E35,
            [EuropeanPropTypeName.D40] = RevitBase.PROP_D40,
            [EuropeanPropTypeName.D45] = RevitBase.PROP_D45
        };

        public const double SHORE_MAIN_HEIGHT = 180.0;

        public const double SHORE_MAIN_HALF_WIDTH = 60.0;

        public const double SHORE_TELESCOPIC_HEIGHT = 165.0;

        #endregion
        
        #region Cost

        public const string FORMWORK_ELEMENT_COST_FILE = "Formwork Elements Cost.csv";

        public const int NO_DAYS_PER_MONTH = 26;


        public static List<FormworkElementCost> ElementsCostDB = new List<FormworkElementCost>()
        {
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.BETOFILM_18MM , Price = 250 , UnitCost = UnitCostMeasure.AREA,NumberOfUses = 10 },
           new PurchaseFormworkElementCost(){Name = FormworkCostElements.COFIFORM_PLUS_1705MM , Price = 300 , UnitCost = UnitCostMeasure.AREA , NumberOfUses = 10 },
           new PurchaseFormworkElementCost(){Name = FormworkCostElements.DOUGLAS_FIR_19MM , Price = 350 , UnitCost = UnitCostMeasure.AREA , NumberOfUses = 10},
           new PurchaseFormworkElementCost(){Name = FormworkCostElements.WISAFORM_BIRCH_18MM , Price = 400 , UnitCost = UnitCostMeasure.AREA,NumberOfUses = 10},

           new RentFormworkElementCost(){Name = FormworkCostElements.ACROW_BEAM_S12 , Price = 10 , UnitCost = UnitCostMeasure.LENGTH},
           new RentFormworkElementCost(){Name = FormworkCostElements.DOUBLE_ACROW_BEAM_S12 , Price = 20 , UnitCost = UnitCostMeasure.LENGTH},
           new RentFormworkElementCost(){Name = FormworkCostElements.ALUMINUM_BEAM , Price = 7 , UnitCost = UnitCostMeasure.LENGTH},
          new RentFormworkElementCost(){Name = FormworkCostElements.DOUBLE_ALUMINUM_BEAM , Price = 14 , UnitCost = UnitCostMeasure.LENGTH},
           new RentFormworkElementCost(){Name = FormworkCostElements.S_BEAM_12 , Price = 10 , UnitCost = UnitCostMeasure.LENGTH},
           new RentFormworkElementCost(){Name = FormworkCostElements.TIMBER_H20 , Price = 7.5 , UnitCost = UnitCostMeasure.LENGTH},

            new PurchaseFormworkElementCost(){Name = FormworkCostElements.TIMBER_2X4 , Price = 42.5 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.TIMBER_2X5 , Price = 53.13 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.TIMBER_2X6 , Price = 63.75 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.TIMBER_2X8 , Price = 85 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.TIMBER_3X3 , Price = 47.81 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.TIMBER_3X5 , Price = 79.69 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.TIMBER_3X6 , Price = 95.63 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.TIMBER_4X4 , Price = 85 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},


           new RentFormworkElementCost(){Name = FormworkCostElements.DOUBLE_TIMBER_H20 , Price = 15 , UnitCost = UnitCostMeasure.LENGTH},

           new PurchaseFormworkElementCost(){Name = FormworkCostElements.DOUBLE_TIMBER_2X5 , Price = 106.25 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.DOUBLE_TIMBER_2X6 , Price = 127.5 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.DOUBLE_TIMBER_3X6 , Price = 191.25 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},
            new PurchaseFormworkElementCost(){Name = FormworkCostElements.DOUBLE_TIMBER_2X8 , Price = 170 , UnitCost = UnitCostMeasure.LENGTH,NumberOfUses=10},


            new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_0050M_STEEL37 , Price = 2.3 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_100M_STEEL37 , Price = 4.1 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_1050M_STEEL37 , Price = 6.15 , UnitCost = UnitCostMeasure.NUMBER},
          new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_200M_STEEL37 , Price = 7.89 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_2050M_STEEL37 , Price = 9.79 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_300M_STEEL37 , Price = 11.4 , UnitCost = UnitCostMeasure.NUMBER},

            new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_0050M_STEEL52 , Price = 2.99 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_100M_STEEL52 , Price = 5.33 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_1050M_STEEL52 , Price = 8 , UnitCost = UnitCostMeasure.NUMBER},
          new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_200M_STEEL52 , Price = 10.26 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_2050M_STEEL52 , Price = 12.73 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_VERTICAL_300M_STEEL52 , Price = 14.82 , UnitCost = UnitCostMeasure.NUMBER},


           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_0060M_STEEL37 , Price = 2.12 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_0090M_STEEL37 , Price = 2.46 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_1020M_STEEL37 , Price = 2.93 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_1050M_STEEL37 , Price = 3.68 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_1080M_STEEL37 , Price = 4.41 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_2010M_STEEL37 , Price = 5.12 , UnitCost = UnitCostMeasure.NUMBER},
            new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_2040M_STEEL37 , Price = 5.84 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_2070M_STEEL37 , Price = 6.56 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_300M_STEEL37 , Price = 7.12 , UnitCost = UnitCostMeasure.NUMBER},


           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_0060M_STEEL52 , Price = 2.76 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_0090M_STEEL52 , Price = 3.2 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_1020M_STEEL52 , Price = 3.81 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_1050M_STEEL52 , Price = 4.78 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_1080M_STEEL52 , Price = 5.73 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_2010M_STEEL52 , Price = 6.66 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_2040M_STEEL52 , Price = 7.59 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_2070M_STEEL52 , Price = 8.53 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CUPLOCK_LEDGER_300M_STEEL52 , Price = 9.26 , UnitCost = UnitCostMeasure.NUMBER},


           new RentFormworkElementCost(){Name = FormworkCostElements.U_HEAD_JACK_SOLID , Price = 5.3 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.POST_HEAD_JACK_SOLID , Price = 4.26 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.ROUND_SPIGOT , Price = 0.38 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SPRING_CLIP , Price = 0.03 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.RIVET_PIN_16MM_L9CM , Price = 0.38 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.PRESSED_PROP_SWIVEL_COUPLER , Price = 1.16 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.PROP_LEG , Price = 4.52 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.U_HEAD_FOR_PROPS , Price = 3.5 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SHOREBRACE_CONNECTOR , Price = 0.5 , UnitCost = UnitCostMeasure.NUMBER},


           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_100M , Price = 1.32 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_1050M , Price = 2 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_200M , Price = 2.63 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_2050M , Price = 3.29 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_300M , Price = 3.95 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_3050M , Price = 4.61 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_400M , Price = 5.27 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_4050M , Price = 5.93 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_500M , Price = 6.58 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.SCAFFOLDING_TUBE_600M , Price = 7.9 , UnitCost = UnitCostMeasure.NUMBER},

            new RentFormworkElementCost(){Name = FormworkCostElements.ACROW_PROP_D40 , Price = 6.28 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.ACROW_PROP_D45 , Price = 6.79 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.ACROW_PROP_E30 , Price = 4.61 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.ACROW_PROP_E35 , Price = 6.05 , UnitCost = UnitCostMeasure.NUMBER},

            new RentFormworkElementCost(){Name = FormworkCostElements.SHOREBRACE_FRAME , Price = 16.04 , UnitCost = UnitCostMeasure.NUMBER},
            new RentFormworkElementCost(){Name = FormworkCostElements.SHOREBRACE_TELESCOPIC_FRAME , Price = 13.83 , UnitCost = UnitCostMeasure.NUMBER},


            new RentFormworkElementCost(){Name = FormworkCostElements.CROSS_BRACE_0090M , Price = 3.65 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CROSS_BRACE_1020 , Price = 4.1 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CROSS_BRACE_1050 , Price = 4.4 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CROSS_BRACE_1080 , Price = 4.9 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CROSS_BRACE_2010 , Price = 5.3 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CROSS_BRACE_2040 , Price = 5.65 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CROSS_BRACE_2070 , Price = 6.12 , UnitCost = UnitCostMeasure.NUMBER},
           new RentFormworkElementCost(){Name = FormworkCostElements.CROSS_BRACE_300 , Price = 6.54 , UnitCost = UnitCostMeasure.NUMBER},
          

        };

        #endregion

    }
}
