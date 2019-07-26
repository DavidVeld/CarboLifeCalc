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
            BaseMaterials.DeSerializeXML("BaseMaterials");

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

                    CarboDatabase newFloorOptionCategory = tryParseData(dt);

                    //dgv_Data.ItemsSource = dt.DefaultView;
                    dgv_Data.ItemsSource = newFloorOptionCategory.getData();
                    newFloorOptionCategory.SerializeXML(name);
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

            int newIdNr = 10000;
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
                newMaterial.Id = newIdNr;
                //columncount = 0;
                bool valueisParameterOrMaterial = false; ;

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
                                double parsedValue = 0;
                                bool isNumber = false;
                                isNumber = double.TryParse(value, out parsedValue);

                                if (isNumber)
                                {
                                    if (property.PropertyType == typeof(bool))
                                    {
                                        bool boolValue = Convert.ToBoolean(Convert.ToInt32(value));
                                        property.SetValue(newMaterial, boolValue);
                                    }
                                    else
                                    {
                                        parsedValue = Math.Round(parsedValue, 2);
                                        property.SetValue(newMaterial, parsedValue);
                                    }



                                }
                                else
                                {
                                    //is string
                                    property.SetValue(newMaterial, value);
                                }

                                //Value Successfully added
                                valueisParameterOrMaterial = false;

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
                            valueisParameterOrMaterial = true;
                        }
                    }

                    if (valueisParameterOrMaterial == true)
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
                resultList.Add(newMaterial);
                //lbl_Status.Text = newOption.Name;

                //lbl_Status.Refresh();
                //this.Refresh();
                //Next Id Nr:
                newIdNr++;
            }
            //result.Floortype = -1;
            //result.FloorTypeName = txt_TypeName.Text;
            //result.FloorTypeDescription = txt_Description.Text;
            result.setData(resultList);
            return result;
        }

    }
}
