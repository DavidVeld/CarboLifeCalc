using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;

namespace CarboCircle.data
{
    internal class carboCircleUtils
    {
        internal static void ExportDataToCSV(List<carboCircleElement> dataCombined, string path)
        {
            if (File.Exists(path) && DataExportUtils.IsFileLocked(path) == true)
                return;

            //DataExportUtils.CsvLine, the same builder the Carbo Life exports use, so there is
            //one place that decides how a number or a piece of text becomes a csv field. It
            //formats every number in the invariant culture, which this file needs more than
            //most: it is comma separated and read back positionally, so a comma decimal
            //separator does not merely look odd, it adds a field and shifts every column after
            //it. It also writes the separator between fields rather than after each one, which
            //is what keeps the row the same width as the header.
            StringBuilder fileString = new StringBuilder();

            //Create Headers;
            //
            //Positional, and read back by index, so a column may only ever be APPENDED - see
            //GetElementsFromCVSFile. The three at the end were added when the matcher started
            //depending on them; a file written before that simply stops short and the reader
            //leaves them at their defaults.
            //
            //The names used to be written as one string with ", " between them, which left a
            //leading space on all but the first, so the header read "id", " GUID", " humanId".
            fileString.Append(new DataExportUtils.CsvLine().AddRange(
                "id",                   //0
                "GUID",                 //1
                "humanId",              //2
                "category",             //3
                "name",                 //4
                "materialName",         //5
                "materialClass",        //6
                "length",               //7
                "volume",               //8
                "netLength",            //9
                "netVolume",            //10
                "grade",                //11
                "quality",              //12
                "isVolumeElement",      //13
                "standardName",         //14
                "standardDepth",        //15
                "standardWidth",        //16
                "standardCategory",     //17
                "Iy",                   //18
                "Wy",                   //19
                "Iz",                   //20
                "Wz",                   //21
                "matchGUID",            //22
                "isOffcut",             //23
                "sectionConfidence",    //24
                "sourceGUID",           //25
                "massPerMetre"          //26
                ).ToLine());

            //Advanced
            foreach (carboCircleElement ccE in dataCombined)
            {
                try
                {
                    DataExportUtils.CsvLine row = new DataExportUtils.CsvLine();

                    row.Add(ccE.id);                    //0
                    row.Add(ccE.GUID);                  //1
                    row.Add(ccE.humanId);               //2
                    row.Add(ccE.category);              //3
                    row.Add(ccE.name);                  //4
                    row.Add(ccE.materialName);          //5
                    row.Add(ccE.materialClass);         //6

                    row.Add(ccE.length);                //7
                    row.Add(ccE.volume);                //8
                    row.Add(ccE.netLength);             //9
                    row.Add(ccE.netVolume);             //10

                    row.Add(ccE.grade);                 //11
                    row.Add(ccE.quality);               //12
                    row.Add(ccE.isVolumeElement);       //13

                    row.Add(ccE.standardName);          //14
                    row.Add(ccE.standardDepth);         //15
                    row.Add(ccE.standardWidth);         //16
                    row.Add(ccE.standardCategory);      //17
                    row.Add(ccE.Iy);                    //18
                    row.Add(ccE.Wy);                    //19
                    row.Add(ccE.Iz);                    //20
                    //Wz, at last. This column has always been written with Wy in it, under a
                    //header saying Wz, and read back into Wy again - so Wz came home as zero on
                    //every round trip. That mattered little while nothing used it; the matcher
                    //now gates on minor-axis capacity, and a zero there quietly skips the gate.
                    row.Add(ccE.Wz);                    //21
                    row.Add(ccE.matchGUID);             //22
                    row.Add(ccE.isOffcut);              //23

                    //Appended for the matcher. sectionConfidence in particular: without it every
                    //imported element claims an exact section identity, and two rows sharing a
                    //name become a "100% match" on a name nobody confirmed.
                    row.Add(ccE.sectionConfidence);     //24
                    row.Add(ccE.sourceGUID);            //25
                    row.Add(ccE.massPerMetre);          //26

                    fileString.Append(row.ToLine());
                }
                catch (Exception ex)
                {
                    //Nothing here touches a file, so the IOException this used to catch could
                    //never fire while a null on the element would escape the whole export.
                }
            }

            DataExportUtils.WriteCVSFile(fileString.ToString(), path);


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
                        if (pair == null || pair.required_element == null || pair.mined_Element == null)
                            continue;

                        carboCircleMatchElement ccme
                            = new carboCircleMatchElement();

                        //convert the pairs to simplified data
                        //required Element
                        ccme.required_id = pair.required_element.id;
                        ccme.required_humanId = pair.required_element.humanId;
                        ccme.required_Name = pair.required_element.name;
                        ccme.required_standardName = pair.required_element.standardName;
                        ccme.required_length = pair.required_element.length;
                        ccme.required_volume = pair.required_element.volume;

                        //mined elemnent
                        ccme.mined_id = pair.mined_Element.id;
                        ccme.mined_humanId = pair.mined_Element.humanId;
                        ccme.mined_Name = pair.mined_Element.name;
                        ccme.mined_standardName = pair.mined_Element.standardName;
                        ccme.mined_netLength = pair.mined_Element.netLength;

                        //What this requirement actually consumes, not the whole piece offered.
                        ccme.mined_netVolume = pair.used_netVolume;

                        //The six fields below were declared on this class from the start and
                        //never filled in. isOffcut is the reason the "from an offcut" category
                        //could not be shown at all.
                        ccme.isOffcut = pair.mined_Element.isOffcut;
                        ccme.isVolumeElement = pair.mined_Element.isVolumeElement;
                        ccme.matchRank = pair.matchClass;
                        ccme.used_netLength = pair.used_netLength;
                        ccme.offcut_netLength = pair.offcut_netLength;

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
                            cce.id = Convert.ToInt32(DataExportUtils.ReadCsvDouble(dr[0].ToString()));
                            cce.GUID = dr[1].ToString();
                            cce.humanId = dr[2].ToString();
                            cce.category = dr[3].ToString();
                            cce.name = dr[4].ToString();
                            cce.materialName = dr[5].ToString();
                            cce.materialClass = dr[6].ToString();

                            cce.length = DataExportUtils.ReadCsvDouble(dr[7].ToString());
                            cce.volume = DataExportUtils.ReadCsvDouble(dr[8].ToString());
                            cce.netLength = DataExportUtils.ReadCsvDouble(dr[9].ToString());
                            cce.netVolume = DataExportUtils.ReadCsvDouble(dr[10].ToString());

                            cce.grade = dr[11].ToString();
                            cce.quality = Convert.ToInt32(DataExportUtils.ReadCsvDouble(dr[12].ToString()));
                            
                            bool parseOk = false;
                            bool isVolume = true;
                            bool isOffcut = true;

                            parseOk = Boolean.TryParse(dr[13].ToString(), out isVolume);
                            if(parseOk)
                                cce.isVolumeElement = isVolume;

                            cce.standardName = dr[14].ToString();
                            cce.standardDepth = DataExportUtils.ReadCsvDouble(dr[15].ToString());
                            cce.standardWidth = DataExportUtils.ReadCsvDouble(dr[16].ToString());
                            cce.standardCategory = dr[17].ToString();
                            cce.Iy = DataExportUtils.ReadCsvDouble(dr[18].ToString());
                            cce.Wy = DataExportUtils.ReadCsvDouble(dr[19].ToString());
                            cce.Iz = DataExportUtils.ReadCsvDouble(dr[20].ToString());
                            //Was read into Wy a second time, so Wz was always zero.
                            cce.Wz = DataExportUtils.ReadCsvDouble(dr[21].ToString());
                            cce.matchGUID = dr[22].ToString();

                            parseOk = Boolean.TryParse(dr[23].ToString(), out isOffcut);
                            if (parseOk)
                                cce.isOffcut = isOffcut;

                            //Appended columns. A file written before they existed is shorter, so
                            //each is guarded and simply keeps its default.
                            //
                            //sectionConfidence defaults to 0 = Exact on the class, which is right
                            //for an element the importer has just resolved and wrong for one
                            //arriving from a file. An older csv therefore lands on Assumed: the
                            //numbers may still earn a substitution, but the name alone will not
                            //be presented as a 100% match.
                            if (dr.Table.Columns.Count > 24)
                                cce.sectionConfidence = (int)Math.Round(DataExportUtils.ReadCsvDouble(dr[24].ToString()));
                            else
                                cce.sectionConfidence = 1;

                            if (dr.Table.Columns.Count > 25)
                                cce.sourceGUID = dr[25].ToString();

                            if (dr.Table.Columns.Count > 26)
                                cce.massPerMetre = DataExportUtils.ReadCsvDouble(dr[26].ToString());



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
            //Carbon values for the reused materials come from the file named in the
            //CarboCircle settings, falling back to the copy shipped in circledb.
            string databasepath = circleProject.settings.getMaterialDatabasePath();

            if (databasepath != null && File.Exists(databasepath))
            {
                CarboProject result = new CarboProject(databasepath);
                //Get Materials
                if (result != null)
                {
                    List<CarboElement> elements = new List<CarboElement>();

                    //Get all reused elements;
                    foreach (carboCirclePair ccp in circleProject.carboCircleMatchedPairs)
                    {
                        //A requirement nothing could serve is carried as a real pair so the
                        //schedule is complete on screen, but there is no reused material behind
                        //it and it must not reach the carbon calculation.
                        if (ccp == null || ccp.matchClass == carboCircleMatchRules.ClassNoMatch)
                            continue;

                        CarboElement carboElement = new CarboElement();
                        carboElement.Name = ccp.mined_Element.name;

                        //What this requirement consumed, not the whole piece it came out of.
                        //Taking the piece counted a 9 m beam in full against a 6 m requirement,
                        //and then counted its offcut again on the next match.
                        carboElement.Volume = ccp.used_netVolume;

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

    }
}