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
    /// Interaction logic for MaterialLifePicker.xaml
    /// </summary>
    public partial class MaterialLifePicker : Window
    {
        internal bool isAccepted;
        public CarboB1B5Properties materialB1B5Properties;
        public List<LookupItem> componentLifeItemList;

        public MaterialLifePicker(CarboB1B5Properties materialB1B5Properties)
        {
            this.materialB1B5Properties = materialB1B5Properties;
            componentLifeItemList = LoadLookupItems("Use");

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            foreach (LookupItem loi in componentLifeItemList)
            {
                cbb_Type.Items.Add(loi.name);
            }


            RefreshInterface();
            UpdateValue();
        }

        private List<LookupItem> LoadLookupItems(string file)
        {
            List<LookupItem> result = new List<LookupItem>();

            //Find Profilelist;
            string myPath = Utils.getAssemblyPath() + "\\data\\" + file + ".csv";

            if (File.Exists(myPath))
            {
                FileInfo path = new FileInfo(myPath);
                if (IsFileLocked(path) == false)
                {
                    DataTable profileTable = Utils.LoadCSV(myPath);
                    foreach (DataRow dr in profileTable.Rows)
                    {
                        LookupItem newItem = new LookupItem();

                        string name = dr[0].ToString();
                        double value = Utils.ConvertMeToDouble(dr[1].ToString());

                        newItem.name = name;
                        newItem.value = value;

                        result.Add(newItem);
                    }
                }
                else
                {
                    MessageBox.Show("File: " + myPath + " was locked, make sure you don't have it open or you cannot load it's data");
                }
            }
            else
            {
                MessageBox.Show("File: " + myPath + " could not be found, make sure you have the Eol list located in indicated folder");
            }

            return result;
        }
        private static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //The file is open
                return true;
            }

            //All is ok
            return false;
        }


        private void RefreshInterface()
        {
            //B1
            txt_B1Value.Text = materialB1B5Properties.B1.ToString();
            txt_B2Value.Text = materialB1B5Properties.B2.ToString();
            txt_B3Value.Text = materialB1B5Properties.B3.ToString();
            txt_B4Factor.Text = materialB1B5Properties.B4.ToString();
            txt_B5Value.Text = materialB1B5Properties.B5.ToString();
            txt_B6Value.Text = materialB1B5Properties.B6.ToString();
            txt_B7Value.Text = materialB1B5Properties.B7.ToString();

            txt_AssetReferencePeriod.Text = materialB1B5Properties.buildingdesignlife.ToString();
            txt_ComponentLifespan.Text = materialB1B5Properties.elementdesignlife.ToString();

            cbb_Type.Text = materialB1B5Properties.assetType;
        }

        private async void Txt_Life_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                UpdateValue();
            }
        }

        private async void Txt_Value2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;
            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                UpdateValue();
            }
        }

        private void UpdateValue()
        {
            materialB1B5Properties.calculate();

            lbl_Calc.Content = "Total = " + materialB1B5Properties.B4 + " x ( "
                + materialB1B5Properties.B1 + " + "
                + materialB1B5Properties.B2 + " + "
                + materialB1B5Properties.B3 + " + "
                + materialB1B5Properties.B5 + " + "
                + materialB1B5Properties.B6 + " + "
                + materialB1B5Properties.B7 + ") = "
                + Math.Round(materialB1B5Properties.totalValue * materialB1B5Properties.B4,3 ) + " kgCO2/kg ";

            txt_B4Factor.Text = materialB1B5Properties.B4.ToString();
            txt_AssetReferencePeriod.Text = materialB1B5Properties.buildingdesignlife.ToString();
            txt_ComponentLifespan.Text = materialB1B5Properties.elementdesignlife.ToString();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;

            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void txt_B1Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                materialB1B5Properties.B1 = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private async void txt_B2Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                materialB1B5Properties.B2 = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private async void txt_B3Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                materialB1B5Properties.B3 = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private async void txt_ComponentLifespan_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                materialB1B5Properties.elementdesignlife = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private async void txt_AssetReferencePeriod_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                materialB1B5Properties.buildingdesignlife = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private async void txt_B4Factor_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                materialB1B5Properties.B4 = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private async void txt_B5Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                materialB1B5Properties.B5 = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private async void txt_B6Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                materialB1B5Properties.B6 = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private async void txt_B7Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                materialB1B5Properties.B7 = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private void cbb_Type_DropDownClosed(object sender, EventArgs e)
        {
            string nameToFind = cbb_Type.Text;
            if (nameToFind != "")
            {
                try
                {
                    LookupItem selectedValue = componentLifeItemList.First(item => item.name == nameToFind);

                    if (selectedValue != null)
                    {
                        materialB1B5Properties.elementdesignlife = selectedValue.value;
                        materialB1B5Properties.assetType = selectedValue.name;
                    }
                }
                catch (Exception ex)
                {
                    materialB1B5Properties.elementdesignlife = 50;
                    materialB1B5Properties.buildingdesignlife = 50;
                    materialB1B5Properties.assetType = "General";
                }
            }
            UpdateValue();
        }
    }
}
