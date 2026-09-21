using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
    public partial class MaterialMapper : Window
    {
        internal bool isAccepted;
        public string sourcePath;
        public List<CarboMapElement> mappinglist { get; set; }

        /// <summary>
        /// Every material name this project's database holds, sorted, spelled the way the database
        /// spells it. The one list every box in the grid is resolved against.
        /// </summary>
        private List<string> materialNameList = new List<string>();

        /// <summary>
        /// A FRESH COPY of <see cref="materialNameList"/> on every read - one per row.
        ///
        /// The type-to-search narrowing is done with Items.Filter, and that filter lives on the
        /// CollectionView, of which WPF makes exactly one per collection INSTANCE. Hand every row
        /// the same List and typing in one row silently narrows every other row's dropdown too.
        /// Each ComboBox binding here calls this getter once, so each gets a list of its own; the
        /// same trap is written up at length in CarboParameterPicker.
        /// </summary>
        public List<string> materialNames
        {
            get { return new List<string>(materialNameList); }
        }

        /// <summary>
        /// The material template this project is mapping into. Every row written from this dialog
        /// belongs to it, so rows for the other templates in the shared file are never touched.
        /// </summary>
        private string mappingTemplateName = "";

        public MaterialMapper(CarboProject carboProject)
        {
            List<CarboMaterial> list = carboProject.CarboDatabase.CarboMaterialList.OrderBy(o => o.Name).ToList();
            //list.Sort();


            this.InitializeComponent();

            try
            {
                mappingTemplateName = carboProject.CarboDatabase.templateName ?? "";

                mappinglist = new List<CarboMapElement>();
                mappinglist = Utils.GenerateMappinglist(carboProject);

                //Plain strings rather than a wrapper object: every row gets its own copy of this
                //list (see materialNames), and a copy of a few hundred strings is free where a
                //copy of a few hundred objects would not be. Duplicates dropped because the
                //dropdown showing the same name twice tells the user nothing.
                materialNameList = new List<string>();
                foreach (CarboMaterial cm in list)
                {
                    if (cm == null || string.IsNullOrEmpty(cm.Name))
                        continue;

                    if (materialNameList.Contains(cm.Name) == false)
                        materialNameList.Add(cm.Name);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error generating mapping list: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            DataContext = this;

        }

        /// <summary>
        /// Shows the mapper over a project and applies whatever the user mapped.
        ///
        /// Everything a caller has ever wanted from this dialog, in one place: the Revit import
        /// offers it straight after collecting elements, and the main window offers it from the
        /// ribbon. Both have to leave the project in the same state, or a material mapped during
        /// an import would behave differently from the same material mapped a minute later.
        /// </summary>
        /// <returns>
        /// True when the user accepted and the project was changed, so a caller with a view on
        /// screen knows it has to recalculate and refresh.
        /// </returns>
        public static bool MapProject(CarboProject carboProject)
        {
            if (carboProject == null)
                return false;

            MaterialMapper mapper = new MaterialMapper(carboProject);
            mapper.ShowDialog();

            if (mapper.isAccepted == false)
                return false;

            carboProject.carboMaterialMap = mapper.mappinglist;

            //The user just chose these in the mapper, so the groups they cover are marked as
            //user assigned and stop being flagged for review.
            carboProject.mapAllMaterials(CarboMaterialSource.UserAssigned);

            return true;
        }

        public MaterialMapper()
        {
           // this.InitializeComponent();

            //Locations = new List<MyLocation> { new MyLocation { Location = "London", NAMEID = 1 }, new MyLocation { Location = "Amsterdam" } };
            //Persons = new List<Person> { new Person { NAME = "Jack", NAMEID = 1 }, new Person { NAME = "Jill", NAMEID = 2 } };

            //DataContext = this;
        }

        private List<CarboMapElement> GenerateMappinglist(CarboProject returnedDatabase)
        {
            List<CarboMapElement> result = new List<CarboMapElement>
            {
                new CarboMapElement{revitName = "Revit Material 1", carboNAME = "Carbo Material 1" },
                new CarboMapElement{revitName = "Revit Material 2", carboNAME = "Carbo Material 2" }
            };

        return result;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ViewModel vm = new ViewModel();
            //dgData.ItemsSource = vm;
        }


        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            //Whatever is in the box the user is still standing in. A box writes its text to the
            //row when the focus leaves it, and clicking a button normally does that first - but
            //Accept can also be reached without the focus ever having been in a box's editor, so
            //this is not left to chance.
            SettleFocusedMaterialBox();

            if (WarnAboutUnknownMaterials() == false)
                return;

            if(chk_SaveMappingFile.IsChecked == true)
            {
                try
                {
                    //Only this session's rows, and only the ones for the template this project is
                    //using. GenerateMappinglist already stamps that template on every row it
                    //makes; filtering again means a row for someone else's template can never be
                    //written from here even if one somehow reached this list.
                    CarboMapFile CurrentMappingFile = new CarboMapFile();
                    CurrentMappingFile.mappingTable = mappinglist;
                    CurrentMappingFile.mappingTable = CurrentMappingFile.RowsForTemplate(mappingTemplateName);

                    //No load-then-merge here any more: that was the race. SaveToXml re-reads the
                    //file at the moment of writing and merges these rows into whatever it finds,
                    //so a colleague who saved in the meantime keeps their work. It also refuses
                    //outright if the file exists but cannot be parsed.
                    string error;
                    if (CurrentMappingFile.SaveToXml("", out error) == false)
                    {
                        System.Windows.MessageBox.Show(error, "Mapping not saved",
                                                       MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error saving mapping file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

            isAccepted = true;
            this.Close();
        }

        public IEnumerable<DataGridRow> GetDataGridRows(System.Windows.Controls.DataGrid grid)
        {
            var itemsSource = grid.ItemsSource as System.Collections.IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }



        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        //  The material column
        //
        //  WHAT WAS WRONG WITH IT
        //
        //  The box was bound to SelectedValue, and an editable combo box only turns typed text
        //  into a selection when its own prefix search is doing the matching. That search is off
        //  here on purpose - it is prefix only and it rewrites what you type, so "c32" could
        //  never find "Concrete C32/40" - which left nothing at all turning text into a value.
        //  Type or paste a material name and it showed in the box, looked accepted, and was
        //  never written to the row. That is the "it does not save what I paste" report.
        //
        //  The narrowing made it worse. It replaced ItemsSource on every keystroke, and replacing
        //  the items of a combo box clears its selection - which, through the SelectedValue
        //  binding, wrote an empty material over the row. Then a SelectionChanged handler put the
        //  full list back DURING the selection, clearing it a second time. So picking from the
        //  narrowed list, the one thing that was supposed to work, could also come out empty.
        //
        //  HOW IT WORKS NOW
        //
        //  The row's value is bound to Text, so it is the text - typed, pasted, or written there
        //  by picking from the list - that is stored, and no amount of fiddling with the item
        //  list can wipe it. Narrowing is done with Items.Filter on the box's own copy of the
        //  list, which never touches ItemsSource. Everything that touches the filter restores the
        //  text and the caret in the same synchronous block, because dropping or applying a
        //  filter can still clear the editor on its way past.
        //
        //  A name the database does not hold is kept, not rejected - the user may be half way
        //  through typing it - but the box is outlined and Accept says so, because such a row is
        //  looked up with GetExcactMatch and silently does nothing.
        //
        //  The same ground is covered for the settings dialog by CarboParameterPicker; this is
        //  not shared with it because a grid cell has to survive being handed to another row.
        // ═══════════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// What one material box has to remember, parked on the control itself rather than in a
        /// table keyed by control - a table like that would outlive every dialog that ever opened.
        /// </summary>
        private class MaterialBoxState
        {
            /// <summary>
            /// Re-entrancy guard. Restoring the text after a filter raises TextChanged again, and
            /// opening the dropdown from inside the filter raises DropDownOpened; without this the
            /// handlers chase each other round.
            /// </summary>
            public bool Busy;

            /// <summary>The editor inside the control template, once it exists.</summary>
            public System.Windows.Controls.TextBox Editor;
        }

        private void MaterialBox_Loaded(object sender, RoutedEventArgs e)
        {
            AttachMaterialBox(sender as System.Windows.Controls.ComboBox);
        }

        /// <summary>
        /// Wires one cell's combo box up as a type-to-search picker over this project's materials.
        /// Safe to call again on a box that is already wired: rows scroll out of view and back.
        /// </summary>
        private void AttachMaterialBox(System.Windows.Controls.ComboBox box)
        {
            if (box == null)
                return;

            if (box.Tag is MaterialBoxState)
            {
                //Already wired. Only the mark is worth redoing, in case the row changed while the
                //box was out of view.
                MarkMaterialBox(box);
                return;
            }

            //The editor only exists once the template has been applied, and inside a DataGrid the
            //cell's own Loaded can beat the template to it.
            box.ApplyTemplate();

            System.Windows.Controls.TextBox editor = box.Template == null
                ? null
                : box.Template.FindName("PART_EditableTextBox", box) as System.Windows.Controls.TextBox;

            if (editor == null)
                return;   //Not templated yet: no state stored, so the next Loaded has another go.

            MaterialBoxState state = new MaterialBoxState();
            state.Editor = editor;
            box.Tag = state;

            //TextChanged rather than KeyUp. KeyUp misses a paste made from the context menu and a
            //drag and drop into the box entirely, and sees Ctrl+V only as the V coming back up -
            //which is the other half of the "my paste is ignored" report.
            editor.TextChanged += delegate
            {
                if (state.Busy)
                    return;

                state.Busy = true;

                try
                {
                    FilterMaterialBox(box, state, true);
                }
                finally
                {
                    state.Busy = false;
                }
            };

            //Leaving the box is the moment the answer counts.
            editor.LostFocus += delegate
            {
                SettleMaterialBox(box, state);
            };

            box.SelectionChanged += delegate
            {
                if (state.Busy)
                    return;

                //Raised BEFORE the combo box has written the pick into its editor, so reading the
                //text here hands back the value before the pick. Settle once it has finished with
                //itself; the box keeps the focus after a pick, so nothing else would do it.
                box.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                           new Action(delegate { SettleMaterialBox(box, state); }));
            };

            box.PreviewKeyDown += delegate (object s, System.Windows.Input.KeyEventArgs args)
            {
                if (args.Key == Key.Escape && box.IsDropDownOpen)
                {
                    //Close the list, and do not let the keystroke travel on to the window - where
                    //it would be read as Cancel and throw away everything mapped so far.
                    box.IsDropDownOpen = false;
                    args.Handled = true;
                    return;
                }

                if (args.Key == Key.Enter && box.IsDropDownOpen == false)
                {
                    //Enter on a closed list is the user saying "that is the one" without moving
                    //off the cell. Nothing else in this window listens for it.
                    SettleMaterialBox(box, state);
                    args.Handled = true;
                }
            };

            MarkMaterialBox(box);
        }

        /// <summary>
        /// Narrows the list to the materials that match what is typed, then puts the typed text
        /// and the caret back.
        /// </summary>
        /// <param name="narrow">
        /// False restores the full list. Used on the way out, so a box is never left holding a
        /// narrowed view for the next person to look at it to inherit.
        /// </param>
        private void FilterMaterialBox(System.Windows.Controls.ComboBox box, MaterialBoxState state, bool narrow)
        {
            string typed = state.Editor.Text == null ? "" : state.Editor.Text;
            int caret = state.Editor.SelectionStart;

            if (narrow == false || typed.Trim().Length == 0)
            {
                box.Items.Filter = null;
            }
            else
            {
                //Every word has to appear somewhere in the name, in any order. Material names run
                //"Concrete, Cast In Situ, C32/40" and the user thinks "concrete c32": a single
                //substring finds that no better than a prefix does.
                string[] needles = typed.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                box.Items.Filter = delegate (object candidate)
                {
                    string name = candidate as string;

                    if (name == null)
                        return false;

                    foreach (string needle in needles)
                    {
                        if (name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;
                    }

                    return true;
                };
            }

            //Applying or dropping the filter can have dropped the current item out of the list,
            //at which point the combo box clears its selection and writes the new selection's
            //text - the empty string - over the editor. This is the well known "my typing
            //disappears" bug, and this is the line that undoes it.
            if (state.Editor.Text != typed)
                state.Editor.Text = typed;

            state.Editor.SelectionStart = caret > typed.Length ? typed.Length : caret;
            state.Editor.SelectionLength = 0;

            //Open the list only while the user is actually typing into it, and only when there is
            //something to show. An empty popup under the cursor is worse than none.
            if (narrow && state.Editor.IsKeyboardFocusWithin)
                box.IsDropDownOpen = box.Items.Count > 0;
        }

        /// <summary>
        /// The user has finished with this box: drop the filter, turn what they left in it into a
        /// material the database actually holds where that is possible, and write it to the row.
        /// </summary>
        private void SettleMaterialBox(System.Windows.Controls.ComboBox box, MaterialBoxState state)
        {
            if (state.Busy)
                return;

            state.Busy = true;

            try
            {
                string typed = box.Text == null ? "" : box.Text;
                int caret = state.Editor.SelectionStart;

                box.Items.Filter = null;
                box.IsDropDownOpen = false;

                string resolved = ResolveMaterialName(typed);

                if (resolved != null && resolved.Length > 0)
                {
                    //Snapped to the database's own spelling. A name pasted out of a spreadsheet or
                    //a schedule arrives with the wrong case or a trailing space often enough, and
                    //the lookup that uses it later is exact - "concrete c32/40 " maps to nothing.
                    typed = resolved;
                    box.SelectedItem = resolved;
                }
                else
                {
                    box.SelectedItem = null;
                }

                //Both branches above can have cleared the editor on their way past.
                if (box.Text != typed)
                    box.Text = typed;

                state.Editor.SelectionStart = caret > typed.Length ? typed.Length : caret;
                state.Editor.SelectionLength = 0;

                //Explicit, because the binding writes on lost focus and picking from the list does
                //not move the focus anywhere.
                CommitMaterialBox(box);
            }
            finally
            {
                state.Busy = false;
            }

            MarkMaterialBox(box);
        }

        /// <summary>
        /// Writes the box's text to the row behind it now, rather than when the focus leaves.
        /// </summary>
        private static void CommitMaterialBox(System.Windows.Controls.ComboBox box)
        {
            BindingExpression binding = BindingOperations.GetBindingExpression(
                box, System.Windows.Controls.ComboBox.TextProperty);

            if (binding != null)
                binding.UpdateSource();
        }

        /// <summary>
        /// Shows whether the box names a material this project's database holds. A name it does
        /// not hold is never rejected - the user may be half way through typing it, or pasting a
        /// column out of a spreadsheet - but it has to be visible, because a row like that is
        /// looked up with GetExcactMatch and then quietly does nothing at all.
        /// </summary>
        private void MarkMaterialBox(System.Windows.Controls.ComboBox box)
        {
            if (box == null)
                return;

            if (ResolveMaterialName(box.Text) != null)
            {
                box.ClearValue(System.Windows.Controls.ComboBox.BorderBrushProperty);
                box.ClearValue(System.Windows.Controls.ComboBox.BorderThicknessProperty);
                box.ToolTip = null;
            }
            else
            {
                box.BorderBrush = System.Windows.Media.Brushes.Firebrick;
                box.BorderThickness = new Thickness(2);
                box.ToolTip = "This project has no material called \"" + (box.Text == null ? "" : box.Text.Trim()) +
                              "\"." + Environment.NewLine +
                              "Pick one from the list, or this row will be ignored.";
            }
        }

        /// <summary>
        /// The database's own spelling of the material this text names, or null when the database
        /// holds no such material. Case and stray spaces are forgiven; empty text resolves to
        /// empty, because a row is allowed to map to nothing.
        /// </summary>
        private string ResolveMaterialName(string typed)
        {
            string needle = typed == null ? "" : typed.Trim();

            if (needle.Length == 0)
                return "";

            foreach (string name in materialNameList)
            {
                if (string.Equals(name, needle, StringComparison.OrdinalIgnoreCase))
                    return name;
            }

            return null;
        }

        /// <summary>
        /// Settles the box the user is standing in, if they are standing in one.
        /// </summary>
        private void SettleFocusedMaterialBox()
        {
            DependencyObject node = Keyboard.FocusedElement as DependencyObject;

            while (node != null)
            {
                System.Windows.Controls.ComboBox box = node as System.Windows.Controls.ComboBox;

                if (box != null)
                {
                    MaterialBoxState state = box.Tag as MaterialBoxState;

                    if (state != null)
                        SettleMaterialBox(box, state);

                    return;
                }

                node = node is System.Windows.Media.Visual || node is System.Windows.Media.Media3D.Visual3D
                    ? System.Windows.Media.VisualTreeHelper.GetParent(node)
                    : LogicalTreeHelper.GetParent(node);
            }
        }

        /// <summary>
        /// Says out loud which rows name a material this project does not have, and asks whether
        /// to carry on. Not a refusal: a map is allowed to carry rows for materials that exist in
        /// another template, and rows the user never touched can already be in that state. But a
        /// row like that changes nothing when the map is applied, and it used to do so in silence.
        /// </summary>
        /// <returns>False when the user chose to go back and fix them.</returns>
        private bool WarnAboutUnknownMaterials()
        {
            if (mappinglist == null)
                return true;

            List<string> unknown = new List<string>();

            foreach (CarboMapElement row in mappinglist)
            {
                if (row == null)
                    continue;

                if (ResolveMaterialName(row.carboNAME) != null)
                    continue;

                string name = row.carboNAME.Trim();

                if (unknown.Contains(name) == false)
                    unknown.Add(name);
            }

            if (unknown.Count == 0)
                return true;

            string names = string.Join(Environment.NewLine, unknown.Take(6).ToArray());

            if (unknown.Count > 6)
                names += Environment.NewLine + "...and " + (unknown.Count - 6) + " more.";

            string heading = unknown.Count == 1
                ? "One name in this map is not a material this project has:"
                : unknown.Count + " names in this map are not materials this project has:";

            MessageBoxResult answer = System.Windows.MessageBox.Show(
                heading + Environment.NewLine + Environment.NewLine +
                names + Environment.NewLine + Environment.NewLine +
                "The rows using them will be ignored. Carry on anyway?",
                "Unknown materials", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            return answer == MessageBoxResult.Yes;
        }

        private void MaterialBox_DropDownOpened(object sender, EventArgs e)
        {
            System.Windows.Controls.ComboBox box = sender as System.Windows.Controls.ComboBox;
            MaterialBoxState state = box == null ? null : box.Tag as MaterialBoxState;

            if (state == null || state.Busy)
                return;   //Opened by the filter, one keystroke at a time: leave the narrowed list.

            //Opened by the arrow, which is the user asking to browse: the whole list, scrolled to
            //whatever the row holds now rather than to the top of the alphabet.
            state.Busy = true;

            try
            {
                string typed = box.Text == null ? "" : box.Text;

                box.Items.Filter = null;

                string resolved = ResolveMaterialName(typed);

                if (resolved != null && resolved.Length > 0)
                {
                    typed = resolved;
                    box.SelectedItem = resolved;
                }

                if (box.Text != typed)
                    box.Text = typed;
            }
            finally
            {
                state.Busy = false;
            }
        }

        private void MaterialBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox box = sender as System.Windows.Controls.ComboBox;
            MaterialBoxState state = box == null ? null : box.Tag as MaterialBoxState;

            if (state == null)
                return;

            //This box has been handed to another row. Virtualization is set to Standard so it
            //should not happen, but a narrowed list belonging to the previous row would hide the
            //new row's own material, so it is dropped here rather than trusted not to arise.
            state.Busy = true;

            try
            {
                box.Items.Filter = null;
            }
            finally
            {
                state.Busy = false;
            }

            //After the text binding has caught up with the new row.
            box.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                                       new Action(delegate { MarkMaterialBox(box); }));
        }


    }
}
