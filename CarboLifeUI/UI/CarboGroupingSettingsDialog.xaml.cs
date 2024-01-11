using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using CarboLifeAPI.Data.Superseded;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

            //TemplateList

            //get DefaultTemplate:
            templateCollection = PathUtils.getTemplateFiles();
            if (templateCollection != null)
            {
                foreach (var template in templateCollection)
                {
                    cbb_Template.Items.Add(template.Key);
                }
                cbb_Template.SelectedIndex = 0;
            }

            loadSettingsToUI();
 
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

            //RC
            chk_MapReinforcement.IsChecked = importSettings.mapReinforcement;

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
            chk_CombineExistingAndDemo.IsChecked = importSettings.CombineExistingAndDemo;

            //Additional Parameter
            cbb_ExtraImportType.Items.Clear();
            cbb_ExtraImportType.Items.Add("Type Parameter");
            cbb_ExtraImportType.Items.Add("Instance Parameter");

            chk_AdditionalImport.IsChecked = importSettings.IncludeAdditionalParameter;
            cbb_ExtraImportType.SelectedItem = importSettings.AdditionalParameterElementType;
            txt_ExtraImportValue.Text = importSettings.AdditionalParameter;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            dialogOk = MessageBoxResult.Cancel;
            this.Close();
        }

        private void Btn_ImportClose_Click(object sender, RoutedEventArgs e)
        {
            string result;
            if (templateCollection.TryGetValue(cbb_Template.Text, out result))

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
            settings.defaultCarboGroupSettings.CombineExistingAndDemo = chk_CombineExistingAndDemo.IsChecked.Value;

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
            settings.defaultCarboGroupSettings.RCMaterialName = importSettings.RCMaterialName;
            settings.defaultCarboGroupSettings.rcQuantityMap = importSettings.rcQuantityMap;
            settings.defaultCarboGroupSettings.RCMaterialCategory = importSettings.RCMaterialCategory;

            //Seve as default for next time/project;
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Carbo Life Project File (*.clcx)|*.clcx|All files (*.*)|*.*";

            var path = openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName))
            {
                this.projectPath = openFileDialog.FileName;
                txt_ProjectPath.Text = openFileDialog.FileName;
            }
        }

        private void btn_ReinforcementImport_Click(object sender, RoutedEventArgs e)
        {
            MaterialConcreteMapper rcMapper = new MaterialConcreteMapper(importSettings);
            rcMapper.ShowDialog();
            if(rcMapper.isAccepted == true)
            {
                importSettings.RCMaterialName = rcMapper.carboMaterialName;
                importSettings.RCParameterName = rcMapper.categoryName;
                importSettings.RCParameterType = rcMapper.categoryType;
                importSettings.RCMaterialCategory = rcMapper.carboMaterialCategory;

                importSettings.rcQuantityMap = rcMapper.rcMap;
            }


        }



    }
}
