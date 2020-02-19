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

        /// <summary>
        /// Excact Match
        /// </summary>
        /// <param name="materialName"></param>
        /// <returns></returns>
        public CarboMaterial GetExcactMatch(string materialName)
        {
            CarboMaterial result = null;

            foreach (CarboMaterial cm in this.CarboMaterialList)
            {
                // string materialname = cm.Name;
                if (cm.Name == materialName)
                {
                    result = cm;
                    break;
                }
            }

            return result;
        }
        /// <summary>
        /// Approx Match
        /// </summary>
        /// <param name="materialToLookup"></param>
        /// <returns></returns>
        public CarboMaterial getClosestMatch(CarboMaterial materialToLookup)
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
        public CarboMaterial getClosestMatch(string materialToLookup)
        {
            CarboMaterial result = new CarboMaterial();
            int basedist = 100;

            foreach (CarboMaterial cm in this.CarboMaterialList)
            {
                // string materialname = cm.Name;
                int dist = Utils.CalcLevenshteinDistance(materialToLookup, cm.Name);

                if (dist < basedist)
                {
                    basedist = dist;
                    result = cm;
                }
            }

            return result;
        }

        public List<string> getCategoryList()
        {
            List<string> result = new List<string>();

            foreach (CarboMaterial cm in CarboMaterialList)
            {
                bool uniqueCategory = true;

                foreach (string mc in result)
                {
                    if (mc == cm.Category)
                    {
                        uniqueCategory = false;
                    }
                }
                if (uniqueCategory == true)
                {
                    result.Add(cm.Category);
                }
            }
            result.Sort();
            return result;

        }

        /// <summary>
        /// Serialises a materialDatabase
        /// </summary>
        /// <param name="fileName">Current Options are: "db\\UserMaterial" and "db\\BaseMaterial"</param>
        public void SerializeXML(string fileName)
        {
            string myPath = fileName;

            //if its a relative path use:

            if (!(File.Exists(myPath)))
            {
                if (fileName == "")
                    fileName = "db\\UserMaterials.xml";
                else
                    fileName = fileName + ".xml";

            myPath = Utils.getAssemblyPath() + "\\" + fileName;

            }


            XmlSerializer ser = new XmlSerializer(typeof(CarboDatabase));

            using (FileStream fs = new FileStream(myPath, FileMode.Create))
            {
                ser.Serialize(fs, this);
            }

            ///MessageBox.Show("Database saved to: " + myPath);

        }
        /// <summary>
        /// De-Serialises a material Database No file extension. 
        /// Current Options are: "db\\UserMaterials" and "db\\BaseMaterials" use "" for UserMaterial
        /// </summary>
        /// <param name="fileName"></param>
        public CarboDatabase DeSerializeXML(string fileName)
        {
            string myPath = fileName;

            //if its a relative path use:
            if (!(File.Exists(myPath)))
                {
                if (fileName == "")
                    fileName = "db\\UserMaterials.xml";
                else
                    fileName = fileName + ".xml";

            myPath = Utils.getAssemblyPath() + "\\" + fileName;
            }
            //Reatemp

            if (File.Exists(myPath))
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboDatabase));
                CarboDatabase bufferproject;

                try
                {
                    using (FileStream fs = new FileStream(myPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as CarboDatabase;

                    }

                return bufferproject;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            else
            {
                MessageBox.Show("file: " + myPath + " cannot be found");
            }

            return null;

        }

        public void Update(CarboDatabase cdb)
        {
            List<CarboMaterial> newMaterials = new List<CarboMaterial>();

            foreach (CarboMaterial cmNew in cdb.CarboMaterialList)
            {
                bool isfound = false;

                foreach (CarboMaterial cmCurrent in this.CarboMaterialList)
                {
                    if (cmCurrent.Name == cmNew.Name)
                    {
                        cmCurrent.Copy(cmNew);
                        isfound = true;
                        break;
                    }
                }
                if (isfound == false)
                {
                    //This is a new material add it to the list;
                    newMaterials.Add(cmNew);
                }

            }
            if(newMaterials.Count > 0)
            {
                foreach(CarboMaterial cm in newMaterials)
                {
                    this.CarboMaterialList.Add(cm);
                }
            }
        }

        public bool AddMaterial(CarboMaterial newMaterial)
        {
            bool result = false;
            bool exists = false;
            int id = getUniqueId();
            newMaterial.Id = id;

            foreach (CarboMaterial cm in CarboMaterialList)
            {
                if(cm.Name == newMaterial.Name)
                {
                    exists = true;
                    break;
                }
            }

            if (exists == false)
            {
                CarboMaterialList.Add(newMaterial);
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }

        private int getUniqueId()
        {
            Found:
            Random rnd = new Random();
            int id = rnd.Next(20000, 30000);  // creates a number between 1 and 12

            bool isUnique = isUniqueId(id);

            if (isUnique == false)
                goto Found;
            else
                return id;
        }

        private bool isUniqueId(int id)
        {
            bool result = true;
            foreach (CarboMaterial cm in CarboMaterialList)
            {
                if (cm.Id == id)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
    

        public void deleteMaterial(int id)
        {
            string name = "";
            bool ok = false;

            try
            {
                for (int i = CarboMaterialList.Count -1; i > 0; i--)
                {
                    if (CarboMaterialList[i].Id == id)
                    {
                        name = CarboMaterialList[i].Name;
                        CarboMaterialList.RemoveAt(i);
                        ok = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK);

            }

            if (ok == true)
                MessageBox.Show(name + " deleted.", "Deleted", MessageBoxButton.OK);
        }
    }
}
