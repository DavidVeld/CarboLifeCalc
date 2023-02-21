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

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ImportSettingsWindow : Window
    {
        public MessageBoxResult dialogOk;
        //public List<CarboLevel> carboLevelList;
        public CarboRevitImportSettings importSettings;
        public ImportSettingsWindow()
        {
            dialogOk = MessageBoxResult.Cancel;
           // carboLevelList = levelList;

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
            categorylist.Add("Type Name");
            categorylist.Add("Type Comment");
            categorylist.Add("Comment");


            foreach (string str in categorylist)
            {
                cbb_MainGroup.Items.Add(str);
            }

            cbb_MainGroup.Text = "(Revit) Category";

            //TemplateList
            string dbpath = PathUtils.getAssemblyPath() + "\\db\\";

            string[] files = System.IO.Directory.GetFiles(dbpath, "*.cxml");

            if (files.Length > 0) {
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        string fimeName = System.IO.Path.GetFileName(file);
                        cbb_Template.Items.Add(fimeName);
                    }
                }
            }
            

            cbb_MainGroup.Text = settings.CategoryName;
            txt_IsSubstructureParam.Text = settings.IsSubStructureParamName;
            //txt_SpecialTypes.Text = settings.TypeNameSeparators;
            chk_ImportDemolished.IsChecked = settings.IncludeDemo;
            chk_ImportExisting.IsChecked = settings.IncludeExisting;
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
            settings.CategoryName = cbb_MainGroup.Text;
            settings.IsSubStructureParamName = txt_IsSubstructureParam.Text;
            settings.IncludeDemo = chk_ImportDemolished.IsChecked.Value;
            settings.IncludeExisting = chk_ImportExisting.IsChecked.Value;

            settings.SerializeXML();
            importSettings = settings;
        }
    }
}
