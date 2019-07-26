using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarboLifeUI.UI
{
    public static class Utils_SummaryScreen
    {
        internal static IList<UIElement> generateImage(Canvas chartCanvas, CarboProject carboProject)
        {
            IList<UIElement> result = new List<UIElement>();

            double CarbonTotal = carboProject.EC;
            double CarbonMin = carboProject.EE;

            try
            {
                //double Xoffset = 100;
                //double Yoffset = 50;

                //Write Values
                TextBlock tb = new TextBlock();
                tb.Text = "Total: " + CarbonTotal + " tCo2";

                tb.Foreground = Brushes.Black;
                tb.TextWrapping = TextWrapping.WrapWithOverflow;
                tb.VerticalAlignment = VerticalAlignment.Top;
                tb.FontSize = 10;
                tb.Width = chartCanvas.ActualWidth;

                Canvas.SetLeft(tb, 0);
                Canvas.SetBottom(tb, 0);
                result.Add(tb);

                //Write The max values



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return result;
        }

    }
}
