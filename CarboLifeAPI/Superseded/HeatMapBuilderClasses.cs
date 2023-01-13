/*
 * Carbo Calc Copywrite 2023
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
    /// <summary>
    /// [Obsolete] To be deleted
    /// </summary>
    public static class HeatMapBuilderClasses
    {
        public static System.Drawing.Color Session_minOutColour { get; set; }
        public static System.Drawing.Color Session_maxOutColour { get; set; }
        public static System.Drawing.Color Session_minRangeColour { get; set; }
        public static System.Drawing.Color Session_midRangeColour { get; set; }
        public static System.Drawing.Color Session_maxRangeColour { get; set; }

        //The Values we need to construct the graph

        public static CarboGraphResult result;

        private static List<CarboValues> workingValues;
        private static double canvasWidth;
        private static double canvasHeight;

        //CanvasScale
        //The mins and maxes of the dataset
        private static double X_max;
        private static double X_min;
        private static double Y_max;
        private static double Y_min;

        //The Mins and Maxes of the user's filters:
        //private static double deviationFactor;
        private static double avgX;
        private static double minBoundX;
        private static double maxBoundX;
        private static double minXCutoff;
        private static double maxXCutoff;
        //These are the offsets of the graph in the canvas

        private static double Xorigin;
        private static double Yorigin;
        private static double border;
        private static double fontsize;
        private static double graphLength;
        private static double graphHeight;

        //The scale of the current graph;
        private static double scalex;
        private static double scaley;


        /// <summary>
        /// This function uses the CarboProject Class to get a set of datapoints, it can then be calles as
        /// CarboGraphResult = GetByMaterialMassChart().Calculate();
        /// </summary>
        /// <param name="carboProject">The Project of </param>
        /// <returns></returns>
        public static CarboGraphResult GetMaterialMassData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            List<CarboElement> bufferList = carboProject.getTemporaryElementListWithTotals();

            CarboGraphResult thisResult = new CarboGraphResult();

            thisResult.xName = "ECI";
            thisResult.yName = "Mass";

            //This part collects the required data we need to build the graph later on.
            foreach (CarboElement carboElement in bufferList)
            {
                CarboValues value = new CarboValues();
                value.xValue = carboElement.ECI_Total;
                value.yValue = carboElement.Mass;
                value.Id = carboElement.Id;
                thisResult.elementData.Add(value);
            }

            //set the values of which to calculate;
            result = thisResult;
            return thisResult;
        }

        public static CarboGraphResult GetMaterialVolumeData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            List<CarboElement> bufferList = carboProject.getTemporaryElementListWithTotals();

            CarboGraphResult thisResult = new CarboGraphResult();

            thisResult.xName = "ECI";
            thisResult.yName = "Mass";

            //This part collects the required data we need to build the graph later on.
            foreach (CarboElement carboElement in bufferList)
            {
                CarboValues value = new CarboValues();
                value.xValue = (carboElement.EC_Total / carboElement.Volume_Total);
                value.yValue = carboElement.Volume_Total;
                value.Id = carboElement.Id;
                thisResult.elementData.Add(value);
            }

            //set the values of which to calculate;
            result = thisResult;
            return thisResult;
        }

        public static CarboGraphResult GetPerGroupData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            CarboGraphResult thisResult = new CarboGraphResult();

            thisResult.xName = "EC";
            thisResult.yName = "ElementsInGroup";

            foreach (CarboGroup cgr in carboProject.getGroupList)
            {
                //This part collects the required data we need to build the graph later on.
                foreach (CarboElement carboElement in cgr.AllElements)
                {
                    CarboValues value = new CarboValues();
                    value.xValue = (cgr.EC);
                    value.yValue = cgr.AllElements.Count;
                    value.Id = carboElement.Id;

                    thisResult.elementData.Add(value);
                }
            }

            //set the values of which to calculate;
            result = thisResult;
            return thisResult;
        }

        public static CarboGraphResult GetPerElementData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            List<CarboElement> bufferList = carboProject.getTemporaryElementListWithTotals();

            CarboGraphResult thisResult = new CarboGraphResult();

            thisResult.xName = "EC";
            thisResult.yName = "Mass";

            //This part collects the required data we need to build the graph later on.
            foreach (CarboElement carboElement in bufferList)
            {
                CarboValues value = new CarboValues();
                value.xValue = (carboElement.EC_Total / 1000);
                value.yValue = carboElement.Mass;
                value.Id = carboElement.Id;
                thisResult.elementData.Add(value);
            }

            //set the values of which to calculate;
            result = thisResult;
            return thisResult;
        }

        public static CarboGraphResult GetMaterialTotalData(CarboProject carboProject)
        {
            //This is the most usefull set of Data To work with for now:
            CarboGraphResult thisResult = new CarboGraphResult();

            thisResult.xName = "EC";
            thisResult.yName = "ElementsInGroup";
            
            foreach (CarboGroup cgr in carboProject.getGroupList)
            {
                //This part collects the required data we need to build the graph later on.
                foreach (CarboElement carboElement in cgr.AllElements)
                {
                    CarboValues value = new CarboValues();
                    value.xValue = (cgr.EC);
                    value.yValue = cgr.AllElements.Count;
                    value.Id = carboElement.Id;

                    thisResult.elementData.Add(value);
                }
            }

            //set the values of which to calculate;
            result = thisResult;
            return thisResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actualWidth"></param>
        /// <param name="actualHeight"></param>
        /// <param name="deviationFact"></param>
        /// <param name="xminCutoff"></param>
        /// <param name="xmaxCutoff"></param>
        /// <returns></returns>
        public static CarboGraphResult GetScatterPlot(double actualWidth, double actualHeight, double xminCutoff = 0, double xmaxCutoff = 0)
        {
            //Set the values that define the bounds and scale of the plot.
            canvasWidth = actualWidth;
            canvasHeight = actualHeight;
            //deviationFactor = deviationFact;
            minXCutoff = xminCutoff;
            maxXCutoff = xmaxCutoff;
            result.UIData.Clear();

            try
            {

                //get the right colour range.
                SetColours();

                //remove items out of bounds (provided min and max)
                SetValidDataPoints();

                //Using the valid datapoints, not create the axis, and set the bounds. 
                if (workingValues != null && workingValues.Count > 0)
                {

                    //Get the axis
                    IList<UIElement> axisUIData = DrawAxis();
                    result.Add(axisUIData);

                    //Get the boundaries
                    IList<UIElement> boundariesUIData = CalculateBounds();
                    result.Add(boundariesUIData);

                    //colour the values;
                    result = ColourByValues();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return null;
            }

            return result;
        }

        private static void SetValidDataPoints()
        {
                try
                {

                workingValues = new List<CarboValues>();
                workingValues.Clear();
                    if (result != null)
                    {
                        if (result.elementData != null && result.elementData.Count > 0)
                        {
                            foreach (CarboValues cv in result.elementData)
                            {
                                if (cv.xValue > minXCutoff && cv.xValue < maxXCutoff)
                                {
                                    workingValues.Add(cv);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            
        }

        private static IList<UIElement> CalculateBounds()
        {
            IList<UIElement> thisResult = new List<UIElement>();
            IList<CarboValues> dataResult = workingValues;
            //Get the graph scale;


            //get the standard deviation
            //double standDevValue = CalculateStandardDeviation(dataResult);

            //avgX = dataResult.Average(item => item.xValue);
            //minBoundX = avgX - (standDevValue * deviationFactor);
            //maxBoundX = avgX + ( standDevValue * deviationFactor);

            double closestMin = getClosest(minXCutoff, dataResult, true);
            double closestMax = getClosest(maxXCutoff, dataResult, false);

            minBoundX = closestMin;
            maxBoundX = closestMax;

            //draw min and max cutoff:
            Line minCutoffLine = GetLine(minXCutoff, 0, minXCutoff, (Y_max * 1));
            minCutoffLine.Stroke = Brushes.Blue;
            minCutoffLine.StrokeThickness = 2;

            Line maxCutoffLine = GetLine(maxXCutoff, 0, maxXCutoff, (Y_max * 1));
            maxCutoffLine.Stroke = Brushes.Red;
            maxCutoffLine.StrokeThickness = 2;
            
            //The closest Values
            Line minValueLine = GetLine(closestMin, 0, closestMin, (Y_max * .5));
            minValueLine.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, Session_minRangeColour.R, Session_minRangeColour.G, Session_minRangeColour.B));
            minValueLine.StrokeThickness = 2;

            Line maxValueLine = GetLine(closestMax, 0, closestMax, (Y_max * .5));
            maxValueLine.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, Session_maxRangeColour.R, Session_maxRangeColour.G, Session_maxRangeColour.B));
            maxValueLine.StrokeThickness = 2;


            thisResult.Add(minCutoffLine);
            thisResult.Add(maxCutoffLine);
            thisResult.Add(maxValueLine);
            thisResult.Add(minValueLine);

            //The closest actual value:
            /*
            double avgset = 0;

            if (workingValues.Count > 0)
                avgset = workingValues.Average(item => item.xValue);

            Line avgSetline = GetLine(avgset, 0, avgset, Y_max);
            avgSetline.Stroke = Brushes.Orange;

            thisResult.Add(avgSetline);
            */
            return thisResult;
        }

        private static double getClosest(double valuetofind, IList<CarboValues> dataResult, bool findMin)
        {
            //First we need to make sure the list is sorted;
            List<CarboValues> SortedList = dataResult.OrderBy(o => o.xValue).ToList();

            double result = 0;
            double buffer;

            if (findMin)
                goto lowhigh;
            else
                goto highlow;

            lowhigh:
            return SortedList[0].xValue;
            //We're going up the list untill we find a value higher than the value then we stop
            //This is to find the lowest value in the list closest to the valuetofind
            for (int i = 0; i < SortedList.Count; i++)
            {
                buffer = SortedList[i].xValue;
                if (buffer < valuetofind)
                    result = buffer;
                else
                    break;
            }
            return result;

        highlow:
            return SortedList[SortedList.Count -1].xValue;

            //We#re going up the list untill we find a value higher than the value then we stop
            for (int i = SortedList.Count; i > 0; i--)
            {
                buffer = SortedList[i -1].xValue;
                if (buffer > valuetofind)
                    result = buffer;
                else
                    break;
            }
            return result;


        }

        /// <summary>
        /// draws a line on given coordinates bases on current scales, based for absolute values.
        /// </summary>
        /// <param name="p1x">Point 1 x</param>
        /// <param name="p1y">Point 1 y</param>
        /// <param name="p2x">Point 2 x</param>
        /// <param name="p2y">Point 3 y</param>
        /// <returns></returns>
        private static Line GetLine(double p1x, double p1y, double p2x, double p2y)
        {
            //Yaxis
            Line line = new Line();
            Thickness ythickness = new Thickness(0, 0, 0, 0);
            line.Margin = ythickness;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = 1.5;
            line.Stroke = System.Windows.Media.Brushes.Black;

            //StartPoint
            line.X1 = (p1x * scalex);
            line.Y1 = (p1y * scaley);

            //EndPoint
            line.X2 = (p2x * scalex);
            line.Y2 = (p2y * scaley);

            //Move to the offset relative to the axis:
            Canvas.SetLeft(line, Xorigin);
            Canvas.SetBottom(line, Yorigin);

            return line;
        }

        private static void SetColours()
        {
            //load deafult settings from a file in the future 

            Session_minOutColour = System.Drawing.Color.FromArgb(255, 0, 0, 255);
            Session_maxOutColour = System.Drawing.Color.FromArgb(255, 250, 0, 0);

            Session_minRangeColour = System.Drawing.Color.FromArgb(255, 141, 241, 41);
            Session_midRangeColour = System.Drawing.Color.FromArgb(255, 242, 116, 40);
            Session_maxRangeColour = System.Drawing.Color.FromArgb(255, 240, 40, 9);
        }

        private static IList<UIElement> DrawAxis()
        {
            IList<UIElement> thisResult = new List<UIElement>();
            IList<CarboValues> dataResult = result.elementData;

            //The Below sets the scale of the plot for all items:
            //Get the mins and maxes of the dataset
            X_max = dataResult.Max(t => t.xValue);
            X_min = dataResult.Min(t => t.xValue);
            Y_max = dataResult.Max(t => t.yValue);
            Y_min = dataResult.Min(t => t.yValue);

            //These are the offsets of the graph in the canvas
            Xorigin = 35;
            Yorigin = 35;
            border = 35;
            fontsize = 10;
            graphLength = canvasWidth - Xorigin - border;
            graphHeight = canvasHeight - Yorigin - border;

            //Some Data checks:
            if (X_min > 0)
                X_min = 0;
            X_max = Convert.ToInt32(X_max);

            //Call this for all items that need rotating 90degrees

            //Rotate the text that need rotating
            TransformGroup myTransformGroup = new TransformGroup();
            RotateTransform RT = new RotateTransform();
            RT.Angle = 90;
            myTransformGroup.Children.Add(RT);


            /// Calculate the scale between the points 
            double scaleLength = X_max - X_min;
            double scaleHeight = Y_max - Y_min;

            scalex = graphLength / scaleLength;
            scaley = graphHeight / scaleHeight;

            //Main Axis Lines:
            //Xaxis
            Line xline = GetLine(X_min, 0, X_max, 0);
            //Yaxis
            Line yline = GetLine(X_min, 0, 0, Y_max);

            ///Set a grid for the values
            /// Xaxis

            double div_X = X_max - X_min;
            double step = div_X / 10;
            for(double i = X_min; i <= Math.Round(X_max,3); i = Math.Round(i + step,3))
            {
                Line x_stepline = GetLine(i, 0, i, (Y_max * .1));
                x_stepline.StrokeThickness = 1;
                x_stepline.Stroke = System.Windows.Media.Brushes.Gray; 

                //value
                TextBlock xval = new TextBlock();
                xval.Text = Math.Round(i, 2).ToString("N");
                xval.Foreground = Brushes.Black;
                xval.FontSize = fontsize;
                xval.LayoutTransform = myTransformGroup;

                Canvas.SetLeft(xval, i * scalex + Xorigin -5);
                Canvas.SetBottom(xval, 10);

                thisResult.Add(x_stepline);
                thisResult.Add(xval);

            }

            
            TextBlock xtb = new TextBlock();
            xtb.Text = Convert.ToString("X (" + result.xName + ")");
            xtb.Foreground = Brushes.Black;
            xtb.FontSize = fontsize;
            Canvas.SetLeft(xtb, Xorigin +20);
            Canvas.SetBottom(xtb, 10);

            ///The DataValue
            TextBlock ytb = new TextBlock();
            ytb.Text = Convert.ToString("Y (" + result.yName + ")");
            ytb.Foreground = Brushes.Black;
            ytb.FontSize = fontsize;
            ytb.TextAlignment = TextAlignment.Right;
            Canvas.SetLeft(ytb, Xorigin - 20);
            Canvas.SetBottom(ytb, Yorigin + 25);
            ytb.LayoutTransform = myTransformGroup;

            TextBlock ymin = new TextBlock();
            ymin.Text = Convert.ToString(Math.Round(Y_min, 2).ToString("N"));
            ymin.Foreground = Brushes.Black;
            ymin.TextAlignment = TextAlignment.Right;

            ymin.Name = "ymin";
            ymin.FontSize = fontsize;

            ymin.TextAlignment = TextAlignment.Right;

            Canvas.SetLeft(ymin, Xorigin - 28);
            Canvas.SetBottom(ymin, Yorigin);

            TextBlock ymax = new TextBlock();
            ymax.Text = Convert.ToString(Math.Round(Y_max, 2).ToString("N"));
            ymax.Foreground = Brushes.Black;
            ymax.Name = "ymax";
            ymax.FontSize = fontsize;

            ymax.TextAlignment = TextAlignment.Right;
            Canvas.SetLeft(ymax, Xorigin - 28);
            Canvas.SetBottom(ymax, Yorigin + graphHeight);

            /*
            TextBlock xmin = new TextBlock();
            xmin.Text = Math.Round(X_min, 2).ToString("N");
            xmin.Foreground = Brushes.Black;
            xmin.FontSize = fontsize;
            Canvas.SetLeft(xmin, Xorigin);
            Canvas.SetBottom(xmin, 10);

            TextBlock xmax = new TextBlock();
            xmax.Text = Math.Round(X_max,2).ToString("N");
            xmax.Foreground = Brushes.Black;
            xmax.FontSize = fontsize;
            Canvas.SetLeft(xmax, Xorigin + graphLength);
            Canvas.SetBottom(xmax, 10);




            

            xmin.LayoutTransform = myTransformGroup;
            xmax.LayoutTransform = myTransformGroup;

            //Add them all to the canvas
            thisResult.Add(xmin);
            thisResult.Add(xmax);


            */

            thisResult.Add(ymin);
            thisResult.Add(ymax);

            thisResult.Add(xtb);
            thisResult.Add(ytb);

            thisResult.Add(xline);
            thisResult.Add(yline);
            return thisResult;
        }

        private static CarboGraphResult ColourByValues()
        {
            IList<UIElement> thisResult = new List<UIElement>();

            //Draw a little rectangle
            foreach (CarboValues cv in workingValues)
            {
                //GetTheColour:
                System.Drawing.Color colorBrush = GetColour(cv);

                Rectangle rect = new Rectangle();

                SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                mySolidColorBrush.Color = System.Windows.Media.Color.FromArgb(255, colorBrush.R, colorBrush.G, colorBrush.B);

                rect.Fill = mySolidColorBrush;
                rect.Stroke = mySolidColorBrush;

                rect.StrokeThickness = 3;
                rect.Width = 6;
                rect.Height = 6;

                double x = cv.xValue * scalex;
                double y = cv.yValue * scaley;

                Canvas.SetLeft(rect, Xorigin + x - 1);
                Canvas.SetBottom(rect, Yorigin + y - 1 );

                //add the value in the datapoint;
                cv.r = colorBrush.R;
                cv.g = colorBrush.G;
                cv.b = colorBrush.B;

                thisResult.Add(rect);
            }
            // Add the out-of bounds elements as required.

            result.Add(thisResult);

            return result;
        }

        private static System.Drawing.Color GetColour(CarboValues cv)
        {
            System.Drawing.Color colourResult = Utils.GetBlendedColor(maxBoundX, minBoundX, cv.xValue, Session_minRangeColour, Session_midRangeColour, Session_maxRangeColour);
            return colourResult;
        }

        private static double CalculateStandardDeviation(IList<CarboValues> values)
        {
            double result = 0;

            List<double> ValidValues = new List<double>();

            //Get a liost of valid values below the max and above the min cutoff;
            if(values.Count > 0)
            {
                foreach(CarboValues cv in values)
                {
                    if (cv.xValue > minXCutoff && cv.xValue < maxXCutoff)
                    {
                        ValidValues.Add(cv.xValue);
                     }
                }
            }

            if (ValidValues != null)
            {
                if (ValidValues.Count > 1)
                {
                    // Compute the average.     
                    double avg = ValidValues.Average();

                    // Perform the Sum of (value-avg)_2_2.      
                    double sum = ValidValues.Sum(d => Math.Pow(d - avg, 2));

                    // Put it all together.      
                    result = Math.Sqrt((sum) / (ValidValues.Count() - 1));
                }
            }

            return result;
        }


    }


}



