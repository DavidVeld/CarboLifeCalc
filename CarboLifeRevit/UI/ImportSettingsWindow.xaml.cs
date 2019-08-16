using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarboLifeRevit
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ImportSettingsWindow : Window
    {
        public MessageBoxResult dialogOk;
        public List<CarboLevel> carboLevelList;
        public CarboRevitImportSettings importSettings;
        public ImportSettingsWindow(List<CarboLevel> levelList)
        {
            dialogOk = MessageBoxResult.Cancel;
            carboLevelList = levelList;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dialogOk = MessageBoxResult.Cancel;

            CarboRevitImportSettings settings = new CarboRevitImportSettings();
            settings = settings.DeSerializeXML();
            importSettings = settings;

            List<string> categorylist = new List<string>();
            categorylist.Add("(Revit) Category");
            categorylist.Add("Type Comment");
            categorylist.Add("Family Name");
            categorylist.Add("Level");
            categorylist.Add("CarboLifeCategory");
            categorylist.Add("");

            foreach (string str in categorylist)
            {
                cbb_MainGroup.Items.Add(str);
                cbb_SecGroup.Items.Add(str);
            }

            cbb_MainGroup.Text = "(Revit) Category";
            cbb_SecGroup.Text = "Family Name";

            //LevelList
            if (carboLevelList.Count > 0) {
                foreach (CarboLevel cl in carboLevelList)
                {
                    cbb_Levels.Items.Add(cl.Name);
                }
                cbb_Levels.SelectedIndex = 0;
            }

            cbb_MainGroup.Text = settings.MainCategory;
            cbb_SecGroup.Text = settings.SubCategory;
            cbb_Levels.Text = settings.CutoffLevel;
            //txt_SpecialTypes.Text = settings.TypeNameSeparators;
            chk_ImportDemolished.IsChecked = settings.IncludeDemo;

        }

        private double getCutoffLevel()
        {
            double result = 0;

            foreach (CarboLevel cl in carboLevelList)
            {
                if (cl.Name == cbb_Levels.Text)
                {
                    result = cl.Level;
                }
            }
            return result;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            dialogOk = MessageBoxResult.Cancel;
            this.Close();
        }

        private void Btn_ImportClose_Click(object sender, RoutedEventArgs e)
        {
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
            CarboRevitImportSettings settings = new CarboRevitImportSettings();
            settings.MainCategory = cbb_MainGroup.Text;
            settings.SubCategory = cbb_SecGroup.Text;
            settings.IncludeDemo = chk_ImportDemolished.IsChecked.Value;
            settings.CutoffLevel = cbb_Levels.Text;
            settings.CutoffLevelValue = getCutoffLevel();
            settings.SerializeXML();

            importSettings = settings;
        }
    }
}
