using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
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
using LiveCharts;
using LiveCharts.Wpf;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialBasePicker.xaml
    /// </summary>
    public partial class MaterialSelector : Window
    {
        public bool isAccepted;
        /// <summary>
        /// The base material
        /// </summary>
        public CarboMaterial currentMaterial;
        /// <summary>
        /// The selected material
        /// </summary>
        public CarboMaterial selectedMaterial;
        /// <summary>
        /// The entire material database in the current project
        /// </summary>
        public CarboDatabase materialDatabase;

        public MaterialSelector(string selectedMaterialName, CarboDatabase database)
        {
            isAccepted = false;

            try
            {
                //originalDatabase = database;
                materialDatabase = database;
                currentMaterial = database.GetExcactMatch(selectedMaterialName);

                if (currentMaterial == null)
                {
                    MessageBox.Show("This material could not be found in the database, a closest match will now be found for comparison");
                    currentMaterial = database.getClosestMatch(selectedMaterialName);
                }
                selectedMaterial = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            List<string> materialCategories = new List<string>();
            materialCategories = materialDatabase.getCategoryList();

            materialCategories.Sort();

            foreach (string cat in materialCategories)
            {
                cbb_Categories.Items.Add(cat);
            }

            cbb_Categories.Items.Add("All");
            if (currentMaterial != null)
            {
                cbb_Categories.Text = currentMaterial.Category;
            }
            else
            {
                cbb_Categories.Text = "All";

            }

            RefreshMaterialList();
            UpdateSelectedMaterial();
        }

        private void RefreshMaterialList()
        {
            //selectedMaterial = null;
            string cat = cbb_Categories.Text;
            string searchtext = txt_Search.Text;

            liv_materialList.Items.Clear();

            foreach (CarboMaterial cm in materialDatabase.CarboMaterialList)
            {
                if (cm.Category == cat || cat == "All" || cat == "")
                {
                    //Search Bar
                    int hit = cm.Name.IndexOf(searchtext, StringComparison.OrdinalIgnoreCase);
                    if (searchtext == "" || hit >= 0)
                    {
                        liv_materialList.Items.Add(cm);
                    }
                }
            }
            if (selectedMaterial != null)
                liv_materialList.SelectedItem = selectedMaterial;

            //Sort list
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(liv_materialList.Items);

            if (view != null)
            {
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Category", System.ComponentModel.ListSortDirection.Ascending));
            }

        }


        private void UpdateSelectedMaterial()
        {
            try
            {
                if (currentMaterial != null)
                {
                    lbl_Current.Content = "Current: " + currentMaterial.Name;
                    txt_CurrentValue.Text = Math.Round(currentMaterial.ECI,3).ToString();
                }
                if (selectedMaterial != null)
                {
                    lbl_Selectedname.Content = "Selected: " + selectedMaterial.Name;
                    txt_SelectedValue.Text = Math.Round(selectedMaterial.ECI,3).ToString();
                }
                //Get and compares the existing and selected material
                if (currentMaterial != null && selectedMaterial != null)
                {
                    Buildgraph();
                }
            }
            catch
            {

            }
            
        }

        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        private void Buildgraph()
        {
            SeriesCollection compareBar = new SeriesCollection();

            //Build series
            compareBar = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Values = new ChartValues<double>
                    {
                        Math.Round(currentMaterial.ECI_A1A3,2),
                        Math.Round(selectedMaterial.ECI_A1A3,2)
                    },
                    StackMode = StackMode.Values,
                    DataLabels = true,
                    Title = "A1-A3"

                },
                new StackedColumnSeries
                {
                    Values = new ChartValues<double>
                    {
                        Math.Round(currentMaterial.ECI_A4,2),
                        Math.Round(selectedMaterial.ECI_A4,2)
                    },
                    StackMode = StackMode.Values,
                    DataLabels = true,
                    Title = "A4"
                },
                new StackedColumnSeries
                {
                    Values = new ChartValues<double>
                    {
                        Math.Round(currentMaterial.ECI_A5,2),
                        Math.Round(selectedMaterial.ECI_A5,2)
                    },
                    StackMode = StackMode.Values,
                    DataLabels = true,
                    Title = "A5"
                },
                new StackedColumnSeries
                {
                    Values = new ChartValues<double>
                    {
                        Math.Round(currentMaterial.ECI_C1C4,2),
                        Math.Round(selectedMaterial.ECI_C1C4,2)
                    },
                    StackMode = StackMode.Values,
                    DataLabels = true,
                    Title = "C1-C4"
                },
                new StackedColumnSeries
                {
                    Values = new ChartValues<double>
                    {
                        Math.Round(currentMaterial.ECI_D,2),
                        Math.Round(selectedMaterial.ECI_D,2)
                    },
                    StackMode = StackMode.Values,
                    DataLabels = true,
                    Title = "D"
                },
                new StackedColumnSeries
                {
                    Values = new ChartValues<double>
                    {
                        Math.Round(currentMaterial.ECI_Mix,2),
                        Math.Round(selectedMaterial.ECI_Mix,2)
                    },
                    StackMode = StackMode.Values,
                    DataLabels = true,
                     Title = "Added"
                }
            };

            //adding series updates and animates the chart
            /*
            compareBar.Add(new StackedColumnSeries
            {
                Values = new ChartValues<double> { 6, 2, 7 },
                StackMode = StackMode.Values
            });
            */
            //adding values also updates and animates
            //compareBar[1].Values.Add(4d);


            Labels = new[] { "Current", "Selected" };

            Func<double, string> Formatter = value => value + " kgCO₂/kg";
            DataContext = this;

            barchart.Series = compareBar;
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void Cbb_Categories_DropDownClosed(object sender, EventArgs e)
        {
            RefreshMaterialList();
        }

        private async void Txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                RefreshMaterialList();
            }
        }

        private void Liv_materialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedMaterial = liv_materialList.SelectedItem as CarboMaterial;
            //MaterialMap selectedFloorMap = GetMaterialMap(cbb_FloorTypes.Text);

            if (selectedMaterial != null)
            {
                UpdateSelectedMaterial();
            }
        }
    }
}
