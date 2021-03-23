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
    public partial class ReinforcementWindow : Window
    {
        public CarboDatabase materials;
        public CarboGroup concreteGroup;
        public CarboGroup reinforcementGroup;
        public double addtionalValue;
        public string additionalDescription;

        public bool isAccepted;
        public bool createNew;
        public ReinforcementWindow(CarboDatabase materialDatabase, CarboGroup myConcreteGroup)
        {
            isAccepted = false;
            createNew = false;
            materials = materialDatabase;
            concreteGroup = myConcreteGroup;
            reinforcementGroup = new CarboGroup();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get a clean material list:
            if (materials.CarboMaterialList != null && materials.CarboMaterialList.Count > 0)
            {
                List<CarboMaterial> sortedMaterialList = materials.CarboMaterialList.OrderBy(o => o.Name).ToList();

                foreach (CarboMaterial cm in sortedMaterialList)
                {
                    cbb_ReinforcementMaterial.Items.Add(cm.Name);
                }

                cbb_ReinforcementMaterial.SelectedItem = cbb_ReinforcementMaterial.Items[0];
                txt_Volume.Text = concreteGroup.Volume.ToString();
                rd_Insert.IsChecked = true;
                refreshInterface();
            }
            else
            {
                MessageBox.Show("No materials found in database, please create some and try again");
                this.Close();
            }
        }


        private void refreshInterface()
        {
            CarboMaterial material = materials.GetExcactMatch(cbb_ReinforcementMaterial.Text);
            double volume = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Volume.Text);
            double density = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Density.Text);



            if (material != null && txt_Volume.Text != "" && txt_Density.Text != "")
            {

                reinforcementGroup = calculateRebar(material, reinforcementGroup, volume, density);

                addtionalValue = calculateMixedMaterial(material, concreteGroup.Density, density);
                additionalDescription = reinforcementGroup.Description;

                txt_VolumeRebar.Text = reinforcementGroup.Volume.ToString();
                txt_WeightRebar.Text = reinforcementGroup.Mass.ToString();
                txt_MixResult.Text = Math.Round(addtionalValue,3).ToString();
            }
            
        }

        private CarboGroup calculateRebar(CarboMaterial material, CarboGroup reinforcementGroup, double volume, double density)
        {
            double steelDensity = material.Density;
            double steelWeight = volume * density;
            double steelVolume = steelWeight / steelDensity;

            reinforcementGroup.Volume = steelVolume;
            reinforcementGroup.Mass = steelWeight;
            reinforcementGroup.SubCategory = "";
            reinforcementGroup.Category = "Reinforcement";
            reinforcementGroup.Material = material;
            reinforcementGroup.MaterialName = material.Name;
            reinforcementGroup.Density =  material.Density;
            reinforcementGroup.ECI =  material.ECI;

            reinforcementGroup.Description = "Reinforcement of: " + volume + "m³ " +  " With: " + density + " kg/m³ Reinforcement";
           
            return reinforcementGroup;
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;

            if(rd_NewGroup.IsChecked.Value == true)
            {
                createNew = true;
            }
            else
            {
                createNew = false;
            }

            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void Txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshInterface();
        }

        private void Cbb_Categories_DropDownClosed(object sender, EventArgs e)
        {
            refreshInterface();
        }

        private double calculateMixedMaterial(CarboMaterial material, double densityBase, double densityToMix)
        {
            double result = 0;
            try
            {
                 result = (densityToMix * material.ECI) / densityBase;
            }
            catch
            {
                return 0;
            }

            return result;
        }

    }
}
