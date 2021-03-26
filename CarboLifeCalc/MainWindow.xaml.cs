using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using CarboLifeAPI.Data;
using CarboLifeUI;
using CarboLifeUI.UI;
using Microsoft.Win32;

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
            CarboProject newProject = new CarboProject();
            Dispatcher.BeginInvoke(new Action(() => OpenProject(newProject)), DispatcherPriority.ContextIdle, null);
        }

        private void OpenProject(CarboProject project)
        {
            CarboLifeUI.UI.CarboLifeMainWindow CarboApp = new CarboLifeMainWindow(project);
            this.Visibility = Visibility.Hidden;
            CarboApp.ShowDialog();
            Environment.Exit(0);
            this.Close();
        }

        private void btn_Materials_Click(object sender, RoutedEventArgs e)
        {
            CarboProject newProject = new CarboProject();
            CarboDatabase cd = newProject.CarboDatabase;
            cd.DeSerializeXML("");
            if (newProject != null && cd != null && cd.CarboMaterialList.Count > 0)
            {
                MaterialEditor mateditor = new MaterialEditor(cd.CarboMaterialList[0].Name, cd);
                mateditor.ShowDialog();
            }
            else
            {
                MessageBox.Show("Template Database not found");
            }
        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Project File (*.clcx)|*.clcx|Carbo Life Project File (*.xml)| *.xml|All files (*.*)|*.*";

                var path = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "")
                {
                    CarboProject newProject = new CarboProject();

                    CarboProject buffer = new CarboProject();
                    newProject = buffer.DeSerializeXML(openFileDialog.FileName);

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
    }
}
