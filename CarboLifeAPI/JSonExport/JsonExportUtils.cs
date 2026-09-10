using CarboLifeAPI.Data;
using LCAx;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
//using System.Web.Script.Serialization;
using System.Text;
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
        /// What came out of an LCAx file, and what did not.
        /// </summary>
        public class LcaxImportResult
        {
            public CarboProject Project { get; set; }

            public int AssembliesRead { get; set; }
            public int AssembliesSkipped { get; set; }
            public int ProductsRead { get; set; }
            public int ProductsSkipped { get; set; }

            /// <summary>Anything the reader had to decide, guess or give up on.</summary>
            public List<string> Notes { get; set; }

            public LcaxImportResult()
            {
                Notes = new List<string>();
            }

            public void Note(string text)
            {
                if (Notes.Contains(text) == false)
                    Notes.Add(text);
            }

            public string Summary()
            {
                StringBuilder text = new StringBuilder();

                text.AppendLine(AssembliesRead.ToString(CultureInfo.InvariantCulture) + " assembly/assemblies read as groups, "
                    + ProductsRead.ToString(CultureInfo.InvariantCulture) + " product(s) read as elements.");

                if (AssembliesSkipped > 0 || ProductsSkipped > 0)
                    text.AppendLine("Skipped: " + AssembliesSkipped.ToString(CultureInfo.InvariantCulture)
                        + " assembly/assemblies and " + ProductsSkipped.ToString(CultureInfo.InvariantCulture) + " product(s).");

                if (Notes.Count > 0)
                {
                    text.AppendLine();
                    foreach (string note in Notes)
                        text.AppendLine("- " + note);
                }

                return text.ToString();
            }
        }

        /// <summary>
        /// Reads an LCAx file into a Carbo Life project.
        /// </summary>
        /// <remarks>
        /// The mapping is the reverse of the export: an assembly is a group, a product is an
        /// element, and a product's EPD is a material. What cannot be reversed is reported
        /// rather than guessed at, because a file from another tool is not obliged to carry what
        /// this application needs.
        ///
        /// Two things are worth knowing about the numbers.
        ///
        /// This application works in volume and density: mass is volume times density and the
        /// carbon is mass times an intensity per kg. An LCAx product states a quantity in
        /// whatever unit suits it, and an EPD states its impacts per its own declared unit, so
        /// both have to be brought back to that footing. Density is looked for in the EPD's
        /// metadata (which is where this application's own export puts it) and then in the EPD's
        /// unit conversions. Where it cannot be found, the density is set to 1 so that the
        /// carbon still comes out right, and a note says so, because the mass column will then
        /// read as the volume rather than as a mass.
        ///
        /// EN 15804 counts biogenic carbon inside the total, and the export writes sequestration
        /// into gwp a1a3 with a copy of its own under gwp_bio. Reading back therefore subtracts
        /// gwp_bio from a1a3 and puts it in the sequestration field, or the credit would be
        /// counted twice. The mix allowance has no life cycle stage of its own in LCAx, so it
        /// cannot be separated out again and stays inside a1a3: a round trip through LCAx moves
        /// that number rather than losing it.
        /// </remarks>
        public static LcaxImportResult ImportLCAx(string path)
        {
            LcaxImportResult result = new LcaxImportResult();

            if (string.IsNullOrWhiteSpace(path) || File.Exists(path) == false)
            {
                result.Note("The file could not be found.");
                return result;
            }

            Lcax lcaxFile;

            try
            {
                string jsonString = File.ReadAllText(path);
                lcaxFile = JsonSerializer.Deserialize<Lcax>(jsonString, getLcaxSerializerOptions());
            }
            catch (Exception ex)
            {
                result.Note("The file could not be read as LCAx: " + ex.Message);
                return result;
            }

            if (lcaxFile == null)
            {
                result.Note("The file held no LCAx project.");
                return result;
            }

            result.Project = convertToCarboCalcProject(lcaxFile, result);

            return result;
        }

        /// <summary>
        /// Builds a Carbo Life project out of a deserialised LCAx file.
        /// </summary>
        private static CarboProject convertToCarboCalcProject(Lcax lcaxFile, LcaxImportResult report)
        {
            CarboProject result = new CarboProject();

            if (lcaxFile == null)
                return result;

            //Header
            result.Name = string.IsNullOrWhiteSpace(lcaxFile.Name) ? "Imported LCAx project" : lcaxFile.Name;
            result.Description = lcaxFile.Description;
            result.Number = lcaxFile.Id;

            if (string.IsNullOrWhiteSpace(lcaxFile.Comment) == false)
                result.Category = lcaxFile.Comment;

            if (lcaxFile.ReferenceStudyPeriod.HasValue && lcaxFile.ReferenceStudyPeriod.Value > 0)
                result.designLife = (int)lcaxFile.ReferenceStudyPeriod.Value;

            //The uncertainty factor cuts both ways and has to be handled on both sides.
            //
            //A quantity in an LCAx file is final: whatever allowance the tool that wrote it made
            //is already inside the number. But this application does not store a quantity, it
            //stores a net volume and re-applies waste and uncertainty on every calculation, and
            //it also DERIVES the A5 and C1 globals from the floor area times that same factor.
            //Zeroing the factor therefore fixes the quantities and shrinks the globals; keeping
            //it fixes the globals and inflates the quantities.
            //
            //So the factor is restored where the file records it, and the volumes are divided by
            //it on the way in, so that re-applying it lands back on the number in the file.
            result.UncertFact = 0;
            readLcaxGlobal(lcaxFile, "uncertaintyFactor", v => result.UncertFact = v);

            double quantityDivisor = 1 + result.UncertFact;

            if (quantityDivisor <= 0)
                quantityDivisor = 1;

            //Gross floor area, so the per m2 figures mean something straight away.
            if (lcaxFile.ProjectInfo != null && lcaxFile.ProjectInfo.GrossFloorArea != null
                && lcaxFile.ProjectInfo.GrossFloorArea.Value > 0)
            {
                result.Area = lcaxFile.ProjectInfo.GrossFloorArea.Value;
                result.AreaNew = result.Area;
            }
            else
            {
                report.Note("No gross floor area in the file, so the per m2 figures will read against the default area until it is set.");
            }

            applyLcaxReportedStages(lcaxFile, result, report);

            //This application's own exports put the globals in metadata; another tool's file will
            //not have them, and they simply stay at zero.
            //A0 is a figure the user enters, and the property that reads it back applies the
            //uncertainty factor, so the uplifted number in the metadata has to come back down
            //before it is stored or it gains the factor twice.
            readLcaxGlobal(lcaxFile, "a0GlobalTCo2e", v => result.A0Global = (v * 1000) / quantityDivisor);
            readLcaxGlobal(lcaxFile, "socialCostPerTonne", v => result.SocialCost = v);

            //The A5, C1 and B6-B7 globals are not stored: the calculation derives them from the
            //floor area, the demolition area and the energy figures every time it runs, so
            //anything put here would be overwritten a moment later. A file from another tool
            //carries none of those inputs, and those globals will read as zero until they are
            //entered.

            if (lcaxFile.Assemblies == null || lcaxFile.Assemblies.Count == 0)
            {
                report.Note("The file holds no assemblies, so there is nothing to build groups from.");
                return result;
            }

            //One material per EPD, shared between the products that reference it.
            Dictionary<string, CarboMaterial> materials = new Dictionary<string, CarboMaterial>();
            int nextMaterialId = 900000;
            int nextGroupId = 1;

            List<CarboGroup> groups = new List<CarboGroup>();

            foreach (KeyValuePair<string, Assembly> entry in lcaxFile.Assemblies)
            {
                Assembly assembly = entry.Value;

                if (assembly == null)
                {
                    report.AssembliesSkipped++;
                    continue;
                }

                //The "reference" half of the union points at an assembly held somewhere else.
                //There is nothing here to read, and following a uri is not something an import
                //should do on its own.
                if (string.IsNullOrEmpty(assembly.Type) == false
                    && assembly.Type.Equals("reference", StringComparison.OrdinalIgnoreCase))
                {
                    report.AssembliesSkipped++;
                    report.Note("One or more assemblies are references to another file and were skipped.");
                    continue;
                }

                if (assembly.Products == null || assembly.Products.Count == 0)
                {
                    report.AssembliesSkipped++;
                    report.Note("One or more assemblies hold no products and were skipped.");
                    continue;
                }

                List<CarboElement> elements = new List<CarboElement>();
                CarboMaterial groupMaterial = null;

                foreach (KeyValuePair<string, Product> productEntry in assembly.Products)
                {
                    Product product = productEntry.Value;

                    if (product == null)
                    {
                        report.ProductsSkipped++;
                        continue;
                    }

                    if (string.IsNullOrEmpty(product.Type) == false
                        && product.Type.Equals("reference", StringComparison.OrdinalIgnoreCase))
                    {
                        report.ProductsSkipped++;
                        report.Note("One or more products are references to another file and were skipped.");
                        continue;
                    }

                    CarboMaterial material = materialFromLcax(product, materials, ref nextMaterialId, report);

                    double density = material == null || material.Density <= 0 ? 1 : material.Density;
                    double volume = lcaxQuantityToVolume(product.Quantity, product.Unit, density, report);

                    if (double.IsNaN(volume))
                    {
                        report.ProductsSkipped++;
                        continue;
                    }

                    //Net, so that the uncertainty factor re-applied by the calculation lands
                    //back on the quantity the file states.
                    volume = volume / quantityDivisor;

                    CarboElement element = new CarboElement();

                    element.Id = nextElementId(product, elements.Count);
                    element.Name = string.IsNullOrWhiteSpace(product.Name) ? productEntry.Key : product.Name;
                    element.GUID = product.Id == null ? "" : product.Id;
                    element.Category = string.IsNullOrWhiteSpace(assembly.Comment) ? assembly.Name : assembly.Comment;
                    element.SubCategory = "";
                    element.Volume = volume;
                    element.Volume_Total = volume;
                    element.includeInCalc = true;

                    if (material != null)
                    {
                        element.MaterialName = material.Name;
                        element.CarboMaterialName = material.Name;
                        element.MaterialCategoryName = material.Category;
                        element.Grade = material.Grade;
                        element.Density = material.Density;
                    }

                    readLcaxProductMetaData(product, element);

                    elements.Add(element);
                    report.ProductsRead++;

                    if (groupMaterial == null)
                        groupMaterial = material;
                }

                if (elements.Count == 0)
                {
                    report.AssembliesSkipped++;
                    continue;
                }

                CarboGroup group = new CarboGroup();

                group.Id = nextGroupId++;
                group.Category = string.IsNullOrWhiteSpace(assembly.Comment) ? assembly.Name : assembly.Comment;
                group.SubCategory = "";
                group.Description = assembly.Name;
                group.Origin = CarboGroupOrigin.Import;

                if (groupMaterial != null)
                    group.setMaterial(groupMaterial);

                //setMaterial puts the material's default waste allowance on the group. An LCAx
                //quantity is the quantity, already carrying whatever waste the tool that wrote
                //it applied, so adding the default on top would inflate it a second time.
                group.Waste = 0;

                foreach (CarboElement element in elements)
                    group.AllElements.Add(element);

                group.Volume = elements.Sum(x => x.Volume);

                groups.Add(group);
                report.AssembliesRead++;
            }

            //The materials the products referred to, so they can be edited afterwards.
            foreach (CarboMaterial material in materials.Values)
                result.CarboDatabase.AddMaterial(material);

            result.AddGroups(groups);
            result.CalculateProject();

            return result;
        }

        /// <summary>
        /// Turns the file's lifeCycleStages into this application's calculation switches.
        /// </summary>
        /// <remarks>
        /// This matters more than it looks. A group's ECI is assembled from the material's
        /// stages according to these switches, so a project that imported every figure
        /// correctly but left the switches at their defaults still reported a different total:
        /// stage D is off by default, and the steel credit of -1.61 kgCO2e/kg simply was not in
        /// the sum. lifeCycleStages is the file saying which stages its assessment covers, which
        /// is exactly the question the switches answer.
        /// </remarks>
        private static void applyLcaxReportedStages(Lcax lcaxFile, CarboProject result, LcaxImportResult report)
        {
            if (lcaxFile.LifeCycleStages == null || lcaxFile.LifeCycleStages.Count == 0)
            {
                report.Note("The file does not say which life cycle stages it covers, so the "
                    + "calculation switches have been left at their defaults. Check them against the source.");
                return;
            }

            List<LifeCycleStage> stages = lcaxFile.LifeCycleStages;

            result.calculateA0 = stages.Contains(LifeCycleStage.A0);
            result.calculateA13 = stages.Contains(LifeCycleStage.A1A3);
            result.calculateA4 = stages.Contains(LifeCycleStage.A4);
            result.calculateA5 = stages.Contains(LifeCycleStage.A5);

            result.calculateB = stages.Contains(LifeCycleStage.B1) || stages.Contains(LifeCycleStage.B2)
                || stages.Contains(LifeCycleStage.B3) || stages.Contains(LifeCycleStage.B4)
                || stages.Contains(LifeCycleStage.B5);

            result.calculateB67 = stages.Contains(LifeCycleStage.B6) || stages.Contains(LifeCycleStage.B7);

            result.calculateC = stages.Contains(LifeCycleStage.C1) || stages.Contains(LifeCycleStage.C2)
                || stages.Contains(LifeCycleStage.C3) || stages.Contains(LifeCycleStage.C4);

            result.calculateD = stages.Contains(LifeCycleStage.D);

            //Sequestration has no life cycle stage of its own; it is reported as a biogenic
            //impact category, so its presence there is what says it was assessed.
            result.calculateSeq = lcaxFile.ImpactCategories != null
                && lcaxFile.ImpactCategories.Contains(ImpactCategoryKey.GwpBio);

            //The mix allowance is this application's own idea and has nowhere to live in LCAx,
            //so it cannot be read back either way.
            report.Note("Calculation switches were set from the file's lifeCycleStages. The mix "
                + "allowance has no equivalent in LCAx and has been left at its default.");
        }

        private static void readLcaxGlobal(Lcax lcaxFile, string key, Action<double> apply)
        {
            if (lcaxFile.MetaData == null)
                return;

            string text;
            if (lcaxFile.MetaData.TryGetValue(key, out text) == false)
                return;

            double value;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                apply(value);
        }

        private static long nextElementId(Product product, int fallback)
        {
            long id;

            if (product.Id != null && long.TryParse(product.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                return id;

            return fallback + 1;
        }

        /// <summary>
        /// Copies back the few element facts this application's own export writes into a
        /// product's metadata. A file from another tool simply has none of these.
        /// </summary>
        private static void readLcaxProductMetaData(Product product, CarboElement element)
        {
            if (product.MetaData == null)
                return;

            string text;

            if (product.MetaData.TryGetValue("revitMaterialName", out text))
                element.MaterialName = text;

            if (product.MetaData.TryGetValue("category", out text) && string.IsNullOrWhiteSpace(text) == false)
                element.Category = text;

            if (product.MetaData.TryGetValue("subCategory", out text))
                element.SubCategory = text;

            if (product.MetaData.TryGetValue("levelName", out text))
                element.LevelName = text;

            if (product.MetaData.TryGetValue("levelElevation", out text))
                element.Level = DataExportUtils.ReadCsvDouble(text);

            if (product.MetaData.TryGetValue("isSubstructure", out text))
                element.isSubstructure = text.Trim().ToLowerInvariant() == "true";

            if (product.MetaData.TryGetValue("isDemolished", out text))
                element.isDemolished = text.Trim().ToLowerInvariant() == "true";

            if (product.MetaData.TryGetValue("isExisting", out text))
                element.isExisting = text.Trim().ToLowerInvariant() == "true";

            if (product.MetaData.TryGetValue("includedInCalculation", out text))
                element.includeInCalc = text.Trim().ToLowerInvariant() != "false";

            if (product.MetaData.TryGetValue("volumeCorrection", out text))
                element.Correction = text;

            if (product.MetaData.TryGetValue("additionalData", out text))
                element.AdditionalData = text;
        }

        /// <summary>
        /// A product quantity brought back to cubic metres, which is what this application
        /// calculates in. Returns NaN when the unit cannot be converted.
        /// </summary>
        private static double lcaxQuantityToVolume(double quantity, Unit unit, double density, LcaxImportResult report)
        {
            if (unit == Unit.M3)
                return quantity;

            if (unit == Unit.Kg)
                return density > 0 ? quantity / density : quantity;

            if (unit == Unit.Tones)
                return density > 0 ? (quantity * 1000) / density : quantity * 1000;

            report.Note("Quantities given in " + LcaxKey.Of(unit) + " cannot be turned into a volume, "
                + "so those products were skipped. This application calculates in m3 and kg.");

            return double.NaN;
        }

        /// <summary>
        /// The material behind a product, built from its EPD and shared between the products
        /// that name the same one.
        /// </summary>
        private static CarboMaterial materialFromLcax(Product product, Dictionary<string, CarboMaterial> materials,
            ref int nextMaterialId, LcaxImportResult report)
        {
            Epd epd = product.ImpactData;

            if (epd == null)
            {
                report.Note("One or more products carry no impact data and were imported with no material.");
                return null;
            }

            if (string.IsNullOrEmpty(epd.Type) == false
                && epd.Type.Equals("reference", StringComparison.OrdinalIgnoreCase))
            {
                report.Note("One or more products point at an EPD held in another file, so they were imported with no material.");
                return null;
            }

            string key = string.IsNullOrWhiteSpace(epd.Id) ? epd.Name : epd.Id;

            if (string.IsNullOrWhiteSpace(key))
                key = "unnamed";

            CarboMaterial existing;
            if (materials.TryGetValue(key, out existing))
                return existing;

            CarboMaterial material = new CarboMaterial();

            int parsedId;
            material.Id = int.TryParse(epd.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedId)
                ? parsedId
                : nextMaterialId++;

            material.Name = string.IsNullOrWhiteSpace(epd.Name) ? key : epd.Name;
            material.Description = epd.Comment;
            material.Category = readLcaxMetaData(epd.MetaData, "category", "Other");
            material.Grade = readLcaxMetaData(epd.MetaData, "grade", "");

            if (epd.Source != null && string.IsNullOrWhiteSpace(epd.Source.Url) == false)
                material.EPDurl = epd.Source.Url;

            material.Density = lcaxDensity(epd, report);

            applyLcaxImpacts(epd, material, report);

            //The overrides keep the values that came out of the file rather than letting the
            //A1A3 table rules put them back to whatever the name suggests.
            material.ECI_A1A3_Override = true;
            material.ECI_A4_Override = true;
            material.ECI_A5_Override = true;
            material.ECI_C1C4_Override = true;
            material.ECI_D_Override = true;
            material.ECI_Seq_Override = true;

            material.CalculateTotals();

            materials.Add(key, material);

            return material;
        }

        private static string readLcaxMetaData(Dictionary<string, string> metaData, string key, string fallback)
        {
            if (metaData == null)
                return fallback;

            string text;
            return metaData.TryGetValue(key, out text) && string.IsNullOrWhiteSpace(text) == false ? text : fallback;
        }

        /// <summary>
        /// The density of an EPD's material in kg/m3: from the metadata this application's own
        /// export writes, then from the EPD's own unit conversions, and 1 when neither says.
        /// </summary>
        private static double lcaxDensity(Epd epd, LcaxImportResult report)
        {
            string text = readLcaxMetaData(epd.MetaData, "densityKgM3", "");

            double density;
            if (string.IsNullOrWhiteSpace(text) == false
                && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out density)
                && density > 0)
                return density;

            //A conversion states how much of another unit one declared unit is worth, so a kg
            //EPD converting to m3 gives 1/density, and an m3 EPD converting to kg gives density.
            if (epd.Conversions != null)
            {
                foreach (Conversion conversion in epd.Conversions)
                {
                    if (conversion == null || conversion.Value <= 0)
                        continue;

                    if (epd.DeclaredUnit == Unit.Kg && conversion.To == Unit.M3)
                        return 1 / conversion.Value;

                    if (epd.DeclaredUnit == Unit.M3 && conversion.To == Unit.Kg)
                        return conversion.Value;
                }
            }

            report.Note("One or more materials give no density, so it has been set to 1 kg/m3. "
                + "The carbon figures are right, but their mass reads as their volume until a density is entered.");

            return 1;
        }

        /// <summary>
        /// Turns an EPD's impacts into intensities per kg on the material.
        /// </summary>
        private static void applyLcaxImpacts(Epd epd, CarboMaterial material, LcaxImportResult report)
        {
            if (epd.Impacts == null || epd.Impacts.Count == 0)
            {
                report.Note("One or more EPDs carry no impact figures, so their material was imported at zero.");
                return;
            }

            Dictionary<string, double?> gwp = findLcaxImpact(epd.Impacts, ImpactCategoryKey.Gwp);

            if (gwp == null)
            {
                report.Note("One or more EPDs report no gwp figures. This application reads global warming potential only.");
                return;
            }

            Dictionary<string, double?> bio = findLcaxImpact(epd.Impacts, ImpactCategoryKey.GwpBio);

            //Per declared unit, brought to per kg.
            double perKg = 1;

            if (epd.DeclaredUnit == Unit.M3)
                perKg = material.Density > 0 ? 1 / material.Density : 1;
            else if (epd.DeclaredUnit == Unit.Tones)
                perKg = 0.001;
            else if (epd.DeclaredUnit != Unit.Kg)
                report.Note("One or more EPDs declare their impacts per " + LcaxKey.Of(epd.DeclaredUnit)
                    + ", which cannot be turned into a figure per kg. They were read as if declared per kg.");

            double sequestration = stage(bio, LifeCycleStage.A1A3) * perKg;

            //a1a3 is taken as it stands and the biogenic figure is read alongside it, NOT
            //subtracted from it. An EPD's impacts block states the material's own declared
            //figures, and the export writes a1a3 and gwp_bio from two separate fields, so
            //subtracting here counted the sequestration credit twice: a timber material came
            //back with several times the production carbon it went out with.
            //
            //The results blocks elsewhere in the file do fold sequestration into a1a3, the way
            //EN 15804 counts biogenic carbon inside the total. Those are computed figures and
            //are recalculated on import rather than read, so the two conventions do not meet.
            material.ECI_A1A3 = stage(gwp, LifeCycleStage.A1A3) * perKg;
            material.ECI_A4 = stage(gwp, LifeCycleStage.A4) * perKg;
            material.ECI_A5 = stage(gwp, LifeCycleStage.A5) * perKg;

            material.ECI_B1B5 = (stage(gwp, LifeCycleStage.B1) + stage(gwp, LifeCycleStage.B2)
                + stage(gwp, LifeCycleStage.B3) + stage(gwp, LifeCycleStage.B4)
                + stage(gwp, LifeCycleStage.B5)) * perKg;

            material.ECI_C1C4 = (stage(gwp, LifeCycleStage.C1) + stage(gwp, LifeCycleStage.C2)
                + stage(gwp, LifeCycleStage.C3) + stage(gwp, LifeCycleStage.C4)) * perKg;

            material.ECI_D = stage(gwp, LifeCycleStage.D) * perKg;
            material.ECI_Seq = sequestration;
            material.ECI_Mix = 0;
        }

        private static Dictionary<string, double?> findLcaxImpact(
            Dictionary<string, Dictionary<string, double?>> impacts, ImpactCategoryKey category)
        {
            string wanted = LcaxKey.Of(category);

            foreach (KeyValuePair<string, Dictionary<string, double?>> entry in impacts)
            {
                if (string.Equals(entry.Key, wanted, StringComparison.OrdinalIgnoreCase))
                    return entry.Value;
            }

            return null;
        }

        private static double stage(Dictionary<string, double?> figures, LifeCycleStage lifeCycleStage)
        {
            if (figures == null)
                return 0;

            string wanted = LcaxKey.Of(lifeCycleStage);

            foreach (KeyValuePair<string, double?> entry in figures)
            {
                if (string.Equals(entry.Key, wanted, StringComparison.OrdinalIgnoreCase))
                    return entry.Value.HasValue ? entry.Value.Value : 0;
            }

            return 0;
        }


        private static JsCarboElement ConvertoToJsCarboElement(CarboElement ce, CarboProject carboProject)
        {
            JsCarboElement JsCe = new JsCarboElement();

            JsCe.Name = ce.Name;
            JsCe.Id = ce.Id;
            JsCe.GUID = ce.GUID;
            JsCe.MaterialName = ce.MaterialName;
            JsCe.CarboMaterialName = ce.CarboMaterialName;
            JsCe.MaterialCategoryName = ce.MaterialCategoryName;
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
            //A group stands in for its own element here, so the matched material name is the
            //group's material. Left unset it was null, which is not what any reader expects in
            //the column beside MaterialName.
            JsCe.CarboMaterialName = grp.MaterialName;
            //The group has no Revit material class of its own; the assigned material's
            //category is the same idea and is where CarboElement takes it from too.
            JsCe.MaterialCategoryName = grp.Material != null ? grp.Material.Category : "";
            JsCe.Category = grp.Category;
            JsCe.SubCategory = grp.SubCategory;
            JsCe.AdditionalData = grp.additionalData;
            JsCe.Grade = grp.Grade;
            JsCe.LevelName = "";

            JsCe.RCDensity = grp.RcDensity;
            JsCe.Correction = grp.Correction;
            JsCe.GUID = "";

            JsCe.Volume = grp.Volume;
            JsCe.Volume_Total = grp.TotalVolume;
            JsCe.Volume_Cumulative = grp.TotalVolume;

            JsCe.Area = 0;

            JsCe.Level = 0;
            JsCe.Density = grp.Density;
            //Left unset this exported as zero, while every stage total beside it was a real
            //figure derived from exactly this mass.
            JsCe.Mass = grp.Mass;

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

        /// <summary>
        /// Opens an LCAx file as a project. Kept for callers that only want the project; use
        /// ImportLCAx directly to see what the reader had to skip.
        /// </summary>
        public static bool openLCAx(string path, out CarboProject carboLifeProject)
        {
            LcaxImportResult imported = ImportLCAx(path);

            carboLifeProject = imported.Project;

            if (carboLifeProject == null)
                return false;

            return imported.AssembliesRead > 0;
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
