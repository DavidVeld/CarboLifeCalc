using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;

namespace CarboCircle.data
{
    internal class carboCircleUtils
    {
        internal static void ExportDataToCSV(List<carboCircleElement> dataCombined, string path)
        {
            if (File.Exists(path) && DataExportUtils.IsFileLocked(path) == true)
                return;

            string fileString = "";

            //Create Headers;
            fileString =
                "id, GUID, humanId, category, name, materialName, materialClass, length, " +
                "volume, netLength, netVolume, grade, quality, isVolumeElement, " +
                "standardName, standardDepth, standardWidth, standardCategory, Iy, Wy, " +
                "Iz, Wz, matchGUID, isOffcut" + Environment.NewLine;
            //Advanced
            foreach (carboCircleElement ccE in dataCombined)
            {
                try
                {
                    string resultString = "";

                    resultString += DataExportUtils.CVSFormat(ccE.id.ToString()) + ","; //1
                    resultString += DataExportUtils.CVSFormat(ccE.GUID) + ","; //2
                    resultString += DataExportUtils.CVSFormat(ccE.humanId) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.category) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.name) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.materialName) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.materialClass) + ","; //3
                    resultString += ccE.length + ","; //7
                    resultString += ccE.volume + ","; //7
                    resultString += ccE.netLength + ","; //7
                    resultString += ccE.netVolume + ","; //7

                    resultString += DataExportUtils.CVSFormat(ccE.grade) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.quality.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.isVolumeElement.ToString()) + ","; //3

                    resultString += DataExportUtils.CVSFormat(ccE.standardName.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.standardDepth.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.standardWidth.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.standardCategory.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.Iy.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.Wy.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.Iz.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.Wy.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.matchGUID.ToString()) + ","; //3
                    resultString += DataExportUtils.CVSFormat(ccE.isOffcut.ToString()) + ","; //3

                    resultString += Environment.NewLine;

                    fileString += resultString;
                }
                catch (IOException ex)
                {
                   // Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            DataExportUtils.WriteCVSFile(fileString, path);


        }
        internal static List<carboCircleMatchElement> getCarboMatchListSimplified(List<carboCirclePair> carboCircleMatchedPairs)
        {
            List<carboCircleMatchElement> result = new List<carboCircleMatchElement>();

            if(carboCircleMatchedPairs != null )
            {
                if(carboCircleMatchedPairs.Count > 0) 
                { 
                    foreach(carboCirclePair pair in carboCircleMatchedPairs)
                    {
                        carboCircleMatchElement ccme
                            = new carboCircleMatchElement();

                        //convert the pairs to simplified data
                        //required Element
                        ccme.required_id = pair.required_element.id;
                        ccme.required_Name = pair.required_element.name;
                        ccme.required_standardName = pair.required_element.standardName;
                        ccme.required_length = pair.required_element.length;

                        //mined elemnent
                        ccme.mined_id = pair.mined_Element.id;
                        ccme.mined_Name = pair.mined_Element.name;
                        ccme.mined_standardName = pair.mined_Element.standardName;
                        ccme.mined_netLength = pair.mined_Element.netLength;

                        ccme.match_Score = pair.match_Score;

                        ccme.description = pair.description;

                        result.Add(ccme);
                    }
                }
            }

            return result;


        }

        internal static List<carboCircleElement> GetElementsFromCVSFile(string importPath)
        {
            List<carboCircleElement> cmList = new List<carboCircleElement>();

            try
            {
                if (File.Exists(importPath) && DataExportUtils.IsFileLocked(importPath) == false)
                {
                    DataTable profileTable = Utils.LoadCSV(importPath);

                    foreach (DataRow dr in profileTable.Rows)
                    {
                        try
                        {
                            carboCircleElement cce = new carboCircleElement();
                            cce.id = Convert.ToInt32(Utils.ConvertMeToDouble(dr[0].ToString()));
                            cce.GUID = dr[1].ToString();
                            cce.humanId = dr[2].ToString();
                            cce.category = dr[3].ToString();
                            cce.name = dr[4].ToString();
                            cce.materialName = dr[5].ToString();
                            cce.materialClass = dr[6].ToString();

                            cce.length = Utils.ConvertMeToDouble(dr[7].ToString());
                            cce.volume = Utils.ConvertMeToDouble(dr[8].ToString());
                            cce.netLength = Utils.ConvertMeToDouble(dr[9].ToString());
                            cce.netVolume = Utils.ConvertMeToDouble(dr[10].ToString());

                            cce.grade = dr[11].ToString();
                            cce.quality = Convert.ToInt32(Utils.ConvertMeToDouble(dr[12].ToString()));
                            
                            bool parseOk = false;
                            bool isVolume = true;
                            bool isOffcut = true;

                            parseOk = Boolean.TryParse(dr[13].ToString(), out isVolume);
                            if(parseOk)
                                cce.isVolumeElement = isVolume;

                            cce.standardName = dr[14].ToString();
                            cce.standardDepth = Utils.ConvertMeToDouble(dr[15].ToString());
                            cce.standardWidth = Utils.ConvertMeToDouble(dr[16].ToString());
                            cce.standardCategory = dr[17].ToString();
                            cce.Iy = Utils.ConvertMeToDouble(dr[18].ToString());
                            cce.Wy = Utils.ConvertMeToDouble(dr[19].ToString());
                            cce.Iz = Utils.ConvertMeToDouble(dr[20].ToString());
                            cce.Wy = Utils.ConvertMeToDouble(dr[21].ToString());
                            cce.matchGUID = dr[22].ToString();

                            parseOk = Boolean.TryParse(dr[23].ToString(), out isOffcut);
                            if (parseOk)
                                cce.isOffcut = isOffcut;



                            cmList.Add(cce);
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
            catch 
            {
                return null;
            }

            return cmList;



        }


        internal static CarboLifeAPI.Data.CarboProject convertToCarboLifeProject(carboCircleProject circleProject)
        {
            string databasepath = getCircleDatabasePath();
            if (File.Exists(databasepath))
            {
                CarboProject result = new CarboProject(databasepath);
                //Get Materials
                if (result != null)
                {
                    List<CarboElement> elements = new List<CarboElement>();

                    //Get all reused elements;
                    foreach (carboCirclePair ccp in circleProject.carboCircleMatchedPairs)
                    {
                        CarboElement carboElement = new CarboElement();
                        carboElement.Name = ccp.mined_Element.name;
                        carboElement.Volume = ccp.mined_Element.netVolume;
                        carboElement.MaterialName = ccp.mined_Element.materialName;
                        carboElement.Id = ccp.mined_Element.id;
                        elements.Add(carboElement);
                    }

                    if (elements.Count > 0)
                    {
                        foreach (CarboElement ce in elements)
                            result.AddElement(ce);
                    }

                    result.Audit();
                    result.CreateGroups();
                    result.CalculateProject();
                }
                else
                {
                    return null;
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds the location of the Carbo Circle Material Database
        /// </summary>
        /// <returns>Settings File path</returns>
        internal static string getCircleDatabasePath()
        {
            //string fileName = "db\\CarboSettings.xml";
            string myLocalPath = Utils.getAssemblyPath() + "\\db\\" + "carboCircleMaterials.cxml";
            try
            {
                if (File.Exists(myLocalPath))
                    return myLocalPath;
                else
                {
                    MessageBox.Show("Could not find a path reference to the carboCircleMaterials.cxml re-used material database file, you possibly have to re-install the software" + Environment.NewLine +
                            "Target: " + myLocalPath + Environment.NewLine +
                            "Target: " + myLocalPath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return myLocalPath;
                }
            }
            catch
            {
                return null;
            }

        }
    }
}