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
using System.Data;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        public CarboProject CarboLifeProject;
        string summaryTextMemory = "";
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

                //CategoryList:
                if (cbb_BuildingType.Items.Count == 0)
                {
                    cbb_BuildingType.Items.Clear();

                    IList<LetiScore> letiList = new List<LetiScore>();

                    letiList = ScorsIndicator.getLetiList();
                    IList<string> typelist = letiList.Select(x => x.BuildingType).Distinct().ToList();

                    foreach (string name in typelist)
                    {
                        cbb_BuildingType.Items.Add(name);
                    }
                }

                if (cbb_GraphType.Items.Count == 0)
                {
                    cbb_GraphType.Items.Add("Material");
                    cbb_GraphType.Items.Add("Category");

                }
                cbb_GraphType.SelectedItem = "Material";
                cbb_BuildingType.SelectedItem = CarboLifeProject.Category;
                txt_Area.Text = CarboLifeProject.Area.ToString();

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

                    SeriesCollection pieSeries = null;
                    //SeriesCollection pieLifeSeries = GraphBuilder.GetPieChartTotals(CarboLifeProject);

                    if (cbb_GraphType.SelectedValue == null)
                    {
                        DataTable currentProjectResult = CarboCalcTextUtils.getResultTable(CarboLifeProject);

                        if (currentProjectResult != null)
                            pieSeries = GraphBuilder.GetPieChart(currentProjectResult);
                    }
                    else if (cbb_GraphType.SelectedValue.ToString() == "Material")
                    {
                        DataTable currentProjectResult = CarboCalcTextUtils.getResultTable(CarboLifeProject);

                        if (currentProjectResult != null)
                            pieSeries = GraphBuilder.GetPieChart(currentProjectResult);
                    }

                    else if (cbb_GraphType.SelectedValue.ToString() == "Category")
                    {
                        DataTable currentProjectResult = CarboCalcTextUtils.getResultTable(CarboLifeProject);
                        if (currentProjectResult != null)
                            pieSeries = GraphBuilder.GetPieChart(currentProjectResult, "Category");

                    }
                    else
                    {
                        DataTable currentProjectResult = CarboCalcTextUtils.getResultTable(CarboLifeProject);

                        if (currentProjectResult != null)
                            pieSeries = GraphBuilder.GetPieChart(currentProjectResult);
                    }

                    if (pieSeries != null)
                    {
                        pie_Chart1.Series = pieSeries;
                        pie_Chart1.SeriesColors = GraphBuilder.getColours();

                    }
                    /*
                    if (pieLifeSeries != null)
                    {
                        pie_Chart2.Series = pieLifeSeries;
                        pie_Chart2.SeriesColors = GraphBuilder.getColours();

                    }
                    */
                    //Totals

                    lbl_Title.Content = CarboLifeProject.Number + " - " + CarboLifeProject.Name + " Overview: ";

                    chx_A1A3.IsChecked = CarboLifeProject.calculateA13;
                    chx_A4.IsChecked = CarboLifeProject.calculateA4;
                    chx_A5.IsChecked = CarboLifeProject.calculateA5;
                    chx_B1B7.IsChecked = CarboLifeProject.calculateB;
                    chx_C1C4.IsChecked = CarboLifeProject.calculateC;
                    chx_D.IsChecked = CarboLifeProject.calculateD;
                    chx_Seq.IsChecked = CarboLifeProject.calculateSeq;
                    chx_Added.IsChecked = CarboLifeProject.calculateAdd;

                    RefreshLetiGraph();
                    RefreshPhasePie();
                    refreshSumary();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshLetiGraph()
        {
            cnv_Leti.Children.Clear();

            if (CarboLifeProject.Area > 0)
            {
                //UpfrontOnly
                List<CarboDataPoint> resultPointsA1A5 = CarboLifeProject.getPhaseTotals(true);
                //No Sequestration or D
                List<CarboDataPoint> resultPointsA1C = CarboLifeProject.getPhaseTotals(true);


                double valueA1A5 = getDataTotals(resultPointsA1A5);
                double valueA1C = getDataTotals(resultPointsA1C);

                IEnumerable<UIElement> letiGraph = ScorsIndicator.generateImage(cnv_Leti, valueA1A5, valueA1C, CarboLifeProject.Area, cbb_BuildingType.Text);
                foreach (UIElement uielement in letiGraph)
                {
                    cnv_Leti.Children.Add(uielement);
                }
            }
        }

        private double getDataTotals(List<CarboDataPoint> resultPointsA1A5)
        {
            double result = 0;

            foreach(CarboDataPoint cdp in resultPointsA1A5)
            {
                result += cdp.Value;
            }

            return result;
        }

        private void refreshSumary()
        {
            if (CarboLifeProject != null)
            {
                cnv_Totals.Children.Clear();

                summaryTextMemory = "";

                TextBlock TotalText = new TextBlock();
                TotalText.Text = "Total Embodied Carbon: " + CarboLifeProject.getTotalEC().ToString() + " tCO₂e";
                summaryTextMemory += TotalText.Text + Environment.NewLine;
                TotalText.FontStyle = FontStyles.Normal;
                TotalText.FontWeight = FontWeights.Bold;

                TotalText.Foreground = Brushes.Black;
                TotalText.TextWrapping = TextWrapping.WrapWithOverflow;
                TotalText.VerticalAlignment = VerticalAlignment.Top;
                TotalText.FontSize = 16;

                Canvas.SetLeft(TotalText, 5);
                Canvas.SetTop(TotalText, 5);
                cnv_Totals.Children.Add(TotalText);

                List<string> textGroups = CarboLifeProject.getCalcText();
                if (textGroups.Count == 3)
                {
                    string textItems = textGroups[0];
                    string textResults = textGroups[1];
                    string textGeneral = textGroups[2];

                    int numLines = textItems.Split('\n').Length;


                    TextBlock summaryText = new TextBlock();
                    summaryText.Text = textItems;
                    //summaryTextMemory += summaryText.Text;
                    summaryText.FontStyle = FontStyles.Normal;
                    summaryText.FontWeight = FontWeights.Normal;

                    summaryText.Foreground = Brushes.Black;
                    summaryText.TextWrapping = TextWrapping.WrapWithOverflow;
                    summaryText.VerticalAlignment = VerticalAlignment.Top;
                    summaryText.FontSize = 13;

                    Canvas.SetLeft(summaryText, 5);
                    Canvas.SetTop(summaryText, 25);
                    cnv_Totals.Children.Add(summaryText);

                    TextBlock summaryValues = new TextBlock();
                    summaryValues.Text = textResults;
                    //summaryTextMemory += summaryText.Text;
                    summaryValues.FontStyle = FontStyles.Normal;
                    summaryValues.FontWeight = FontWeights.Normal;

                    summaryValues.Foreground = Brushes.Black;
                    //summaryValues.TextWrapping = TextWrapping.WrapWithOverflow;
                    summaryValues.VerticalAlignment = VerticalAlignment.Top;
                    summaryValues.FontSize = 13;
                    summaryValues.HorizontalAlignment = HorizontalAlignment.Right;
                    summaryValues.TextAlignment = TextAlignment.Right;
                    Canvas.SetLeft(summaryValues, 110);
                    Canvas.SetTop(summaryValues, 25);
                    cnv_Totals.Children.Add(summaryValues);

                    TextBlock textGeneralValues = new TextBlock();
                    textGeneralValues.Text = textGeneral;
                    //summaryTextMemory += summaryText.Text;
                    textGeneralValues.FontStyle = FontStyles.Normal;
                    textGeneralValues.FontWeight = FontWeights.Normal;

                    textGeneralValues.Foreground = Brushes.Black;
                    //summaryValues.TextWrapping = TextWrapping.WrapWithOverflow;
                    textGeneralValues.VerticalAlignment = VerticalAlignment.Top;
                    textGeneralValues.FontSize = 13;
                    textGeneralValues.HorizontalAlignment = HorizontalAlignment.Right;
                    textGeneralValues.TextAlignment = TextAlignment.Left;
                    Canvas.SetLeft(textGeneralValues, 5);
                    Canvas.SetTop(textGeneralValues, (numLines * 17) + 17);

                    cnv_Totals.Children.Add(textGeneralValues);
                }

            }

        }

        private void Cnv_Summary_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;

        }
      
        private void SaveSettings()
        {
            CarboLifeProject.CalculateProject();           
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

        private void btn_EditDescription_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CarboLifeProject != null && summaryTextMemory != "")
                {
                    Clipboard.SetText(summaryTextMemory);
                    MessageBox.Show("Text copied to clipboard", "Friendly Message", MessageBoxButton.OK);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Message", MessageBoxButton.OK);
            }
        }

        private void cbb_BuildingType_DropDownClosed(object sender, EventArgs e)
        {
            if (cbb_BuildingType.Text != "")
            {
                RefreshLetiGraph();
                CarboLifeProject.Category = cbb_BuildingType.Text;
            }
        }

        private async void txt_Area_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = txt_Area.Text;
            try
            {
                TextBox tb = (TextBox)sender;
                int startLength = tb.Text.Length;

                await Task.Delay(500);
                if (startLength == tb.Text.Length)
                {
                    double convertedText = Utils.ConvertMeToDouble(tb.Text);
                    if (convertedText != 0)
                    {
                        CarboLifeProject.Area = convertedText;
                        txt_Area.Text = convertedText.ToString();
                        RefreshLetiGraph();
                    }
                }
            }
            catch(Exception ex)
            {
                //Resume async error.
            }
        }

        private void RefreshPhasePie()
        {
            try
            {                
                if (CarboLifeProject != null && cbb_GraphType != null )
                {
                    SeriesCollection pieLifeSeries = null;

                    pieLifeSeries = GraphBuilder.GetPhasePieChartTotals(CarboLifeProject);


                    if (pieLifeSeries != null)
                    {
                        pie_Chart2.Series = pieLifeSeries;
                        pie_Chart2.SeriesColors = GraphBuilder.getColours();

                    }
                    //Totals
                    refreshSumary();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void chx_A1A3_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_A1A3 != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateA13 = chx_A1A3.IsChecked.Value;
                RefreshInterFace();
            }
        }

        private void chx_A4_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_A1A3 != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateA4 = chx_A4.IsChecked.Value;
                RefreshInterFace();
            }
        }

        private void chx_A5_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_A5 != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateA5 = chx_A5.IsChecked.Value;
                RefreshInterFace();
            }
        }

        private void chx_B_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_B1B7 != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateB = chx_B1B7.IsChecked.Value;
                RefreshInterFace();
            }
        }

        private void chx_C_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_C1C4 != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateC = chx_C1C4.IsChecked.Value;
                RefreshInterFace();
            }
        }

        private void chx_D_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_D != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateD = chx_D.IsChecked.Value;
                RefreshInterFace();
            }
        }

        private void chx_Add_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_Added != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateAdd = chx_Added.IsChecked.Value;
                RefreshInterFace();
            }
        }

        private void chx_Seq_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_Seq != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateSeq = chx_Seq.IsChecked.Value;
                RefreshInterFace();
            }
        }


        private void chx_A0_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_A0 != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateA0 = chx_A0.IsChecked.Value;
                RefreshInterFace();
            }
        }
        private void chx_B67D2_Changed(object sender, RoutedEventArgs e)
        {
            if (chx_Operational != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateA0 = chx_Operational.IsChecked.Value;
                RefreshInterFace();
            }
        }

        private void cbb_GraphType_DropDownClosed(object sender, EventArgs e)
        {
            RefreshInterFace();
        }

    }
}
