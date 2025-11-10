using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;
using CarboLifeAPI.Data;
using CarboLifeUI;
using CarboLifeUI.UI;
using Microsoft.Win32;
using Path = System.IO.Path;
//using CarboCircle;
using System.Security.Cryptography;
using CarboLifeAPI;

namespace CarboLifeCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_Launch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SkiaSharp.Views.WPF.dll"));
                //
                Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "System.Drawing.Common.dll"));

            }
            catch
            {

            }

            CarboProject newProject = new CarboProject();
            Dispatcher.BeginInvoke(new Action(() => OpenProject(newProject)), DispatcherPriority.ContextIdle, null);
        }

        private void OpenProject(CarboProject project)
        {
            try
            {
                Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SkiaSharp.Views.WPF.dll"));
                //
                Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "System.Drawing.Common.dll"));

            }
            catch
            {

            }

            CarboLifeUI.UI.CarboLifeMainWindow CarboApp = new CarboLifeMainWindow(project);
            //this.Visibility = Visibility.Hidden;
            CarboApp.ShowDialog();
            //Environment.Exit(0);
            //this.Close();
        }

        private void btn_Materials_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CarboSettings settings = new CarboSettings();
                settings = settings.Load();
                string Startpath = settings.templatePath;
                string pathForViewing = Path.GetDirectoryName(Startpath);

                string pathToOpen = Utils.OpenCarboMaterialLibrary(pathForViewing);

                if (pathToOpen != "")
                {
                    CarboProject newProject = new CarboProject();
                    CarboDatabase bufferDatabase = newProject.CarboDatabase;

                    CarboDatabase cd = bufferDatabase.DeSerializeXML(pathToOpen);

                    MaterialEditor mateditor = new MaterialEditor(cd.CarboMaterialList[0].Name, cd);
                    mateditor.ShowDialog();

                    if (mateditor.acceptNew == true)
                    {
                        CarboDatabase database = mateditor.returnedDatabase;
                        database.SerializeXML(pathToOpen);
                    }
                    else
                    {
                        MessageBox.Show("Closed without saving");
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string FilePath = Utils.OpenCarboProject();


                if (FilePath != "" && File.Exists(FilePath))
                {
                    CarboProject newProject = new CarboProject();

                    CarboProject buffer = new CarboProject();
                    newProject = buffer.DeSerializeXML(FilePath);
                    newProject.justSaved = true;

                    Dispatcher.BeginInvoke(new Action(() => OpenProject(newProject)), DispatcherPriority.ContextIdle, null);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Addin_Click(object sender, RoutedEventArgs e)
        {
            CarboLifeUI.UI.RevitActivator revitActivator = new RevitActivator();
            revitActivator.ShowDialog();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true // Required to open URLs or launch with default apps
                };
                Process.Start(psi);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening link: {ex.Message}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CarboLifeAPI.PathUtils.CheckFileLocationsNew();
            //Assembly.GetExecutingAssembly().GetName().Version.ToString();
            lbl_Version.Content = "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string a = lbl_Version.Content.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CarboLifeAPI.PathUtils.CleanOnlineDir();
        }

        private void btn_Test_Click(object sender, RoutedEventArgs e)
        {
            string showme = CarboLifeAPI.Utils.Crypt("CarboLife");
            Clipboard.SetText(showme);
            MessageBox.Show(showme);
        }


    }

  
}
