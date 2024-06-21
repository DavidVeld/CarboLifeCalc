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
using Microsoft.Win32;
using LiveCharts.Helpers;
using LiveCharts.Definitions.Charts;
using System.Windows.Forms;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class CarboCompare : System.Windows.Controls.UserControl
    {
        public CarboProject CarboLifeProject;

        List<CarboProject> projectListToCompareTo = new List<CarboProject>();


        public CarboCompare()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            CarboLifeProject = new CarboProject();
            projectListToCompareTo = new List<CarboProject>();

            InitializeComponent();
        }

        public CarboCompare(CarboProject carboLifeProject)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            CarboLifeProject = carboLifeProject;
            projectListToCompareTo = new List<CarboProject>();

            InitializeComponent();

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

                    if (cbb_GraphType.Items.Count <= 0)
                    {
                        //cbb_GraphType.Items.Add("Materials");
                        cbb_GraphType.Items.Add("Phases");
                        cbb_GraphType.Items.Add("Life Line");

                        cbb_GraphType.Text = "Phases";
                    }


                    RefreshInterFace();


                }


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
        private void RefreshInterFace()
        {
            try
            {
                if (chx_Energy != null && chx_Sequestration != null)
                {
                    chx_Energy.Visibility = Visibility.Hidden;
                    chx_Sequestration.Visibility = Visibility.Hidden;
                }

                string GraphType = cbb_GraphType.Text;
                if (GraphType == "Materials")
                {
                    ShowMaterialsGraph();
                }
                else if (GraphType == "Phases")
                {
                    ShowPhasesGraph();
                }
                else if (GraphType == "Life Line")
                {
                    ShowLifeLineGraph();
                }

                /*
                if (CarboLifeProject != null)
                {
                    chx_Project0.Content = CarboLifeProject.Name + " (Current) " + Environment.NewLine + Math.Round(CarboLifeProject.ECTotal, 2) + " kgCO₂";


                    if (liv_Projects != null)
                    {
                        liv_Projects.ItemsSource = null;
                        liv_Projects.ItemsSource = projectListToCompareTo;
                    }
                    SeriesCollection currentProjectSeriesCollection = new SeriesCollection();
                    if (chx_Project0.IsChecked == true)
                    {
                        currentProjectSeriesCollection = GraphBuilder.BuildComparingTotalsBarGraph(CarboLifeProject, projectListToCompareTo);
                    }
                    else
                    {
                        currentProjectSeriesCollection = GraphBuilder.BuildComparingTotalsBarGraph(null, projectListToCompareTo);
                    }

                    Func<double, string> Formatter = value => value + " kgCO₂";

                    //Build the labels
                    List<string> projectlist = new List<string>();

                    if (chx_Project0.IsChecked == true)
                        projectlist.Add("Current");

                    foreach (CarboProject cp in projectListToCompareTo)
                    {
                        projectlist.Add(cp.Name);
                    }
                    //Labels = null;
                    //set the axis:
                    AxesCollection XaxisCollection = new AxesCollection();
                    Axis XAxis = new Axis { Title = "Projects", Position = AxisPosition.LeftBottom, Foreground = Brushes.Black, Labels = null };
                    XaxisCollection.Add(XAxis);

                    AxesCollection YaxisCollection = new AxesCollection();
                    Axis YAxis = new Axis { Title = "Total Embodied Carbon (tCO2)", Position = AxisPosition.LeftBottom, Foreground = Brushes.Black };
                    YaxisCollection.Add(YAxis);

                    barchart.AxisX = XaxisCollection;
                    barchart.AxisY = YaxisCollection;

                    Labels = projectlist.ToArray();

                    barchart.SeriesColors = GraphBuilder.getColours();
                    barchart.Series = currentProjectSeriesCollection;

                    DataContext = this;

                }

                */
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void ShowLifeLineGraph()
        {
            try
            {
                if (CarboLifeProject != null)
                {
                    chx_Energy.Visibility = Visibility.Visible;
                    chx_Sequestration.Visibility = Visibility.Visible;

                    SeriesCollection currentProjectSeriesCollection = new SeriesCollection();

                    bool calcSequesteration = chx_Sequestration.IsChecked.Value;
                    bool calcEnergy = chx_Energy.IsChecked.Value;
                    bool calcDemolition = true;

                    currentProjectSeriesCollection = GraphBuilder.BuildLifeLine(CarboLifeProject, projectListToCompareTo, calcSequesteration, calcEnergy, calcDemolition);

                    //always count from 0;
                    double min = GraphBuilder.min;

                    if (min > 0)
                    {
                        min = -10;
                    }
                    else
                    {
                        min = min - 10;
                    }

                    //set the axis:
                    AxesCollection XaxisCollection = new AxesCollection();
                    Axis XAxis = new Axis { Title = "Years from Construction Completion", Position = AxisPosition.LeftBottom, Foreground = Brushes.Black };
                    XaxisCollection.Add(XAxis);

                    AxesCollection YaxisCollection = new AxesCollection();
                    Axis YAxis = new Axis { Title = "Total Carbon Footprint (tCO2)", MinValue = min, Position = AxisPosition.LeftBottom, Foreground = Brushes.Black };
                    YaxisCollection.Add(YAxis);


                    barchart.AxisX = XaxisCollection;
                    barchart.AxisY = YaxisCollection;

                    barchart.Series = currentProjectSeriesCollection;


                    DataContext = this;
                }


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }


    private void ShowPhasesGraph()
        {
            try
            {

                if (CarboLifeProject != null)
                {
                    chx_Project0.Content = CarboLifeProject.Name + " (Current) " + Environment.NewLine + Math.Round(CarboLifeProject.ECTotal, 2) + " kgCO₂";


                    if (liv_Projects != null)
                    {
                        liv_Projects.ItemsSource = null;
                        liv_Projects.ItemsSource = projectListToCompareTo;
                    }
                    SeriesCollection currentProjectSeriesCollection = new SeriesCollection();
                    if (chx_Project0.IsChecked == true)
                    {
                        currentProjectSeriesCollection = GraphBuilder.BuildComparingTotalsBarGraph(CarboLifeProject, projectListToCompareTo);
                    }
                    else
                    {
                        currentProjectSeriesCollection = GraphBuilder.BuildComparingTotalsBarGraph(null, projectListToCompareTo);
                    }

                    Func<double, string> Formatter = value => value + " kgCO₂";

                    //Build the labels
                    List<string> projectlist = new List<string>();

                    if (chx_Project0.IsChecked == true)
                        projectlist.Add("Current");

                    foreach (CarboProject cp in projectListToCompareTo)
                    {
                        projectlist.Add(cp.Name);
                    }
                    //Labels = null;
                    //set the axis:
                    AxesCollection XaxisCollection = new AxesCollection();
                    Axis XAxis = new Axis { Title = "Projects", Position = AxisPosition.LeftBottom, Foreground = Brushes.Black, Labels = null };
                    XaxisCollection.Add(XAxis);

                    AxesCollection YaxisCollection = new AxesCollection();
                    Axis YAxis = new Axis { Title = "Total Carbon Footprint (tCO2)", Position = AxisPosition.LeftBottom, Foreground = Brushes.Black };
                    YaxisCollection.Add(YAxis);

                    barchart.AxisX = XaxisCollection;
                    barchart.AxisY = YaxisCollection;

                    Labels = projectlist.ToArray();

                    barchart.SeriesColors = GraphBuilder.getColours();
                    barchart.Series = currentProjectSeriesCollection;

                    DataContext = this;

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void ShowMaterialsGraph()
        {
            //To be coded
        }


        private void setProject(CarboProject clp)
        {
            if(clp != null)
            {
                projectListToCompareTo.Add(clp);
            }
            RefreshInterFace();
        }

        private CarboProject openNewProject()
        {
            CarboProject result = null;

            try
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Project File (*.clcx)|*.clcx|Carbo Life Project File (*.xml)| *.xml|All files (*.*)|*.*";

                var path = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "")
                {
                    CarboProject buffer = new CarboProject();
                    result = buffer.DeSerializeXML(openFileDialog.FileName);

                    result.Audit();
                    result.CalculateProject();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }

            return result;

        }

         private List<CarboProject> openNewProjects()
        {
            List<CarboProject> result = new List<CarboProject>();

            try
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Project File (*.clcx)|*.clcx|Carbo Life Project File (*.xml)| *.xml|All files (*.*)|*.*";
                openFileDialog.Multiselect = true;
                openFileDialog.Title = "Select Project to Compare";

                DialogResult dr = openFileDialog.ShowDialog();

                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    // Read the files
                    foreach (String file in openFileDialog.FileNames)
                    {
                        // Create a PictureBox.
                        try
                        {
                            CarboProject buffer = new CarboProject();


                            CarboProject project = buffer.DeSerializeXML(file);

                            project.Audit();
                            project.CalculateProject();

                            result.Add(project);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                         }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }

            return result;

        }

        private void btn_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (liv_Projects.SelectedItems.Count > 0)
            {
                CarboProject selectedProject = liv_Projects.SelectedItem as CarboProject;
                projectListToCompareTo.Remove(selectedProject);
            }

            RefreshInterFace();

        }

        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            //CarboProject clp = openNewProject();
           // if (clp != null)
            //    setProject(clp);

            List<CarboProject> clp_list = openNewProjects();

            if(clp_list != null)
                foreach(CarboProject proj in clp_list)
                    setProject(proj);

        }

        private void chx_Project0_Click(object sender, RoutedEventArgs e)
        {
            if (CarboLifeProject != null)
            {
                RefreshInterFace();
            }
        }

        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            DataExportUtils.ExportComaringGraphs(CarboLifeProject, projectListToCompareTo, chx_Project0.IsChecked.Value);
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

        private void directRefresh(object sender, RoutedEventArgs e)
        {
            if (CarboLifeProject != null)
            {
                RefreshInterFace();
            }
        }

        private void cbb_GraphType_DropDownClosed(object sender, EventArgs e)
        {
            if (CarboLifeProject != null)
            {
                RefreshInterFace();
            }
        }
    }
}
