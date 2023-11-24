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
    public partial class GroupAdvancedEditor : Window
    {
        internal bool isAccepted;
        public CarboGroup group;
        public CarboDatabase database;


        public List<LookupItem> wasteItemList;
        public List<LookupItem> componentLifeItemList;

        public GroupAdvancedEditor(CarboGroup carboGroup, CarboDatabase dataBase)
        {
            this.group = carboGroup;
            this.database = dataBase; 

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateValue();

            //Load Waste Table
            wasteItemList = LoadLookupItems("Waste");
            componentLifeItemList = LoadLookupItems("Use");

            foreach(LookupItem loi in wasteItemList)
            {
                cbb_WasteItem.Items.Add(loi.name);
            }

            cbb_WasteItem.Text = group.WasteDescription;

        }

        private void UpdateValue()
        {

            ///Formula
            txt_Formula.Text = group.Correction;
            txt_FormulaDescription.Text = group.CorrectionDescription;
            ///Waste
            txt_WasteFactor.Text = group.Waste.ToString();
            cbb_Type.Text = group.WasteDescription;
            //Additional
            txt_AdditionalValue.Text = group.Additional.ToString();
            txt_AdditionalDescription.Text = group.AdditionalDescription.ToString();
            ///B4


            cbb_WasteItem.Text = group.WasteDescription;
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
                    MessageBox.Show("File: " + myPath + " was locked, make sure you don't have it open or you cannot load it's data.");
                }
            }
            else
            {
                MessageBox.Show("File: " + myPath + " could not be found, make sure you have the Eol list located in indicated folder.");
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

        /// <summary>
        /// Formula
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void txt_Formula_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                group.Correction = tb.Text;
                UpdateValue();

            }
        }
        private async void txt_FormulaDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                group.CorrectionDescription = tb.Text;
                UpdateValue();

            }
        }
        /// <summary>
        /// Waste
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbb_WasteItem_DropDownClosed(object sender, EventArgs e)
        {
            string nameToFind = cbb_WasteItem.Text;
            if (nameToFind != "")
            {
                try
                {
                    LookupItem selectedValue = wasteItemList.First(item => item.name == nameToFind);

                    if (selectedValue != null)
                    {
                        group.Waste = selectedValue.value;
                        group.WasteDescription = selectedValue.name;
                    }
                }
                catch
                {
                    group.Waste = 0;
                    group.WasteDescription = "";
                }
            }
            UpdateValue();
        }

        private async void txt_WasteFactor_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                group.Waste = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
        }

        /// <summary>
        /// B4
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void txt_B4Factor_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                group.B4Factor = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();

            }
            */
        }

        private async void txt_AssetReferencePeriod_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                group.AssetLifePeriod = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
            */
        }

        private async void txt_ComponentLifespan_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                group.ComponentLifePeriod = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();
            }
            */
        }

        /// <summary>
        /// Additional Value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void txt_AdditionalValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                group.Additional = Utils.ConvertMeToDouble(tb.Text);
                UpdateValue();

            }
        }

        private async void txt_AdditionalDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(500);
            if (startLength == tb.Text.Length)
            {
                group.AdditionalDescription = tb.Text;
            }
            UpdateValue();
        }

        private void btn_EditAdditional_Click(object sender, RoutedEventArgs e)
        {
            //MaterialAddMix mixWindow = new MaterialAddMix(database, group.Density);
            ReinforcementWindow reinforementWindow = new ReinforcementWindow(database, group);
            reinforementWindow.rd_Insert.Visibility = Visibility.Hidden;
            reinforementWindow.rd_NewGroup.Visibility = Visibility.Hidden;

            reinforementWindow.ShowDialog();


            if (reinforementWindow.isAccepted == true)
            {
                group.Additional = reinforementWindow.addtionalValue;
                group.AdditionalDescription = reinforementWindow.additionalDescription;
            }
            UpdateValue();
        }

        private void cbb_Type_DropDownClosed(object sender, EventArgs e)
        {
            /*
            string nameToFind = cbb_Type.Text;
            if (nameToFind != "")
            {
                try
                {
                    LookupItem selectedValue = componentLifeItemList.First(item => item.name == nameToFind);

                    if (selectedValue != null)
                    {
                        group.ComponentLifePeriod = selectedValue.value;
                        group.B4Description = selectedValue.name;
                    }
                }
                catch
                {
                    group.ComponentLifePeriod = 0;
                    group.B4Description = "";
                }
            }
            UpdateValue();
            */
        }
    }



}
