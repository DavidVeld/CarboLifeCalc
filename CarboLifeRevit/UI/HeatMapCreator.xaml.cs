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
using CarboLifeAPI;
using Microsoft.Win32;
using System.IO;

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
            if (project != null)
                carboProject = project;
            else
                carboProject = new CarboProject();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this is just to confirm the window loaded
            lbl_Range.Content = carboProject.Name;
            UpdateData();
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
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Project File (*.clcx)|*.clcx|All files (*.*)|*.*";

                var path = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName))
                {
                    string projectPath = openFileDialog.FileName;

                    //Open the project
                    CarboProject projectToOpen = new CarboProject();

                    CarboProject projectToUpdate = new CarboProject();
                    CarboProject buffer = new CarboProject();
                    projectToUpdate = buffer.DeSerializeXML(projectPath);

                    projectToUpdate.Audit();
                    projectToUpdate.CalculateProject();

                    carboProject = projectToUpdate;
                }

                UpdateData();

            }
            catch (Exception ex)
            {

            }
        }


        private void UpdateData()
        {
            if (carboProject != null)
            {
                lbl_name.Content = carboProject.Name;
                lbl_total.Content = carboProject.getTotalEC().ToString("N") + " tCO2";

                CarboGraphResult graphData = new CarboGraphResult();

                //Define the type of graph to make:
                if (rad_Bymaterial.IsChecked == true)
                {
                    //This will plot each element based on their material the X axis is the embodied carbon (kgCo2/kg) the Y axis it the weight or mass.

                    graphData = CarboLifeAPI.HeatMapBuilderClasses.GetByMaterialMassChart(carboProject, cnv_Graph.ActualWidth, cnv_Graph.ActualHeight);
                    cnv_Graph.Children.Clear();

                    foreach (UIElement uielement in graphData.UIData)
                    {
                        cnv_Graph.Children.Add(uielement);
                    }
                }
            }
        }

        private void btn_Preview_Click(object sender, RoutedEventArgs e)
        {
           // revitWindow = uiapp.MainWindowHandle; //Revit 2019 and above

        }

        private void btn_Show_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
