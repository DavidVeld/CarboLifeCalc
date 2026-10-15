using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Asks for the three things a connection allowance needs before it is built: the material
    /// category the allowance is worked out from, the material it is made of, and the percentage.
    ///
    /// Those three used to live in the Revit import settings only. A project imported without them
    /// - which is every project where the user never opened that dialog - had nothing to work from,
    /// so the generator produced no groups and the interface could only report afterwards which
    /// setting was missing. This asks first instead, the way the reinforcement mapper does, and
    /// shows what the allowance comes to before anything is created.
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        private readonly CarboProject project;

        /// <summary>True for the steel allowance, false for the timber one.</summary>
        private readonly bool isSteel;

        /// <summary>The material categories the project's own groups use, sorted.</summary>
        private readonly List<string> categories = new List<string>();

        /// <summary>False while Window_Loaded fills the lists, so the preview is not run half built.</summary>
        private bool uiReady;

        /// <summary>The allowance used when the project carries no usable percentage.</summary>
        private const double defaultSteelPercentage = 5;
        private const double defaultTimberPercentage = 0.15;

        public bool isAccepted;

        /// <summary>The material category the groups getting an allowance must be in.</summary>
        public string SelectedCategory;

        /// <summary>The material the allowance is made of.</summary>
        public string SelectedMaterialName;

        /// <summary>The allowance as a percentage of the group volume.</summary>
        public double SelectedPercentage;

        /// <summary>True when the choices should also be stored as the import default.</summary>
        public bool SaveAsDefault;

        /// <param name="carboProject">The project the allowance is built in, it supplies both the
        /// material database and the groups that can be matched</param>
        /// <param name="steel">True for the steel allowance, false for the timber one</param>
        public ConnectionWindow(CarboProject carboProject, bool steel)
        {
            project = carboProject;
            isSteel = steel;

            isAccepted = false;
            SelectedCategory = "";
            SelectedMaterialName = "";
            SelectedPercentage = 0;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string volumeName = isSteel ? "steel" : "timber";

            this.Title = isSteel ? "Carbo Life: Steel Connections" : "Carbo Life: Timber Connections";
            lbl_Title.Content = isSteel ? "Steel connection allowance" : "Timber connection allowance";
            lbl_Category.Text = isSteel ? "Steelwork material category" : "Timber material category";
            lbl_PercentageUnit.Text = "% of the " + volumeName + " volume";

            //Timber connections are made of steel fixings, which is not obvious from the field name.
            if (isSteel == false)
                cbb_Material.ToolTip = "The material the generated connection groups are made of, " +
                                       "usually the steel of the fixings rather than timber";

            CarboGroupSettings settings = GetSettings();

            if (FillCategories(settings) == false)
                return;

            if (FillMaterials(settings) == false)
                return;

            double percentage = isSteel ? settings.SteelConnectionPercentage : settings.TimberConnectionPercentage;

            //A project that never had the allowance set carries a 0 here, and an allowance of 0
            //builds nothing at all, so the standard one is offered instead.
            if (percentage <= 0)
                percentage = isSteel ? defaultSteelPercentage : defaultTimberPercentage;

            txt_Percentage.Text = percentage.ToString(CultureInfo.CurrentCulture);

            uiReady = true;
            RefreshInterface();
        }

        /// <summary>
        /// The settings the allowance is read from and written back to. A project that has none is
        /// given the stored defaults rather than a null reference.
        /// </summary>
        private CarboGroupSettings GetSettings()
        {
            if (project.RevitImportSettings == null)
                project.RevitImportSettings = new CarboGroupSettings().DeSerializeXML();

            return project.RevitImportSettings;
        }

        /// <summary>
        /// Fills the category list with the material categories that are actually used by the
        /// groups of this project.
        /// A category no group uses can only ever produce an empty allowance, so offering the whole
        /// template category list here would just be another way to end up with nothing.
        /// </summary>
        /// <returns>False when the project has nothing that could carry an allowance</returns>
        private bool FillCategories(CarboGroupSettings settings)
        {
            categories.Clear();

            foreach (CarboGroup grp in GetSourceGroups())
            {
                if (categories.Contains(grp.Material.Category) == false)
                    categories.Add(grp.Material.Category);
            }

            if (categories.Count == 0)
            {
                MessageBox.Show(
                    "This project has no groups that could be given a connection allowance." +
                    Environment.NewLine + Environment.NewLine +
                    "Import or create some groups and give them a material first.",
                    this.Title, MessageBoxButton.OK, MessageBoxImage.Information);

                //Closing from Loaded is safe, ShowDialog has not returned yet.
                this.Close();
                return false;
            }

            categories.Sort();

            foreach (string category in categories)
                cbb_Category.Items.Add(category);

            string wanted = isSteel ? settings.SteelMaterialCategory : settings.TimberMaterialCategory;

            string selected = FindExact(categories, wanted);

            //Nothing usable was stored, so the name of the category is the only clue there is.
            if (selected == null)
                selected = FindByKeyword(categories, isSteel
                    ? new string[] { "steel", "metal" }
                    : new string[] { "timber", "wood" });

            cbb_Category.SelectedItem = selected;
            return true;
        }

        /// <summary>
        /// Fills the material list from the project's own database, which is the database the
        /// generator looks the connection material up in.
        /// </summary>
        /// <returns>False when the project has no materials to choose from</returns>
        private bool FillMaterials(CarboGroupSettings settings)
        {
            List<string> names = new List<string>();

            if (project.CarboDatabase != null && project.CarboDatabase.CarboMaterialList != null)
            {
                foreach (CarboMaterial cm in project.CarboDatabase.CarboMaterialList)
                {
                    if (cm != null && string.IsNullOrEmpty(cm.Name) == false)
                        names.Add(cm.Name);
                }
            }

            if (names.Count == 0)
            {
                MessageBox.Show(
                    "This project has no materials in its database, so there is nothing to make the " +
                    "connections out of." + Environment.NewLine + Environment.NewLine +
                    "Load a material template and try again.",
                    this.Title, MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
                return false;
            }

            names.Sort();

            foreach (string name in names)
                cbb_Material.Items.Add(name);

            string wanted = isSteel ? settings.SteelConnectionMaterialName : settings.TimberConnectionMaterialName;

            string selected = FindExact(names, wanted);

            //Both allowances are fixings, so both look for steel: timber connections are the bolts
            //and the plates in the timber, not more timber. Plate before the generic steel names,
            //it is the closest thing to a connection in the databases that ship with this.
            if (selected == null)
                selected = FindByKeyword(names, new string[]
                {
                    "connection", "fixing", "fastener", "bolt", "plate", "galvanised", "galvanized", "steel"
                });

            cbb_Material.SelectedItem = selected;
            return true;
        }

        /// <summary>
        /// The volume a group contributes. TotalVolume is the corrected one, the same figure the
        /// group columns show, and it is what the allowance is a percentage of. It is only filled
        /// once the project has been calculated, so the raw volume stands in until then.
        /// </summary>
        private static double GetGroupVolume(CarboGroup grp)
        {
            return grp.TotalVolume > 0 ? grp.TotalVolume : grp.Volume;
        }

        /// <summary>
        /// The groups an allowance can be worked out from: the ones holding real material, with a
        /// material on them. The same set CarboProject.getConnectionGroups walks.
        /// </summary>
        private IEnumerable<CarboGroup> GetSourceGroups()
        {
            if (project.getGroupList == null)
                yield break;

            foreach (CarboGroup grp in project.getGroupList)
            {
                //A generated group is an allowance itself and never gets one of its own.
                if (grp == null || grp.IsAutoGenerated() == true)
                    continue;

                if (grp.Material == null || string.IsNullOrEmpty(grp.Material.Category))
                    continue;

                yield return grp;
            }
        }

        /// <summary>
        /// What the allowance would come to, worked out group by group the way the generator and
        /// then CarboGroup.CalculateTotals do it, so the figures on screen are the ones the project
        /// ends up with rather than an order of magnitude.
        /// </summary>
        private class AllowancePreview
        {
            /// <summary>How many groups the category matches.</summary>
            public int GroupCount;

            /// <summary>Their volume, as the group table reports it.</summary>
            public double GroupVolume;

            /// <summary>The volume of the allowance itself.</summary>
            public double Volume;

            /// <summary>Its weight, in kg.</summary>
            public double Mass;

            /// <summary>Its embodied carbon, in tonnes.</summary>
            public double EC;
        }

        /// <param name="category">The material category the groups must be in</param>
        /// <param name="material">The material the allowance is made of, null when none is chosen</param>
        /// <param name="percentage">The allowance percentage</param>
        /// <param name="percentageValid">False when the box does not hold a usable percentage</param>
        private AllowancePreview GetPreview(string category, CarboMaterial material, double percentage, bool percentageValid)
        {
            AllowancePreview result = new AllowancePreview();

            if (string.IsNullOrEmpty(category))
                return result;

            //The stages this project actually counts, the same ones CalculateProject hands to every
            //group. The material's own ECI is the sum of all of them, including the negative module
            //D and sequestration, so reading that instead would report a different number from the
            //one the generated group ends up showing.
            double stageECI = material == null ? 0 : GetProjectECI(material);

            foreach (CarboGroup grp in GetSourceGroups())
            {
                if (grp.Material.Category != category)
                    continue;

                double volume = GetGroupVolume(grp);

                result.GroupCount++;
                result.GroupVolume += volume;

                if (material == null || percentageValid == false)
                    continue;

                double groupWaste = 1 + (grp.Waste / 100);

                //Waste is free text on the group, and a factor of 0 or less would make the preview
                //meaningless rather than merely imprecise.
                if (groupWaste <= 0)
                    groupWaste = 1;

                //The generated group is this group's volume before its waste, times the allowance,
                //carrying the waste of the connection material instead. See CarboGroup.CalculateTotals.
                double allowanceVolume = (volume / groupWaste) * (percentage / 100) * (1 + (material.WasteFactor / 100));
                double allowanceMass = allowanceVolume * material.Density;

                //Under the same switches as CarboGroup.CalculateTotals.
                double inUseECI = grp.inUseProperties == null || project.calculateB == false ? 0 : grp.inUseProperties.totalECI;
                double additional = project.calculateAdd ? grp.Additional : 0;
                double replacement = grp.inUseProperties == null ? 1 : grp.inUseProperties.B4;

                result.Volume += allowanceVolume;
                result.Mass += allowanceMass;

                //The generated group is a copy of this one, so it inherits its in use properties,
                //its replacement count and anything added to it by hand.
                result.EC += (allowanceMass * (stageECI + additional + inUseECI) * replacement) / 1000;
            }

            return result;
        }

        /// <summary>
        /// The material's ECI over the life cycle stages this project is set to count.
        /// </summary>
        private double GetProjectECI(CarboMaterial material)
        {
            double result = 0;

            if (project.calculateA13 == true)
                result += material.ECI_A1A3;
            if (project.calculateA4 == true)
                result += material.ECI_A4;
            if (project.calculateA5 == true)
                result += material.ECI_A5;
            if (project.calculateB == true)
                result += material.ECI_B1B5;
            if (project.calculateC == true)
                result += material.ECI_C1C4;
            if (project.calculateD == true)
                result += material.ECI_D;
            if (project.calculateSeq == true)
                result += material.ECI_Seq;
            if (project.calculateAdd == true)
                result += material.ECI_Mix;

            return result;
        }

        /// <summary>
        /// The entry that matches the wanted value, ignoring case and surrounding spaces.
        /// </summary>
        /// <returns>The matching entry, null when there is none or nothing was wanted</returns>
        private static string FindExact(IList<string> candidates, string wanted)
        {
            if (string.IsNullOrWhiteSpace(wanted) || candidates == null)
                return null;

            foreach (string candidate in candidates)
            {
                if (candidate != null &&
                    string.Equals(candidate.Trim(), wanted.Trim(), StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }

            return null;
        }

        /// <summary>
        /// The first entry containing one of the keywords, the keywords in order of preference.
        /// This only ever picks what the boxes open on, the user still decides.
        /// </summary>
        /// <returns>The first match, null when no entry contains any of the keywords</returns>
        private static string FindByKeyword(IList<string> candidates, string[] keywords)
        {
            if (candidates == null)
                return null;

            foreach (string keyword in keywords)
            {
                foreach (string candidate in candidates)
                {
                    if (candidate != null && candidate.ToLower().Contains(keyword))
                        return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Shows what the current choices come to: how much of the project they cover, and the
        /// volume, the weight and the carbon the allowance would add.
        /// </summary>
        private void RefreshInterface()
        {
            if (uiReady == false)
                return;

            string category = cbb_Category.SelectedItem as string;
            CarboMaterial material = GetSelectedMaterial();

            //The database is not necessarily calculated when it is read, and the stage totals are
            //what the group calculation uses, so they are brought up to date before being read.
            if (material != null)
                material.CalculateTotals();

            double percentage;
            bool percentageValid = TryReadPercentage(out percentage);

            AllowancePreview preview = GetPreview(category, material, percentage, percentageValid);

            txt_Matches.Text = preview.GroupCount == 1
                ? "1 group in this category, " + Math.Round(preview.GroupVolume, 2).ToString() + " m³."
                : preview.GroupCount.ToString() + " groups in this category, " +
                  Math.Round(preview.GroupVolume, 2).ToString() + " m³.";

            txt_VolumeResult.Text = Math.Round(preview.Volume, 3).ToString();
            txt_WeightResult.Text = Math.Round(preview.Mass, 1).ToString();
            txt_ECResult.Text = Math.Round(preview.EC, 3).ToString();

            ShowWarning(preview.GroupCount, material, percentage, percentageValid);
        }

        /// <summary>
        /// Says what would stop these choices from producing anything worth having, while there is
        /// still something the user can do about it.
        /// </summary>
        private void ShowWarning(int groupCount, CarboMaterial material, double percentage, bool percentageValid)
        {
            string message = "";

            if (percentageValid == false)
            {
                message = "The allowance must be a number of 0 or more.";
            }
            else if (percentage == 0)
            {
                message = "An allowance of 0% adds nothing.";
            }
            else if (groupCount == 0)
            {
                message = "No groups use a material in this category, so nothing would be created.";
            }
            else if (material == null)
            {
                message = "Pick the material the connections are made of.";
            }
            else if (material.Density <= 0)
            {
                //The allowance is a volume, and a volume of a material with no density weighs
                //nothing and so carries no carbon. It would be created and add exactly zero.
                message = "\"" + material.Name + "\" has no density, so the allowance would add no weight " +
                          "and no carbon. Pick another material, or give this one a density in the database.";
            }

            txt_Warning.Text = message;
            txt_Warning.Visibility = message == "" ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// The selected material, taken from the project database the generator itself looks in.
        /// A copy: the preview calls CalculateTotals on it, and that reaches into the material's
        /// sub properties, which would otherwise be the database's own.
        /// </summary>
        private CarboMaterial GetSelectedMaterial()
        {
            string name = cbb_Material.SelectedItem as string;

            if (string.IsNullOrEmpty(name) || project.CarboDatabase == null)
                return null;

            CarboMaterial material = project.CarboDatabase.GetExcactMatch(name);

            return material == null ? null : material.DeepClone();
        }

        /// <summary>
        /// Reads the allowance from the box.
        /// </summary>
        /// <returns>False when the box does not hold a percentage of 0 or more</returns>
        private bool TryReadPercentage(out double percentage)
        {
            if (CarboLifeAPI.Utils.TryConvertToDouble(txt_Percentage.Text, out percentage) == false)
                return false;

            return percentage >= 0;
        }

        private void Cbb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshInterface();
        }

        private void Txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshInterface();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            string category = cbb_Category.SelectedItem as string;
            CarboMaterial material = GetSelectedMaterial();

            if (string.IsNullOrEmpty(category) || material == null)
            {
                MessageBox.Show(
                    "Pick both a material category and a connection material.",
                    this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double percentage;
            if (TryReadPercentage(out percentage) == false)
            {
                MessageBox.Show(
                    "The allowance must be a number of 0 or more.",
                    this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Everything below this point builds nothing, which is exactly the silent outcome this
            //dialog exists to prevent, so it is said out loud before the settings are accepted.
            if (percentage == 0)
            {
                if (MessageBox.Show(
                        "An allowance of 0% adds nothing." + Environment.NewLine + Environment.NewLine +
                        "Continue anyway?",
                        this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
            }

            if (GetPreview(category, material, percentage, true).GroupCount == 0)
            {
                MessageBox.Show(
                    "No groups use a material in category \"" + category + "\", so nothing would be created.",
                    this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedCategory = category;
            SelectedMaterialName = material.Name;
            SelectedPercentage = percentage;
            SaveAsDefault = chk_SaveAsDefault.IsChecked == true;

            isAccepted = true;
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }
    }
}
