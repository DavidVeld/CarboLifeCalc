using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
    /// Interaction logic for MaterialBasePicker.xaml
    /// </summary>
    public partial class MaterialA1A3Picker : Window
    {        
        public bool isAccepted;
        private A1A3Collection a1a3Collection;

        /// <summary>
        /// The material's current A1-A3 source on the way in, the chosen table row on the way
        /// out. Only replaced by a row the user picked, never by a blank.
        /// </summary>
        public A1A3Element a1a3ElementSelected;

        /// <summary>The selected row in the results, null until one is picked.</summary>
        private A1A3Element chosen;

        private const string AllCategories = "All";

        /// <summary>Set while the lists are filled from code, so their events stay quiet.</summary>
        private bool loading;

        public MaterialA1A3Picker(A1A3Element a1a3Element)
        {
            isAccepted = false;
            this.a1a3ElementSelected = a1a3Element;

            //LoadAll does not throw: what it could not read is in LoadErrors.
            a1a3Collection = new A1A3Collection();
            a1a3Collection.LoadAll();

            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> groupNames = a1a3Collection.GetGroupNames();

            if (a1a3Collection.LoadErrors.Count > 0)
            {
                MessageBox.Show("Some A1-A3 data could not be loaded:" + Environment.NewLine + Environment.NewLine +
                    string.Join(Environment.NewLine, a1a3Collection.LoadErrors), "A1-A3 tables", MessageBoxButton.OK);
            }

            refreshInterface();

            if (groupNames.Count == 0)
            {
                MessageBox.Show("No A1-A3 tables were found, so there is nothing to choose from.", "A1-A3 tables", MessageBoxButton.OK);
                return;
            }

            loading = true;

            foreach (string grp in groupNames)
            {
                cbb_Group.Items.Add(grp);
            }

            //Open where the material's current source is, when it is still in the tables.
            A1A3Element current = findCurrent();

            string group = current != null ? current.Group : groupNames[0];
            cbb_Group.SelectedItem = group;
            loadCategories(group);
            cbb_Categories.SelectedItem = current != null ? current.Category : AllCategories;

            loading = false;

            loadPossibleElements();

            if (current != null)
            {
                lib_Materials.SelectedItem = current;
                lib_Materials.ScrollIntoView(current);
            }
        }

        /// <summary>The table row the material's current source came from, or null.</summary>
        private A1A3Element findCurrent()
        {
            if (a1a3ElementSelected == null || string.IsNullOrWhiteSpace(a1a3ElementSelected.Name))
                return null;

            string name = a1a3ElementSelected.Name.Trim();

            foreach (A1A3List list in a1a3Collection.a1a3List)
            {
                if (a1a3ElementSelected.Group != null && list.Name != a1a3ElementSelected.Group)
                    continue;

                foreach (A1A3Element element in list.Elements)
                {
                    if (element.Name == name)
                        return element;
                }
            }

            return null;
        }

        private void loadCategories(string group)
        {
            cbb_Categories.Items.Clear();

            foreach (string cat in a1a3Collection.GetCategoryList(group))
            {
                cbb_Categories.Items.Add(cat);
            }

            cbb_Categories.Items.Add(AllCategories);
            cbb_Categories.SelectedItem = AllCategories;
        }


        private void lib_Materials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Only a row the user can see counts: a refresh that clears the list clears the choice.
            chosen = lib_Materials.SelectedItem as A1A3Element;
            refreshInterface();
        }

        /// <summary>A search box filters as you type, through a real debounce. See MaterialEditor.</summary>
        private readonly UiDebouncer searchDebouncer = new UiDebouncer(250);

        private void Txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchDebouncer.Poke(loadPossibleElements);
        }

        private void loadPossibleElements()
        {
            if (loading)
                return;

            A1A3Element keep = chosen;

            lib_Materials.ItemsSource = null;
            lib_Materials.Items.Clear();

            string group = cbb_Group.SelectedItem as string ?? "";
            string category = cbb_Categories.SelectedItem as string ?? AllCategories;
            string search = (txt_Search.Text ?? "").Trim();

            foreach (A1A3List list in a1a3Collection.a1a3List)
            {
                if (list.Name != group)
                    continue;

                foreach (A1A3Element a1a3Element in list.Elements)
                {
                    if (category != AllCategories && a1a3Element.Category != category)
                        continue;

                    if (Matches(a1a3Element, search))
                        lib_Materials.Items.Add(a1a3Element);
                }

                break;
            }

            //Still in the list after a new search: keep it selected.
            if (keep != null && lib_Materials.Items.Contains(keep))
                lib_Materials.SelectedItem = keep;
        }

        /// <summary>The search text in the name, category or description, ignoring case.</summary>
        private static bool Matches(A1A3Element element, string search)
        {
            if (search == "")
                return true;

            return Contains(element.Name, search) || Contains(element.Category, search) || Contains(element.Description, search);
        }

        private static bool Contains(string text, string search)
        {
            return text != null && text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void refreshInterface()
        {
            //The chosen row, or the material's current source until one is chosen.
            A1A3Element shown = chosen ?? a1a3ElementSelected;

            txt_Name.Text = shown?.Name ?? "";
            txt_Description.Text = shown?.Description ?? "";
            txt_Category.Text = shown?.Category ?? "";
            txt_Density.Text = shown == null ? "" : (shown.Density > 0 ? shown.Density.ToString() : "not given");
            txt_A1A3.Text = shown?.ECI_A1A3.ToString() ?? "";

            btn_Accept.IsEnabled = chosen != null;
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            //Accept is only enabled with a row chosen, this is the guard behind it.
            if (chosen == null)
                return;

            a1a3ElementSelected = chosen;

            isAccepted = true;
            this.Close();
        }
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void Cbb_Categories_DropDownClosed(object sender, EventArgs e)
        {
            loadPossibleElements();
        }

        private void cbb_Group_DropDownClosed(object sender, EventArgs e)
        {
            string selectedGroup = cbb_Group.SelectedItem as string;

            if (selectedGroup == null)
                return;

            loadCategories(selectedGroup);
            loadPossibleElements();
        }

        private void btn_EditDescription_Click(object sender, RoutedEventArgs e)
        {
            DescriptionEditor editor = new DescriptionEditor(txt_Description.Text);
            editor.ShowDialog();
            if (editor.isAccepted == true)
                txt_Description.Text = editor.description;
        }
    }
}
