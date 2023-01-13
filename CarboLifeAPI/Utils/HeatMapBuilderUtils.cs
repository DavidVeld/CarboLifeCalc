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

namespace CarboLifeAPI
{
    /// <summary>
    /// [Obsolete] To be deleted
    /// </summary>
    public static class HeatMapBuilderUtils
    {
        public static System.Drawing.Color Session_minOutColour { get; set; }
        public static System.Drawing.Color Session_maxOutColour { get; set; }
        public static System.Drawing.Color Session_minRangeColour { get; set; }
        public static System.Drawing.Color Session_midRangeColour { get; set; }
        public static System.Drawing.Color Session_maxRangeColour { get; set; }


        private static double CalculateStandardDeviation(List<double> values)
        {
            double standardDeviation = 0;

            if (values != null)
            {
                if (values.Count > 1)
                {
                    // Compute the average.     
                    double avg = values.Average();

                    // Perform the Sum of (value-avg)_2_2.      
                    double sum = values.Sum(d => Math.Pow(d - avg, 2));

                    // Put it all together.      
                    standardDeviation = Math.Sqrt((sum) / (values.Count() - 1));
                }
            }

            return standardDeviation;
        }

        public static CarboProject CreateIntensityHeatMap(CarboProject carboProject, 
            System.Drawing.Color minOutColour, System.Drawing.Color maxOutColour, System.Drawing.Color minRangeColour, System.Drawing.Color midRangeColour, System.Drawing.Color maxRangeColour,
            double standardNr = 2)
        {
            
            carboProject.clearHeatmapAndValues();
            carboProject.CalculateProject();

            List<CarboElement> bufferList = carboProject.getTemporaryElementListWithTotals();
            List<double> valuelist = new List<double>();
            //List<double> valueRangelist = new List<double>();

            //Get a List<double> for only the calculated values.
            foreach (CarboElement carboElement in bufferList)
            {
                valuelist.Add(carboElement.ECI_Total);
            }

            double mean = valuelist.Average();
            double standardDev = CalculateStandardDeviation(valuelist);
            double max = mean + (standardDev * standardNr) ;
            double min = mean - (standardDev * standardNr);

            double maxRange = 0;
            double minRange = 0;

            bool first = true;

            foreach (double value in valuelist)
            {
                if (value >= min && value <= max)
                {
                    if (first == true)
                    {
                        maxRange = value;
                        minRange = value;
                        first = false;
                    }

                    //valueRangelist.Add(value);

                    if (value > maxRange)
                        maxRange = value;
                    if (value < minRange)
                        minRange = value;
                }
                else
                {
                    double v = value;
                }
            }
            string text = "Setting Values:" + Environment.NewLine
            + "mean: " + mean + Environment.NewLine
        + "standard dev: " + standardDev + Environment.NewLine
        + "max: " + max + Environment.NewLine
        + "min: " + min + Environment.NewLine + Environment.NewLine
        + "maxRange: " + maxRange + Environment.NewLine
        + "minRange: " + minRange + Environment.NewLine;

            //MessageBox.Show(text);
            //This should be embedded in a single iteration
            //maxRange = valueRangelist.Max();
            // minRange = valueRangelist.Min();

            //Possibly split the range to min and maxes.
            //See how big the extreems are...
            //
            foreach (CarboGroup grp in carboProject.getGroupList)
            {

                List<CarboElement> elements = grp.AllElements;
                foreach (CarboElement cel in elements)
                {
                    //See if this elements falls within the range.
                    double ECI = cel.ECI_Total;

                    if(ECI > maxRange)
                    //Over the max
                    {
                        // Apply Max Colour (Purple for now)
                        cel.r = maxOutColour.R;
                        cel.g = maxOutColour.G;
                        cel.b = maxOutColour.B;

                    }
                    else if(ECI < minRange)
                    {
                        // Apply Min Colour
                        cel.r = minOutColour.R;
                        cel.g = minOutColour.G;
                        cel.b = minOutColour.B;
                    }
                    else
                    {
                        //Within range, calculate value.
                        System.Drawing.Color groupColour = Utils.GetBlendedColor(maxRange, minRange, ECI, minRangeColour, midRangeColour, maxRangeColour);
                        cel.r = groupColour.R;
                        cel.g = groupColour.G;
                        cel.b = groupColour.B;
                    }
                }
            }

            return carboProject;
        }

        public static CarboProject CreateIntensityHeatMapVolume(CarboProject carboProject,
    System.Drawing.Color minOutColour, System.Drawing.Color maxOutColour, System.Drawing.Color minRangeColour, System.Drawing.Color midRangeColour, System.Drawing.Color maxRangeColour,
    double standardNr = 2)
        {

            carboProject.clearHeatmapAndValues();
            carboProject.CalculateProject();

            List<CarboElement> bufferList = carboProject.getTemporaryElementListWithTotals();
            List<double> valuelist = new List<double>();
            //List<double> valueRangelist = new List<double>();

            //Get a List<double> for only the calculated values.
            foreach (CarboElement carboElement in bufferList)
            {
                if(carboElement.Volume_Total > 0)
                    valuelist.Add(carboElement.EC_Total / carboElement.Volume_Total);
            }

            double mean = valuelist.Average();
            double standardDev = CalculateStandardDeviation(valuelist);
            double max = mean + (standardDev * standardNr);
            double min = mean - (standardDev * standardNr);

            double maxRange = 0;
            double minRange = 0;

            bool first = true;

            foreach (double value in valuelist)
            {
                if (value >= min && value <= max)
                {
                    if (first == true)
                    {
                        maxRange = value;
                        minRange = value;
                        first = false;
                    }

                    if (value > maxRange)
                        maxRange = value;
                    if (value < minRange)
                        minRange = value;
                }
            }
            string text = "Setting Values:" + Environment.NewLine
            + "mean: " + mean + Environment.NewLine
        + "standard dev: " + standardDev + Environment.NewLine
        + "max: " + max + Environment.NewLine
        + "min: " + min + Environment.NewLine + Environment.NewLine
        + "maxRange: " + maxRange + Environment.NewLine
        + "minRange: " + minRange + Environment.NewLine;

            //MessageBox.Show(text);
            //This should be embedded in a single iteration
            //maxRange = valueRangelist.Max();
            // minRange = valueRangelist.Min();

            //Possibly split the range to min and maxes.
            //See how big the extreems are...
            //
            foreach (CarboGroup grp in carboProject.getGroupList)
            {

                List<CarboElement> elements = grp.AllElements;
                foreach (CarboElement cel in elements)
                {
                    //See if this elements falls within the range.
                    double ECI_Volume = (cel.EC_Total / cel.Volume_Total);

                    if (ECI_Volume > maxRange)
                    //Over the max
                    {
                        // Apply Max Colour (Purple for now)
                        cel.r = maxOutColour.R;
                        cel.g = maxOutColour.G;
                        cel.b = maxOutColour.B;

                    }
                    else if (ECI_Volume < minRange)
                    {
                        // Apply Min Colour
                        cel.r = minOutColour.R;
                        cel.g = minOutColour.G;
                        cel.b = minOutColour.B;
                    }
                    else
                    {
                        //Within range, calculate value.
                        System.Drawing.Color groupColour = Utils.GetBlendedColor(maxRange, minRange, ECI_Volume, minRangeColour, midRangeColour, maxRangeColour);
                        cel.r = groupColour.R;
                        cel.g = groupColour.G;
                        cel.b = groupColour.B;
                    }
                }
            }

            return carboProject;
        }

        public static CarboProject CreateByGroupHeatMap(CarboProject carboProject, Color minOutColour, Color maxOutColour, Color minRangeColour, Color midRangeColour, Color maxRangeColour, double standardNr = 2)
        {
            carboProject.clearHeatmapAndValues();
            carboProject.CalculateProject();

            //List<CarboElement> bufferList = carboProject.getTemporaryElementListWithTotals();
            List<double> valuelist = new List<double>();
            List<double> valueRangelist = new List<double>();

            //Get a List<double> for the group totals
            foreach (CarboGroup grp in carboProject.getGroupList)
            {
                valuelist.Add(grp.EC);
            }


            double mean = valuelist.Average();
            double standardDev = CalculateStandardDeviation(valuelist);
            double max = mean + (standardDev * standardNr);
            double min = mean - (standardDev * standardNr);

            double maxRange = 0;
            double minRange = 0;

            bool first = true;

            foreach (double value in valuelist)
            {
                if (value >= min && value <= max)
                {
                    if (first == true)
                    {
                        maxRange = value;
                        minRange = value;
                        first = false;
                    }

                    //valueRangelist.Add(value);

                    if (value > maxRange)
                        maxRange = value;
                    if (value < minRange)
                        minRange = value;
                }
            }
            string text = "Setting Values:" + Environment.NewLine
            + "mean: " + mean + Environment.NewLine
        + "standard dev: " + standardDev + Environment.NewLine
        + "max: " + max + Environment.NewLine
        + "min: " + min + Environment.NewLine + Environment.NewLine
        + "maxRange: " + maxRange + Environment.NewLine
        + "minRange: " + minRange + Environment.NewLine;

            //MessageBox.Show(text);

            foreach (CarboGroup grp in carboProject.getGroupList)
            {
                List<CarboElement> elements = grp.AllElements;
                //See if this elements falls within the range.
                double EC = grp.EC;

                if (EC > maxRange)
                //Over the max
                {
                    // Apply Max Colour 
                    foreach (CarboElement cel in elements)
                    {
                        cel.r = maxOutColour.R;
                        cel.g = maxOutColour.G;
                        cel.b = maxOutColour.B;
                    }

                }
                else if (EC < minRange)
                {
                    // Apply Min Colour
                    foreach (CarboElement cel in elements)
                    {
                        cel.r = minOutColour.R;
                        cel.g = minOutColour.G;
                        cel.b = minOutColour.B;
                    }
                }
                else
                {
                    //Within range, calculate value.
                    System.Drawing.Color groupColour = Utils.GetBlendedColor(maxRange, minRange, EC, minRangeColour, midRangeColour, maxRangeColour);
                    foreach (CarboElement cel in elements)
                    {
                        cel.r = groupColour.R;
                        cel.g = groupColour.G;
                        cel.b = groupColour.B;
                    }
                }
            }
            return carboProject;

        }

        public static CarboProject CreateByElementHeatMap(CarboProject carboProject, Color minOutColour, Color maxOutColour, Color minRangeColour, Color midRangeColour, Color maxRangeColour, double standardNr = 2)
        {

            carboProject.clearHeatmapAndValues();
            carboProject.CalculateProject();

            List<CarboElement> bufferList = carboProject.getTemporaryElementListWithTotals();
            List<double> valuelist = new List<double>();
            List<double> valueRangelist = new List<double>();

            //Get a List<double> for only the calculated values.
            foreach (CarboElement carboElement in bufferList)
            {
                valuelist.Add(carboElement.EC_Total);
            }

            double mean = valuelist.Average();
            double standardDev = CalculateStandardDeviation(valuelist);
            double max = mean + (standardDev * standardNr);
            double min = mean - (standardDev * standardNr);

            double maxRange = 0;
            double minRange = 0;

            bool first = true;

            foreach (double value in valuelist)
            {
                if (value >= min && value <= max)
                {
                    if(first == true)
                    {
                        maxRange = value;
                        minRange = value;
                        first = false;
                    }

                    valueRangelist.Add(value);

                    if (value > maxRange)
                        maxRange = value;
                    if (value < minRange)
                        minRange = value;
                }
            }
            string text = "Setting Values:" + Environment.NewLine 
                + "mean: " + mean + Environment.NewLine
            +"standard dev: " + standardDev + Environment.NewLine
            +"max: " + max + Environment.NewLine
            +"min: " + min + Environment.NewLine + Environment.NewLine
            +"maxRange: " + maxRange + Environment.NewLine
            + "minRange: " + minRange + Environment.NewLine;

            //MessageBox.Show(text);
            //This should be embedded in a single iteration
            //maxRange = valueRangelist.Max();
            // minRange = valueRangelist.Min();

            //Possibly split the range to min and maxes.
            //See how big the extreems are...
            //
            foreach (CarboGroup grp in carboProject.getGroupList)
            {

                List<CarboElement> elements = grp.AllElements;
                foreach (CarboElement cel in elements)
                {
                    //See if this elements falls within the range.
                    double EC = cel.EC_Total;

                    if (EC > maxRange)
                    //Over the max
                    {
                        // Apply Max Colour (Purple for now)
                        cel.r = maxOutColour.R;
                        cel.g = maxOutColour.G;
                        cel.b = maxOutColour.B;

                    }
                    else if (EC < minRange)
                    {
                        // Apply Min Colour
                        cel.r = minOutColour.R;
                        cel.g = minOutColour.G;
                        cel.b = minOutColour.B;
                    }
                    else
                    {
                        //Within range, calculate value.
                        System.Drawing.Color groupColour = Utils.GetBlendedColor(maxRange, minRange, EC, minRangeColour, midRangeColour, maxRangeColour);
                        cel.r = groupColour.R;
                        cel.g = groupColour.G;
                        cel.b = groupColour.B;
                    }
                }
            }
            return carboProject;
        }
    }

    }



