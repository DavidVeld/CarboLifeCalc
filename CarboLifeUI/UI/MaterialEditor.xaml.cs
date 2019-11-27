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

        //private CarboMaterial originalMaterial;
        //private CarboDatabase originalDatabase;
        private CarboDatabase baseMaterials;


        public CarboMaterial selectedMaterial;
        public CarboDatabase returnedDatabase;

        public MaterialEditor(string materialName, CarboDatabase database)
        {
            //originalMaterial = material;
            //selectedMaterial = material;

            //originalDatabase = database;
            returnedDatabase = database;
            selectedMaterial = database.GetExcactMatch(materialName);
            if(selectedMaterial == null)
            {
                MessageBox.Show("This material could not be found in the database, the closest match will now be found");
                selectedMaterial = database.getClosestMatch(materialName);
            }

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
            selectMaterial(selectedMaterial.Name);

            UpdateMaterialSettings();  
        }

        private void selectMaterial(string name)
        {
            int index = 0;

            if (liv_materialList.Items.Count > 0)
            {
                foreach (CarboMaterial it in liv_materialList.Items)
                {
                    if (it.Name == name)
                    {
                        break;
                    }
                    index++;
                }
            }
            
            liv_materialList.SelectedIndex = index;
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

            txt_A1_A3.Text = Math.Round(selectedMaterial.ECI_A1A3,4).ToString();
            txt_A1_A3_Setting.Text = selectedMaterial.GetCarboProperty("ECI_A1A3_Settings").Value;

            txt_A4.Text = Math.Round(selectedMaterial.ECI_A4, 4).ToString();
            txt_A4_Setting.Text = selectedMaterial.GetCarboProperty("ECI_A4_Settings").Value;

            txt_A5.Text = Math.Round(selectedMaterial.ECI_A5, 4).ToString();
            txt_A5_Setting.Text = selectedMaterial.GetCarboProperty("ECI_A5_Settings").Value;

            txt_B1_B5.Text = Math.Round(selectedMaterial.ECI_B1B5, 4).ToString();
            txt_B1_B5_Setting.Text = selectedMaterial.GetCarboProperty("ECI_B1B5_Settings").Value;

            txt_C1_C4.Text = Math.Round(selectedMaterial.ECI_C1C4, 4).ToString();
            txt_C1_C4_Setting.Text = selectedMaterial.GetCarboProperty("ECI_C1C4_Settings").Value;

            txt_D.Text = Math.Round(selectedMaterial.ECI_D,4).ToString();
            txt_D_Setting.Text = selectedMaterial.GetCarboProperty("ECI_D_Settings").Value;

            string calc = "";

            calc = "B1B5 x (A1-A3 + A4 + A5 + B1-B5 + C1C4 + ECI_D) = ECI Total" + Environment.NewLine;
            calc += Math.Round(Utils.ConvertMeToDouble(txt_B1_B5.Text),2) + 
                " x (" + Math.Round(Utils.ConvertMeToDouble(txt_A1_A3.Text),2) +
                " + "+ Math.Round(Utils.ConvertMeToDouble(txt_A4.Text),2) + 
                " + " + Math.Round(Utils.ConvertMeToDouble(txt_A5.Text), 2) + 
                " + " + Math.Round(Utils.ConvertMeToDouble(txt_C1_C4.Text), 2) + 
                " + " + Math.Round(Utils.ConvertMeToDouble(txt_D.Text), 2) + 
                " ) = " + Math.Round(Utils.ConvertMeToDouble(txt_ECI.Text), 2);
            Calc.Content = calc;

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

        private void Btn_A4_Click(object sender, RoutedEventArgs e)
        {
            MaterialTransportPicker materialTransportPicker = new MaterialTransportPicker(txt_A4_Setting.Text, Utils.ConvertMeToDouble(txt_A4.Text), Utils.ConvertMeToDouble(txt_Density.Text));
            materialTransportPicker.ShowDialog();
            if (materialTransportPicker.isAccepted == true)
            {
                //selectedMaterial.Category = materialTransportPicker.selectedBaseMaterial.Category;
                selectedMaterial.ECI_A4 = materialTransportPicker.Value;
                selectedMaterial.SetProperty("ECI_A4_Settings", materialTransportPicker.Settings);
            }
            UpdateMaterialSettings();
        }

        private void Btn_A5_Click(object sender, RoutedEventArgs e)
        {
            MaterialConstructionPicker materialConstructionPicker = new MaterialConstructionPicker(txt_A5_Setting.Text, Utils.ConvertMeToDouble(txt_A5.Text));
            materialConstructionPicker.ShowDialog();
            if (materialConstructionPicker.isAccepted == true)
            {
                //selectedMaterial.Category = materialTransportPicker.selectedBaseMaterial.Category;
                selectedMaterial.ECI_A5 = materialConstructionPicker.Value;
                selectedMaterial.SetProperty("ECI_A5_Settings", materialConstructionPicker.Settings);
            }
            UpdateMaterialSettings();
        }

        private void Btn_B1_B5_Click(object sender, RoutedEventArgs e)
        {
            MaterialLifePicker materialLifePicker = new MaterialLifePicker(txt_B1_B5_Setting.Text, Utils.ConvertMeToDouble(txt_B1_B5.Text));
            materialLifePicker.ShowDialog();
            if (materialLifePicker.isAccepted == true)
            {
                //selectedMaterial.Category = materialTransportPicker.selectedBaseMaterial.Category;
                selectedMaterial.ECI_B1B5 = materialLifePicker.Value;
                selectedMaterial.SetProperty("ECI_B1B5_Settings", materialLifePicker.Settings);
            }
            UpdateMaterialSettings();
        }

        private void Btn_C1_C4_Click(object sender, RoutedEventArgs e)
        {
            MaterialEndofLifePicker materialEndofLifePicker = new MaterialEndofLifePicker(txt_C1_C4_Setting.Text, Utils.ConvertMeToDouble(txt_C1_C4.Text));
            materialEndofLifePicker.ShowDialog();
            if (materialEndofLifePicker.isAccepted == true)
            {
                //selectedMaterial.Category = materialTransportPicker.selectedBaseMaterial.Category;
                selectedMaterial.ECI_C1C4 = materialEndofLifePicker.Value;
                selectedMaterial.SetProperty("ECI_C1C4_Settings", materialEndofLifePicker.Settings);
            }
            UpdateMaterialSettings();
        }

        private void Btn_D_Click(object sender, RoutedEventArgs e)
        {
            MaterialAdditionalPicker materialAdditionalPicker = new MaterialAdditionalPicker(txt_D_Setting.Text, Utils.ConvertMeToDouble(txt_D.Text));
            materialAdditionalPicker.ShowDialog();
            if (materialAdditionalPicker.isAccepted == true)
            {
                //selectedMaterial.Category = materialTransportPicker.selectedBaseMaterial.Category;
                selectedMaterial.ECI_D = materialAdditionalPicker.Value;
                selectedMaterial.SetProperty("ECI_D_Settings", materialAdditionalPicker.Settings);
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
            selectedMaterial.ECI_A4 = Utils.ConvertMeToDouble(txt_A4.Text);
            selectedMaterial.ECI_A5 = Utils.ConvertMeToDouble(txt_A5.Text);
            selectedMaterial.ECI_B1B5 = Utils.ConvertMeToDouble(txt_B1_B5.Text);
            selectedMaterial.ECI_C1C4 = Utils.ConvertMeToDouble(txt_C1_C4.Text);
            selectedMaterial.ECI_D = Utils.ConvertMeToDouble(txt_D.Text);

            UpdateMaterialSettings();

            //selectedMaterial.EEI = Utils.ConvertMeToDouble(txt_EEI.Text);

        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            acceptNew = false;
            this.Close();
        }

        private void ValueText_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateMaterialSettings();
        }

        private void Txt_D_KeyDown(object sender, KeyEventArgs e)
        {
            selectedMaterial.ECI_D = Utils.ConvertMeToDouble(txt_D.Text);
            selectedMaterial.SetProperty("ECI_D_Settings", "Manual Override");
            UpdateMaterialSettings();
        }

        private void Btn_New_Click(object sender, RoutedEventArgs e)
        {
            ValueDialogBox vdb = new ValueDialogBox("New Material Name");
            vdb.ShowDialog();
            if (vdb.isAccepted == true)
            {
                CarboMaterial newMaterial = new CarboMaterial();
                newMaterial.Name = vdb.Value;
                returnedDatabase.AddMaterial(newMaterial);

                RefreshMaterialList();
                selectMaterial(vdb.Value);
            }
        }
    }
}
