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
    /// Interaction logic for MaterialEditor.xaml
    /// </summary>
    public partial class MaterialEditor : Window
    {
        public bool acceptNew;

        private CarboMaterial originalMaterial;
        private CarboDatabase originalDatabase;
        private CarboDatabase baseMaterials;


        public CarboMaterial selectedMaterial;
        public CarboMaterial returnedMaterial;
        public CarboDatabase returnedDatabase;

        public MaterialEditor(CarboMaterial material, CarboDatabase database)
        {
            originalMaterial = material;
            returnedMaterial = material;
            selectedMaterial = material;

            originalDatabase = database;
            returnedDatabase = database;

            baseMaterials = new CarboDatabase();
            baseMaterials.DeSerializeXML("BaseMaterials");


            acceptNew = false;

            InitializeComponent();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            acceptNew = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> materialCategories = new List<string>();

            foreach(CarboMaterial cm in returnedDatabase.CarboMaterialList)
            {
                bool uniqueCategory = true;

                liv_materialList.Items.Add(cm);
                foreach(string mc in materialCategories)
                {
                    if(mc == cm.Category)
                    {
                        uniqueCategory = false;
                    }
                }
                if(uniqueCategory == true)
                {
                    materialCategories.Add(cm.Category);
                    cbb_Categories.Items.Add(cm.Category);
                    cbb_Category.Items.Add(cm.Category);
                }
            }

            liv_materialList.SelectedItem = selectedMaterial;
            cbb_Categories.Items.Add("All");
            cbb_Categories.Text = "All";

            UpdateMaterialSettings();
            
        }

        private void Liv_materialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedMaterial = liv_materialList.SelectedItem as CarboMaterial;
            //MaterialMap selectedFloorMap = GetMaterialMap(cbb_FloorTypes.Text);
            
            if (selectedMaterial != null)
            {
                returnedMaterial = selectedMaterial;
                UpdateMaterialSettings();
            }
        }

        private void UpdateMaterialSettings()
        {
            txt_Name.Text = selectedMaterial.Name;
            txt_Description.Text = selectedMaterial.Description;
            cbb_Category.Text = selectedMaterial.Category;
            txt_Density.Text = selectedMaterial.Density.ToString();
            txt_ECI.Text = selectedMaterial.ECI.ToString();
            txt_EEI.Text = selectedMaterial.EEI.ToString();
            txt_A1_A3.Text = selectedMaterial.ECI_A1A3.ToString();
            txt_A4_A5.Text = selectedMaterial.ECI_A4A5.ToString();
            txt_B1_B7.Text = selectedMaterial.ECI_B1B7.ToString();
            txt_C1_C4.Text = selectedMaterial.ECI_C1C4.ToString();
            chk_Locked.IsChecked = selectedMaterial.isLocked;

            if(selectedMaterial.isLocked == true)
            {
                grd_Edit.Visibility = Visibility.Hidden;
            }
            else
            {
                grd_Edit.Visibility = Visibility.Visible;

            }

        }

        private void Cbb_Categories_DropDownClosed(object sender, EventArgs e)
        {
            RefreshMaterialList();
        }

        private void RefreshMaterialList()
        {
            selectedMaterial = null;
            string cat = cbb_Categories.SelectedItem.ToString();
            liv_materialList.Items.Clear();
            foreach(CarboMaterial cm in returnedDatabase.CarboMaterialList)
            {
                if(cm.Category == cat || cat == "All")
                {
                    if (chb_ShowICE.IsChecked == true)
                    {
                        //show all
                            liv_materialList.Items.Add(cm);
                    }
                    else
                    {
                        //Only show non locked items
                        if (cm.isLocked == false)
                        {
                            liv_materialList.Items.Add(cm);
                        }
                    }
                }
            }
        }

        private void Chb_ShowICE_Click(object sender, RoutedEventArgs e)
        {
            RefreshMaterialList();
        }
    }
}
