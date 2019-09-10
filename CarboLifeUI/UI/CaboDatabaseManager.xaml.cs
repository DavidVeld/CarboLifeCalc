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
        private CarboDatabase UserMaterials;
        private CarboDatabase BaseMaterials;

        public CaboDatabaseManager(CarboDatabase userMaterials)
        {
            UserMaterials = userMaterials;
            BaseMaterials = new CarboDatabase();
            BaseMaterials = BaseMaterials.DeSerializeXML("db\\BaseMaterials");

            InitializeComponent();
        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            //Get the profile from a cvs:
            if (cbb_ViewableTable.Text == "User Materials")
                name = "UserMaterials";
            else if (cbb_ViewableTable.Text == "Base Materials")
                name = "BaseMaterials";
            
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
                    else if(name == "UserMaterials")
                        UserMaterials = newMaterialDatabase;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbb_ViewableTable.Items.Add("User Materials");
            cbb_ViewableTable.Items.Add("Base Materials");
            cbb_ViewableTable.Text = cbb_ViewableTable.Items[0].ToString();
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
                            //The column is a propery value, thus it need to be added as such
                            try
                            {
                                string value = dr[dc].ToString();
                                double parsedDoubleValue = 0;
                                int parcedIntValue;
                                bool isDouble = false;
                                bool isInt = false;
                                isDouble = double.TryParse(value, out parsedDoubleValue);
                                isInt = int.TryParse(value, out parcedIntValue);                           

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

                                //Value Successfully added
                                valueisParameter = false;

                                //removethePropertyFromlistToSpeedThingsUpNextRound;
                                propertyList.Remove(property);

                                break;
                            }
                            catch (Exception ex)
                            {
                                messageString += ex.Message;
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

            if (cbb_ViewableTable.Text == "Base Materials")
            {
                dgv_Data.ItemsSource = null;
                dgv_Data.Items.Clear();
                if(BaseMaterials != null)
                    dgv_Data.ItemsSource = BaseMaterials.getData();
            }
            else
            {
                dgv_Data.ItemsSource = null;
                dgv_Data.Items.Clear();
                if(UserMaterials != null)
                    dgv_Data.ItemsSource = UserMaterials.getData();
            }
        }
    }
}
