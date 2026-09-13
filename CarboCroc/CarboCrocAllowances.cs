using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using CarboLifeAPI.Data;
using CarboLifeAPI.Data.Superseded;
using Grasshopper.Kernel;

namespace CarboCroc
{
    /// <summary>
    /// The reinforcement and connection allowances, as a Grasshopper user states them.
    ///
    /// These are the same allowances the main application builds at import, and they are driven
    /// entirely by the import settings and a group's category, with nothing Revit specific in
    /// either. The settings themselves live in a dialog the Grasshopper user never opens, so
    /// this carries them into a definition instead, where they are visible next to the model
    /// they apply to.
    /// </summary>
    public class CarboCrocAllowanceSet
    {
        public string RebarMaterialName { get; set; }
        public string RebarMaterialCategory { get; set; }

        /// <summary>Element category to kg per m3, for rates not stated on the elements.</summary>
        public List<CarboNumProperty> RebarRates { get; set; }

        public bool SteelConnections { get; set; }
        public string SteelConnectionMaterialName { get; set; }
        public string SteelMaterialCategory { get; set; }
        public double SteelConnectionPercentage { get; set; }

        public bool TimberConnections { get; set; }
        public string TimberConnectionMaterialName { get; set; }
        public string TimberMaterialCategory { get; set; }
        public double TimberConnectionPercentage { get; set; }

        public CarboCrocAllowanceSet()
        {
            RebarMaterialName = "";
            RebarMaterialCategory = "";
            RebarRates = new List<CarboNumProperty>();

            SteelConnections = false;
            SteelConnectionMaterialName = "";
            SteelMaterialCategory = "";
            SteelConnectionPercentage = 0;

            TimberConnections = false;
            TimberConnectionMaterialName = "";
            TimberMaterialCategory = "";
            TimberConnectionPercentage = 0;
        }

        /// <summary>
        /// Writes these allowances onto a project's import settings, which is where
        /// CreateReinforcementGroup and CreateConnectionGroups read them from.
        /// </summary>
        internal void ApplyTo(CarboGroupSettings settings)
        {
            if (settings == null)
                return;

            settings.RCMaterialName = RebarMaterialName;
            settings.RCMaterialCategory = RebarMaterialCategory;
            settings.rcQuantityMap = RebarRates ?? new List<CarboNumProperty>();

            settings.mapSteelConnections = SteelConnections;
            settings.SteelConnectionMaterialName = SteelConnectionMaterialName;
            settings.SteelMaterialCategory = SteelMaterialCategory;
            settings.SteelConnectionPercentage = SteelConnectionPercentage;

            settings.mapTimberConnections = TimberConnections;
            settings.TimberConnectionMaterialName = TimberConnectionMaterialName;
            settings.TimberMaterialCategory = TimberMaterialCategory;
            settings.TimberConnectionPercentage = TimberConnectionPercentage;
        }

        /// <summary>Whether a reinforcement allowance has been asked for at all.</summary>
        internal bool WantsReinforcement
        {
            get
            {
                return string.IsNullOrWhiteSpace(RebarMaterialName) == false
                    && string.IsNullOrWhiteSpace(RebarMaterialCategory) == false;
            }
        }
    }

    public class CarboCrocAllowances : GH_Component
    {
        public CarboCrocAllowances()
        : base("Carbo Allowances", "Carbo Allowances",
               "Reinforcement and connection allowances, as a percentage or a rate on the groups they belong to",
               "CarboCroc", "Solvers")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Rebar Material", "RM",
                "Reinforcement material, named exactly as the template names it. Leave empty for no reinforcement allowance.",
                GH_ParamAccess.item, "");                                                       //0
            pManager.AddTextParameter("Rebar In Category", "RC",
                "Material category the reinforcement is added to, for example Concrete.",
                GH_ParamAccess.item, "Concrete");                                               //1
            pManager.AddTextParameter("Rebar Rates", "RR",
                "Reinforcement rates for elements that do not state their own, one per line as Category=kg/m3, for example Floors=110. A rate set on the element itself always wins.",
                GH_ParamAccess.list);                                                           //2

            pManager.AddBooleanParameter("Steel Connections", "SC",
                "Add a steel connection allowance.", GH_ParamAccess.item, false);               //3
            pManager.AddTextParameter("Steel Conn Material", "SM",
                "Steel connection material, named exactly as the template names it.",
                GH_ParamAccess.item, "");                                                       //4
            pManager.AddTextParameter("Steel In Category", "SIC",
                "Material category the steel allowance is added to.", GH_ParamAccess.item, "Steel"); //5
            pManager.AddNumberParameter("Steel Conn %", "S%",
                "Steel connection allowance as a percentage of the group volume.",
                GH_ParamAccess.item, 5);                                                        //6

            pManager.AddBooleanParameter("Timber Connections", "TC",
                "Add a timber connection allowance.", GH_ParamAccess.item, false);              //7
            pManager.AddTextParameter("Timber Conn Material", "TM",
                "Timber connection material, named exactly as the template names it.",
                GH_ParamAccess.item, "");                                                       //8
            pManager.AddTextParameter("Timber In Category", "TIC",
                "Material category the timber allowance is added to.", GH_ParamAccess.item, "Timber"); //9
            pManager.AddNumberParameter("Timber Conn %", "T%",
                "Timber connection allowance as a percentage of the group volume.",
                GH_ParamAccess.item, 0.15);                                                     //10

            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Allowances", "A", "Pass these into the project solver"); //0
            pManager.Register_StringParam("Message", "M", "What was understood, and anything that was not"); //1
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            CarboCrocAllowanceSet result = new CarboCrocAllowanceSet();
            List<string> notes = new List<string>();

            string rebarMaterial = "";
            string rebarCategory = "";
            List<string> rebarRates = new List<string>();

            DA.GetData(0, ref rebarMaterial);
            DA.GetData(1, ref rebarCategory);
            DA.GetDataList(2, rebarRates);

            bool steelOn = false;
            string steelMaterial = "";
            string steelCategory = "";
            double steelPercent = 0;

            DA.GetData(3, ref steelOn);
            DA.GetData(4, ref steelMaterial);
            DA.GetData(5, ref steelCategory);
            DA.GetData(6, ref steelPercent);

            bool timberOn = false;
            string timberMaterial = "";
            string timberCategory = "";
            double timberPercent = 0;

            DA.GetData(7, ref timberOn);
            DA.GetData(8, ref timberMaterial);
            DA.GetData(9, ref timberCategory);
            DA.GetData(10, ref timberPercent);

            result.RebarMaterialName = (rebarMaterial ?? "").Trim();
            result.RebarMaterialCategory = (rebarCategory ?? "").Trim();
            result.RebarRates = ParseRates(rebarRates, notes);

            result.SteelConnections = steelOn;
            result.SteelConnectionMaterialName = (steelMaterial ?? "").Trim();
            result.SteelMaterialCategory = (steelCategory ?? "").Trim();
            result.SteelConnectionPercentage = steelPercent;

            result.TimberConnections = timberOn;
            result.TimberConnectionMaterialName = (timberMaterial ?? "").Trim();
            result.TimberMaterialCategory = (timberCategory ?? "").Trim();
            result.TimberConnectionPercentage = timberPercent;

            //Say now, while the names are on screen, whether the template actually holds them.
            //getConnectionGroups and CreateReinforcementGroup both give up quietly on a name or a
            //category that is not there - CreateReinforcementGroup returns 0 and says nothing -
            //so without this the allowance simply never appears and the total looks complete.
            CarboDatabase database = LoadDatabase(notes);

            if (database != null)
            {
                CheckMaterial(database, result.RebarMaterialName, "reinforcement", true, notes);
                CheckCategory(database, result.RebarMaterialCategory, "reinforcement", notes);

                if (result.SteelConnections)
                {
                    CheckMaterial(database, result.SteelConnectionMaterialName, "steel connection", false, notes);
                    CheckCategory(database, result.SteelMaterialCategory, "steel connection", notes);
                }

                if (result.TimberConnections)
                {
                    CheckMaterial(database, result.TimberConnectionMaterialName, "timber connection", false, notes);
                    CheckCategory(database, result.TimberMaterialCategory, "timber connection", notes);
                }
            }

            if (result.SteelConnections && result.SteelConnectionPercentage <= 0)
                notes.Add("Steel connections are on but the percentage is " + steelPercent
                          + ", so no allowance will be built.");

            if (result.TimberConnections && result.TimberConnectionPercentage <= 0)
                notes.Add("Timber connections are on but the percentage is " + timberPercent
                          + ", so no allowance will be built.");

            if (string.IsNullOrWhiteSpace(result.RebarMaterialName) == false
                && string.IsNullOrWhiteSpace(result.RebarMaterialCategory))
                notes.Add("A reinforcement material was named but no material category to add it to, "
                          + "so no reinforcement will be built.");

            foreach (string note in notes)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, note);

            DA.SetData(0, result);
            DA.SetData(1, notes.Count == 0 ? "Ok" : string.Join(Environment.NewLine, notes.ToArray()));
        }

        /// <summary>
        /// Reads the "Category=kg/m3" lines. Invariant and current culture are both tried, so a
        /// comma typed as the decimal separator on a Dutch or German machine still reads as one.
        /// </summary>
        private static List<CarboNumProperty> ParseRates(List<string> lines, List<string> notes)
        {
            List<CarboNumProperty> result = new List<CarboNumProperty>();

            if (lines == null)
                return result;

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                int split = line.LastIndexOfAny(new[] { '=', ':' });
                if (split <= 0 || split == line.Length - 1)
                {
                    notes.Add("Could not read the reinforcement rate \"" + line
                              + "\". Write one per line as Category=kg/m3, for example Floors=110.");
                    continue;
                }

                string category = line.Substring(0, split).Trim();
                string value = line.Substring(split + 1).Trim();

                double rate;
                if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out rate) == false
                    && double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out rate) == false)
                {
                    notes.Add("\"" + value + "\" in \"" + line + "\" is not a number.");
                    continue;
                }

                if (rate < 0)
                {
                    notes.Add("The reinforcement rate for " + category + " is negative and was ignored.");
                    continue;
                }

                result.Add(new CarboNumProperty { PropertyName = category, Value = rate });
            }

            return result;
        }

        /// <summary>The template these names are checked against, read once.</summary>
        private static CarboDatabase LoadDatabase(List<string> notes)
        {
            try
            {
                return new CarboProject(CarboCrocUtils.getSetTemplatePath("")).CarboDatabase;
            }
            catch (Exception ex)
            {
                notes.Add("Could not read the template to check these names against: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Checks a material name against the template. Reinforcement additionally needs a
        /// density, because the allowance is a mass converted into a volume by dividing by it.
        /// </summary>
        private static void CheckMaterial(CarboDatabase database, string name, string what,
                                          bool needsDensity, List<string> notes)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                if (what != "reinforcement")
                    notes.Add("No " + what + " material was named, so no allowance will be built.");
                return;
            }

            CarboMaterial found = database.GetExcactMatch(name);

            if (found == null)
            {
                notes.Add("The template holds no material called \"" + name
                          + "\", so the " + what + " allowance will be built from the closest match instead. "
                          + "Use the material list or selector component to get the name exactly right.");
                return;
            }

            if (needsDensity && found.Density <= 0)
                notes.Add("\"" + name + "\" has no density, so a reinforcement mass cannot be turned "
                          + "into a volume and no allowance will be built.");
        }

        /// <summary>
        /// Checks the category an allowance is added to.
        ///
        /// This is the category of the material in the template, not the category of the
        /// elements, and the two are easy to confuse: the shipped template puts
        /// "Concrete - Mass Concrete" in "Mass Concrete" rather than in "Concrete", so an
        /// allowance aimed at "Concrete" passes over it and nothing is built.
        /// </summary>
        private static void CheckCategory(CarboDatabase database, string category, string what,
                                          List<string> notes)
        {
            if (string.IsNullOrWhiteSpace(category))
                return;   //Reported separately, together with the material name.

            List<string> available = new List<string>();

            foreach (CarboMaterial cm in database.CarboMaterialList)
            {
                if (cm == null || string.IsNullOrWhiteSpace(cm.Category))
                    continue;

                if (string.Equals(cm.Category, category, StringComparison.OrdinalIgnoreCase))
                    return;   //Something carries it, nothing to say.

                if (available.Contains(cm.Category) == false)
                    available.Add(cm.Category);
            }

            available.Sort(StringComparer.InvariantCultureIgnoreCase);

            notes.Add("No material in the template is in the category \"" + category
                      + "\", so the " + what + " allowance will be added to nothing. "
                      + "This is the material's own category in the template, not the element category. "
                      + "The template offers: " + string.Join(", ", available.ToArray()) + ".");
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{6E1C9A54-3D8B-4A17-9F52-0C7B41E6D2A8}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return CarboCroc.Properties.Resources.CarboGroup; }
        }
    }
}
