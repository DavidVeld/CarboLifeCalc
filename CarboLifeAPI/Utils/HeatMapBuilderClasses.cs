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
using Brushes = System.Windows.Media.Brushes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace CarboLifeAPI
{

    public static class HeatMapBuilderClasses
    {
        public static CarboGraphResult GetByMaterialMassChart(CarboProject carboProject, double actualWidth, double actualHeight)
        {
            List<CarboElement> bufferList = carboProject.getTemporaryElementListWithTotals();

            CarboGraphResult result = new CarboGraphResult();
            IList<UIElement> graphResult = new List<UIElement>();
            IList<CarboValues> dataResult = new List<CarboValues>();

            result.xName = "ECI";
            result.yName = "Mass";
            //Get a List<double> for only the calculated values.
            foreach (CarboElement carboElement in bufferList)
            {
                CarboValues value = new CarboValues();
                value.xValue = carboElement.ECI_Total;
                value.yValue = carboElement.Mass;
                dataResult.Add(value);
            }

            //Get the mins and maxes of the dataset
            double X_max = dataResult.Max(t => t.xValue);
            double X_min = dataResult.Min(t => t.xValue);
            double Y_max = dataResult.Max(t => t.yValue);
            double Y_min = dataResult.Min(t => t.yValue);

            //These are the offsets of the graph in the canvas
            double Xorigin = 25;
            double Yorigin = 25;
            double border = 25;
            double fontsize = 10;
            double realLength = actualWidth - Xorigin - border;
            double realHeight = actualHeight - Yorigin - border;

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
            Canvas.SetLeft(xtb, Xorigin +25);
            Canvas.SetBottom(xtb, 0);

            TextBlock xmin = new TextBlock();
            xmin.Text = Math.Round(X_min, 2).ToString("N");
            xmin.Foreground = Brushes.Black;
            xmin.FontSize = fontsize;
            Canvas.SetLeft(xmin, Xorigin);
            Canvas.SetBottom(xmin, 0);

            TextBlock xmax = new TextBlock();
            xmax.Text = Math.Round(X_max,2).ToString("N");
            xmax.Foreground = Brushes.Black;
            xmax.FontSize = fontsize;
            Canvas.SetLeft(xmax, Xorigin + realLength);
            Canvas.SetBottom(xmax, 0);

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

            Canvas.SetLeft(ymin, Xorigin - 22);
            Canvas.SetBottom(ymin, Yorigin);

            TextBlock ymax = new TextBlock();
            ymax.Text = Convert.ToString(Math.Round(Y_max, 2).ToString("N"));
            ymax.Foreground = Brushes.Black;
            ymax.Name = "ymax";
            ymax.FontSize = fontsize;

            ymax.TextAlignment = TextAlignment.Right;
            Canvas.SetLeft(ymax, Xorigin - 22);
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
            graphResult.Add(xtb);
            graphResult.Add(xmin);
            graphResult.Add(xmax);
            graphResult.Add(xline);

            graphResult.Add(ytb);
            graphResult.Add(ymin);
            graphResult.Add(ymax);
            graphResult.Add(yline);

            /// Calculate the scale between the points 
            double scaleLength = X_max - X_min;
            double scaleHeight = Y_max - Y_min;

            double scalex = realLength / scaleLength;
            double scaley = realHeight / scaleHeight;

            //
            foreach (CarboElement ce in bufferList)
            {
                Rectangle rect = new Rectangle();
                rect.Stroke = System.Windows.Media.Brushes.Black;
                rect.StrokeThickness = 2;
                rect.Width = 3;
                rect.Height = 3;

                double x = ce.ECI_Total * scalex;
                double y = ce.Mass * scaley;

                Canvas.SetLeft(rect, Xorigin + x - 1);
                Canvas.SetBottom(rect, Yorigin + y - 1 );

                graphResult.Add(rect);
            }

            //Marker for Origin
            Rectangle rectt = new Rectangle();
            rectt.Stroke = System.Windows.Media.Brushes.Red;
            rectt.StrokeThickness = 1;
            rectt.Width = 3;
            rectt.Height = 3;
            Canvas.SetLeft(rectt, Xorigin + 0);
            Canvas.SetBottom(rectt, Yorigin + 0);
            graphResult.Add(rectt);


            //Marker for Extreme
            Rectangle rectE = new Rectangle();
            rectE.Stroke = System.Windows.Media.Brushes.Red;
            rectE.StrokeThickness = 1;
            rectE.Width = 3;
            rectE.Height = 3;

            Canvas.SetLeft(rectE, Xorigin + ((X_max - X_min) * scalex) - 1);
            Canvas.SetBottom(rectE, Yorigin + ((Y_max - Y_min) * scaley) - 1);
            graphResult.Add(rectE);
            
            result.UIData = graphResult;
            result.elementData = null;

            return result;
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
    }
}



