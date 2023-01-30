using CarboLifeAPI;
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

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialConstructionPicker.xaml
    /// </summary>
    public partial class ProjectEnergyUsage : Window
    {
        internal bool isAccepted;
        public CarboEnergyProperties projectEnergyProperties;

        public ProjectEnergyUsage()
        {
            projectEnergyProperties = new CarboEnergyProperties();

            InitializeComponent();
        }

        public ProjectEnergyUsage(CarboEnergyProperties _projectEnergyProperties)
        {
            this.projectEnergyProperties = _projectEnergyProperties;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Electricty.Text = projectEnergyProperties.electricityPerYear.ToString();
            txt_Gas.Text = projectEnergyProperties.gasPerYear.ToString();
            txt_Water.Text = projectEnergyProperties.waterPerYear.ToString();

            txt_Description.Text = projectEnergyProperties.comment;
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;

            projectEnergyProperties.electricityPerYear = Utils.ConvertMeToDouble(txt_Electricty.Text);
            projectEnergyProperties.gasPerYear = Utils.ConvertMeToDouble(txt_Gas.Text);
            projectEnergyProperties.waterPerYear = Utils.ConvertMeToDouble(txt_Water.Text);
            projectEnergyProperties.comment = txt_Description.Text;

            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }
    }
}
