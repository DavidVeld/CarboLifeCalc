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
    /// Interaction logic for MaterialLifePicker.xaml
    /// </summary>
    public partial class MaterialLifePicker : Window
    {
        internal bool isAccepted;
        public CarboB1B5Properties materialB1B5Properties;

        public MaterialLifePicker()
        {
            InitializeComponent();
        }

        public MaterialLifePicker(string settings, double value)
        {
            this.materialB1B5Properties = new CarboB1B5Properties();

            InitializeComponent();
        }

        public MaterialLifePicker(CarboB1B5Properties materialB1B5Properties)
        {
            this.materialB1B5Properties = materialB1B5Properties;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Life.Text = materialB1B5Properties.buildingdesignlife.ToString();
            txt_ReplaceValue.Text = materialB1B5Properties.elementdesignlife.ToString();

            UpdateValue();
        }



        private async void Txt_Life_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                UpdateValue();
            }
        }

        private async void Txt_Value2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                UpdateValue();
            }
        }

        private void UpdateValue()
        {
            materialB1B5Properties.elementdesignlife = CarboLifeAPI.Utils.ConvertMeToDouble(txt_ReplaceValue.Text);
            materialB1B5Properties.buildingdesignlife = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Life.Text);

            materialB1B5Properties.calculate();

            txt_Value.Text = Math.Round(materialB1B5Properties.value, 3).ToString();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            materialB1B5Properties.value = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Value.Text);
            materialB1B5Properties.name = txt_Life.Text + " Years design life ";
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
