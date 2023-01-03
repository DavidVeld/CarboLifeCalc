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
using Autodesk.Revit.UI;

namespace CarboLifeRevit
{

    /// <summary>
    /// Interaction logic for HeatMapCreator.xaml
    /// </summary>
    public partial class HeatMapCreator : Window
    {
        private CarboProject carboProject;
        private CarboGraphResult resultList;

        private ColourViewerHandler m_Handler;
        private ExternalEvent m_ExEvent;

        public HeatMapCreator(ExternalEvent exEvent, ColourViewerHandler handler, CarboProject project)
        {
            InitializeComponent();

            this.m_ExEvent = exEvent;
            this.m_Handler = handler;

            if (project != null)
                carboProject = project;
            else
                carboProject = new CarboProject();

        }

        //for Non-Modeless Usage;
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
        private void rad_Bymaterial_Click(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        private void rad_Bymaterial2_Click(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        private void rad_ByGroup_Click(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        private void rad_ByElement_Click(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        private void btn_Info_Click(object sender, RoutedEventArgs e)
        {
            UpdateData();
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
            Random rnd = new Random();

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
                }

                //print the results;
                cnv_Graph.Children.Clear();

                foreach (UIElement uielement in graphData.UIData)
                {
                    cnv_Graph.Children.Add(uielement);
                }
                resultList = graphData;

                //This is a random bit of code to test colours
                if(resultList.elementData.Count > 0)
                {
                    foreach(CarboValues cv in resultList.elementData)
                    {
                        cv.r = Convert.ToByte(rnd.Next(1, 250));
                        cv.g = Convert.ToByte(rnd.Next(1, 250));
                        cv.b = Convert.ToByte(rnd.Next(1, 250));
                    }
                }

            }
        }

        private void btn_Show_Click(object sender, RoutedEventArgs e)
        {

            m_Handler.ColourTheModel(resultList,true);
            m_ExEvent.Raise();
            //DozeOff();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CarboLifeApp.thisApp.CloseHeatmap();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //Before the form is closed, everything must be disposed properly
            m_ExEvent.Dispose();
            m_ExEvent = null;

            //clear the handler
            m_Handler = null;

            //You have to call the base class
            base.OnClosing(e);
        }

        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            m_Handler.ColourTheModel(resultList, false);
            m_ExEvent.Raise();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }
    }
}
