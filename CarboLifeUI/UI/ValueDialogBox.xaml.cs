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
    public partial class ValueDialogBox : Window
    {
        internal bool isAccepted;
        public string Value;

        public ValueDialogBox()
        {
            this.Value = "";

            InitializeComponent();
        }

        public ValueDialogBox(string value="")
        {
            this.Value = value;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Value.Text = Value.ToString();
            txt_Value.SelectAll();
            txt_Value.Focus();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Value.Text != "")
            {
                isAccepted = true;
                Value = txt_Value.Text;
                this.Close();
            }
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
