using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using LiveCharts.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
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
            try
            {
                InitializeComponent();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                    HideRibbonThing();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void HideRibbonThing()
        {
            /*
            PropertyInfo pi = typeof(Ribbon).GetProperty("QatAbove", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo mi = pi.GetGetMethod(true);
            QuickAccessToolBar quat = (QuickAccessToolBar)mi.Invoke(ribbon, new object[0]);
            if (quat == null)
            {
                return;
            }

            StackPanel stackPanel = (StackPanel)VisualTreeHelper.GetChild(quat, 0);
            foreach (object child in stackPanel.Children)
            {
                if (child is QuickAccessToolBarCustomizeButton)
                {
                    ((QuickAccessToolBarCustomizeButton)child).Visibility = Visibility.Hidden;
                }
            }
            */
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

        private void Mnu_DeleteGroup_Click(object sender, MouseButtonEventArgs e)
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

            lbl_Total.Content = "TOTAL: " + Math.Round(totals, 2) + " tCO₂e";


            SortData();
        }

        private void Btn_Material_Click(object sender, RoutedEventArgs e)
        {

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
                                MessageBox.Show("The volume of this group is calculated using the elements' volumes extracted from the 3D model," + Environment.NewLine + " you need to purge the elements before overriding the volume");
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
                                carboGroup.inUseProperties.B4 = b4;

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


                for (int i = 1; i <= (selectedGroups.Count - 1); i++)
                {
                    CarboGroup carboGroupTemp = selectedGroups[i];
                    bufferGroup.Volume += carboGroupTemp.Volume;
                }

                if (bufferGroup != null)
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
            if (cbb_SortValue.Text == "Material")
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
            if (chx_AdvancedShow.IsChecked == true)
            {
                //column_Volume.Visibility = Visibility.Visible;
                column_Correction.Visibility = System.Windows.Visibility.Visible;
                column_Addition.Visibility = System.Windows.Visibility.Visible;
                column_Waste.Visibility = System.Windows.Visibility.Visible;
                column_B4.Visibility = System.Windows.Visibility.Visible;
                column_B1B7ECI.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                // column_Volume.Visibility = Visibility.Hidden;
                column_Correction.Visibility = System.Windows.Visibility.Hidden;
                column_Addition.Visibility = System.Windows.Visibility.Hidden;
                column_Waste.Visibility = System.Windows.Visibility.Hidden;
                column_B4.Visibility = System.Windows.Visibility.Hidden;
                column_B1B7ECI.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void Mnu_MoveToNewGroup_Click(object sender, RoutedEventArgs e)
        {
            //IList<DataGridCellInfo> selectedElementList = dgv_Elements.SelectedCells;
            try
            {
                List<CarboElement> selectedCarboElementList = new List<CarboElement>();
                List<CarboElement> carboElementsToCopy = new List<CarboElement>();

                selectedCarboElementList = dgv_Elements.SelectedItems.Cast<CarboElement>().ToList();

                if (selectedCarboElementList.Count > 0)
                {

                    foreach (CarboElement carboElement in selectedCarboElementList)
                    {
                        carboElementsToCopy.Add(carboElement.CopyMe());
                    }

                    CarboGroup selectedCarboGroup = (CarboGroup)dgv_Overview.SelectedItem;

                    //Reset all findme flags.
                    CarboLifeProject.ResetElementFlags();

                    //Flag the elements that require updating
                    foreach (CarboElement ce in selectedCarboElementList)
                    {
                        ce.isUpdated = true;
                    }

                    List<CarboElement> allCarboElementList = selectedCarboGroup.AllElements;
                    CarboGroup newGroup = selectedCarboGroup.Copy();

                    //move all elements to the new group
                    newGroup.AllElements = carboElementsToCopy;
                    newGroup.Description = "Copy of: " + newGroup.Description;

                    //remove the old ones from the list
                    foreach (CarboElement ce in selectedCarboElementList)
                    {
                        for (int j = selectedCarboGroup.AllElements.Count - 1; j >= 0; j--)
                        {
                            CarboElement ceg = selectedCarboGroup.AllElements[j];
                            if(ceg != null)
                            {
                                if (ce.Id == ceg.Id)
                                {
                                    selectedCarboGroup.AllElements.RemoveAt(j);
                                }
                            }
                        }
                    }

                    //Add the new group
                    CarboLifeProject.AddGroup(newGroup);

                    MessageBox.Show(selectedCarboElementList.Count + " elements moved to new group", "Message", MessageBoxButton.OK);

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

        private void Mnu_MergeGroup_Click(object sender, MouseButtonEventArgs e)
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
                        if (gc.AllElements.Count > 0)
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
                mnu_EditElements.Visibility = System.Windows.Visibility.Hidden;

            }
            else
            {
                grd_Elements.Height = new GridLength(200, GridUnitType.Pixel);
                btn_Collaps.Content = "▼";
                mnu_EditElements.Visibility = System.Windows.Visibility.Visible;

            }
        }

        private void mnu_CreateMaterialFromElement_Click(object sender, RoutedEventArgs e)
        {
            List<CarboElement> selectedCarboElementList = new List<CarboElement>();
            selectedCarboElementList = dgv_Elements.SelectedItems.Cast<CarboElement>().ToList();

            if (selectedCarboElementList.Count == 1)
            {
                CarboElement newBufferElement = selectedCarboElementList[0];
                string materialName = newBufferElement.MaterialName;
                if (materialName != "")
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

                tb.Text = Math.Round(value, 3).ToString();
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
            if (materialMapper.isAccepted == true)
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
                        if (cg != null)
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

        private void RibbonMenuButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedItems = dgv_Overview.SelectedItems;
            IList<CarboGroup> selectedGroups = new List<CarboGroup>();

            // ... Add all Names to a List.
            foreach (var item in selectedItems)
            {
                CarboGroup cg = item as CarboGroup;
                if (cg != null)
                    selectedGroups.Add(cg);
            }

            if (selectedGroups.Count > 1)
                MessageBox.Show("Please select only a single group");
            else
            {
                GroupWindow splitter = new GroupWindow(selectedGroups[0]);
                splitter.ShowDialog();

                if(splitter.dialogOk == true)
                {
                    CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
                    
                    //Update List of Current
                    carboGroup.AllElements.Clear();

                    foreach (CarboElement el in splitter.GrpPassed.AllElements)
                    {
                        carboGroup.AllElements.Add(el);
                    }
                    
                    //Add new list
                    CarboLifeProject.AddGroup(splitter.GrpFiltered);
                }


            }

        }
        private void btn_EditInUseValues_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = dgv_Overview.SelectedItems;
            IList<CarboGroup> selectedGroups = new List<CarboGroup>();

            // ... Add all Names to a List.
            foreach (var item in selectedItems)
            {
                CarboGroup cg = item as CarboGroup;
                if (cg != null)
                    selectedGroups.Add(cg);
            }

            CarboGroup bufferGroup = null;

            //Get one item to edit.
            if (selectedGroups.Count > 0)
                 bufferGroup = selectedGroups[0].Copy();


            if (bufferGroup != null)
            {
                CarboB1B7Properties propertiesToEdit = bufferGroup.inUseProperties;
                if (propertiesToEdit != null)
                {
                    MaterialLifePicker inUseEditor = new MaterialLifePicker(propertiesToEdit, CarboLifeProject.designLife);
                    inUseEditor.ShowDialog();

                    if (inUseEditor.isAccepted == true)
                    {
                        //Update all the selected CarboGroups
                        foreach (var item in selectedItems)
                        {
                            CarboGroup cg = item as CarboGroup;
                            if (cg != null)
                            {
                                cg.inUseProperties = inUseEditor.inUseProperties;

                                //cg.B4Factor = inUseEditor.materialB1B5Properties.B4;
                            }

                        }
                    }
                }

            }

            CarboLifeProject.CalculateProject();
            refreshData();

        }

        private void rbn_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void RibbonQuickAccessToolBar_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void fe_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;

        }

        private void Mnu_AutoRCGroups(object sender, RoutedEventArgs e)
        {
            if(CarboLifeProject.RevitImportSettings == null)
            {
                CarboLifeProject.RevitImportSettings = CarboLifeProject.RevitImportSettings.DeSerializeXML();
            }

            MaterialConcreteMapper concreteMapper = new MaterialConcreteMapper(CarboLifeProject.RevitImportSettings);
            concreteMapper.ShowDialog();
            if (concreteMapper.isAccepted == true)
            {
                try
                {
                    CarboLifeProject.RevitImportSettings.rcQuantityMap = concreteMapper.rcMap;
                    CarboLifeProject.RevitImportSettings.RCParameterType = concreteMapper.categoryType;
                    CarboLifeProject.RevitImportSettings.RCParameterName = concreteMapper.categoryName;
                    CarboLifeProject.RevitImportSettings.RCMaterialName = concreteMapper.carboMaterialName;
                    CarboLifeProject.RevitImportSettings.RCMaterialCategory = concreteMapper.carboMaterialCategory;

                    CarboLifeProject.CreateReinforcementGroup();
                }
                catch (Exception ex) { }
            }
        }
    }
}
