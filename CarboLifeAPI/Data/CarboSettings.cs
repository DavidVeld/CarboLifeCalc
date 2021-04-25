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
    public class CarboSettings
    {
        public string DataBasePath { get; set; }
        public string UserPath { get; set; }

        public CarboSettings()
        {
            DataBasePath = "Local";
            UserPath = "Local";
        }

        public CarboSettings DeSerializeXML()
        {
            string fileName = "db\\CarboSettings.xml";
            string myPath = Utils.getAssemblyPath() + "\\" + fileName;

            if (File.Exists(fileName))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(CarboSettings));
                    CarboSettings bufferproject;

                    using (FileStream fs = new FileStream(myPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as CarboSettings;
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
                CarboSettings newsettings = new CarboSettings();
                newsettings.SerializeXML();
                return newsettings;
            }
        }
        public bool SerializeXML()
        {

            string fileName = "db\\CarboSettings.xml";

            string myPath = Utils.getAssemblyPath() + "\\" + fileName;

            bool result = false;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboSettings));

                using (FileStream fs = new FileStream(myPath, FileMode.Create))
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
