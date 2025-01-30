using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.IO;

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
    }
}