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
    public partial class B4EmissionPicker : Window
    {
        internal bool isAccepted;
        public CarboB1B7Properties materialB1B5Properties;
        public int desinglife;

        public B4EmissionPicker(CarboB1B7Properties materialB1B5Properties)
        {
            this.materialB1B5Properties = materialB1B5Properties;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_ComponentLifespan.Text = materialB1B5Properties.elementdesignlife.ToString();
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
            materialB1B5Properties.elementdesignlife = CarboLifeAPI.Utils.ConvertMeToDouble(txt_ComponentLifespan.Text);

            materialB1B5Properties.calculate(desinglife);

            txt_Value.Text = Math.Round(materialB1B5Properties.totalValue, 3).ToString();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            materialB1B5Properties.totalValue = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Value.Text);
            materialB1B5Properties.name = txt_ComponentLifespan.Text + " Years design life ";
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void chx_EndOfLife_Changed(object sender, RoutedEventArgs e)
        {
            materialB1B5Properties.designLifeToEnd = chx_EndOfLife.IsChecked.Value;
        }
    }
}
