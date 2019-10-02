using CarboLifeAPI;
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
        public CarboDatabase returnedDatabase;

        public MaterialEditor(CarboMaterial material, CarboDatabase database)
        {
            originalMaterial = material;
            selectedMaterial = material;

            originalDatabase = database;
            returnedDatabase = database;

            baseMaterials = new CarboDatabase();
            baseMaterials = baseMaterials.DeSerializeXML("db\\BaseMaterials");

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
            materialCategories = returnedDatabase.getCategoryList();

            foreach (string cat in materialCategories)
            {
                cbb_Categories.Items.Add(cat);
                cbb_Category.Items.Add(cat);
            }
            cbb_Categories.Items.Add("All");
            cbb_Categories.Text = "All";

            RefreshMaterialList();


            liv_materialList.SelectedItem = selectedMaterial;

            UpdateMaterialSettings();  
        }

        private void Liv_materialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedMaterial = liv_materialList.SelectedItem as CarboMaterial;
            //MaterialMap selectedFloorMap = GetMaterialMap(cbb_FloorTypes.Text);
            
            if (selectedMaterial != null)
            {
                UpdateMaterialSettings();
            }
        }

        private void UpdateMaterialSettings()
        {
            selectedMaterial.CalculateTotals();

            txt_Name.Text = selectedMaterial.Name;
            txt_Description.Text = selectedMaterial.Description;
            cbb_Category.Text = selectedMaterial.Category;
            txt_Density.Text = selectedMaterial.Density.ToString();
            txt_ECI.Text = selectedMaterial.ECI.ToString();
            //txt_EEI.Text = selectedMaterial.EEI.ToString();

            txt_A1_A3.Text = selectedMaterial.ECI_A1A3.ToString();
            txt_A1_A3_Setting.Text = selectedMaterial.GetCarboProperty("ECI_A1A3_Settings").Value;

            txt_A4_A5.Text = selectedMaterial.ECI_A4A5.ToString();
            txt_A4_A5_Setting.Text = selectedMaterial.GetCarboProperty("ECI_A4A5_Settings").Value;

            txt_B1_B7.Text = selectedMaterial.ECI_B1B7.ToString();
            txt_B1_B7_Setting.Text = selectedMaterial.GetCarboProperty("ECI_B1B7_Settings").Value;

            txt_C1_C4.Text = selectedMaterial.ECI_C1C4.ToString();
            txt_C1_C4_Setting.Text = selectedMaterial.GetCarboProperty("ECI_C1C4_Settings").Value;

            txt_D.Text = selectedMaterial.ECI_D.ToString();
            txt_D_Setting.Text = selectedMaterial.GetCarboProperty("ECI_D_Settings").Value;

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
            //selectedMaterial = null;
            string cat = cbb_Categories.Text;
            liv_materialList.Items.Clear();
            foreach(CarboMaterial cm in returnedDatabase.CarboMaterialList)
            {
                if(cm.Category == cat || cat == "All" || cat == "")
                {
                    liv_materialList.Items.Add(cm);
                }
            }
            if (selectedMaterial != null)
                liv_materialList.SelectedItem = selectedMaterial;
        }

        private void Chb_ShowICE_Click(object sender, RoutedEventArgs e)
        {
            RefreshMaterialList();
        }

        private void Btn_A1_A3_Click(object sender, RoutedEventArgs e)
        {
            MaterialBasePicker materialBase = new MaterialBasePicker(baseMaterials, txt_A1_A3_Setting.Text);
            materialBase.ShowDialog();
            if(materialBase.isAccepted == true)
            {
                selectedMaterial.Category = materialBase.selectedBaseMaterial.Category;
                selectedMaterial.ECI_A1A3 = materialBase.selectedBaseMaterial.ECI_A1A3;
                selectedMaterial.SetProperty("ECI_A1A3_Settings", materialBase.selectedBaseMaterial.Name);
            }

            UpdateMaterialSettings();

        }

        private void Btn_A4_A5_Click(object sender, RoutedEventArgs e)
        {
            MaterialTransportPicker materialTransportPicker = new MaterialTransportPicker(txt_A4_A5_Setting.Text, Utils.ConvertMeToDouble(txt_A4_A5.Text));
            materialTransportPicker.ShowDialog();
            if (materialTransportPicker.isAccepted == true)
            {
                //selectedMaterial.Category = materialTransportPicker.selectedBaseMaterial.Category;
                selectedMaterial.ECI_A4A5 = materialTransportPicker.Value;
                selectedMaterial.SetProperty("ECI_A4A5_Settings", materialTransportPicker.Settings);
            }
            UpdateMaterialSettings();
        }

        private void Btn_B1_B7_Click(object sender, RoutedEventArgs e)
        {
            MaterialConstructionPicker materialConstructionPicker = new MaterialConstructionPicker(txt_B1_B7_Setting.Text, Utils.ConvertMeToDouble(txt_B1_B7.Text));
            materialConstructionPicker.ShowDialog();
            if (materialConstructionPicker.isAccepted == true)
            {
                //selectedMaterial.Category = materialTransportPicker.selectedBaseMaterial.Category;
                selectedMaterial.ECI_B1B7 = materialConstructionPicker.Value;
                selectedMaterial.SetProperty("ECI_B1B7_Settings", materialConstructionPicker.Settings);
            }
            UpdateMaterialSettings();
        }

        private void Btn_C1_C4_Click(object sender, RoutedEventArgs e)
        {
            MaterialLifePicker materialLifePicker = new MaterialLifePicker(txt_C1_C4_Setting.Text, Utils.ConvertMeToDouble(txt_C1_C4.Text));
            materialLifePicker.ShowDialog();
            if (materialLifePicker.isAccepted == true)
            {
                //selectedMaterial.Category = materialTransportPicker.selectedBaseMaterial.Category;
                selectedMaterial.ECI_C1C4 = materialLifePicker.Value;
                selectedMaterial.SetProperty("ECI_C1C4_Settings", materialLifePicker.Settings);
            }
            UpdateMaterialSettings();
        }

        private void Btn_D_Click(object sender, RoutedEventArgs e)
        {
            MaterialEndofLifePicker materialEndofLifePicker = new MaterialEndofLifePicker(txt_D_Setting.Text, Utils.ConvertMeToDouble(txt_D.Text));
            materialEndofLifePicker.ShowDialog();
            if (materialEndofLifePicker.isAccepted == true)
            {
                //selectedMaterial.Category = materialTransportPicker.selectedBaseMaterial.Category;
                selectedMaterial.ECI_D = materialEndofLifePicker.Value;
                selectedMaterial.SetProperty("ECI_D_Settings", materialEndofLifePicker.Settings);
            }
            UpdateMaterialSettings();
        }

        private void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateMaterialSettings();
        }

        private void Btn_Apply_Click(object sender, RoutedEventArgs e)
        {
            selectedMaterial.Name = txt_Name.Text;

            selectedMaterial.Description = txt_Description.Text;

            selectedMaterial.Category = cbb_Category.Text;
            selectedMaterial.Density = Utils.ConvertMeToDouble(txt_Density.Text);
            selectedMaterial.ECI = Utils.ConvertMeToDouble(txt_ECI.Text);
            selectedMaterial.ECI_A1A3 = Utils.ConvertMeToDouble(txt_A1_A3.Text);
            selectedMaterial.ECI_A4A5 = Utils.ConvertMeToDouble(txt_A4_A5.Text);
            selectedMaterial.ECI_B1B7 = Utils.ConvertMeToDouble(txt_B1_B7.Text);
            selectedMaterial.ECI_C1C4 = Utils.ConvertMeToDouble(txt_C1_C4.Text);
            selectedMaterial.ECI_D = 0;

            UpdateMaterialSettings();

            //selectedMaterial.EEI = Utils.ConvertMeToDouble(txt_EEI.Text);

        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            acceptNew = false;
            this.Close();
        }


    }
}
