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
        private double Density;

        internal bool isAccepted;
        //public double Value;
        //public string Settings;
        public List<Vehicle> transportationlist;
        public CarboA4Properties c2Properties;

        public MaterialTransportPicker(CarboA4Properties c2Properties, CarboMaterial materialToBeTransported)
        {
            this.c2Properties = c2Properties;
            this.c2Properties.density = materialToBeTransported.Density;
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            transportationlist = LoadElements();

            foreach (Vehicle tm in transportationlist)
            {
                cbb_Type.Items.Add(tm.name);
            }
            loadSettings();
        }

        private void loadSettings()
        {
            cbb_Type.Text = c2Properties.name;

            txt_Capacity.Text = c2Properties.capacity.ToString();
            txt_Construction.Text = c2Properties.construction.ToString();
            txt_MaxDistance.Text = c2Properties.maxDistance.ToString();
            txt_CarboPerkm.Text = c2Properties.emissionPerKm.ToString();
            txt_DistanceToSite.Text = c2Properties.distanceToSite.ToString();

            txt_Calculation.Text = c2Properties.calcResult;
            txt_Value.Text = c2Properties.value.ToString();
        }

        private List<Vehicle> LoadElements()
        {
            List<Vehicle> result = new List<Vehicle>();

            //Find Profilelist;
            string myPath = Utils.getAssemblyPath() + "\\data\\" + "Transport.csv";

            if (File.Exists(myPath))
            {
                DataTable profileTable = Utils.LoadCSV(myPath);
                foreach (DataRow dr in profileTable.Rows)
                {
                    Vehicle newElement = new Vehicle();

                    string name = dr[0].ToString();
                    double carboNew = Utils.ConvertMeToDouble(dr[1].ToString());
                    double maxDistance = Utils.ConvertMeToDouble(dr[2].ToString());
                    double volumePerTransport = Utils.ConvertMeToDouble(dr[3].ToString());
                    double eCPerkm = Utils.ConvertMeToDouble(dr[4].ToString());

                    newElement.name = name;
                    newElement.construction = carboNew;
                    newElement.maxDistance = maxDistance;
                    newElement.capacity = volumePerTransport;
                    newElement.emissionPerKm = eCPerkm;

                    result.Add(newElement);
                }
            }
            else
            {
                MessageBox.Show("File: " + myPath + " could not be found, make sure you have the Eol list located in indicated folder");
            }

            return result;
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            Refresh();

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

            Vehicle selectedValue = transportationlist.First(item => item.name == nameToFind);

            if (selectedValue != null)
            {
                c2Properties.name = cbb_Type.Text;

                c2Properties.capacity = selectedValue.capacity;
                c2Properties.construction = selectedValue.construction;
                c2Properties.maxDistance = selectedValue.maxDistance;
                c2Properties.emissionPerKm = selectedValue.emissionPerKm;

                txt_DistanceToSite.Text = c2Properties.distanceToSite.ToString();
                
                c2Properties.calculate();
                loadSettings();
            }
        }

        private void Refresh()
        {
            //Store data in list.
            StoreData();
            c2Properties.calculate();
            loadSettings();
        }

        private void StoreData()
        {
            //
            c2Properties.name = cbb_Type.Text;

            c2Properties.capacity = Math.Round(Utils.ConvertMeToDouble(txt_Capacity.Text), 3);
            c2Properties.construction = Math.Round(Utils.ConvertMeToDouble(txt_Construction.Text), 3);
            c2Properties.maxDistance = Math.Round(Utils.ConvertMeToDouble(txt_MaxDistance.Text), 3);
            c2Properties.emissionPerKm = Math.Round(Utils.ConvertMeToDouble(txt_CarboPerkm.Text), 3);
            c2Properties.distanceToSite = Math.Round(Utils.ConvertMeToDouble(txt_DistanceToSite.Text),3);

        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            Refresh();
        }

        private void btn_Calculate_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

    }


}
