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

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class CarboCompare : UserControl
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
            InitializeComponent();

            CarboLifeProject = carboLifeProject;
            projectListToCompareTo = new List<CarboProject>();

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

                    if(cbb_GraphType.Items.Count <= 0)
                    {
                        cbb_GraphType.Items.Add("Materials");
                        cbb_GraphType.Items.Add("Totals");
                        cbb_GraphType.Text = "Totals";
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
        private void RefreshInterFace()
        {
            try
            {
                if (CarboLifeProject != null)
                {
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

                    barchart.Series = currentProjectSeriesCollection;

                    Func<double, string> Formatter = value => value + " kgCO2/kg";
                    DataContext = this;

                    Labels = new[] { "Current", "Selected" };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void cbb_GraphType_DropDownClosed(object sender, EventArgs e)
        {
            RefreshInterFace();
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
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Project File (*.xml)|*.xml|All files (*.*)|*.*";

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
                MessageBox.Show(ex.Message);
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
            CarboProject clp = openNewProject();
            if (clp != null)
                setProject(clp);
        }

        private void chx_Project0_Checked(object sender, RoutedEventArgs e)
        {
            RefreshInterFace();
        }
    }
}
