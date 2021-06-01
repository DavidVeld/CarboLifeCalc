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

                    SeriesCollection pieMaterialSeries = GraphBuilder.GetPieChartMaterials(CarboLifeProject);

                    SeriesCollection pieLifeSeries = GraphBuilder.GetPieChartTotals(CarboLifeProject);

                    if (pieMaterialSeries != null)
                    {
                        pie_Chart1.Series = pieMaterialSeries;
                        pie_Chart1.SeriesColors = GraphBuilder.getColours();

                    }

                    if (pieLifeSeries != null)
                    {
                        pie_Chart2.Series = pieLifeSeries;
                        pie_Chart2.SeriesColors = GraphBuilder.getColours();

                    }
                    txt_ProjectName.Text = CarboLifeProject.Name;
                    txt_Number.Text = CarboLifeProject.Number;
                    txt_Category.Text = CarboLifeProject.Category;
                    txt_Desctiption.Text = CarboLifeProject.Description;

                    txt_Area.Text = CarboLifeProject.Area.ToString();
                    
                    //A5
                    txt_Value.Text = CarboLifeProject.Value.ToString();
                    txt_ValueA5Fact.Text = CarboLifeProject.A5Factor.ToString();

                    txt_SocialCost.Text = CarboLifeProject.SocialCost.ToString();

                    //C1
                    txt_DemoArea.Text = CarboLifeProject.demoArea.ToString();
                    txt_DemoC1Fact.Text = CarboLifeProject.C1Factor.ToString();

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


                TextBlock summaryText = new TextBlock();
                summaryText.Text = CarboLifeProject.getSummaryText(true, true, true, true);
                summaryTextMemory += summaryText.Text;
                summaryText.FontStyle = FontStyles.Normal;
                summaryText.FontWeight = FontWeights.Normal;

                summaryText.Foreground = Brushes.Black;
                summaryText.TextWrapping = TextWrapping.WrapWithOverflow;
                summaryText.VerticalAlignment = VerticalAlignment.Top;
                summaryText.FontSize = 13;

                Canvas.SetLeft(summaryText, 5);
                Canvas.SetTop(summaryText, 25);
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

            CarboLifeProject.SocialCost = CarboLifeAPI.Utils.ConvertMeToDouble(txt_SocialCost.Text);
            CarboLifeProject.Area = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Area.Text);
            CarboLifeProject.SocialCost = CarboLifeAPI.Utils.ConvertMeToDouble(txt_SocialCost.Text);
            //A5
            CarboLifeProject.Value = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Value.Text);
            CarboLifeProject.A5Factor = CarboLifeAPI.Utils.ConvertMeToDouble(txt_ValueA5Fact.Text);

            //C1
            CarboLifeProject.demoArea = CarboLifeAPI.Utils.ConvertMeToDouble(txt_DemoArea.Text);
            CarboLifeProject.C1Factor = CarboLifeAPI.Utils.ConvertMeToDouble(txt_DemoC1Fact.Text);

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

        private void btn_EditDescription_Click(object sender, RoutedEventArgs e)
        {
            DescriptionEditor editor = new DescriptionEditor(txt_Desctiption.Text);
            editor.ShowDialog();
            if (editor.isAccepted == true)
            {
                txt_Desctiption.Text = editor.description;
            }
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
    }
}
