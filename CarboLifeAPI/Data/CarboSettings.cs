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

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboSettings
    {
        public string templatePath { get; set; }
        public bool useLocal { get; set; }
        public bool firstLaunch { get; set; }
        public int defaultDesignLife { get; set; }
        public string Currency { get; set; }
        public string ecRevitParameter { get; set; }
        public string secretMessage { get; set; }

        public string carboLegendName { get; set; }
        public string carboDashboardName { get; set; }


        public List<CarboColourPreset> colourPresets {get; set;}

        public CarboGroupSettings defaultCarboGroupSettings;
        public CarboSettings()
        {
            templatePath = "Local";
            useLocal = true;
            firstLaunch = true;
            defaultDesignLife = 60;
            Currency = "£";
            colourPresets = new List<CarboColourPreset>();
            ecRevitParameter = "CLC_EmbodiedCarbon";
            carboLegendName = "CLC_ColourLegend";
            carboLegendName = "CLC_ResultsView";
            secretMessage = "";

            defaultCarboGroupSettings = new CarboGroupSettings();

            if (colourPresets.Count>0)
            {
                CarboColourPreset preset = new CarboColourPreset();
                colourPresets.Add(preset);
            }
        }

        public CarboSettings Load()
        {
            return DeSerializeXML();
        }
        public bool Save()
        {
            return SerializeXML();
        }
        private CarboSettings DeSerializeXML()
        {

            string mySettingsPath = PathUtils.getSettingsFilePath();

            if (File.Exists(mySettingsPath))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(CarboSettings));
                    CarboSettings bufferproject;

                    using (FileStream fs = new FileStream(mySettingsPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as CarboSettings;
                    }

                    if (bufferproject.defaultCarboGroupSettings.rcQuantityMap.Count == 0)
                    {
                        bufferproject.defaultCarboGroupSettings.rcQuantityMap = getCurrentRCMap();
                    }

                    //If the settings exists and all is well use this:
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
                CarboSettings newsettings = new CarboSettings();
                newsettings.SerializeXML();
                return newsettings;
            }
        }
        private bool SerializeXML()
        {

            string mySettingsPath = PathUtils.getSettingsFilePath();

            //string myPath = Utils.getAssemblyPath() + "\\" + fileName;

            bool result = false;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboSettings));

                using (FileStream fs = new FileStream(mySettingsPath, FileMode.Create))
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
    }
}
