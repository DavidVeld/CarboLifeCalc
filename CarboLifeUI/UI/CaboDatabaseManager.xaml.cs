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
    [Obsolete]
    public partial class CaboDatabaseManager : Window
    {
        public CarboDatabase UserMaterials;
        ///public CarboDatabase BaseMaterials;

        public bool isOk;


        public CaboDatabaseManager(CarboDatabase userMaterials)
        {
            UserMaterials = userMaterials;
            //BaseMaterials = new CarboDatabase();
            //BaseMaterials = BaseMaterials.DeSerializeXML("db\\BaseMaterials");
            isOk = false;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshTable();
        }
        
        [Obsolete]
        private CarboDatabase tryParseData(DataTable dt)
        {
            CarboDatabase result = new CarboDatabase();

            //List<String> fieldNameList = new List<string>();
            List<CarboMaterial> resultList = new List<CarboMaterial>();
            //Reach row is one element
            //Loop through datatable

            //int newIdNr = 10000;
            //int column count = 0;

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
                //column count = 0;
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
                        /*
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
                        */
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

        private void RefreshTable()
        {
            dgv_Data.ItemsSource = null;
            dgv_Data.Items.Clear();
            if (UserMaterials != null)
                dgv_Data.ItemsSource = UserMaterials.getData();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isOk = true;
            this.Close();
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

        private void mnu_LoadTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Do you want to replace the current materials in your project with the ones in the template? User made materials will be removed." + Environment.NewLine +
                    "[TO BE IMPLEMENTED]", "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Utils.WriteToLog(ex.Message);
            }
        }

        /// <summary>
        /// Saves the current project materials as a template
        /// </summary>
        private void mnu_SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Do you want to replace your template file with the current project materials?" + Environment.NewLine + "You will lose materials that exist in the template, but not in this project" + Environment.NewLine
                    , "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    string name = "db\\UserMaterials";

                        if (name != "" && result == MessageBoxResult.Yes)
                        {
                            if (name == "db\\UserMaterials")
                            {
                                UserMaterials.SerializeXML(name);
                                MessageBox.Show("Current Project Materials saved as template", "Warning", MessageBoxButton.OK);
                            }
                            else
                            {
                                MessageBox.Show("Dataset not saved");
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Utils.WriteToLog(ex.Message);
            }
        }

        private void mnu_SyncFromOnline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Load materials from a online database, download a database and sync with it" + Environment.NewLine +
                    "[TO BE IMPLEMENTED]", "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Utils.WriteToLog(ex.Message);
            }
        }

        private void mnu_SyncToTemplate_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            try
            {
                MessageBoxResult result = MessageBox.Show("Do you want to update the material template with the current project materials ?" + Environment.NewLine +
                    "Materials with excact same names will be overwritten with new values", "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "xml files (*.cxml)|*.cxml|All files (*.*)|*.*";

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

        private void mnu_SyncFromTemplate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Mnu_SaveDataAs_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            try
            {
                MessageBoxResult mresult = MessageBox.Show("Do you want to save the current Project Materials in a seperate file for sharing or use in anoter project ?", "Warning", MessageBoxButton.YesNo);

                if (mresult == MessageBoxResult.Yes)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "carboLife Materials (*.clm)|*.cml";

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
                                MessageBox.Show("Project Materials Saved", "Information", MessageBoxButton.OK);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void mnu_OpenDataFrom(object sender, RoutedEventArgs e)
        {
            //string name = "";

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "carboLife Materials (*.cml)|*.cml";

                var path = openFileDialog.ShowDialog();
                FileInfo finfo = new FileInfo(openFileDialog.FileName);
                if (openFileDialog.FileName != "")
                {
                    string filePath = openFileDialog.FileName;
                    CarboDatabase newMaterialDatabase = new CarboDatabase();
                    if (File.Exists(filePath))
                    {
                        newMaterialDatabase = newMaterialDatabase.DeSerializeXML(filePath);
                        if (newMaterialDatabase != null)
                        {
                            UserMaterials = newMaterialDatabase;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}

