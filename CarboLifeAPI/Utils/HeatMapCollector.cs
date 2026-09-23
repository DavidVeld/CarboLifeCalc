/*
 * Carbo Calc Copywrite 2023
 * This static class will generate a bar chart WPF UIElements from a CarboProject
 * First a valid set of data needs to be collected in using a CarboProjectElement
 * Secondly a the data can be trimmed if required
 * Finally the UIElements can be extracted as CarboResultClass
 */
using CarboLifeAPI.Data;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.XPath;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace CarboLifeAPI
{

    public static class HeatMapCollector
    {
        /// <summary>
        /// The grouped elements whose carbon is part of the project total.
        ///
        /// An element ticked out of the calculation, or substructure while substructure is off,
        /// is left out of the heat map altogether rather than coloured as zero: zero would read
        /// as "this element has no carbon", when the truth is "this element is not counted". Left
        /// out, it keeps its own Revit graphics and stands apart from the coloured ones.
        /// </summary>
        private static IEnumerable<CarboElement> getCountedElements(CarboProject carboProject)
        {
            return carboProject.getElementsFromGroups()
                .Where(ce => ce.countsInCalculation(carboProject.calculateSubStructure));
        }

        /// <summary>
        /// The ids of elements no part of which is counted. An element split over two groups with
        /// one part counted is still on the heat map, so it is not listed here.
        /// </summary>
        private static List<Int64> getExcludedIds(CarboProject carboProject)
        {
            HashSet<Int64> counted = new HashSet<Int64>(getCountedElements(carboProject).Select(ce => ce.Id));

            return carboProject.getElementsFromGroups()
                .Select(ce => ce.Id)
                .Where(id => counted.Contains(id) == false)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// This function uses the CarboProject Class to get a set of datapoints, it can then be called as
        /// CarboGraphResult = GetByMaterialMassChart().Calculate();
        /// </summary>
        /// <param name="carboProject">The Carbo Life Project</param>
        /// <returns>CarboGraphResult</returns>
        public static CarboGraphResult GetMaterialMassData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            IEnumerable<CarboElement> bufferList = getCountedElements(carboProject);
            CarboGraphResult thisResult = new CarboGraphResult();
            thisResult.excludedIds = getExcludedIds(carboProject);

            try
            {
                thisResult.ValueName = "ECI";
                thisResult.Unit = "kgCO₂/kg";

                //This part collects the required data we need to build the graph later on.
                foreach (CarboElement carboElement in bufferList)
                {
                    CarboValues value = new CarboValues();

                    value.Id = carboElement.Id;
                    value.Value = carboElement.ECI_Cumulative;
                    value.ValueName = carboElement.CarboMaterialName;
                    value.ValueCategory = carboElement.Category;

                    thisResult.entireProjectData.Add(value);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return thisResult;
            }
            //set the values of which to calculate;
            return thisResult;
        }

        public static CarboGraphResult GetMaterialVolumeData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            IEnumerable<CarboElement> bufferList = getCountedElements(carboProject);
            CarboGraphResult thisResult = new CarboGraphResult();
            thisResult.excludedIds = getExcludedIds(carboProject);

            try
            {

                thisResult.ValueName = "ECI";
                thisResult.Unit = "kgCO₂/m³";

                //This part collects the required data we need to build the graph later on.
                foreach (CarboElement carboElement in bufferList)
                {
                    //An element without a volume has no carbon density: dividing anyway yields infinity,
                    //which then sets the scale of the whole graph.
                    if (carboElement.Volume_Cumulative <= 0)
                        continue;

                    //Both cumulative: the carbon of every part of this Revit element over the volume
                    //of every part. Revit holds one colour per element id, so a layered wall has to
                    //get one value. It used to be divided by this part's own Volume, which gave each
                    //layer of the same wall a different value (a concrete and insulation wall
                    //came out at 262.5 and 525 instead of 175) and whichever reached Revit last
                    //won. Volume_Cumulative is the adjusted volume, which is also what keeps
                    //reinforcement right: the rebar part is a copy of the concrete element, so its
                    //Volume is the concrete's again, while its Volume_Total is the steel.
                    CarboValues value = new CarboValues();
                    value.Id = carboElement.Id;
                    value.Value = (carboElement.EC_Cumulative / carboElement.Volume_Cumulative);
                    value.ValueName = carboElement.CarboMaterialName;
                    value.ValueCategory = carboElement.Category;

                    thisResult.entireProjectData.Add(value);
                }

                //set the values of which to calculate;
                return thisResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return thisResult;
            }
        }

        public static CarboGraphResult GetPerGroupData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            CarboGraphResult thisResult = new CarboGraphResult();
            thisResult.excludedIds = getExcludedIds(carboProject);
            try
            {
                thisResult.ValueName = "EC";
                thisResult.Unit = "tCO₂e";

                foreach (CarboGroup cgr in carboProject.getGroupList)
                {
                    //This part collects the required data we need to build the graph later on.
                    foreach (CarboElement carboElement in cgr.AllElements)
                    {
                        if (carboElement.countsInCalculation(carboProject.calculateSubStructure) == false)
                            continue;

                        CarboValues value = new CarboValues();
                        value.Id = carboElement.Id;
                        value.Value = cgr.EC;
                        value.ValueName = cgr.MaterialName;
                        value.ValueCategory = cgr.Category;
                        thisResult.entireProjectData.Add(value);
                    }
                }

                //set the values of which to calculate;
                return thisResult;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return thisResult;
            }
        }

        public static CarboGraphResult GetPerElementData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            IEnumerable<CarboElement> bufferList = getCountedElements(carboProject);

            CarboGraphResult thisResult = new CarboGraphResult();
            thisResult.excludedIds = getExcludedIds(carboProject);

            thisResult.ValueName = "EC";
            thisResult.Unit = "tCO₂e";

            try
            {
                //This part collects the required data we need to build the graph later on.
                foreach (CarboElement carboElement in bufferList)
                {
                    CarboValues value = new CarboValues();
                    value.Id = carboElement.Id;
                    value.GUID = carboElement.GUID;
                    value.Value = carboElement.EC_Cumulative;
                    value.ValueName = carboElement.CarboMaterialName;
                    value.ValueCategory = carboElement.Category;

                    thisResult.entireProjectData.Add(value);
                }
                //set the values of which to calculate;
                return thisResult;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return thisResult;
            }

        }

        public static CarboGraphResult GetMaterialTotalData(CarboProject carboProject)
        {
            //make sure all carbo materials are written in the elements;
            //This is the most usefull set of Data To work with for now:
            CarboGraphResult thisResult = new CarboGraphResult();
            thisResult.excludedIds = getExcludedIds(carboProject);
            CarboGraphResult result = new CarboGraphResult();
            result.excludedIds = thisResult.excludedIds;

            thisResult.ValueName = "EC";
            thisResult.Unit = "tCO₂e";

            result.ValueName = "EC";
            result.Unit = "tCO₂e";

            try
            {
                carboProject.CalculateProject();
                List<CarboDataPoint> materialData = carboProject.getMaterialTotals();
                IEnumerable<CarboElement> bufferList = getCountedElements(carboProject);



                //List<CarboDataPoint> materialist = carboProject.getMaterialTotals();

                //add the elements per material;

                foreach (CarboDataPoint cdp in materialData)
                {
                    foreach (CarboElement carboElement in bufferList)
                    {
                        if (carboElement.CarboMaterialName == cdp.Name)
                        {
                            CarboValues value = new CarboValues();
                            value.Id = carboElement.Id;
                            value.Value = cdp.Value;
                            value.ValueName = carboElement.CarboMaterialName;
                            value.ValueCategory = carboElement.Category;

                            thisResult.entireProjectData.Add(value);
                        }
                    }

                }

                //Combine if materials were combined

                foreach (CarboValues value in thisResult.entireProjectData)
                {
                    bool exists = false;

                    foreach (CarboValues valueNew in result.entireProjectData)
                    {
                        if(valueNew.Id == value.Id)
                        {
                            //This element has already a material assigned, add the values
                            valueNew.Value += value.Value;
                            valueNew.ValueName = valueNew.ValueName + " + " + value.ValueName;
                            exists = true;
                        }
                    }

                    if (exists == false)
                    {
                        result.entireProjectData.Add(value);
                    }

                }


            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

            return result;
        }

    }


}



