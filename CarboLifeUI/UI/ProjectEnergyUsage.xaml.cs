using CarboLifeAPI;
using CarboLifeAPI.Data;
using LiveCharts.Wpf;
using LiveCharts;
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

using TextBox = System.Windows.Controls.TextBox;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialConstructionPicker.xaml
    /// </summary>
    public partial class ProjectEnergyUsage : Window
    {
        internal bool isAccepted;
        private int designPeriod;

        public CarboEnergyProperties projectEnergyProperties;

        public ProjectEnergyUsage(CarboEnergyProperties _projectEnergyProperties, int _designPeriod)
        {
            this.projectEnergyProperties = _projectEnergyProperties;
            this.designPeriod = _designPeriod;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadInterface();
        }

        private void loadInterface()
        {
            if (projectEnergyProperties != null)
            {
//Electricity
                txt_Electricty.Text = projectEnergyProperties.ElectricityUsedPerYear.ToString();
                txt_ElectrictyCost.Text = projectEnergyProperties.CO2CostPerkWh.ToString();

//Water
                txt_Water.Text = projectEnergyProperties.WaterUsedPerYear.ToString();
                txt_WaterCost.Text = projectEnergyProperties.CO2CostPerm3.ToString();

//Generated
                txt_EnergyGeneration.Text = projectEnergyProperties.ElectricitygeneratedPerYear.ToString();

//Decarbon & other
                txt_decarbofact.Text = projectEnergyProperties.decabornisationFactor.ToString();
//Comments
                txt_Description.Text = projectEnergyProperties.comment;
            }
         } 

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            //Electricity

            projectEnergyProperties.ElectricityUsedPerYear = Utils.ConvertMeToDouble(txt_Electricty.Text);
            projectEnergyProperties.CO2CostPerkWh = Utils.ConvertMeToDouble(txt_ElectrictyCost.Text);
            //Water

            projectEnergyProperties.WaterUsedPerYear = Utils.ConvertMeToDouble(txt_Water.Text);
            projectEnergyProperties.CO2CostPerm3 = Utils.ConvertMeToDouble(txt_WaterCost.Text);

            //Generated
            projectEnergyProperties.ElectricitygeneratedPerYear = Utils.ConvertMeToDouble(txt_EnergyGeneration.Text);

            //decarbon & other
            projectEnergyProperties.decabornisationFactor = Utils.ConvertMeToDouble(txt_decarbofact.Text);

            projectEnergyProperties.comment = txt_Description.Text;


            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void btn_EnergyAdvice_Click(object sender, RoutedEventArgs e)
        {
            ShowInfoBox("B6 - Operational Energy", "For operational energy a LETI advised limits are: \n Residential: 35kWh/m²/year \n Offices: 55kWh/m²/year \n Schools: 65kWh/m²/year \n\n For the emissions per kWh look at the local values a UK average is approx. 0.193 kgCo2/kWh");
        }


        private void btn_WaterAdvice_Click(object sender, RoutedEventArgs e)
        {
            ShowInfoBox("B7 - Water Usage", "Water usage depends on the type of building and it's usage. For Residential: Assuming 65m³ of water per person per year which is between 1.5-2.5 m³ water /m²/year. For the emissions per m³ of water look at the local values. The UK average is approx. 0.0015 kgCo2/m³");

        }

        private void btn_GeneratedAdvice_Click(object sender, RoutedEventArgs e)
        {
            ShowInfoBox("D2 - Energy Generation", "This value specifies the amount of energy a building will produce per year. As an example a solar panel produces approx. 0.150 Watt/m² per hour, or around 750-1000 kWh / year. \n Use: Area x 750 = kWh/year \n The exact values are depending position, type and age of solar panels.");
        }

        private void btn_DecarbonisationAdvice_Click(object sender, RoutedEventArgs e)
        {
            ShowInfoBox("Decarbonisation", "Generally assumed is that the energy grid will 'decarbonise', a conservative value would be around 1-3%, more optimistic would be 5-10%, refer to local national data to establish this factor");
        }

        private void ShowInfoBox(string title, string content)
        {
            CarboInfoBox cib = new CarboInfoBox(title, content);
            cib.ShowDialog();
        }
        private async void txt_Electricty_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                projectEnergyProperties.ElectricityUsedPerYear = Utils.ConvertMeToDouble(tb.Text);
            }
            refreshData();

        }

        private async void txt_ElectrictyCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                projectEnergyProperties.CO2CostPerkWh = Utils.ConvertMeToDouble(tb.Text);
            }
            refreshData();

        }

        private async void txt_Water_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                projectEnergyProperties.WaterUsedPerYear = Utils.ConvertMeToDouble(tb.Text);
            }
            refreshData();

        }

        private async void txt_WaterCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                projectEnergyProperties.CO2CostPerm3 = Utils.ConvertMeToDouble(tb.Text);
            }
            refreshData();

        }

        private async void txt_EnergyGeneration_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                projectEnergyProperties.ElectricitygeneratedPerYear = Utils.ConvertMeToDouble(tb.Text);
            }
            refreshData();

        }

        private async void txt_decarbofact_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                projectEnergyProperties.decabornisationFactor = Utils.ConvertMeToDouble(tb.Text);
            }
            refreshData();

        }

        private async void txt_Description_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                projectEnergyProperties.comment = tb.Text;
            }
            refreshData();

        }

        private void refreshData()
        {
            projectEnergyProperties.calculate(designPeriod);

            SeriesCollection energyLines = new SeriesCollection();
            LineSeries lineSeries = new LineSeries();

            ChartValues<double> Values = new ChartValues<double>();

            for(int i = 0; i < designPeriod; i++)
            {
                double value = projectEnergyProperties.getTotalValue(i);
                Values.Add(Math.Round(( value / 1000), 1));
            }

            lineSeries.Values = Values;
            lineSeries.Title = "Energy";
            lineSeries.DataLabels = false;
            lineSeries.Foreground = Brushes.Black;
            lineSeries.PointGeometrySize = 5;
            lineSeries.Width = 1;

            //check min max
            /*
            if (Values.Max() > max)
                max = Values.Max();
            if (Values.Min() < min)
                min = Values.Min();
            */

            energyLines.Add(lineSeries);


            //set the axis:
            /*
            AxesCollection XaxisCollection = new AxesCollection();
            Axis XAxis = new Axis { Title = "Years From Construction Completion", Position = AxisPosition.LeftBottom, Foreground = Brushes.Black };
            XaxisCollection.Add(XAxis);

            AxesCollection YaxisCollection = new AxesCollection();
            Axis YAxis = new Axis { Title = "Total embodied Carbon (tCO2)", MinValue = min, Position = AxisPosition.LeftBottom, Foreground = Brushes.Black };
            YaxisCollection.Add(YAxis);
            

            barchart.AxisX = XaxisCollection;
            barchart.AxisY = YaxisCollection;
            */

            chrt_Preview.Series = energyLines;

        }
    }
}
