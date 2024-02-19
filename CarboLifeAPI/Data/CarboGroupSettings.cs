using CarboLifeAPI;
using CarboLifeAPI.Data.Superseded;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using System.Xml.Serialization.Configuration;

namespace CarboLifeAPI.Data
{
    [Serializable]
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

            rcQuantityMap = new List<CarboNumProperty>();
            
        }

        public CarboGroupSettings DeSerializeXML()
        {
            string importSettingsPath  = PathUtils.getRevitImportSettingspath();

            if (File.Exists(importSettingsPath))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(CarboGroupSettings));
                    CarboGroupSettings bufferproject;

                    using (FileStream fs = new FileStream(importSettingsPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as CarboGroupSettings;
                    }



                    return bufferproject;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return null;
                }
            }
            else
            {
                CarboGroupSettings newsettings = new CarboGroupSettings();
                newsettings.SerializeXML();
                return newsettings;
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

        public bool SerializeXML()
        {
            string importSettingsPath = PathUtils.getRevitImportSettingspath();

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
    }
}
