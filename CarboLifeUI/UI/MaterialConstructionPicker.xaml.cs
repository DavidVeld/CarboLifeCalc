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
    public partial class MaterialConstructionPicker : Window
    {
        internal bool isAccepted;

        public CarboA5Properties materialA5Properties;

        public MaterialConstructionPicker()
        {
            materialA5Properties = new CarboA5Properties();
            InitializeComponent();
        }

        public MaterialConstructionPicker(CarboA5Properties materialA5Properties)
        {
            this.materialA5Properties = materialA5Properties;
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Value.Text = materialA5Properties.value.ToString();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            materialA5Properties.value = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Value.Text);
            materialA5Properties.name = "Calculated";
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
