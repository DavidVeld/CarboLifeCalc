using CarboLifeAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarboLifeRevit
{
    [Serializable]
    public class CarboRevitImportSettings
    {
        public string MainCategory { get; set; }
        public string SubCategory { get; set; }
        public string CutoffLevel { get; set; }
        public double CutoffLevelValue { get; set; }
        public bool IncludeDemo { get; set; }
        public bool IncludeExisting { get; set; }

        public CarboRevitImportSettings()
        {
            MainCategory = "(Revit) Category";
            SubCategory = "";
            CutoffLevel = "Ground Floor";
            CutoffLevelValue = 0;
            IncludeDemo = false;
            IncludeExisting = false;
        }

        public CarboRevitImportSettings DeSerializeXML()
        {
            string importSettingsPath  = PathUtils.getRevitImportSettingspath();

            if (File.Exists(importSettingsPath))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(CarboRevitImportSettings));
                    CarboRevitImportSettings bufferproject;

                    using (FileStream fs = new FileStream(importSettingsPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as CarboRevitImportSettings;
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
                CarboRevitImportSettings newsettings = new CarboRevitImportSettings();
                newsettings.SerializeXML();
                return newsettings;
            }
        }
        public bool SerializeXML()
        {
            string importSettingsPath = PathUtils.getRevitImportSettingspath();

            bool result = false;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboRevitImportSettings));

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

    }
}
