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
        public CarboDatabase materialData;
        public CarboGroupSettings carboGroupSettings;

        public GroupWindow(ObservableCollection<CarboElement> elementList, CarboDatabase userMaterialData, CarboGroupSettings groupSettings)
        {
            dialogOk = false;
            carboElementList = elementList;
            carboGroupList = new ObservableCollection<CarboGroup>();
            carboGroupSettings = groupSettings;

            materialData = userMaterialData;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dialogOk = false;

            chk_GroupMain.IsChecked = carboGroupSettings.groupCategory;
            chk_GroupSec.IsChecked = carboGroupSettings.groupSubCategory;
            chk_GroupType.IsChecked = carboGroupSettings.groupType ;
            chk_material.IsChecked = carboGroupSettings.groupMaterial;
            chk_GroupSuperSubStructure.IsChecked = carboGroupSettings.groupSubStructure;
            chk_GroupDemolishedItems.IsChecked = carboGroupSettings.groupDemolition;
            txt_SpecialTypes.Text = carboGroupSettings.uniqueTypeNames;

        }

        private void Btn_Group_Click(object sender, RoutedEventArgs e)
        {
            //Reset the groups
            carboGroupList = new ObservableCollection<CarboGroup>();
            //Build Groups Based on new
            carboGroupList = CarboElementImporter.GroupElementsAdvanced(
                carboElementList, 
                chk_GroupMain.IsChecked.Value, 
                chk_GroupSec.IsChecked.Value, 
                chk_GroupType.IsChecked.Value, 
                chk_material.IsChecked.Value,
                chk_GroupSuperSubStructure.IsChecked.Value,
                chk_GroupDemolishedItems.IsChecked.Value, 
                materialData,
                txt_SpecialTypes.Text
                );

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
            dialogOk = true;

            carboGroupSettings.groupCategory = chk_GroupMain.IsChecked.Value;
            carboGroupSettings.groupSubCategory = chk_GroupSec.IsChecked.Value;
            carboGroupSettings.groupType = chk_GroupType.IsChecked.Value;
            carboGroupSettings.groupMaterial = chk_material.IsChecked.Value;
            carboGroupSettings.groupSubStructure = chk_GroupSuperSubStructure.IsChecked.Value;
            carboGroupSettings.groupDemolition = chk_GroupDemolishedItems.IsChecked.Value;
            carboGroupSettings.uniqueTypeNames = txt_SpecialTypes.Text;

            carboGroupSettings.SerializeXML();

            this.Close();
        }


    }
}
