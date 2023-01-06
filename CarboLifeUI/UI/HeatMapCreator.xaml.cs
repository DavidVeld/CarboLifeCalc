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
using System.Runtime.ConstrainedExecution;

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
                    
                    //Show the data
                    lbl_name.Content = carboProject.Name;
                    lbl_total.Content = carboProject.getTotalEC().ToString("N") + " tCO2";
                }

                UpdateDataSource();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateGraph();
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
                    thisResult = CarboLifeAPI.HeatMapBuilderClasses.GetMaterialVolumeData(carboProject);
                }
                else if(rad_ByGroup.IsChecked == true)
                {
                    thisResult = CarboLifeAPI.HeatMapBuilderClasses.GetPerGroupData(carboProject);
                }
                else if(rad_ByElement.IsChecked == true)
                {
                    thisResult = CarboLifeAPI.HeatMapBuilderClasses.GetPerElementData(carboProject);
                }
                else if(rad_MaterialTotals.IsChecked == true)
                {
                    thisResult = CarboLifeAPI.HeatMapBuilderClasses.GetMaterialTotalData(carboProject);
                }
                else
                {

                }
            }

            //if data was collected make it the source and update the graph
            //clear if no data
            if (thisResult.elementData.Count > 0)
            {
                double maxValue = thisResult.elementData.Max(item => item.xValue);
                double minValue = thisResult.elementData.Min(item => item.xValue);

                //Some Data checks:
                if (minValue > 0)
                    minValue = 0;
                maxValue = Convert.ToInt32(maxValue);

                txt_CutoffMax.Text = maxValue.ToString();
                txt_CutoffMin.Text = minValue.ToString();
                
                sld_Max.Minimum = minValue;
                sld_Max.Maximum = maxValue;
                sld_Max.Value = maxValue;

                sld_Min.Minimum = minValue;
                sld_Min.Maximum = maxValue;
                sld_Min.Value = minValue;

                graphData = thisResult;
                UpdateGraph();
            }
            else
                cnv_Graph.Children.Clear(); 

        }

        //this method updates the graph based on current settings.
        public void UpdateGraph()
        {
            if (cnv_Graph != null)
            {
                cnv_Graph.Visibility = Visibility.Hidden;
                cnv_Graph.Children.Clear();
                cnv_Graph.Visibility = Visibility.Visible;
                if (carboProject != null && graphData != null)
                {
                    //Values we'd need for all options:
                    double xMaxCutoff = Utils.ConvertMeToDouble(txt_CutoffMax.Text);
                    double xMinCutoff = Utils.ConvertMeToDouble(txt_CutoffMin.Text);

                    if (graphData.elementData.Count > 0)
                    {
                        graphData = CarboLifeAPI.HeatMapBuilderClasses.Calculate(cnv_Graph.ActualWidth, cnv_Graph.ActualHeight, xMinCutoff, xMaxCutoff);

                        cnv_Graph.Children.Clear();
                        foreach (UIElement uielement in graphData.UIData)
                        {
                            cnv_Graph.Children.Add(uielement);
                        }
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

        //Radiocontrollbuttons
        private void rad_Control_Click(object sender, RoutedEventArgs e)
        {
            UpdateDataSource();
        }

        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            //graphData = new CarboGraphResult();
            cnv_Graph.Children.Clear();
        }

        private void sld_Max_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txt_CutoffMax.Text = Math.Round(sld_Max.Value,3).ToString();
            UpdateGraph();
        }

        private void sld_Min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txt_CutoffMin.Text = Math.Round(sld_Min.Value, 3).ToString();
            UpdateGraph();

        }

        private void dummy(object sender, RoutedEventArgs e)
        {

        }
    }
}