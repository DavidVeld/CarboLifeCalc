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

        public List<carboCircleElement> availableData;
        public List<carboCircleElement> requiredData;

        private static carboCircleProject project;

        private int dataSwitch = 0;
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

            availableData = new List<carboCircleElement>();

            // Subscribe to the DataReady event
            m_Handler.DataReady += OnDataReady;
        }

        private void OnDataReady(object sender, List<carboCircleElement> e)
        {
            if (e != null)
            {
                if (dataSwitch == 0)
                {
                    availableData = e;
                    liv_availableMaterialList.ItemsSource = availableData;
                }
                else
                {
                    requiredData = e;
                    liv_requiredMaterialList.ItemsSource = requiredData;
                }
            }
        }

        private void btn_ImportmaterialsRevit_Click(object sender, RoutedEventArgs e)
        {
            if (m_ExEvent != null)
            {
                dataSwitch = 0;
                m_Handler.SetSwitch(1);
                m_ExEvent.Raise();
            }
        }

        private void btn_ImportProjectRevit_Click(object sender, RoutedEventArgs e)
        {
            if (m_ExEvent != null)
            {
                dataSwitch = 1;
                m_Handler.SetSwitch(1);
                m_ExEvent.Raise();
            }
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

        private void btn_GotoMine_Click(object sender, RoutedEventArgs e)
        {
            tab_Main.TabIndex = 1;
        }

        private void btn_ImportProjectSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbb_ImportProjectSetting.Items.Add("All Visible In View");
            cbb_ImportProjectSetting.Items.Add("All Demolished In View");
            cbb_ImportProjectSetting.Items.Add("Selected");

            cbb_MineSetting.Items.Add("All Visible In View");
            cbb_MineSetting.Items.Add("All Demolished In View");
            cbb_MineSetting.Items.Add("Selected");
            
        }
    }
}
