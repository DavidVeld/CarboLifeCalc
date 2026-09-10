using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using System.Xml;

namespace CarboLifeAPI.Data
{
    [Serializable]
    [XmlRoot("CarboDatabase")]
    public class CarboDatabase
    {
        [XmlArray("CarboMaterials"), XmlArrayItem(typeof(CarboMaterial), ElementName = "CarboMaterial")]
        public List<CarboMaterial> CarboMaterialList { get;  set; }
        public string templateName { get; set; }

        /// <summary>
        /// The match index. This is a PRIVATE field, so XmlSerializer never sees it and the
        /// .cxml schema {CarboMaterials, templateName} is untouched. It self validates against
        /// the material list on every lookup, see CarboMaterialMatcher.
        /// </summary>
        private CarboMaterialMatcher matcher;

        public List<CarboMaterial> getData()
        {
            return CarboMaterialList;
        }

        public void setData(List<CarboMaterial> data)
        {
            CarboMaterialList = data;
            EnsureSystemMaterials();
            InvalidateMatchIndex();
        }

        public CarboDatabase()
        {
            //DeSerialiseThedataBase
            CarboMaterialList = new List<CarboMaterial>();
            EnsureSystemMaterials();
        }

        /// <summary>
        /// Guarantees the built in "&lt;Empty&gt;" material is in this database, exactly once and
        /// with its defined values.
        ///
        /// Called on every path that produces or loads a database, including an existing project
        /// file, so the material is there whatever template the project was built from and
        /// however old the file is. Idempotent: running it twice adds nothing.
        ///
        /// It is appended rather than inserted at the front, because several places take
        /// CarboMaterialList[0] as a fallback material and prepending would silently make that
        /// fallback the empty one.
        /// </summary>
        /// <returns>True when the list was changed.</returns>
        public bool EnsureSystemMaterials()
        {
            if (CarboMaterialList == null)
                CarboMaterialList = new List<CarboMaterial>();

            bool changed = false;
            CarboMaterial found = null;

            //Walk backwards: a file that somehow carries two copies keeps the first and loses
            //the rest, rather than leaving duplicates in every picker.
            for (int i = CarboMaterialList.Count - 1; i >= 0; i--)
            {
                CarboMaterial cm = CarboMaterialList[i];

                if (cm == null || cm.IsSystemEmpty() == false)
                    continue;

                if (found == null)
                {
                    found = cm;
                }
                else
                {
                    CarboMaterialList.RemoveAt(i);
                    changed = true;
                }
            }

            if (found == null)
            {
                CarboMaterialList.Add(CarboMaterial.CreateSystemEmpty());
                changed = true;
            }
            else if (found.Density != 0 || found.ECI != 0 || found.Id != CarboMaterial.SystemEmptyId
                     || found.isLocked == false)
            {
                //A copy that drifted, or one edited in an older version, is put back.
                found.ResetToSystemEmpty();
                changed = true;
            }

            if (changed)
                InvalidateMatchIndex();

            return changed;
        }

        /// <summary>
        /// Returns the matching engine for this database, creating it on first use.
        /// </summary>
        private CarboMaterialMatcher GetMatcher()
        {
            if (matcher == null)
                matcher = new CarboMaterialMatcher(this);

            return matcher;
        }

        /// <summary>
        /// Excact Match.
        /// The comparison is case insensitive on the trimmed name, a case sensitive compare here
        /// is why a saved user mapping could silently stop applying.
        /// Still returns the LIVE database object (the material editor edits it in place) and
        /// still returns null when nothing matches.
        /// </summary>
        /// <param name="materialName"></param>
        /// <returns></returns>
        public CarboMaterial GetExcactMatch(string materialName)
        {
            CarboMaterial result = null;

            if (materialName == null || this.CarboMaterialList == null)
                return null;

            string wanted = materialName.Trim();

            foreach (CarboMaterial cm in this.CarboMaterialList)
            {
                if (cm == null || cm.Name == null)
                    continue;

                // string materialname = cm.Name;
                if (string.Equals(cm.Name.Trim(), wanted, StringComparison.OrdinalIgnoreCase))
                {
                    result = cm;
                    break;
                }
            }

            return result;
        }
        /// <summary>
        /// This is the heart of the mapping feature, a suggested material name, a category and a grade can be provided, the function will return the closest match from the database.
        /// The work is done by CarboMaterialMatcher, see CarboMaterialMatcher.cs.
        /// This method NEVER returns null and ALWAYS returns a fresh deep clone, callers mutate
        /// the result (CarboProject calls CalculateTotals() on it).
        /// Argument 2 is triaged before use: a value that parses as a grade is routed to the
        /// grade slot, a Revit ELEMENT category is routed to the form hint slot, anything else
        /// is treated as a Revit MaterialClass. An unrecognised value contributes exactly zero.
        /// When nothing scores above the low confidence threshold the best effort candidate is
        /// still returned, see CarboMatchOptions.Policy. When the database is empty a sentinel
        /// with Id == -1 and Name "(no match: X)" is returned.
        /// </summary>
        /// <param name="materialToLookup"></param>
        /// <param name="RevitMaterialCategory"></param>
        /// <param name="grade"></param>
        /// <returns></returns>
        public CarboMaterial getClosestMatch(string materialToLookup, string RevitMaterialCategory = "", string grade = "")
        {
            CarboMatchResult result = FindMatch(materialToLookup, RevitMaterialCategory, grade);
            return result.Material;
        }

        /// <summary>
        /// Full match result: the clone plus confidence, tier, reason and runner up.
        /// </summary>
        public CarboMatchResult FindMatch(CarboLookup query)
        {
            return GetMatcher().FindMatch(query);
        }

        /// <summary>
        /// Convenience overload, applies the same argument 2 triage as the legacy method.
        /// </summary>
        public CarboMatchResult FindMatch(string name, string revitMaterialClass, string grade)
        {
            return GetMatcher().FindMatch(name, revitMaterialClass, grade);
        }

        /// <summary>
        /// Top N ranked candidates with their component breakdown, for a "did you mean" dialog.
        /// Each returned Material is a deep clone.
        /// </summary>
        public List<CarboMatchCandidate> RankCandidates(CarboLookup query, int take)
        {
            return GetMatcher().RankCandidates(query, take);
        }

        /// <summary>
        /// Injects the user's saved alias table so it can be consulted before any scoring runs.
        /// Pass null to disable. Not serialised.
        /// </summary>
        public void SetAliasTable(CarboMapFile aliasMap)
        {
            GetMatcher().SetAliasTable(aliasMap);
        }

        /// <summary>
        /// Forces the match index to rebuild. The index also self validates on every lookup
        /// against list identity, Count and a content hash, so this is belt and braces, call it
        /// after editing a live material row in place.
        /// </summary>
        public void InvalidateMatchIndex()
        {
            if (matcher != null)
                matcher.Invalidate();
        }

        /// <summary>
        /// Flushes the buffered match log exactly once. Safe to call always, a no-op when
        /// CarboMatchDiagnostics.Enabled is false. Never throws.
        /// </summary>
        public static void FlushMatchLog()
        {
            CarboMatchDiagnostics.Flush();
        }
        // Superseded by getClosestMatch / CarboMaterialMatcher. Kept commented out for reference
        // only, so it is not compiled and cannot drift out of step with the real matcher.

//         public CarboMaterial getClosestMatchOldBackup(string materialToLookup, string RevitMaterialCategory = "", string grade = "")
//         {
//             int score = 0;
//             int highscore = -50;
//             int basescore = materialToLookup.Length;
// 
//             int gradebase = grade.Length;
// 
// 
//             CarboMaterial result = new CarboMaterial();
//             //string[] wordsInMaterialToLookup = materialToLookup.Split(' ');
// 
//             //better split:
//             string[] wordsInMaterialToLookup = materialToLookup.Split(new Char[] { ' ', '_', '-' });
// 
//             Utils.MatchLogWrite("New Material: " + materialToLookup);
// 
//             foreach (string str in wordsInMaterialToLookup)
//             {
//                 Utils.MatchLogWrite(str);
//             }
// 
// 
//             foreach (CarboMaterial cm in this.CarboMaterialList)
//             {
//                 string logEntry = "";
// 
//                 score = 0;
//                 int matchScore = 0;
//                 int distScore = 0;
// 
//                 int gradeScore = 0;
//                 int wordScore = 0;
//                 string wordsthatmatch = "";
//                 int categoryScore1 = 0;
//                 int categoryScore2 = 0;
// 
// 
//                 //Direct Hit == LOADS OF POINTS;
// 
//                 if (cm.Name.ToLower() == materialToLookup.ToLower())
//                 {
//                     matchScore = (basescore * 3);
//                 }
//                 // string materialname = cm.Name;
//                 int dist = Utils.CalcLevenshteinDistance(materialToLookup, cm.Name);
//                 distScore = (basescore - dist);
// 
//                 //Get additional score points;
// 
//                 // GRADE points
//                 if (cm.Grade != "" && grade != "")
//                 {
//                     int gradeDist = Utils.CalcLevenshteinDistance(cm.Grade.ToLower(), grade.ToLower());
//                     gradeScore = (cm.Grade.Length - gradeDist) * 2;
//                 }
// 
//                 //See if any of the words have a direct match in the lookup;
//                 foreach (string word in wordsInMaterialToLookup)
//                 {
//                     word.Trim();
//                     if (word != "" || word != " ")
//                     {
//                         string lowerWord = word.ToLower();
//                         string materialName = cm.Name.ToLower();
//                         string categoryName = cm.Category.ToLower();
// 
//                         //Thif the material contains a word from the carbolist; get points
//                         bool contains = materialName.IndexOf(lowerWord, StringComparison.OrdinalIgnoreCase) >= 0;
// 
//                         bool containsCategory1 = categoryName.IndexOf(lowerWord, StringComparison.OrdinalIgnoreCase) >= 0;
// 
// 
//                         bool containsCategory2 = false;
//                         if (RevitMaterialCategory != "")
//                             containsCategory2 = RevitMaterialCategory.IndexOf(lowerWord, StringComparison.OrdinalIgnoreCase) >= 0;
// 
// 
//                         if (contains == true)
//                         {
//                             wordScore += word.Length * 2;
//                             wordsthatmatch += word + ":" + wordScore.ToString() + " ";
//                         }
//                         //else ///penalty if word does not exist in searched string
//                         //wordScore += -1 * (word.Length);
// 
//                         //Checks if the material category contains the word
//                         if (containsCategory1 == true)
//                         {
//                             categoryScore1 += word.Length * 5;
//                             //wordsthatmatch += word + ":" + wordScore.ToString() + " ";
//                         }
// 
//                         //If a revit Category has been provided, check for extra points;
//                         if (containsCategory2 == true)
//                         {
//                             categoryScore2 += word.Length * 5;
//                         }
//                     }
//                 }
// 
//                 //Calculate Total Score;
//                 score += matchScore + distScore + gradeScore + wordScore + categoryScore1 + categoryScore2;
// 
//                 //Make a decision of this is better than current best;
//                 if (score > highscore)
//                 {
//                     highscore = score;
//                     result = cm.Clone() as CarboMaterial;
//                 }
// 
//                 logEntry = materialToLookup + "," + cm.Name.ToLower() + "," + matchScore.ToString() + "," + distScore.ToString() + "," + gradeScore.ToString() + "," + wordScore.ToString() + "," + score + " Matching Words:(" + wordsthatmatch + ")";
// 
// 
//                 Utils.MatchLogWrite(logEntry);
// 
//             }
//             //The match
//             Utils.MatchLogWrite(materialToLookup + "," + result.Name.ToLower() + "," + "Match" + "," + " Dist" + "," + "Grade" + "," + "WordsMatching" + "," + highscore);
//             return result;
//         }

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
                    fileName = "db\\materials\\UserMaterials.cxml";
                else
                    fileName = fileName + ".cxml";

                myPath = Utils.getAssemblyPath() + "\\" + fileName;

                //override if file exists on userfolder;
                string TemplateFile = PathUtils.getTemplateFile();

                if (File.Exists(TemplateFile))
                    myPath = TemplateFile;

            }

            try
            {
                //delete all resultdata
                foreach (CarboMaterial cm in CarboMaterialList)
                {
                    cm.materiaA4Properties.calcResult = "";
                    cm.materialC1C4Properties.c2Properties.calcResult = "";
                    cm.materialC1C4Properties.calcResult = "";
                }

                //
                XmlSerializer ser = new XmlSerializer(typeof(CarboDatabase));

                //New Way
                //MemoryStream memoryStreamSerialize = new MemoryStream();
                //XmlSerializer xmlSerializerSerialize = new XmlSerializer(typeof(CarboDatabase));
                //XmlTextWriter xmlTextWriterSerialize = new XmlTextWriter(memoryStreamSerialize, Encoding.UTF8);

                //xmlSerializerSerialize.Serialize(xmlTextWriterSerialize, this);
                //memoryStreamSerialize = (MemoryStream)xmlTextWriterSerialize.BaseStream;

                // converts a byte array of unicode values (UTF-8 enabled) to a string
                //UTF8Encoding encodingSerialize = new UTF8Encoding();
                //string serializedXml = encodingSerialize.GetString(memoryStreamSerialize.ToArray());


                using (FileStream fs = new FileStream(myPath, FileMode.Create))
                {
                    ser.Serialize(fs, this);

                }

                //xmlTextWriterSerialize.Close();
                //memoryStreamSerialize.Close();
                //memoryStreamSerialize.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show("Database saved to: " + myPath);

        }
        /// <summary>
        /// Loads a material template from disk, both .cxml and .csv templates are supported.
        /// </summary>
        /// <param name="templateFilePath">Full path to the template file, "" loads the default template</param>
        /// <returns>The template database, never null but possibly empty</returns>
        public static CarboDatabase LoadTemplate(string templateFilePath)
        {
            if (templateFilePath == null)
                templateFilePath = "";

            if (templateFilePath.EndsWith("csv", StringComparison.OrdinalIgnoreCase))
            {
                CarboDatabase csvResult = new CarboDatabase();
                try
                {
                    foreach (CarboMaterial cm in DataExportUtils.GetMaterialDatabaseFromCVSFile(templateFilePath))
                        csvResult.CarboMaterialList.Add(cm);
                }
                catch
                {
                    MessageBox.Show("Error importing materials from CSV, please review", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return csvResult;
            }

            CarboDatabase result = new CarboDatabase().DeSerializeXML(templateFilePath);

            return result != null ? result : new CarboDatabase();
        }

        /// <summary>
        /// De-Serialises a material Database No file extension.
        /// Current Options are: "db\\UserMaterials" and "db\\BaseMaterials" use "" for UserMaterial
        /// </summary>
        /// <param name="fileName"></param>
        public CarboDatabase DeSerializeXML(string fileName)
        {
            string myPath = fileName;
            PathUtils.CheckFileLocations();
            string fileNameNoExtension = "";

            //if its a relative path use:
            if (!(File.Exists(myPath)))
                {
                if (fileName == "")
                    fileName = "db\\materials\\UserMaterials.cxml";
                else
                    fileName = fileName + ".cxml";

                myPath = Utils.getAssemblyPath() + "\\" + fileName;

                //override if file exists on userfolder;
                string TemplateFile = PathUtils.getTemplateFile();  

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
                    fileNameNoExtension = Path.GetFileNameWithoutExtension(myPath);

                    using (FileStream fs = new FileStream(myPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as CarboDatabase;
                    }
                    //Utils.WriteToLog("Deserialised: " + myPath);
                    bufferproject.templateName = fileNameNoExtension;

                    //Whatever the template holds, it gets the built in "<Empty>" material.
                    bufferproject.EnsureSystemMaterials();

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
                InvalidateMatchIndex();
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
                        //The built in material is part of every database by definition, and code
                        //elsewhere assumes it is there. EnsureSystemMaterials would put it back
                        //on the next load anyway, so refuse plainly rather than appear to work.
                        if (CarboMaterialList[i].IsSystemEmpty())
                        {
                            MessageBox.Show(CarboMaterial.SystemEmptyName + " is a built in material and cannot be deleted.",
                                            "Built in material", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        name = CarboMaterialList[i].Name;
                        CarboMaterialList.RemoveAt(i);
                        InvalidateMatchIndex();
                        ok = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK);

            }

            //if (ok == true)
                //MessageBox.Show(name + " deleted.", "Deleted", MessageBoxButton.OK);
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
                        //See the overload above: the built in material is not removable.
                        if (CarboMaterialList[i].IsSystemEmpty())
                            return false;

                        CarboMaterialList.RemoveAt(i);
                        InvalidateMatchIndex();
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

            //Rows were added or edited in place, the match index has to be rebuilt.
            InvalidateMatchIndex();

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

        public bool SyncCSVMaterials(CarboDatabase importedDb, bool deleteMaterials)
        {
            //An empty import is never a legitimate instruction, and with deleteMaterials set it
            //used to empty the library: Clear() ran first and then there was nothing to put
            //back. A csv the reader could not make sense of - the wrong column order, or one
            //Excel re-saved semicolon separated - produced exactly that empty list, and the
            //method still returned true, so the caller reported success and the editor wrote
            //the emptied library back over the user's template file.
            //
            //Refusing here rather than in the dialog because this is the last point before the
            //data is gone, and more than one caller reaches it.
            if (importedDb == null || importedDb.CarboMaterialList == null || importedDb.CarboMaterialList.Count == 0)
                return false;

            //validate Ids
            foreach(CarboMaterial cm in importedDb.CarboMaterialList)
            {
                if(cm.Id == 0)
                {
                    cm.Id = getUniqueId();
                }
            }

            //delete if required
            if(deleteMaterials == true)
            {
                this.CarboMaterialList.Clear();
                InvalidateMatchIndex();
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

                            carboMaterial.Description = newcarboMaterial.Description;

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

            //Rows were added or edited in place, the match index has to be rebuilt.
            InvalidateMatchIndex();

            //success:
            return true;
        }
    }
}
