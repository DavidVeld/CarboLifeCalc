using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using CarboLifeAPI.Data.Superseded;
using LiveCharts.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialConcreteMapper.xaml
    /// </summary>
    public partial class MaterialConcreteMapper : Window
    {
        internal bool isAccepted;
        public string sourcePath;


        public List<CarboNumProperty> rcMap { get; set; }
        public string categoryType { get; set; }
        public string categoryName { get; set; }
        public string carboMaterialName { get; set; }
        public string carboMaterialCategory { get; set; }


        public MaterialConcreteMapper(CarboGroupSettings carboSettings)
        {
            CarboDatabase template = new CarboDatabase();
            template = template.DeSerializeXML(""); 

            //List<string> materialList = new List<string>();

            this.InitializeComponent();

            foreach(CarboMaterial cm in template.CarboMaterialList)
            {
                cbb_RCImportMaterial.Items.Add(cm.Name);
            }

            cbb_RCImportType.Items.Clear();
            cbb_RCImportType.Items.Add("Type Parameter");
            cbb_RCImportType.Items.Add("Instance Parameter");

            List<string> CategoryList = template.getCategoryList();
            foreach(string category in CategoryList) 
            {
                cbb_RCMaterialCategory.Items.Add(category);
            }

            rcMap = carboSettings.rcQuantityMap;
            categoryType = carboSettings.RCParameterType;
            categoryName = carboSettings.RCParameterName;
            carboMaterialName = carboSettings.RCMaterialName;
            carboMaterialCategory = carboSettings.RCMaterialCategory;

            if (rcMap.Count == 0)
            {
                System.Windows.MessageBox.Show("No RC properties found in the project, default mapping table will be used.", "Warning", MessageBoxButton.OK);

                CarboSettings settings = new CarboSettings();
                settings = settings.Load();

                rcMap = settings.defaultCarboGroupSettings.rcQuantityMap;
                categoryType = settings.defaultCarboGroupSettings.RCParameterType;
                categoryName = settings.defaultCarboGroupSettings.RCParameterName;
                carboMaterialName = settings.defaultCarboGroupSettings.RCMaterialName;
                carboMaterialCategory = settings.defaultCarboGroupSettings.RCMaterialCategory;
            }

                DataContext = this;

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbb_RCImportType.SelectedItem = categoryType;
            txt_RCImportValue.Text = categoryName;
            cbb_RCImportMaterial.SelectedItem = carboMaterialName;
            cbb_RCMaterialCategory.SelectedItem = carboMaterialCategory;

        }


        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;

            carboMaterialName = cbb_RCImportMaterial.Text;
            categoryName = txt_RCImportValue.Text;
            categoryType = cbb_RCImportType.Text;
            carboMaterialCategory = cbb_RCMaterialCategory.Text;

            this.Close();
        }

        public IEnumerable<DataGridRow> GetDataGridRows(System.Windows.Controls.DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as System.Collections.IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }


        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

 
    }
}
