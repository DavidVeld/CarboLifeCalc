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

        public A1A3Element a1a3ElementSelected;

        public MaterialA1A3Picker(A1A3Element a1a3Element)
        {
            isAccepted = false;
            this.a1a3ElementSelected = a1a3Element;

            a1a3Collection = new A1A3Collection();
            if(a1a3Collection != null)
                a1a3Collection.LoadAll();

            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> groupNames = a1a3Collection.GetGroupNames();
            List<string> categorynames = a1a3Collection.GetCategoryList(groupNames[0]);

            //Check if a element leeds to be loaded, if not show blank screen

            foreach (string grp in groupNames)
            {
                cbb_Group.Items.Add(grp);
            }

            if (a1a3ElementSelected != null)
            {
                a1a3ElementSelected = new A1A3Element();
            }

            if (a1a3ElementSelected.Group != null)
            {
                cbb_Group.Text = a1a3ElementSelected.Group;
            }
            else
            {
                cbb_Group.Text = groupNames[0];
            }

            if (a1a3ElementSelected.Category != null)
            {
                cbb_Categories.Text = a1a3ElementSelected.Category;
            }
            else
            {
                cbb_Categories.Text = "ALL";
            }

            //lib_Materials.SelectedItem = a1a3ElementSelected;
        }


        private void lib_Materials_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            A1A3Element a1a3Element = lib_Materials.SelectedItem as A1A3Element;
            if (a1a3Element != null)
            {
                a1a3ElementSelected = a1a3Element;
                //txt_Search.Text = a1a3Element.Name;
                refreshInterface();
            }
        }

        private async void Txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                loadPossibleElements();
            }
        }

        private void loadPossibleElements()
        {
            lib_Materials.ItemsSource = null;
            lib_Materials.Items.Clear();

            foreach (A1A3List list in a1a3Collection.a1a3List)
            {

                if (cbb_Group.Text == list.Name && cbb_Group.Text != "")
                {
                    foreach (A1A3Element a1a3Element in list.Elements)
                    {
                        if (a1a3Element.Category == cbb_Categories.Text ||
                            cbb_Categories.Text == "" ||
                            cbb_Categories.Text == "All")
                        {
                            bool contains = a1a3Element.Name.IndexOf(txt_Search.Text, StringComparison.OrdinalIgnoreCase) >= 0;

                            if (contains == true ||
                                txt_Search.Text == "")
                            {
                                lib_Materials.Items.Add(a1a3Element);
                            }
                        }
                    }

                    break;
                }
            }
        }

        private void refreshInterface()
        {
            txt_Name.Text = a1a3ElementSelected.Name;
            txt_Description.Text = a1a3ElementSelected.Description;
            txt_Category.Text = a1a3ElementSelected.Category;
            txt_Density.Text = a1a3ElementSelected.Density.ToString();
            txt_A1A3.Text = a1a3ElementSelected.ECI_A1A3.ToString();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
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
            string selectedGroup = cbb_Group.Text;

            List<string> categoryList = a1a3Collection.GetCategoryList(selectedGroup);

            cbb_Categories.Items.Clear();

            foreach(string cat in categoryList)
            {
                cbb_Categories.Items.Add(cat);
            }

            cbb_Categories.Items.Add("All");

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
