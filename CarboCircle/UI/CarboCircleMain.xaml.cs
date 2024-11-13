using Autodesk.Revit.UI;
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
    /// Interaction logic for CarboCircleMain.xaml
    /// </summary>
    public partial class CarboCircleMain : Window
    {
        //Used for Revit handlers
        private CarboCircleHandler m_Handler;
        private ExternalEvent m_ExEvent;
        private List<int> visibleElements;
        public CarboCircleMain()
        {
            InitializeComponent();
        }

        private void btn_ImportmaterialsRevit_Click(object sender, RoutedEventArgs e)
        {
            m_Handler.GrabData(1);
            m_ExEvent.Raise();
        }


        public CarboCircleMain(ExternalEvent exEvent, CarboCircleHandler handler)
        {
            //settings
            //carboSettings = new CarboSettings();
            //carboSettings.Load();
            //settings

            InitializeComponent();

            this.m_ExEvent = exEvent;
            this.m_Handler = handler;


        }
    }
}
