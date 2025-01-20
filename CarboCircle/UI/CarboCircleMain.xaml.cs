using Autodesk.Revit.UI;
using CarboCircle.data;
using CarboLifeAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace CarboCircle.UI
{
    /// <summary>
    /// Interaction logic for CarboCircleMain.xaml
    /// </summary>
    public partial class CarboCircleMain : Window
    {
        //Used for Revit handlers
        private CarboCircleHandler m_Handler;
        private ExternalEvent m_ExEvent;

        private static carboCircleProject activeProject;
        private static List<carboCircleElement> collectedElements;

        private int dataSwitch = 0;
        public CarboCircleMain()
        {
            InitializeComponent();
            activeProject = new carboCircleProject();

        }

        public CarboCircleMain(ExternalEvent exEvent, CarboCircleHandler handler)
        {
            InitializeComponent();

            this.m_ExEvent = exEvent;
            this.m_Handler = handler;

            activeProject = new carboCircleProject();

            // Subscribe to the DataReady event
            m_Handler.DataReady += OnDataReady;

            
        }

        private void OnDataReady(object sender, List<carboCircleElement> e)
        {
            if (e != null)
            {
                if (dataSwitch == 0)
                {
                    collectedElements = e;
                    activeProject.ParseMinedData(collectedElements);

                    //liv_MinedData.Items.Clear();
                    //liv_MinedData.ItemsSource = "";
                    liv_MinedData.ItemsSource = activeProject.minedData;
                    liv_MinedMassObjects.ItemsSource = activeProject.minedVolumes;
                }
                else
                {
                    collectedElements = e;
                    activeProject.ParseRequiredData(collectedElements);

                    //liv_requiredMaterialList.Items.Clear();
                    //liv_requiredMaterialList.ItemsSource = "";
                    liv_requiredMaterialList.ItemsSource = activeProject.requiredData;
                    liv_RequiredMassObjects.ItemsSource = activeProject.requiredVolumes;
                }
            }
        }

        private void btn_ImportmaterialsRevit_Click(object sender, RoutedEventArgs e)
        {
            activeProject.settings.extractionMethod = cbb_MineSetting.Text;

            if (m_ExEvent != null)
            {
                dataSwitch = 0;
                m_Handler.SetSwitch(1);
                m_Handler.SetSettings(activeProject);

                m_ExEvent.Raise();
            }
        }

        private void btn_ImportProjectRevit_Click(object sender, RoutedEventArgs e)
        {
            activeProject.settings.extractionMethod = cbb_ImportProjectSetting.Text;

            if (m_ExEvent != null)
            {
                dataSwitch = 1;
                m_Handler.SetSwitch(1);
                m_Handler.SetSettings(activeProject.settings);

                m_ExEvent.Raise();
            }
        }

        private void btn_Visualise_Click(object sender, RoutedEventArgs e)
        {
            
            if (m_ExEvent != null)
            {
                dataSwitch = 2;
                m_Handler.SetSwitch(2);
                m_Handler.SetSettings(activeProject);

                m_ExEvent.Raise();
            }
        }

        private void btn_Select_Click(object sender, RoutedEventArgs e)
        {
            if (liv_MatchedFraming.SelectedItem != null)
            {
                try
                {
                    carboCircleMatchElement selectedMatch = liv_MatchedFraming.SelectedItem as carboCircleMatchElement;

                    if (selectedMatch != null)
                    {
                        if (m_ExEvent != null)
                        {
                            dataSwitch = 3;
                            m_Handler.SetSwitch(3);
                            m_Handler.SetSettings(selectedMatch);

                            m_ExEvent.Raise();
                        }
                    }
                }
                catch { }
        }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //Before the form is closed, everything must be disposed properly
            try
            {
                m_ExEvent.Dispose();
                m_ExEvent = null;

                //clear the handler
                m_Handler._revitEvent.Dispose();
                m_Handler._revitEvent = null;
                m_Handler = null;

                FormStatusChecker.isWindowOpen = false;
                //You have to call the base class
                base.OnClosing(e);
            }
            catch 
            {
            }

        }

        private void btn_GotoMine_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => tab_Main.SelectedIndex = 1));
        }
        private void btn_GotoProject_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => tab_Main.SelectedIndex = 2));

        }
        private void btn_ImportProjectSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowSettings()
        {
            CarboCircleSettings settings = new CarboCircleSettings(activeProject);
            settings.Show();
            if (settings.isAccepted)
            {
                activeProject.settings = settings.settings.Copy();
                ShowSettings();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            LoadInterFaceFromSettings();

        }

        private void LoadInterFaceFromSettings()
        {
            cbb_ImportProjectSetting.Items.Clear();
            cbb_MineSetting.Items.Clear();

            cbb_ImportProjectSetting.Items.Add("All Visible in View");// Index 0;
            cbb_ImportProjectSetting.Items.Add("All New in View"); // Index 1;
            cbb_ImportProjectSetting.Items.Add("Selected");// Index 2;
            cbb_ImportProjectSetting.SelectedIndex = 1;

            cbb_MineSetting.Items.Add("All Visible in View"); //Index: 0
            cbb_MineSetting.Items.Add("All Demolished in View"); //Index: 1
            cbb_MineSetting.Items.Add("Selected"); //Index: 2
            cbb_MineSetting.SelectedIndex = 1;

            txt_BeamStrengthTolerance.Text = activeProject.settings.strengthRange.ToString();
            txt_SteelBeamDepthTolerance.Text = activeProject.settings.depthRange.ToString();


        }

        private void btn_Go_Click(object sender, RoutedEventArgs e)
        {            
            //get activesettings:
            activeProject.settings.strengthRange = double.Parse(txt_SteelBeamDepthTolerance.Text);
            activeProject.settings.depthRange = double.Parse(txt_SteelBeamDepthTolerance.Text);

            //Mainscript:
            if (activeProject.minedData.Count > 0 && activeProject.requiredData.Count > 0)
            {
                activeProject.FindOpportunities();
                liv_MatchedFraming.ItemsSource = activeProject.getCarboMatchesListSimplified();
                liv_MatchedVolumes.ItemsSource = activeProject.getCarboVolumeOpportunities();
                liv_LeftOverData.ItemsSource = activeProject.getLeftOverData();
            }
        }

        private void btn_MineSettings_Click(object sender, RoutedEventArgs e)
        {
            CarboCircleSettings settings = new CarboCircleSettings(activeProject);
            settings.Show();
            if (settings.isAccepted)
            {
                activeProject.settings = settings.settings.Copy();
                ShowSettings();
            }
        }

        private void txt_ParseTextSettings_TextChanged(object sender, TextChangedEventArgs e)
        {



        }

        private void btn_ExportMinedToCSV(object sender, RoutedEventArgs e)
        {
            List<carboCircleElement> dataCombined = new List<carboCircleElement>();

            string path = DataExportUtils.GetSaveAsLocation();


            List<carboCircleElement> dataToExport = activeProject.minedData;
            List<carboCircleElement> volumesToExport = activeProject.minedVolumes;

            foreach(carboCircleElement dat in dataToExport)
            {
                dataCombined.Add(dat.Copy());
            }

            foreach (carboCircleElement vol in volumesToExport)
            {
                dataCombined.Add(vol.Copy());
            }

            if(path != null)
            {
                carboCircleUtils.ExportDataToCSV(dataCombined, path);

            }


            if (File.Exists(path))
            {
                System.Windows.MessageBox.Show("CSV export successful. Click OK to open export directory.", "Success!", MessageBoxButton.OK);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }

        }

        private void btn_ExportProjectData_Click(object sender, RoutedEventArgs e)
        {
            List<carboCircleElement> dataCombined = new List<carboCircleElement>();

            string path = DataExportUtils.GetSaveAsLocation();


            List<carboCircleElement> dataToExport = activeProject.requiredData;
            List<carboCircleElement> volumesToExport = activeProject.requiredVolumes;

            foreach (carboCircleElement dat in dataToExport)
            {
                dataCombined.Add(dat.Copy());
            }

            foreach (carboCircleElement vol in volumesToExport)
            {
                dataCombined.Add(vol.Copy());
            }

            if (path != null)
            {
                carboCircleUtils.ExportDataToCSV(dataCombined, path);

            }


            if (File.Exists(path))
            {
                System.Windows.MessageBox.Show("CSV export successful. Click OK to open export directory.", "Success!", MessageBoxButton.OK);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
        }
    }
}
