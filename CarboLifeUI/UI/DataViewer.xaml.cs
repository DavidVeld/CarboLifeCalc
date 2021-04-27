using CarboLifeAPI;
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
                SortData();

                //Load images
                string _path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string imgpath = System.IO.Path.GetDirectoryName(_path);

                //Attempt1: edit button
                //BitmapImage bimg_edit = new BitmapImage(new Uri(imgpath + @"\img\editicon.png"));
                //ImageSource rcs = bimg_edit;
                //img_editButton.Source = rcs;


                BitmapImage bimg_ref = new BitmapImage(new Uri(imgpath + @"\img\refreshicon.png"));
                ImageSource src_ref = bimg_ref;
                img_refreshbutton.Source = src_ref;


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
            Calculate();
        }

        private void Calculate()
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

            //GetTotals
            double totals = 0;

            if (CarboLifeProject.getGroupList.Count > 0)
            {
                totals = CarboLifeProject.getTotalsGroup().EC;
            }
            else
            {
                totals = 0;
            }

            lbl_Total.Content = "TOTAL: " + Math.Round(totals,2) + " tCo2";


            SortData();
        }

        private void Btn_Material_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (dgv_Overview.SelectedItems.Count > 0)
            {
                try
                {
                    var selectedItems = dgv_Overview.SelectedItems;
                    IList<CarboGroup> selectedGroups = new List<CarboGroup>();

                    // ... Add all Names to a List.
                    foreach (var item in selectedItems)
                    {
                        CarboGroup cg = item as CarboGroup;
                        selectedGroups.Add(cg);
                    }

                    if (selectedGroups.Count > 0)
                    {
                        CarboGroup carboGroup = selectedGroups[0];

                        MaterialEditor materialEditor = new MaterialEditor(carboGroup.Material.Name, CarboLifeProject.CarboDatabase);
                        materialEditor.ShowDialog();

                        if (materialEditor.acceptNew == true)
                        {
                            CarboLifeProject.CarboDatabase = materialEditor.returnedDatabase;

                            foreach (CarboGroup cg in selectedGroups)
                            {
                                CarboLifeProject.UpdateMaterial(cg, materialEditor.selectedMaterial);
                            }
                            CarboLifeProject.UpdateAllMaterials();
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            */

            if (dgv_Overview.SelectedItems.Count > 0)
            {
                try
                {
                    //Select all the groups
                    var selectedItems = dgv_Overview.SelectedItems;
                    IList<CarboGroup> selectedGroups = new List<CarboGroup>();

                    // ... Add all Names to a List.
                    foreach (var item in selectedItems)
                    {
                        CarboGroup cg = item as CarboGroup;
                        selectedGroups.Add(cg);
                    }

                    if (selectedGroups.Count > 0)
                    {
                        CarboGroup carboGroup = selectedGroups[0];

                        MaterialSelector materialEditor = new MaterialSelector(carboGroup.Material.Name, CarboLifeProject.CarboDatabase);
                        materialEditor.ShowDialog();
                        //If okay change the materials and re-calculate project
                        if (materialEditor.isAccepted == true)
                        {
                            foreach (CarboGroup cg in selectedGroups)
                            {
                                CarboLifeProject.UpdateMaterial(cg, materialEditor.selectedMaterial);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
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
                CarboSettings settings = new CarboSettings().Load();

                //CarboGroupSettings groupSettings = new CarboGroupSettings();
                //groupSettings = groupSettings.DeSerializeXML();

                GroupWindow importGroupWindow = new GroupWindow(CarboLifeProject.getAllElements, CarboLifeProject.CarboDatabase, settings);
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
            SortData();
        }

        private void Mnu_DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                CarboLifeProject.DeleteGroup(carboGroup);
            }
            SortData();
        }

        private void Dgv_Overview_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (dgv_Overview != null)
            {
                CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;

                if (carboGroup != null)
                {
                    TextBox t = e.EditingElement as TextBox;
                    DataGridColumn dgc = e.Column;

                    if (t != null)
                    {
                        //Corrections:
                        if (dgc.Header.ToString().StartsWith("Correction"))
                        {
                            string textExpression = t.Text;
                            if (Utils.isValidExpression(textExpression) == true)
                            {
                                carboGroup.Correction = textExpression;
                                carboGroup.CalculateTotals();

                                CarboLifeProject.UpdateGroup(carboGroup);

                            }
                            else
                            {
                                carboGroup.Correction = "";
                                carboGroup.CalculateTotals();

                                CarboLifeProject.UpdateGroup(carboGroup);
                            }
                        }
                        if (dgc.Header.ToString().StartsWith("Volume"))
                        {
                            if (carboGroup.AllElements.Count > 0)
                            {
                                MessageBox.Show("The volume of this group is calculated using the element volumes extracted from the 3D model," + Environment.NewLine + " you need to purge the elements before overriding the volume");
                                carboGroup.CalculateTotals();
                                CarboLifeProject.UpdateGroup(carboGroup);

                                //System.Threading.Thread.Sleep(500);
                                //Calculate();
                            }
                            else
                            {
                                double volumeEdit = Utils.ConvertMeToDouble(t.Text);
                                if (volumeEdit != 0)
                                {
                                    carboGroup.Volume = volumeEdit;

                                    carboGroup.CalculateTotals();
                                    CarboLifeProject.UpdateGroup(carboGroup);
                                    //carboGroup.CalculateTotals();
                                }
                            }
                        }
                        //Waste
                        //Corrections:
                        if (dgc.Header.ToString().StartsWith("Waste"))
                        {
                            double wastevalue = Utils.ConvertMeToDouble(t.Text);
                            if (wastevalue != 0)
                            {
                                carboGroup.Waste = wastevalue;

                                carboGroup.CalculateTotals();
                                CarboLifeProject.UpdateGroup(carboGroup);
                                //carboGroup.CalculateTotals();
                            }
                        }
                        //Additional:
                        if (dgc.Header.ToString().StartsWith("Additional"))
                        {
                            double additional = Utils.ConvertMeToDouble(t.Text);
                            if (additional != 0)
                            {
                                carboGroup.Additional = additional;

                                carboGroup.CalculateTotals();
                                CarboLifeProject.UpdateGroup(carboGroup);
                                //carboGroup.CalculateTotals();
                            }
                        }

                        //B4:
                        if (dgc.Header.ToString().StartsWith("Group"))
                        {
                            double b4 = Utils.ConvertMeToDouble(t.Text);
                            if (b4 != 0)
                            {
                                carboGroup.B4Factor = b4;

                                carboGroup.CalculateTotals();
                                CarboLifeProject.UpdateGroup(carboGroup);
                                //carboGroup.CalculateTotals();
                            }
                        }
                        //The below triggers an error when switching cells too fast, no idea why need to resolve.
                        //dgv_Overview.ItemsSource = null;
                        //dgv_Overview.ItemsSource = CarboLifeProject.getGroupList;
                        //SortData();
                    }
                }
            }
        }

        private void Mnu_DuplicateGroup_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                CarboLifeProject.DuplicateGroup(carboGroup);
            }
            SortData();
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
            SortData();
        }

        private void Mnu_Reinforce_Click(object sender, RoutedEventArgs e)
        {
            if (dgv_Overview.SelectedItems.Count > 0)
            {
                var selectedItems = dgv_Overview.SelectedItems;
                IList<CarboGroup> selectedGroups = new List<CarboGroup>();

                // ... Add all Names to a List.
                foreach (var item in selectedItems)
                {
                    CarboGroup cg = item as CarboGroup;
                    selectedGroups.Add(cg);
                }

                CarboGroup bufferGroup = selectedGroups[0].Copy();


                for(int i=1 ; i <= (selectedGroups.Count -1) ; i++)
                {
                    CarboGroup carboGroupTemp = selectedGroups[i];
                    bufferGroup.Volume += carboGroupTemp.Volume;
                }

                if(bufferGroup != null)
                {
                    ReinforcementWindow reinforementWindow = new ReinforcementWindow(CarboLifeProject.CarboDatabase, bufferGroup);
                    reinforementWindow.ShowDialog();

                    if (reinforementWindow.isAccepted == true)
                    {
                        if (reinforementWindow.createNew == true)
                        {
                            CarboLifeProject.AddGroup(reinforementWindow.reinforcementGroup);
                        }
                        else
                        {
                            foreach (var item in selectedItems)
                            {
                                CarboGroup cg = item as CarboGroup;
                                if (cg != null)
                                {
                                    cg.Additional = reinforementWindow.addtionalValue;
                                    cg.AdditionalDescription = reinforementWindow.additionalDescription;
                                }

                            }
                        }

                    }
                }
            }
           
            CarboLifeProject.CalculateProject();
            refreshData();
        }

        private void Mnu_Metaldeck_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;

            if (carboGroup != null)
            {
                    ProfileWindow ProfileWindowWindow = new ProfileWindow(CarboLifeProject.CarboDatabase, carboGroup);
                    ProfileWindowWindow.ShowDialog();

                if (ProfileWindowWindow.isAccepted == true)
                {
                    CarboLifeProject.AddGroup(ProfileWindowWindow.profileGroup);
                }
            }
            SortData();
        }

        private void SortByMaterial()
        {
            if (CarboLifeProject.getGroupList != null)
            {
                ListCollectionView collectionView = new ListCollectionView(CarboLifeProject.getGroupList);
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("MaterialName"));
                dgv_Overview.ItemsSource = null;
                dgv_Overview.ItemsSource = collectionView;
            }
        }

        private void SortByCategoty()
        {
            if (CarboLifeProject.getGroupList != null)
            {
                ListCollectionView collectionView = new ListCollectionView(CarboLifeProject.getGroupList);
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                dgv_Overview.ItemsSource = null;
                dgv_Overview.ItemsSource = collectionView;
            }
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            SortData();
        }

        private void SortData()
        {
            if(cbb_Sort.Text == "Material")
            {
                SortByMaterial();
            }
            else
            {
                SortByCategoty();
            }
        }

        private void Btn_ShowHideCorrections_Click(object sender, RoutedEventArgs e)
        {

            if (column_Correction.Visibility == Visibility.Hidden)
            {
                //column_Volume.Visibility = Visibility.Visible;
                column_Correction.Visibility = Visibility.Visible;
                column_Addition.Visibility = Visibility.Visible;
                column_Waste.Visibility = Visibility.Visible;
                column_B4.Visibility = Visibility.Visible;

            }
            else
            {
               // column_Volume.Visibility = Visibility.Hidden;
                column_Correction.Visibility = Visibility.Hidden;
                column_Addition.Visibility = Visibility.Hidden;
                column_Waste.Visibility = Visibility.Hidden;
                column_B4.Visibility = Visibility.Hidden;
            }

        }

        private void Mnu_MoveToNewGroup_Click(object sender, RoutedEventArgs e)
        {
            //IList<DataGridCellInfo> selectedElementList = dgv_Elements.SelectedCells;
            try
            {
                List<CarboElement> selectedCarboElementList = new List<CarboElement>();
                selectedCarboElementList = dgv_Elements.SelectedItems.Cast<CarboElement>().ToList();

                    if (selectedCarboElementList.Count > 0)
                {
                    CarboGroup selectedCarboGroup = (CarboGroup)dgv_Overview.SelectedItem;

                    //Reset all findme flags.
                    CarboLifeProject.ResetElementFlags();

        //Flag the elements that require updating
                    foreach (CarboElement ce in selectedCarboElementList)
                    {
                        ce.isUpdated = true;
                    }

                    int carbogroupId = selectedCarboGroup.Id;
                    List<CarboElement> allCarboElementList = selectedCarboGroup.AllElements;
                    CarboGroup newGroup = selectedCarboGroup.Copy();

                    //move all elements to the new group
                    newGroup.AllElements = selectedCarboElementList;
                    newGroup.Description = "A new Group";

                    int delcounter = 0;

                    //remove the old ones from the list
                    foreach (CarboElement ce in selectedCarboElementList)
                    {
                        for (int i = 0; i >= 0; i--)
                        {
                            CarboElement oldce = allCarboElementList[i];
                            if (oldce.isUpdated == true)
                            {
                                allCarboElementList.RemoveAt(i);
                                delcounter++;
                            }
                        }

                    }
                    //Now there should be two lists, one with the selcted items and one without.
                    foreach (CarboGroup cg in CarboLifeProject.getGroupList)
                    {
                        if (cg.Id == carbogroupId)
                            cg.AllElements = allCarboElementList;
                    }
                    CarboLifeProject.AddGroup(newGroup);

                    MessageBox.Show(delcounter + " Elements moved to new group", "Message", MessageBoxButton.OK);

                    CarboLifeProject.CalculateProject();
                    refreshData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK);
            }

            //List<CarboElement> selectedElement = dgv_Elements.SelectedCells;
        }

        private void Mnu_MergeGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<CarboGroup> selectedCarboGroupList = new List<CarboGroup>();
                selectedCarboGroupList = dgv_Overview.SelectedItems.Cast<CarboGroup>().ToList();


                if (selectedCarboGroupList != null && selectedCarboGroupList.Count > 1)
                {
                    CarboGroup FirstCarboGroup = selectedCarboGroupList[0];

                    CarboGroup mergedCarboGroup = FirstCarboGroup.Copy();
                    mergedCarboGroup.AllElements = new List<CarboElement>();

                    foreach (CarboGroup gc in selectedCarboGroupList)
                    {
                        if(gc.AllElements.Count > 0)
                        {
                            foreach (CarboElement ce in gc.AllElements)
                            {
                                mergedCarboGroup.AllElements.Add(ce);
                            }
                        }
                    }
                    CarboLifeProject.AddGroup(mergedCarboGroup);
                    foreach (CarboGroup cg in selectedCarboGroupList)
                    {
                        CarboLifeProject.DeleteGroup(cg);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK);
            }
        }

        private void Btn_Collaps_Click(object sender, RoutedEventArgs e)
        {
            double length = grd_Elements.Height.Value;
            if (length > 0)
            {
                grd_Elements.Height = new GridLength(0, GridUnitType.Pixel);
                btn_Collaps.Content = "▲";
                mnu_EditElements.Visibility = Visibility.Hidden;
          
            }
            else
            {
                grd_Elements.Height = new GridLength(200, GridUnitType.Pixel);
                btn_Collaps.Content = "▼";
                mnu_EditElements.Visibility = Visibility.Visible;

            }
        }

        private void mnu_CreateMaterialFromElement_Click(object sender, RoutedEventArgs e)
        {
            List<CarboElement> selectedCarboElementList = new List<CarboElement>();
            selectedCarboElementList = dgv_Elements.SelectedItems.Cast<CarboElement>().ToList();

            if(selectedCarboElementList.Count == 1)
            {
                CarboElement newBufferElement = selectedCarboElementList[0];
                string materialName = newBufferElement.MaterialName;
                if(materialName != "")
                    CarboLifeProject.CarboDatabase.AddMaterial(new CarboMaterial(materialName));
            }
            else
            {
                MessageBox.Show("Please select a element from the list");
            }
        }

        private void RoundValue(object sender, RoutedEventArgs e)
        {
            TextBlock tb = ((TextBlock)sender);

            // do anything with textblock    
            if (tb.Text != null)
            {
                double value = Utils.ConvertMeToDouble(tb.Text);

                tb.Text = Math.Round(value,2).ToString();
            }
        }

        private void PercentValue(object sender, RoutedEventArgs e)
        {
            TextBlock tb = ((TextBlock)sender);

            // do anything with textblock    
            if (tb.Text != null)
            {
                double value = Utils.ConvertMeToDouble(tb.Text);

                tb.Text = Math.Round(value, 2).ToString() + " % ";
            }
        }

        private void mnu_MapElements_Click(object sender, RoutedEventArgs e)
        {
            MaterialMapper materialMapper = new MaterialMapper(this.CarboLifeProject);
            materialMapper.ShowDialog();
            if(materialMapper.isAccepted == true)
            {
                this.CarboLifeProject.carboMaterialMap = materialMapper.mappinglist;
                this.CarboLifeProject.mapAllMaterials();
            }
        }

        private void btn_OpenMaterialEditor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check if a group has been selected:
                CarboGroup PotentialSelectedCarboGroup = new CarboGroup();
                PotentialSelectedCarboGroup.MaterialName = "";

                if (dgv_Overview.SelectedItems.Count > 0)
                {

                    var selectedItems = dgv_Overview.SelectedItems;
                    IList<CarboGroup> selectedGroups = new List<CarboGroup>();

                    // ... Add all Names to a List.
                    foreach (var item in selectedItems)
                    {
                        CarboGroup cg = item as CarboGroup;
                        if(cg != null)
                            selectedGroups.Add(cg);
                    }

                    if (selectedGroups.Count > 0)
                    {
                        PotentialSelectedCarboGroup = selectedGroups[0];
                    }
                }

                if (CarboLifeProject.CarboDatabase.CarboMaterialList.Count > 0)
                {
                    CarboMaterial carbomat = CarboLifeProject.CarboDatabase.CarboMaterialList[0];

                    if (PotentialSelectedCarboGroup.MaterialName != "")
                    {
                        //A group with a valid material was selected
                        carbomat = PotentialSelectedCarboGroup.Material;
                    }

                    if (carbomat == null)
                        carbomat = new CarboMaterial();

                    MaterialEditor materialEditor = new MaterialEditor(carbomat.Name, CarboLifeProject.CarboDatabase);
                    materialEditor.ShowDialog();

                    if (materialEditor.acceptNew == true)
                    {
                        CarboLifeProject.CarboDatabase = materialEditor.returnedDatabase;

                        CarboLifeProject.UpdateAllMaterials();
                    }
                }
                else
                {
                    MessageBox.Show("There were no materials found in the project, please re-create your project");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            CarboLifeProject.CalculateProject();
            refreshData();
        }

        private void btn_EditAdvanced_Click(object sender, RoutedEventArgs e)
        {

            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;

            if (carboGroup != null)
            {
                GroupAdvancedEditor advancedEditor = new GroupAdvancedEditor(carboGroup, CarboLifeProject.CarboDatabase);
                advancedEditor.ShowDialog();

                if (advancedEditor.isAccepted == true)
                {
                    carboGroup = advancedEditor.group;
                }
            }
            else
            {
                MessageBox.Show("Please select a group");
            }

            CarboLifeProject.CalculateProject();
            refreshData();
        }
    }
}
