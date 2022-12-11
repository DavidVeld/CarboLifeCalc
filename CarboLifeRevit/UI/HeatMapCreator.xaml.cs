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

using LiveCharts;
using LiveCharts.Defaults;

namespace CarboLifeRevit
{

    /// <summary>
    /// Interaction logic for HeatMapCreator.xaml
    /// </summary>
    public partial class HeatMapCreator : Window
    {
        private CarboProject carboProject;

        public HeatMapCreator(CarboProject project)
        {
            carboProject = project;
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this is just to confirm the window loaded
            lbl_Range.Content = carboProject.Name;
        }

        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rad_Bymaterial_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rad_Bymaterial2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rad_ByGroup_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rad_ByElement_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Info_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
