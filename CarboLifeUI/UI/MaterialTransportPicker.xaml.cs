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
    /// Interaction logic for MaterialTransportPicker.xaml
    /// </summary>
    public partial class MaterialTransportPicker : Window
    {
        private double Density;

        internal bool isAccepted;
        public double Value;
        public string Settings;
        public TransportationList transportationlist;
        private Vehicle selectedTransmeans;

        public MaterialTransportPicker()
        {
            InitializeComponent();
            transportationlist = new TransportationList();
        }

        public MaterialTransportPicker(string settings, double value, double density)
        {
            this.Settings = settings;
            this.Value = value;
            this.Density = density;
            transportationlist = new TransportationList();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(Vehicle tm in transportationlist)
            {
                cbb_Type.Items.Add(tm.Name);
            }
            txt_Value.Text = Value.ToString();
            string[] settingSplit = Settings.Split(',');

            if (settingSplit.Length > 0)
            {
                cbb_Type.SelectedItem = settingSplit[0];

                Vehicle selectedValue = transportationlist.First(item => item.Name == cbb_Type.Text);

                selectedTransmeans = selectedValue;
            }
            if (settingSplit.Length > 1)
                txt_PerTransport.Text = settingSplit[1];
            if (settingSplit.Length > 2)
                txt_NewTransport.Text = settingSplit[2];
            if (settingSplit.Length > 3)
                txt_Range.Text = settingSplit[3];
            if (settingSplit.Length > 4)
                txt_CarboPerkm.Text = settingSplit[4];
            if (settingSplit.Length > 5)
                txt_TotalDistance.Text = settingSplit[5];
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            Value = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Value.Text);
            Settings = cbb_Type.Text + "," + txt_PerTransport.Text + "," + txt_NewTransport.Text + "," + txt_Range.Text + "," + txt_CarboPerkm.Text + "," + txt_TotalDistance.Text;
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cbb_Type_DropDownClosed(object sender, EventArgs e)
        {
            string nameToFind = cbb_Type.Text;

            Vehicle selectedValue = transportationlist.First(item => item.Name == nameToFind);

            selectedTransmeans = selectedValue;

            UpdateList();
        }

        private void UpdateList()
        {
            //Store data in list.
            

            txt_CarboPerkm.Text = selectedTransmeans.ECPerkm.ToString();
            txt_NewTransport.Text = selectedTransmeans.CarboNew.ToString();
            txt_PerTransport.Text = selectedTransmeans.VolumePerTransport.ToString();
            txt_Range.Text = selectedTransmeans.MaxDistance.ToString();

            Calculate();

        }

        private void Calculate()
        {
            //Calculate total nr of trips;
            string calcResult = "";
            double carboperkm = CarboLifeAPI.Utils.ConvertMeToDouble(txt_CarboPerkm.Text);
            double costofnewvehiclem = CarboLifeAPI.Utils.ConvertMeToDouble(txt_NewTransport.Text);
            double volumePerTransport = CarboLifeAPI.Utils.ConvertMeToDouble(txt_PerTransport.Text);
            double totalDistPervehicle = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Range.Text);
            double tripDistance = CarboLifeAPI.Utils.ConvertMeToDouble(txt_TotalDistance.Text);
            try
            {


                double conversion = 1;
                string units = " km";

                double costPerTrip = Math.Round(tripDistance * carboperkm);

                double costFromNewVehicle = costofnewvehiclem * (tripDistance / totalDistPervehicle);

                double costTotalPerTrip = costFromNewVehicle + costPerTrip;

                double massPerTransport = Math.Round(volumePerTransport * Density);

                double co2prtkg = Math.Round(((1 / massPerTransport) * costTotalPerTrip), 5);

                calcResult += "This calculation will try to create a CO2 per kg value based on the given parameters." + System.Environment.NewLine;
                calcResult += "One trip costs: " + tripDistance + units + " x " + carboperkm + "kgCo2/" + units + "= " + costPerTrip + " kgCo2" + System.Environment.NewLine;
                calcResult += System.Environment.NewLine;
                calcResult += "This will use: " + costFromNewVehicle + "kgCO2 from a new vehicle" + System.Environment.NewLine;
                calcResult += "New vehicle = " + costofnewvehiclem + "kgCO2 x (" + tripDistance + units + " / " + totalDistPervehicle + units + ") = " + costFromNewVehicle + "kgCO2" + System.Environment.NewLine;
                calcResult += System.Environment.NewLine;
                calcResult += "Total CEI per trip then is " + costFromNewVehicle + " kgCO2 + " + costPerTrip + " kgCO2 = " + costTotalPerTrip + " kgCO2" + System.Environment.NewLine;

                calcResult += "One " + selectedTransmeans.Name + " can carry " + volumePerTransport + " m³ per trip" + System.Environment.NewLine;
                calcResult += "This weighs: " + Density + " kg/m³ x " + volumePerTransport + " m³ = " + massPerTransport + "kg/transport" + System.Environment.NewLine; ;
                calcResult += System.Environment.NewLine;

                calcResult += "So per kg this will cost : (1 /" + massPerTransport + ") x " + costTotalPerTrip + " = " + co2prtkg + " CO2/kg" + System.Environment.NewLine; ;


                txt_Calculation.Text = calcResult;
                txt_Value.Text = co2prtkg.ToString();
            }
            catch(Exception ex)
            { }
        }

        private void Txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            Calculate();
        }
    }


}
