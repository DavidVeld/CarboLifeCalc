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
    public partial class MaterialSequestrationPicker : Window
    {
        internal bool isAccepted;
        public CarboSeqProperties materialSeqProperties;

        public MaterialSequestrationPicker()
        {
            materialSeqProperties = new CarboSeqProperties();

            InitializeComponent();
        }

        public MaterialSequestrationPicker(CarboSeqProperties materialseqProperties)
        {
            this.materialSeqProperties = materialseqProperties;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Value.Text = materialSeqProperties.value.ToString();
            txt_Year.Text = materialSeqProperties.sequestrationPeriod.ToString();
            txt_Description.Text = materialSeqProperties.comment;
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            materialSeqProperties.value = Utils.ConvertMeToDouble(txt_Value.Text);
            materialSeqProperties.sequestrationPeriod = (int)Utils.ConvertMeToDouble(txt_Year.Text);
            materialSeqProperties.comment = txt_Description.Text;
            materialSeqProperties.propertyName = "Sequestration";

            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
