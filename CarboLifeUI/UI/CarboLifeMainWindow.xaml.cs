using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        //public CarboDatabase carboDataBase { get; set; }
        public CarboLifeMainWindow()
        {
            carboLifeProject = new CarboProject();
            InitializeComponent();
        }

        public CarboLifeMainWindow(CarboProject myProject)
        {
            carboLifeProject = myProject;
            //carboDataBase = carboDataBase.DeSerializeXML("");

            carboLifeProject.CreateGroups();
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

        private void Mnu_openDataBasemanager_Click(object sender, RoutedEventArgs e)
        {
            CaboDatabaseManager dataBaseManager = new CaboDatabaseManager(carboLifeProject.CarboDatabase);
            dataBaseManager.ShowDialog();

        }
    }
}
