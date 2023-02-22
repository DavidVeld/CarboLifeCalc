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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;
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

            List<string> categorylist = new List<string>();
            categorylist.Add("(Revit) Category");
            //categorylist.Add("Type Name");
            //categorylist.Add("Type Comment");
            //categorylist.Add("Comment");


            foreach (string str in categorylist)
            {
                cbb_MainGroup.Items.Add(str);
            }

            cbb_MainGroup.Text = "(Revit) Category";

            //TemplateList

            //get DefaultTemplate:
            string defaultTemplate = PathUtils.getTemplateFolder();

            string defaultfimeName = System.IO.Path.GetFileName(defaultTemplate);
            cbb_Template.Items.Add(defaultTemplate);


            string myLocalPath = PathUtils.getAssemblyPath() + "\\db\\";
            string[] files = System.IO.Directory.GetFiles(myLocalPath, "*.cxml");

            if (files.Length > 0) {
                foreach (string file in files)
                {
                    string fimeName = System.IO.Path.GetFileName(file);

                    if (System.IO.File.Exists(file))
                    {
                        //add if not default of the buffer
                        if (!fimeName.Contains("buffer") || defaultfimeName != fimeName)
                        {
                            cbb_Template.Items.Add(fimeName);
                        }
                    }
                }
            }
            cbb_Template.Text = defaultfimeName;
            cbb_MainGroup.Text = "(Revit) Category";
            txt_SubstructureParamName.Text = importSettings.SubStructureParamName;
            chk_ImportSubstructure.IsChecked = importSettings.IncludeSubStructure;
            //txt_SpecialTypes.Text = settings.TypeNameSeparators;
            chk_ImportExisting.IsChecked = importSettings.IncludeExisting;
            txt_ExistingPhaseName.Text = importSettings.ExistingPhaseName;

            chk_ImportDemolished.IsChecked = importSettings.IncludeDemo;
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
            
            importSettings.CategoryName = cbb_MainGroup.Text;

            importSettings.SubStructureParamName = txt_SubstructureParamName.Text;
            importSettings.IncludeSubStructure = chk_ImportSubstructure.IsChecked.Value;

            importSettings.IncludeDemo = chk_ImportDemolished.IsChecked.Value;
            importSettings.IncludeExisting = chk_ImportExisting.IsChecked.Value;

            //Save the latest settings in the default;
            CarboSettings settings = new CarboSettings();
            settings.Load();
            settings.defaultCarboGroupSettings = importSettings;
            settings.Save();

            //importSettings.SerializeXML();
        }
    }
}
