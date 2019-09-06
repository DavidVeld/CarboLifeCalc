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

                    SetupInterFace();
                }

                //Rebuild the materialList

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetupInterFace()
        {
            try
            {
                if (CarboLifeProject != null)
                {
                    //cnv_Summary.Children.Clear();
                    List<CarboDataPoint> PieceList = new List<CarboDataPoint>();
                    List<KeyValuePair<string, double>> valueList = new List<KeyValuePair<string, double>>();

                    PieceList = CarboLifeProject.getTotals("Material");


                    PieceList = PieceList.OrderByDescending(o => o.Value).ToList();

                    /*
                    IList<UIElement> uIElements = PieChartGenerator.generateImage(cnv_Summary, PieceList);

                    foreach (UIElement uiE in uIElements)
                    {
                        //cnv_Summary.Children.Add(uiE);
                    }
                    */

                    if (PieceList.Count > 0)
                    {
                        foreach (CarboDataPoint pp in PieceList)
                        {
                            KeyValuePair<string, double> pair = new KeyValuePair<string, double>(pp.Name, pp.Value);
                            valueList.Add(pair);
                        }
                    }
                    //chr_Pie.DataContext = valueList;
                    //var chartArea1 = new System.Windows.Controls.DataVisualization.Charting.Chart();
                    Style st = new Style();

                    //st.
                    //var chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
                    //LiveChart sample

                    Func<ChartPoint, string> labelPoint = chartPoint =>
    string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

                    SeriesCollection pieSeries = new SeriesCollection();

                    foreach (CarboDataPoint ppin in PieceList)
                    {
                        PieSeries newSeries = new PieSeries
                        {
                            Title = ppin.Name,
                            Values = new ChartValues<double> { Math.Round(ppin.Value,0) },
                            PushOut = 10,
                            DataLabels = true,
                            LabelPoint = labelPoint
                        };

                        pieSeries.Add(newSeries);

                    }

                    pie_Chart1.Series = pieSeries;


                    txt_ProjectName.Text = CarboLifeProject.Name;
                    txt_Number.Text = CarboLifeProject.Number;
                    txt_Category.Text = CarboLifeProject.Category;
                    txt_Desctiption.Text = CarboLifeProject.Description;

                    txt_Area.Text = CarboLifeProject.Area.ToString();
                    txt_Value.Text = CarboLifeProject.Value.ToString();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Cnv_Summary_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;

        }
      

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetupInterFace();
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
