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
using System.IO;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.DB;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        public CarboProject CarboLifeProject;
        string summaryTextMemory = "";
        public SeriesCollection SeriesCollection { get; set; }
        public string[] levelLabels { get; set; }
        public Func<double, string> Formatter { get; set; }

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
                    cbb_GraphType.Items.Add("Category Merged");
                    cbb_GraphType.Items.Add("Super - SubStructure");
                    cbb_GraphType.Items.Add("By Level, Material");
                    cbb_GraphType.Items.Add("By Level, Category");
                    cbb_GraphType.Items.Add("By Level, Totals");


                }
                cbb_GraphType.SelectedItem = "Material";
                cbb_BuildingType.SelectedItem = CarboLifeProject.Category;
                txt_Area.Text = CarboLifeProject.Area.ToString();
                txt_AreaNew.Text = CarboLifeProject.AreaNew.ToString();
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
                    SeriesCollection levelSeries = null;

                    //SeriesCollection pieLifeSeries = GraphBuilder.GetPieChartTotals(CarboLifeProject);

                    chart_Level.Visibility = System.Windows.Visibility.Hidden;
                    pie_Chart1.Visibility = System.Windows.Visibility.Visible;

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
                    else if (cbb_GraphType.SelectedValue.ToString() == "Category Merged")
                    {
                        DataTable currentProjectResult = null;
                        List<CarboElement> projectElements = CarboLifeProject.getElementsFromGroups().ToList();
                        if (projectElements != null)
                            pieSeries = GraphBuilder.GetPieChart(currentProjectResult, "Category Merged", projectElements);

                    }
                    else if (cbb_GraphType.SelectedValue.ToString() == "Super - SubStructure")
                    {
                        DataTable currentProjectResult = null;
                        List<CarboElement> projectElements = CarboLifeProject.getElementsFromGroups().ToList();
                        if (projectElements != null)
                            pieSeries = GraphBuilder.GetPieChart(currentProjectResult, "Super - SubStructure", projectElements);

                    }
                    else if (cbb_GraphType.SelectedValue.ToString() == "By Level, Material" ||
                        cbb_GraphType.SelectedValue.ToString() == "By Level, Category" ||
                        cbb_GraphType.SelectedValue.ToString() == "By Level, Totals")
                    {
                        string graphType = "";

                        chart_Level.Visibility = System.Windows.Visibility.Visible;
                        pie_Chart1.Visibility = System.Windows.Visibility.Hidden;

                        if (cbb_GraphType.SelectedValue.ToString() == "By Level, Material")
                            graphType = "Material";
                        else if(cbb_GraphType.SelectedValue.ToString() == "By Level, Category")
                            graphType = "Category";
                        else
                            graphType = "Totals";

                        //JsCarboProject convertedProject = JsonExportUtils.converToJsProject(CarboLifeProject);
                        List<string> labels = new List<string>();

                        List<string> levelList = CarboLifeProject.getSortedLevelList();

                        //DataTable currentProjectResult = CarboCalcTextUtils.getByElementTable(CarboLifeProject);

                        List<CarboElement> elementData = CarboLifeProject.getElementsFromGroups().ToList();

                        if (elementData.Count > 0)
                            levelSeries = GraphBuilder.getLevelChartMaterial(elementData, graphType, out labels);

                        //refresh:
                        chart_Level.Series = null;
                        levelLabels = null; ;
                        DataContext = this;

                        levelLabels = labels.ToArray();
                        DataContext = this;

                        if (levelSeries != null)
                        {
                            chart_Level.Series = levelSeries;
                            chart_Level.SeriesColors = GraphBuilder.getColours();

                            levelLabels = labels.ToArray();
                            Formatter = x => x + "tCO2";
                            DataContext = this;
                        }

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
                    chx_Operational.IsChecked = CarboLifeProject.calculateB67;


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

            if (CarboLifeProject.Area > 0 || CarboLifeProject.AreaNew > 0)
            {
                double areaTotal = CarboLifeProject.Area;
                double areaNew = CarboLifeProject.AreaNew;

                double area = areaTotal;

                if(rad_AreaNew.IsChecked == true && areaNew > 0)
                    area = areaNew;

                //UpfrontOnly
                double resultPointsA1A5 = CarboLifeProject.getUpfrontTotals();
                //No Sequestration or D
                double resultPointsA1C = CarboLifeProject.getEmbodiedTotals();


                IEnumerable<UIElement> letiGraph = ScorsIndicator.generateImage(cnv_Leti, resultPointsA1A5, resultPointsA1C, area, cbb_BuildingType.Text);
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
                TotalText.Text = "Calculated Carbon Footprint: " + CarboLifeProject.getTotalEC().ToString() + " tCO₂e";
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
                if (textGroups.Count == 2)
                {
                    string textItems = textGroups[0];
                    string textResults = textGroups[1];

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
                    Canvas.SetLeft(summaryValues, 125);
                    Canvas.SetTop(summaryValues, 25);
                    cnv_Totals.Children.Add(summaryValues);

                    //Add the general text

                    string textGeneral = CarboLifeProject.getGeneralText();

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
            this.Visibility = System.Windows.Visibility.Hidden;
            this.Visibility = System.Windows.Visibility.Visible;

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
                string result = "";

                result += "Total Embodied Carbon: " + CarboLifeProject.getTotalEC().ToString() + " tCO₂e" + Environment.NewLine;

                //List<string> textGroups = CarboLifeProject.getCalcText();
                result = ReportBuilder.getFlattenedCalText(CarboLifeProject);
                result += Environment.NewLine;
                result += CarboLifeProject.getGeneralText();


                if (CarboLifeProject != null && result != "")
                {
                    Clipboard.SetText(result);
                    MessageBox.Show("Text copied to clipboard: " + Environment.NewLine + Environment.NewLine + result, "Friendly Message", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("Nothing to copy", "Friendly Message", MessageBoxButton.OK);

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
        private async void txt_AreaNew_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = txt_AreaNew.Text;
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
                        CarboLifeProject.AreaNew = convertedText;
                        txt_AreaNew.Text = convertedText.ToString();
                        RefreshLetiGraph();
                    }
                }
            }
            catch (Exception ex)
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
                    //refreshSumary();
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
                CarboLifeProject.calculateB67 = chx_Operational.IsChecked.Value;
                RefreshInterFace();
            }
        }

        private void cbb_GraphType_DropDownClosed(object sender, EventArgs e)
        {
            RefreshInterFace();
        }

        private void chx_SubStructure_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chx_Operational != null && CarboLifeProject != null)
            {
                CarboLifeProject.calculateSubStructure = chx_SubStructure.IsChecked.Value;

                RefreshInterFace();
            }
        }

        private void rad_AreaNew_Checked(object sender, RoutedEventArgs e)
        {
            if (rad_AreaNew != null && rad_AreaTotal != null && CarboLifeProject != null)
            {
                RefreshLetiGraph();
            }
        }
    }
}
