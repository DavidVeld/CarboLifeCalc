using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboCircle.data;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using CarboLifeUI.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public CarboCircleMain(ExternalEvent exEvent, CarboCircleHandler handler)
        {
            InitializeComponent();

            this.m_ExEvent = exEvent;
            this.m_Handler = handler;

            try
            {
                //initiate new projece
                activeProject = new carboCircleProject();
                //Load() always hands back a usable instance, so no Copy() is needed here.
                activeProject.settings = new carboCircleSettings().Load();

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
                if (File.Exists(tempImgpath))
                    File.Delete(tempImgpath);
            }
            else
            {
                System.Windows.MessageBox.Show("Error");
            }
        }

        private void OnDataReady(object sender, List<carboCircleElement> e)
        {
            if (e == null) return;

            if (dataSwitch == 0)
            {
                collectedElements = e;
                activeProject.ParseMinedData(collectedElements);

                liv_MinedData.ItemsSource = null;
                liv_MinedData.ItemsSource = activeProject.minedData;

                liv_MinedMassObjects.ItemsSource = null;
                liv_MinedMassObjects.ItemsSource = activeProject.minedVolumes;

                setMineOk();
            }
            else if (dataSwitch == 1)
            {
                collectedElements = e;
                activeProject.ParseRequiredData(collectedElements);

                liv_requiredMaterialList.ItemsSource = null;
                liv_requiredMaterialList.ItemsSource = activeProject.requiredData;

                liv_RequiredMassObjects.ItemsSource = null;
                liv_RequiredMassObjects.ItemsSource = activeProject.requiredVolumes;

                setRequiredOk();
            }
        }

        private void setRequiredOk()
        {
            btn_GotoProject.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 125, 218, 88));
        }

        private void setMineOk()
        {
            btn_GotoMine.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 125, 218, 88));
        }

        private void btn_ImportmaterialsRevit_Click(object sender, RoutedEventArgs e)
        {
            //Remember the choice as the mine side preference, then hand it to the import
            //that is about to run. The import is told the method directly: it is a property
            //of this one request, and the settings file only remembers preferences.
            string method = chosenMethod(cbb_MineSetting, activeProject.settings.MineExtractionMethod);

            activeProject.settings.MineExtractionMethod = method;
            activeProject.settings.Save();

            if (m_ExEvent != null)
            {
                dataSwitch = 0;
                m_Handler.SetSwitch(1);
                m_Handler.SetSettings(activeProject);
                m_Handler.SetExtractionMethod(method);

                m_ExEvent.Raise();
            }
        }

        private void btn_ImportProjectRevit_Click(object sender, RoutedEventArgs e)
        {
            //Remember the choice as the project side preference, then hand it to the
            //import that is about to run. See the mine side above.
            string method = chosenMethod(cbb_ImportProjectSetting, activeProject.settings.RequiredExtractionMethod);

            activeProject.settings.RequiredExtractionMethod = method;
            activeProject.settings.Save();

            if (m_ExEvent != null)
            {
                dataSwitch = 1;
                m_Handler.SetSwitch(1);
                m_Handler.SetSettings(activeProject);
                m_Handler.SetExtractionMethod(method);

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

        /*
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
        */
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            LoadInterFaceFromSettings();

        }

        private void LoadInterFaceFromSettings()
        {


            cbb_ImportProjectSetting.Items.Clear();
            cbb_MineSetting.Items.Clear();

            //Filled from the same constants the collector switches on, so a label and the
            //branch it is meant to select cannot drift apart.
            foreach (string method in carboCircleExtractionMethod.RequiredMethods())
                cbb_ImportProjectSetting.Items.Add(method);

            foreach (string method in carboCircleExtractionMethod.MineMethods())
                cbb_MineSetting.Items.Add(method);

            //The two sides offer different methods, so each remembers its own choice.
            selectRemembered(cbb_ImportProjectSetting, activeProject.settings.RequiredExtractionMethod,
                carboCircleExtractionMethod.AllNewInView);
            selectRemembered(cbb_MineSetting, activeProject.settings.MineExtractionMethod,
                carboCircleExtractionMethod.AllDemolishedInView);

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

        /// <summary>
        /// Selects the remembered entry, falling back to <paramref name="fallback"/> when the
        /// setting is empty or is not one of the methods this side offers. Named rather than
        /// positional: the fallback used to be "index 1", which only happened to be the
        /// intended default.
        /// </summary>
        private static void selectRemembered(System.Windows.Controls.ComboBox combo, string remembered, string fallback)
        {
            int index = string.IsNullOrEmpty(remembered) ? -1 : combo.Items.IndexOf(remembered);

            if (index < 0)
                index = combo.Items.IndexOf(fallback);

            combo.SelectedIndex = index >= 0 ? index : 0;
        }

        /// <summary>
        /// The extraction method chosen in a combo, falling back to the remembered
        /// preference when nothing is selected.
        ///
        /// Never returns empty. This value now decides which branch the collector takes, so
        /// an empty one would silently mean "everything visible" - which is how the import
        /// used to ignore the choice altogether.
        /// </summary>
        private static string chosenMethod(System.Windows.Controls.ComboBox combo, string remembered)
        {
            string chosen = combo.SelectedItem as string;

            if (!string.IsNullOrEmpty(chosen))
                return chosen;

            return string.IsNullOrEmpty(remembered)
                ? carboCircleExtractionMethod.AllVisibleInView
                : remembered;
        }

        private void btn_Go_Click(object sender, RoutedEventArgs e)
        {
            // Get active settings. The two tolerances are separate values - the depth box
            // used to be written into both, silently discarding the strength tolerance.
            storeSettings();

            // Main script:
            if (activeProject.minedData.Count > 0 && activeProject.requiredData.Count > 0)
            {
                activeProject.FindOpportunities();

                liv_MatchedFraming.ItemsSource = null;
                liv_MatchedFraming.ItemsSource = activeProject.getCarboMatchesListSimplified();

                liv_MatchedVolumes.ItemsSource = null;
                liv_MatchedVolumes.ItemsSource = activeProject.getCarboVolumeOpportunities();

                liv_LeftOverData.ItemsSource = null;
                liv_LeftOverData.ItemsSource = activeProject.getLeftOverData();
            }

            // TODO: colours
        }

        private void btn_MineSettings_Click(object sender, RoutedEventArgs e)
        {
            //Carry what the main window owns into the settings object first, so the dialog
            //opens showing the tolerances currently on screen.
            storeSettings();

            CarboCircleSettings settingsWindow = new CarboCircleSettings(activeProject);
            settingsWindow.ShowDialog();

            if (settingsWindow.isAccepted)
            {
                //The dialog edited and saved its own snapshot; adopt it and redraw.
                activeProject.settings = settingsWindow.settings;
                LoadInterFaceFromSettings();
            }
        }

        /// <summary>
        /// Pushes the settings the main window edits directly into the settings object.
        /// Colours are written straight through by their own click handlers.
        /// </summary>
        private void storeSettings()
        {
            if (!string.IsNullOrWhiteSpace(txt_BeamStrengthTolerance.Text))
                activeProject.settings.strengthRange = Utils.ConvertMeToDouble(txt_BeamStrengthTolerance.Text);

            if (!string.IsNullOrWhiteSpace(txt_SteelBeamDepthTolerance.Text))
                activeProject.settings.depthRange = Utils.ConvertMeToDouble(txt_SteelBeamDepthTolerance.Text);
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
            storeSettings();
            activeProject.settings.Save();
            FormStatusChecker.isWindowOpen = false;

            this.Hide(); // instead of Close()
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
            System.Windows.MessageBox.Show("Select a csv containing elements for import,", "Message for You!");
            string openPath = DataExportUtils.GetOpenCSVLocation();

            if (!string.IsNullOrWhiteSpace(openPath))
            {
                List<carboCircleElement> importedElements = carboCircleUtils.GetElementsFromCVSFile(openPath);
                if (importedElements != null && importedElements.Count > 0)
                {
                    activeProject.ParseRequiredData(importedElements);

                    liv_requiredMaterialList.ItemsSource = null;
                    liv_requiredMaterialList.ItemsSource = activeProject.requiredData;

                    liv_RequiredMassObjects.ItemsSource = null;
                    liv_RequiredMassObjects.ItemsSource = activeProject.requiredVolumes;

                    setRequiredOk();
                }
            }
        }

        private void btn_ImportmaterialsCSV_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Select a csv file containing elements that can be reused,", "Message for You!");
            string openPath = DataExportUtils.GetOpenCSVLocation();

            if (!string.IsNullOrWhiteSpace(openPath))
            {
                List<carboCircleElement> importedElements = carboCircleUtils.GetElementsFromCVSFile(openPath);
                if (importedElements != null && importedElements.Count > 0)
                {
                    activeProject.ParseMinedData(importedElements);

                    liv_MinedData.ItemsSource = null;
                    liv_MinedData.ItemsSource = activeProject.minedData;

                    liv_MinedMassObjects.ItemsSource = null;
                    liv_MinedMassObjects.ItemsSource = activeProject.minedVolumes;

                    setMineOk();
                }
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            storeSettings();
            activeProject.settings.Save();
            FormStatusChecker.isWindowOpen = false;

            this.Hide(); // instead of Close()
        }
    }
}