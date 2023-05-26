using CarboLifeAPI;
using CarboLifeAPI.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for CarboLifeMainWindow.xaml
    /// </summary>
    public partial class CarboLifeMainWindow : Window
    {
        public CarboProject carboLifeProject { get; set; }
        /// <summary>
        /// If the app is launched from Revit IsRevit = true to allow extra settings.
        /// </summary>
        public bool IsRevit { get; set; }
        /// <summary>
        /// If a heat map will be created after exit;
        /// </summary>

        //For async excel exporter:
        public string ExcelExportPath { get; private set; }
        public bool ExportExcel_Completed { get; private set; }
        public bool ExcelExportResult { get; private set; }
        public bool ExcelExportElements { get; private set; }
        public bool ExcelExportMaterials { get; private set; }

        /////////////

        //public CarboDatabase carboDataBase { get; set; }
        public CarboLifeMainWindow()
        {
            //UserPaths
            PathUtils.CheckFileLocationsNew();

            IsRevit = false;
            carboLifeProject = new CarboProject();
            InitializeComponent();
        }

        public CarboLifeMainWindow(CarboProject myProject)
        {
            //UserPaths
            PathUtils.CheckFileLocationsNew();

            carboLifeProject = myProject;
            IsRevit = false;
            //carboDataBase = carboDataBase.DeSerializeXML("");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            //carboLifeProject.CreateGroups();
            InitializeComponent();
        }
        private void Menu_Loaded(object sender, RoutedEventArgs e)
        {

        }

        internal CarboProject getCarbonLifeProject()
        {
            if (carboLifeProject != null)
                return carboLifeProject;
            else
                return null;
        }

        /// <summary>
        /// SaveAs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mnu_saveDataBase_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
        }
        /// <summary>
        /// Start A new Project
        /// </summary>
        private void mnu_newProject_Click(object sender, RoutedEventArgs e)
        {
            bool fileSaved = false;

            //This bit is a verification code, to make sure the user is given the opportunity to save teh work before continuing:
            if (carboLifeProject.justSaved == false)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save your project first?", "Warning", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    if (carboLifeProject.filePath == "")
                        fileSaved = SaveFileAs();
                    else
                        fileSaved = SaveFile(carboLifeProject.filePath);
                }
                else if (result == MessageBoxResult.No)
                {
                    //The user didnt want to save
                    fileSaved = true;
                }
                else
                {
                    //the user cancels
                    fileSaved = false;
                }
            }
            else
            {
                //The file was already saved
                fileSaved = true;
            }
            //
            //the file is either saved, or used didnt want to save:
            if (fileSaved == true)
            {
                try
                {
                    carboLifeProject = new CarboProject();

                    carboLifeProject.Audit();
                    carboLifeProject.CalculateProject();
                    carboLifeProject.justSaved = false;

                    tab_Main.Visibility = Visibility.Hidden;
                    tab_Main.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Mnu_openDataBase_Click(object sender, RoutedEventArgs e)
        {
            bool fileSaved = false;

            //This bit is a verification code, to make sure the user is given the opportunity to save teh work before continuing:
            if (carboLifeProject.justSaved == false)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save your project first?", "Warning", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    if (carboLifeProject.filePath == "")
                        fileSaved = SaveFileAs();
                    else
                        fileSaved = SaveFile(carboLifeProject.filePath);
                }
                else if (result == MessageBoxResult.No)
                {
                    //The user didnt want to save
                    fileSaved = true;
                }
                else
                {
                    //the user cancels
                    fileSaved = false;
                }
            }
            else
            {
                //The file was already saved
                fileSaved = true;
            }
            //
            //the file is either saved, or used didnt want to save:
            if (fileSaved == true)
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Carbo Life Project File (*.clcx)|*.clcx|All files (*.*)|*.*";

                    var path = openFileDialog.ShowDialog();

                    if (openFileDialog.FileName != "")
                    {
                        CarboProject newProject = new CarboProject();

                        CarboProject buffer = new CarboProject();
                        carboLifeProject = buffer.DeSerializeXML(openFileDialog.FileName);

                        carboLifeProject.Audit();
                        carboLifeProject.CalculateProject();

                        tab_Main.Visibility = Visibility.Hidden;
                        tab_Main.Visibility = Visibility.Visible;

                        carboLifeProject.justSaved = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        
       private void Mnu_saveProject_Click(object sender, RoutedEventArgs e)
        {
            string path = carboLifeProject.filePath;
            if(File.Exists(path))
            {
                SaveFile(path);
            }
            else
            {
                SaveFileAs();
            }
        }

        /// <summary>
        /// Saves the file
        /// </summary>
        /// <param name="path">the path to the Carbo Life Project File</param>
        private bool SaveFile(string path, bool newFile = false)
        {
            try
            {
                if (File.Exists(path) || newFile == true)
                {
                    bool ok = carboLifeProject.SerializeXML(path);
                    if (ok == true)
                    {
                        MessageBox.Show("Project Saved");
                        carboLifeProject.justSaved = true;
                        carboLifeProject.filePath = path;
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("There was a problem while saving the file, please use save-as to re-save your file.");
                        return false;
                    }
                }
                else
                {
                    //if the user saves the file, all is good
                    bool ok = SaveFileAs();
                    return ok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        private bool SaveFileAs()
        {
            bool result = false;

            //Create a File and save it as a xml file
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Specify path";
            saveDialog.Filter = "All files (*.*)|*.*| Carbo Life Project File(*.clcx) | *.clcx";
            saveDialog.FilterIndex = 2;
            saveDialog.RestoreDirectory = true;

            saveDialog.ShowDialog();

            string Path = saveDialog.FileName;
            if (Path != "")
            {                
                //is true when succesfull
                result = SaveFile(Path, true);
            }
            else
            {
                result = false;
            }

            return result;
        }

        //When assembly cant be find bind to current
        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            System.Reflection.Assembly ayResult = null;
            string sShortAssemblyName = args.Name.Split(',')[0];
            System.Reflection.Assembly[] ayAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly ayAssembly in ayAssemblies)
            {
                if (sShortAssemblyName == ayAssembly.FullName.Split(',')[0])
                {
                    ayResult = ayAssembly;
                    break;
                }
            }
            return ayResult;
        }

        private void Mnu_About_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("This program was written by David In't Veld, and is provided AS IS. Any further queries contact me on: https://github.com/DavidVeld", "About Carbo Life Calculator",MessageBoxButton.OK,MessageBoxImage.Information);
            CarboAbout aboutWindow = new CarboAbout();
            aboutWindow.ShowDialog();
        }

        private void mnu_BuildReport_Click(object sender, RoutedEventArgs e)
        {
            if (carboLifeProject != null)
            {
                carboLifeProject.CalculateProject();

                Bitmap Chart1 = null;
                Bitmap Chart2 = null;
                Bitmap LetiChart = null;


                LiveCharts.Wpf.PieChart foundchart1 = Panel_Overview.pie_Chart1;
                LiveCharts.Wpf.PieChart foundchart2 = Panel_Overview.pie_Chart2;
                Canvas foundletiChart = Panel_Overview.cnv_Leti;


                if (foundchart1 != null)
                {
                    Chart1 = ChartUtils.ControlToImage(foundchart1, 300, 300);
                    //Chart1.Save(@"C:\Users\David\Documents\img1.jpg");
                }

                if (foundchart2 != null)
                {
                    Chart2 = ChartUtils.ControlToImage(foundchart2, 300, 300);
                }

                if (foundletiChart != null)
                {
                    LetiChart = ChartUtils.ControlToImage(foundletiChart, 300, 300);
                }


                ReportBuilder.CreateReport(carboLifeProject, Chart1, Chart2, LetiChart);
            }
        }

        private void mnu_CloseMe_Click(object sender, RoutedEventArgs e)
        {
                this.Close();
        }

        private void mnu_Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Please visit https://github.com/DavidVeld/CarboLifeCalc for resources and updates ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool fileSaved = false;

            if (carboLifeProject.justSaved == false)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save your project first?", "Warning", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    string Path = carboLifeProject.filePath;
                    if (File.Exists(Path))
                    {
                        bool ok = carboLifeProject.SerializeXML(Path);
                        if (ok == true)
                        {
                            MessageBox.Show("Project Saved");
                            //the file was saved
                            fileSaved = true;
                        }
                        else
                        {
                            MessageBox.Show("There was a problem while saving the file, please use save-as to re-save your file.");
                            fileSaved = false;
                            e.Cancel = true; //do not close window
                        }
                    }
                    else
                    {
                        bool ok = SaveFileAs();
                        if (ok == true)
                            fileSaved = true;
                    }
                }
                else if (result == MessageBoxResult.No)
                {
                    //The user didnt want to save
                    fileSaved = true;
                }
                else
                {
                    //the user cancels
                    fileSaved = false;
                    e.Cancel = true;
                }

            }
            else
            {
                //the file was already saved
                fileSaved = true;
            }


            if (fileSaved == false)
                e.Cancel = true; //do not close window, something went wrong, or user canceled the exit


            PathUtils.CleanOnlineDir();

        }
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mnu_Activate_Click(object sender, RoutedEventArgs e)
        {
            RevitActivator revitActivator = new RevitActivator();
            revitActivator.ShowDialog();
        }

        private void mnu_ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportPicker exportMenu = new ExportPicker();
            exportMenu.ShowDialog();

            if (exportMenu.isAccepted == true)
            {
                ExcelExportResult = exportMenu.results;
                ExcelExportElements = exportMenu.elements;
                ExcelExportMaterials = exportMenu.materials;

                //Async Attempt 2:

                if (carboLifeProject != null)
                {
                    carboLifeProject.CalculateProject();

                    //int ProgressToBeupdated = 100;

                    ExcelExportPath = DataExportUtils.GetSaveAsLocation();

                    if (ExcelExportPath != null && ExcelExportPath != "")
                    {
                        BackgroundWorker ExportThread = new BackgroundWorker();
                        ExportThread.WorkerReportsProgress = true;
                        ExportThread.DoWork += ExportThread_DoWork;
                        ExportThread.ProgressChanged += ExportThreadProgressChanged;
                        ExportThread.RunWorkerCompleted += ExportThreadCompleted;
                        ExportThread.RunWorkerAsync(new object());
                    }
                    else
                    {
                        MessageBox.Show("The file is open by another process, or cannot be openend, please specify another location");
                    }
                }

            }
        }
        void ExportThread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (ExcelExportPath != null || ExcelExportPath != "")
            {
                ExportExcel_Completed = false;

                BackgroundWorker ExportFile = new BackgroundWorker();
                ExportFile.WorkerReportsProgress = true;
                ExportFile.DoWork += ExportFile_DoWork;
                ExportFile.RunWorkerCompleted += ExportFile_Completed;
                ExportFile.RunWorkerAsync(new object());

                while (ExportExcel_Completed == false)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Thread.Sleep(100);
                        (sender as BackgroundWorker).ReportProgress(i);
                        if (i == 99)
                            i = 0;
                        if (ExportExcel_Completed == true)
                            break;
                    }
                }
            }
            //pgr_Exporter.Value = 0;
            //end export thread
        }

        private void ExportThreadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pgr_Exporter.Value = 0;

            if (File.Exists(ExcelExportPath))
            {
                System.Windows.MessageBox.Show("CVS export succesful, click OK to open!", "Success!", MessageBoxButton.OK);
                System.Diagnostics.Process.Start(ExcelExportPath);
            }

        }
        void ExportThreadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pgr_Exporter.Value = e.ProgressPercentage;
        }

        //The file works:
        private void ExportFile_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            ExportExcel_Completed = true;
        }
        private void ExportFile_DoWork(object sender, DoWorkEventArgs e)
        {
            //DataExportUtils.ExportToExcel(carboLifeProject, ExcelExportPath, ExcelExportResult, ExcelExportElements, ExcelExportMaterials);
            DataExportUtils.ExportToCSV(carboLifeProject, ExcelExportPath, ExcelExportResult, ExcelExportElements, ExcelExportMaterials);

        }

        private void mnu_Settings_Click(object sender, RoutedEventArgs e)
        {
            CarboSettingsMenu settingsWindow = new CarboSettingsMenu();
            settingsWindow.ShowDialog();
        }

    }
}
