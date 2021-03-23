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
    public partial class MaterialAddMix : Window
    {
        public CarboDatabase materials;

        public bool isAccepted;
        private double baseMaterialDensity;

        public double valueToBeMixed { get; internal set; }
        public string selectedMaterialDescription { get; internal set; }

        public MaterialAddMix(CarboDatabase materialDatabase, double basematerialDensity)
        {
            isAccepted = false;
            materials = materialDatabase;
            valueToBeMixed = 0;
            selectedMaterialDescription = "";
            baseMaterialDensity = basematerialDensity;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbb_Unit.Items.Add("kg/m³");
            cbb_Unit.Items.Add("m³/m³");
            cbb_Unit.SelectedIndex = 0;

            txt_DensityBase.Text = baseMaterialDensity.ToString();

            //Get a clean material list:
            List<CarboMaterial> sortedMaterialList = materials.CarboMaterialList.OrderBy(o => o.Name).ToList();

            foreach (CarboMaterial cm in sortedMaterialList)
            {
                cbb_MixMaterial.Items.Add(cm.Name);
            }

            if(cbb_MixMaterial.Items.Count <= 0)
            {
                MessageBox.Show("No materials found in current database");
            }
            if (cbb_MixMaterial.Items.Count > 0)
            {
                cbb_MixMaterial.SelectedItem = cbb_MixMaterial.Items[0];
            }

            refreshInterface();
        }


        private void refreshInterface()
        {
            try
            {
                //Fins material and set density
                CarboMaterial material = materials.GetExcactMatch(cbb_MixMaterial.Text);
                if (material != null && cbb_MixMaterial.Text != "")
                {
                    

                    //Set the variables used later;
                    double densityToMix = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Density.Text);
                    double densityBase = CarboLifeAPI.Utils.ConvertMeToDouble(txt_DensityBase.Text);

                    if (cbb_Unit.Text == "m³/m³" && densityToMix > 1)
                    {
                        txt_Density.Text = "1";
                    }

                    double result = 0;

                    if (material != null && densityBase >= 0)
                    {
                        result = calculateMixedMaterial(material, densityBase, densityToMix, cbb_Unit.Text);

                        txt_MixResult.Text = Math.Round(result, 3).ToString();

                        valueToBeMixed = result;
                        selectedMaterialDescription = material.Name + "(" + densityToMix + " " + cbb_Unit.Text + ")";

                    }
                }
            }
            catch
            {
//
            }
            
        }

        private double calculateMixedMaterial(CarboMaterial material, double densityBase, double densityToMix, string unit)
        {
            double result = 0;
            try
            {
                if (unit == "m³/m³")
                {
                    result = (((densityToMix / 1) * material.ECI) * material.Density ) / densityBase;
                }
                else //kg/m³
                {
                    result = (densityToMix * material.ECI) / densityBase;
                }
            }
            catch(Exception ex)
            {
                selectedMaterialDescription = ex.Message;
            }

            return result;
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

        private async void Txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = txt_Density.Text;

            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                txt_Density.Text= text;
                refreshInterface();
            }


        }

        private void Cbb_Categories_DropDownClosed(object sender, EventArgs e)
        {
            refreshInterface();
        }
    }
}
