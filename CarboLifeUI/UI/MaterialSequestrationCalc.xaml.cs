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
    public partial class MaterialSequestrationCalc : Window
    {
        internal bool isAccepted;
        public CarboDProperties materialDProperties;

        public MaterialSequestrationCalc()
        {
            materialDProperties = new CarboDProperties();

            InitializeComponent();
        }

        public MaterialSequestrationCalc(CarboDProperties materialDProperties)
        {
            this.materialDProperties = materialDProperties;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Value.Text = materialDProperties.value.ToString();
            txt_Description.Text = materialDProperties.calcResult;
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            materialDProperties.value = Utils.ConvertMeToDouble(txt_Value.Text);
            materialDProperties.calcResult = txt_Description.Text;
            materialDProperties.name = "Enhanced value";
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
