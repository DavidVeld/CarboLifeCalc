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
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class DataViewer : UserControl
    {
        public CarboProject CarboLifeProject;

        public DataViewer()
        {
            InitializeComponent();

        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            { 
                
                DependencyObject parent = VisualTreeHelper.GetParent(this);
                Window parentWindow = Window.GetWindow(parent);
                CarboLifeMainWindow mainViewer = parentWindow as CarboLifeMainWindow;

                if (mainViewer != null)
                    CarboLifeProject = mainViewer.getCarbonLifeProject();
                    
                if (CarboLifeProject != null)
                {
                    //A project Is loaded, Proceed to next

                    SetupInterFace();
                }


                //Rebuild the materialList




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetupInterFace()
        {
            try
            {
                dgv_Overview.ItemsSource = CarboLifeProject.getGroupList;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Dgv_Overview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                dgv_Elements.ItemsSource = carboGroup.AllElements;
            }
        }

        private void Btn_Calculate_Click(object sender, RoutedEventArgs e)
        {
            CarboLifeProject.CalculateProject();
            refreshData();
        }


        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                CarboLifeProject.DeleteGroup(carboGroup);
            }
            CarboLifeProject.CalculateProject();

            refreshData();
        }

        public void refreshData()
        {
            dgv_Overview.ItemsSource = null;
            dgv_Overview.ItemsSource = CarboLifeProject.getGroupList;
        }

        private void Btn_Material_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                MaterialEditor materialEditor = new MaterialEditor(carboGroup.Material, CarboLifeProject.CarboDatabase);
                materialEditor.ShowDialog();

                if(materialEditor.acceptNew == true)
                {
                    CarboLifeProject.CarboDatabase = materialEditor.returnedDatabase;

                    CarboLifeProject.UpdateMaterial(carboGroup, materialEditor.selectedMaterial);
                    /*
                    carboGroup.Material = materialEditor.selectedMaterial;
                    carboGroup.MaterialName = materialEditor.selectedMaterial.Name;
                    */
                }

            }
            CarboLifeProject.CalculateProject();
            refreshData();

        }

        private void Mnu_reGroupData_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Regrouping the set can remove any groups that you have created thus far, do you want to proceed?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Stop);
            if (result == MessageBoxResult.Yes)
            {
                CarboGroupSettings groupSettings = new CarboGroupSettings();
                groupSettings = groupSettings.DeSerializeXML();

                GroupWindow importGroupWindow = new GroupWindow(CarboLifeProject.getAllElements, CarboLifeProject.CarboDatabase, groupSettings);
                importGroupWindow.ShowDialog();
                if (importGroupWindow.dialogOk == true)
                {
                    //Save non-element items;

                    ObservableCollection<CarboGroup> userGroups = new ObservableCollection<CarboGroup>();
                    userGroups = CarboLifeProject.GetGroupsWithoutElements();

                    CarboLifeProject.SetGroups(importGroupWindow.carboGroupList);
                    CarboLifeProject.AddGroups(userGroups);
                    refreshData();
                }
            }
        }

        private void Mnu_NewGroup_Click(object sender, RoutedEventArgs e)
        {
            CarboLifeProject.CreateNewGroup();
        }

        private void Mnu_DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                CarboLifeProject.DeleteGroup(carboGroup);
            }
        }

        private void Dgv_Overview_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                CarboLifeProject.UpdateGroup(carboGroup);
            }
        }

        private void Mnu_DuplicateGroup_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                CarboLifeProject.DuplicateGroup(carboGroup);
            }
        }

        private void Mnu_PurgeElements_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                if (carboGroup.AllElements.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Do you really want to remove all elements from this collection? This action is can NOT be undone", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                    if (result == MessageBoxResult.Yes)
                    {
                        CarboLifeProject.PurgeElements(carboGroup);
                    }
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("This collection contains no elements", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Stop);
                }
            }
        }

        private void Mnu_Reinforce_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                ReinforcementWindow reinforementWindow = new ReinforcementWindow(CarboLifeProject.CarboDatabase, carboGroup);
                reinforementWindow.ShowDialog();

                if (reinforementWindow.isAccepted == true)
                {
                    CarboLifeProject.AddGroup(reinforementWindow.reinforcementGroup);
                }
            }
        }

        private void Mnu_Metaldeck_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                if (carboGroup.AllElements.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("To apply a profile decking to this item you need to trucate all elements", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        CarboLifeProject.PurgeElements(carboGroup);
                    }
                }
                else
                {
                    ProfileWindow ProfileWindowWindow = new ProfileWindow(CarboLifeProject.CarboDatabase, carboGroup);
                    ProfileWindowWindow.ShowDialog();

                    if (ProfileWindowWindow.isAccepted == true)
                    {
                        CarboLifeProject.AddGroup(ProfileWindowWindow.profileGroup);
                    }
                }
            }
        }

        private void Mnu_sortMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (CarboLifeProject.getGroupList != null)
            {
                ListCollectionView collectionView = new ListCollectionView(CarboLifeProject.getGroupList);
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("MaterialName"));
                dgv_Overview.ItemsSource = null;
                dgv_Overview.ItemsSource = collectionView;
            }
        }

        private void Mnu_sortCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CarboLifeProject.getGroupList != null)
            {
                ListCollectionView collectionView = new ListCollectionView(CarboLifeProject.getGroupList);
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                dgv_Overview.ItemsSource = null;
                dgv_Overview.ItemsSource = collectionView;
            }
        }

        private void Mnu_noSort_Click(object sender, RoutedEventArgs e)
        {

            dgv_Overview.ItemsSource = null;
            dgv_Overview.ItemsSource = CarboLifeProject.getGroupList;
        }
    }
}
