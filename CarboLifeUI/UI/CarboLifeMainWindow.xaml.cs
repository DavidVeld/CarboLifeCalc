using CarboLifeAPI;
using CarboLifeAPI.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        public bool createHeatmap {get; set;}

        //public CarboDatabase carboDataBase { get; set; }
        public CarboLifeMainWindow()
        {
            IsRevit = false;
            carboLifeProject = new CarboProject();
            InitializeComponent();
        }

        public CarboLifeMainWindow(CarboProject myProject)
        {
            carboLifeProject = myProject;
            IsRevit = false;
            //carboDataBase = carboDataBase.DeSerializeXML("");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            //carboLifeProject.CreateGroups();
            InitializeComponent();
        }
        private void Menu_Loaded(object sender, RoutedEventArgs e)
        {
            //Delete log
            string fileName = "db\\log.txt";
            string logPath = Utils.getAssemblyPath() + "\\" + fileName;

            if (File.Exists(logPath))
                File.Delete(logPath);

            Utils.WriteToLog("New Log Started: " + carboLifeProject.Name);
        }

        internal CarboProject getCarbonLifeProject()
        {
            if (carboLifeProject != null)
                return carboLifeProject;
            else
                return null;
        }

        private void Mnu_openDataBasemanager_Click(object sender, RoutedEventArgs e)
        {
            CaboDatabaseManager dataBaseManager = new CaboDatabaseManager(carboLifeProject.CarboDatabase);
            dataBaseManager.ShowDialog();

            if (dataBaseManager.isOk == true)
            {
                carboLifeProject.CarboDatabase = dataBaseManager.UserMaterials;
                carboLifeProject.UpdateAllMaterials();
                DataScreen.Visibility = Visibility.Hidden;
                DataScreen.Visibility = Visibility.Visible;
            }

        }

        private void Mnu_saveDataBase_Click(object sender, RoutedEventArgs e)
        {
            //Create a File and save it as a xml file
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Specify path";
            saveDialog.Filter = "XML Files|*.xml";
            saveDialog.FilterIndex = 2;
            saveDialog.RestoreDirectory = true;

            saveDialog.ShowDialog();

            string Path = saveDialog.FileName;
            if (Path != "")
            {
                bool ok = carboLifeProject.SerializeXML(Path);
                if (ok == true)
                    MessageBox.Show("Project Saved");
            }
        }

        private void Mnu_openDataBase_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Save First?", "Warning", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";

                    var path = openFileDialog.ShowDialog();

                    if (openFileDialog.FileName != "")
                    {
                        CarboProject newProject = new CarboProject();

                        CarboProject buffer = new CarboProject();
                        carboLifeProject = buffer.DeSerializeXML(openFileDialog.FileName);

                        carboLifeProject.Audit();

                        tab_Main.Visibility = Visibility.Hidden;
                        tab_Main.Visibility = Visibility.Visible;



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
            string Path = carboLifeProject.filePath;
            if(File.Exists(Path))
            {
                bool ok = carboLifeProject.SerializeXML(Path);
                if (ok == true)
                    MessageBox.Show("Project Saved");
            }
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
            MessageBox.Show("This program was written by David In't Veld, and is provided AS IS. Any further queries contact me on: davidveld@gmail.com","About Carbo Life Calculator",MessageBoxButton.OK,MessageBoxImage.Information);
        }

        private void mnu_BuildReport_Click(object sender, RoutedEventArgs e)
        {
            if (carboLifeProject != null)
                CarboLifeAPI.ReportBuilder.CreateReport(carboLifeProject);
        }

        private void mnu_CloseMe_Click(object sender, RoutedEventArgs e)
        {
                this.Close();
        }

        private void mnu_Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Help files tbc, please visit www.davidveld.nl for resources and updates ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to save your project? ", "Warning", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                string Path = carboLifeProject.filePath;
                if (File.Exists(Path))
                {
                    bool ok = carboLifeProject.SerializeXML(Path);
                    if (ok == true)
                    {
                        MessageBox.Show("Project Saved");
                    }
                }
            }
            else
            {
            }
        }

        private void mnu_Heatmap_Click(object sender, RoutedEventArgs e)
        {
            IsRevit = true;
            if (IsRevit == true)
            {
                HeatMapBuilder heatmapBuilder = new HeatMapBuilder();
                heatmapBuilder.Total = carboLifeProject.getTotalEC();

                heatmapBuilder.ShowDialog();
                if (heatmapBuilder.isAccepted == true)
                {
                    MessageBox.Show("The date to build a heat map image will be stored within the calculation, after you close the aplication, Revit will colour in your view.", "Warning", MessageBoxButton.OK);
                    if (heatmapBuilder.rad_Bymaterial.IsChecked == true)
                    {
                        carboLifeProject.CreateMaterialHeat();
                    }
                    else if(heatmapBuilder.rad_ByGroup.IsChecked == true)
                    {
                        carboLifeProject.CreateGroupHeat();
                    }
                    else if (heatmapBuilder.rad_ByElement.IsChecked == true)
                    {
                        carboLifeProject.CreateElementHeat();
                    }

                    chx_AcceptHeatmap.Visibility = Visibility.Visible;
                    chx_AcceptHeatmap.IsChecked = true;
                    lbl_AcceptHeatmap.Visibility = Visibility.Visible;
                    createHeatmap = true;


                }
            }
            else
            {
                MessageBox.Show("This feature is available once you launch the program from Autodesk Revit");
            }
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void chx_AcceptHeatmap_Click(object sender, RoutedEventArgs e)
        {
            if (chx_AcceptHeatmap.IsChecked == true)
                createHeatmap = true;
            else
                createHeatmap = false;

        }
    }
}
