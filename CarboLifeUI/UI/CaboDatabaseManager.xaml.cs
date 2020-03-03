using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
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
using CarboLifeAPI;
using CarboLifeAPI.Data;
using Microsoft.Win32;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for CaboDatabaseManager.xaml
    /// </summary>
    public partial class CaboDatabaseManager : Window
    {
        public CarboDatabase UserMaterials;
        public CarboDatabase BaseMaterials;

        public bool isOk;


        public CaboDatabaseManager(CarboDatabase userMaterials)
        {
            UserMaterials = userMaterials;
            BaseMaterials = new CarboDatabase();
            BaseMaterials = BaseMaterials.DeSerializeXML("db\\BaseMaterials");
            isOk = false;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbb_ViewableTable.Items.Add("User Materials");
            cbb_ViewableTable.Items.Add("Base Materials");
            cbb_ViewableTable.Text = cbb_ViewableTable.Items[0].ToString();
            cbb_ViewableTable.SelectedIndex = 0;
            RefreshTable();

        }

        private CarboDatabase tryParseData(DataTable dt)
        {
            CarboDatabase result = new CarboDatabase();

            //List<String> fieldNameList = new List<string>();
            List<CarboMaterial> resultList = new List<CarboMaterial>();
            //Reach row is one element
            //Loop through datatab;e

            //int newIdNr = 10000;
            //int columncount = 0;

            PropertyInfo[] propertyValues = typeof(CarboMaterial).GetProperties();
            List<PropertyInfo> fullPropertyList = propertyValues.ToList();
            int propertyValuesCount = propertyValues.Length;

            foreach (DataRow dr in dt.Rows)
            {
                CarboMaterial newMaterial = new CarboMaterial();
                List<PropertyInfo> propertyList = new List<PropertyInfo>();
                foreach (PropertyInfo prInf in fullPropertyList)
                {
                    propertyList.Add(prInf);
                }
                //Set Id;
                //newMaterial.Id = newIdNr;
                //columncount = 0;
                bool valueisParameter = false; ;

                foreach (DataColumn dc in dt.Columns)
                {

                    string ccolumnName = dc.ColumnName.ToString().Trim();
                    string messageString = "";

                    for (int i = 0; i < propertyList.Count; i++)
                    {
                        PropertyInfo property = propertyList[i];
                        string propertyName = property.Name.ToString();

                        if (propertyName == ccolumnName)
                        {
                            string value = dr[dc].ToString();

                            //The column is a propery value, thus it need to be added as such
                            try
                            {

                                //Improved:

                                if (property.PropertyType == typeof(bool))
                                {
                                    int parcedIntValue;
                                    bool isInt = false;

                                    isInt = int.TryParse(value, out parcedIntValue);

                                    if (isInt == false)
                                        parcedIntValue = 0;

                                    bool boolValue = Convert.ToBoolean(Convert.ToInt32(parcedIntValue));
                                    property.SetValue(newMaterial, boolValue);
                                }
                                else if (property.PropertyType == typeof(int))
                                {
                                    int parcedIntValue;
                                    bool isInt = false;

                                    isInt = int.TryParse(value, out parcedIntValue);

                                    if (isInt == false)
                                        parcedIntValue = 0;

                                    //must be int
                                    property.SetValue(newMaterial, parcedIntValue);
                                }
                                else if (property.PropertyType == typeof(double))
                                {
                                    double parsedDoubleValue = 0;
                                    bool isDouble = false;

                                    isDouble = double.TryParse(value, out parsedDoubleValue);

                                    if (isDouble == false)
                                        parsedDoubleValue = 0;

                                    property.SetValue(newMaterial, parsedDoubleValue);

                                }
                                else if (property.PropertyType == typeof(string))
                                {
                                    if (value == null)
                                        value = "";

                                    property.SetValue(newMaterial, value);
                                }
                                else
                                {
                                    //Skip
                                }

                                //OLD
                                /*
                                if (isDouble == true)
                                {
                                    //Handle as number or bolean

                                    if (property.PropertyType == typeof(bool))
                                    {
                                        bool boolValue = Convert.ToBoolean(Convert.ToInt32(value));
                                        property.SetValue(newMaterial, boolValue);
                                    }
                                    else
                                    {
                                        if (property.PropertyType == typeof(int))
                                        {
                                            //must be int
                                            property.SetValue(newMaterial, parcedIntValue);
                                        }
                                        else
                                        {
                                            //must be double
                                            //parsedDoubleValue = Math.Round(parsedDoubleValue, 2);
                                            property.SetValue(newMaterial, parsedDoubleValue);
                                        }
                                            //see if is int or double
                                    }
                                }
                                else
                                {
                                    //is string
                                    property.SetValue(newMaterial, value);
                                }
                                */


                                //Value Successfully added
                                valueisParameter = false;

                                //removethePropertyFromlistToSpeedThingsUpNextRound;
                                propertyList.Remove(property);

                                //End the loop and go to next column
                                break;
                            }
                            catch (Exception ex)
                            {
                                messageString += value + " : " + ex.Message;
                            }
                        }
                        else
                        {
                            valueisParameter = true;
                        }
                    }

                    if (valueisParameter == true)
                    {
                        // If its not a property it will be added as a free parameter.
                        try
                        {

                            CarboProperty newProperty = new CarboProperty();

                            string propertyValue = dr[dc].ToString();
                            string propertyName = ccolumnName;

                            newProperty.PropertyName = propertyName;
                            newProperty.Value = propertyValue;

                            if (ccolumnName.StartsWith("_"))
                            {
                                //This is a material
                                newProperty.PropertyName = propertyName.TrimStart('_');
                                newMaterial.Properties.Add(newProperty);
                            }
                            else
                            {
                                //This is a property
                                newMaterial.Properties.Add(newProperty);
                            }
                        }
                        catch (Exception ex)
                        {
                            messageString += ex.Message;
                        }
                    }

                    //columncount++;
                }
                newMaterial.CalculateTotals();
                resultList.Add(newMaterial);
                //lbl_Status.Text = newOption.Name;

                //lbl_Status.Refresh();
                //this.Refresh();
                //Next Id Nr:
                //newIdNr++;
            }
            //result.Floortype = -1;
            //result.FloorTypeName = txt_TypeName.Text;
            //result.FloorTypeDescription = txt_Description.Text;
            result.setData(resultList);
            return result;
        }

        private void Cbb_ViewableTable_DropDownClosed(object sender, EventArgs e)
        {
            RefreshTable();
        }

        private void RefreshTable()
        {
            if (cbb_ViewableTable.Text == "Base Materials")
            {
                dgv_Data.ItemsSource = null;
                dgv_Data.Items.Clear();
                if (BaseMaterials != null)
                    dgv_Data.ItemsSource = BaseMaterials.getData();
            }
            else
            {
                dgv_Data.ItemsSource = null;
                dgv_Data.Items.Clear();
                if (UserMaterials != null)
                    dgv_Data.ItemsSource = UserMaterials.getData();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isOk = true;
            this.Close();
        }

        private void Mnu_ImportNew_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            //Get the profile from a cvs:
            if (cbb_ViewableTable.Text == "User Materials")
                name = "db\\UserMaterials";
            else if (cbb_ViewableTable.Text == "Base Materials")
                name = "db\\BaseMaterials";

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "cvs files (*.csv)|*.csv|All files (*.*)|*.*";

                var path = openFileDialog.ShowDialog();
                FileInfo finfo = new FileInfo(openFileDialog.FileName);
                if (openFileDialog.FileName != "")
                {
                    string filePath = openFileDialog.FileName;

                    DataTable dt = CarboLifeAPI.Utils.LoadCSV(filePath);

                    CarboDatabase newMaterialDatabase = tryParseData(dt);

                    //dgv_Data.ItemsSource = dt.DefaultView;
                    dgv_Data.ItemsSource = newMaterialDatabase.getData();
                    newMaterialDatabase.SerializeXML(name);

                    if (name == "BaseMaterials")
                        BaseMaterials = newMaterialDatabase;
                    else if (name == "UserMaterials")
                        UserMaterials = newMaterialDatabase;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Mnu_SaveData_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            //Get the profile from a cvs:
            try
            {
                if (cbb_ViewableTable.Text == "User Materials")
                    name = "db\\UserMaterials";
                else if (cbb_ViewableTable.Text == "Base Materials")
                    name = "db\\BaseMaterials";
                else
                {
                    name = "";
                }

                MessageBoxResult result = MessageBox.Show("This will overwite the current default materials, do you want to proceed?", "Warning", MessageBoxButton.YesNo);

                if (name != "" && result == MessageBoxResult.Yes)
                {
                    if (name == "db\\UserMaterials")
                    {
                        UserMaterials.SerializeXML(name);
                        MessageBox.Show("UserMaterials Saved");

                    }
                    else if (name == "db\\BaseMaterials")
                    {
                        BaseMaterials.SerializeXML(name);
                    }
                }
                else
                {
                    MessageBox.Show("Dataset not saved");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void Mnu_EXportToCVS(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("To Be Implemented");
        }

        private void Mnu_UpdateUserMaterials(object sender, RoutedEventArgs e)
        {
            string name = "";
            //Get the profile from a cvs:
            try
            {
                MessageBoxResult result = MessageBox.Show("Do you want to update the base set of usermaterials based on these project settings ?" + Environment.NewLine +
                    "Materials with excact same names will be overwritten", "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";

                    var path = openFileDialog.ShowDialog();
                    FileInfo finfo = new FileInfo(openFileDialog.FileName);
                    if (openFileDialog.FileName != "")
                    {
                        name = openFileDialog.FileName;

                        CarboDatabase buffer = UserMaterials.DeSerializeXML(name);
                        
                        bool syncResult = UserMaterials.SyncMaterials(buffer);

                        if (syncResult == true)
                        {
                            buffer.SerializeXML(name);
                            Utils.WriteToLog("Database saved to: " + name);
                            MessageBox.Show("Dataset saved");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void Mnu_SaveDataAs_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "xml files (*.xml)|*.xml";

                var path = saveFileDialog.ShowDialog();
                FileInfo finfo = new FileInfo(saveFileDialog.FileName);
                if (saveFileDialog.FileName != "")
                {
                    if (File.Exists(name))
                    {

                        MessageBoxResult result = MessageBox.Show("Do you want to override the file?", "Warning", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            UserMaterials.SerializeXML(name);
                            MessageBox.Show("User Database Saved", "Information", MessageBoxButton.OK);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void mnu_ImportUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Do you want to update the PROJECT MATERIALS based on the DEFAULT USER MATERIALS?" + Environment.NewLine +
                    "Materials with IDENTICAL names will be OVERWRITTEN, NON-EXISTING  materials will be ADDED", "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    string name = "db\\UserMaterials";
                    CarboDatabase buffer = UserMaterials.DeSerializeXML(name);

                    bool syncResult = UserMaterials.SyncMaterials(buffer);

                    if (syncResult == true)
                    {
                        buffer.SerializeXML(name);
                        Utils.WriteToLog("Database saved to: " + name);
                        MessageBox.Show("Dataset saved");
                    }
                }
                else
                {
                    MessageBox.Show("Dataset not saved");
                    Utils.WriteToLog("Database not saved");

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Utils.WriteToLog(ex.Message);

            }
            RefreshTable();

        }

        private void mnu_ExportUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Do you want to update the DEFAULT USER MATERIALS based on these PROJECT settings ?" + Environment.NewLine +
                    "Materials with IDENTICAL names will be OVERWRITTEN, NON-EXISTING  materials will be ADDED", "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    string name = "db\\UserMaterials";
                    CarboDatabase buffer = UserMaterials.DeSerializeXML(name);
                    bool syncResult = buffer.SyncMaterials(UserMaterials);

                    if (syncResult == true)
                    {
                        buffer.SerializeXML(name);
                        Utils.WriteToLog("Database saved to: " + name);

                    }
                    MessageBox.Show("Dataset saved");
                }

                else
                {
                    MessageBox.Show("Dataset not saved");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Utils.WriteToLog(ex.Message);

            }
            RefreshTable();
        }

        private void mnu_OpenDataas_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

