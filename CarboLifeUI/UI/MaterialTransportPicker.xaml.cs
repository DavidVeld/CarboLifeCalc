using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for MaterialTransportPicker.xaml
    /// </summary>
    public partial class MaterialTransportPicker : Window
    {
        //private double Density;

        internal bool isAccepted;
        //public double Value;
        //public string Settings;
        public List<Vehicle> vehicleList;
        public List<Fuel> fuelList;

        public CarboA4Properties a4Properties;

        private bool noUpdates = true;

        public MaterialTransportPicker(CarboA4Properties c2Properties, CarboMaterial materialToBeTransported)
        {
            this.a4Properties = c2Properties;
            if (c2Properties.materialDensity != materialToBeTransported.Density)
            {
                MessageBoxResult dialogResult = MessageBox.Show("The transportation density does not match the meterial density, would you like to use: " + materialToBeTransported.Density + " kg/m³ as  a transportation density value?", "Warning", MessageBoxButton.YesNo);
                if(dialogResult == MessageBoxResult.Yes)
                    this.a4Properties.materialDensity = materialToBeTransported.Density;
            }
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            vehicleList = LoadVehicles();
            fuelList = LoadFuels();

            foreach (Vehicle tm in vehicleList)
            {
                cbb_Type.Items.Add(tm.name);
            }

            foreach (Fuel fl in fuelList)
            {
                cbb_Fuel.Items.Add(fl.name);
            }

            loadSettings();
        }

        private List<Fuel> LoadFuels()
        {
            List<Fuel> result = new List<Fuel>();

            //Find Profilelist;
            string myPath = Utils.getAssemblyPath() + "\\data\\" + "Fuel.csv";

            if (File.Exists(myPath))
            {
                DataTable table = Utils.LoadCSV(myPath);
                foreach (DataRow dr in table.Rows)
                {
                    /*
                     *         
        public string name { get; set; }
        public double emission { get; set; }
        public double production { get; set; }
        public string unit { get; set; }
                     * */
                    Fuel newFuel = new Fuel();

                    string name = dr[0].ToString();
                    double emission = Utils.ConvertMeToDouble(dr[1].ToString());
                    double production = Utils.ConvertMeToDouble(dr[2].ToString());
                    string unit = dr[3].ToString();

                    newFuel.name = name;
                    newFuel.emission = emission;
                    newFuel.production = production;
                    newFuel.unit = unit;

                    result.Add(newFuel);
                }
            }
            else
            {
                MessageBox.Show("File: " + myPath + " could not be found, make sure you have the Eol list located in indicated folder");
            }

            return result;
        }


        private List<Vehicle> LoadVehicles()
        {
            List<Vehicle> result = new List<Vehicle>();

            //Find Profilelist;
            string myPath = Utils.getAssemblyPath() + "\\data\\" + "Transport.csv";

            if (File.Exists(myPath))
            {
                DataTable profileTable = Utils.LoadCSV(myPath);
                foreach (DataRow dr in profileTable.Rows)
                {
                    /*
                     *         
        public string name { get; set; }
        public double maxVolume { get; set; }
        public double maxLoad { get; set; }
        public double kmPer { get; set; }
        public double carboCostNew { get; set; }
        public double totalDistance { get; set; }
        public string defaultFuel { get; set; }
        public string description { get; set; }
                     * */
                    Vehicle newVehicle = new Vehicle();

                    string name = dr[0].ToString();
                    double maxVolume = Utils.ConvertMeToDouble(dr[1].ToString());
                    double maxLoad = Utils.ConvertMeToDouble(dr[2].ToString());
                    double kmPer = Utils.ConvertMeToDouble(dr[3].ToString());
                    double carboCostNew = Utils.ConvertMeToDouble(dr[4].ToString());
                    double totalDistance = Utils.ConvertMeToDouble(dr[5].ToString());
                    string defaultFuel = dr[6].ToString();
                    string description = dr[8].ToString();

                    newVehicle.name = name;
                    newVehicle.maxVolume = maxVolume;
                    newVehicle.maxLoad = maxLoad;
                    newVehicle.kmPer = kmPer;
                    newVehicle.carboCostNew = carboCostNew;
                    newVehicle.totalDistance = totalDistance;
                    newVehicle.defaultFuel = defaultFuel;
                    newVehicle.description = description;

                    result.Add(newVehicle);
                }
            }
            else
            {
                MessageBox.Show("File: " + myPath + " could not be found, make sure you have the Eol list located in indicated folder");
            }

            return result;
        }


        private void loadSettings()
        {
            noUpdates = true;

            //Load User Settings:
            txt_DistanceToSite.Text = a4Properties.distanceToSite.ToString();
            txt_Density.Text = a4Properties.materialDensity.ToString();
            txt_Empty.Text = a4Properties.emptyRun.ToString();
            chx_Return.IsChecked = a4Properties.emptyReturn;

            //Load Vehicle
            cbb_Type.Text = a4Properties.vehicleSettings.name;

            txt_MaxLoad.Text = a4Properties.vehicleSettings.maxLoad.ToString();
            txt_MaxVolume.Text = a4Properties.vehicleSettings.maxVolume.ToString();
            txt_UnitPerkm.Text = a4Properties.vehicleSettings.kmPer.ToString();
            lbl_unitPerkm.Content = a4Properties.vehicleFuel.unit + "/km";

            txt_Construction.Text = a4Properties.vehicleSettings.carboCostNew.ToString();
            txt_MaxDistance.Text = a4Properties.vehicleSettings.totalDistance.ToString();

            txt_Description.Text = a4Properties.vehicleSettings.description;

            //Load Fuel Settings
            cbb_Fuel.Text = a4Properties.vehicleFuel.name;
            txt_FuelCO2.Text = a4Properties.vehicleFuel.emission.ToString();
            txt_FuelProductionCO2.Text = a4Properties.vehicleFuel.production.ToString();
            lbl_perUnit1.Content = "kgCO₂/" + a4Properties.vehicleFuel.unit;
            lbl_perUnit2.Content = "kgCO₂/" + a4Properties.vehicleFuel.unit;

            txt_Calculation.Text = a4Properties.calcResult;
            txt_Value.Text = a4Properties.value.ToString();

            noUpdates = false;
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

        private void Cbb_Type_DropDownClosed(object sender, EventArgs e)
        {
            string nameToFind = cbb_Type.Text;
            if (noUpdates == false && nameToFind != "")
            {
                try
                {
                    Vehicle selectedValue = vehicleList.First(item => item.name == nameToFind);

                    if (selectedValue != null)
                    {
                        a4Properties.vehicleSettings = selectedValue;

                        Fuel fuel = selectFuel(selectedValue.defaultFuel);
                        if (fuel != null)
                        {
                            a4Properties.vehicleFuel = fuel;
                        }

                        a4Properties.calculate();
                        loadSettings();
                    }
                }
                catch
                { 
                }
            }
        }

        private Fuel selectFuel(string defaultFuel)
        {
            Fuel result = null;

            if (noUpdates == false && defaultFuel != "")
            {
                try
                {
                    Fuel selectedValue = fuelList.First(item => item.name == defaultFuel);

                    if (selectedValue != null)
                    {
                        result = selectedValue;
                        loadSettings();
                    }
                }
                catch
                { }
            }

            return result;
        }

        private void btn_Calculate_Click(object sender, RoutedEventArgs e)
        {
            a4Properties.calculate();
            loadSettings();
        }

        private void btn_EditDescription_Click(object sender, RoutedEventArgs e)
        {
            DescriptionEditor editor = new DescriptionEditor(txt_Description.Text);
            editor.ShowDialog();
            if (editor.isAccepted == true)
            {
                txt_Description.Text = editor.description;
                //UpdateMaterialSettings();
            }
        }

        private async void txt_DistanceToSite_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.distanceToSite = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }

        private async void txt_Empty_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.emptyRun = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }

        private async void txt_Density_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.materialDensity = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }

        private void chx_Return_Click(object sender, RoutedEventArgs e)
        {
            a4Properties.emptyReturn = chx_Return.IsChecked.Value;
            a4Properties.calculate();
            loadSettings();
        }

        private void cbb_Fuel_DropDownClosed(object sender, EventArgs e)
        {
            string nameToFind = cbb_Fuel.Text;
            if (noUpdates == false && nameToFind != "")
            {
                try
                {
                    Fuel fuel = selectFuel(nameToFind);
                    if (fuel != null)
                    {
                        a4Properties.vehicleFuel = fuel;
                    }
                    a4Properties.calculate();
                    loadSettings();
                }
                catch
                { }
            }
        }

        private async void txt_MaxVolume_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.vehicleSettings.maxVolume = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }

        private async void txt_UnitPerkm_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.vehicleSettings.kmPer = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }

        private async void txt_MaxDistance_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.vehicleSettings.totalDistance = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }

        private async void txt_Construction_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.vehicleSettings.carboCostNew = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }

        private async void txt_FuelCO2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.vehicleFuel.emission = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }

        private async void txt_FuelProductionCO2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.vehicleFuel.production = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }

        private async void txt_MaxLoad_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                a4Properties.vehicleSettings.maxLoad = Utils.ConvertMeToDouble(tb.Text);
                a4Properties.calculate();
                loadSettings();
            }
        }
    }


}
