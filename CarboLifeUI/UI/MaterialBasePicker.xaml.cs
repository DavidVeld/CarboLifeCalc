using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for MaterialBasePicker.xaml
    /// </summary>
    public partial class MaterialBasePicker : Window
    {
        public CarboDatabase basematerials;
        public CarboMaterial selectedBaseMaterial;

        public bool isAccepted;

        public MaterialBasePicker(string selection = "")
        {
            isAccepted = false;
            basematerials = new CarboDatabase();

            basematerials = basematerials.DeSerializeXML("");
            selectedBaseMaterial = basematerials.GetExcactMatch(selection);

            InitializeComponent();
        }

        public MaterialBasePicker(CarboDatabase baseMaterials, string selection = "")
        {
            basematerials = baseMaterials;
            isAccepted = false;

            selectedBaseMaterial = basematerials.GetExcactMatch(selection);

            InitializeComponent();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(basematerials != null)
            {
                IList<string> categories = basematerials.getCategoryList();
                foreach(string cat in categories)
                {
                    cbb_Categories.Items.Add(cat);
                }
                cbb_Categories.Items.Add("All");
                cbb_Categories.Text = "All";
            }

            lib_Materials.ItemsSource = basematerials.CarboMaterialList;
        }

        private void lib_Materials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CarboMaterial cm = lib_Materials.SelectedItem as CarboMaterial;
            if (cm != null)
            {
                txt_Search.Text = cm.Name;
                refreshInterface();
            }
        }

        private void Txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            CarboMaterial material = basematerials.getClosestMatch(txt_Search.Text);
            selectedBaseMaterial = material;
            cbb_Categories.Text = selectedBaseMaterial.Category;
            //lib_Materials.SelectedItem = selectedBaseMaterial;

            //refreshInterface();
        }

        private void refreshInterface()
        {
            dgv_Details.ItemsSource = CarboLifeAPI.Utils.ToDataTables(selectedBaseMaterial).DefaultView;
        }
        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            this.Close();
        }
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void Cbb_Categories_DropDownClosed(object sender, EventArgs e)
        {
            lib_Materials.ItemsSource = null;
            lib_Materials.Items.Clear();

            foreach (CarboMaterial cm in basematerials.CarboMaterialList)
            {

                if(cbb_Categories.Text == cm.Category || cbb_Categories.Text == "All")
                {
                    lib_Materials.Items.Add(cm);
                }
            }
        }
    }
}
