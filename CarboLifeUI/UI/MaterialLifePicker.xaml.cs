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
        public double Value;
        public string Settings;

        public MaterialLifePicker()
        {
            InitializeComponent();
        }

        public MaterialLifePicker(string settings, double value)
        {
            this.Settings = settings;
            this.Value = value;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Value.Text = Value.ToString();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            Value = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Value.Text);
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Txt_Life_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            double yearspan = CarboLifeAPI.Utils.ConvertMeToDouble(txt_ReplaceValue.Text);
            double year = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Life.Text);

            double value = year / yearspan;
            txt_Value.Text = Math.Round(value, 2).ToString();

        }

        private void Txt_Value2_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValue();
        }
    }
}
