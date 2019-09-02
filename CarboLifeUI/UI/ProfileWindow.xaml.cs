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
    public partial class ProfileWindow : Window
    {
        public CarboDatabase materials;
        public CarboGroup concreteGroup;
        public CarboGroup profileGroup;

        public bool isAccepted;

        public ProfileWindow(CarboDatabase materialDatabase, CarboGroup myConcreteGroup)
        {
            isAccepted = false;
            materials = materialDatabase;
            concreteGroup = myConcreteGroup;
            profileGroup = new CarboGroup();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<Profile> profiles = new List<Profile>();
            //CarboLifeAPI.Utils.LoadCSV("");

            foreach (CarboMaterial cm in materials.CarboMaterialList)
            {
                if (cm.Category == "Steel" ||
                    cm.Name.Contains("metaldeck") ||
                    cm.Name.Contains("comflor"))
                {
                    cbb_ProfileMaterial.Items.Add(cm.Name);
                }
            }
            if(cbb_ProfileMaterial.Items.Count <= 0)
            {
                MessageBox.Show("No rebar or steel materials found in database, please create one and try again");
            }
            cbb_ProfileMaterial.SelectedItem = cbb_ProfileMaterial.Items[0];
            txt_Volume.Text = concreteGroup.Volume.ToString();

            refreshInterface();
        }

        private void refreshInterface()
        {
            CarboMaterial material = materials.LookupMaterial(cbb_ProfileMaterial.Text);
            double volume = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Volume.Text);
            double thickness = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Thickness.Text);

            if (material != null && txt_Volume.Text != "" && thickness != 0)
            {
                lbl_Area.Content = volume / (thickness/1000) + " m²";
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

            reinforcementGroup.Description = "Reinforcement of: " + volume + "m³ " +  "/ With: " + density + " kg/m³ Reinforcement";

            return reinforcementGroup;
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

    }

    internal class Profile
    {
    }
}
