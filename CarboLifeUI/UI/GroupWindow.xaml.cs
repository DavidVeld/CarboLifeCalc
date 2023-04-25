using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GroupWindow : Window
    {
        public bool dialogOk;

        public ObservableCollection<CarboElement> carboElementList;
        public ObservableCollection<CarboGroup> carboGroupList;
        public ObservableCollection<CarboGroup> carboGroupNewList;

        public CarboDatabase materialData;

        public GroupWindow(CarboGroup groupSettings)
        {
            dialogOk = false;
            carboGroupList = new ObservableCollection<CarboGroup>();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dialogOk = false;
            cbb_MainGroup.Items.Add("Category");
            cbb_MainGroup.Items.Add("Type Name");
            cbb_MainGroup.Items.Add(" Name");

        }

        private void Btn_Group_Click(object sender, RoutedEventArgs e)
        {
            string typeNames = "";

            //CarboElementImporter.G

            if(carboGroupList != null)
            {
                dgv_Preview.ItemsSource = null;
                dgv_Preview.ItemsSource = carboGroupList;
            }
        }
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            dialogOk = false;
            this.Close();
        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            if (carboGroupList.Count > 0)
            {
                dialogOk = true;
            }
            else
                dialogOk = false;

            this.Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
