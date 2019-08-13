using CarboLifeAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarboLifeUI
{
    [Serializable]
    public class CarboGroupSettings
    {
        public bool groupCategory { get; set; }
        public bool groupSubCategory { get; set; }
        public bool groupType { get; set; }
        public bool groupMaterial { get; set; }
        public bool groupSubStructure { get; set; }
        public bool groupDemolition { get; set; }
        public bool groupuniqueTypeNames { get; set; }

        public string uniqueTypeNames { get; set; }
        public CarboGroupSettings()
        {
            groupCategory = true;
            groupSubCategory = false;
            groupType = false;
            groupMaterial = true;
            groupSubStructure = true;
            groupDemolition = false;
            groupuniqueTypeNames = false;
            uniqueTypeNames = "";
        }

        public CarboGroupSettings DeSerializeXML()
        {
            string fileName = "GroupSettings.xml";
            string myPath = Utils.getAssemblyPath() + "\\" + fileName;

            if (File.Exists(fileName))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(CarboGroupSettings));
                    CarboGroupSettings bufferproject;

                    using (FileStream fs = new FileStream(myPath, FileMode.Open))
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

            string fileName = "GroupSettings.xml";

            string myPath = Utils.getAssemblyPath() + "\\" + fileName;

            bool result = false;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboGroupSettings));

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
