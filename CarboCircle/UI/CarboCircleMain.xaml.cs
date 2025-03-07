using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboCircle.data;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using CarboLifeUI.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
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
        string reportPath = "";

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

            try
            {
                //initiate new projece
                activeProject = new carboCircleProject();
                carboCircleSettings settings = new carboCircleSettings();
                settings = settings.Load().Copy();
                activeProject.settings = settings;

                // Subscribe to the DataReady event
                m_Handler.DataReady += OnDataReady;
                m_Handler.ImageReady += OnImageReady;
            }
            catch (Exception ex)
            {
                this.Close();
            }
            
        }

        private void OnImageReady(object sender, string tempImgpath)
        {
            //Create a report after ImageCreation:
            //check if image was created:
            //get temp Filepath
            //string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //string MyAssemblyDir = System.IO.Path.GetDirectoryName(MyAssemblyPath);
            //string tempImgpath = MyAssemblyDir + "\\tempCircleImg.jpg";
            if (File.Exists(tempImgpath))
            {
                string imgstring = carboCircleReportUtils.getImageAsString(tempImgpath);
                carboCircleReportUtils.ExportReport(activeProject, imgstring, reportPath);
                //if all ok delete the temp image:
                if(File.Exists(tempImgpath))
                    File.Delete(tempImgpath);
            }
            else
            {
                System.Windows.MessageBox.Show("Error");
            }
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
                    setMineOk();
                }
                else if (dataSwitch == 1)
                {
                    collectedElements = e;
                    activeProject.ParseRequiredData(collectedElements);

                    liv_requiredMaterialList.Items.Clear();
                    liv_requiredMaterialList.ItemsSource = "";
                    liv_RequiredMassObjects.Items.Clear();
                    liv_RequiredMassObjects.ItemsSource = "";

                    liv_requiredMaterialList.ItemsSource = activeProject.requiredData;
                    liv_RequiredMassObjects.ItemsSource = activeProject.requiredVolumes;
                    setRequiredOk();

                }
            }
        }

        private void setRequiredOk()
        {
            btn_GotoProject.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 125, 218,88));
        }

        private void setMineOk()
        {
            btn_GotoMine.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 125, 218, 88));
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
                m_Handler.SetSettings(activeProject);

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
                activeProject.settings.Save();

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
                //ShowSettings();
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

            //load colours
            btn_ColourMinedNotReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 
                activeProject.settings.colour_NotReused.r, activeProject.settings.colour_NotReused.g, activeProject.settings.colour_NotReused.b));
            btn_ColourMinedReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 
                activeProject.settings.colour_ReusedMinedData.r, activeProject.settings.colour_ReusedMinedData.g, activeProject.settings.colour_ReusedMinedData.b));

            btn_ColourRequiredNotReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 
                activeProject.settings.colour_NotFromReused.r, activeProject.settings.colour_NotFromReused.g, activeProject.settings.colour_NotFromReused.b));
            btn_ColourRequiredReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 
                activeProject.settings.colour_FromReusedData.r, activeProject.settings.colour_FromReusedData.g, activeProject.settings.colour_FromReusedData.b));
            btn_ColourMassReusable.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                activeProject.settings.colour_ReusedMinedVolumes.r, activeProject.settings.colour_ReusedMinedVolumes.g, activeProject.settings.colour_ReusedMinedVolumes.b));

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

            //colours
            

        }

        private void btn_MineSettings_Click(object sender, RoutedEventArgs e)
        {
            storeSettings();

            CarboCircleSettings settingsWindow = new CarboCircleSettings(activeProject);
            settingsWindow.Show();
            if (settingsWindow.isAccepted)
            {
                activeProject.settings = settingsWindow.settings.Copy();
                ShowSettings();
            }
        }

        private void storeSettings()
        {
            activeProject.settings.strengthRange = Utils.ConvertMeToDouble(txt_BeamStrengthTolerance.Text);
            activeProject.settings.depthRange = Utils.ConvertMeToDouble(txt_SteelBeamDepthTolerance.Text);

            //colours


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

        private void btn_ColourMinedReused_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourMinedReused.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_ReusedMinedData = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourMinedReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_ColourMinedNotReused_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourMinedNotReused.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_NotReused = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourMinedNotReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        private void btn_ColourRequiredReused_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourRequiredReused.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_FromReusedData = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourRequiredReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_ColourRequiredNotReused_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourRequiredNotReused.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_NotFromReused = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourRequiredNotReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_ColourMassReusable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourMassReusable.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_ReusedMinedVolumes = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourMassReusable.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        private System.Drawing.Color GetColor(System.Windows.Media.Brush startColour)
        {
            //System.Windows.Media.Color color = ((SolidColorBrush)startColour).Color;
            //System.Drawing.Color oldC = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            try
            {
                System.Drawing.Color oldC = ConvertToColor(startColour);

                System.Windows.Forms.ColorDialog MyDialog = new System.Windows.Forms.ColorDialog();
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
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
                return System.Drawing.Color.FromArgb(255, 0, 0, 0);
            }
        }

        private System.Drawing.Color ConvertToColor(System.Windows.Media.Brush brush)
        {
            try
            {
                System.Windows.Media.Color color = ((SolidColorBrush)brush).Color;
                System.Drawing.Color oldC = System.Drawing.Color.FromArgb(color.R, color.G, color.B);

                return oldC;
            }
            catch (Exception ex)
            {
                return System.Drawing.Color.FromArgb(255, 0, 0, 0);
            }
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            activeProject.settings.Save();
            this.Close();
        }

        private void btn_Report_Click(object sender, RoutedEventArgs e)
        {
            //
            bool createReport = true;
            //Create a File and save it as a HTML File
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Specify report directory";
            saveDialog.Filter = "HTML Files|*.html";
            saveDialog.FilterIndex = 2;
            saveDialog.RestoreDirectory = true;

            saveDialog.ShowDialog();

            string Path = saveDialog.FileName;

            if (File.Exists(Path))
            {
                MessageBoxResult msgResult = System.Windows.MessageBox.Show("This file already exists, do you want to overwrite this file ?", "", MessageBoxButton.YesNo);

                if (msgResult == MessageBoxResult.Yes)
                {
                    using (var fs = File.Open(Path, FileMode.Open))
                    {
                        var canRead = fs.CanRead;
                        var canWrite = fs.CanWrite;

                        if (canWrite == false)
                        {
                            System.Windows.MessageBox.Show("This file cannot be opened, please close the file and try again", "Warning", MessageBoxButton.OK);
                            createReport = false;
                        }
                    }
                    createReport = true;
                }
                else
                {
                    createReport = false;
                }
            }
            else if (Path == "")
            {
                //The dialog box was canceled;
                createReport = false;
            }


            if (createReport == true && Path != "")
            {
                if (m_ExEvent != null)
                {
                    dataSwitch = -1;
                    reportPath = Path;

                    m_Handler.SetSwitch(4);
                    m_Handler.SetSettings(activeProject);

                    m_ExEvent.Raise();
                }
            }

        }

        private void btn_ImportProjectCSV_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Select a csv containing elements for import, ","Message for You!");
            string openPath = DataExportUtils.GetOpenCSVLocation();

            if (openPath != null && openPath != "")
            {
                List<carboCircleElement> importedElements = carboCircleUtils.GetElementsFromCVSFile(openPath);
                if(importedElements != null && importedElements.Count > 0)
                {
                    activeProject.ParseRequiredData(importedElements);

                    liv_requiredMaterialList.Items.Clear();
                    liv_RequiredMassObjects.Items.Clear();

                    liv_requiredMaterialList.ItemsSource = activeProject.requiredData;
                    liv_RequiredMassObjects.ItemsSource = activeProject.requiredVolumes;
                }
                setRequiredOk();

            }
        }

        private void btn_ImportmaterialsCSV_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Select a csv file containing elements that can be-reused, ", "Message for You!!");
            string openPath = DataExportUtils.GetOpenCSVLocation();

            if (openPath != null && openPath != "")
            {
                List<carboCircleElement> importedElements = carboCircleUtils.GetElementsFromCVSFile(openPath);
                if (importedElements != null && importedElements.Count > 0)
                {
                    activeProject.ParseMinedData(importedElements);

                    liv_MinedData.Items.Clear();
                    liv_MinedMassObjects.Items.Clear();

                    liv_MinedData.ItemsSource = activeProject.minedData;
                    liv_MinedMassObjects.ItemsSource = activeProject.minedVolumes;
                }
                setMineOk();

            }
        }

        private void btn_GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                CarboProject myProject = carboCircleUtils.convertToCarboLifeProject(activeProject);


                if (myProject != null)
                {
                    try
                    {
                        CarboLifeUI.UI.CarboLifeMainWindow CarboApp = new CarboLifeMainWindow(myProject);
                        CarboApp.ShowDialog();
                    }
                    catch { }
                }
            }
            catch
            { }



        }


    }
}
