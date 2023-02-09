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
    public partial class CarboInfoBox : Window
    {
        internal bool isAccepted;
        public string description;
        public string title;

        public CarboInfoBox()
        {
            description = "";

            InitializeComponent();
        }

        public CarboInfoBox(string description, int width=400, int height=300)
        {
            this.description = description;
            this.Width = width;
            this.Height = height;
            InitializeComponent();

        }

        public CarboInfoBox(string _title, string _description, int width = 400, int height = 300)
        {
            this.title = _title;
            this.description = _description;
            this.Width = width;
            this.Height = height;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lbl_Title.Content = title;
            txt_Description.Text = description;
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;

            this.Close();
        }
    }
}
