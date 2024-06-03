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
    public partial class ExportPicker : Window
    {
        internal bool isAccepted;
        public string description;

        public bool results;
        public bool elements;
        public bool materials;
        public bool project;

        public ExportPicker()
        {
            description = "";
            results = true;
            elements = true;
            materials = true;
            project = true;

            InitializeComponent();
        }

        public ExportPicker(string description)
        {
            this.description = description;
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;

            results = check_Results.IsChecked.Value;
            elements = check_Elements.IsChecked.Value;
            materials = check_Materials.IsChecked.Value;
            project = check_Project.IsChecked.Value;

            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;

            results = false;
            elements = false;
            materials = false;
            project = false;

            this.Close();
        }
    }
}
