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
        /// A material database picked with btn_TemplatePath. Empty means the template list is
        /// built where the settings point; set, the list is built from this file's folder, so a
        /// database chosen from somewhere else - a team share, a project folder - and the ones
        /// beside it appear in the list before anything has been saved.
        /// </summary>
        private string templateSourcePath;

        /// <summary>
        /// The mapping file this import will read the previous material matches from, and write
        /// the new ones back to. Resolved the same way the import resolves it, and repointed by
        /// btn_MappingPath.
        /// </summary>
        private string mappingFilePath;

        /// <summary>
        /// The mapping file the settings ask for, before resolution. Kept to spot the case where
        /// the file that will actually be used is not the one that was configured.
        /// </summary>
        private string configuredMappingPath;

        /// <summary>
        /// True once the user has pointed at another mapping file here. Only then is the path
        /// written back to the settings: the box shows the resolved path, which falls back to the
        /// local default while a shared folder is briefly unreachable, and saving that fallback
        /// would detach the user from the shared file for good.
        /// </summary>
        private bool mappingFilePathChanged;

        /// <summary>Colours of the two file status lines, matching the WarningText style.</summary>
        private static readonly System.Windows.Media.Brush statusProblemBrush =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC0, 0x50, 0x00));

        private static readonly System.Windows.Media.Brush statusOkBrush = System.Windows.Media.Brushes.Gray;

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

        /// <summary>
        /// One settings field that names something in the Revit model: a parameter, a workset or
        /// a phase. Every one of them is looked up by the import and silently skipped when the
        /// model has no such thing, so each is checked here and marked when it cannot be found.
        /// </summary>
        private class NameField
        {
            /// <summary>What to call it in the warning at the top of the dialog.</summary>
            public string Label;

            public WpfComboBox Box;

            /// <summary>False while the setting this field belongs to is switched off.</summary>
            public Func<bool> IsActive;

            /// <summary>Which of the model's lists decides whether this name exists.</summary>
            public Func<CarboNameKind> Kind;

            /// <summary>
            /// The Type/Instance box beside it, where leaving it unset stops the import reading
            /// the parameter at all. Null where there is none, or where an unset one is harmless.
            /// </summary>
            public WpfComboBox RequiredTypeBox;

            /// <summary>
            /// Whatever the XAML gave the box, kept so describing a problem in the tooltip can be
            /// undone without losing the description of the field itself.
            /// </summary>
            public object DefaultToolTip;

            public bool DefaultToolTipRead;
        }

        private List<NameField> nameFields;

        /// <summary>The outline of a field naming something this model does not have.</summary>
        private static readonly System.Windows.Media.Brush missingNameBorderBrush =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC0, 0x50, 0x00));

        /// <summary>The outline every other field keeps, matching flatTextBox in MyStyles.</summary>
        private static readonly System.Windows.Media.Brush normalNameBorderBrush =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x97, 0x97, 0x97));

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
            LoadTemplateList(null);

            LoadMappingFilePath();

            BuildAllowanceBlocks();

            loadSettingsToUI();

            //Fill the allowance lists from the selected template and select the stored values.
            LoadActiveTemplate();
            LoadAllowanceListsToUI();

            ShowFilePaths();

            //After loadSettingsToUI: the pickers it attaches have to exist before they can be
            //checked, and the Type/Instance boxes have to hold their stored values before it is
            //known which of the model's lists each name should be found in.
            BuildNameFields();
            RefreshMissingNames();

            uiReady = true;
        }

        /// <summary>
        /// Fills the template list from the materials folder.
        /// </summary>
        /// <param name="preferred">
        /// The template to keep selected when it is still on the list. Null selects the stored default.
        /// </param>
        private void LoadTemplateList(string preferred)
        {
            templateCollection = string.IsNullOrEmpty(templateSourcePath)
                ? PathUtils.getTemplateFiles()
                : PathUtils.GetTemplateFiles(templateSourcePath);

            cbb_Template.Items.Clear();

            if (templateCollection == null)
                return;

            foreach (var template in templateCollection)
                cbb_Template.Items.Add(template.Key);

            if (cbb_Template.Items.Count == 0)
                return;

            //What the user had picked stays picked, it is only gone if the file itself is.
            if (string.IsNullOrEmpty(preferred) == false && cbb_Template.Items.Contains(preferred))
            {
                cbb_Template.SelectedItem = preferred;
                return;
            }

            //The template saved in the settings, or the user's own materials. Falling back to
            //index 0 meant whichever file the materials folder listed first, which is a
            //reference database like Okobaudat rather than the materials the user maintains.
            string selected = PathUtils.GetDefaultTemplateSelection(templateCollection.Keys);

            if (selected != null)
                cbb_Template.SelectedItem = selected;
            else
                cbb_Template.SelectedIndex = 0;
        }

        /// <summary>
        /// Reads the mapping file location the same way CarboMapFile reads it, so the path shown
        /// is the file the import will really use rather than the one the settings ask for.
        /// </summary>
        private void LoadMappingFilePath()
        {
            try
            {
                CarboSettings settings = new CarboSettings().Load();
                configuredMappingPath = settings.mappingPath;
            }
            catch
            {
                //An unreadable settings file leaves nothing to compare against, the resolved
                //path below is still worth showing.
                configuredMappingPath = "";
            }

            mappingFilePath = PathUtils.GetMappingFilePath();
            mappingFilePathChanged = false;
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

            CarboParameterPicker.Attach(cbb_CategoryparamName, importSettings.CategoryParamName,
                                        CategoryNameKind(), OnNameFieldChanged);

            //Only the enabled state, not CheckCaregoryParam: that one also empties the box, which
            //is right when the user switches to "(Revit) Category" and wrong on the way in - it
            //would throw away a parameter name that is still in the settings file.
            ApplyCategoryParamEnabled();

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

            CarboParameterPicker.Attach(cbb_SubstructureParamName, importSettings.SubStructureParamName,
                                        SubstructureNameKind(), OnNameFieldChanged);

            ShowSubstructureHint();

            //Grade
            cbb_GradeImportType.Items.Clear();
            cbb_GradeImportType.Items.Add("Type Parameter");
            cbb_GradeImportType.Items.Add("Instance Parameter");
            //cbb_GradeImportType.Items.Add("Material Parameter");

            chk_MaterialGrade.IsChecked = importSettings.IncludeGradeParameter;
            cbb_GradeImportType.SelectedItem = importSettings.GradeParameterType;

            CarboParameterPicker.Attach(cbb_GradeImportValue, importSettings.GradeParameterName,
                                        ParameterKindOf(cbb_GradeImportType), OnNameFieldChanged);

            //CorrectionList
            cbb_CorrectionImportType.Items.Clear();
            cbb_CorrectionImportType.Items.Add("Type Parameter");
            cbb_CorrectionImportType.Items.Add("Instance Parameter");

            chk_doCorrection.IsChecked = importSettings.IncludeCorrectionParameter;
            cbb_CorrectionImportType.SelectedItem = importSettings.CorrectionParameterType;

            CarboParameterPicker.Attach(cbb_CorrectionImportValue, importSettings.CorrectionParameterName,
                                        ParameterKindOf(cbb_CorrectionImportType), OnNameFieldChanged);

            //Existing
            chk_ImportExisting.IsChecked = importSettings.IncludeExisting;

            CarboParameterPicker.Attach(cbb_ExistingPhaseName, importSettings.ExistingPhaseName,
                                        CarboNameKind.Phase, OnNameFieldChanged);

            //Demolished
            chk_ImportDemolished.IsChecked = importSettings.IncludeDemo;
            //chk_CombineExistingAndDemo.IsChecked = importSettings.CombineExistingAndDemo;

            //Additional Parameter
            cbb_ExtraImportType.Items.Clear();
            cbb_ExtraImportType.Items.Add("Type Parameter");
            cbb_ExtraImportType.Items.Add("Instance Parameter");

            chk_AdditionalImport.IsChecked = importSettings.IncludeAdditionalParameter;
            cbb_ExtraImportType.SelectedItem = importSettings.AdditionalParameterElementType;

            CarboParameterPicker.Attach(cbb_ExtraImportValue, importSettings.AdditionalParameter,
                                        ParameterKindOf(cbb_ExtraImportType), OnNameFieldChanged);

            chk_UseMappedMaterialData.IsChecked = importSettings.UseImportedMap;

            txt_UncertFact.Text = (importSettings.UncertaintyFactor * 100).ToString();

            //GIA. Settings written before these fields existed deserialise them as null, which
            //would blank the boxes and quietly switch the lookup off, so fall back to the names
            //the parameter check tool adds.
            CarboParameterPicker.Attach(cbb_GIAParamName,
                                        importSettings.GIAParameterName == null
                                            ? CarboGroupSettings.DefaultGIAParameterName
                                            : importSettings.GIAParameterName,
                                        CarboNameKind.ProjectInformation, OnNameFieldChanged);

            CarboParameterPicker.Attach(cbb_GIANewParamName,
                                        importSettings.GIANewParameterName == null
                                            ? CarboGroupSettings.DefaultGIANewParameterName
                                            : importSettings.GIANewParameterName,
                                        CarboNameKind.ProjectInformation, OnNameFieldChanged);

            txt_GIAMethod.Text = string.IsNullOrEmpty(importSettings.GIADeterminationMethod)
                ? CarboGroupSettings.GIAFromRevitEstimate
                : importSettings.GIADeterminationMethod;
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

            //An import with no material database behind it matches every material to a blank row
            //and prices the whole model at zero. This used to warn and then import anyway, with
            //selectedTemplateFile left empty. The dialog now stays open so the user can pick a
            //template that exists.
            if (TryResolveSelectedTemplate() == false)
                return;

            dialogOk = MessageBoxResult.Yes;
            SaveSettings();
            this.Close();
        }

        /// <summary>
        /// Resolves the template chosen in the combo box to a file on disk.
        /// Reports what is wrong and returns false when it cannot, leaving the dialog open.
        /// </summary>
        private bool TryResolveSelectedTemplate()
        {
            string chosen = cbb_Template.Text == null ? "" : cbb_Template.Text.Trim();

            if (chosen.Length == 0)
            {
                System.Windows.MessageBox.Show(
                    "No material template is selected." + Environment.NewLine + Environment.NewLine +
                    "Pick one from the Template list before importing.",
                    "Template required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            string result = ResolveSelectedTemplatePath();

            if (string.IsNullOrEmpty(result) || File.Exists(result) == false)
            {
                System.Windows.MessageBox.Show(
                    "The selected template could not be found:" + Environment.NewLine + Environment.NewLine +
                    chosen + Environment.NewLine + Environment.NewLine +
                    "Nothing has been imported. Pick a template that exists, or check the materials folder:" +
                    Environment.NewLine + PathUtils.GetMaterialsDir(),
                    "Template not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            //A template that holds no materials is as useless as a missing one, and it fails
            //silently rather than loudly, so it is caught here too.
            try
            {
                //LoadTemplate rather than DeSerializeXML: the lists offer .csv databases and the
                //import reads them, but reading one as XML fails, so a .csv selection was turned
                //away here as "contains no materials".
                CarboDatabase check = CarboDatabase.LoadTemplate(result);

                if (check == null || check.CarboMaterialList == null || check.CarboMaterialList.Count == 0)
                {
                    System.Windows.MessageBox.Show(
                        "The selected template contains no materials:" + Environment.NewLine + Environment.NewLine +
                        result + Environment.NewLine + Environment.NewLine +
                        "Nothing has been imported. Pick another template.",
                        "Template empty", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "The selected template could not be read:" + Environment.NewLine + Environment.NewLine +
                    result + Environment.NewLine + Environment.NewLine + ex.Message +
                    Environment.NewLine + Environment.NewLine + "Nothing has been imported.",
                    "Template unreadable", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            selectedTemplateFile = result;
            return true;
        }

        private void Btn_OkClose_Click(object sender, RoutedEventArgs e)
        {
            //Ok saves the same settings that Ok & Import does, so it validates them the same way.
            //Without this an allowance could be saved switched on with no material behind it and
            //only fail later, during an import started from somewhere else.
            if (AllowanceSettingsAreValid() == false)
                return;

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
            settings.defaultCarboGroupSettings.CategoryParamName = CarboParameterPicker.ValueOf(cbb_CategoryparamName);

            settings.defaultCarboGroupSettings.IncludeSubStructure = chk_ImportSubstructure.IsChecked.Value;
            settings.defaultCarboGroupSettings.SubStructureParamName = CarboParameterPicker.ValueOf(cbb_SubstructureParamName);
            settings.defaultCarboGroupSettings.SubStructureParamType = cbb_SubstructureImportType.Text;

            settings.defaultCarboGroupSettings.IncludeDemo = chk_ImportDemolished.IsChecked.Value;
            settings.defaultCarboGroupSettings.IncludeExisting = chk_ImportExisting.IsChecked.Value;
            settings.defaultCarboGroupSettings.ExistingPhaseName = CarboParameterPicker.ValueOf(cbb_ExistingPhaseName);
            //settings.defaultCarboGroupSettings.CombineExistingAndDemo = chk_CombineExistingAndDemo.IsChecked.Value;

            //additional value
            settings.defaultCarboGroupSettings.IncludeAdditionalParameter = chk_AdditionalImport.IsChecked.Value;
            settings.defaultCarboGroupSettings.AdditionalParameter = CarboParameterPicker.ValueOf(cbb_ExtraImportValue);
            settings.defaultCarboGroupSettings.AdditionalParameterElementType = cbb_ExtraImportType.Text;

            //Grade
            settings.defaultCarboGroupSettings.IncludeGradeParameter = chk_MaterialGrade.IsChecked.Value;
            settings.defaultCarboGroupSettings.GradeParameterName = CarboParameterPicker.ValueOf(cbb_GradeImportValue);
            settings.defaultCarboGroupSettings.GradeParameterType = cbb_GradeImportType.Text;

            //CorrectionList
            settings.defaultCarboGroupSettings.IncludeCorrectionParameter = chk_doCorrection.IsChecked.Value;
            settings.defaultCarboGroupSettings.CorrectionParameterType = cbb_CorrectionImportType.Text;
            settings.defaultCarboGroupSettings.CorrectionParameterName = CarboParameterPicker.ValueOf(cbb_CorrectionImportValue);

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

            //GIA. The names are the user's to choose; the method is written by the import, so it
            //is carried over rather than read back off the read only box.
            settings.defaultCarboGroupSettings.GIAParameterName = CarboParameterPicker.ValueOf(cbb_GIAParamName);
            settings.defaultCarboGroupSettings.GIANewParameterName = CarboParameterPicker.ValueOf(cbb_GIANewParamName);
            settings.defaultCarboGroupSettings.GIADeterminationMethod =
                string.IsNullOrEmpty(importSettings.GIADeterminationMethod)
                    ? CarboGroupSettings.GIAFromRevitEstimate
                    : importSettings.GIADeterminationMethod;

            //As LoadActiveTemplate: the resolved path of the selected database, which may sit
            //outside the materials folder. An empty selection leaves the stored one alone rather
            //than replacing it with a default the user never picked.
            string fullTemplatePath = ResolveSelectedTemplatePath();

            if (string.IsNullOrEmpty(fullTemplatePath) == false)
                settings.templatePath = fullTemplatePath;

            //Only what the user pointed at here, see mappingFilePathChanged: writing back the
            //resolved path would turn a momentarily unreachable shared file into the local default.
            if (mappingFilePathChanged == true && string.IsNullOrEmpty(mappingFilePath) == false)
                settings.mappingPath = mappingFilePath;

            //Save as default for next time/project;
            settings.Save();

            importSettings = settings.defaultCarboGroupSettings;
        }

        private void cbb_MainGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Populating the list during Window_Loaded is not a user choice.
            if (uiReady == false)
                return;

            CheckCaregoryParam();

            //"Type Parameter" and "Instance Parameter" are looked up in different places, so the
            //same name can exist as one and not the other.
            CarboParameterPicker.Repoint(cbb_CategoryparamName, CategoryNameKind());

            RefreshMissingNames();
        }

        private void CheckCaregoryParam()
        {
            if (TypeChoiceOf(cbb_MainGroup) == "(Revit) Category")
            {
                CarboParameterPicker.SetValue(cbb_CategoryparamName, "");
                cbb_CategoryparamName.IsEnabled = false;
            }
            else
                cbb_CategoryparamName.IsEnabled = true;
        }

        /// <summary>
        /// The enabled state CheckCaregoryParam sets, without emptying the box. Used on the way
        /// in, where a stored parameter name is worth keeping even though the category is
        /// currently taken from Revit.
        /// </summary>
        private void ApplyCategoryParamEnabled()
        {
            cbb_CategoryparamName.IsEnabled = TypeChoiceOf(cbb_MainGroup) != "(Revit) Category";
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

        /// <summary>
        /// Re-reads the materials folder and the selected database, and refills everything that
        /// comes out of it. This is what to press after editing a database or dropping a new one
        /// into the materials folder: the lists are built once when the dialog opens, so without
        /// it the import runs against the version that was on disk at that moment.
        /// </summary>
        private void btn_ReloadTemplates_Click(object sender, RoutedEventArgs e)
        {
            //A mapping file the user pointed at here stays pointed at; only a configured one is
            //re-read, its date may have moved or the settings may have been changed elsewhere.
            if (mappingFilePathChanged == false)
                LoadMappingFilePath();

            ReloadTemplateList(cbb_Template.SelectedItem as string);
        }

        /// <summary>
        /// Points this import at a material database anywhere on disk, not only at one already in
        /// the materials folder. The list is rebuilt from the folder the file came from and the
        /// new database is read straight away, so the allowance lists and the paths shown all
        /// belong to it. Saved with the rest of the settings, so Cancel changes nothing.
        /// </summary>
        private void btn_TemplatePath_Click(object sender, RoutedEventArgs e)
        {
            string startIn = "";

            try
            {
                string current = ResolveSelectedTemplatePath();

                if (string.IsNullOrEmpty(current) == false)
                    startIn = Path.GetDirectoryName(current);
            }
            catch
            {
                //A path the framework will not take apart just means the dialog opens elsewhere.
            }

            //csv included: the list offers those too, so the picker has to be able to return one.
            string picked = Utils.OpenCarboMaterialLibrary(startIn, true);

            //Cancelled, or something unusable: keep the database that is already in use.
            if (string.IsNullOrEmpty(picked))
                return;

            templateSourcePath = picked;

            ReloadTemplateList(Path.GetFileName(picked));
        }

        /// <summary>
        /// Rebuilds the template list and everything that comes out of the selected database.
        /// </summary>
        /// <param name="preferred">The template to end up selected, null takes the stored default</param>
        private void ReloadTemplateList(string preferred)
        {
            //Emptying and refilling the list raises SelectionChanged, which is not a user choice.
            bool wasReady = uiReady;
            uiReady = false;

            try
            {
                LoadTemplateList(preferred);
            }
            finally
            {
                uiReady = wasReady;
            }

            ReloadActiveTemplateIntoUI();
        }

        /// <summary>
        /// Points this import at another mapping file. Saved with the rest of the settings, so
        /// cancelling the dialog leaves the configured mapping file alone.
        /// </summary>
        private void btn_MappingPath_Click(object sender, RoutedEventArgs e)
        {
            string startIn = "";

            try
            {
                if (string.IsNullOrEmpty(mappingFilePath) == false)
                    startIn = Path.GetDirectoryName(mappingFilePath);
            }
            catch
            {
                //A path the framework will not take apart just means the dialog opens elsewhere.
            }

            string picked = Utils.OpenCarboMappingLibrary(startIn);

            //Cancelled, or something unusable: keep the file that is already in use.
            if (string.IsNullOrEmpty(picked))
                return;

            mappingFilePath = picked;
            mappingFilePathChanged = true;

            ShowFilePaths();
        }

        /// <summary>
        /// Shows the two files this import depends on: the material database the quantities are
        /// priced with, and the mapping file the previous material matches come from.
        ///
        /// Both are resolved rather than configured directly, and both fall back silently - a
        /// missing template to another database, an unreachable mapping file to the local default.
        /// An import that took a fallback still produced numbers, and nothing on screen said so.
        /// </summary>
        private void ShowFilePaths()
        {
            ShowPath(txt_TemplateFilePath, txt_TemplateFileStatus, ResolveSelectedTemplatePath(),
                     "No material database selected: every material would be priced at zero.");

            ShowPath(txt_MappingFilePath, txt_MappingFileStatus, mappingFilePath,
                     "No mapping file set: nothing can be matched to the materials used before.");

            //A configured mapping file on a share that is offline resolves to the local default,
            //so the import would quietly run against a different set of matches. Added to
            //whatever ShowPath found rather than replacing it: the fallback can be missing too.
            if (mappingFilePathChanged == false && UsingAnotherMappingFile() == true)
            {
                AppendStatus(txt_MappingFileStatus,
                             "Not the configured file: " + configuredMappingPath + " could not be reached.");
            }
        }

        /// <summary>
        /// True when the mapping file in use is not the one the settings ask for.
        /// A settings value that is only a file name is no mismatch, it resolves to itself.
        /// </summary>
        private bool UsingAnotherMappingFile()
        {
            if (string.IsNullOrEmpty(configuredMappingPath) || string.IsNullOrEmpty(mappingFilePath))
                return false;

            try
            {
                if (string.IsNullOrEmpty(Path.GetDirectoryName(configuredMappingPath)))
                    return false;
            }
            catch
            {
                return false;
            }

            return string.Equals(configuredMappingPath, mappingFilePath,
                                 StringComparison.OrdinalIgnoreCase) == false;
        }

        /// <summary>
        /// Puts a file path in a read only box and says underneath whether it is there.
        /// The modification date is part of the answer to "am I using the right file": two
        /// databases of the same name on two machines are told apart by little else.
        /// </summary>
        /// <param name="box">The box holding the path</param>
        /// <param name="status">The line underneath it</param>
        /// <param name="path">The resolved path, empty when there is none</param>
        /// <param name="missingMessage">What to say when nothing could be resolved at all</param>
        private static void ShowPath(WpfTextBox box, TextBlock status, string path, string missingMessage)
        {
            box.Text = string.IsNullOrEmpty(path) ? "" : path;
            box.ToolTip = string.IsNullOrEmpty(path) ? null : path;

            if (string.IsNullOrEmpty(path))
            {
                SetStatus(status, missingMessage, true);
                return;
            }

            if (File.Exists(path) == false)
            {
                //Offline share against deleted file: the first repairs itself, the second does not.
                if (PathUtils.IsTemporarilyUnavailable(path) == true)
                    SetStatus(status, "This location cannot be reached right now.", true);
                else
                    SetStatus(status, "This file does not exist.", true);

                return;
            }

            string changed;

            try
            {
                changed = File.GetLastWriteTime(path).ToString("dd MMM yyyy HH:mm", CultureInfo.CurrentCulture);
            }
            catch
            {
                changed = "";
            }

            SetStatus(status, changed == "" ? "Found." : "Found, last changed " + changed + ".", false);
        }

        /// <summary>
        /// Adds a line to a status line, and colours the whole of it as something to act on.
        /// </summary>
        private static void AppendStatus(TextBlock status, string message)
        {
            string text = string.IsNullOrEmpty(status.Text)
                ? message
                : status.Text + Environment.NewLine + message;

            SetStatus(status, text, true);
        }

        /// <summary>
        /// Writes one of the file status lines. Anything the user should act on before importing
        /// is coloured like the allowance warnings.
        /// </summary>
        private static void SetStatus(TextBlock status, string message, bool isProblem)
        {
            status.Text = message;
            status.ToolTip = message;
            status.Foreground = isProblem ? statusProblemBrush : statusOkBrush;
            status.FontWeight = isProblem ? System.Windows.FontWeights.SemiBold : System.Windows.FontWeights.Normal;
        }

        /// <summary>
        /// The material database file behind the template selected in cbb_Template.
        /// Empty when the selection resolves to no file at all.
        /// </summary>
        private string ResolveSelectedTemplatePath()
        {
            //SelectedItem rather than Text, Text still holds the old value while SelectionChanged runs.
            string templateName = cbb_Template.SelectedItem as string;

            if (string.IsNullOrEmpty(templateName))
                templateName = cbb_Template.Text == null ? "" : cbb_Template.Text.Trim();

            if (string.IsNullOrEmpty(templateName))
                return "";

            string path = null;

            if (templateCollection != null)
                templateCollection.TryGetValue(templateName, out path);

            //The combo box holds file names; fall back to the usual resolution for a full path.
            if (string.IsNullOrEmpty(path) || File.Exists(path) == false)
                path = PathUtils.getTemplateFilePath(templateName);

            return string.IsNullOrEmpty(path) ? "" : path;
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
            //Through the template list, not getTemplateFilePath: that one only searches the local
            //materials folder, so a database on a share or picked with btn_TemplatePath would be
            //silently read from a same named local file, or from the default.
            activeTemplate = CarboDatabase.LoadTemplate(ResolveSelectedTemplatePath());
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

            ReloadActiveTemplateIntoUI();
        }

        /// <summary>
        /// Re-reads the selected template and refills everything that comes out of it, keeping
        /// what the user has picked.
        /// </summary>
        private void ReloadActiveTemplateIntoUI()
        {
            LoadActiveTemplate();

            //Keep what the user was working with, the new template may name things differently.
            //An empty box means the previous template had no match, so keep aiming for the original.
            foreach (AllowanceBlock block in allowanceBlocks)
            {
                block.WantedMaterial = Preserve(block.MaterialBox, block.WantedMaterial);
                block.WantedCategory = Preserve(block.CategoryBox, block.WantedCategory);
            }

            LoadAllowanceListsToUI();

            ShowFilePaths();
        }

        private static string Preserve(WpfComboBox box, string fallback)
        {
            return string.IsNullOrEmpty(box.Text) ? fallback : box.Text;
        }

        // ─────────────────────────────────────────────────────────────────────────────────────
        //  Names that have to exist in the Revit model
        //
        //  Every field below holds the name of something the import looks up: a parameter, a
        //  workset, a phase. Not one of them fails when the name is absent - the lookup returns
        //  nothing and the import carries on - so a name left over from the last project produces
        //  a complete, plausible, wrong result. These pickers offer what the model holds, and
        //  outline in red whatever is set but not there.
        // ─────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// What one of the fixed Type/Instance/Workset boxes is set to.
        ///
        /// SelectedItem rather than Text, and for the same reason as ResolveSelectedTemplatePath:
        /// inside a SelectionChanged handler, Text still holds the value the box had before the
        /// change. Reading it there would re-point a picker at the list the user just moved away
        /// from, and mark the new name against the old one's model list. Text is the fallback for
        /// a box with nothing selected at all, which is how an unset AdditionalParameterElementType
        /// arrives out of an older settings file.
        /// </summary>
        private static string TypeChoiceOf(WpfComboBox typeBox)
        {
            if (typeBox == null)
                return "";

            string selected = typeBox.SelectedItem as string;

            if (string.IsNullOrEmpty(selected) == false)
                return selected;

            return typeBox.Text == null ? "" : typeBox.Text;
        }

        /// <summary>
        /// Where the import looks the category override up. Compared the way getCategoryValue
        /// compares it, on the whole string, so the dialog and the import agree.
        /// </summary>
        private CarboNameKind CategoryNameKind()
        {
            return TypeChoiceOf(cbb_MainGroup) == "Type Parameter"
                ? CarboNameKind.TypeParameter
                : CarboNameKind.InstanceParameter;
        }

        /// <summary>
        /// Substructure is the awkward one: the same box holds either an instance parameter name
        /// or a fragment of a workset name, and the import decides which by looking for "workset"
        /// in the type - so that is what is looked for here too.
        /// </summary>
        private CarboNameKind SubstructureNameKind()
        {
            return TypeChoiceOf(cbb_SubstructureImportType).ToLower().Contains("workset")
                ? CarboNameKind.Workset
                : CarboNameKind.InstanceParameter;
        }

        /// <summary>
        /// Type or instance, read from one of the Type/Instance boxes the same way
        /// getParametervalue reads it.
        /// </summary>
        private static CarboNameKind ParameterKindOf(WpfComboBox typeBox)
        {
            return TypeChoiceOf(typeBox).ToLower().Contains("type")
                ? CarboNameKind.TypeParameter
                : CarboNameKind.InstanceParameter;
        }

        /// <summary>
        /// Every field whose value has to name something in the model, with what decides whether
        /// it is in use and which of the model's lists should hold it.
        /// </summary>
        private void BuildNameFields()
        {
            nameFields = new List<NameField>();

            nameFields.Add(new NameField
            {
                Label = "Category parameter",
                Box = cbb_CategoryparamName,
                IsActive = delegate { return TypeChoiceOf(cbb_MainGroup) != "(Revit) Category"; },
                Kind = CategoryNameKind
            });

            nameFields.Add(new NameField
            {
                Label = "Substructure parameter",
                Box = cbb_SubstructureParamName,
                IsActive = delegate { return chk_ImportSubstructure.IsChecked == true; },
                Kind = SubstructureNameKind
            });

            nameFields.Add(new NameField
            {
                Label = "Material grade parameter",
                Box = cbb_GradeImportValue,
                IsActive = delegate { return chk_MaterialGrade.IsChecked == true; },
                Kind = delegate { return ParameterKindOf(cbb_GradeImportType); },
                RequiredTypeBox = cbb_GradeImportType
            });

            nameFields.Add(new NameField
            {
                Label = "Correction parameter",
                Box = cbb_CorrectionImportValue,
                IsActive = delegate { return chk_doCorrection.IsChecked == true; },
                Kind = delegate { return ParameterKindOf(cbb_CorrectionImportType); },
                RequiredTypeBox = cbb_CorrectionImportType
            });

            nameFields.Add(new NameField
            {
                Label = "Additional parameter",
                Box = cbb_ExtraImportValue,
                IsActive = delegate { return chk_AdditionalImport.IsChecked == true; },
                Kind = delegate { return ParameterKindOf(cbb_ExtraImportType); },
                RequiredTypeBox = cbb_ExtraImportType
            });

            nameFields.Add(new NameField
            {
                Label = "Existing phase",
                Box = cbb_ExistingPhaseName,
                IsActive = delegate { return chk_ImportExisting.IsChecked == true; },
                Kind = delegate { return CarboNameKind.Phase; }
            });

            //The GIA fields have no tick box: empty means "measure the floors instead", which is
            //a documented answer rather than an unfinished one, so only a name that is filled in
            //and absent is worth marking.
            nameFields.Add(new NameField
            {
                Label = "Total GIA parameter",
                Box = cbb_GIAParamName,
                IsActive = delegate { return true; },
                Kind = delegate { return CarboNameKind.ProjectInformation; }
            });

            nameFields.Add(new NameField
            {
                Label = "New GIA parameter",
                Box = cbb_GIANewParamName,
                IsActive = delegate { return true; },
                Kind = delegate { return CarboNameKind.ProjectInformation; }
            });
        }

        /// <summary>
        /// Called by the pickers once the user has settled on a value.
        /// </summary>
        private void OnNameFieldChanged()
        {
            //Window_Loaded does its own check once everything is attached.
            if (uiReady == false)
                return;

            RefreshMissingNames();
        }

        /// <summary>
        /// A tick box that decides whether one of the name fields is used at all. Switching it off
        /// is a way of resolving a warning, so the count has to follow it.
        /// </summary>
        private void MissingNameSetting_Toggled(object sender, RoutedEventArgs e)
        {
            if (uiReady == false)
                return;

            RefreshMissingNames();
        }

        private void cbb_SubstructureImportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (uiReady == false)
                return;

            CarboParameterPicker.Repoint(cbb_SubstructureParamName, SubstructureNameKind());

            ShowSubstructureHint();
            RefreshMissingNames();
        }

        private void cbb_GradeImportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RepointParameterPicker(cbb_GradeImportValue, cbb_GradeImportType);
        }

        private void cbb_CorrectionImportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RepointParameterPicker(cbb_CorrectionImportValue, cbb_CorrectionImportType);
        }

        private void cbb_ExtraImportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RepointParameterPicker(cbb_ExtraImportValue, cbb_ExtraImportType);
        }

        private void RepointParameterPicker(WpfComboBox box, WpfComboBox typeBox)
        {
            if (uiReady == false)
                return;

            CarboParameterPicker.Repoint(box, ParameterKindOf(typeBox));

            RefreshMissingNames();
        }

        /// <summary>
        /// Says what the box beside the substructure type is for, because it is two different
        /// things: a parameter name to be matched in full, or a fragment any workset name may
        /// contain. A model with no worksets at all is worth saying out loud, since choosing
        /// worksets in that model cannot ever mark anything as substructure.
        /// </summary>
        private void ShowSubstructureHint()
        {
            if (SubstructureNameKind() != CarboNameKind.Workset)
            {
                txt_SubstructureHint.Text =
                    "Define a Yes/No instance parameter to identify the building's substructure";
                return;
            }

            if (CarboModelNames.HasModel == true && CarboModelNames.CountOf(CarboNameKind.Workset) == 0)
            {
                txt_SubstructureHint.Text =
                    "Every element whose workset name contains the text below counts as substructure. " +
                    "This model has no worksets, so nothing would be found.";
                return;
            }

            txt_SubstructureHint.Text =
                "Every element whose workset name contains the text below counts as substructure";
        }

        /// <summary>
        /// Outlines every field that names something this model does not hold, and says how many
        /// there are at the top of the dialog.
        /// </summary>
        private void RefreshMissingNames()
        {
            if (nameFields == null)
                return;

            List<string> missing = new List<string>();

            foreach (NameField field in nameFields)
            {
                string problem = ProblemWith(field);

                MarkNameField(field, problem);

                if (problem != null)
                    missing.Add(field.Label);
            }

            ShowMissingNames(missing);
        }

        /// <summary>
        /// What is wrong with one field, as a sentence for its tooltip. Null when nothing is.
        /// </summary>
        private static string ProblemWith(NameField field)
        {
            //Switched off: the import never reads it, so whatever is in the box is only a
            //remembered value and not a problem with this import.
            if (field.IsActive() == false)
                return null;

            string value = CarboParameterPicker.ValueOf(field.Box);

            //Empty means "not used" everywhere these are read.
            if (value.Length == 0)
                return null;

            //getParametervalue needs both the name and the type, and does nothing at all without
            //the type - so a named parameter with no type selected is read as nothing, silently.
            if (field.RequiredTypeBox != null && string.IsNullOrWhiteSpace(TypeChoiceOf(field.RequiredTypeBox)))
            {
                return "No parameter type is selected, so \"" + value + "\" is never read." +
                       Environment.NewLine +
                       "Pick Type Parameter or Instance Parameter in the box to the left.";
            }

            CarboNameKind kind = field.Kind();

            if (CarboModelNames.Contains(kind, value) == true)
                return null;

            string description = CarboModelNames.DescriptionOf(kind);

            if (CarboModelNames.CountOf(kind) == 0)
            {
                return "This model has no " + description + "s at all, so \"" + value +
                       "\" cannot be found." + Environment.NewLine +
                       "The import will look for it, find nothing and carry on.";
            }

            if (kind == CarboNameKind.Workset)
            {
                return "No workset in this model has a name containing \"" + value + "\"." +
                       Environment.NewLine +
                       "Nothing would be marked as substructure.";
            }

            return "This model has no " + description + " called \"" + value + "\"." +
                   Environment.NewLine +
                   "The import will look for it, find nothing and carry on, so this setting would " +
                   "have no effect.";
        }

        /// <summary>
        /// Outlines one field, or puts it back to normal. The tooltip the field was given in the
        /// XAML is restored rather than cleared, so explaining the problem does not cost the
        /// explanation of the field.
        /// </summary>
        private static void MarkNameField(NameField field, string problem)
        {
            if (field.DefaultToolTipRead == false)
            {
                field.DefaultToolTip = field.Box.ToolTip;
                field.DefaultToolTipRead = true;
            }

            if (problem == null)
            {
                field.Box.BorderBrush = normalNameBorderBrush;
                field.Box.BorderThickness = new Thickness(1);
                field.Box.ToolTip = field.DefaultToolTip;
                return;
            }

            field.Box.BorderBrush = missingNameBorderBrush;
            field.Box.BorderThickness = new Thickness(2);
            field.Box.ToolTip = problem;
        }

        /// <summary>
        /// The line above the three columns: how many fields name something that is not there, or
        /// that nothing could be checked at all.
        /// </summary>
        private void ShowMissingNames(List<string> missing)
        {
            string message = "";

            if (missing.Count > 0)
            {
                string subject = missing.Count == 1 ? "1 setting names" : missing.Count + " settings name";
                string boxes = missing.Count == 1 ? "the box marked in red" : "the boxes marked in red";

                message = subject + " something this model does not have: " +
                          string.Join(", ", missing.ToArray()) + ". " +
                          "The import looks each one up and carries on without it, so please review " +
                          boxes + " before importing.";
            }

            //Nothing was read, so nothing is known to be missing. Say that, rather than let an
            //absence of red borders read as an all clear. Added to whatever was found rather than
            //replacing it: a field with no parameter type selected is wrong whether or not there
            //is a model to check it against, and it is still marked in red below.
            if (CarboModelNames.HasModel == false)
            {
                if (message != "")
                    message += Environment.NewLine;

                message +=
                    "The Revit model could not be read, so the parameter, workset and phase names " +
                    "below have not been checked against it and the lists offer nothing to pick from. " +
                    "Type any name; the import will use it as it always has.";
            }

            if (message == "")
            {
                pnl_MissingNames.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            SetMissingNamesBanner(message, missing.Count > 0);
        }

        private void SetMissingNamesBanner(string message, bool isProblem)
        {
            txt_MissingNames.Text = message;
            txt_MissingNames.Foreground = isProblem ? statusProblemBrush : statusOkBrush;

            pnl_MissingNames.BorderBrush = isProblem ? statusProblemBrush : statusOkBrush;
            pnl_MissingNames.Background = isProblem
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xF6, 0xE9))
                : System.Windows.Media.Brushes.WhiteSmoke;

            pnl_MissingNames.Visibility = System.Windows.Visibility.Visible;
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
            //Save what is on screen first, so nothing typed here is lost if the import is
            //abandoned. SaveSettings writes the whole settings file; the extra
            //importSettings.SerializeXML() that used to sit here wrote a CarboGroupSettings
            //document straight over it, so cancelling the file dialog below left the settings
            //file unreadable and every setting reset on the next launch.
            SaveSettings();

            string pathNewFile = importSettings.ImportSettingsFile();

            //Cancelled, or nothing usable picked: leave the current settings exactly as they are.
            if (string.IsNullOrEmpty(pathNewFile) || !File.Exists(pathNewFile))
                return;

            string path = PathUtils.getSettingsFilePath();

            if (PathUtils.OverrideSettingsFile(pathNewFile, path) == false)
                return;

            System.Windows.MessageBox.Show("Settings imported. Restart CarboLifeCalculator to load the settings.");

            this.Close();
        }
    }
}
