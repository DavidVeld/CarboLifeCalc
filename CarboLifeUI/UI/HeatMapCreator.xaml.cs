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
using Microsoft.Win32;
using System.IO;
using CarboLifeAPI;

namespace CarboLifeUI.UI
{

    /// <summary>
    /// Interaction logic for HeatMapCreator.xaml
    /// </summary>
    public partial class HeatMapCreator : Window
    {
        private CarboProject carboProject;
        CarboGraphResult graphData;
        public HeatMapCreator(CarboProject project)
        {
            carboProject = project;
            InitializeComponent();

        }

        public HeatMapCreator()
        {
            carboProject = null;
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this is just to confirm the window loaded
            if (carboProject != null)
            {
                lbl_name.Content = carboProject.Name;
            }
        }

        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateGraph();
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

                UpdateDataSource();

            }
            catch (Exception ex)
            {

            }
        }

        private void LoadProject()
        {
            lbl_name.Content = carboProject.Name;
            lbl_total.Content = carboProject.getTotalEC().ToString("N") + " tCO2";
            UpdateDataSource();
            
        }

        /// <summary>
        /// This method loads new data from the carboproject
        /// </summary>
        public void UpdateDataSource()
        {
            graphData = new CarboGraphResult();
            CarboGraphResult thisResult = new CarboGraphResult();
            //Define the type of graph to make:
            if (carboProject != null)
            {
                if (rad_ByDensitykg.IsChecked == true)
                {
                    //This will plot each element based on their material the X axis is the embodied carbon (kgCo2/kg) the Y axis it the weight or mass.

                    thisResult = CarboLifeAPI.HeatMapBuilderClasses.GetMaterialMassData(carboProject);

                }
                else if (rad_ByDensitym.IsChecked == true)
                {

                }
                else if(rad_ByGroup.IsChecked == true)
                {

                }
                else if(rad_ByElement.IsChecked == true)
                {

                }
                else if(rad_MaterialTotals.IsChecked == true)
                {

                }
                else
                {

                }
            }

            //if data was collected make it the source and update the graph
            //clear if no data
            if (thisResult.elementData.Count > 0)
            {
                graphData = thisResult;
                UpdateGraph();
            }
            else
                cnv_Graph.Children.Clear(); 

        }

        //this method updates the graph based on current settings.
        public void UpdateGraph()
        {
            cnv_Graph.Visibility = Visibility.Hidden;
            cnv_Graph.Children.Clear();
            cnv_Graph.Visibility = Visibility.Visible;

            if (carboProject != null && graphData!= null )
            {
                //Values we'd need for all options:
                double deviationFact = Utils.ConvertMeToDouble(txt_standard.Text);
                double xMaxCutoff = Utils.ConvertMeToDouble(txt_CutoffMax.Text);
                double xMinCutoff = Utils.ConvertMeToDouble(txt_CutoffMin.Text);

                if (graphData.elementData.Count > 0)
                {
                    graphData = CarboLifeAPI.HeatMapBuilderClasses.Calculate(cnv_Graph.ActualWidth, cnv_Graph.ActualHeight, deviationFact, xMinCutoff, xMaxCutoff);
                    
                    cnv_Graph.Children.Clear();
                    foreach (UIElement uielement in graphData.UIData)
                    {
                        cnv_Graph.Children.Add(uielement);
                    }
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGraph();
        }

        private void Btn_Ok_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is the value of the standard deviation which is used to set the outer bounds");
        }

        //Radiocontrollbuttons
        private void rad_Control_Click(object sender, RoutedEventArgs e)
        {
            UpdateDataSource();
        }
    }
}