using CarboLifeAPI.Data;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
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
                    txt_CurrentValue.Text = Math.Round(currentMaterial.ECI - currentMaterial.ECI_D, 3).ToString();
                }
                if (selectedMaterial != null)
                {
                    lbl_Selectedname.Content = "Selected: " + selectedMaterial.Name;
                    txt_SelectedValue.Text = Math.Round(selectedMaterial.ECI - currentMaterial.ECI_D, 3).ToString();
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
            List<ISeries> compareBarV2 = new List<ISeries>();
            List<ISeries> compareBar = new List<ISeries>();

            compareBarV2.Add(new StackedColumnSeries<double>
            {
                Values = new List<double>
                    {
                        Math.Round(currentMaterial.ECI_Seq,2),
                        Math.Round(selectedMaterial.ECI_Seq,2)
                    },
                Name = "Sequestration",
                Fill = new SolidColorPaint(GraphBuilder.getSKColour(1)),
                YToolTipLabelFormatter = point => $"{point.Model:0.000} kgCO₂e",
                DataLabelsSize = 11
                //string.Format("{0} tCO₂e", chartPoint.Y

            });

            //Build series
            compareBarV2.Add(new StackedColumnSeries<double>
            {
                Values = new List<double>
                    {
                        Math.Round(currentMaterial.ECI_A1A3,2),
                        Math.Round(selectedMaterial.ECI_A1A3,2)
                    },
                Name = "A1-A3",
                Fill = new SolidColorPaint(GraphBuilder.getSKColour(2)),
                YToolTipLabelFormatter = point => $"{point.Model:0.000} kgCO₂e",
                DataLabelsSize = 11
                //string.Format("{0} tCO₂e", chartPoint.Y

            });

            compareBarV2.Add(new StackedColumnSeries<double>
            {
                Values = new List<double>
                    {
                        Math.Round(currentMaterial.ECI_A4,2),
                        Math.Round(selectedMaterial.ECI_A4,2)
                    },
                Name = "A4",
                Fill = new SolidColorPaint(GraphBuilder.getSKColour(3)),
                YToolTipLabelFormatter = point => $"{point.Model:0.000} kgCO₂e",
                DataLabelsSize = 11
                //string.Format("{0} tCO₂e", chartPoint.Y

            });

            compareBarV2.Add(new StackedColumnSeries<double>
            {
                Values = new List<double>
                    {
                        Math.Round(currentMaterial.ECI_A5,2),
                        Math.Round(selectedMaterial.ECI_A5,2)
                    },
                Name = "A5",
                Fill = new SolidColorPaint(GraphBuilder.getSKColour(4)),
                YToolTipLabelFormatter = point => $"{point.Model:0.000} kgCO₂e",
                DataLabelsSize = 11
                //string.Format("{0} tCO₂e", chartPoint.Y

            });

            compareBarV2.Add(new StackedColumnSeries<double>
            {
                Values = new List<double>
                    {
                        Math.Round(currentMaterial.ECI_C1C4,2),
                        Math.Round(selectedMaterial.ECI_C1C4,2)
                    },
                Name = "C1-C4",
                Fill = new SolidColorPaint(GraphBuilder.getSKColour(5)),
                YToolTipLabelFormatter = point => $"{point.Model:0.000} kgCO₂e",
                DataLabelsSize = 11
                //string.Format("{0} tCO₂e", chartPoint.Y

            });

            compareBarV2.Add(new StackedColumnSeries<double>
            {
                Values = new List<double>
                    {
                        Math.Round(currentMaterial.ECI_D,2),
                        Math.Round(selectedMaterial.ECI_D,2)
                    },
                Name = "D",
                Fill = new SolidColorPaint(GraphBuilder.getSKColour(6)),
                YToolTipLabelFormatter = point => $"{point.Model:0.000} kgCO₂e",
                DataLabelsSize = 11
                //string.Format("{0} tCO₂e", chartPoint.Y

            });



            Labels = new[] { "Current", "Selected" };

            DataContext = this;


            List<ICartesianAxis> xaxis = new List<ICartesianAxis>();
            List<ICartesianAxis> yaxis = new List<ICartesianAxis>();
            List<string> elements = new List<string> { "Selected", "Current" };

            xaxis.Add(
                new Axis
                {
                    LabelsRotation = 0,
                    Labels = elements,
                    TextSize = 12
                });

            yaxis.Add(
                new Axis
                {
                    Name = "Intensity (kgCO₂e)",
                    NameTextSize = 11,
                    Labeler = value => $"{value:0.000} ",
                    TextSize = 12
                
                });

            barchart.Series = compareBarV2;
            barchart.XAxes = xaxis.ToArray();
            barchart.YAxes= yaxis.ToArray();
            barchart.LegendTextSize = 12;
            barchart.TooltipTextSize = 12;

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
