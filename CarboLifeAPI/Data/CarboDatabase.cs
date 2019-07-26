using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace CarboLifeAPI.Data
{
    [Serializable]
    [XmlRoot("CarboDatabase")]
    public class CarboDatabase
    {
        [XmlArray("CarboMaterials"), XmlArrayItem(typeof(CarboMaterial), ElementName = "CarboMaterial")]
        public List<CarboMaterial> CarboMaterialList { get;  set; }
        public List<CarboMaterial> getData()
        {
            return CarboMaterialList;
        }

        public void setData(List<CarboMaterial> data)
        {
            CarboMaterialList = data;
        }

        public CarboDatabase()
        {
            //DeSerialiseThedataBase
            CarboMaterialList = new List<CarboMaterial>();
        }

        internal CarboMaterial getClosestMatch(CarboMaterial materialToLookup)
        {
            CarboMaterial result = new CarboMaterial();
            int basedist = 100;

            foreach ( CarboMaterial cm in this.CarboMaterialList)
            {
               // string materialname = cm.Name;
                int dist = Utils.CalcLevenshteinDistance(materialToLookup.Name, cm.Name);

                if (dist < basedist)
                {
                    basedist = dist;
                    result = cm;
                }
            }

            return result;


        }
        /// <summary>
        /// Serialises a materialDatabase
        /// </summary>
        /// <param name="fileName">Current Options are: "UserMaterial" and "BaseMaterial"</param>
        public void SerializeXML(string fileName)
        {
            if (fileName == "")
                fileName = "UserMaterials.xml";
            else
                fileName = fileName + ".xml";

            string myPath = Utils.getAssemblyPath() + "\\" + fileName;

            XmlSerializer ser = new XmlSerializer(typeof(CarboDatabase));

            using (FileStream fs = new FileStream(myPath, FileMode.Create))
            {
                ser.Serialize(fs, this);
            }

            ///MessageBox.Show("Database saved to: " + myPath);

        }
        /// <summary>
        /// De-Serialises a material Database No file extension. 
        /// Current Options are: "UserMaterial" and "BaseMaterial" use "" for UserMaterial
        /// </summary>
        /// <param name="fileName"></param>
        public CarboDatabase DeSerializeXML(string fileName)
        {
            if (fileName == "")
                fileName = "UserMaterials.xml";
            else
                fileName = fileName + ".xml";

            //Reatemp
            string myPath = Utils.getAssemblyPath() + "\\" + fileName;

            if (File.Exists(myPath))
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboDatabase));
                CarboDatabase bufferproject;


                using (FileStream fs = new FileStream(myPath, FileMode.Open))
                {
                    bufferproject = ser.Deserialize(fs) as CarboDatabase;

                }


                return bufferproject;
            }
            else
            {
                MessageBox.Show("file: " + myPath + " cannot be found");
            }

            return null;

        }


    }
}
