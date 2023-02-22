using CarboLifeAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboGroupSettings
    {
        public string CategoryName { get; set; }
        public string SubStructureParamName { get; set; }
        public string ExistingPhaseName { get; set; }
        public bool IncludeSubStructure { get; set; }
        public bool IncludeDemo { get; set; }
        public bool IncludeExisting { get; set; }

        public CarboGroupSettings()
        {
            CategoryName = "(Revit) Category";
            SubStructureParamName = "IsSubstructure";

            ExistingPhaseName = "Existing";

            IncludeSubStructure = false;
            IncludeDemo = false;
            IncludeExisting = false;
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

    }
}
