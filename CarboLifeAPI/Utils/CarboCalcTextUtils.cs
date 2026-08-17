using CarboLifeAPI.Data;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CarboLifeAPI
{
    public static class CarboCalcTextUtils
    {

        public static string GetTransportCalc()
        {
            string result = "";

            return result;
        }


        /// <summary>
        /// Returns a DataTable with the current detailed breakdown of each group
        /// </summary>
        /// <param name="project">This is the project you want the results back from</param>
        /// <param name="fullResult">To get all the results (including the ones that have been disabled in the project set as true to get the full table </param>
        /// <returns></returns>
        public static DataTable getResultTable(CarboProject project, bool fullResult = false)
        {
            if (fullResult == true)
                project.CalculateProjectByPhase();
            else
                project.CalculateProject();

            DataTable result = new DataTable();
            result.Columns.Add("Category");
            result.Columns.Add("Material");
            result.Columns.Add("Description");
            result.Columns.Add("Correction");

            result.Columns.Add("Waste");
            result.Columns.Add("Added");
            result.Columns.Add("B4");
            result.Columns.Add("Density");
            result.Columns.Add("Mass");
            result.Columns.Add("ECI");
            result.Columns.Add("ECIm3");
            result.Columns.Add("EC");
            result.Columns.Add("Percent");

            result.Columns.Add("A1-A3");
            result.Columns.Add("A4");
            result.Columns.Add("A5");
            result.Columns.Add("B1-B7");
            result.Columns.Add("C1-C4");
            result.Columns.Add("D");
            result.Columns.Add("Sequestration");
            result.Columns.Add("Mix");
            result.Columns.Add("IsSubstructure");


            foreach (CarboGroup cg in project.getGroupList)
            {
                DataRow dr = result.NewRow();

                dr["Category"] = cg.Category;
                dr["Material"] = cg.Material.Name;
                dr["Description"] = cg.Description;
                dr["Correction"] = cg.Correction;

                dr["Waste"] = cg.Waste;
                dr["Added"] = cg.Additional;
                dr["B4"] = cg.inUseProperties.B4;
                dr["Density"] = cg.Density;
                dr["Mass"] = cg.Mass;
                dr["ECI"] = cg.ECI;
                dr["ECIm3"] = cg.getVolumeECI;
                dr["EC"] = cg.EC;
                dr["Percent"] = cg.PerCent;

                if(cg.isSubstructure == true)
                    dr["IsSubstructure"] = "1";
                else
                    dr["IsSubstructure"] = "0";

                if (project.calculateA13 == true || fullResult == true)
                    dr["A1-A3"] = cg.getTotalA1A3;

                if (project.calculateA4 == true || fullResult == true)
                    dr["A4"] = cg.getTotalA4;

                if (project.calculateA5 == true || fullResult == true)
                    dr["A5"] = cg.getTotalA5;

                if (project.calculateB == true || fullResult == true)
                    dr["B1-B7"] = cg.getTotalB1B7;

                if (project.calculateC == true || fullResult == true)
                    dr["C1-C4"] = cg.getTotalC1C4;

                if (project.calculateD == true || fullResult == true)
                    dr["D"] = cg.getTotalD;

                if (project.calculateSeq == true || fullResult == true)
                    dr["Sequestration"] = cg.getTotalSeq;

                if (project.calculateAdd == true || fullResult == true)
                    dr["Mix"] = cg.getTotalMix;


                result.Rows.Add(dr);
            }


            return result;
        }

        public static DataTable getByElementTable(CarboProject project, bool fullResult = false)
        {
            if (fullResult == true)
                project.CalculateProjectByPhase();
            else
                project.CalculateProject();

            DataTable result = new DataTable();
            result.Columns.Add("Level"); //0
            result.Columns.Add("Elevation");//1
            result.Columns.Add("Category"); //2
            result.Columns.Add("SubCategory");
            result.Columns.Add("Material"); //4
            result.Columns.Add("IsSubstructure");//5

            result.Columns.Add("TotalEC"); //6

            result.Columns.Add("A1-A3");
            result.Columns.Add("A4");
            result.Columns.Add("A5");
            result.Columns.Add("B1-B7");
            result.Columns.Add("C1-C4");
            result.Columns.Add("D");
            result.Columns.Add("Sequestration");
            result.Columns.Add("Mix");

            //IList<CarboElement> elementList = project.getElementsFromGroups().ToList();
            JsCarboProject jsProject = JsonExportUtils.converToJsProject(project);

            foreach (JsCarboElement ce in jsProject.elementList)
            {
                if (ce.isSubstructure == true && project.calculateSubStructure == false)
                    continue;

                DataRow dr = result.NewRow();

                dr["Level"] = ce.LevelName;
                dr["Elevation"] = ce.Level.ToString();

                dr["Category"] = ce.Category;
                dr["SubCategory"] = ce.SubCategory;
                dr["Material"] = ce.CarboMaterialName;
                dr["IsSubstructure"] = "No";
                if (ce.isSubstructure == true)
                {
                    dr["IsSubstructure"] = "Yes";
                }

                double totalEC = 0;

                if (project.calculateA13 == true || fullResult == true)
                {
                    dr["A1-A3"] = ce.EC_A1A3_Total;
                    totalEC += ce.EC_A1A3_Total;
                }

                if (project.calculateA4 == true || fullResult == true)
                {
                    dr["A4"] = ce.EC_A4_Total;
                    totalEC += ce.EC_A4_Total;
                }

                if (project.calculateA5 == true || fullResult == true)
                {
                    dr["A5"] = ce.EC_A5_Total;
                    totalEC += ce.EC_A5_Total;
                }

                if (project.calculateB == true || fullResult == true)
                {
                    dr["B1-B7"] = ce.EC_B1B7_Total;
                    totalEC += ce.EC_B1B7_Total;

                }

                if (project.calculateC == true || fullResult == true)
                {
                    dr["C1-C4"] = ce.EC_C1C4_Total;
                    totalEC += ce.EC_C1C4_Total;
                }

                if (project.calculateD == true || fullResult == true)
                {
                    dr["D"] = ce.EC_D_Total;
                    totalEC += ce.EC_D_Total;
                }


                if (project.calculateSeq == true || fullResult == true)
                {
                    dr["Sequestration"] = ce.EC_Sequestration_Total;
                    totalEC += ce.EC_Sequestration_Total;
                }

                if (project.calculateAdd == true || fullResult == true)
                {
                    dr["Mix"] = ce.EC_Mix_Total;
                    totalEC += ce.EC_Mix_Total;
                }

                dr["TotalEC"] = totalEC;


                result.Rows.Add(dr);
            }


            return result;
        }


        /// <summary>
        /// Converts a ResultTable to a DataPoint List for use in graphs
        /// </summary>
        /// <param name="table"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static List<CarboDataPoint> ConvertResultTableToDataPoints(DataTable table, string Type = "Material", List<CarboElement> projectElements = null)
        {
            List<CarboDataPoint> valueList = new List<CarboDataPoint>();

            if(Type == "Super - SubStructure" && table == null)
            {
                valueList = ConvertResultTableToDataPointsSubSuperStruct(projectElements);
                return valueList;
            }

            if (Type == "Category Merged" && table == null)
            {
                valueList = ConvertResultTableToDataPointsMerged(projectElements);
                return valueList;
            }

            if (Type == "Category Merged +" && table == null)
            {
                valueList = ConvertResultTableToDataPointsMergedPlus(projectElements);
                return valueList;
            }

            //below for normal pie charts
            try
            {
                foreach (DataRow dr in table.Rows)
                {
                    CarboDataPoint newelement = new CarboDataPoint();
                    if (Type == "Material")
                        newelement.Name = dr["Material"].ToString();
                    else //category
                        newelement.Name = dr["Category"].ToString();

                    newelement.Value = Utils.ConvertMeToDouble(dr["EC"].ToString());

                    bool merged = false;

                    //Add a new databoint, orr add value if exists
                    if (valueList.Count > 0)
                    {
                        foreach (CarboDataPoint pp in valueList)
                        {
                            if (pp.Name == newelement.Name)
                            {
                                pp.Value += newelement.Value;
                                merged = true;
                                break;
                            }
                        }
                    }
                    if (merged == false)
                        valueList.Add(newelement);
                }
            }
            catch
            {
                return null;
            }

            //Values should return now;
            return valueList;
        }
        /// <summary>
        /// The category an element should be reported under once the generated allowances are folded
        /// away. Reinforcement and the steel and timber connection allowances are carbon that belongs
        /// to the element they were worked out from, so they are reported against that element rather
        /// than as a category of their own. The generators record which element that was in the
        /// SubCategory, see CarboProject.getRCGroup and getConnectionGroup.
        /// </summary>
        private static string getMergedCategoryName(CarboElement cEl)
        {
            //An allowance with nothing to fold into keeps its own category, which is better than
            //collecting it under a blank name.
            if (CarboGroupCategories.IsGeneratedAllowance(cEl.Category) == true
                && string.IsNullOrEmpty(cEl.SubCategory) == false)
            {
                return cEl.SubCategory;
            }

            return cEl.Category;
        }

        /// <summary>
        /// This code will combine the
        /// </summary>
        /// <param name="projectElements"></param>
        /// <returns></returns>
        public static List<CarboDataPoint> ConvertResultTableToDataPointsMergedPlus(List<CarboElement> projectElements)
        {
            List<CarboDataPoint> valueList = new List<CarboDataPoint>();
            try
            {
                foreach (CarboElement cEl in projectElements)
                {
                    // Determine the name based on your priority rules
                    string targetName;
                    if (cEl.isSubstructure)
                        targetName = "Substructure";
                    else
                        targetName = getMergedCategoryName(cEl);

                    // Find if this bucket already exists in our list
                    var existingPoint = valueList.FirstOrDefault(p => p.Name == targetName);

                    if (existingPoint != null)
                    {
                        // Add the raw value (we divide by 1000 at the very end or sum it raw)
                        existingPoint.Value += (cEl.EC / 1000.0);
                    }
                    else
                    {
                        valueList.Add(new CarboDataPoint
                        {
                            Name = targetName,
                            Value = (cEl.EC / 1000.0)
                        });
                    }
                }
            }
            catch { return null; }
            return valueList;
        }
        /// <summary>
        /// By category dataset, however the generated allowance values (reinforcement, steel and
        /// timber connections) are added to the category of the element they belong to.
        /// </summary>
        /// <param name="projectElements"></param>
        /// <returns></returns>
        private static List<CarboDataPoint> ConvertResultTableToDataPointsMerged(List<CarboElement> projectElements)
        {
            List<CarboDataPoint> valueList = new List<CarboDataPoint>();
            try
            {
                //loop through each element
                foreach (CarboElement cEl in projectElements)
                {
                    CarboDataPoint newelement = new CarboDataPoint();

                    //Reinforcement and connection allowances go in with their parent element.
                    newelement.Name = getMergedCategoryName(cEl);

                    newelement.Value = (cEl.EC/1000);

                    bool merged = false;
                    //valueList.Add(newelement);

                    //Add a new datapoint, or add value if exists
                    if (valueList.Count > 0)
                    {
                        foreach (CarboDataPoint pp in valueList)
                        {
                            if (pp.Name == newelement.Name)
                            {
                                pp.Value += (newelement.Value);
                                merged = true;
                                break;
                            }
                        }
                    }
                    if (merged == false)
                        valueList.Add(newelement);

                }
            }
            catch
            {
                return null;
            }



            //Values should return now;
            return valueList;
        }

        /// <summary>
        /// Converts a ResultTable to a DataPoint List split between substructure and superstructure
        /// </summary>
        /// <param name="table"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static List<CarboDataPoint> ConvertResultTableToDataPointsSubSuperStruct(List<CarboElement> projectElements)
        {
            try
            {
                return projectElements
                    .GroupBy(c => c.isSubstructure ? "Substructure" : "Superstructure")
                    .Select(group => new CarboDataPoint
                    {
                        Name = group.Key,
                        // Perform the division once at the end of the sum to maintain precision
                        Value = group.Sum(c => c.EC) / 1000.0
                    })
                    .ToList();
            }
            catch
            {
                return null;
            }
        }


    }
}



