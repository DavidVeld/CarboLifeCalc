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
        private static double deviationFactor;
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
        private static double realLength;
        private static double realHeight;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actualWidth"></param>
        /// <param name="actualHeight"></param>
        /// <param name="deviationFact"></param>
        /// <param name="xminCutoff"></param>
        /// <param name="xmaxCutoff"></param>
        /// <returns></returns>
        public static CarboGraphResult Calculate(double actualWidth, double actualHeight, double deviationFact = 1, double xminCutoff = 0, double xmaxCutoff = 0)
        {
            canvasWidth = actualWidth;
            canvasHeight = actualHeight;
            deviationFactor = deviationFact;
            minXCutoff = xminCutoff;
            maxXCutoff = xmaxCutoff;

            //get the right colour range.
            SetColours();

            //remove items out of bounds
            SetValidDataPoints();

            //Using the valid datapoints, not create the axis, and set the bounds. 
            if (workingValues.Count > 0)
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

            return result;
        }

        private static void SetValidDataPoints()
        {
            workingValues = new List<CarboValues>();

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

        private static IList<UIElement> CalculateBounds()
        {
            IList<UIElement> thisResult = new List<UIElement>();
            IList<CarboValues> dataResult = workingValues;
            //Get the graph scale;


            //get the standard deviation
            double standDevValue = CalculateStandardDeviation(dataResult);

            avgX = dataResult.Average(item => item.xValue);
            minBoundX = avgX - (standDevValue * deviationFactor);
            maxBoundX = avgX + ( standDevValue * deviationFactor);

            double closestMin = getClosest(minBoundX, dataResult, true);
            double closestMax = getClosest(minBoundX, dataResult, false);

            minBoundX = closestMin;
            maxBoundX = closestMax;

            //draw three lines on these locations
            //Yaxis
            Line minline = GetLine(minBoundX, 0, minBoundX, (Y_max * .1));
            minline.Stroke = Brushes.Blue;

            Line maxline = GetLine(maxBoundX, 0, maxBoundX, (Y_max * .1));
            maxline.Stroke = Brushes.Red;

            Line avgline = GetLine(avgX, 0, avgX, (Y_max * .1));
            avgline.Stroke = Brushes.Black;

            thisResult.Add(minline);
            thisResult.Add(maxline);
            thisResult.Add(avgline);

            double avgset = 0;

            if (workingValues.Count > 0)
                avgset = workingValues.Average(item => item.xValue);

            Line avgSetline = GetLine(avgset, 0, avgset, Y_max);
            avgSetline.Stroke = Brushes.Orange;

            thisResult.Add(avgSetline);

            return thisResult;
        }

        private static double getClosest(double valuetofind, IList<CarboValues> dataResult, bool roundUp)
        {
            double result = 0;
            double buffer;

            if (roundUp)
                goto lowhigh;
            else
                goto highlow;

            lowhigh:
            //We#re going up the list untill we find a value higher than the value then we stop
            for (int i = 0; i < dataResult.Count; i++)
            {
                buffer = dataResult[i].xValue;
                if (buffer < valuetofind)
                    result = buffer;
                else
                    break;
            }
            return result;

        highlow:
            //We#re going up the list untill we find a value higher than the value then we stop
            for (int i = dataResult.Count; i > 0; i--)
            {
                buffer = dataResult[i -1].xValue;
                if (buffer > valuetofind)
                    result = buffer;
                else
                    break;
            }
            return result;


        }

        /// <summary>
        /// draws a line on given coordinates bases on current scales.
        /// </summary>
        /// <param name="maxBoundX1"></param>
        /// <param name="v"></param>
        /// <param name="maxBoundX2"></param>
        /// <param name="y_max"></param>
        /// <returns></returns>
        private static Line GetLine(double p1x, int p1y, double p2x, double p2y)
        {
            //Yaxis
            Line line = new Line();
            Thickness ythickness = new Thickness(0, 0, 0, 0);
            line.Margin = ythickness;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = 1;
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

            Session_minOutColour = System.Drawing.Color.FromArgb(255, 93, 140, 140);
            Session_maxOutColour = System.Drawing.Color.FromArgb(255, 163, 87, 82);

            Session_minRangeColour = System.Drawing.Color.FromArgb(255, 93, 140, 140);
            Session_midRangeColour = System.Drawing.Color.FromArgb(255, 222, 146, 80);
            Session_maxRangeColour = System.Drawing.Color.FromArgb(255, 163, 87, 82);
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
            realLength = canvasWidth - Xorigin - border;
            realHeight = canvasHeight - Yorigin - border;

            /// Calculate the scale between the points 
            double scaleLength = X_max - X_min;
            double scaleHeight = Y_max - Y_min;

            scalex = realLength / scaleLength;
            scaley = realHeight / scaleHeight;

            //Xaxis
            Line xline = new Line();
            Thickness xthickness = new Thickness(0, 0, 0, 0);
            xline.Margin = xthickness;
            xline.Visibility = System.Windows.Visibility.Visible;
            xline.StrokeThickness = 1;
            xline.Stroke = System.Windows.Media.Brushes.Black;

            //StartPoint
            xline.X1 = 0; 
            xline.Y1 = 0;

            //EndPoint
            xline.X2 = realLength;
            xline.Y2 = 0;

            Canvas.SetLeft(xline, Xorigin);
            Canvas.SetBottom(xline, Yorigin);

            ///The DataValue
            /// Xaxis
            TextBlock xtb = new TextBlock();
            xtb.Text = Convert.ToString("X (" + result.xName + ")");
            xtb.Foreground = Brushes.Black;
            xtb.FontSize = fontsize;
            Canvas.SetLeft(xtb, Xorigin +20);
            Canvas.SetBottom(xtb, 10);

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
            Canvas.SetLeft(xmax, Xorigin + realLength);
            Canvas.SetBottom(xmax, 10);

            //Yaxis
            Line yline = new Line();
            Thickness ythickness = new Thickness(0, 0, 0, 0);
            yline.Margin = ythickness;
            yline.Visibility = System.Windows.Visibility.Visible;
            yline.StrokeThickness = 1;
            yline.Stroke = System.Windows.Media.Brushes.Black;

            //StartPoint
            yline.X1 = 0; 
            yline.Y1 = 0;

            //EndPoint
            yline.X2 = 0;
            yline.Y2 = realHeight;

            Canvas.SetLeft(yline, Xorigin);
            Canvas.SetBottom(yline, Yorigin);

            ///The DataValue
            TextBlock ytb = new TextBlock();
            ytb.Text = Convert.ToString("Y (" + result.yName + ")");
            ytb.Foreground = Brushes.Black;
            ytb.FontSize = fontsize;
            ytb.TextAlignment = TextAlignment.Right;
            Canvas.SetLeft(ytb, Xorigin - 20);
            Canvas.SetBottom(ytb, Yorigin + 25);

            TextBlock ymin = new TextBlock();
            ymin.Text = Convert.ToString (Math.Round(Y_min, 2).ToString("N"));
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
            Canvas.SetBottom(ymax, Yorigin + realHeight);

            //Rotate the text that need rotating
            TransformGroup myTransformGroup = new TransformGroup();
            RotateTransform RT = new RotateTransform();
            RT.Angle = 90;
            myTransformGroup.Children.Add(RT);

            xmin.LayoutTransform = myTransformGroup;
            xmax.LayoutTransform = myTransformGroup;
            ytb.LayoutTransform = myTransformGroup;

            //Add them all to the canvas
            thisResult.Add(xtb);
            thisResult.Add(xmin);
            thisResult.Add(xmax);
            thisResult.Add(xline);

            thisResult.Add(ytb);
            thisResult.Add(ymin);
            thisResult.Add(ymax);
            thisResult.Add(yline);

            return thisResult;
        }

        private static CarboGraphResult ColourByValues()
        {
            IList<UIElement> thisResult = new List<UIElement>();

            //Draw a little rectangle
            foreach (CarboValues cv in result.elementData)
            {
                //GetTheColour:
                System.Drawing.Color colorBrush = GetColour(cv);

                Rectangle rect = new Rectangle();

                SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                mySolidColorBrush.Color = System.Windows.Media.Color.FromArgb(colorBrush.R, colorBrush.G, colorBrush.B, 255);

                rect.Fill = mySolidColorBrush;
                rect.Stroke = mySolidColorBrush;

                rect.StrokeThickness = 2;
                rect.Width = 3;
                rect.Height = 3;

                double x = cv.xValue * scalex;
                double y = cv.yValue * scaley;

                Canvas.SetLeft(rect, Xorigin + x - 1);
                Canvas.SetBottom(rect, Yorigin + y - 1 );

                thisResult.Add(rect);
            }

            //Marker for Origin
            /*
            Rectangle rectt = new Rectangle();
            rectt.Stroke = System.Windows.Media.Brushes.Red;
            rectt.StrokeThickness = 1;
            rectt.Width = 3;
            rectt.Height = 3;
            Canvas.SetLeft(rectt, Xorigin + 0);
            Canvas.SetBottom(rectt, Yorigin + 0);
            thisResult.Add(rectt);


            //Marker for Extreme
            Rectangle rectE = new Rectangle();
            rectE.Stroke = System.Windows.Media.Brushes.Red;
            rectE.StrokeThickness = 1;
            rectE.Width = 3;
            rectE.Height = 3;

            Canvas.SetLeft(rectE, Xorigin + ((X_max - X_min) * scalex) - 1);
            Canvas.SetBottom(rectE, Yorigin + ((Y_max - Y_min) * scaley) - 1);
            thisResult.Add(rectE);
            */
            //Add items to the UIclass
            result.Add(thisResult);

            return result;
        }

        private static System.Drawing.Color GetColour(CarboValues cv)
        {
            System.Drawing.Color colourResult = Utils.GetBlendedColor(X_max, X_min, cv.xValue, Session_minRangeColour, Session_midRangeColour, Session_maxRangeColour);
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



