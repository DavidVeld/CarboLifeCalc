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

namespace Aardwolf
{
    public partial class OverViewViewer : Window
    {
        public CarboProject sharedProject = null;
        public OverViewViewer(CarboProject project)
        {
            InitializeComponent();
            sharedProject = project;
            Panel_Overview.CarboLifeProject = project;
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var tagProject = this.Tag;
            CarboProject cb = tagProject as CarboProject;
            if(cb != null)
            {
                Panel_Overview.CarboLifeProject = cb;
            }
        }
    }
}
