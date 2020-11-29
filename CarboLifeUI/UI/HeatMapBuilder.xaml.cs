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

        public HeatMapBuilder()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rad_Bymaterial.IsChecked = true;
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void rad_Bymaterial_Click(object sender, RoutedEventArgs e)
        {
            lbl_Text.Content = "The heatmap will be based on " + Environment.NewLine + "the material density (Intensity map)";
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
            string info = "A normalized heatmap will distribute the embodied carbon values more evenly over the available range of colours. This is useful when you want to show the contrast between elements if there are some great extremes in your data. You cannot however use the colouring as an index for your values.";
            CarboInfoBox infoBox = new CarboInfoBox(info, 400, 200);
            infoBox.ShowDialog();
        }
    }
}
