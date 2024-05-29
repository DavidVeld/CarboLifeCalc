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
    /// <summary>
    /// [Obsolete] To be rewritten
    /// </summary>
    public static class HeatMapBarBuilder
    {

        //The Values we need to construct the graph

        public static CarboGraphResult projectData; //as instance so it can be called throughout the project.
        private static List<double> uniqueValues; //this List contains a simple set of unique values used for the final bar chart,

        private static double canvasWidth;
        private static double canvasHeight;

        private static CarboColourPreset colourSettings;

        //CanvasScale
        //The mins and maxes of the dataset
        private static double BarCount; //nr of bars
        private static double ValueMin;
        private static double ValueMax;

        //Variables For the graph
        private static double Xorigin;
        private static double Yorigin;
        private static double border;
        private static double fontsize;
        private static double graphLength;
        private static double graphHeight;

        //The scale of the current graph;
        private static double scalex;
        private static double scaley;
        private static double barwidth;

        /// <summary>
        /// This Function will return a bar-chart based on the provided projectdata, min and max and settings, it will also give the colour code for each element.
        /// </summary>
        /// <param name="actualWidth"></param>
        /// <param name="actualHeight"></param>
        /// <param name="deviationFact"></param>
        /// <param name="xminCutoff"></param>
        /// <param name="xmaxCutoff"></param>
        /// <returns></returns>
        public static (CarboGraphResult, IList<UIElement>) GetBarGraph(CarboGraphResult _projectData, double _canvasWidth, double _canvasHeight, CarboColourPreset _colourTemplate)
        {
            //Set the values that define the bounds and scale of the plot.
            projectData = _projectData;
            canvasWidth = _canvasWidth;
            canvasHeight = _canvasHeight;
            colourSettings = _colourTemplate;

            IList<UIElement> graph = new List<UIElement>();

            try
            {
                if(_canvasWidth <= 0 || _canvasHeight <= 0)
                    return (projectData, graph);

                //get the right colour range.
                //SetColours();

                //Set the Scale of the graph
                SetScales();
                //Using the valid datapoints, not create the axis, and set the bounds. 
                if (projectData.validData != null && projectData.validData.Count > 0)
                {
                    //Draw the axis
                    IList<UIElement> axisUIData = DrawAxis();
                    graph = AddList(graph, axisUIData);

                    //Draw the bars
                    IList<UIElement> bars = DrawBars();
                    graph = AddList(graph, bars);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return (null, null);
            }

            return (projectData, graph);
        }
        
        private static void SetColours()
        {
            //load deafult settings from a file in the future 
            /*
            Session_minOutColour = System.Drawing.Color.FromArgb(255, 0, 0, 255);
            Session_maxOutColour = System.Drawing.Color.FromArgb(255, 250, 0, 0);

            Session_minRangeColour = System.Drawing.Color.FromArgb(255, 141, 241, 41);
            Session_midRangeColour = System.Drawing.Color.FromArgb(255, 242, 116, 40);
            Session_maxRangeColour = System.Drawing.Color.FromArgb(255, 240, 40, 9);
            */
        }
        private static void SetScales()
        {
            IList<CarboValues> dataResult = projectData.validData;


            //Get the mins and maxes of the dataset, the unique values to plot and the min and maxes to find the scale;
            uniqueValues = projectData.GetUniqueValues();
            
            //The Below sets the scale of the plot for all items:
            BarCount = uniqueValues.Count;
            ValueMax = projectData.getMaxValue();
            ValueMin = projectData.getMinValue();

            //These are the offsets of the graph in the canvas
            Xorigin = 35;
            Yorigin = 35;
            border = 35;
            fontsize = 10;
            graphLength = canvasWidth - Xorigin - border;
            graphHeight = canvasHeight - Yorigin - border;

            //The Values Always starts at 0 ??
            if(ValueMin > 0)
                ValueMin = 0;

            //Round Higest up
            ValueMax = Math.Round(ValueMax, 3);

            /// Calculate the scale between the points 
            double scaleLength = BarCount;
            double scaleHeight = ValueMax;

            //Now set the scales with which we cal plot the graph
            scalex = graphLength / scaleLength;
            scaley = graphHeight / scaleHeight;
            barwidth = scalex * 1;
        }
        private static IList<UIElement> DrawAxis()
        {
            IList<UIElement> thisResult = new List<UIElement>();

            //Call this for all items that need rotating 90degrees
            TransformGroup myTransformGroup = new TransformGroup();
            RotateTransform RT = new RotateTransform();
            RT.Angle = 90;
            myTransformGroup.Children.Add(RT);

            //Main Axis Lines:
            //Xaxis
            Line xline = GetLine(0, 0, BarCount, 0);
            //Yaxis
            Line yline = GetLine(0, 0, 0, ValueMax);

            ///Set a grid for the values
            /// Yaxis
            double div_Y = ValueMax - ValueMin;
            double stepy = div_Y / 10;

            for(double i = 0; i <= Math.Round(ValueMax, 3); i = Math.Round(i + stepy, 3))
            {
                Line y_stepline = GetLine(0, i, BarCount, i);
                y_stepline.StrokeThickness = 1;
                y_stepline.Stroke = System.Windows.Media.Brushes.LightGray; 

                //value
                TextBlock yval = new TextBlock();
                yval.Text = Math.Round(i, 2).ToString("N");
                yval.Foreground = Brushes.Black;
                yval.FontSize = fontsize;
               // yval.LayoutTransform = myTransformGroup;

                Canvas.SetLeft(yval, 10);
                Canvas.SetBottom(yval, (i * scaley) + (Yorigin - 5));

                thisResult.Add(y_stepline);
                thisResult.Add(yval);
            }

            TextBlock xtb = new TextBlock();
            xtb.Text = Convert.ToString("");
            xtb.Foreground = Brushes.Black;
            xtb.FontSize = fontsize;
            Canvas.SetLeft(xtb, Xorigin +20);
            Canvas.SetBottom(xtb, 10);

            ///The DataValue
            TextBlock ytb = new TextBlock();
            ytb.Text = Convert.ToString("Y (" + projectData.ValueName + " " + projectData.Unit +")");
            ytb.Foreground = Brushes.Black;
            ytb.FontSize = fontsize;
            ytb.TextAlignment = TextAlignment.Right;
            Canvas.SetLeft(ytb, Xorigin - 40);
            Canvas.SetBottom(ytb, Yorigin + 25);
            ytb.LayoutTransform = myTransformGroup;

            thisResult.Add(xtb);
            thisResult.Add(ytb);

            thisResult.Add(xline);
            thisResult.Add(yline);

            return thisResult;
        }
        private static IList<UIElement> DrawBars()
        {
            IList<UIElement> thisResult = new List<UIElement>();
            uniqueValues.Sort();

            //Call this for all items that need rotating 90degrees
            TransformGroup myTransformGroup = new TransformGroup();
            RotateTransform RT = new RotateTransform();
            RT.Angle = 90;
            myTransformGroup.Children.Add(RT);

            //Draw a bar for each unique double starting from low:
            if (uniqueValues.Count > 0)
            {
                int p = 0;

                for (int i = 0; i < uniqueValues.Count; i++)
                {
                    double value = uniqueValues[i];

                    //get the list of results that match the bar
                    List<CarboValues> listOfResults = new List<CarboValues>();
                    string valueString = "";
                    foreach(CarboValues cv in projectData.validData)
                    {
                        if (Math.Round(cv.Value, 3) == Math.Round(value, 3))
                        {
                            listOfResults.Add(cv);
                        }
                    }
                   // Use only unique values
                    List<string> listOfResultsUnique = listOfResults.Select(item => item.ValueName).Distinct().ToList();
                    foreach (string str in listOfResultsUnique)
                    {
                        valueString += str + Environment.NewLine;
                    }
                    valueString = valueString.ToString().TrimEnd('\r', '\n');


                    //only draw the notes on each bas if <20
                    bool drawtags = false;
                    if (p > 10)
                        p = 0;
                    if (uniqueValues.Count < 20)
                    {
                        drawtags = true;
                    }
                    else if(i == 0 || i == uniqueValues.Count - 1 || p == 10)
                    {
                        drawtags = true;

                    }
                    else
                    {
                        drawtags = false;
                    }

                    //Draw the shape:
                    //GetTheColour:
                    System.Drawing.Color colorBrush = GetColour(i, uniqueValues.Count);

                    Rectangle rect = new Rectangle();

                    SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                    mySolidColorBrush.Color = System.Windows.Media.Color.FromArgb(255, colorBrush.R, colorBrush.G, colorBrush.B);

                    rect.Fill = mySolidColorBrush;
                    rect.Stroke = System.Windows.Media.Brushes.Black;

                    rect.StrokeThickness = 1;
                    rect.Width = barwidth;
                    //Correct for -
                    if (value >= 0)
                        rect.Height = value * scaley;
                    else
                        rect.Height = (value * scaley * -1);


                    Canvas.SetLeft(rect, Xorigin + (i * barwidth));
                    Canvas.SetBottom(rect, Yorigin);
                    thisResult.Add(rect);

                    //if there are too many bars, don't show the legend;
                    if (drawtags == true)
                    {
                        //Set a text Value
                        TextBlock label = new TextBlock();
                        label.Text = Convert.ToString(Math.Round(value, 3)) + Environment.NewLine + valueString;
                        label.Foreground = Brushes.Black;
                        label.FontSize = fontsize;
                        if (i == uniqueValues.Count - 1)
                        {
                            //last label
                            label.TextAlignment = TextAlignment.Left;
                            Canvas.SetBottom(label, value * scaley - 75);
                        }
                        else
                        {
                            label.TextAlignment = TextAlignment.Right;
                            Canvas.SetBottom(label, value * scaley + 40);
                        }
                        label.VerticalAlignment = VerticalAlignment.Center;
                        label.HorizontalAlignment = HorizontalAlignment.Center;

                        Canvas.SetLeft(label, Xorigin + (i * barwidth) + .20 * barwidth);
                       // Canvas.SetBottom(label, value * scaley + 40);

                        //Canvas.SetBottom(label, -100);

                        label.LayoutTransform = myTransformGroup;

                        thisResult.Add(label);
                    }
                    //Colour all the CarboValues so they can be moved to Revit.
                    foreach (CarboValues cv in projectData.validData)
                    {
                        double roundedValue = Math.Round(cv.Value, 3);

                        if (roundedValue == value)
                        {
                            //Apply the colour to each element in the workingValueList:
                            //add the value in the datapoint;
                            cv.r = colorBrush.R;
                            cv.g = colorBrush.G;
                            cv.b = colorBrush.B;
                        }
                    }

                    p++;
                }

                foreach (CarboValues cv in projectData.outOfBoundsMaxData)
                {
                    //Apply the colour to each element in the workingValueList:
                    //add the value in the datapoint;
                    cv.r = colourSettings.outmax.r;
                    cv.g = colourSettings.outmax.g;
                    cv.b = colourSettings.outmax.b;

                }

                foreach (CarboValues cv in projectData.outOfBoundsMinData)
                {
                    //Apply the colour to each element in the workingValueList:
                    //add the value in the datapoint;
                    cv.r = colourSettings.outmin.r;
                    cv.g = colourSettings.outmin.g;
                    cv.b = colourSettings.outmin.b;
                }


            }
            // Add the out-of bounds elements as required.

            return thisResult;
        }

        //**************************************************************************
        //Helpers
        //**************************************************************************

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
            double p1xFixed = p1x * scalex + Xorigin;
            double p1yFixed = canvasHeight - (p1y * scaley + Yorigin);
            double p2xFixed = p2x * scalex + Xorigin;
            double p2yFixed = canvasHeight - (p2y * scaley + Yorigin);

            //Yaxis
            Line line = new Line();
            //Thickness ythickness = new Thickness(0, 0, 0, 0);
            //line.Margin = ythickness;
            line.Visibility = System.Windows.Visibility.Visible;
            line.StrokeThickness = 1.5;
            line.Stroke = System.Windows.Media.Brushes.Black;

            //StartPoint
            line.X1 = p1xFixed;
            line.Y1 = p1yFixed;

            //EndPoint
            line.X2 = p2xFixed;
            line.Y2 = p2yFixed;
            //line.Y2 = (p2y * scaley + Yorigin);

            //Move to the offset relative to the axis:
            //Canvas.SetLeft(line, Xorigin);
            //Canvas.SetBottom(line, p2y * scaley + Yorigin);

            return line;
        }
        private static IList<UIElement> AddList(IList<UIElement> graph, IList<UIElement> boundariesUIData)
        {
            if(graph != null && boundariesUIData != null)
            {
                foreach (UIElement uie in boundariesUIData)
                {
                    graph.Add(uie);
                }
            }
            return graph;
        }
        private static System.Drawing.Color GetColour(int position, int ListCount)
        {
            System.Drawing.Color minColour = System.Drawing.Color.FromArgb(colourSettings.min.a, colourSettings.min.r, colourSettings.min.g, colourSettings.min.b);
            System.Drawing.Color midColour = System.Drawing.Color.FromArgb(colourSettings.mid.a, colourSettings.mid.r, colourSettings.mid.g, colourSettings.mid.b);
            System.Drawing.Color maxolour = System.Drawing.Color.FromArgb(colourSettings.max.a, colourSettings.max.r, colourSettings.max.g, colourSettings.max.b);

            System.Drawing.Color colourResult = Utils.GetBlendedColor(ListCount, 0, position, minColour, midColour, maxolour);
            return colourResult;
        }

    }


}



