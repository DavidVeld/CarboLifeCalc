/*
 * Carbo Calc Copywrite 2023
 * This static class will generate a bar chart WPF UIElements from a CarboProject
 * First a valid set of data needs to be collected in using a CarboProjectElement
 * Secondly a the data can be trimmed if required
 * Finally the UIElements can be extracted as CarboResultClass
 */
using CarboLifeAPI.Data;
using Microsoft.Office.Interop.Excel;
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
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.XPath;

namespace CarboLifeAPI
{
    public static class CarboTimeLine
    {
        private static CarboProject project;
        private static bool calcSequestration;
        private static bool calcEnergy;
        private static bool calcDemolition;
        private static int designLife;

        /// <summary>
        /// This Function returns a list of datapoints each point being a point in time and the embodied carbon that the project contains, it iterated through the project per year and collates the date
        /// </summary>
        /// <param name="_projectData">the Project</param>
        /// <param name="calcSequestration"></param>
        /// <param name="calcEnergy"></param>
        /// <param name="calcDemolition"></param>
        /// <returns></returns>
        public static IList<CarboDataPoint> GetTimeLineDataPoints(CarboProject _projectData, bool _calcSequestration, bool _calcEnergy, bool _calcDemolition)
        {
            project = _projectData;
            calcSequestration = _calcSequestration;
            calcEnergy = _calcEnergy;
            calcDemolition = _calcDemolition;
            designLife = _projectData.designLife;


            IList<CarboDataPoint> result = new List<CarboDataPoint>();


            if (project != null && designLife > 1) ;
            {
                project.CalculateProject();

                for(int i = 0; i < designLife + 5; i++)
                {
                    CarboDataPoint dataPoint = new CarboDataPoint();
                    dataPoint = GetDataFromYear(i);
                    result.Add(dataPoint);
                }
            }

            return result;
        }

        private static CarboDataPoint GetDataFromYear(int i)
        {
            CarboDataPoint result = new CarboDataPoint();

            double a1a5Factor = 0;
            double seqFactor = 0;
            double energyFactor = 0;
            double demolitionFactor = 0;


            //We always use the a1a5factor
            if(i >= 0)
            {
                // This will return the A1-A3, A4 and A5 carbon costs 
                a1a5Factor = 1;
            }

            if(i > 1)
            {
                // This wil interpolate the between Construction, 
                energyFactor = i / designLife;
            }


            if(i >= designLife)
            {
                // The end of life bit
                demolitionFactor = 1;
            }


            //Calulate each item
            project.getPhaseTotals();
            ObservableCollection<CarboGroup> groupList = project.getGroupList;

            double totalECAcumulated = 0;

            //Get all values;
            foreach(CarboGroup group in groupList)
            {
                //Get the total A1-A3
                totalECAcumulated += (group.getTotalA1A3 * a1a5Factor);

                //Get total A4
                totalECAcumulated += (group.getTotalA4 * a1a5Factor);

                //Get Toal A5
                totalECAcumulated += ((group.getTotalA5 + project.A5Global) * a1a5Factor);


                //ignore if not selected:
                if (calcEnergy == false)
                    energyFactor = 0;
                //Get EnergyPerElements
                totalECAcumulated += (group.getTotalB1B7 * energyFactor); //THIS IS ONLY GROUP SPECIFIC, GLOBAL IS DOE LATER

                //get the sequestration

                int seqPeriod = group.Material.materialSeqProperties.sequestrationPeriod;
                seqFactor = (double)i / (double)seqPeriod;
                if (seqFactor > 1)
                    seqFactor = 1;

                //ignore if not selected:
                if (calcSequestration == false)
                    seqFactor = 0;


                totalECAcumulated += (group.getTotalSeq * seqFactor);


                //Get Demo
                totalECAcumulated += ((group.getTotalC1C4 + project.C1Global) * demolitionFactor);

            }

            //Get the Energy for year "i"
            if (calcEnergy == true)
            {
                double energyValue = project.energyProperties.getTotalValue(i);

                totalECAcumulated += (energyValue * 1);
            }

            result.Name = i.ToString();
            result.Value = totalECAcumulated;

            return result;

        }
    }


}



