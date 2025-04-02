using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CarboLifeUI.UI
{
    [Obsolete]
    public static class PieChartGenerator
    {
        internal static IList<UIElement> generateImage(Canvas chartCanvas, List<CarboDataPoint> Pieces)
        {
            IList<UIElement> result = new List<UIElement>();

            double total = getTotal(Pieces);
            if (total == 0)
                return result;

            double yoffset = 75;
            double xCentre = chartCanvas.ActualWidth / 2;
            double yCentre = chartCanvas.ActualHeight / 2;
            double diameter = 200;
            //Temp works
            //Draw a circle
            bool draft = true;
            if (draft == true)
            {
                Thickness pthickness = new Thickness(0, 0, 0, 0);

                Ellipse chart = new Ellipse();
                chart.Width = diameter;
                chart.Height = diameter;
                chart.Margin = pthickness;
                chart.Visibility = System.Windows.Visibility.Visible;
                chart.StrokeThickness = 1;
                chart.Stroke = System.Windows.Media.Brushes.Black;
                Canvas.SetLeft(chart, xCentre - (chart.Width / 2));
                Canvas.SetTop(chart, yCentre - (chart.Height / 2));
                result.Add(chart);
            }

            try
            {
                //double Xoffset = 100;
                //double Yoffset = 50;
                double startAngle = 0;
                //Write Values
                foreach (CarboDataPoint pcp in Pieces)
                {
                    double valueScale = pcp.Value / total;
                    double valuePerCent = valueScale * 100;

                    double endAngle = 360 * valueScale;

                    Point startPoint = new Point(xCentre, yCentre);
                    Point endpoint = new Point(xCentre, yCentre - diameter/2);

                    endpoint = Rotate(endpoint, endAngle, startPoint) ;

                    Line ln = new Line();
                    ln.X1 = startPoint.X;
                    ln.Y1 = startPoint.Y;
                    ln.X2 = endpoint.X;
                    ln.Y2 = endpoint.Y;

                    ln.Stroke = Brushes.Black;
                    ln.StrokeThickness = 2;

                    result.Add(ln);





                    TextBlock tb = new TextBlock();
                    tb.Text = pcp.Name + " - " + Math.Round(pcp.Value / 1000,2) + " tCO₂e = " + Math.Round(valuePerCent,2) + " %";

                    tb.Foreground = Brushes.Black;
                    tb.TextWrapping = TextWrapping.WrapWithOverflow;
                    tb.VerticalAlignment = VerticalAlignment.Top;
                    tb.FontSize = 13;
                    tb.Width = chartCanvas.ActualWidth;

                    Canvas.SetLeft(tb, 15);
                    Canvas.SetTop(tb, yoffset);

                    result.Add(tb);

                    yoffset += 17;
                    startAngle = endAngle;
                }
                //Write The max values

                TextBlock tbtotal = new TextBlock();
                tbtotal.Text = "Total: " + " - " + Math.Round(total / 1000,2) + " tCO₂e = " + "100.00" + " %";

                tbtotal.Foreground = Brushes.Black;
                tbtotal.TextWrapping = TextWrapping.WrapWithOverflow;
                tbtotal.VerticalAlignment = VerticalAlignment.Top;
                tbtotal.FontSize = 10;
                tbtotal.Width = chartCanvas.ActualWidth;

                Canvas.SetLeft(tbtotal, 15);
                Canvas.SetTop(tbtotal, yoffset);

                result.Add(tbtotal);
                yoffset += 15;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        private static double getTotal(List<CarboDataPoint> pieces)
        {
            double result = 0;
            if(pieces.Count > 0)
            {
                foreach (CarboDataPoint pcp in pieces)
                {
                    result += pcp.Value;
                }
            }
            return result;
        }

        public static Point Rotate(this Point pt, double angle, Point center)
        {
            Vector v = new Vector(pt.X - center.X, pt.Y - center.Y).Rotate(angle);
            return new Point(v.X + center.X, v.Y + center.Y);
        }

        public static Vector Rotate(this Vector v, double degrees)
        {
            return v.RotateRadians(degrees * Math.PI / 180);
        }

        public static Vector RotateRadians(this Vector v, double radians)
        {
            double ca = Math.Cos(radians);
            double sa = Math.Sin(radians);
            return new Vector(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }
    }
}
