using CarboLifeAPI;
using CarboLifeAPI.Data;
using LiveCharts.Wpf.Charts.Base;
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
        public string ExcelExportPrefix { get; private set; }

        public bool ExportExcel_Completed { get; private set; }
        public bool ExcelExportResult { get; private set; }
        public bool ExcelExportElements { get; private set; }
        public bool ExcelExportMaterials { get; private set; }
        public bool ExcelExportProject { get; private set; }

        /////////////

        //public static CarboLifeMainWindow Instance;


        //public CarboDatabase carboDataBase { get; set; }
        [Obsolete]
        public CarboLifeMainWindow()
        {
            //UserPaths
            PathUtils.CheckFileLocationsNew();

            IsRevit = false;
            carboLifeProject = new CarboProject();

            try
            {
                InitializeComponent();
                //Instance = this;
            }
            catch (Exception ex)
            {
                // Log error (including InnerExceptions!)
                // Handle exception
            }
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

            CheckLic();
        }

        private void CheckLic()
        {
            CarboSettings setting = new CarboSettings();
            setting = setting.Load();

            //Hi, yes you ae welcome to take the secret message from the sourcecode
            //The software is free and the contribution is volontary
            //If you want to find out the secret message pass the string "LOmaRc4Q9HO7UU18crErdwvOQNXv8Hf+" through Utils.Decrypt
            //The return in the secret message.
            string secretmessage = setting.secretMessage;
            string secretEnCrypted = Utils.Crypt(secretmessage);

            if (secretEnCrypted == "LOmaRc4Q9HO7UU18crErdwvOQNXv8Hf+") 
            { 
                //No warnings
            }
            else
            {
                Random random = new Random();
                int randomNumber = random.Next(1, 5);
                if(randomNumber == 2)
                {
                    //only show message after certain publication date
                    DateTime startDate = new DateTime(2025, 3, 1);
                    DateTime currentDate = DateTime.Now;

                    if (currentDate > startDate)
                    {
                        string message =
    @"This is a friendly message to remind you that this software is free. 
You have a 1/5 chance to see each time you run this app. 
Do you want to buy me a coffee and you get a key to remove this message?";

                        MessageBoxResult result = System.Windows.MessageBox.Show(message, "Hello", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://buymeacoffee.com/davidveld");
                        }
                    }
                }
            }

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
                    TemplateSelector templateSelectionDialog = new TemplateSelector();
                    templateSelectionDialog.ShowDialog();

                    if (templateSelectionDialog.isAccepted == true)
                    {
                        string templatePath = templateSelectionDialog.selectedTemplateFile;

                        if (File.Exists(templatePath))
                        {
                            carboLifeProject = new CarboProject(templatePath);

                        }
                    }
                

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

                this.WindowState = WindowState.Maximized;
                //this.WindowStyle = WindowStyle.None;

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
                ExcelExportProject = exportMenu.project;

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
                        MessageBox.Show("The file is open by another process, or cannot be opened, please specify another location.");
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

            string exportpath = System.IO.Path.GetDirectoryName(ExcelExportPath);
            string prefix = System.IO.Path.GetFileNameWithoutExtension(ExcelExportPath);

            if (prefix != "")
                prefix = prefix + "_";

            string exportpathResult = exportpath + "\\" + prefix + "Results.csv";
            string exportpathElements = exportpath + "\\" + prefix + "Elements.csv";
            string exportpathMaterials = exportpath + "\\" + prefix + "Materials.csv";
            string exportpathProject = exportpath + "\\" + prefix + "Project.csv";



            if (File.Exists(exportpathResult) || File.Exists(exportpathElements) || File.Exists(exportpathMaterials) || File.Exists(exportpathProject))
            {
                System.Windows.MessageBox.Show("CSV export successful. Click OK to open export directory.", "Success!", MessageBoxButton.OK);
                System.Diagnostics.Process.Start("explorer.exe", exportpath);
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
            //MessageBox.Show("Export completed");

            //string dirPath = System.IO.Path.GetDirectoryName(ExcelExportPath);
            //if(System.IO.Directory.Exists(dirPath))
            //    System.Diagnostics.Process.Start("explorer.exe", dirPath);

        }
        private void ExportFile_DoWork(object sender, DoWorkEventArgs e)
        {
            //DataExportUtils.ExportToExcel(carboLifeProject, ExcelExportPath, ExcelExportResult, ExcelExportElements, ExcelExportMaterials);
            DataExportUtils.ExportToCSV(carboLifeProject, ExcelExportPath, ExcelExportResult, ExcelExportElements, ExcelExportMaterials, ExcelExportProject);

        }

        private void mnu_Settings_Click(object sender, RoutedEventArgs e)
        {
            CarboSettingsMenu settingsWindow = new CarboSettingsMenu();
            settingsWindow.ShowDialog();
        }

        private void mnu_EditTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CarboSettings settings = new CarboSettings();
                settings = settings.Load();
                string Startpath = settings.templatePath;
                string pathForViewing = System.IO.Path.GetDirectoryName(Startpath);

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Material File (*.cxml)|*.cxml|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = pathForViewing;

                var path = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName))
                {
                    CarboProject newProject = new CarboProject();
                    CarboDatabase bufferDatabase = newProject.CarboDatabase;

                    CarboDatabase cd = bufferDatabase.DeSerializeXML(openFileDialog.FileName);

                    MaterialEditor mateditor = new MaterialEditor(cd.CarboMaterialList[0].Name, cd);
                    mateditor.ShowDialog();

                    if (mateditor.acceptNew == true)
                    {
                        CarboDatabase database = mateditor.returnedDatabase;
                        database.SerializeXML(openFileDialog.FileName);
                    }
                    else
                    {

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Drawing.Rectangle resolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            int height = resolution.Height;

            if(height > 768)
            {
                this.Height = (height - 100);

                //centre the window 

                double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                double windowWidth = this.Width;
                double windowHeight = this.Height;
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);

            }

        }

        private void mnu_ExportToJSON_Click(object sender, RoutedEventArgs e)
        {

            string path = JsonExportUtils.GetSaveAsLocation();
            if (path != null)
            {
                bool ok = JsonExportUtils.ExportToJson(path, carboLifeProject);
                if(ok == true)
                {
                    MessageBox.Show("File exported to: " + path);
                }
            }
        }

        private void mnu_ExportToLCAx_Click(object sender, RoutedEventArgs e)
        {
            string path = JsonExportUtils.GetSaveAsLocation();
            if (path != null)
            {
                bool ok = JsonExportUtils.ExportToLCAx(path, carboLifeProject);
                if (ok == true)
                {
                    MessageBox.Show("File exported to: " + path);
                }
            }
        }

        private void mnu_ImportElements_Click(object sender, RoutedEventArgs e)
        {

            DataImportDialog elementImportDialog = new DataImportDialog();
            elementImportDialog.ShowDialog();

            if (elementImportDialog.isAccepted == true)
            {
                List<CarboElement> elements = elementImportDialog.elementList;
                if (elements.Count > 0)
                {
                    try
                    {
                        //
                        carboLifeProject.DeleteAllGroups();

                        foreach (CarboElement ce in elements)
                        {
                            carboLifeProject.AddorUpdateElement(ce);
                        }

                        //run once;
                        carboLifeProject.CreateGroups();
                        //carboLifeProject.CreateReinforcementGroup();
                        carboLifeProject.CalculateProject();
                    }
                    catch (Exception ex)
                    {

                    }
                    
                }
            }
        }

        private void mnu_ImportLCAx_Click(object sender, RoutedEventArgs e)
        {
            string path = JsonExportUtils.GetLCAxFileLocation();
            if (path != null && path != "")
            {
                CarboProject carboLifeProject = null;
                bool ok = JsonExportUtils.openLCAx(path,out carboLifeProject);
                if (ok == true)
                {
                    this.carboLifeProject = carboLifeProject;
                    
                }
            }
        }

        private void mnu_ExportToOneClick_Click(object sender, RoutedEventArgs e)
        {
            //get a location and save the data
            if (carboLifeProject != null)
            {
                carboLifeProject.CalculateProject();

                //get a CSV location:
                string savePath = DataExportUtils.GetSaveAsLocation();

                if (savePath != null && savePath != "")
                {
                    DataExportUtils.ExportToOneClick(carboLifeProject, savePath);
                }
            }
        }

    }
}
