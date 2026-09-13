using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using System.Windows.Markup;
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
                //SortData owns ItemsSource now, and refreshData also fills in the TOTAL label,
                //which used to sit on its placeholder text until the first recalculation.
                refreshData();

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

        /// <summary>
        /// Deletes the selected groups.
        ///
        /// Wired to Click, not PreviewMouseDown. A destructive action on mouse-down fires while
        /// the button is still held, so pressing and dragging away still deleted; and being a
        /// tunnelling preview event it ran before the button's own behaviour.
        /// </summary>
        private void Mnu_DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroups = dgv_Overview.SelectedItems.Cast<CarboGroup>().ToList();

            if (!selectedGroups.Any())
            {
                MessageBox.Show("Please select one or more groups to delete.",
                                "No Selection",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            // Confirm deletion
            var confirm = MessageBox.Show(
                $"Are you sure you want to delete {selectedGroups.Count} group(s)?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            // Perform deletion
            foreach (var carboGroup in selectedGroups)
            {
                CarboLifeProject.DeleteGroup(carboGroup);
            }

            CarboLifeProject.CalculateProject();
            refreshData();

            // Notify completion
            MessageBox.Show($"{selectedGroups.Count} group(s) deleted successfully.",
                            "Deletion Complete",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        /// <summary>
        /// Recalculates the project and puts the result on screen.
        ///
        /// Anything that changes a group, a material or an element should call this rather than
        /// SortData: a sort only reorders what is already there, so a handler that sorted without
        /// recalculating left the grid looking updated while the numbers and the TOTAL label were
        /// still the previous ones. The placeholder on that label - "TOTAL: xxx tCO₂e (Recalculate
        /// to refresh)" - is what that used to feel like.
        /// </summary>
        private void ApplyAndRefresh()
        {
            if (CarboLifeProject == null)
                return;

            CarboLifeProject.CalculateProject();
            refreshData();
        }

        public void refreshData()
        {
            if (CarboLifeProject == null)
                return;

            //A cell still in edit blocks the collection view from refreshing.
            dgv_Overview.CommitEdit(DataGridEditingUnit.Row, true);

            //Where the user was, so the grid stops jumping back to the top and losing the row
            //they were working on every time a value changes.
            List<int> selectedIds = GetSelectedGroupIds();
            double scrollOffset = GetVerticalScrollOffset(dgv_Overview);

            SortData();

            UpdateTotalLabel();

            RestoreSelection(selectedIds, scrollOffset);
        }

        /// <summary>
        /// Puts the project total on the label. One place, so no path can leave it stale.
        /// </summary>
        private void UpdateTotalLabel()
        {
            double totals = 0;

            if (CarboLifeProject != null && CarboLifeProject.getGroupList != null
                && CarboLifeProject.getGroupList.Count > 0)
            {
                totals = CarboLifeProject.getTotalsGroup().EC;
            }

            lbl_Total.Content = "TOTAL: " + Math.Round(totals, 2) + " tCO₂e";
        }

        private List<int> GetSelectedGroupIds()
        {
            List<int> ids = new List<int>();

            if (dgv_Overview.SelectedItems == null)
                return ids;

            foreach (object item in dgv_Overview.SelectedItems)
            {
                CarboGroup cg = item as CarboGroup;

                if (cg != null)
                    ids.Add(cg.Id);
            }

            return ids;
        }

        /// <summary>
        /// Reselects the groups that were selected before the refresh and scrolls back to where
        /// the user was. Selection is restored by group Id rather than by object reference,
        /// because a rebuilt collection view hands back different row containers.
        /// </summary>
        private void RestoreSelection(List<int> selectedIds, double scrollOffset)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                return;

            try
            {
                dgv_Overview.SelectedItems.Clear();

                CarboGroup first = null;

                foreach (CarboGroup cg in CarboLifeProject.getGroupList)
                {
                    if (selectedIds.Contains(cg.Id) == false)
                        continue;

                    dgv_Overview.SelectedItems.Add(cg);

                    if (first == null)
                        first = cg;
                }

                //The element grid below follows the selection, so put it back too.
                if (first != null)
                    dgv_Elements.ItemsSource = first.AllElements;

                SetVerticalScrollOffset(dgv_Overview, scrollOffset);
            }
            catch
            {
                //Losing the selection is not worth an error dialog.
            }
        }

        private static double GetVerticalScrollOffset(DependencyObject grid)
        {
            ScrollViewer viewer = FindScrollViewer(grid);
            return viewer != null ? viewer.VerticalOffset : 0;
        }

        private static void SetVerticalScrollOffset(DependencyObject grid, double offset)
        {
            if (offset <= 0)
                return;

            ScrollViewer viewer = FindScrollViewer(grid);

            if (viewer != null)
                viewer.ScrollToVerticalOffset(offset);
        }

        /// <summary>
        /// The DataGrid's own scroll viewer, which only exists once the template has been applied.
        /// </summary>
        private static ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            if (parent == null)
                return null;

            ScrollViewer found = parent as ScrollViewer;

            if (found != null)
                return found;

            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                ScrollViewer child = FindScrollViewer(VisualTreeHelper.GetChild(parent, i));

                if (child != null)
                    return child;
            }

            return null;
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
            ApplyAndRefresh();
        }

        //A second Mnu_DeleteGroup_Click taking RoutedEventArgs used to live here. It was never
        //wired to anything - the XAML pointed at the MouseButtonEventArgs overload - and it
        //deleted a single group with no confirmation, so it would have been the worse of the two
        //had anyone connected it.

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
            ApplyAndRefresh();
        }

        private void Mnu_PurgeElements_Click(object sender, RoutedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                if (carboGroup.AllElements.Count > 0)
                {
                    MessageBoxResult result = MessageBox.Show("Do you really want to remove all elements from this collection? This action can NOT be undone", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Stop);
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
            ApplyAndRefresh();
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
            ApplyAndRefresh();
        }

        //SortByMaterial and SortByCategoty are gone: SortData covers both against one reused
        //collection view, instead of each building its own and re-sourcing the grid.

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            SortData();
        }

        /// <summary>The one collection view behind the group grid, kept and reused.</summary>
        private ListCollectionView groupView;

        /// <summary>What groupView is grouped by, so it is only rebuilt when that changes.</summary>
        private string groupViewGroupedBy;

        /// <summary>
        /// Groups the grid by material or by category.
        ///
        /// The view is built once and then refreshed in place. It used to be rebuilt from scratch
        /// on every call, and because refreshData assigned ItemsSource, then SortByX assigned null
        /// and then a brand new ListCollectionView, the grid was re-sourced three times per
        /// refresh and landed on a view that had never heard of the previous selection. That is
        /// why the selected row, the scroll position and the element grid were lost every time.
        /// </summary>
        private void SortData()
        {
            if (CarboLifeProject == null || CarboLifeProject.getGroupList == null)
                return;

            string groupBy = cbb_SortValue.Text == "Material" ? "MaterialName" : "Category";

            bool needsRebuild = groupView == null
                             || groupViewGroupedBy != groupBy
                             || ReferenceEquals(groupView.SourceCollection, CarboLifeProject.getGroupList) == false;

            if (needsRebuild)
            {
                RebuildGroupView(groupBy);
                return;
            }

            //Same list, same grouping: re-read the values. CarboGroup raises no change
            //notifications, so this is what makes new EC and mass figures appear.
            try
            {
                groupView.Refresh();
            }
            catch (InvalidOperationException)
            {
                //A view mid-edit refuses to refresh; rebuilding always works.
                RebuildGroupView(groupBy);
            }
        }

        private void RebuildGroupView(string groupBy)
        {
            groupView = new ListCollectionView(CarboLifeProject.getGroupList);
            groupView.GroupDescriptions.Add(new PropertyGroupDescription(groupBy));
            groupViewGroupedBy = groupBy;

            dgv_Overview.ItemsSource = groupView;
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

                ApplyAndRefresh();
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
                {
                    CarboLifeProject.CarboDatabase.AddMaterial(new CarboMaterial(materialName));

                    //The database gained a row, so the grid and the totals are re-read. The new
                    //material carries no carbon yet, so nothing should move - but if it does, the
                    //user sees it now rather than at the next unrelated recalculation.
                    ApplyAndRefresh();
                }
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
                string textValue = Math.Round(value, 3).ToString(CultureInfo.InvariantCulture);
                tb.Text = textValue;
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
            //Same call the Revit import makes when the user asks for the mapper straight after
            //an import, so both routes leave the project in the same state.
            //
            //The mapper has just changed the materials, so the totals and the grid have to
            //follow. Without this the numbers stayed on screen from before the mapping until
            //the user happened to press Calculate.
            if (MaterialMapper.MapProject(this.CarboLifeProject) == true)
                ApplyAndRefresh();
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

        private void Mnu_SplitGroup_Click(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Rebuilds the reinforcement groups: the mapper collects the quantities, then the old
        /// generated groups are replaced by a fresh set. Wired to both the Auto Groups button and
        /// its Reinforcement menu item.
        /// </summary>
        private void Mnu_AutoRCGroups(object sender, RoutedEventArgs e)
        {
            //A RibbonSplitButton shares its Click event with the menu items in its drop down, and it
            //bubbles, so a menu item click would run its own handler and then this one again.
            e.Handled = true;

            //DeSerializeXML is an instance method, so calling it on the reference that was just
            //found to be null threw a NullReferenceException instead of recovering the settings.
            if (CarboLifeProject.RevitImportSettings == null)
            {
                CarboLifeProject.RevitImportSettings = new CarboGroupSettings().DeSerializeXML();
            }

            //Passing the project adds the material and category boxes to the mapper. Without them
            //a project that was imported before those were ever set had nothing to reinforce with,
            //and the command could only report that afterwards.
            MaterialConcreteMapper concreteMapper = new MaterialConcreteMapper(CarboLifeProject.RevitImportSettings, CarboLifeProject);
            concreteMapper.ShowDialog();
            if (concreteMapper.isAccepted == true)
            {
                try
                {
                    CarboLifeProject.RevitImportSettings.rcQuantityMap = concreteMapper.rcMap;
                    CarboLifeProject.RevitImportSettings.RCParameterType = concreteMapper.categoryType;
                    CarboLifeProject.RevitImportSettings.RCParameterName = concreteMapper.categoryName;

                    CarboLifeProject.RevitImportSettings.RCMaterialName = concreteMapper.materialName;
                    CarboLifeProject.RevitImportSettings.RCMaterialCategory = concreteMapper.materialCategory;

                    //Asking for the groups here switches the allowance on, see CreateReinforcementGroup.
                    int added = CarboLifeProject.CreateReinforcementGroup();

                    CarboLifeProject.CalculateProject();
                    refreshData();

                    if (added == 0)
                    {
                        CarboGroupSettings settings = CarboLifeProject.RevitImportSettings;

                        MessageBox.Show("No reinforcement groups were added." + Environment.NewLine + Environment.NewLine +
                                        WhyNoAllowanceGroups("reinforcement", settings.RCMaterialName,
                                                              settings.RCMaterialCategory, null),
                                        "Reinforcement", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    //Swallowed silently before, so a failed rebuild looked like a rebuild that
                    //found nothing to do. Reported the same way as the connection groups below.
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// Rebuilds the steel connection allowance groups. Asking for them here switches the
        /// allowance on, so it does not matter whether it was ticked at import time.
        /// </summary>
        private void Mnu_AutoSteelConnectionGroups(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            BuildConnectionGroups(true);
        }

        /// <summary>
        /// Rebuilds the timber connection allowance groups. See Mnu_AutoSteelConnectionGroups.
        /// </summary>
        private void Mnu_AutoTimberConnectionGroups(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            BuildConnectionGroups(false);
        }

        /// <summary>
        /// Asks for the settings one kind of connection allowance needs, rebuilds it and reports the
        /// outcome.
        ///
        /// The settings used to be taken straight from the import settings, which are empty in every
        /// project where nobody opened that dialog: the command then built nothing and could only
        /// say afterwards which setting was missing. ConnectionWindow asks first, showing whatever
        /// the project already has, so the command always has something to work with.
        /// </summary>
        /// <param name="steel">True for the steel allowance, false for the timber one</param>
        private void BuildConnectionGroups(bool steel)
        {
            if (CarboLifeProject == null)
                return;

            //See Mnu_AutoRCGroups: this recovers the stored defaults instead of dereferencing null.
            if (CarboLifeProject.RevitImportSettings == null)
            {
                CarboLifeProject.RevitImportSettings = new CarboGroupSettings().DeSerializeXML();
            }

            CarboGroupSettings settings = CarboLifeProject.RevitImportSettings;

            string name = steel ? "steel connection" : "timber connection";

            ConnectionWindow connectionWindow = new ConnectionWindow(CarboLifeProject, steel);
            connectionWindow.ShowDialog();

            if (connectionWindow.isAccepted == false)
                return;

            string material = connectionWindow.SelectedMaterialName;
            string category = connectionWindow.SelectedCategory;
            double percentage = connectionWindow.SelectedPercentage;

            //The generators read these back out of the project, so they are stored before the call.
            if (steel == true)
            {
                settings.SteelConnectionMaterialName = material;
                settings.SteelMaterialCategory = category;
                settings.SteelConnectionPercentage = percentage;
            }
            else
            {
                settings.TimberConnectionMaterialName = material;
                settings.TimberMaterialCategory = category;
                settings.TimberConnectionPercentage = percentage;
            }

            if (connectionWindow.SaveAsDefault == true)
            {
                SaveConnectionDefaults(steel, material, category, percentage);
            }

            try
            {
                int added = steel
                    ? CarboLifeProject.CreateSteelConnectionGroups()
                    : CarboLifeProject.CreateTimberConnectionGroups();

                CarboLifeProject.CalculateProject();
                refreshData();

                if (added == 0)
                {
                    MessageBox.Show("No " + name + " groups were added." + Environment.NewLine + Environment.NewLine +
                                    WhyNoAllowanceGroups(name, material, category, percentage),
                                    "Connections", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                MessageBox.Show(added.ToString() + " " + name + " group(s) added.",
                                "Connections", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Stores one connection allowance in the application defaults, so the next project starts
        /// with it instead of asking for the same three values again. The other settings in that
        /// file are left exactly as they are.
        /// </summary>
        /// <param name="steel">True for the steel allowance, false for the timber one</param>
        /// <param name="material">The material the allowance is made of</param>
        /// <param name="category">The material category the allowance is worked out from</param>
        /// <param name="percentage">The allowance as a percentage of the group volume</param>
        private static void SaveConnectionDefaults(bool steel, string material, string category, double percentage)
        {
            try
            {
                CarboSettings appSettings = new CarboSettings().Load();

                if (appSettings == null || appSettings.defaultCarboGroupSettings == null)
                    return;

                if (steel == true)
                {
                    appSettings.defaultCarboGroupSettings.mapSteelConnections = true;
                    appSettings.defaultCarboGroupSettings.SteelConnectionMaterialName = material;
                    appSettings.defaultCarboGroupSettings.SteelMaterialCategory = category;
                    appSettings.defaultCarboGroupSettings.SteelConnectionPercentage = percentage;
                }
                else
                {
                    appSettings.defaultCarboGroupSettings.mapTimberConnections = true;
                    appSettings.defaultCarboGroupSettings.TimberConnectionMaterialName = material;
                    appSettings.defaultCarboGroupSettings.TimberMaterialCategory = category;
                    appSettings.defaultCarboGroupSettings.TimberConnectionPercentage = percentage;
                }

                appSettings.Save();
            }
            catch (Exception ex)
            {
                //The groups themselves are unaffected by this, so it is reported and no more.
                MessageBox.Show("The defaults could not be saved: " + ex.Message,
                                "Connections", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Works out which setting stopped an allowance from producing anything.
        /// </summary>
        /// <param name="name">What to call the allowance in the message</param>
        /// <param name="material">The material set for it</param>
        /// <param name="category">The material category its parent groups must be in</param>
        /// <param name="percentage">The allowance percentage, null for reinforcement which has none</param>
        private static string WhyNoAllowanceGroups(string name, string material, string category, double? percentage)
        {
            if (string.IsNullOrEmpty(material))
                return "No " + name + " material is set for this project.";

            if (string.IsNullOrEmpty(category))
                return "No material category is set for the " + name + " allowance.";

            if (percentage.HasValue && percentage.Value <= 0)
                return "The " + name + " allowance is set to " + percentage.Value.ToString() + "%.";

            return "No groups use a material in category \"" + category + "\".";
        }

        private void Mnu_RemoveAutoReinforcement(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            RemoveAutoGroups("reinforcement", CarboGroupOrigin.Reinforcement);
        }

        private void Mnu_RemoveSteelConnections(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            RemoveAutoGroups("steel connection", CarboGroupOrigin.SteelConnection);
        }

        private void Mnu_RemoveTimberConnections(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            RemoveAutoGroups("timber connection", CarboGroupOrigin.TimberConnection);
        }

        private void Mnu_RemoveAllAutoGroups(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            //No origins means every generated group.
            RemoveAutoGroups("generated");
        }

        /// <summary>
        /// Drops the generated groups of the given origins and reports what went. Only groups the
        /// app made itself are ever touched, so nothing holding real elements can be lost here.
        /// </summary>
        /// <param name="name">What to call them in the message</param>
        /// <param name="origins">Which generated groups to remove, all of them when left empty</param>
        private void RemoveAutoGroups(string name, params CarboGroupOrigin[] origins)
        {
            if (CarboLifeProject == null)
                return;

            int removed = CarboLifeProject.RemoveAutoGroups(origins);

            if (removed == 0)
            {
                MessageBox.Show("There are no " + name + " groups to remove.",
                                "Remove groups", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            CarboLifeProject.CalculateProject();
            refreshData();

            MessageBox.Show(removed.ToString() + " " + name + " group(s) removed.",
                            "Remove groups", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void mnu_noWaste(object sender, RoutedEventArgs e)
        {
            CarboLifeProject.RemoveWaste();
            CarboLifeProject.CalculateProject();
            refreshData();
        }

        //A second mnu_MapElements_Click taking MouseButtonEventArgs used to live here, wired to
        //the ribbon's PreviewMouseDown while an identical RoutedEventArgs copy sat unused higher
        //up. The ribbon now uses Click, so the RoutedEventArgs one is the live handler and this
        //duplicate is gone.

    }
}
