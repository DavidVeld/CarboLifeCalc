using CarboLifeAPI;
using CarboLifeAPI.Data.Superseded;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
//using System.Xml.Serialization.Configuration;

namespace CarboLifeAPI.Data
{
    //[Serializable]
    [XmlType(Namespace = "")]

    public class CarboGroupSettings
    {
        public string CategoryName { get; set; }
        public string CategoryParamName { get; set; }

        public string ExistingPhaseName { get; set; }
        public string VolumeConversionFactor { get; set; }

        public bool IncludeSubStructure { get; set; }
        public string SubStructureParamType { get; set; }
        public string SubStructureParamName { get; set; }

        public bool IncludeDemo { get; set; }
        public bool IncludeExisting { get; set; }
        public bool CombineExistingAndDemo { get; set; }

        //Additional parameter
        public bool IncludeAdditionalParameter { get; set; }
        public string AdditionalParameter { get; set; }
        public string AdditionalParameterElementType { get; set; }

        //Grade parameter
        public bool IncludeGradeParameter { get; set; }
        public string GradeParameterName { get; set; }
        public string GradeParameterType { get; set; }

        //Correction parameter
        public bool IncludeCorrectionParameter { get; set; }
        public string CorrectionParameterName { get; set; }
        public string CorrectionParameterType { get; set; }

        //RC parameter
        public bool mapReinforcement { get; set; }
        public string RCParameterName { get; set; }
        public string RCParameterType { get; set; }
        public string RCMaterialName { get; set; }
        public List<CarboNumProperty> rcQuantityMap { get; set; }
        public string RCMaterialCategory { get; set; }
        public bool UseImportedMap { get; set; }

        //Steel connection allowance
        public bool mapSteelConnections { get; set; }
        public string SteelConnectionMaterialName { get; set; }
        public string SteelMaterialCategory { get; set; }

        /// <summary>
        /// The connection allowance as a percentage of the steel volume in a matching group.
        /// </summary>
        public double SteelConnectionPercentage { get; set; }

        //Timber connection allowance
        public bool mapTimberConnections { get; set; }
        public string TimberConnectionMaterialName { get; set; }
        public string TimberMaterialCategory { get; set; }

        /// <summary>
        /// The connection allowance as a percentage of the timber volume in a matching group.
        /// </summary>
        public double TimberConnectionPercentage { get; set; }

        public double UncertaintyFactor { get; set; }

        public CarboGroupSettings()
        {
            CategoryName = "(Revit) Category";
            CategoryParamName = "";

            SubStructureParamName = "IsSubstructure";
            SubStructureParamType = "Parameter (Instance Boolean)";
            ExistingPhaseName = "Existing";

            IncludeSubStructure = false;
            IncludeDemo = false;
            IncludeExisting = false;
            CombineExistingAndDemo = false;
            VolumeConversionFactor = "";

            IncludeAdditionalParameter = false;
            AdditionalParameter = "";
            AdditionalParameterElementType = "";

            IncludeGradeParameter = false;
            GradeParameterName = "";
            GradeParameterType = "";

            IncludeCorrectionParameter = false;
            CorrectionParameterName = "";
            CorrectionParameterType = "";

            mapReinforcement = true;
            RCParameterName = "";
            RCParameterType = "";
            RCMaterialName = "Reinforcement";
            RCMaterialCategory = "";

            mapSteelConnections = false;
            SteelConnectionMaterialName = "";
            SteelMaterialCategory = "";
            SteelConnectionPercentage = 5;

            mapTimberConnections = false;
            TimberConnectionMaterialName = "";
            TimberMaterialCategory = "";
            TimberConnectionPercentage = 0.15;

            UseImportedMap = true;

            UncertaintyFactor = 0.10;

            rcQuantityMap = new List<CarboNumProperty>();
            
        }

        public CarboGroupSettings DeSerializeXML()
        {
            //All error handeling exists within getSettingsFilePath:
            string importSettingsPath  = PathUtils.getSettingsFilePath();

            if (File.Exists(importSettingsPath))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(CarboGroupSettings));
                    CarboGroupSettings buffer;

                    using (FileStream fs = new FileStream(importSettingsPath, FileMode.Open))
                    {
                        buffer = ser.Deserialize(fs) as CarboGroupSettings;
                    }
                    return buffer;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return new CarboGroupSettings();
                }
            }
            else
            {
                return new CarboGroupSettings();
            }
        }

        private List<CarboNumProperty> getCurrentRCMap()
        {
            List<CarboNumProperty> result = new List<CarboNumProperty>();

            //Find Profilelist;
            string myPath = Utils.getAssemblyPath() + "\\data\\" + "ReinforcementCats.csv";

            if (File.Exists(myPath))
            {
                DataTable table = Utils.LoadCSV(myPath);
                foreach (DataRow dr in table.Rows)
                {
                    CarboNumProperty property = new CarboNumProperty();

                    string category = dr[0].ToString();
                    double value = Utils.ConvertMeToDouble(dr[1].ToString());

                    property.PropertyName = category;
                    property.Value = value;


                    result.Add(property);
                }
            }
            else
            {
                MessageBox.Show("File: " + myPath + " could not be found, make sure you have the Eol list located in indicated folder");
            }

            return result;

        }

        public bool SerializeXML(string path = "")
        {
            string importSettingsPath = path;

            if (path == "")
            {
                importSettingsPath = PathUtils.getSettingsFilePath();
            }
            else
            {                 
                importSettingsPath = path;
            }

            bool result = false;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboGroupSettings));

                using (FileStream fs = new FileStream(importSettingsPath, FileMode.Create))
                {
                    ser.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return false;
            }

            return result;
        }

        public void ReloadRCMap()
        {
            rcQuantityMap = getCurrentRCMap();
        }

        /// <summary>
        /// Opens a SaveFileDialog to let the user choose a destination, 
        /// then copies the source file to that location.
        /// </summary>
        /// <param name="sourceFilePath">The full path to the existing XML file.</param>
        public void ExportSettingsFile(string sourceFilePath)
        {
            // 1. Verify source exists before bothering the user
            if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                MessageBox.Show($"Source file not found:\n{sourceFilePath}",
                                "Export Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            // 2. Configure the Save Dialog (WPF Version)
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Title = "Export Revit Import Settings";
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.AddExtension = true;

            // Optional: Default to the current filename
            saveFileDialog.FileName = Path.GetFileName(sourceFilePath);

            // 3. Show the dialog
            // In WPF, ShowDialog returns bool? (nullable boolean)
            if (saveFileDialog.ShowDialog() == true)
            {
                string destFilePath = saveFileDialog.FileName;

                try
                {
                    // 4. Perform the copy
                    // The 'true' parameter allows overwriting if the user selected an existing file
                    File.Copy(sourceFilePath, destFilePath, true);

                    MessageBox.Show("Settings exported successfully.",
                                    "Export Complete",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (IOException ioEx)
                {
                    MessageBox.Show($"File access error: {ioEx.Message}",
                                    "Export Failed",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred: {ex.Message}",
                                    "Export Failed",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Opens an OpenFileDialog to let the user select an existing XML file.
        /// </summary>
        /// <returns>The selected file path if successful; otherwise, null.</returns>
        public string ImportSettingsFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Open Revit Import Settings";
            openFileDialog.Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog.DefaultExt = "xml";
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

    }
}
