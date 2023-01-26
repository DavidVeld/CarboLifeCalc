using CarboLifeAPI.Data;
using Microsoft.SqlServer.Server;
using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace CarboLifeCalc
{
    /// <summary>
    /// Interaction logic for ChartTest.xaml
    /// </summary>
    public partial class ChartTest : Window
    {
        private CarboProject carboProject;

        public ChartTest()
        {
            InitializeComponent();
        }

        private void btn_Chart_Click(object sender, RoutedEventArgs e)
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
                //Get all the visible elements (all project)

                UpdateDataSource();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateDataSource()
        {
            throw new NotImplementedException();
        }
    }
}
