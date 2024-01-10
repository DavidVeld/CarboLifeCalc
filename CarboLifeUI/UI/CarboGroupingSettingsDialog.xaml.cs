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

            //Category Settings
            List<string> categorylist = new List<string>();
            categorylist.Add("(Revit) Category");
            categorylist.Add("Type Parameter");
            categorylist.Add("Instance Parameter");

            foreach (string str in categorylist)
            {
                cbb_MainGroup.Items.Add(str);
            }

            cbb_MainGroup.Text = "(Revit) Category";

            txt_CategoryparamName.Text = importSettings.CategoryParamName;
            txt_CategoryparamName.IsEnabled = false;

            CheckCaregoryParam();

            //RC
            if (importSettings.mapReinforcement == true)
            {
                chk_MapReinforcement.IsChecked = true;
            }
            else
            {
                chk_MapReinforcement.IsChecked = false;
            }

            //Substructure
            txt_SubstructureParamName.Text = importSettings.SubStructureParamName;
            chk_ImportSubstructure.IsChecked = importSettings.IncludeSubStructure;

            cbb_SubstructureImportType.Items.Add("Parameter (Instance Boolean)");
            cbb_SubstructureImportType.Items.Add("Workset Name Contains");
            cbb_SubstructureImportType.SelectedItem = importSettings.SubStructureParamType;

            //Grade
            cbb_GradeImportType.Items.Add("Type Parameter");
            cbb_GradeImportType.Items.Add("Instance Parameter");
            cbb_GradeImportType.Items.Add("Material Parameter");

            if (importSettings.IncludeGradeParameter == true)
            {
                cbb_GradeImportType.Text = importSettings.GradeParameterType.ToString();
                txt_GradeImportValue.Text = importSettings.GradeParameterName.ToString();
                chk_MaterialGrade.IsChecked = true;
            }
            else
            {
                chk_MaterialGrade.IsChecked = false;
                cbb_GradeImportType.Text = "Type Parameter";
                txt_GradeImportValue.Text = "";
            }

            txt_GradeImportValue.Text = importSettings.GradeParameterName;



            //CorrectionList
            cbb_CorrectionImportType.Items.Clear();
            cbb_CorrectionImportType.Items.Add("Type Parameter");
            cbb_CorrectionImportType.Items.Add("Instance Parameter");
            cbb_CorrectionImportType.SelectedItem = "Type Parameter";

            if (importSettings.IncludeCorrectionParameter == true)
            { 
                chk_doCorrection.IsChecked = true;
            }
            else
            {
                chk_doCorrection.IsChecked =  false;
            }





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
            cbb_ExtraImportType.SelectedItem = "Type Parameter";

            if (importSettings.IncludeAdditionalParameter == true)
                chk_AdditionalImport.IsChecked = true;

            if (importSettings.AdditionalParameterElementType.ToLower().Contains("type"))
                cbb_ExtraImportType.Text = "Type Parameter";
            else
                cbb_ExtraImportType.Text = "Instance Parameter";

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

            if (cbb_ExtraImportType.Text == "Type Parameter")
                settings.defaultCarboGroupSettings.AdditionalParameterElementType = "Type Parameter";
            else
                settings.defaultCarboGroupSettings.AdditionalParameterElementType = "Instance Parameter";

            //Grade
            settings.defaultCarboGroupSettings.IncludeGradeParameter = chk_MaterialGrade.IsChecked.Value;
            settings.defaultCarboGroupSettings.GradeParameterName = txt_GradeImportValue.Text;
            settings.defaultCarboGroupSettings.GradeParameterType = cbb_GradeImportType.Text;

            //RC
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
