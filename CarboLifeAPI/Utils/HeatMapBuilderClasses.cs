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
using System.Windows.Media;
using System.Windows.Shapes;
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
        private static double canvasWidth;
        private static double canvasHeight;

        //CanvasScale
        //The mins and maxes of the dataset
        private static double X_max;
        private static double X_min;
        private static double Y_max;
        private static double Y_min;

        //These are the offsets of the graph in the canvas
        private static double Xorigin;
        private static double Yorigin;
        private static double border;
        private static double fontsize;
        private static double realLength;
        private static double realHeight;

        public static CarboGraphResult GetByMaterialMassChart(CarboProject carboProject, double actualWidth, double actualHeight)
        {
            List<CarboElement> bufferList = carboProject.getTemporaryElementListWithTotals();
            SetColours();

            result = new CarboGraphResult();
            canvasWidth = actualWidth;
            canvasHeight = actualHeight;

            IList<CarboValues> dataResult = new List<CarboValues>();

            result.xName = "ECI";
            result.yName = "Mass";

            //This Bits collects the required information we need to build the graph
            foreach (CarboElement carboElement in bufferList)
            {
                CarboValues value = new CarboValues();
                value.xValue = carboElement.ECI_Total;
                value.yValue = carboElement.Mass;
                value.Id = carboElement.Id;

                dataResult.Add(value);
            }

            //Add the values to the result;
            result.elementData = dataResult;

            IList<UIElement> axisUIInfo = DrawAxis();
            result.Add(axisUIInfo);


            result = ColourByValues();

            return result;
        }

        private static void SetColours()
        {
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

            /// Calculate the scale between the points 
            double scaleLength = X_max - X_min;
            double scaleHeight = Y_max - Y_min;

            double scalex = realLength / scaleLength;
            double scaley = realHeight / scaleHeight;

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

            //Add items to the UIclass
            result.Add(thisResult);

            return result;
        }

        private static System.Drawing.Color GetColour(CarboValues cv)
        {
            System.Drawing.Color colourResult = Utils.GetBlendedColor(X_max, X_min, cv.xValue, Session_minRangeColour, Session_midRangeColour, Session_maxRangeColour);
            return colourResult;
        }
    }

    public class CarboValues
    {
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
        public int Id { get; set; }
        public double xValue { get; set; }
        public double yValue { get; set; }
        public int EC_Total { get; set; }

    }

    public class CarboGraphResult
    {
        public string xName { get; set; }
        public string yName { get; set; }

        /// <summary>
        /// This paramater owns all the data for import to Revit
        /// </summary>
        public IList<CarboValues> elementData;
        /// <summary>
        /// This is the Graph Showing the Data
        /// </summary>
        public IList<UIElement> UIData;

        public CarboGraphResult()
        {
            elementData = new List<CarboValues>();
            UIData = new List<UIElement>();
        }

        internal void Add(IList<UIElement> listOfUiData)
        {
            if (listOfUiData.Count > 0)
            {
                foreach (UIElement uie in listOfUiData)
                {
                    this.UIData.Add(uie);
                }
            }
        }
    }
}



