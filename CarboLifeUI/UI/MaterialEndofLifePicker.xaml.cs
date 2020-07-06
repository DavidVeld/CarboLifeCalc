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
    /// Interaction logic for MaterialEndofLifePicker.xaml
    /// </summary>
    public partial class MaterialEndofLifePicker : Window
    {
        internal CarboMaterial carboMaterial;
        internal CarboC1C4Properties eolProperties;

        internal bool isAccepted;
        //public double Value;
        //public string Settings;

        List<EoLElement> materialList;

        public MaterialEndofLifePicker(CarboMaterial material)
        {
            carboMaterial = material;
            eolProperties = material.materialC1C4Properties;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Load Interface:

            materialList = LoadElements();
            //CarboLifeAPI.Utils.LoadCSV("");

            foreach (EoLElement eolE in materialList)
            {
                cbb_Type.Items.Add(eolE.Material);

            }
            cbb_Type.Items.Add("Manual");

            //get presettings
            loadSettings();
        }

        private List<EoLElement> LoadElements()
        {
            List<EoLElement> result = new List<EoLElement>();

            //Find Profilelist;
            string myPath = Utils.getAssemblyPath() + "\\data\\" + "Eol.csv";

            double a1a3Value = carboMaterial.ECI_A1A3;
            if (a1a3Value < 0)
                a1a3Value = a1a3Value * -1;

            if (File.Exists(myPath))
            {
                DataTable profileTable = Utils.LoadCSV(myPath);
                foreach (DataRow dr in profileTable.Rows)
                {
                    EoLElement newElement = new EoLElement();

                    string material = dr[0].ToString();
                    double inc = Utils.ConvertMeToDouble(dr[1].ToString());
                    double incP = Utils.ConvertMeToDouble(dr[2].ToString());
                    double landf = Utils.ConvertMeToDouble(dr[3].ToString());
                    double landfP = Utils.ConvertMeToDouble(dr[4].ToString());

                    newElement.Material = material;

                    newElement.Incineration = a1a3Value;
                    newElement.IncinerationP = incP;
                    newElement.Landfill = landf;
                    newElement.LandfillP = landfP;

                    result.Add(newElement);
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
            eolProperties.calculate();

            txt_C1Thickness.Text = eolProperties.c1density.ToString();
            txt_C1BaseValue.Text = eolProperties.c1BaseValue.ToString();
            txt_C1Value.Text = eolProperties.c1Value.ToString();

            txt_C2Name.Text = eolProperties.c2Properties.name;
            txt_C2Value.Text = eolProperties.c2Value.ToString();

            txt_C3Value.Text = eolProperties.c3Value.ToString();

            //cbb_Type.Text = eolProperties.c4DisposalName.ToString();

            txt_landfillP.Text = eolProperties.c4landfP.ToString();
            txt_landfillValue.Text = eolProperties.c4landfV.ToString();
            txt_incineratedP.Text = eolProperties.c4incfP.ToString(); ;
            txt_incineratedValue.Text = eolProperties.c4incfV.ToString();
            txt_reusedP.Text = eolProperties.c4reUseP.ToString();
            txt_reusedP.Text = eolProperties.c4reUseV.ToString();

            txt_additional.Text = eolProperties.other.ToString();

            txt_Calculation.Text = eolProperties.calcResult;

            //Totals;
            txt_Value.Text = eolProperties.value.ToString();
        }
               
        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            StoreData();
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

            EoLElement selectedMaterial = materialList.Find(x => x.Material.Contains(cbb_Type.Text));

            if (selectedMaterial != null)
            {
                cbb_Type.Text = selectedMaterial.Material;

                eolProperties.c4DisposalName = selectedMaterial.Material;
                eolProperties.c4incfP = Math.Round(selectedMaterial.IncinerationP, 3);
                eolProperties.c4incfV = Math.Round(selectedMaterial.Incineration, 3);
                eolProperties.c4landfP = Math.Round(selectedMaterial.LandfillP, 3);
                eolProperties.c4landfV = Math.Round(selectedMaterial.Landfill, 3);

                eolProperties.calculate();
                loadSettings();
            }

        }

        private void Refresh()
        {
            //Store data in list.
            StoreData();
            eolProperties.calculate();
            loadSettings();
        }

        private void StoreData()
        {
            //C1
            eolProperties.propertyName = cbb_Type.Text;

            eolProperties.c1density = Math.Round(Utils.ConvertMeToDouble(txt_C1Thickness.Text), 3);
            eolProperties.c1BaseValue = Math.Round(Utils.ConvertMeToDouble(txt_C1BaseValue.Text), 3);
            eolProperties.c1Value = Math.Round(Utils.ConvertMeToDouble(txt_C1Value.Text), 3);
            //C2 - C3
            eolProperties.c2Value = Math.Round(Utils.ConvertMeToDouble(txt_C2Value.Text), 3);
            eolProperties.c3Value = Math.Round(Utils.ConvertMeToDouble(txt_C3Value.Text), 3);
            //C4
            eolProperties.c4DisposalName = cbb_Type.Text;
            eolProperties.c4landfP = Math.Round(Utils.ConvertMeToDouble(txt_landfillP.Text), 3);
            eolProperties.c4landfV = Math.Round(Utils.ConvertMeToDouble(txt_landfillValue.Text), 3);
            eolProperties.c4incfP = Math.Round(Utils.ConvertMeToDouble(txt_incineratedP.Text), 3);
            eolProperties.c4incfV = Math.Round(Utils.ConvertMeToDouble(txt_incineratedValue.Text), 3);
            eolProperties.c4reUseP = Math.Round(Utils.ConvertMeToDouble(txt_reusedP.Text), 3);
            eolProperties.c4reUseV = Math.Round(Utils.ConvertMeToDouble(txt_reusedValue.Text), 3);

            //Other
            eolProperties.other = Math.Round(Utils.ConvertMeToDouble(txt_additional.Text), 3);

        }


        private void txt_landfillP_TextChanged(object sender, TextChangedEventArgs e)
        {
            double value = 0;

            if (double.TryParse(txt_landfillP.Text, out value))
                Refresh();
        }

        private void txt_incineratedP_TextChanged(object sender, TextChangedEventArgs e)
        {
            double value = 0;

            if (double.TryParse(txt_incineratedP.Text, out value))
                Refresh();
        }


        private void btn_C2Pick_Click(object sender, RoutedEventArgs e)
        {
            MaterialTransportPicker transportSelector = new MaterialTransportPicker(eolProperties.c2Properties, carboMaterial);
            transportSelector.ShowDialog();

            if (transportSelector.isAccepted == true)
            {
                eolProperties.c2Properties = transportSelector.c2Properties;
                eolProperties.c2Value = transportSelector.c2Properties.value;

                txt_C2Name.Text = eolProperties.c2Properties.name;
                txt_C2Value.Text = eolProperties.c2Value.ToString();

            }

            Refresh();
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            Refresh();
        }

        private void btn_Calculate_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private async void txt_landfillValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                Refresh();
            }
        }
    }

}
