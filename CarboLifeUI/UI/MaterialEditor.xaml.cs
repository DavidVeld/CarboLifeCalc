using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialEditor.xaml
    /// </summary>
    public partial class MaterialEditor : Window
    {
        public bool acceptNew;

        public CarboMaterial selectedMaterial;
        public CarboDatabase returnedDatabase;

        public MaterialEditor(string selectedMaterialName, CarboDatabase database)
        {
            try
            {
                //originalDatabase = database;
                returnedDatabase = database;
                selectedMaterial = database.GetExcactMatch(selectedMaterialName);
                if (selectedMaterial == null)
                {
                    MessageBox.Show("This material could not be found in the database, the closest match will now be found");
                    selectedMaterial = database.getClosestMatch(selectedMaterialName);
                }

                acceptNew = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            InitializeComponent();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            acceptNew = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadMaterialCategories();


            RefreshMaterialList();
            selectMaterial(selectedMaterial.Name);

            UpdateMaterialSettings();  
        }

        private void ReloadMaterialCategories()
        {
            cbb_Categories.Items.Clear();

            List<string> materialCategories = new List<string>();
            materialCategories = returnedDatabase.getCategoryList();
            materialCategories.Sort();

            foreach (string cat in materialCategories)
            {
                cbb_Categories.Items.Add(cat);
                cbb_Category.Items.Add(cat);
            }

            cbb_Categories.Items.Add("All");
            cbb_Categories.Text = "All";
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
        /// <summary>
        /// Updates the details to the selected material
        /// </summary>
        private void UpdateMaterialSettings()
        {
            //StoreGeneralProperties();
            //StoreEmissionProperties();

            if (selectedMaterial != null)
            {
                selectedMaterial.CalculateTotals();

                txt_Name.Text = selectedMaterial.Name;
                txt_Description.Text = selectedMaterial.Description;
                cbb_Category.Text = selectedMaterial.Category;
                txt_Density.Text = selectedMaterial.Density.ToString();
                txt_ECI.Text = selectedMaterial.ECI.ToString();
                txt_EPDLink.Text = selectedMaterial.EPDurl;
                txt_Waste.Text = selectedMaterial.WasteFactor.ToString();
                txt_Grade.Text = selectedMaterial.Grade;

                chx_A4_Manual.IsChecked = selectedMaterial.ECI_A4_Override;
                chx_A5_Manual.IsChecked = selectedMaterial.ECI_A5_Override;
                chx_C1_C4_Manual.IsChecked = selectedMaterial.ECI_C1C4_Override;
                chx_D_Manual.IsChecked = selectedMaterial.ECI_D_Override;
                chx_Seq_Manual.IsChecked = selectedMaterial.ECI_Seq_Override;

                //Materials
                SetA1A3();
                //Construction
                SetA4();
                //Transport
                SetA5();
                //Transport
                SetB1B5();
                //End of Life
                SetC1C4();
                //End of Life
                SetD();
                //
                SetSeq();
                //Mix
                SetMix();

                string calc = "";

                calc = "A1-A3 + A4 + A5 + C1-C4 + Seq + D + Added = ECI Total" + Environment.NewLine;
                calc += Math.Round(Utils.ConvertMeToDouble(txt_A1_A3.Text), 4) +
                    " + " + Math.Round(Utils.ConvertMeToDouble(txt_A4.Text), 4) +
                    " + " + Math.Round(Utils.ConvertMeToDouble(txt_A5.Text), 4) +
                    " + " + Math.Round(Utils.ConvertMeToDouble(txt_B1_B5.Text), 4) +
                    " + " + Math.Round(Utils.ConvertMeToDouble(txt_C1_C4.Text), 4) +
                    " + " + Math.Round(Utils.ConvertMeToDouble(txt_D.Text), 4) +
                    " + " + Math.Round(Utils.ConvertMeToDouble(txt_Mix.Text), 4) +
                    " = " + Math.Round(Utils.ConvertMeToDouble(txt_ECI.Text), 5);
                Calc.Content = calc;

                //chk_Locked.IsChecked = selectedMaterial.isLocked;

                if (selectedMaterial.isLocked == true)
                {
                    grd_Edit.Visibility = Visibility.Hidden;
                }
                else
                {
                    grd_Edit.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetMix()
        {
            txt_Mix_Setting.Text = selectedMaterial.ECI_Mix_Info;
            txt_Mix_Setting.IsReadOnly = false;
            txt_Mix_Setting.Foreground = Brushes.Black;

            txt_Mix.Text = Math.Round(selectedMaterial.ECI_Mix, 2).ToString();
            txt_Mix.Foreground = Brushes.Black;
            txt_Mix.IsReadOnly = false;
        }

        private void SetA1A3()
        {
            selectedMaterial.materialA1A3Properties.Calculate();

            if (selectedMaterial.ECI_A1A3_Override == true)
            {
                //Manual
                chx_A1_A3_Manual.IsChecked = true;

                txt_A1_A3_Setting.Text = "Manual";
                txt_A1_A3.Text = selectedMaterial.ECI_A1A3.ToString();
                txt_A1_A3.IsReadOnly = false;

                txt_A1_A3.Foreground = Brushes.Black;
            }
            else
            {
                //Calculated
                chx_A1_A3_Manual.IsChecked = false;

                txt_A1_A3_Setting.Text = selectedMaterial.materialA1A3Properties.Name;
                txt_A1_A3.Text = selectedMaterial.ECI_A1A3.ToString();
                txt_A1_A3.IsReadOnly = true;

                txt_A1_A3.Foreground = Brushes.LightGray;

            }
        }

        private void SetA4()
        {
            selectedMaterial.materiaA4Properties.calculate();

            if (selectedMaterial.ECI_A4_Override == true)
            {
                //Manual
                chx_A4_Manual.IsChecked = true;
                txt_A4_Setting.Text = "Manual";
                txt_A4.IsReadOnly = false;
                txt_A4.Foreground = Brushes.Black;
            }
            else
            {
                //Calculated
                chx_A4_Manual.IsChecked = false;
                txt_A4_Setting.Text = selectedMaterial.materiaA4Properties.name;
                txt_A4.IsReadOnly = true;
                txt_A4.Foreground = Brushes.LightGray;
            }
                txt_A4.Text = selectedMaterial.ECI_A4.ToString();
        }

        private void SetA5()
        {
            if (selectedMaterial.ECI_A5_Override == true)
            {
                //Manual
                chx_A5_Manual.IsChecked = true;
                txt_A5_Setting.Text = "Manual";
                txt_A5.IsReadOnly = false;
                txt_A5.Foreground = Brushes.Black;

            }
            else
            {
                //Calculated
                chx_A5_Manual.IsChecked = false;
                txt_A5_Setting.Text = selectedMaterial.materialA5Properties.name;
                txt_A5.IsReadOnly = true;
                txt_A5.Foreground = Brushes.LightGray;
            }

                txt_A5.Text = selectedMaterial.ECI_A5.ToString();

        }

        private void SetB1B5()
        {

            //Calculated

            txt_B1_B5_Setting.Text = "Manual";

            txt_B1_B5_Setting.IsReadOnly = true;

            txt_B1_B5.IsReadOnly = false;

            txt_B1_B5.Text = selectedMaterial.ECI_B1B5.ToString();
        }

        private void SetC1C4()
        {
            if (selectedMaterial.ECI_C1C4_Override == true)
            {
                //Manual
                chx_C1_C4_Manual.IsChecked = true;

                selectedMaterial.ECI_C1C4_Override= true;

                txt_C1_C4_Setting.Text = "Manual";
                txt_C1_C4.IsReadOnly = false;

                txt_C1_C4.Foreground = Brushes.Black;

            }
            else
            {
                //Calculated
                chx_C1_C4_Manual.IsChecked = false;

                txt_C1_C4_Setting.Text = "Calculated";
                txt_C1_C4.IsReadOnly = true;
                txt_C1_C4.Foreground = Brushes.LightGray;

            }

            txt_C1_C4.Text = selectedMaterial.ECI_C1C4.ToString();

        }

        private void SetD()
        {
            if (selectedMaterial.ECI_D_Override == true)
            {
                //Manual
                chx_D_Manual.IsChecked = true;

                txt_D_Setting.Text = "Manual";
                //txt_A5.Text = "";
                txt_D.IsReadOnly = false;
                txt_D.Foreground = Brushes.Black;

            }
            else
            {
                //Calculated
                chx_D_Manual.IsChecked = false;

                txt_D_Setting.Text = "Calculated";
                txt_D.IsReadOnly = true;
                txt_D.Foreground = Brushes.LightGray;

            }

            txt_D.Text = selectedMaterial.ECI_D.ToString();
        }

        private void SetSeq()
        {
            if (selectedMaterial.ECI_Seq_Override == true)
            {
                //Manual
                chx_Seq_Manual.IsChecked = true;

                txt_Seq_Setting.Text = "Manual";
                //txt_A5.Text = "";
                txt_Seq.IsReadOnly = false;
                txt_Seq.Foreground = Brushes.Black;

            }
            else
            {
                //Calculated
                chx_Seq_Manual.IsChecked = false;

                txt_Seq_Setting.Text = "Calculated";
                txt_Seq.IsReadOnly = true;
                txt_Seq.Foreground = Brushes.LightGray;

            }

            txt_Seq.Text = selectedMaterial.ECI_Seq.ToString();
        }


        private void Cbb_Categories_DropDownClosed(object sender, EventArgs e)
        {
            //StoreGeneralProperties();
            RefreshMaterialList();
        }

        private void RefreshMaterialList()
        {
            //selectedMaterial = null;
            string cat = cbb_Categories.Text;
            string searchtext = txt_Search.Text;

            liv_materialList.Items.Clear();

            foreach(CarboMaterial cm in returnedDatabase.CarboMaterialList)
            {
                if(cm.Category == cat || cat == "All" || cat == "")
                {
                    //Search Bar
                    int hit = cm.Name.IndexOf(searchtext, StringComparison.OrdinalIgnoreCase);
                    if (searchtext == "" || hit >= 0)
                    {
                        liv_materialList.Items.Add(cm);
                    }
                }
            }
            if (selectedMaterial != null)
                liv_materialList.SelectedItem = selectedMaterial;

            //Sort list
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(liv_materialList.Items);

            if (view != null)
            {
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Category", System.ComponentModel.ListSortDirection.Ascending));
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));

                /*
                PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
                view.GroupDescriptions.Add(groupDescription);

                liv_materialList.Items.Clear();
                liv_materialList.ItemsSource = null;
                
                liv_materialList.ItemsSource = view;
                */
            }

        }

        private void Chb_ShowICE_Click(object sender, RoutedEventArgs e)
        {
            RefreshMaterialList();
        }

        private void Btn_A1_A3_Click(object sender, RoutedEventArgs e)
        {
            A1A3Element selectedA1A3Values = selectedMaterial.materialA1A3Properties;

            if (selectedA1A3Values == null)
                selectedA1A3Values = new A1A3Element();

            MaterialA1A3Picker materialBase = new MaterialA1A3Picker(selectedA1A3Values);
            materialBase.ShowDialog();

            if (materialBase.isAccepted == true)
            {
                chx_A1_A3_Manual.IsChecked = false;
                selectedMaterial.ECI_A1A3_Override = false;
                selectedMaterial.materialA1A3Properties = materialBase.a1a3ElementSelected;

                selectedMaterial.Category = selectedMaterial.materialA1A3Properties.Category;
                selectedMaterial.ECI_A1A3 = selectedMaterial.materialA1A3Properties.ECI_A1A3;
                
                if (selectedMaterial.materialA1A3Properties.Density != selectedMaterial.Density)
                {
                    MessageBoxResult result = MessageBox.Show("The material density of your selected material does not match the material density of your base material, do you wish to override this with the new value?", "Warning", MessageBoxButton.YesNo);
                    if(result == MessageBoxResult.Yes)
                        selectedMaterial.Density = selectedMaterial.materialA1A3Properties.Density;
                }
            }
            StoreGeneralProperties();
            UpdateMaterialSettings();

        }

        private void Btn_A4_Click(object sender, RoutedEventArgs e)
        {
            MaterialTransportPicker materialTransportPicker = new MaterialTransportPicker(selectedMaterial.materiaA4Properties, selectedMaterial);
            materialTransportPicker.ShowDialog();
            if (materialTransportPicker.isAccepted == true)
            {
                chx_A4_Manual.IsChecked = false;
                selectedMaterial.ECI_A4_Override = false;

                selectedMaterial.materiaA4Properties = materialTransportPicker.a4Properties;
                selectedMaterial.ECI_A4 = selectedMaterial.materiaA4Properties.value;

                //selectedMaterial.SetProperty("ECI_A4_Settings", materialTransportPicker.Settings);
            }
            StoreGeneralProperties();
            UpdateMaterialSettings();
        }

        private void Btn_A5_Click(object sender, RoutedEventArgs e)
        {
            MaterialConstructionPicker materialConstructionPicker = new MaterialConstructionPicker(selectedMaterial.materialA5Properties);
            materialConstructionPicker.ShowDialog();
            if (materialConstructionPicker.isAccepted == true)
            {
                chx_A5_Manual.IsChecked = false;
                selectedMaterial.ECI_A5_Override = false;

                selectedMaterial.materialA5Properties = materialConstructionPicker.materialA5Properties;
                selectedMaterial.ECI_A5 = selectedMaterial.materialA5Properties.value;
                
            }
            StoreGeneralProperties();
            UpdateMaterialSettings();
        }


        private void Btn_C1_C4_Click(object sender, RoutedEventArgs e)
        {
            MaterialEndofLifePicker materialEndofLifePicker = new MaterialEndofLifePicker(selectedMaterial);
            materialEndofLifePicker.ShowDialog();
            if (materialEndofLifePicker.isAccepted == true)
            {
                chx_C1_C4_Manual.IsChecked = false;
                selectedMaterial.ECI_C1C4_Override = false;

                selectedMaterial.materialC1C4Properties = materialEndofLifePicker.eolProperties;
                selectedMaterial.ECI_C1C4 = selectedMaterial.materialC1C4Properties.value;
            }
            StoreGeneralProperties();
            UpdateMaterialSettings();
        }

        private void Btn_D_Click(object sender, RoutedEventArgs e)
        {
            MaterialAdditionalPicker materialAdditionalPicker = new MaterialAdditionalPicker(selectedMaterial.materialDProperties);
            materialAdditionalPicker.ShowDialog();
            if (materialAdditionalPicker.isAccepted == true)
            {
                chx_D_Manual.IsChecked = false;
                selectedMaterial.ECI_D_Override = false;

                selectedMaterial.materialDProperties = materialAdditionalPicker.materialDProperties;
                selectedMaterial.ECI_D = selectedMaterial.materialDProperties.value;
            }
            StoreGeneralProperties();
            UpdateMaterialSettings();
        }


        private void btn_Seq_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                MaterialSequestrationPicker materialSequestrationPicker = new MaterialSequestrationPicker(selectedMaterial.materialSeqProperties);
                materialSequestrationPicker.ShowDialog();
                if (materialSequestrationPicker.isAccepted == true)
                {
                    chx_Seq_Manual.IsChecked = false;
                    selectedMaterial.ECI_Seq_Override = false;

                    selectedMaterial.materialSeqProperties = materialSequestrationPicker.materialSeqProperties;
                    selectedMaterial.ECI_Seq = selectedMaterial.materialSeqProperties.value;
                }
            }
            StoreGeneralProperties();
            UpdateMaterialSettings();
        }

        private void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateMaterialSettings();
        }

        private void Btn_Apply_Click(object sender, RoutedEventArgs e)
        {
            //Strore the general Properties
            StoreGeneralProperties();
            StoreEmissionProperties();
            UpdateMaterialSettings();
            RefreshMaterialList();
            selectMaterial(txt_Name.Text);

        }

        private void StoreGeneralProperties()
        {
            if (selectedMaterial != null)
            {
                selectedMaterial.Name = txt_Name.Text;
                selectedMaterial.Description = txt_Description.Text;
                selectedMaterial.Category = cbb_Category.Text;
                selectedMaterial.Density = Utils.ConvertMeToDouble(txt_Density.Text);
                selectedMaterial.EPDurl = txt_EPDLink.Text;
                selectedMaterial.Grade = txt_Grade.Text;
            }
        }
        private void StoreEmissionProperties()
        {
            if (selectedMaterial != null)
            {
                selectedMaterial.ECI = Utils.ConvertMeToDouble(txt_ECI.Text);
                selectedMaterial.ECI_A1A3 = Utils.ConvertMeToDouble(txt_A1_A3.Text);
                selectedMaterial.ECI_A4 = Utils.ConvertMeToDouble(txt_A4.Text);
                selectedMaterial.ECI_A5 = Utils.ConvertMeToDouble(txt_A5.Text);
                selectedMaterial.ECI_B1B5 = Utils.ConvertMeToDouble(txt_B1_B5.Text);
                selectedMaterial.ECI_C1C4 = Utils.ConvertMeToDouble(txt_C1_C4.Text);
                selectedMaterial.ECI_D = Utils.ConvertMeToDouble(txt_D.Text);
            }
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

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                List<int> idList = new List<int>();

                if (liv_materialList != null)
                {
                    foreach (var current in liv_materialList.SelectedItems)
                    {
                        try
                        {
                            CarboMaterial material = current as CarboMaterial;
                            if (material != null)
                                idList.Add(material.Id);
                        }
                        catch
                        {//continue
                        }
                    }
                }

                if (idList.Count > 0)
                {
                    try
                    {
                        foreach (int i in idList)
                        {
                            returnedDatabase.deleteMaterial(i);
                        }
                        //if (ok == true)
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Error while deleting materials", "Deleted", MessageBoxButton.OK);
                    }

                    MessageBox.Show(idList.Count + " material(s) deleted.", "Deleted", MessageBoxButton.OK);
                }
                RefreshMaterialList();

                liv_materialList.SelectedIndex = 0;
            }
        }

        private void btn_Duplicate_Click(object sender, RoutedEventArgs e)
        {

            if (selectedMaterial != null)
            {
                ValueDialogBox vdb = new ValueDialogBox(selectedMaterial.Name + " Copy");
                //vdb.txt_Value.Text = ;
                vdb.WindowStartupLocation  = WindowStartupLocation.CenterOwner;
                vdb.txt_Value.Focus();
                vdb.ShowDialog();

                if (vdb.isAccepted == true)
                {
                    CarboMaterial newMaterial = DeepCopy<CarboMaterial>(selectedMaterial);
                    newMaterial.Name = vdb.Value;
                    returnedDatabase.AddMaterial(newMaterial);
                                                         
                    RefreshMaterialList();
                    selectMaterial(vdb.Value);
                }
            }
        }

        public static T DeepCopy<T>(T other)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, other);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        private void chx_A1_A3_Manual_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                selectedMaterial.ECI_A1A3_Override = chx_A1_A3_Manual.IsChecked.Value;

                StoreGeneralProperties();
                UpdateMaterialSettings();
            }
        }

        private void chx_A4_Manual_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                selectedMaterial.ECI_A4_Override = chx_A4_Manual.IsChecked.Value;

                StoreGeneralProperties();
                UpdateMaterialSettings();
            }
        }

        private void chx_A5_Manual_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                selectedMaterial.ECI_A5_Override = chx_A5_Manual.IsChecked.Value;

                StoreGeneralProperties();
                UpdateMaterialSettings();
            }
        }


        private void chx_C1_C4_Manual_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                selectedMaterial.ECI_C1C4_Override = chx_C1_C4_Manual.IsChecked.Value;

                StoreGeneralProperties();
                UpdateMaterialSettings();
            }
        }

        private void chx_D_Manual_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                selectedMaterial.ECI_D_Override = chx_D_Manual.IsChecked.Value;

                StoreGeneralProperties();
                UpdateMaterialSettings();
            }
        }

        private void chx_Seq_Manual_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                selectedMaterial.ECI_Seq_Override = chx_Seq_Manual.IsChecked.Value;

                StoreGeneralProperties();
                UpdateMaterialSettings();
            }
        }

        private async void txt_A1_A3_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int caretIndex = txt_A1_A3.CaretIndex;

            await Task.Delay(1000);
            if (selectedMaterial.ECI_A1A3_Override == true)
            {
                selectedMaterial.ECI_A1A3 = Utils.ConvertMeToDouble(txt_A1_A3.Text);

                StoreGeneralProperties();
                UpdateMaterialSettings();
                txt_A1_A3.CaretIndex = txt_A1_A3.Text.Length;
            }
        }

        private async void txt_A4_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
            if (selectedMaterial.ECI_A4_Override == true)
            {
                selectedMaterial.ECI_A4 = Utils.ConvertMeToDouble(txt_A4.Text);

                StoreGeneralProperties();
                UpdateMaterialSettings();
            }
        }

        private async void txt_A5_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);

            if (selectedMaterial.ECI_A5_Override == true)
            {
                selectedMaterial.ECI_A5 = Utils.ConvertMeToDouble(txt_A5.Text);

                StoreGeneralProperties();
                UpdateMaterialSettings();
            }

        }

        private async void txt_B1_B5_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
                    selectedMaterial.ECI_B1B5 = Utils.ConvertMeToDouble(txt_B1_B5.Text);
            StoreGeneralProperties();
            UpdateMaterialSettings();
        }
        private async void txt_C1_C4_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(1000);
            if (selectedMaterial.ECI_C1C4_Override == true)
            {
                selectedMaterial.ECI_C1C4 = Utils.ConvertMeToDouble(txt_C1_C4.Text);
                StoreGeneralProperties();
                UpdateMaterialSettings();
            }
        }

        private async void txt_D_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
                 if (selectedMaterial.ECI_D_Override == true)
                    {
                        selectedMaterial.ECI_D = Utils.ConvertMeToDouble(txt_D.Text);
                StoreGeneralProperties();
                UpdateMaterialSettings();
            }           
           }

        private async void txt_Seq_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);

                if (selectedMaterial.ECI_Seq_Override == true)
                {
                    selectedMaterial.ECI_Seq = Utils.ConvertMeToDouble(txt_Seq.Text);
                StoreGeneralProperties();
                UpdateMaterialSettings();
            }

        }

        private async void txt_Mix_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
                selectedMaterial.ECI_Mix = Utils.ConvertMeToDouble(txt_Mix.Text);
            StoreGeneralProperties();
            UpdateMaterialSettings();
        }

        private void btn_EditDescription_Click(object sender, RoutedEventArgs e)
        {
            DescriptionEditor editor = new DescriptionEditor(txt_Description.Text);
            editor.ShowDialog();
            if(editor.isAccepted == true)
            {
                if (selectedMaterial != null)
                {
                    txt_Description.Text = editor.description;

                    StoreGeneralProperties();
                    UpdateMaterialSettings();
                }
            }
        }

        private async void Txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
            if (startLength == tb.Text.Length)
            {
                RefreshMaterialList();
            }
        }

        private void btn_AddCategory_Click(object sender, RoutedEventArgs e)
        {
            ValueDialogBox valDia = new ValueDialogBox();
            valDia.ShowDialog();
            if(valDia.isAccepted == true)
            {
                string newCat = valDia.Value;
                if (newCat != "")
                {
                    List<string> categories = returnedDatabase.getCategoryList();

                    bool catExistins = categories.Contains(newCat);
                    if(catExistins == false)
                    {
                        cbb_Category.Items.Add(newCat);
                        cbb_Categories.Items.Add(newCat);
                        cbb_Category.Text = newCat;
                    }
                    else
                    {
                        MessageBox.Show("This category allready exists.", "Friendly Warning", MessageBoxButton.OK);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid value", "Friendly Warning", MessageBoxButton.OK);
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = liv_materialList.ActualWidth;
            //liv_materialList.
        }

        private void btn_FromEPD_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                MaterialMyEPD epdForm = new MaterialMyEPD(selectedMaterial.Density, txt_EPDLink.Text);
                epdForm.ShowDialog();
                if (epdForm.isAccepted == true)
                {
                    MessageBoxResult result = MessageBox.Show("Do you want to update values based in EPD input? " + Environment.NewLine + "This will override your current settings", "Warning", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        double A1A3 = epdForm.getA1A3();
                        double A4 = epdForm.kgA4;
                        double A5 = epdForm.kgA5;
                        double B17 = epdForm.kgB17;
                        double C14 = epdForm.getC14();
                        double D = epdForm.kgD;

                        //Override the material properties
                        selectedMaterial.ECI_A1A3_Override = true;
                        selectedMaterial.ECI_A4_Override = true;
                        selectedMaterial.ECI_A5_Override = true;
                        selectedMaterial.ECI_C1C4_Override = true;
                        selectedMaterial.ECI_D_Override = true;

                        selectedMaterial.ECI_A1A3 = A1A3;
                        selectedMaterial.ECI_A4 = A4;
                        selectedMaterial.ECI_A5 = A5;

                        //B1-B7
                        selectedMaterial.ECI_B1B5 = B17;

                        selectedMaterial.ECI_C1C4 = C14;
                        selectedMaterial.ECI_D = D;

                        selectedMaterial.EPDurl = epdForm.txt_EPDLink.Text;

                    }
                }
            }
            StoreGeneralProperties();
            UpdateMaterialSettings();

        }

        private void btn_OpenLink_Click(object sender, RoutedEventArgs e)
        {
            Utils.Openlink(txt_EPDLink.Text);
        }

        private void btn_Sync_Click(object sender, RoutedEventArgs e)
        {
            SyncMaterialsWindow materialnsSyncWindow = new SyncMaterialsWindow(returnedDatabase.Copy());
            materialnsSyncWindow.ShowDialog();
            if (materialnsSyncWindow.isAccepted == true)
                returnedDatabase = materialnsSyncWindow.projectDatabase;

            RefreshMaterialList();

        }

        private void btn_TemplateSync_Click(object sender, RoutedEventArgs e)
        {

            if (selectedMaterial != null)
            {
                StoreGeneralProperties();
                StoreEmissionProperties();
            }

            TemplateSelector templateSelectionDialog = new TemplateSelector();
            templateSelectionDialog.ShowDialog();

            if(templateSelectionDialog.isAccepted == true)
            {
                string templatePath = templateSelectionDialog.selectedTemplateFile;
                List<CarboMaterial> list = liv_materialList.SelectedItems.Cast<CarboMaterial>().ToList();

                if (System.IO.File.Exists(templatePath) && list.Count > 0)
                {


                    try
                    {
                        //Create a buffer database for sync
                        CarboDatabase buffer = new CarboDatabase();

                        foreach (CarboMaterial material in list)
                        {
                            buffer.AddMaterial(material);
                        }

                        //get the template & Sync
                        CarboDatabase db = new CarboDatabase();
                        db = db.DeSerializeXML(templatePath);

                        //sync
                        db.SyncMaterials(buffer);

                        //save template
                        db.SerializeXML(templatePath);

                        MessageBox.Show(selectedMaterial.Name + " added to database.");
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }

        }

        private void btn_Mix_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMaterial != null)
            {
                MaterialAddMix materialmixer = new MaterialAddMix(returnedDatabase, selectedMaterial.Density);
                materialmixer.ShowDialog();
                if (materialmixer.isAccepted == true)
                {
                    selectedMaterial.ECI_Mix = materialmixer.valueToBeMixed;
                    selectedMaterial.ECI_Mix_Info = materialmixer.selectedMaterialDescription;
                }
                StoreGeneralProperties();
                StoreEmissionProperties();
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CarboInfoBox infoBox = new CarboInfoBox("The mix parameter allows you to combine your material's embodied carbon with another. Examples are reinforcement or additives into concrete. Another option would be nails and screws into timber structure.");
            infoBox.ShowDialog();
        }

        private async void txt_Mix_Setting_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            await Task.Delay(1000);
            selectedMaterial.ECI_Mix_Info = tb.Text;

            StoreGeneralProperties();
            StoreEmissionProperties();
        }

        private async void txt_Waste_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
            if (startLength == tb.Text.Length)
            {
                double value = Utils.ConvertMeToDouble(tb.Text);
                if (value < 0)
                    value = 0;
                else if (value > 100)
                    value = 100;

                selectedMaterial.WasteFactor = Math.Round(value,1,MidpointRounding.AwayFromZero);
                
                StoreGeneralProperties();
                StoreEmissionProperties();
                UpdateMaterialSettings();
            }
        }

        private void Btn_B1_B5_Click(object sender, RoutedEventArgs e)
        {
            CarboInfoBox info = new CarboInfoBox("In Use values","This is a generic kgCO2/kg value for material dependent properties, the value should be controlled by Group, not material, default value = 0");
            info.ShowDialog();
        }

        private async void txt_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(1000);
            if (selectedMaterial != null)
            {
                selectedMaterial.Name = txt_Name.Text;
                StoreGeneralProperties();
                //RefreshMaterialList();
            }
        }

        private void cbb_Category_DropDownClosed(object sender, EventArgs e)
        {
            StoreGeneralProperties();
            //RefreshMaterialList();
        }

        private async void txt_Density_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(1000);
            if (selectedMaterial != null)
            {
                selectedMaterial.Density = Utils.ConvertMeToDouble(txt_Density.Text);
                StoreGeneralProperties();
            }
        }

        private async void txt_EPDLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(1000);
            if (selectedMaterial != null)
            {
                selectedMaterial.EPDurl = txt_EPDLink.Text;
                StoreGeneralProperties();
                //RefreshMaterialList();
            }
        }

        private void btn_Sync_Copy_Click(object sender, RoutedEventArgs e)
        {
            CarboDatabase cdb = returnedDatabase;
            string file = "";
            string path = @"C:\Temp\file.csv";

            file = "Id,Name,Description,Category,Density,ECI_A1A3" + Environment.NewLine;

            foreach (CarboMaterial mat in cdb.CarboMaterialList)
            {
                file += mat.Id + "," +
                    DataExportUtils.CVSFormat(mat.Name) + "," +
                    DataExportUtils.CVSFormat(mat.Description) + "," +
                    DataExportUtils.CVSFormat(mat.Category) + "," + 
                    mat.Density + "," + 
                    mat.ECI_A1A3 + Environment.NewLine; ;
            }

            DataExportUtils.WriteCVSFile(file, path);


        }

        /// <summary>
        /// Imports multiple materials based on csv format
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Import_Click(object sender, RoutedEventArgs e)
        {
            MaterialImportDialog matImpDia = new MaterialImportDialog(this.returnedDatabase);
            matImpDia.ShowDialog();

            if(matImpDia.isAccepted == true)
            {
                this.returnedDatabase.SyncCSVMaterials(matImpDia.importedDb, matImpDia.deleteMaterials);
                ReloadMaterialCategories();
            }
            RefreshMaterialList();

        }

        private async void txt_Grade_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(1000);
            if (selectedMaterial != null)
            {
                selectedMaterial.Grade = txt_Grade.Text;

                StoreGeneralProperties();
            }
        }
    }
}
