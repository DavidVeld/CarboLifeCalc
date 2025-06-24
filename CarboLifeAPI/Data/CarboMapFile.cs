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
    public class CarboMapFile
    {
        public List<CarboMapElement> mappingTable { get; set; }

        public CarboMapFile()
        {
            mappingTable = new List<CarboMapElement>();
        }

        public void SaveToXml()
        {
            string myPath = Utils.getAssemblyPath() + "\\data\\" + "defaultmappingfile.xml";
            bool okGo = true;

            if (File.Exists(myPath))
            {
                okGo = false;

                bool islocked = DataExportUtils.IsFileLocked(myPath);
                if (islocked == false)
                    okGo = true;
            }
            if (okGo == true)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CarboMapFile));
                    using (StreamWriter writer = new StreamWriter(myPath))
                    {
                        serializer.Serialize(writer, this);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Error during serialization: " + ex.Message);
                }
            }

        }

        public static CarboMapFile LoadFromXml()
        {
            string myPath = Utils.getAssemblyPath() + "\\data\\" + "defaultmappingfile.xml";

            if (File.Exists(myPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CarboMapFile));
                    using (StreamReader reader = new StreamReader(myPath))
                    {
                        return (CarboMapFile)serializer.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Error during deserialization: " + ex.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public void Merge(List<CarboMapElement> newMappingTable)
        {
            foreach (var newElement in newMappingTable)
            {
                var existingElement = mappingTable.Find(e =>
                    string.Equals(e.revitName, newElement.revitName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.category, newElement.category, StringComparison.OrdinalIgnoreCase)
                );

                if (existingElement != null)
                {
                    // Update the carboNAME of the matching element
                    existingElement.carboNAME = newElement.carboNAME;
                }
                else
                {
                    // Add new element to the mapping table
                    mappingTable.Add(newElement);
                }
            }
        }
    }
}
