using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
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
        private CarboGroup carboGroup;
        private List<CarboElement> elementList;

        private ObservableCollection<CarboElement> passedElementList;
        private ObservableCollection<CarboElement> filteredElementList;

        public bool dialogOk;

        public CarboGroup GrpPassed;
        public CarboGroup GrpFiltered;
        public GroupWindow(CarboGroup groupToSplit)
        {
            dialogOk = false;
            carboGroup = groupToSplit;
            elementList = carboGroup.AllElements;
            passedElementList = new ObservableCollection<CarboElement>();
            filteredElementList = new ObservableCollection<CarboElement>();


            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dialogOk = false;

            if (elementList.Count > 0)
            {
                foreach (var prop in elementList[0].GetType().GetProperties())
                {
                    if(prop.PropertyType == typeof(string))
                    cbb_MainGroup.Items.Add(prop.Name);
                }
            
            cbb_MainGroup.Text = "Name";
        }
            //load elements
            //passedElementList = getDataTablefromList(elementList);

            if (passedElementList != null)
            {
                dgv_Preview.ItemsSource = elementList;
            }
        }

        
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            dialogOk = false;
            this.Close();
        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            dialogOk = true;

            GrpPassed = carboGroup.Clone() as CarboGroup;
            GrpFiltered = carboGroup.Clone() as CarboGroup;

            GrpPassed.AllElements.Clear();
            GrpFiltered.AllElements.Clear();

            foreach (CarboElement ce in passedElementList)
            {
                GrpPassed.AllElements.Add(ce);
            }

            foreach (CarboElement ce in filteredElementList)
            {
                GrpFiltered.AllElements.Add(ce);
            }

            GrpPassed.CalculateTotals();
            GrpFiltered.CalculateTotals();

            this.Close();
        }

        private void btn_Split_Click(object sender, RoutedEventArgs e)
        {
            string queryGroup = cbb_MainGroup.Text;
            string query = txt_SplitValue.Text;

            if (queryGroup != "" && query != "")
            {
                GroupQueryUtils searchContainer = new GroupQueryUtils(elementList, queryGroup, query);
                searchContainer.TrySearch();

                if(searchContainer.Result == true)
                {
                    passedElementList = searchContainer.PassedElementList;
                    filteredElementList = searchContainer.FilteredElementList;

                    dgv_Preview.ItemsSource = passedElementList;
                    dgv_Preview2.ItemsSource = filteredElementList;
                }

            }
        }



    }
}
