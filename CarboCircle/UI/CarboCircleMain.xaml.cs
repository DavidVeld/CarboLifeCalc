using Autodesk.Revit.UI;
using CarboCircle.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private static carboCircleProject project;
        public CarboCircleMain()
        {
            InitializeComponent();
        }

        public CarboCircleMain(ExternalEvent exEvent, CarboCircleHandler handler)
        {
            InitializeComponent();

            this.m_ExEvent = exEvent;
            this.m_Handler = handler;

            project = new carboCircleProject();

            // Subscribe to the DataReady event
            m_Handler.DataReady += OnDataReady;
        }

        private void OnDataReady(object sender, List<carboCircleElement> e)
        {
            liv_availableMaterialList.ItemsSource = e;
        }

        private void btn_ImportmaterialsRevit_Click(object sender, RoutedEventArgs e)
        {
            if (m_ExEvent != null)
            {
                m_Handler.SetSwitch(1);
                m_ExEvent.Raise();
                
            }
        }

        private void btn_ImportmaterialsHandler_Click(object sender, RoutedEventArgs e)
        {
           // List<carboCircleElement> collectedElements = m_Handler.getCollectedElements();
           // liv_availableMaterialList.ItemsSource = collectedElements;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //Before the form is closed, everything must be disposed properly
            m_ExEvent.Dispose();
            m_ExEvent = null;

            //clear the handler
            m_Handler._revitEvent.Dispose();
            m_Handler._revitEvent = null;
            m_Handler = null;
            FormStatusChecker.isWindowOpen = false;
            //You have to call the base class
            base.OnClosing(e);
        }


    }
}
