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
using static CarboLifeAPI.DataExportUtils;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialLifePicker.xaml
    /// </summary>
    public partial class MaterialLifePicker : Window
    {
        internal bool isAccepted;
        public CarboB1B7Properties inUseProperties;
        public List<LookupItem> componentLifeItemList;
        private int desinglife;

        private bool formloaded;
        public MaterialLifePicker(CarboB1B7Properties _inUseProperties, int _desinglife)
        {
            this.inUseProperties = _inUseProperties;
            desinglife = _desinglife;

            componentLifeItemList = LoadLookupItems("Use");
            formloaded = false;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (componentLifeItemList.Count > 0)
            {
                foreach (LookupItem loi in componentLifeItemList)
                {
                    cbb_Type.Items.Add(loi.name);
                }
            }
            else
            {
                cbb_Type.Items.Clear();
            }

            txt_AssetReferencePeriod.Text = desinglife.ToString();


            RefreshInterface();
            
            UpdateValue();
            formloaded = true;
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
            chx_EndOfLife.IsChecked = inUseProperties.designLifeToEnd;

            //B1
            txt_B1Value.Text = inUseProperties.B1.ToString();
            txt_B2Value.Text = inUseProperties.B2.ToString();
            txt_B3Value.Text = inUseProperties.B3.ToString();
            txt_B4Factor.Text = inUseProperties.B4.ToString();
            txt_B5Value.Text = inUseProperties.B5.ToString();
            txt_B6Value.Text = inUseProperties.B6.ToString();
            txt_B7Value.Text = inUseProperties.B7.ToString();

            txt_ComponentLifespan.Text = inUseProperties.elementdesignlife.ToString();

            cbb_Type.Text = inUseProperties.assetType;

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
            if (formloaded == true)
            {
                inUseProperties.calculate(desinglife);

                if (lbl_Calc != null)
                {

                    if (inUseProperties.designLifeToEnd == true) //true equals design life
                    {
                        //Disable Settings
                        cbb_Type.IsEnabled = false;
                        cbb_Type.Text = "";
                       
                        txt_ComponentLifespan.IsReadOnly = true;
                        txt_ComponentLifespan.IsEnabled = false;
                    }
                    else //false user gives value
                    {

                        cbb_Type.IsEnabled = true;
                        txt_ComponentLifespan.IsReadOnly = false;
                        txt_ComponentLifespan.IsEnabled = true;
                    }

                        txt_B4Factor.Text = inUseProperties.B4.ToString();
                        txt_ComponentLifespan.Text = inUseProperties.elementdesignlife.ToString();

                    lbl_Calc.Content = "Total = "
    + inUseProperties.B1 + " + "
    + inUseProperties.B2 + " + "
    + inUseProperties.B3 + " + "
    + inUseProperties.B5 + " + "
    + inUseProperties.B6 + " + "
    + inUseProperties.B7 + " = " +
    Math.Round(inUseProperties.totalECI ,3) + " kgCO₂/kg ( x " + inUseProperties.B4 + " )" ;
                }
            }
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
                inUseProperties.B1 = Utils.ConvertMeToDouble(tb.Text);
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
                inUseProperties.B2 = Utils.ConvertMeToDouble(tb.Text);
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
                inUseProperties.B3 = Utils.ConvertMeToDouble(tb.Text);
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
                inUseProperties.B5 = Utils.ConvertMeToDouble(tb.Text);
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
                inUseProperties.B6 = Utils.ConvertMeToDouble(tb.Text);
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
                inUseProperties.B7 = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }


        private void cbb_Type_DropDownClosed(object sender, EventArgs e)
        {
            if (formloaded == true)
            {
                string nameToFind = cbb_Type.Text;

                if (nameToFind != "")
                {
                    try
                    {
                        LookupItem selectedValue = componentLifeItemList.First(item => item.name == nameToFind);

                        if (selectedValue != null)
                        {
                            inUseProperties.elementdesignlife = selectedValue.value;
                            inUseProperties.assetType = selectedValue.name;
                        }
                    }
                    catch
                    {
                        inUseProperties.elementdesignlife = 50;
                        inUseProperties.assetType = "General";
                    }
                }
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
                inUseProperties.elementdesignlife = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        private void chx_EndOfLife_Changed(object sender, RoutedEventArgs e)
        {
            if (formloaded == true)
            {
                inUseProperties.designLifeToEnd = chx_EndOfLife.IsChecked.Value;
                UpdateValue();
            }
        }
    }
}
