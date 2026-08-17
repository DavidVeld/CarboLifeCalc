using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using CarboLifeAPI.Data.Superseded;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using Path = System.IO.Path;
using WpfButton = System.Windows.Controls.Button;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CarboGroupingSettingsDialog : Window
    {
        public MessageBoxResult dialogOk;
        //public List<CarboLevel> carboLevelList;
        public CarboGroupSettings importSettings;

        private IDictionary<string, string> templateCollection;

        public string selectedTemplateFile;

        public string projectPath;

        /// <summary>
        /// The material database of the template currently selected in cbb_Template.
        /// Drives the reinforcement material and category lists.
        /// </summary>
        private CarboDatabase activeTemplate;

        /// <summary>
        /// False while Window_Loaded builds the UI, so template changes made by the
        /// initial population don't trigger a re-match or a warning.
        /// </summary>
        private bool uiReady;

        /// <summary>
        /// Below this score a match is too weak to offer, see FindClosestMatch.
        /// Anything under roughly this level shares no recognisable word with the original.
        /// </summary>
        private const int minimumMatchScore = 60;

        /// <summary>
        /// One allowance block from the right hand column: the controls that belong to its tick box,
        /// and the material and category it keeps aiming for when the user picks another template.
        /// A template without a match leaves the box empty, so the target is remembered separately.
        /// </summary>
        private class AllowanceBlock
        {
            public string Name;
            public string MatchName;
            public WpfCheckBox Enabled;
            public WpfComboBox MaterialBox;
            public WpfComboBox CategoryBox;
            public TextBlock WarningBox;

            /// <summary>Null for reinforcement, that one takes its quantities from the mapping table.</summary>
            public WpfTextBox PercentageBox;

            /// <summary>Only enabled alongside the tick box, reinforcement has its own button.</summary>
            public WpfButton ExtraButton;

            public string WantedMaterial;
            public string WantedCategory;
        }

        private List<AllowanceBlock> allowanceBlocks;

        public CarboGroupingSettingsDialog(CarboGroupSettings settings)
        {
            importSettings = settings;
            settings.ReloadRCMap();


            dialogOk = MessageBoxResult.Cancel;
           // carboLevelList = levelList;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dialogOk = MessageBoxResult.Cancel;

            FitToScreen();

            // Get DefaultTemplate:
            templateCollection = PathUtils.getTemplateFiles();
            if (templateCollection != null)
            {
                foreach (var template in templateCollection)
                {
                    cbb_Template.Items.Add(template.Key);
                }

                // Select the template saved in settings, fall back to index 0
                CarboSettings settings = new CarboSettings().Load();
                string savedFileName = Path.GetFileName(settings.templatePath);

                int savedIndex = cbb_Template.Items.IndexOf(savedFileName);
                cbb_Template.SelectedIndex = savedIndex >= 0 ? savedIndex : 0;
            }

            BuildAllowanceBlocks();

            loadSettingsToUI();

            //Fill the allowance lists from the selected template and select the stored values.
            LoadActiveTemplate();
            LoadAllowanceListsToUI();

            uiReady = true;
        }

        /// <summary>
        /// The three allowance blocks make this a tall dialog. On a screen that cannot hold it the
        /// window is shrunk to fit and the columns scroll, rather than the footer dropping off screen.
        /// </summary>
        private void FitToScreen()
        {
            double available = System.Windows.SystemParameters.WorkArea.Height - 40;

            if (available > 0 && available < this.Height)
            {
                this.MinHeight = available;
                this.Height = available;
                this.Top = System.Windows.SystemParameters.WorkArea.Top + 10;
            }
        }

        /// <summary>
        /// Ties each allowance tick box to the controls that belong to it, so the reinforcement,
        /// steel and timber blocks can all be filled, checked and enabled by the same code.
        /// </summary>
        private void BuildAllowanceBlocks()
        {
            allowanceBlocks = new List<AllowanceBlock>();

            allowanceBlocks.Add(new AllowanceBlock
            {
                Name = "Reinforcement mapping",
                MatchName = "reinforcement",
                Enabled = chk_MapReinforcement,
                MaterialBox = cbb_RCImportMaterial,
                CategoryBox = cbb_RCMaterialCategory,
                WarningBox = txt_RCWarning,
                ExtraButton = btn_ReinforcementImport,
                WantedMaterial = importSettings.RCMaterialName,
                WantedCategory = importSettings.RCMaterialCategory
            });

            allowanceBlocks.Add(new AllowanceBlock
            {
                Name = "Steel connection allowances",
                MatchName = "steel connection",
                Enabled = chk_AddSteelConnections,
                MaterialBox = cbb_SteelConnectionMaterial,
                CategoryBox = cbb_SteelMaterialCategory,
                WarningBox = txt_SteelWarning,
                PercentageBox = txt_SteelConnectionPercentage,
                WantedMaterial = importSettings.SteelConnectionMaterialName,
                WantedCategory = importSettings.SteelMaterialCategory
            });

            allowanceBlocks.Add(new AllowanceBlock
            {
                Name = "Timber connection allowances",
                MatchName = "timber connection",
                Enabled = chk_AddTimberConnections,
                MaterialBox = cbb_TimberConnectionMaterial,
                CategoryBox = cbb_TimberMaterialCategory,
                WarningBox = txt_TimberWarning,
                PercentageBox = txt_TimberConnectionPercentage,
                WantedMaterial = importSettings.TimberConnectionMaterialName,
                WantedCategory = importSettings.TimberMaterialCategory
            });
        }

        /// <summary>
        /// loads the active importsettings to the UI
        /// </summary>
        private void loadSettingsToUI()
        {
            //Category Settings
            cbb_MainGroup.Items.Clear();
            cbb_MainGroup.Items.Add("(Revit) Category");
            cbb_MainGroup.Items.Add("Type Parameter");
            cbb_MainGroup.Items.Add("Instance Parameter");

            cbb_MainGroup.SelectedItem = importSettings.CategoryName;
            txt_CategoryparamName.Text = importSettings.CategoryParamName;

            //CheckCaregoryParam();

            //Allowances
            chk_MapReinforcement.IsChecked = importSettings.mapReinforcement;

            chk_AddSteelConnections.IsChecked = importSettings.mapSteelConnections;
            txt_SteelConnectionPercentage.Text = importSettings.SteelConnectionPercentage.ToString();

            chk_AddTimberConnections.IsChecked = importSettings.mapTimberConnections;
            txt_TimberConnectionPercentage.Text = importSettings.TimberConnectionPercentage.ToString();

            UpdateAllowanceEnabledState();

            //Substructure
            cbb_SubstructureImportType.Items.Clear();
            cbb_SubstructureImportType.Items.Add("Parameter (Instance Boolean)");
            cbb_SubstructureImportType.Items.Add("Workset Name Contains");

            chk_ImportSubstructure.IsChecked = importSettings.IncludeSubStructure;
            cbb_SubstructureImportType.SelectedItem = importSettings.SubStructureParamType;
            txt_SubstructureParamName.Text = importSettings.SubStructureParamName;


            //Grade
            cbb_GradeImportType.Items.Clear();
            cbb_GradeImportType.Items.Add("Type Parameter");
            cbb_GradeImportType.Items.Add("Instance Parameter");
            //cbb_GradeImportType.Items.Add("Material Parameter");

            chk_MaterialGrade.IsChecked = importSettings.IncludeGradeParameter;
            cbb_GradeImportType.SelectedItem = importSettings.GradeParameterType.ToString();
            txt_GradeImportValue.Text = importSettings.GradeParameterName.ToString();

            //CorrectionList
            cbb_CorrectionImportType.Items.Clear();
            cbb_CorrectionImportType.Items.Add("Type Parameter");
            cbb_CorrectionImportType.Items.Add("Instance Parameter");

            chk_doCorrection.IsChecked = importSettings.IncludeCorrectionParameter;
            cbb_CorrectionImportType.SelectedItem = importSettings.CorrectionParameterType.ToString();
            txt_CorrectionImportValue.Text = importSettings.CorrectionParameterName.ToString();

            //Existing            
            chk_ImportExisting.IsChecked = importSettings.IncludeExisting;
            txt_ExistingPhaseName.Text = importSettings.ExistingPhaseName;

            //Demolished
            chk_ImportDemolished.IsChecked = importSettings.IncludeDemo;
            //chk_CombineExistingAndDemo.IsChecked = importSettings.CombineExistingAndDemo;

            //Additional Parameter
            cbb_ExtraImportType.Items.Clear();
            cbb_ExtraImportType.Items.Add("Type Parameter");
            cbb_ExtraImportType.Items.Add("Instance Parameter");

            chk_AdditionalImport.IsChecked = importSettings.IncludeAdditionalParameter;
            cbb_ExtraImportType.SelectedItem = importSettings.AdditionalParameterElementType;
            txt_ExtraImportValue.Text = importSettings.AdditionalParameter;

            chk_UseMappedMaterialData.IsChecked = importSettings.UseImportedMap;

            txt_UncertFact.Text = (importSettings.UncertaintyFactor * 100).ToString();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            dialogOk = MessageBoxResult.Cancel;
            this.Close();
        }

        private void Btn_ImportClose_Click(object sender, RoutedEventArgs e)
        {
            if (AllowanceSettingsAreValid() == false)
                return;

            string result;
            templateCollection.TryGetValue(cbb_Template.Text, out result);

            if (File.Exists(result))
            {
                selectedTemplateFile = result;
            }
            else
            {
               System.Windows.MessageBox.Show("The selected template could not be found");
            }

            dialogOk = MessageBoxResult.Yes;
            SaveSettings();
            this.Close();
        }

        private void Btn_OkClose_Click(object sender, RoutedEventArgs e)
        {
            dialogOk = MessageBoxResult.OK;
            SaveSettings();
            this.Close();
        }

        private void SaveSettings()
        {

            //Save the latest settings in the default;
            CarboSettings settings = new CarboSettings();
            settings = settings.Load();

            //Write default values as standard
            settings.defaultCarboGroupSettings.CategoryName = cbb_MainGroup.Text;
            settings.defaultCarboGroupSettings.CategoryParamName = txt_CategoryparamName.Text;

            settings.defaultCarboGroupSettings.IncludeSubStructure = chk_ImportSubstructure.IsChecked.Value;
            settings.defaultCarboGroupSettings.SubStructureParamName = txt_SubstructureParamName.Text;
            settings.defaultCarboGroupSettings.SubStructureParamType = cbb_SubstructureImportType.Text;

            settings.defaultCarboGroupSettings.IncludeDemo = chk_ImportDemolished.IsChecked.Value;
            settings.defaultCarboGroupSettings.IncludeExisting = chk_ImportExisting.IsChecked.Value;
            //settings.defaultCarboGroupSettings.CombineExistingAndDemo = chk_CombineExistingAndDemo.IsChecked.Value;

            //additional value
            settings.defaultCarboGroupSettings.IncludeAdditionalParameter = chk_AdditionalImport.IsChecked.Value;
            settings.defaultCarboGroupSettings.AdditionalParameter = txt_ExtraImportValue.Text;
            settings.defaultCarboGroupSettings.AdditionalParameterElementType = cbb_ExtraImportType.Text;

            //Grade
            settings.defaultCarboGroupSettings.IncludeGradeParameter = chk_MaterialGrade.IsChecked.Value;
            settings.defaultCarboGroupSettings.GradeParameterName = txt_GradeImportValue.Text;
            settings.defaultCarboGroupSettings.GradeParameterType = cbb_GradeImportType.Text;

            //CorrectionList
            settings.defaultCarboGroupSettings.IncludeCorrectionParameter = chk_doCorrection.IsChecked.Value;
            settings.defaultCarboGroupSettings.CorrectionParameterType = cbb_CorrectionImportType.Text;
            settings.defaultCarboGroupSettings.CorrectionParameterName = txt_CorrectionImportValue.Text;

            //RC, materials and density map
            settings.defaultCarboGroupSettings.mapReinforcement = chk_MapReinforcement.IsChecked.Value;

            settings.defaultCarboGroupSettings.RCParameterName = importSettings.RCParameterName;
            settings.defaultCarboGroupSettings.RCParameterType = importSettings.RCParameterType;
            settings.defaultCarboGroupSettings.rcQuantityMap = importSettings.rcQuantityMap;

            //The allowance materials and categories are edited here, they belong to the selected template.
            settings.defaultCarboGroupSettings.RCMaterialName = cbb_RCImportMaterial.Text;
            settings.defaultCarboGroupSettings.RCMaterialCategory = cbb_RCMaterialCategory.Text;

            //Steel connections
            settings.defaultCarboGroupSettings.mapSteelConnections = chk_AddSteelConnections.IsChecked.Value;
            settings.defaultCarboGroupSettings.SteelConnectionMaterialName = cbb_SteelConnectionMaterial.Text;
            settings.defaultCarboGroupSettings.SteelMaterialCategory = cbb_SteelMaterialCategory.Text;

            double steelPercentage;
            if (TryReadPercentage(txt_SteelConnectionPercentage, out steelPercentage) == false)
            {
                steelPercentage = importSettings.SteelConnectionPercentage;
            }
            settings.defaultCarboGroupSettings.SteelConnectionPercentage = steelPercentage;

            //Timber connections
            settings.defaultCarboGroupSettings.mapTimberConnections = chk_AddTimberConnections.IsChecked.Value;
            settings.defaultCarboGroupSettings.TimberConnectionMaterialName = cbb_TimberConnectionMaterial.Text;
            settings.defaultCarboGroupSettings.TimberMaterialCategory = cbb_TimberMaterialCategory.Text;

            double timberPercentage;
            if (TryReadPercentage(txt_TimberConnectionPercentage, out timberPercentage) == false)
            {
                timberPercentage = importSettings.TimberConnectionPercentage;
            }
            settings.defaultCarboGroupSettings.TimberConnectionPercentage = timberPercentage;

            settings.defaultCarboGroupSettings.UseImportedMap = chk_UseMappedMaterialData.IsChecked.Value;

            double uncertaintyPercent;
            if (!double.TryParse(txt_UncertFact.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out uncertaintyPercent))
            {
                System.Windows.MessageBox.Show("The uncertainty factor must be a valid number. Keeping the previous value.");
                uncertaintyPercent = importSettings.UncertaintyFactor * 100;
            }
            settings.defaultCarboGroupSettings.UncertaintyFactor = uncertaintyPercent / 100.0;

            string fullTemplatePath = PathUtils.getTemplateFilePath(cbb_Template.Text);
            settings.templatePath = fullTemplatePath;

            //Save as default for next time/project;
            settings.Save();

            importSettings = settings.defaultCarboGroupSettings;
        }

        private void cbb_MainGroup_DropDownClosed(object sender, EventArgs e)
        {
            CheckCaregoryParam();
        }

        private void CheckCaregoryParam()
        {
            if (cbb_MainGroup.Text == "(Revit) Category")
            {
                txt_CategoryparamName.Text = "";
                txt_CategoryparamName.IsEnabled = false;
            }
            else
                txt_CategoryparamName.IsEnabled = true;
        }

        private void btn_ProjectPath_Click(object sender, RoutedEventArgs e)
        {
            string fileToOpen = Utils.OpenCarboProject();

            if (fileToOpen != "")
            {
                this.projectPath = fileToOpen;
                txt_ProjectPath.Text = fileToOpen;
            }
        }

        private void btn_ReinforcementImport_Click(object sender, RoutedEventArgs e)
        {
            MaterialConcreteMapper rcMapper = new MaterialConcreteMapper(importSettings);
            rcMapper.ShowDialog();
            if(rcMapper.isAccepted == true)
            {
                importSettings.RCParameterName = rcMapper.categoryName;
                importSettings.RCParameterType = rcMapper.categoryType;

                importSettings.rcQuantityMap = rcMapper.rcMap;
            }


        }

        /// <summary>
        /// Reads the material database of the template currently selected in cbb_Template.
        /// </summary>
        private void LoadActiveTemplate()
        {
            //SelectedItem rather than Text, Text still holds the old value while SelectionChanged runs.
            string templateName = cbb_Template.SelectedItem as string;
            if (string.IsNullOrEmpty(templateName))
                templateName = cbb_Template.Text;

            activeTemplate = CarboDatabase.LoadTemplate(PathUtils.getTemplateFilePath(templateName));
        }

        /// <summary>
        /// Fills the material and category lists of every allowance block from the active template
        /// and selects the closest match to the values each block is aiming for.
        /// </summary>
        private void LoadAllowanceListsToUI()
        {
            List<string> materialNames = new List<string>();
            List<string> categoryNames = new List<string>();

            if (activeTemplate != null)
            {
                foreach (CarboMaterial cm in activeTemplate.CarboMaterialList)
                    materialNames.Add(cm.Name);

                categoryNames = activeTemplate.getCategoryList();
            }

            foreach (AllowanceBlock block in allowanceBlocks)
                FillAllowanceBlock(block, materialNames, categoryNames);
        }

        /// <summary>
        /// Fills one allowance block's material and category list, selects the closest match to what
        /// the block is aiming for and reports anything that could not be matched exactly.
        /// </summary>
        /// <param name="block">The block to fill</param>
        /// <param name="materialNames">All material names in the active template</param>
        /// <param name="categoryNames">All category names in the active template</param>
        private void FillAllowanceBlock(AllowanceBlock block, List<string> materialNames, List<string> categoryNames)
        {
            block.MaterialBox.Items.Clear();
            foreach (string name in materialNames)
                block.MaterialBox.Items.Add(name);

            block.CategoryBox.Items.Clear();
            foreach (string categoryName in categoryNames)
                block.CategoryBox.Items.Add(categoryName);

            bool materialExact;
            bool categoryExact;
            string material = FindClosestMatch(materialNames, block.WantedMaterial, out materialExact);
            string category = FindClosestMatch(categoryNames, block.WantedCategory, out categoryExact);

            block.MaterialBox.SelectedItem = material;
            block.CategoryBox.SelectedItem = category;

            ShowAllowanceWarning(block.WarningBox, block.MatchName,
                                 block.WantedMaterial, material, materialExact,
                                 block.WantedCategory, category, categoryExact);
        }

        /// <summary>
        /// Tells the user which values of an allowance block had to be re-matched against the template.
        /// The visible text names the fields only, so its height does not depend on how long the
        /// template's material names happen to be. The full detail sits in the tooltip.
        /// </summary>
        private void ShowAllowanceWarning(TextBlock warningBox, string blockName,
                                          string wantedMaterial, string material, bool materialExact,
                                          string wantedCategory, string category, bool categoryExact)
        {
            if (materialExact == true && categoryExact == true)
            {
                warningBox.Text = "";
                warningBox.ToolTip = null;
                warningBox.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            List<string> missing = new List<string>();
            List<string> rematched = new List<string>();
            string detail = "The " + blockName + " settings do not exist in this template:";

            if (materialExact == false)
            {
                if (string.IsNullOrEmpty(material))
                {
                    missing.Add("material");
                    detail += Environment.NewLine + "• Material \"" + wantedMaterial + "\" → nothing similar found";
                }
                else
                {
                    rematched.Add("material");
                    detail += Environment.NewLine + "• Material \"" + wantedMaterial + "\" → \"" + material + "\"";
                }
            }

            if (categoryExact == false)
            {
                if (string.IsNullOrEmpty(category))
                {
                    missing.Add("category");
                    detail += Environment.NewLine + "• Category \"" + wantedCategory + "\" → nothing similar found";
                }
                else
                {
                    rematched.Add("category");
                    detail += Environment.NewLine + "• Category \"" + wantedCategory + "\" → \"" + category + "\"";
                }
            }

            string message = "";

            if (missing.Count > 0)
                message = "Not in this template, please pick a " + string.Join(" and a ", missing.ToArray()) + ".";

            if (rematched.Count > 0)
            {
                if (message != "")
                    message += Environment.NewLine;

                message += "Re-matched the " + string.Join(" and the ", rematched.ToArray()) + ", please check.";
            }

            warningBox.Text = message;
            warningBox.ToolTip = detail;
            warningBox.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Finds the entry in the list that resembles the wanted value the most.
        /// A guess below <see cref="minimumMatchScore"/> is dropped, an obviously empty field asks
        /// the user to pick where a wrong looking match would quietly be accepted.
        /// </summary>
        /// <param name="candidates">The values available in the active template</param>
        /// <param name="wanted">The value to look for</param>
        /// <param name="isExact">True when the wanted value is present in the list, or when nothing was requested</param>
        /// <returns>The best matching candidate, null when nothing resembles the wanted value</returns>
        private static string FindClosestMatch(IList<string> candidates, string wanted, out bool isExact)
        {
            isExact = false;

            //Nothing was set before, so nothing got lost either.
            if (string.IsNullOrWhiteSpace(wanted))
            {
                isExact = true;
                return null;
            }

            if (candidates == null || candidates.Count == 0)
                return null;

            string wantedLower = wanted.Trim().ToLower();
            string[] wantedWords = wantedLower.Split(new char[] { ' ', ',', '_', '-', '/' }, StringSplitOptions.RemoveEmptyEntries);

            string best = null;
            int highscore = int.MinValue;

            foreach (string candidate in candidates)
            {
                if (string.IsNullOrEmpty(candidate))
                    continue;

                string candidateLower = candidate.Trim().ToLower();

                if (candidateLower == wantedLower)
                {
                    isExact = true;
                    return candidate;
                }

                //Similarity as a percentage, so a long name is not punished for being long.
                int dist = Utils.CalcLevenshteinDistance(candidateLower, wantedLower);
                int longest = Math.Max(candidateLower.Length, wantedLower.Length);
                int score = longest == 0 ? 0 : (100 * (longest - dist)) / longest;

                //One name sitting inside the other is a strong signal.
                if (candidateLower.Contains(wantedLower) || wantedLower.Contains(candidateLower))
                    score += 100;

                //Every word of the old name that survives in the new one adds up.
                foreach (string word in wantedWords)
                {
                    if (word.Length > 2 && candidateLower.Contains(word))
                        score += 20;
                }

                if (score > highscore)
                {
                    highscore = score;
                    best = candidate;
                }
            }

            return highscore >= minimumMatchScore ? best : null;
        }

        /// <summary>
        /// An allowance needs a material and a category that exist in the selected template, without
        /// them the import would look for something that isn't there.
        /// </summary>
        private bool AllowanceSettingsAreValid()
        {
            foreach (AllowanceBlock block in allowanceBlocks)
            {
                if (block.Enabled.IsChecked != true)
                    continue;

                if (string.IsNullOrEmpty(block.MaterialBox.Text) || string.IsNullOrEmpty(block.CategoryBox.Text))
                {
                    System.Windows.MessageBox.Show(
                        block.Name + " is switched on, but the material or the category is not set for template \"" +
                        cbb_Template.Text + "\"." + Environment.NewLine + Environment.NewLine +
                        "Pick both from the lists, or switch the allowance off.",
                        block.Name + " incomplete", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                double percentage;
                if (block.PercentageBox != null && TryReadPercentage(block.PercentageBox, out percentage) == false)
                {
                    System.Windows.MessageBox.Show(
                        "The allowance of " + block.Name.ToLower() + " must be a number of 0 or more.",
                        block.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Reads an allowance percentage from the UI.
        /// </summary>
        /// <returns>False when the box does not hold a percentage of 0 or more</returns>
        private static bool TryReadPercentage(WpfTextBox box, out double percentage)
        {
            if (double.TryParse(box.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out percentage) == false)
                return false;

            return percentage >= 0;
        }

        private void UpdateAllowanceEnabledState()
        {
            foreach (AllowanceBlock block in allowanceBlocks)
            {
                bool enabled = block.Enabled.IsChecked == true;

                block.MaterialBox.IsEnabled = enabled;
                block.CategoryBox.IsEnabled = enabled;

                if (block.PercentageBox != null)
                    block.PercentageBox.IsEnabled = enabled;

                if (block.ExtraButton != null)
                    block.ExtraButton.IsEnabled = enabled;
            }
        }

        private void chk_MapReinforcement_Toggled(object sender, RoutedEventArgs e)
        {
            UpdateAllowanceEnabledState();
        }

        private void chk_AddSteelConnections_Toggled(object sender, RoutedEventArgs e)
        {
            UpdateAllowanceEnabledState();
        }

        private void chk_AddTimberConnections_Toggled(object sender, RoutedEventArgs e)
        {
            UpdateAllowanceEnabledState();
        }

        private void cbb_Template_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (uiReady == false)
                return;

            LoadActiveTemplate();

            //Keep what the user was working with, the new template may name things differently.
            //An empty box means the previous template had no match, so keep aiming for the original.
            foreach (AllowanceBlock block in allowanceBlocks)
            {
                block.WantedMaterial = Preserve(block.MaterialBox, block.WantedMaterial);
                block.WantedCategory = Preserve(block.CategoryBox, block.WantedCategory);
            }

            LoadAllowanceListsToUI();
        }

        private static string Preserve(WpfComboBox box, string fallback)
        {
            return string.IsNullOrEmpty(box.Text) ? fallback : box.Text;
        }

        private void btn_ExportSettings_Click(object sender, RoutedEventArgs e)
        {
            //save the settings to current (this is a requirement)
            SaveSettings();

            //importSettings.SerializeXML();
            string path = PathUtils.getSettingsFilePath();

            //Copy The file to a custom Locaiton
            importSettings.ExportSettingsFile(path);

        }

        private void btn_ImportSettings_Click(object sender, RoutedEventArgs e)
        {
            //save the settings to current (this is a requirement)
            SaveSettings();
            importSettings.SerializeXML();

            string pathNewFile = importSettings.ImportSettingsFile();
            string path = PathUtils.getSettingsFilePath();

            PathUtils.OverrideSettingsFile(pathNewFile, path); 

            System.Windows.MessageBox.Show("Settings imported. Restart CarboLifeCalculator to load the settings.");

            this.Close();
        }
    }
}
