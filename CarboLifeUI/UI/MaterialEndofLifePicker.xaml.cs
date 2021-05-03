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

        private bool noUpdates = true;

        public MaterialEndofLifePicker(CarboMaterial material)
        {
            carboMaterial = material;
            eolProperties = material.materialC1C4Properties;
            noUpdates = true;

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
                    double reuse = Utils.ConvertMeToDouble(dr[5].ToString());
                    double reuseP = Utils.ConvertMeToDouble(dr[6].ToString());

                    newElement.Material = material;

                    newElement.Incineration = a1a3Value;
                    newElement.IncinerationP = incP;
                    newElement.Landfill = landf;
                    newElement.LandfillP = landfP;
                    newElement.reuse = reuse;
                    newElement.reuseP = reuseP;

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
            //no updates should trigger text changes events;
            noUpdates = true;

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
            txt_reusedValue.Text = eolProperties.c4reUseV.ToString();

            double totalP = eolProperties.c4landfP + eolProperties.c4incfP + eolProperties.c4reUseP;
            if (totalP != 100)
            {
                lbl_CheckPercent.Content = "Please review total percentages, delta = " + (100 - totalP).ToString() + " %";
                lbl_CheckPercent.Foreground = Brushes.Red;
            }
            else
            {
                lbl_CheckPercent.Content = "";
                lbl_CheckPercent.Foreground = Brushes.Black;

            }

            txt_additional.Text = eolProperties.other.ToString();

            txt_Calculation.Text = eolProperties.calcResult;

            //Totals;
            txt_Value.Text = eolProperties.value.ToString();
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

            EoLElement selectedMaterial = materialList.Find(x => x.Material.Contains(cbb_Type.Text));

            if (selectedMaterial != null)
            {
                cbb_Type.Text = selectedMaterial.Material;

                eolProperties.c4DisposalName = selectedMaterial.Material;
                eolProperties.c4incfP = Math.Round(selectedMaterial.IncinerationP, 3);
                eolProperties.c4incfV = Math.Round(selectedMaterial.Incineration, 3);
                eolProperties.c4landfP = Math.Round(selectedMaterial.LandfillP, 3);
                eolProperties.c4landfV = Math.Round(selectedMaterial.Landfill, 3);
                eolProperties.c4reUseV = Math.Round(selectedMaterial.reuse, 3);
                eolProperties.c4reUseP = Math.Round(selectedMaterial.reuseP, 3);

                eolProperties.calculate();
                loadSettings();
            }

        }

        private void Refresh()
        {
            //Store data in list.
            //StoreData();
            eolProperties.calculate();
            loadSettings();
        }

        private void btn_C2Pick_Click(object sender, RoutedEventArgs e)
        {
            MaterialTransportPicker transportSelector = new MaterialTransportPicker(eolProperties.c2Properties, carboMaterial);
            transportSelector.ShowDialog();

            if (transportSelector.isAccepted == true)
            {
                eolProperties.c2Properties = transportSelector.a4Properties;
                eolProperties.c2Value = transportSelector.a4Properties.value;
            }

            Refresh();
        }

        private void btn_Calculate_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        
        private void GroupBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CarboInfoBox infoBox = new CarboInfoBox("Only use this value for a demolished material, use section C4 for end of life values of a new material. 3.4 kgCO₂/m2 is recommended by RICS guidelines unless specified data is available");
            infoBox.Show();
        }

        private async void txt_C1Thickness_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                eolProperties.c1density = Utils.ConvertMeToDouble(tb.Text);
                Refresh();
            }
        }

        private async void txt_C1BaseValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                eolProperties.c1BaseValue = Utils.ConvertMeToDouble(tb.Text);
                Refresh();
            }
        }

        private async void txt_C1Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
            }
        }

        private async void txt_C2Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                if (noUpdates == false)
                {
                    eolProperties.c2Properties.name = "Manual";
                    eolProperties.c2Value = Utils.ConvertMeToDouble(tb.Text);
                    Refresh();

                }
            }
        }

        private async void txt_C3Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length && noUpdates == false)
            {
                eolProperties.c3Value = Utils.ConvertMeToDouble(tb.Text);
                Refresh();
            }
        }

        private async void txt_landfillP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length && noUpdates == false)
            {
                double landfP = Utils.ConvertMeToDouble(tb.Text);
                //Check Range:
                if (landfP > 100)
                    landfP = 100;
                else if (landfP < 0)
                    landfP = 0;

                //Balance percentages tbc
                eolProperties.c4landfP = landfP;

                Refresh();
            }
        }

        private async void txt_incineratedP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length && noUpdates == false)
            {
                double incP = Utils.ConvertMeToDouble(tb.Text);
                //Check Range:
                if (incP > 100)
                    incP = 100;
                else if (incP < 0)
                    incP = 0;

                //Balance percentages tbc
                eolProperties.c4incfP = incP;

                Refresh();
            }
        }

       private async void txt_reusedP_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length && noUpdates == false)
            {
                double reusep = Utils.ConvertMeToDouble(tb.Text);
                //Check Range:
                if (reusep > 100)
                    reusep = 100;
                else if (reusep < 0)
                    reusep = 0;

                //Balance percentages tbc
                eolProperties.c4reUseP = reusep;

                Refresh();
            }
        }

        private async void txt_landfillValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length && noUpdates == false)
            {
                double p = Utils.ConvertMeToDouble(tb.Text);
                eolProperties.c4landfV = p;
               // Refresh();
            }
        }

        private async void txt_incineratedValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length && noUpdates == false)
            {
                double p = Utils.ConvertMeToDouble(tb.Text);
                eolProperties.c4incfV = p;
               // Refresh();
            }
        }

        private async void txt_reusedValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length && noUpdates == false)
            {
                double p = Utils.ConvertMeToDouble(tb.Text);
                eolProperties.c4reUseV = p;
                // Refresh();
            }
        }

        private async void txt_additional_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length && noUpdates == false)
            {
                double p = Utils.ConvertMeToDouble(tb.Text);
                eolProperties.other = p;
                Refresh();
            }
        }


    }

}
