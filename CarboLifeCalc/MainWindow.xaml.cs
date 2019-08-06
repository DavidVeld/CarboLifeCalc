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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CarboProject dummyProject = new CarboProject();
            dummyProject.GenerateDummyList();

            CarboLifeUI.UI.CarboLifeMainWindow CarboApp = new CarboLifeUI.UI.CarboLifeMainWindow(dummyProject);
            CarboApp.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CarboDatabase cd = new CarboDatabase();
            cd.DeSerializeXML("");

            CaboDatabaseManager dataBaseManager = new CaboDatabaseManager(cd);
            dataBaseManager.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
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
                    newProject = buffer.DeSerializeXML(openFileDialog.FileName);

                    CarboLifeUI.UI.CarboLifeMainWindow CarboApp = new CarboLifeUI.UI.CarboLifeMainWindow(newProject);
                    CarboApp.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
