using CarboCircle.data;
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

namespace CarboCircle.UI
{
    /// <summary>
    /// Interaction logic for CarboCircleSettings.xaml
    /// </summary>
    public partial class CarboCircleSettings : Window
    {
        public carboCircleSettings settings;
        public bool isAccepted { get; internal set; }

        public CarboCircleSettings()
        {
            InitializeComponent();
        }

        public CarboCircleSettings(carboCircleProject activeProject)
        {
            this.settings = activeProject.settings.Copy();
        }

        private void btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            this.Close();
        }
    }
}
