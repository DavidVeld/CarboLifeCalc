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

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        public CarboProject CarboLifeProject;

        public Overview()
        {
            InitializeComponent();
        }

        public Overview(CarboProject carboLifeProject)
        {
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

                    SetupInterFace();
                }
                
                //Rebuild the materialList

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetupInterFace()
        {
            lbl_Values.Content = (CarboLifeProject.EC) / 1000 + " tCO2";
            try
            {
                IList<UIElement> uIElements = Utils_SummaryScreen.generateImage(cnv_Summary, CarboLifeProject);

                foreach (UIElement uiE in uIElements)
                {
                    //cnv_Summary.Children.Add(uiE);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
