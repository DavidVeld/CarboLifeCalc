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

        public double Total { get; internal set; }

        public HeatMapBuilder()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Value.Text = Total.ToString();
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
            lbl_Text.Content = "The heatmap will be based on " + Environment.NewLine + "the individual element's embodied carbon";
        }
    }
}
