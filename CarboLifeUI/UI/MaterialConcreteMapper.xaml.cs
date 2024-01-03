using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialConstructionPicker.xaml
    /// </summary>
    public partial class MaterialConcreteMapper : Window
    {
        internal bool isAccepted;
        public string sourcePath;
        public List<CarboMapElement> mappinglist { get; set; }
        //public CarboMaterialList materialList { get; set; }
        public List<CategoryName> categoryList { get; set; }


        //public List<Person> Persons { get; set; }
        //public List<MyLocation> Locations { get; set; }


        public MaterialConcreteMapper(CarboProject carboProject)
        {
            List<CarboMaterial> list = carboProject.CarboDatabase.CarboMaterialList;



            this.InitializeComponent();

            mappinglist = new List<CarboMapElement>();

            foreach(CarboGroup cg in carboProject.getGroupList)
            {
                //Only add if they have revit elements
                if (cg.AllElements.Count > 0)
                {
                    CarboMapElement mapElement = new CarboMapElement();

                    mapElement.revitName = cg.AllElements[0].MaterialName;
                    mapElement.carboNAME = cg.MaterialName;
                    mapElement.category = cg.Category;

                    mappinglist.Add(mapElement);
                }
            }

            //Get all the Categories
            categoryList = new List<CategoryName>();
            foreach (CarboMaterial cm in list)
            {
                categoryList.Add(new CategoryName { Name = cm.Name });
            }

            DataContext = this;

        }

        public MaterialConcreteMapper()
        {
           // this.InitializeComponent();

            //Locations = new List<MyLocation> { new MyLocation { Location = "London", NAMEID = 1 }, new MyLocation { Location = "Amsterdam" } };
            //Persons = new List<Person> { new Person { NAME = "Jack", NAMEID = 1 }, new Person { NAME = "Jill", NAMEID = 2 } };

            //DataContext = this;
        }

        private List<CarboMapElement> GenerateMappinglist(CarboProject returnedDatabase)
        {
            List<CarboMapElement> result = new List<CarboMapElement>
            {
                new CarboMapElement{revitName = "Revit Material 1", carboNAME = "Carbo Material 1" },
                new CarboMapElement{revitName = "Revit Material 2", carboNAME = "Carbo Material 2" }
            };

        return result;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ViewModel vm = new ViewModel();
            //dgData.ItemsSource = vm;
        }


        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            this.Close();
        }

        public IEnumerable<DataGridRow> GetDataGridRows(System.Windows.Controls.DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as System.Collections.IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }



        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

    }

    public class CategoryName
    {
        public string Name { get; set; }
    }


}
