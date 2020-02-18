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
using LiveCharts;
using LiveCharts.Wpf;
using CarboLifeAPI;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        public CarboProject CarboLifeProject;

        public Overview()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();
        }

        public Overview(CarboProject carboLifeProject)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();

            CarboLifeProject = carboLifeProject;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {

                DependencyObject parent = VisualTreeHelper.GetParent(this);
                Window parentWindow = Window.GetWindow(parent);
                CarboLifeMainWindow mainViewer = parentWindow as CarboLifeMainWindow;

                if (mainViewer != null)
                    CarboLifeProject = mainViewer.getCarbonLifeProject();

                if (CarboLifeProject != null)
                {
                    //A project Is loaded, Proceed to next

                    RefreshInterFace();
                }

                //Rebuild the materialList

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshInterFace()
        {
            try
            {
                if (CarboLifeProject != null)
                {
                    //cnv_Summary.Children.Clear();
                    List<CarboDataPoint> PieceListMaterial = new List<CarboDataPoint>();
                    List<CarboDataPoint> PieceListLifePoint = new List<CarboDataPoint>();

                    //List<KeyValuePair<string, double>> valueListMaterial = new List<KeyValuePair<string, double>>();
                    //List<KeyValuePair<string, double>> valueListLifePoint = new List<KeyValuePair<string, double>>();

                    PieceListMaterial = CarboLifeProject.getTotals("Material");
                    PieceListLifePoint = CarboLifeProject.getTotals("");
                    
                    PieceListMaterial = PieceListMaterial.OrderByDescending(o => o.Value).ToList();

                    Style st = new Style();


                    Func<ChartPoint, string> labelPoint = chartPoint =>
    string.Format("{0} tCO2 ({1:P})", chartPoint.Y, chartPoint.Participation);

                    SeriesCollection pieMaterialSeries = new SeriesCollection();
                    SeriesCollection pieLifeSeries = new SeriesCollection();

                    foreach (CarboDataPoint ppin in PieceListMaterial)
                    {
                        PieSeries newSeries = new PieSeries
                        {
                            Title = ppin.Name,
                            Values = new ChartValues<double> { Math.Round(ppin.Value,0) },
                            PushOut = 5,
                            DataLabels = true,
                            LabelPoint = labelPoint   
                        };

                        pieMaterialSeries.Add(newSeries);

                    }

                    foreach (CarboDataPoint ppin in PieceListLifePoint)
                    {
                        PieSeries newSeries = new PieSeries
                        {
                            Title = ppin.Name,
                            Values = new ChartValues<double> { Math.Round(ppin.Value, 0) },
                            PushOut = 5,
                            DataLabels = true,
                            LabelPoint = labelPoint
                        };

                        pieLifeSeries.Add(newSeries);

                    }

                    pie_Chart1.Series = pieMaterialSeries;
                    pie_Chart2.Series = pieLifeSeries;
                    
                    txt_ProjectName.Text = CarboLifeProject.Name;
                    txt_Number.Text = CarboLifeProject.Number;
                    txt_Category.Text = CarboLifeProject.Category;
                    txt_Desctiption.Text = CarboLifeProject.Description;

                    txt_Area.Text = CarboLifeProject.Area.ToString();
                    txt_Value.Text = CarboLifeProject.Value.ToString();

                    //Totals
                    refreshSumary();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void refreshSumary()
        {
            if (CarboLifeProject != null)
            {
                double totalCO2 = CarboLifeProject.getTotalsGroup().EC;

                TextBlock summaryText = new TextBlock();
                summaryText.Text = "Total: " + Math.Round(totalCO2,2) + " MtCO2e (Metric tons of carbon dioxide equivalent)" + Environment.NewLine + 
                    "This equals to: " + Math.Round(totalCO2 / 68.5, 2) + " average car emission per year. (UK)";

                summaryText.Foreground = Brushes.Black;
                summaryText.TextWrapping = TextWrapping.WrapWithOverflow;
                summaryText.VerticalAlignment = VerticalAlignment.Top;
                summaryText.FontSize = 16;

                Canvas.SetLeft(summaryText, 5);
                Canvas.SetTop(summaryText, 5);
                cnv_Totals.Children.Add(summaryText);
            }

        }

        private void Cnv_Summary_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;

        }
      
        private void SaveSettings()
        {
            CarboLifeProject.Name = txt_ProjectName.Text;
            CarboLifeProject.Number = txt_Number.Text;
            CarboLifeProject.Category = txt_Category.Text;
            CarboLifeProject.Description = txt_Desctiption.Text;

            CarboLifeProject.Area = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Area.Text);
            CarboLifeProject.Value = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Value.Text);
        }

        private void Btn_SaveInfo_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            RefreshInterFace();
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

        private void Pie_Chart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {

        }
    }
}
