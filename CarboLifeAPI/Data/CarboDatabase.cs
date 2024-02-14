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
        public CarboMaterial getClosestMatch(CarboMaterial materialToLookup, string grade = "")
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
        public CarboMaterial getClosestMatch(string materialToLookup, string grade = "")
        {
            int score = 0;
            int highscore = -50;
            int basescore = materialToLookup.Length;

            int gradebase = grade.Length;

            CarboMaterial result = new CarboMaterial();
            string[] wordsInMaterialToLookup = materialToLookup.Split(' ');


            foreach (CarboMaterial cm in this.CarboMaterialList)
            {
                
                score = 0;

                //Direct Hit == LOADS OF POINTS;

                if (cm.Name.ToLower() == materialToLookup.ToLower())
                    score += (basescore * 3);

                // string materialname = cm.Name;
                int dist = Utils.CalcLevenshteinDistance(materialToLookup, cm.Name);
                score += (basescore - dist);

                //Get additional score points;

                // GRADE points
                if (cm.Grade != "" && grade != "")
                {
                    int gradeDist = Utils.CalcLevenshteinDistance(cm.Grade.ToLower(), grade.ToLower());
                    score += (gradebase - gradeDist);
                }

                //See if any of the words have a direct match in the lookup;
                foreach (string word in wordsInMaterialToLookup)
                {
                    string lowerWord = word.ToLower();

                    //Thif the material contains a word from the carbolist; get points
                    bool contains = cm.Name.IndexOf(lowerWord, StringComparison.OrdinalIgnoreCase) >= 0;
                    
                    if (contains == true)
                        score += basescore * 2;
                }

                //Make a decision of this is better than current best;
                if (score > highscore)
                {
                    highscore = score;
                    result = cm;
                }
            }
            Utils.WriteToLog("LookedupMaterial: " + materialToLookup + " Score: " + highscore + " -> " + result.Name);
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
                    fileName = "db\\UserMaterials.cxml";
                else
                    fileName = fileName + ".cxml";

                myPath = Utils.getAssemblyPath() + "\\" + fileName;

                //override if file exists on userfolder;
                string TemplateFile = PathUtils.getTemplateFolder();

                if (File.Exists(TemplateFile))
                    myPath = TemplateFile;

            }

            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CarboDatabase));

                //delete all resultdata
                foreach (CarboMaterial cm in CarboMaterialList)
                {
                    cm.materiaA4Properties.calcResult = "";
                    cm.materialC1C4Properties.c2Properties.calcResult = "";
                    cm.materialC1C4Properties.calcResult = "";
                }

                using (FileStream fs = new FileStream(myPath, FileMode.Create))
                {
                    ser.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show("Database saved to: " + myPath);

        }
        /// <summary>
        /// De-Serialises a material Database No file extension. 
        /// Current Options are: "db\\UserMaterials" and "db\\BaseMaterials" use "" for UserMaterial
        /// </summary>
        /// <param name="fileName"></param>
        public CarboDatabase DeSerializeXML(string fileName)
        {
            string myPath = fileName;
            PathUtils.CheckFileLocationsNew();

            //if its a relative path use:
            if (!(File.Exists(myPath)))
                {
                if (fileName == "")
                    fileName = "db\\UserMaterials.cxml";
                else
                    fileName = fileName + ".cxml";

            myPath = Utils.getAssemblyPath() + "\\" + fileName;

                //override if file exists on userfolder;
                string TemplateFile = PathUtils.getTemplateFolder();  

                if (File.Exists(TemplateFile))
                    myPath = TemplateFile;
                
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
                        Utils.WriteToLog("Deserialised: " + myPath);
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

        public bool AddMaterial(CarboMaterial newMaterial)
        {
            bool result = false;
            bool exists = false;
            bool uniqueId = true;

            //int id = getUniqueId();
            //newMaterial.Id = id;

            foreach (CarboMaterial cm in CarboMaterialList)
            {
                if (cm.Id == newMaterial.Id)
                {
                    uniqueId = false;
                    break;
                }
            }
            if(newMaterial.Id < 0 || uniqueId == false)
            {
                newMaterial.Id = getUniqueId();
            }

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
            Again:
            Random rnd = new Random();
            int id = rnd.Next(200000, 300000);  // creates a number between 1 and 12

            bool isUnique = isUniqueId(id);

            if (isUnique == false)
                goto Again;
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

            //never delete the last entry
            if (CarboMaterialList.Count == 1)
                return;

                try
            {
                for (int i = CarboMaterialList.Count -1; i >= 0; i--)
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

        public bool deleteMaterial(string name)
        {
            //string name = "";
            bool result = false;

            try
            {
                for (int i = CarboMaterialList.Count - 1; i >= 0; i--)
                {
                    if (CarboMaterialList[i].Name == name)
                    {
                        CarboMaterialList.RemoveAt(i);
                        result = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK);

            }

            return result;
        }

        /// <summary>
        /// Will update one database without the other, no elements will be deleted.
        /// </summary>
        /// <param name="userMaterials">The database the user wants to syncronize with</param>
        /// <returns></returns>
        public bool SyncMaterials(CarboDatabase userMaterials)
        {
            try
            {
                //Loop though all materials and update the current ones
                for (int i = userMaterials.CarboMaterialList.Count - 1; i >= 0; i--)
                {
                    CarboMaterial newcarboMaterial = userMaterials.CarboMaterialList[i];
                    bool matchfound = false;

                    //Find a match in current database
                    foreach (CarboMaterial carboMaterial in this.CarboMaterialList)
                    {
                        if (newcarboMaterial.Name == carboMaterial.Name)
                        {
                            //Copy all properties;
                            carboMaterial.Copy(newcarboMaterial);
                            matchfound = true;
                            //next'
                            break;
                        }
                    }

                    //If this part has been reached; 
                    //the material doesn't exist in the database and a new material will be created:
                    if (matchfound == false)
                    {
                        AddMaterial(newcarboMaterial);
                    }
                }
            }
            catch(Exception ex)
            {
                Utils.WriteToLog(ex.Message);
                return false;
            }

            //success:
            return true;
        }
        /// <summary>
        /// Creates a copy of the class and all its materials
        /// </summary>
        /// <returns>CarboDatabase</returns>
        public CarboDatabase Copy()
        {
            CarboDatabase copy = new CarboDatabase();

            foreach(CarboMaterial cm in this.CarboMaterialList)
            {
                CarboMaterial newcarboMaterial = new CarboMaterial();
                //newcarboMaterial.Copy(cm);
                newcarboMaterial = (CarboMaterial)cm.Clone();
                if(newcarboMaterial != null)
                    copy.AddMaterial(newcarboMaterial);
            }

            return copy;

        }

        public bool SyncCSVMaterials(CarboDatabase importedDb, object deleteMaterials)
        {
            //validate Ids
            foreach(CarboMaterial cm in importedDb.CarboMaterialList)
            {
                if(cm.Id == 0)
                {
                    cm.Id = getUniqueId();
                }
            }

            try
            {
                //Loop though all materials and update the current ones
                for (int i = importedDb.CarboMaterialList.Count - 1; i >= 0; i--)
                {
                    CarboMaterial newcarboMaterial = importedDb.CarboMaterialList[i];
                    bool matchfound = false;

                    //Find a match in current database
                    foreach (CarboMaterial carboMaterial in this.CarboMaterialList)
                    {
                        if (newcarboMaterial.Id == carboMaterial.Id)
                        {
                            //Copy properties, if different;

                            carboMaterial.Name = newcarboMaterial.Name;

                            carboMaterial.Category = newcarboMaterial.Category;

                            carboMaterial.Description = newcarboMaterial.Name;

                            carboMaterial.Density = newcarboMaterial.Density;

                            carboMaterial.WasteFactor = newcarboMaterial.WasteFactor;
                            carboMaterial.Grade = newcarboMaterial.Grade;
                            carboMaterial.EPDurl = newcarboMaterial.EPDurl;


                            if (newcarboMaterial.ECI_A1A3 != carboMaterial.ECI_A1A3)
                            {
                                carboMaterial.ECI_A1A3_Override = true;
                                carboMaterial.ECI_A1A3 = newcarboMaterial.ECI_A1A3;
                            }
                            if (newcarboMaterial.ECI_A4 != carboMaterial.ECI_A4)
                            {
                                carboMaterial.ECI_A4_Override = true;
                                carboMaterial.ECI_A4 = newcarboMaterial.ECI_A4;
                            }
                            if (newcarboMaterial.ECI_A5 != carboMaterial.ECI_A5)
                            {
                                carboMaterial.ECI_A5_Override = true;
                                carboMaterial.ECI_A5 = newcarboMaterial.ECI_A5;
                            }
                            if (newcarboMaterial.ECI_B1B5 != carboMaterial.ECI_B1B5)
                            {
                                //carboMaterial.ECI_B1B5 = true;
                                carboMaterial.ECI_B1B5 = newcarboMaterial.ECI_B1B5;
                            }
                            if (newcarboMaterial.ECI_C1C4 != carboMaterial.ECI_C1C4)
                            {
                                carboMaterial.ECI_C1C4_Override = true;
                                carboMaterial.ECI_C1C4 = newcarboMaterial.ECI_C1C4;
                            }
                            if (newcarboMaterial.ECI_D != carboMaterial.ECI_D)
                            {
                                carboMaterial.ECI_D_Override = true;
                                carboMaterial.ECI_D = newcarboMaterial.ECI_D;
                            }
                            if (newcarboMaterial.ECI_Mix != carboMaterial.ECI_Mix)
                            {
                                carboMaterial.ECI_Mix_Info = "Imported Value";
                                carboMaterial.ECI_Mix = newcarboMaterial.ECI_Mix;
                            }
                            if (newcarboMaterial.ECI_Seq != carboMaterial.ECI_Seq)
                            {
                                carboMaterial.ECI_Seq_Override = true;
                                carboMaterial.ECI_Seq = newcarboMaterial.ECI_Seq;
                            }


                            matchfound = true;
                            //next'
                            break;
                        }
                    }

                    //If this part has been reached; 
                    //the material doesn't exist in the database and a new material will be created:
                    if (matchfound == false)
                    {
                        AddMaterial(newcarboMaterial);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.WriteToLog(ex.Message);
                return false;
            }

            //success:
            return true;
        }
    }
}
