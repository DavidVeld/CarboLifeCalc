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
        /// This function uses the CarboProject Class to get a set of datapoints, it can then be called as
        /// CarboGraphResult = GetByMaterialMassChart().Calculate();
        /// </summary>
        /// <param name="carboProject">The Carbo Life Project</param>
        /// <returns>CarboGraphResult</returns>
        public static CarboGraphResult GetMaterialMassData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            IEnumerable<CarboElement> bufferList = carboProject.getElementsFromGroups();
            CarboGraphResult thisResult = new CarboGraphResult();

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
            IEnumerable<CarboElement> bufferList = carboProject.getElementsFromGroups();
            CarboGraphResult thisResult = new CarboGraphResult();

            try
            {

                thisResult.ValueName = "ECI";
                thisResult.Unit = "kgCO₂/m³";

                //This part collects the required data we need to build the graph later on.
                foreach (CarboElement carboElement in bufferList)
                {
                    CarboValues value = new CarboValues();
                    value.Id = carboElement.Id;
                    value.Value = (carboElement.EC_Cumulative / carboElement.Volume);
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
            try
            {
                thisResult.ValueName = "EC";
                thisResult.Unit = "tCO₂e";

                foreach (CarboGroup cgr in carboProject.getGroupList)
                {
                    //This part collects the required data we need to build the graph later on.
                    foreach (CarboElement carboElement in cgr.AllElements)
                    {
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
            IEnumerable<CarboElement> bufferList = carboProject.getElementsFromGroups();

            CarboGraphResult thisResult = new CarboGraphResult();

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
            CarboGraphResult result = new CarboGraphResult();

            thisResult.ValueName = "EC";
            thisResult.Unit = "tCO₂e";

            result.ValueName = "EC";
            result.Unit = "tCO₂e";

            try
            {
                carboProject.CalculateProject();
                List<CarboDataPoint> materialData = carboProject.getMaterialTotals();
                IEnumerable<CarboElement> bufferList = carboProject.getElementsFromGroups();



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



