using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialConstructionPicker.xaml
    /// </summary>
    public partial class HeatMapBuilder : Window
    {
        internal bool isAccepted;
        internal System.Drawing.Color minRangeColour;
        internal System.Drawing.Color midRangeColour;
        internal System.Drawing.Color maxRangeColour;
        internal System.Drawing.Color minOutColour;
        internal System.Drawing.Color maxOutColour;


        internal double standardDev;
        internal bool importData;
        internal bool createHeatmap;

        public HeatMapBuilder()
        {
            InitializeComponent();
        }

        public HeatMapBuilder(bool _importData, bool _createHeatmap)
        {
            this.importData = _importData;
            this.createHeatmap = _createHeatmap;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            minOutColour = HeatMapBuilderUtils.Session_minOutColour;
            if (minOutColour.A > 0)
                btn_MinOut.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(minOutColour.A, minOutColour.R, minOutColour.G, minOutColour.B));
            else
            {
                btn_MinOut.Background = Brushes.CornflowerBlue;
                minOutColour = ConvertToColor(btn_MinOut.Background);
            }

            maxOutColour = HeatMapBuilderUtils.Session_maxOutColour;
            if (maxOutColour.A > 0)
                btn_MaxOut.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(maxOutColour.A, maxOutColour.R, maxOutColour.G, maxOutColour.B));
            else
            {
                btn_MaxOut.Background = Brushes.Purple;
                maxOutColour = ConvertToColor(btn_MaxOut.Background);
            }
            //RANGE
            minRangeColour = HeatMapBuilderUtils.Session_minRangeColour;
            if (minRangeColour.A > 0)
                btn_Low.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(minRangeColour.A, minRangeColour.R, minRangeColour.G, minRangeColour.B));
            else 
            { 
                btn_Low.Background = Brushes.Lime;
                minRangeColour = ConvertToColor(btn_Low.Background);
            }

            midRangeColour = HeatMapBuilderUtils.Session_midRangeColour;
            if (midRangeColour.A > 0)
                btn_Mid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(midRangeColour.A, midRangeColour.R, midRangeColour.G, midRangeColour.B));
            else
            {
                btn_Mid.Background = Brushes.Orange;
                midRangeColour = ConvertToColor(btn_Mid.Background);
            }

            maxRangeColour = HeatMapBuilderUtils.Session_maxRangeColour;
            if (maxRangeColour.A > 0)
                btn_High.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(maxRangeColour.A, maxRangeColour.R, maxRangeColour.G, maxRangeColour.B));
            else
            {
                btn_High.Background = Brushes.Red;
                maxRangeColour = ConvertToColor(btn_High.Background);

            }
            standardDev = 1.5;
            txt_standard.Text = standardDev.ToString();

            chx_ImportValuesToRevit.IsChecked = importData;
            chx_CreateHeatmap.IsChecked = createHeatmap;

            rad_Bymaterial.IsChecked = true;

            UpdateRange();
        }
        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            standardDev = Utils.ConvertMeToDouble(txt_standard.Text);

            HeatMapBuilderUtils.Session_minOutColour = ConvertToColor(btn_MinOut.Background);
            HeatMapBuilderUtils.Session_maxOutColour = ConvertToColor(btn_MaxOut.Background);
            HeatMapBuilderUtils.Session_minRangeColour = ConvertToColor(btn_Low.Background);
            HeatMapBuilderUtils.Session_midRangeColour = ConvertToColor(btn_Mid.Background);
            HeatMapBuilderUtils.Session_maxRangeColour = ConvertToColor(btn_High.Background);

            importData = chx_ImportValuesToRevit.IsChecked.Value;
            createHeatmap = chx_CreateHeatmap.IsChecked.Value;

            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }
        private System.Drawing.Color ConvertToColor(System.Windows.Media.Brush brush)
        {
            System.Windows.Media.Color color = ((SolidColorBrush)brush).Color;
            System.Drawing.Color oldC = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            
            return oldC;

        }

        private void rad_Bymaterial_Click(object sender, RoutedEventArgs e)
        {
            lbl_Text.Content = "The heatmap will be based on " + Environment.NewLine + "the material density (Intensity map) " + Environment.NewLine + "based on kg CO2 per kg";
        }
        private void rad_Bymaterial2_Click(object sender, RoutedEventArgs e)
        {
            lbl_Text.Content = "The heatmap will be based on " + Environment.NewLine + "the material density (Intensity map) " + Environment.NewLine + "based on kg CO2 per m³";
        }
        private void rad_ByGroup_Click(object sender, RoutedEventArgs e)
        {
            lbl_Text.Content = "The heatmap will be based on " + Environment.NewLine + "the groups totals";
        }
        private void rad_ByElement_Click(object sender, RoutedEventArgs e)
        {
            lbl_Text.Content = "The heatmap will be based on " + Environment.NewLine + "the embodied carbon per element";
        }

        private void btn_Info_Click(object sender, RoutedEventArgs e)
        {
            string info = "A high standard deviation will include more elements in your range, a lower one will have more items set as extreems.";
            CarboInfoBox infoBox = new CarboInfoBox(info, 400, 200);
            infoBox.ShowDialog();
        }
        private System.Drawing.Color GetColor(System.Windows.Media.Brush startColour)
        {
            //System.Windows.Media.Color color = ((SolidColorBrush)startColour).Color;
            //System.Drawing.Color oldC = System.Drawing.Color.FromArgb(color.R, color.G, color.B);

            System.Drawing.Color oldC = ConvertToColor(startColour);

            ColorDialog MyDialog = new ColorDialog();
            // Keeps the user from selecting a custom color.
            MyDialog.AllowFullOpen = true;
            MyDialog.FullOpen = true;
            // Allows the user to get help. (The default is false.)
            MyDialog.ShowHelp = true;
            // Sets the initial color select to the current text color.
            MyDialog.Color = oldC;

            // Update the text box color if the user clicks OK 
            if (MyDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return MyDialog.Color;
            else
                return oldC;
        }
        private System.Windows.Media.Color GetColor(System.Drawing.Color drawingColour)
        {
            System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(drawingColour.A, drawingColour.R, drawingColour.G, drawingColour.B);
            return color;
        }

        private void btn_MinOut_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Brush startColour = btn_MinOut.Background;
            System.Drawing.Color pickedColour = GetColor(startColour);
            minOutColour = pickedColour;
            btn_MinOut.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));
            UpdateRange();

        }
        private void btn_MaxOut_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Brush startColour = btn_MaxOut.Background;
            System.Drawing.Color pickedColour = GetColor(startColour);
            maxOutColour = pickedColour;
            btn_MaxOut.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));
            UpdateRange();

        }

        private void btn_Low_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Brush startColour = btn_Low.Background;
            System.Drawing.Color pickedColour = GetColor(startColour);
            minRangeColour = pickedColour;
            btn_Low.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));
            UpdateRange();

        }
        private void btn_High_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Brush startColour = btn_High.Background;
            System.Drawing.Color pickedColour = GetColor(startColour);
            maxRangeColour = pickedColour;
            btn_High.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));
            UpdateRange();
        }

         private void btn_Mid_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Brush startColour = btn_Mid.Background;
            System.Drawing.Color pickedColour = GetColor(startColour);
            midRangeColour = pickedColour;
            btn_Mid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));
            UpdateRange();

        }
        private void UpdateRange()
        {
            LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
            //Min Out of Range:
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(GetColor(minOutColour), 0.0));
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(GetColor(minOutColour), 0.125));

            //Range
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(GetColor(minRangeColour), 0.126));
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(GetColor(midRangeColour), 0.5));
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(GetColor(maxRangeColour), 0.874));
            //Max out
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(GetColor(maxOutColour), 0.875));
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(GetColor(maxOutColour), 1.0));
            lbl_Range.Background = myLinearGradientBrush;
        }


    }
}
