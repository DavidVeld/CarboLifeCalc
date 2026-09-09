using CarboLifeAPI.Data;
using LCAx;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
//using System.Web.Script.Serialization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace CarboLifeAPI
{
    public class JsonExportUtils
    {
        /// <summary>
        /// Exports a carbolifeproject as baked JSON
        /// </summary>
        /// <param name="path"></param>
        /// <param name="carboLifeProject"></param>
        /// <returns></returns>
        public static bool ExportToJson(string path, CarboProject carboLifeProject)
        {
            bool result = false;

            if (string.IsNullOrWhiteSpace(path) || carboLifeProject == null)
                return false;

            JsCarboProject jsProject = converToJsProject(carboLifeProject);

            try
            {
                /* 4.8:
                var JsonSerializer = new JavaScriptSerializer();

                JsonSerializer.MaxJsonLength = Int32.MaxValue;

                var json = JsonSerializer.Serialize(jsProject);
                */

                //Indented so the file can be read by a human, and relaxed escaping so the unit
                //symbols the project carries (£, m², CO₂e) arrive as themselves. The default
                //encoder escapes everything non ASCII, so valueUnit used to come out as an
                //escaped code point rather than the currency symbol itself.
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string jsonString = JsonSerializer.Serialize(jsProject, options);

                //The save dialog only ever hands back an existing folder, but this is also
                //called with a path built in code.
                string folder = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(folder) == false && Directory.Exists(folder) == false)
                    Directory.CreateDirectory(folder);

                File.WriteAllText(path, jsonString);

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return result;
        }


        /// <summary>
        /// Gets a path file to save to
        /// </summary>
        /// <returns>path if valis, null if invalid</returns>
        public static string GetSaveAsLocation()
        {
            //Create a File and save it as a HTML File
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save LCA file";
            saveDialog.Filter = "json file|*.json";
            saveDialog.FilterIndex = 1;
            //So a user who types "MyProject" gets MyProject.json rather than an extensionless file.
            saveDialog.DefaultExt = "json";
            saveDialog.AddExtension = true;
            saveDialog.RestoreDirectory = true;

            //Read the result rather than inferring a cancel from an empty file name: the
            //dialog keeps the name it was given, so cancel does not reliably clear it.
            if (saveDialog.ShowDialog() != true)
                return null;

            string path = saveDialog.FileName;

            if (string.IsNullOrWhiteSpace(path))
                return null;

            //Check if the file can be read and written to.
            if (File.Exists(path))
            {
                //FileInfo fileInfo = new FileInfo(path);
                bool isInUse = IsFileLocked(path);

                if (isInUse == true)
                    return null;
            }

            //If this part is reached; return the valid path;
            return path;

        }
        private static bool IsFileLocked(string file)
        {
            try
            {
                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //The file is open
                return true;
            }

            //All is ok
            return false;
        }

        public static JsCarboProject converToJsProject(CarboProject carboProject) 
        {
            JsCarboProject jsProject = new JsCarboProject();

            jsProject.Name = carboProject.Name;
            jsProject.Number = carboProject.Number;
            jsProject.Category = carboProject.Category;
            jsProject.Description = carboProject.Description;
            jsProject.SocialCost = carboProject.SocialCost;
            jsProject.GIA = carboProject.Area;
            jsProject.GIANew = carboProject.AreaNew;

            //A0GlobalUncert is in kgCO2e, the three globals beside it and ECTotal are all in
            //tCO2e. Divide so every global on the exported project reads in the unit its own
            //doc comment promises, instead of A0 alone arriving a thousand times too large.
            jsProject.A0Global = carboProject.A0GlobalUncert / 1000;
            jsProject.A5Global = carboProject.A5Global;
            jsProject.b675Global = carboProject.b675Global;
            jsProject.C1Global = carboProject.C1Global;
            jsProject.valueUnit = carboProject.valueUnit;
            jsProject.designLife = carboProject.designLife;

            //The project total in tCO2e, materials plus whichever globals the calc switches
            //select. Left unset it exported as zero.
            jsProject.ECTotal = carboProject.getTotalEC();

            //Walk the groups rather than the flat element list. The group carries the material
            //the user actually assigned, its B4 replacement count and its manual Additional,
            //and none of those can be recovered from an element on its own. Re-matching the
            //material name through getClosestMatch, as this used to, went back to the database
            //and so ignored any edit made to the material inside the project, while the
            //materialList below was being read straight off the groups: one file, two answers.
            List<JsCarboElement> elementBuffer = new List<JsCarboElement>();

            foreach (CarboGroup grp in carboProject.getGroupList)
            {
                if (grp.AllElements == null || grp.AllElements.Count == 0)
                    continue;

                CarboMaterial material = grp.Material;

                //B4 multiplies the carbon, not the mass, exactly as in CarboGroup.getTotalXX.
                double b4 = 1;
                if (grp.inUseProperties != null)
                    b4 = grp.inUseProperties.B4;

                foreach (CarboElement ce in grp.AllElements)
                {
                    JsCarboElement JsCe = ConvertoToJsCarboElement(ce, carboProject);

                    //A group with no material has nothing to price the element with, but the
                    //element still belongs in the export, at zero, rather than vanishing from
                    //it silently the way an unmatched element used to.
                    if (material != null)
                    {
                        JsCe.Density = material.Density;
                        JsCe.Grade = material.Grade;

                        double mass = ce.Mass;
                        if (mass == 0)
                            mass = ce.Volume_Total * JsCe.Density;

                        JsCe.EC_A1A3_Total = mass * material.ECI_A1A3 * b4;
                        JsCe.EC_A4_Total = mass * material.ECI_A4 * b4;
                        JsCe.EC_A5_Total = mass * material.ECI_A5 * b4;
                        JsCe.EC_B1B7_Total = mass * material.ECI_B1B5 * b4;
                        JsCe.EC_C1C4_Total = mass * material.ECI_C1C4 * b4;
                        JsCe.EC_D_Total = mass * material.ECI_D * b4;
                        JsCe.EC_Sequestration_Total = mass * material.ECI_Seq * b4;
                        //getTotalMix folds the group's manual addition in with the material's
                        //Mix, so the per element figures add back up to the group figure.
                        JsCe.EC_Mix_Total = mass * (material.ECI_Mix + grp.Additional) * b4;
                    }

                    elementBuffer.Add(JsCe);
                }
            }

            //Keep the Id ordering getElementsFromGroups used to give: the CSV export writes one
            //row per entry in this list, and its row order is this order.
            jsProject.elementList.AddRange(elementBuffer.OrderBy(o => o.Id));

            // get all the groups without elements
            foreach(CarboGroup grp in carboProject.getGroupList)
            {
                if(grp.AllElements == null || grp.AllElements.Count == 0)
                {
                    JsCarboElement JsCe = ConvertoToJsCarboElement(grp, carboProject);
                    jsProject.elementList.Add(JsCe);

                }
            }

            //Get the materials
            List<CarboMaterial> materialList = carboProject.getUsedmaterials();

            if (materialList.Count > 0)
            {
                foreach (CarboMaterial cm in materialList)
                {
                    JsCarboMaterial jsMaterial = converToJsMaterial(cm);

                    if (jsMaterial != null)
                        jsProject.materialList.Add(jsMaterial);
                }
            }


            return jsProject;
        }

        private static JsCarboMaterial converToJsMaterial(CarboMaterial cm)
        {
            JsCarboMaterial result = new JsCarboMaterial();

            try
            {
                result.Id = cm.Id;
                result.Name = cm.Name;
                result.Description = cm.Description;
                result.Category = cm.Category;
                result.Grade = cm.Grade;
                result.Density = cm.Density;
                result.EPDurl = cm.EPDurl;
                result.WasteFactor = cm.WasteFactor;
                result.isLocked = cm.isLocked;

                result.ECI_A1A3 = cm.ECI_A1A3;
                result.ECI_A4 = cm.ECI_A4;
                result.ECI_A5 = cm.ECI_A5;
                result.ECI_B1B5 = cm.ECI_B1B5;
                result.ECI_C1C4 = cm.ECI_C1C4;
                result.ECI_D = cm.ECI_D;
                result.ECI_Mix = cm.ECI_Mix;
                result.ECI_Seq = cm.ECI_Seq;
                result.ECI = cm.ECI;
                result.VolumeECI = cm.getVolumeECI;
            }
            catch
            {
                return null;
            }

            return result;
        }
        /// <summary>
        /// The LCAx format this exporter writes, and the version stamped on every EPD it builds.
        /// </summary>
        private const string LcaxFormatVersion = "2.2.1";

        private const string LcaxSoftwareName = "Carbo Life Calculator";

        /// <summary>
        /// Serialiser settings for LCAx. The property names come from the attributes on the
        /// model, so no naming policy is applied here: a policy would rewrite the dictionary
        /// keys as well, and those are data, not property names.
        /// </summary>
        private static JsonSerializerOptions getLcaxSerializerOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            //The schema marks most fields "type": ["string", "null"]. Dropping the nulls keeps
            //the file to what the project actually knows.
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            return options;
        }

        /// <summary>
        /// Builds an LCAx project from a Carbo Life project, shaped to LCAx_json_Scheme.txt.
        /// </summary>
        /// <remarks>
        /// This used to take a JsCarboProject. It takes the CarboProject now because the flat
        /// Js projection has thrown away the three things the schema needs: which groups the
        /// elements belong to (an LCAx assembly is a group, so keying assemblies off the
        /// element GUID collapsed every group with no elements into a single assembly), the
        /// calculation switches that say which life cycle stages this project actually
        /// reports, and the level list. It still calls converToJsProject internally so the
        /// per element carbon is the same figure the JSON and CSV exports carry.
        /// </remarks>
        public static Lcax convertToLCAx(CarboProject carboProject)
        {
            Lcax lcaxProject = new Lcax();

            if (carboProject == null)
                return lcaxProject;

            JsCarboProject jsProject = converToJsProject(carboProject);

            lcaxProject.FormatVersion = LcaxFormatVersion;
            lcaxProject.Id = string.IsNullOrWhiteSpace(carboProject.Number)
                ? Guid.NewGuid().ToString()
                : carboProject.Number;
            lcaxProject.Name = carboProject.Name ?? string.Empty;
            lcaxProject.Description = emptyToNull(carboProject.Description);
            lcaxProject.Comment = emptyToNull(carboProject.Category);

            //Nothing in a Carbo Life project records a design stage or a site, and the schema
            //would rather have an honest "other"/"unknown" than a guess.
            lcaxProject.ProjectPhase = ProjectPhase.Other;
            lcaxProject.Location = new Location();

            //referenceStudyPeriod is a uint8 in the schema.
            if (carboProject.designLife > 0)
                lcaxProject.ReferenceStudyPeriod = Math.Min(carboProject.designLife, 255);

            lcaxProject.SoftwareInfo = new SoftwareInfo();
            lcaxProject.SoftwareInfo.LcaSoftware = LcaxSoftwareName;
            lcaxProject.SoftwareInfo.CalculationType = "Embodied carbon, kgCO2e, per life cycle stage";
            lcaxProject.SoftwareInfo.GoalAndScopeDefinition = emptyToNull(carboProject.Description);

            lcaxProject.LifeCycleStages = getReportedStages(carboProject);
            lcaxProject.ImpactCategories = new List<ImpactCategoryKey>();
            lcaxProject.ImpactCategories.Add(ImpactCategoryKey.Gwp);
            if (carboProject.calculateSeq == true)
                lcaxProject.ImpactCategories.Add(ImpactCategoryKey.GwpBio);

            lcaxProject.ProjectInfo = buildProjectInfo(carboProject);

            //Elements first, so a group Id that happens to collide with an element Id cannot
            //displace a real element.
            Dictionary<long, JsCarboElement> elementsById = new Dictionary<long, JsCarboElement>();
            foreach (JsCarboElement je in jsProject.elementList)
            {
                if (elementsById.ContainsKey(je.Id) == false)
                    elementsById.Add(je.Id, je);
            }

            //Running project totals, summed from the same per element figures that go into the
            //products, so the three levels of the file agree with each other.
            double a1a3 = 0, a4 = 0, a5 = 0, b1b7 = 0, c1c4 = 0, d = 0, seq = 0, mix = 0;

            foreach (CarboGroup grp in carboProject.getGroupList)
            {
                if (grp == null)
                    continue;

                Assembly assembly = buildAssembly(grp, carboProject, elementsById);

                //One assembly per group, keyed on the group Id. Keying on the element GUID, as
                //this used to, gave every element-less group the same empty key and merged
                //them all into whichever one came first.
                string key = grp.Id.ToString(CultureInfo.InvariantCulture);
                if (lcaxProject.Assemblies.ContainsKey(key) == false)
                    lcaxProject.Assemblies.Add(key, assembly);

                a1a3 += grp.getTotalA1A3;
                a4 += grp.getTotalA4;
                a5 += grp.getTotalA5;
                b1b7 += grp.getTotalB1B7;
                c1c4 += grp.getTotalC1C4;
                d += grp.getTotalD;
                seq += grp.getTotalSeq;
                mix += grp.getTotalMix;
            }

            //The globals sit alongside the material totals rather than inside any assembly:
            //they are whole building figures the project carries in tCO2e, so bring them up to
            //the kgCO2e everything else in an LCAx results block is written in.
            double a0Global = carboProject.calculateA0 ? carboProject.A0GlobalUncert : 0;
            double b6Global = carboProject.calculateB67 ? carboProject.b675Global * 1000 : 0;

            if (carboProject.calculateA5 == true)
                a5 += carboProject.A5Global * 1000;

            if (carboProject.calculateC == true)
                c1c4 += carboProject.C1Global * 1000;

            lcaxProject.Results = buildGwpResults(carboProject, a1a3, a4, a5, b1b7, c1c4, d, seq, mix, a0Global, b6Global);

            lcaxProject.MetaData = new Dictionary<string, string>();
            addMeta(lcaxProject.MetaData, "projectNumber", carboProject.Number);
            addMeta(lcaxProject.MetaData, "buildingType", carboProject.Category);
            addMeta(lcaxProject.MetaData, "giaM2", carboProject.Area);
            addMeta(lcaxProject.MetaData, "giaNewM2", carboProject.AreaNew);
            addMeta(lcaxProject.MetaData, "demolishedAreaM2", carboProject.demoArea);
            addMeta(lcaxProject.MetaData, "uncertaintyFactor", carboProject.UncertFact);
            addMeta(lcaxProject.MetaData, "socialCostPerTonne", carboProject.SocialCost);
            addMeta(lcaxProject.MetaData, "socialCostUnit", carboProject.valueUnit);
            //The headline number the application itself shows, so a reader can check the
            //results block against it without rebuilding the calculation.
            addMeta(lcaxProject.MetaData, "totalEmbodiedCarbonTCo2e", jsProject.ECTotal);
            addMeta(lcaxProject.MetaData, "a0GlobalTCo2e", carboProject.A0GlobalUncert / 1000);
            addMeta(lcaxProject.MetaData, "a5GlobalTCo2e", carboProject.A5Global);
            addMeta(lcaxProject.MetaData, "b6b7GlobalTCo2e", carboProject.b675Global);
            addMeta(lcaxProject.MetaData, "c1GlobalTCo2e", carboProject.C1Global);
            addMeta(lcaxProject.MetaData, "sequestrationHandling",
                "Sequestration and the mix allowance have no life cycle stage of their own in LCAx. "
                + "Both are inside the total the application reports, so both are carried in gwp a1a3; "
                + "the sequestration part is repeated on its own under gwp_bio a1a3.");
            //The two numbers are built differently and a reader should not have to work out why
            //they differ: results follow the stage switches, the headline never has.
            addMeta(lcaxProject.MetaData, "resultsScope",
                "The results blocks carry only the stages listed in lifeCycleStages, which follow the "
                + "project's calculation switches. totalEmbodiedCarbonTCo2e is the application's own "
                + "headline figure and always covers every stage, so the two agree only when every "
                + "switch is on.");

            return lcaxProject;
        }

        /// <summary>
        /// The stages this project reports, taken from the calculation switches rather than
        /// from whatever happens to be non zero.
        /// </summary>
        private static List<LifeCycleStage> getReportedStages(CarboProject carboProject)
        {
            List<LifeCycleStage> stages = new List<LifeCycleStage>();

            if (carboProject.calculateA0 == true)
                stages.Add(LifeCycleStage.A0);

            //Sequestration and the mix allowance are both reported inside a1a3.
            if (carboProject.calculateA13 == true || carboProject.calculateSeq == true || carboProject.calculateAdd == true)
                stages.Add(LifeCycleStage.A1A3);

            if (carboProject.calculateA4 == true)
                stages.Add(LifeCycleStage.A4);

            if (carboProject.calculateA5 == true)
                stages.Add(LifeCycleStage.A5);

            //The application holds B1 to B5 as one number, so it is reported against b1.
            if (carboProject.calculateB == true)
                stages.Add(LifeCycleStage.B1);

            if (carboProject.calculateB67 == true)
                stages.Add(LifeCycleStage.B6);

            //Likewise C1 to C4 is one number, reported against c1.
            if (carboProject.calculateC == true)
                stages.Add(LifeCycleStage.C1);

            if (carboProject.calculateD == true)
                stages.Add(LifeCycleStage.D);

            return stages;
        }

        /// <summary>
        /// A gwp results block, and where there is any, a gwp_bio block holding the biogenic
        /// part on its own. Values are absolute kgCO2e.
        /// </summary>
        private static Dictionary<string, Dictionary<string, double?>> buildGwpResults(CarboProject carboProject,
            double a1a3, double a4, double a5, double b1b7, double c1c4, double d, double seq, double mix,
            double a0Global = 0, double b6Global = 0)
        {
            Dictionary<string, double?> gwp = new Dictionary<string, double?>();

            if (a0Global != 0)
                gwp.Add(LcaxKey.Of(LifeCycleStage.A0), a0Global);

            //a1a3 carries production plus the two figures LCAx has no stage for. Leaving them
            //out here would put gwp below the total the application reports, since the ECI it
            //totals includes both.
            double production = 0;
            bool anyProduction = false;

            if (carboProject.calculateA13 == true)
            {
                production += a1a3;
                anyProduction = true;
            }

            if (carboProject.calculateSeq == true)
            {
                production += seq;
                anyProduction = true;
            }

            if (carboProject.calculateAdd == true)
            {
                production += mix;
                anyProduction = true;
            }

            if (anyProduction == true)
                gwp.Add(LcaxKey.Of(LifeCycleStage.A1A3), production);

            if (carboProject.calculateA4 == true)
                gwp.Add(LcaxKey.Of(LifeCycleStage.A4), a4);

            if (carboProject.calculateA5 == true)
                gwp.Add(LcaxKey.Of(LifeCycleStage.A5), a5);

            if (carboProject.calculateB == true)
                gwp.Add(LcaxKey.Of(LifeCycleStage.B1), b1b7);

            if (b6Global != 0)
                gwp.Add(LcaxKey.Of(LifeCycleStage.B6), b6Global);

            if (carboProject.calculateC == true)
                gwp.Add(LcaxKey.Of(LifeCycleStage.C1), c1c4);

            if (carboProject.calculateD == true)
                gwp.Add(LcaxKey.Of(LifeCycleStage.D), d);

            Dictionary<string, Dictionary<string, double?>> results = new Dictionary<string, Dictionary<string, double?>>();
            results.Add(LcaxKey.Of(ImpactCategoryKey.Gwp), gwp);

            //EN 15804 treats biogenic carbon as a component of the total, so repeating the
            //sequestration here is a split of gwp, not an addition to it.
            if (carboProject.calculateSeq == true && seq != 0)
            {
                Dictionary<string, double?> bio = new Dictionary<string, double?>();
                bio.Add(LcaxKey.Of(LifeCycleStage.A1A3), seq);
                results.Add(LcaxKey.Of(ImpactCategoryKey.GwpBio), bio);
            }

            return results;
        }

        private static ProjectInfo buildProjectInfo(CarboProject carboProject)
        {
            ProjectInfo info = new ProjectInfo();

            //A project carrying a demolition area is describing works on an existing building.
            info.BuildingType = carboProject.demoArea > 0
                ? BuildingType.DeconstructionAndNewConstructionWorks
                : BuildingType.NewConstructionWorks;

            info.BuildingTypology = new List<BuildingTypology>();
            info.BuildingTypology.Add(BuildingTypology.Other);

            info.GeneralEnergyClass = GeneralEnergyClass.Unknown;
            info.RoofType = RoofType.Other;

            if (carboProject.Area > 0)
            {
                info.GrossFloorArea = new AreaType();
                info.GrossFloorArea.Definition = "Gross internal area as entered in Carbo Life Calculator";
                info.GrossFloorArea.Unit = Unit.M2;
                info.GrossFloorArea.Value = carboProject.Area;
            }

            //Storey counts come off the level list the import built, so they describe the model
            //rather than a value typed in somewhere.
            if (carboProject.carboLevelList != null && carboProject.carboLevelList.Count > 0)
            {
                int above = 0;
                int below = 0;

                foreach (CarboLevel level in carboProject.carboLevelList)
                {
                    if (level == null)
                        continue;

                    if (level.Level < 0)
                        below++;
                    else
                        above++;
                }

                info.FloorsAboveGround = above;
                info.FloorsBelowGround = below;
            }

            //Only the two scopes a structural model can actually speak to.
            List<BuildingModelScope> scope = new List<BuildingModelScope>();
            bool anySub = false;
            bool anySuper = false;

            foreach (CarboGroup grp in carboProject.getGroupList)
            {
                if (grp == null)
                    continue;

                if (grp.isSubstructure == true)
                    anySub = true;
                else
                    anySuper = true;
            }

            if (anySub == true)
                scope.Add(BuildingModelScope.Substructure);

            if (anySuper == true)
                scope.Add(BuildingModelScope.SuperstructureFrame);

            if (scope.Count > 0)
                info.BuildingModelScope = scope;

            CarboGroup totals = carboProject.getTotalsGroup();
            if (totals != null && totals.Mass > 0)
            {
                info.BuildingMass = new ValueUnit();
                info.BuildingMass.Unit = Unit.Kg;
                info.BuildingMass.Value = totals.Mass;
            }

            return info;
        }

        /// <summary>
        /// One LCAx assembly per Carbo Life group: the group is the build-up, its elements are
        /// the products. A group with no elements still gets an assembly, carrying a single
        /// product that stands for the group itself.
        /// </summary>
        private static Assembly buildAssembly(CarboGroup grp, CarboProject carboProject,
            Dictionary<long, JsCarboElement> elementsById)
        {
            Assembly assembly = new Assembly();

            assembly.Id = grp.Id.ToString(CultureInfo.InvariantCulture);
            assembly.Name = string.IsNullOrWhiteSpace(grp.Description)
                ? (string.IsNullOrWhiteSpace(grp.Category) ? "Group " + assembly.Id : grp.Category)
                : grp.Description;
            assembly.Description = emptyToNull(grp.MaterialName);
            assembly.Comment = string.IsNullOrWhiteSpace(grp.SubCategory)
                ? emptyToNull(grp.Category)
                : grp.Category + " - " + grp.SubCategory;
            assembly.Quantity = grp.TotalVolume;
            assembly.Unit = Unit.M3;

            assembly.Results = buildGwpResults(carboProject,
                grp.getTotalA1A3, grp.getTotalA4, grp.getTotalA5, grp.getTotalB1B7,
                grp.getTotalC1C4, grp.getTotalD, grp.getTotalSeq, grp.getTotalMix);

            assembly.MetaData = new Dictionary<string, string>();
            addMeta(assembly.MetaData, "category", grp.Category);
            addMeta(assembly.MetaData, "subCategory", grp.SubCategory);
            addMeta(assembly.MetaData, "materialName", grp.MaterialName);
            addMeta(assembly.MetaData, "massKg", grp.Mass);
            addMeta(assembly.MetaData, "densityKgM3", grp.Density);
            addMeta(assembly.MetaData, "wasteFactorPercent", grp.Waste);
            addMeta(assembly.MetaData, "volumeCorrection", grp.Correction);
            addMeta(assembly.MetaData, "isSubstructure", grp.isSubstructure);
            addMeta(assembly.MetaData, "isDemolished", grp.isDemolished);
            addMeta(assembly.MetaData, "isExisting", grp.isExisting);
            addMeta(assembly.MetaData, "additionalEciPerKg", grp.Additional);

            if (grp.inUseProperties != null)
            {
                addMeta(assembly.MetaData, "b4ReplacementCount", grp.inUseProperties.B4);
                addMeta(assembly.MetaData, "elementDesignLifeYears", grp.inUseProperties.elementdesignlife);
                //Carried here because the per stage figures above come from the group's own
                //getTotal getters, and those do not include the in use ECI.
                addMeta(assembly.MetaData, "inUseEciPerKg", grp.inUseProperties.totalECI);
            }

            assembly.Products = new Dictionary<string, Product>();

            if (grp.AllElements != null && grp.AllElements.Count > 0)
            {
                foreach (CarboElement ce in grp.AllElements)
                {
                    if (ce == null)
                        continue;

                    JsCarboElement jsCe;
                    if (elementsById.TryGetValue(ce.Id, out jsCe) == false)
                        continue;

                    addProduct(assembly, buildProduct(jsCe, grp, carboProject));
                }
            }
            else
            {
                //Rebuilt from the group rather than looked up, so a group Id that collides with
                //an element Id cannot pull in the wrong row.
                addProduct(assembly, buildProduct(ConvertoToJsCarboElement(grp, carboProject), grp, carboProject));
            }

            return assembly;
        }

        private static void addProduct(Assembly assembly, Product product)
        {
            string key = product.Id;

            //Products are a keyed object in the schema, so a repeated GUID would otherwise
            //throw the whole export away.
            if (string.IsNullOrEmpty(key) || assembly.Products.ContainsKey(key))
                key = product.Id + "." + assembly.Products.Count.ToString(CultureInfo.InvariantCulture);

            assembly.Products.Add(key, product);
        }

        private static Product buildProduct(JsCarboElement jsCe, CarboGroup grp, CarboProject carboProject)
        {
            Product product = new Product();

            product.Id = string.IsNullOrWhiteSpace(jsCe.GUID)
                ? jsCe.Id.ToString(CultureInfo.InvariantCulture)
                : jsCe.GUID;

            product.Name = string.IsNullOrWhiteSpace(jsCe.Name)
                ? "Element " + jsCe.Id.ToString(CultureInfo.InvariantCulture)
                : jsCe.Name;

            product.Description = emptyToNull(jsCe.CarboMaterialName);
            product.Quantity = jsCe.Volume_Total;
            product.Unit = Unit.M3;

            //Required, and a uint32, so it may not go negative.
            double serviceLife = carboProject.designLife;
            if (grp != null && grp.inUseProperties != null && grp.inUseProperties.elementdesignlife > 0)
                serviceLife = grp.inUseProperties.elementdesignlife;

            product.ReferenceServiceLife = serviceLife > 0 ? (long)Math.Round(serviceLife) : 0;

            product.Results = buildGwpResults(carboProject,
                jsCe.EC_A1A3_Total, jsCe.EC_A4_Total, jsCe.EC_A5_Total, jsCe.EC_B1B7_Total,
                jsCe.EC_C1C4_Total, jsCe.EC_D_Total, jsCe.EC_Sequestration_Total, jsCe.EC_Mix_Total);

            product.MetaData = new Dictionary<string, string>();
            addMeta(product.MetaData, "revitElementId", jsCe.Id);
            addMeta(product.MetaData, "revitMaterialName", jsCe.MaterialName);
            addMeta(product.MetaData, "materialName", jsCe.CarboMaterialName);
            addMeta(product.MetaData, "grade", jsCe.Grade);
            addMeta(product.MetaData, "category", jsCe.Category);
            addMeta(product.MetaData, "subCategory", jsCe.SubCategory);
            addMeta(product.MetaData, "levelName", jsCe.LevelName);
            addMeta(product.MetaData, "levelElevation", jsCe.Level);
            addMeta(product.MetaData, "massKg", jsCe.Mass);
            addMeta(product.MetaData, "densityKgM3", jsCe.Density);
            addMeta(product.MetaData, "netVolumeM3", jsCe.Volume);
            addMeta(product.MetaData, "isSubstructure", jsCe.isSubstructure);
            addMeta(product.MetaData, "isDemolished", jsCe.isDemolished);
            addMeta(product.MetaData, "isExisting", jsCe.isExisting);
            addMeta(product.MetaData, "includedInCalculation", jsCe.includeInCalc);
            addMeta(product.MetaData, "volumeCorrection", jsCe.Correction);
            addMeta(product.MetaData, "additionalData", jsCe.AdditionalData);

            if (grp != null && grp.inUseProperties != null)
                addMeta(product.MetaData, "b4ReplacementCount", grp.inUseProperties.B4);

            product.ImpactData = buildEpd(grp != null ? grp.Material : null, jsCe);

            return product;
        }

        /// <summary>
        /// The material record, written as an EPD. The impacts here are the material's own
        /// figures per kg and are deliberately left unmodified: the group's waste, correction,
        /// B4 and additional allowance belong to the calculation, and they are already in the
        /// product's results block.
        /// </summary>
        private static Epd buildEpd(CarboMaterial material, JsCarboElement jsCe)
        {
            Epd epd = new Epd();

            epd.FormatVersion = LcaxFormatVersion;
            epd.DeclaredUnit = Unit.Kg;
            epd.Location = Country.Unknown;
            //Qualified: PresentationFramework puts a namespace called Standard in scope on
            //net48, and it wins the lookup against the enum.
            epd.Standard = LCAx.Standard.Unknown;
            epd.Subtype = SubType.Generic;
            epd.Version = "1";

            //Not a published EPD, so there is no real publication or expiry date to give. Both
            //are required, and stamping today on them is at least not a claim about validity
            //that nobody made.
            epd.PublishedDate = DateTime.Today;
            epd.ValidUntil = DateTime.Today;

            if (material == null)
            {
                epd.Id = "0";
                epd.Name = string.IsNullOrWhiteSpace(jsCe.CarboMaterialName) ? "Unmatched material" : jsCe.CarboMaterialName;
                epd.Comment = "No material is assigned to this group, so it carries no impact figures.";
                return epd;
            }

            epd.Id = material.Id.ToString(CultureInfo.InvariantCulture);
            epd.Name = string.IsNullOrWhiteSpace(material.Name) ? "Material " + epd.Id : material.Name;

            string comment = "Material record from the " + LcaxSoftwareName
                + " database, not a published EPD. Figures are kgCO2e per kg.";
            if (string.IsNullOrWhiteSpace(material.Description) == false)
                comment = material.Description + " " + comment;

            epd.Comment = comment;

            if (string.IsNullOrWhiteSpace(material.EPDurl) == false)
            {
                epd.Source = new Source();
                epd.Source.Name = "Source data";
                epd.Source.Url = material.EPDurl;
            }

            Dictionary<string, double?> gwp = new Dictionary<string, double?>();
            gwp.Add(LcaxKey.Of(LifeCycleStage.A1A3), material.ECI_A1A3);
            gwp.Add(LcaxKey.Of(LifeCycleStage.A4), material.ECI_A4);
            gwp.Add(LcaxKey.Of(LifeCycleStage.A5), material.ECI_A5);
            gwp.Add(LcaxKey.Of(LifeCycleStage.B1), material.ECI_B1B5);
            gwp.Add(LcaxKey.Of(LifeCycleStage.C1), material.ECI_C1C4);
            gwp.Add(LcaxKey.Of(LifeCycleStage.D), material.ECI_D);

            epd.Impacts.Add(LcaxKey.Of(ImpactCategoryKey.Gwp), gwp);

            if (material.ECI_Seq != 0)
            {
                Dictionary<string, double?> bio = new Dictionary<string, double?>();
                bio.Add(LcaxKey.Of(LifeCycleStage.A1A3), material.ECI_Seq);
                epd.Impacts.Add(LcaxKey.Of(ImpactCategoryKey.GwpBio), bio);
            }

            epd.MetaData = new Dictionary<string, string>();
            addMeta(epd.MetaData, "category", material.Category);
            addMeta(epd.MetaData, "grade", material.Grade);
            addMeta(epd.MetaData, "densityKgM3", material.Density);
            addMeta(epd.MetaData, "defaultWasteFactorPercent", material.WasteFactor);
            addMeta(epd.MetaData, "totalEciPerKg", material.ECI);
            //b1 above holds the B1 to B5 lump and c1 the C1 to C4 lump, and the mix allowance
            //has no stage at all, so say so rather than let a reader assume otherwise.
            addMeta(epd.MetaData, "mixEciPerKg", material.ECI_Mix);
            addMeta(epd.MetaData, "stageMapping",
                "b1 holds the combined B1-B5 figure, c1 the combined C1-C4 figure. "
                + "mixEciPerKg has no life cycle stage in LCAx and is reported inside gwp a1a3 of the results.");

            return epd;
        }

        private static void addMeta(Dictionary<string, string> metaData, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (metaData.ContainsKey(key) == false)
                metaData.Add(key, value);
        }

        private static void addMeta(Dictionary<string, string> metaData, string key, double value)
        {
            addMeta(metaData, key, value.ToString("G", CultureInfo.InvariantCulture));
        }

        private static void addMeta(Dictionary<string, string> metaData, string key, long value)
        {
            addMeta(metaData, key, value.ToString(CultureInfo.InvariantCulture));
        }

        private static void addMeta(Dictionary<string, string> metaData, string key, bool value)
        {
            addMeta(metaData, key, value ? "true" : "false");
        }

        private static string emptyToNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }


        /// <summary>
        /// Reads the project header out of an LCAx file. This does NOT rebuild the groups,
        /// elements or materials, so what comes back is an empty project carrying a name, a
        /// number, a description and a design life.
        /// </summary>
        private static CarboProject convertToCarboCalcProject(Lcax lcaxFile)
        {
            CarboProject result = new CarboProject();

            if (lcaxFile == null)
                return result;

            result.Name = lcaxFile.Name;
            result.Description = lcaxFile.Description;
            result.Number = lcaxFile.Id;

            if (lcaxFile.ReferenceStudyPeriod.HasValue == true)
                result.designLife = lcaxFile.ReferenceStudyPeriod.Value;

            return result;

        }

        private static JsCarboElement ConvertoToJsCarboElement(CarboElement ce, CarboProject carboProject)
        {
            JsCarboElement JsCe = new JsCarboElement();

            JsCe.Name = ce.Name;
            JsCe.Id = ce.Id;
            JsCe.GUID = ce.GUID;
            JsCe.MaterialName = ce.MaterialName;
            JsCe.CarboMaterialName = ce.CarboMaterialName;
            JsCe.Category = ce.Category;
            JsCe.SubCategory = ce.SubCategory;
            JsCe.AdditionalData = ce.AdditionalData;
            JsCe.Grade = ce.Grade;
            JsCe.LevelName = ce.LevelName;

            JsCe.RCDensity = ce.rcDensity;
            JsCe.Correction = ce.Correction;
            JsCe.GUID = ce.GUID;

            JsCe.Volume = ce.Volume;
            JsCe.Volume_Total = ce.Volume_Total;
            JsCe.Mass = ce.Mass;
            JsCe.Level = ce.Level;
            JsCe.Density = ce.Density;
            JsCe.Area = ce.Area;

            JsCe.ECI = ce.ECI;
            JsCe.EC = ce.EC;

            JsCe.Volume_Cumulative = ce.Volume_Cumulative;
            JsCe.ECI_Cumulative = ce.ECI_Cumulative;
            JsCe.EC_Cumulative = ce.EC_Cumulative;

            JsCe.isDemolished = ce.isDemolished;
            JsCe.isExisting = ce.isExisting;
            JsCe.isSubstructure = ce.isSubstructure;
            JsCe.includeInCalc = ce.includeInCalc;

            return JsCe;

        }
        private static JsCarboElement ConvertoToJsCarboElement(CarboGroup grp, CarboProject carboProject)
        {
            JsCarboElement JsCe = new JsCarboElement();

            JsCe.Name = grp.Description;
            JsCe.Id = grp.Id;
            JsCe.MaterialName = grp.MaterialName;
            JsCe.Category = grp.Category;
            JsCe.SubCategory = grp.SubCategory;
            JsCe.AdditionalData = grp.additionalData;
            JsCe.Grade = "";
            JsCe.LevelName = "";

            JsCe.RCDensity = 0;
            JsCe.Correction = grp.Correction;
            JsCe.GUID = "";

            JsCe.Volume = grp.Volume;
            JsCe.Volume_Total = grp.TotalVolume;

            JsCe.Area = 0;

            JsCe.Level = 0;
            JsCe.Density = grp.Density;

            JsCe.ECI = grp.ECI;
            JsCe.EC = grp.EC;
            //The first of these two used to be a second write to EC_Cumulative, so the
            //cumulative ECI of an element-less group always exported as zero.
            JsCe.ECI_Cumulative = grp.ECI;
            JsCe.EC_Cumulative = grp.EC;

            JsCe.isDemolished = grp.isDemolished;
            JsCe.isExisting = grp.isExisting;
            JsCe.isSubstructure = grp.isSubstructure;
            JsCe.includeInCalc = true;

            //ToalValues
            JsCe.EC_A1A3_Total = grp.getTotalA1A3;
            JsCe.EC_A4_Total = grp.getTotalA4;
            JsCe.EC_A5_Total = grp.getTotalA5;
            JsCe.EC_B1B7_Total = grp.getTotalB1B7;
            JsCe.EC_C1C4_Total = grp.getTotalC1C4;
            JsCe.EC_D_Total = grp.getTotalD;
            JsCe.EC_Mix_Total = grp.getTotalMix;
            JsCe.EC_Sequestration_Total = grp.getTotalSeq;


            return JsCe;

        }

        public static bool ExportToLCAx(string path, CarboProject carboLifeProject)
        {
            bool result = false;

            if (string.IsNullOrWhiteSpace(path) || carboLifeProject == null)
                return false;

            try
            {
                //This used to build the LCAx project and then serialise the flat JsCarboProject
                //beside it, so the LCAx menu item wrote the same file as the plain JSON one.
                Lcax lcaProject = convertToLCAx(carboLifeProject);

                string jsonString = JsonSerializer.Serialize(lcaProject, getLcaxSerializerOptions());

                string folder = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(folder) == false && Directory.Exists(folder) == false)
                    Directory.CreateDirectory(folder);

                File.WriteAllText(path, jsonString);

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return result;

        }

        public static bool openLCAx(string path, out CarboProject carboLifeProject)
        {
            bool result = false;

            carboLifeProject = new CarboProject();

            try
            {
                //Read the file. This used to hand the path itself to the deserialiser, which
                //could only ever throw: a path is not a JSON document.
                string jsonString = File.ReadAllText(path);

                Lcax lcaxFile = JsonSerializer.Deserialize<Lcax>(jsonString, getLcaxSerializerOptions());

                carboLifeProject = convertToCarboCalcProject(lcaxFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                carboLifeProject = null;
                return false;
            }

            result = true;
            return result;

        }



        public static string GetLCAxFileLocation()
        {
            string result = "";
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                //The trailing pipe this filter used to carry split into an odd number of
                //segments, which the dialog rejects outright, so the setter threw, the catch
                //below swallowed it and this method could only ever return "".
                openFileDialog.Filter = "LCAx File (*.json)|*.json";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != true)
                    return "";

                if (string.IsNullOrWhiteSpace(openFileDialog.FileName) == false)
                {
                    result = openFileDialog.FileName;
                }
            }
            catch
            {
                return "";
            }

            return result;
        }


    }
}
