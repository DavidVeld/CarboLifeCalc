using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

            dialogOk = MessageBoxResult.Cancel;
           // carboLevelList = levelList;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dialogOk = MessageBoxResult.Cancel;

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
            //cbb_Template.Text = defaultfimeName;
            cbb_MainGroup.Text = "(Revit) Category";
            
            txt_SubstructureParamName.Text = importSettings.SubStructureParamName;
            chk_ImportSubstructure.IsChecked = importSettings.IncludeSubStructure;
            
            chk_ImportExisting.IsChecked = importSettings.IncludeExisting;
            txt_ExistingPhaseName.Text = importSettings.ExistingPhaseName;

            chk_ImportDemolished.IsChecked = importSettings.IncludeDemo;
            chk_CombineExistingAndDemo.IsChecked = importSettings.CombineExistingAndDemo;

            CheckCaregoryParam();

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
               System.Windows.MessageBox.Show("The Selected Template could not be found");
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

            settings.defaultCarboGroupSettings.SubStructureParamName = txt_SubstructureParamName.Text;
            settings.defaultCarboGroupSettings.IncludeSubStructure = chk_ImportSubstructure.IsChecked.Value;

            settings.defaultCarboGroupSettings.IncludeDemo = chk_ImportDemolished.IsChecked.Value;
            settings.defaultCarboGroupSettings.IncludeExisting = chk_ImportExisting.IsChecked.Value;
            settings.defaultCarboGroupSettings.CombineExistingAndDemo = chk_CombineExistingAndDemo.IsChecked.Value;

            settings.Save();

            //importSettings.SerializeXML();
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
    }
}
