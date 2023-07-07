using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    public partial class CarboAbout : Window
    {
        internal bool isAccepted;
        public string description;

        public CarboAbout()
        {
            description = "";

            InitializeComponent();
        }

        public CarboAbout(string description)
        {
            this.description = description;
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lbl_Version.Content = "Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            //description = txt_Description.Text;

            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;

            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://circularecology.com"));
        }

        private void Label_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.istructe.org/resources/guidance/how-to-calculate-embodied-carbon/"));

        
        }

        private void Label_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo(""));

            //
        }
    }
}
