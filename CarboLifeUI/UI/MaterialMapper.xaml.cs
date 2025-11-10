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
    public partial class MaterialMapper : Window
    {
        internal bool isAccepted;
        public string sourcePath;
        public List<CarboMapElement> mappinglist { get; set; }
        //public CarboMaterialList materialList { get; set; }
        public List<CarboName> materialList { get; set; }

        public MaterialMapper(CarboProject carboProject)
        {
            List<CarboMaterial> list = carboProject.CarboDatabase.CarboMaterialList.OrderBy(o => o.Name).ToList();
            //list.Sort();


            this.InitializeComponent();

            try
            {
                mappinglist = new List<CarboMapElement>();
                mappinglist = Utils.GenerateMappinglist(carboProject);

                materialList = new List<CarboName>();
                foreach (CarboMaterial cm in list)
                {
                    materialList.Add(new CarboName { carboNAME = cm.Name });
                }

                //filteredMaterialList = new ObservableCollection<CarboName>(materialList);
                //DataContext = this;

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error generating mapping list: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            DataContext = this;

        }

        public MaterialMapper()
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

            if(chk_SaveMappingFile.IsChecked == true)
            {
                try
                {
                    CarboMapFile CurrentMappingFile = new CarboMapFile();
                    CurrentMappingFile.mappingTable = mappinglist;
                    //CurrentMappingFile.SaveToXml();

                    CarboMapFile SavedMappingFile = new CarboMapFile();
                    SavedMappingFile = CarboMapFile.LoadFromXml();

                    if (SavedMappingFile != null)
                    {
                        SavedMappingFile.Merge(CurrentMappingFile.mappingTable);
                        SavedMappingFile.SaveToXml();
                    }
                    else
                    {
                        //The mapping file needs to be created
                        CurrentMappingFile.SaveToXml();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error saving mapping file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

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

        private void ComboBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                // Ignore navigation keys (optional but recommended)
                if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Escape)
                    return;

                var comboBox = sender as System.Windows.Controls.ComboBox;
                if (comboBox == null)
                    return;

                // Safely get user text
                string currentText = comboBox.Text?.Trim() ?? "";
                string searchText = currentText.ToLower();

                // Get the full material list from the window
                var fullList = this.materialList;
                if (fullList == null)
                    return;

                // Filter results
                var filtered = string.IsNullOrEmpty(searchText)
                    ? fullList
                    : fullList
                        .Where(x => x.carboNAME != null && x.carboNAME.ToLower().Contains(searchText))
                        .ToList();

                // Temporarily detach event to prevent recursive refresh
                comboBox.SelectionChanged -= ComboBox_SelectionChanged;

                // Assign filtered list
                comboBox.ItemsSource = filtered;

                // Reattach event
                comboBox.SelectionChanged += ComboBox_SelectionChanged;

                // Restore text (so typed letter doesn’t disappear)
                comboBox.Text = currentText;

                // Keep dropdown open for live filtering
                comboBox.IsDropDownOpen = true;

                // Move caret to end of text
                SetComboBoxCaretToEnd(comboBox);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Error while filtering ComboBox items:\n\n" + ex.Message,
                    "Filtering Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }



        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox comboBox = sender as System.Windows.Controls.ComboBox;
            comboBox.ItemsSource = materialList;
        }
        private void SetComboBoxCaretToEnd(System.Windows.Controls.ComboBox comboBox)
        {
            try
            {
                if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is System.Windows.Controls.TextBox textBox)
                {
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "Error setting caret position:\n\n" + ex.Message,
                    "Caret Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }


    }

    public class CarboName
    {
        public string carboNAME { get; set; }
    }


}
