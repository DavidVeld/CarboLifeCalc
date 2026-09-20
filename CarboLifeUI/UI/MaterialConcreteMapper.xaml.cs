using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using CarboLifeAPI.Data.Superseded;
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
    /// Interaction logic for MaterialConcreteMapper.xaml
    /// </summary>
    public partial class MaterialConcreteMapper : Window
    {
        internal bool isAccepted;
        public string sourcePath;


        public List<CarboNumProperty> rcMap { get; set; }
        public string categoryType { get; set; }
        public string categoryName { get; set; }

        /// <summary>
        /// The material the reinforcement is made of, and the material category of the groups that
        /// get it. Both are empty in a project nobody set them for, and the generator then builds
        /// nothing at all, so they are asked for here when the mapper is opened on a project.
        /// Null when it was opened without one, the import settings dialog owns them there.
        /// </summary>
        public string materialName { get; set; }
        public string materialCategory { get; set; }

        /// <summary>The project the mapper was opened on, null when opened from the import settings.</summary>
        private readonly CarboProject project;

        /// <summary>The number of groups per material category, for the warning under the boxes.</summary>
        private readonly Dictionary<string, int> categoryCounts = new Dictionary<string, int>();

        /// <summary>False while the lists are filled, so the warning is not run half built.</summary>
        private bool uiReady;

        /// <param name="carboSettings">The settings the mapping and the materials are read from</param>
        /// <param name="carboProject">The project being reinforced. Passing it adds the material and
        /// category boxes, which are otherwise only reachable through the import settings dialog.</param>
        public MaterialConcreteMapper(CarboGroupSettings carboSettings, CarboProject carboProject = null)
        {
            project = carboProject;

            materialName = carboSettings.RCMaterialName;
            materialCategory = carboSettings.RCMaterialCategory;

            this.InitializeComponent();

            cbb_RCImportType.Items.Clear();
            cbb_RCImportType.Items.Add("Type Parameter");
            cbb_RCImportType.Items.Add("Instance Parameter");

            rcMap = carboSettings.rcQuantityMap;
            categoryType = carboSettings.RCParameterType;
            categoryName = carboSettings.RCParameterName;

            if (rcMap.Count == 0)
            {
                System.Windows.MessageBox.Show("No RC properties found in the project, default mapping table will be used.", "Warning", MessageBoxButton.OK);

                CarboSettings settings = new CarboSettings();
                settings = settings.Load();

                rcMap = settings.defaultCarboGroupSettings.rcQuantityMap;
                categoryType = settings.defaultCarboGroupSettings.RCParameterType;
                categoryName = settings.defaultCarboGroupSettings.RCParameterName;
            }

                DataContext = this;

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbb_RCImportType.SelectedItem = categoryType;

            CarboParameterPicker.Attach(cbb_RCImportValue, categoryName,
                                        RCParameterKind(), CheckRCParameter);

            CheckRCParameter();

            FillMaterialBoxes();
        }

        /// <summary>
        /// Where the import looks the density override up, read the way getParametervalue reads
        /// it. SelectedItem rather than Text: inside SelectionChanged, Text is still the old value.
        /// </summary>
        private CarboNameKind RCParameterKind()
        {
            string type = cbb_RCImportType.SelectedItem as string;

            if (string.IsNullOrEmpty(type))
                type = cbb_RCImportType.Text;

            return type != null && type.ToLower().Contains("type")
                ? CarboNameKind.TypeParameter
                : CarboNameKind.InstanceParameter;
        }

        private void cbb_RCImportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //The list is attached in Window_Loaded; before that there is nothing to re-point.
            if (cbb_RCImportValue == null)
                return;

            CarboParameterPicker.Repoint(cbb_RCImportValue, RCParameterKind());

            CheckRCParameter();
        }

        /// <summary>
        /// Marks the density override when it names a parameter this model does not carry. Worth
        /// saying but not alarming: without the parameter the mapping table below simply applies
        /// everywhere, which is what it is for. Silence would leave a per-element override that
        /// was typed, saved and never once read.
        /// </summary>
        private void CheckRCParameter()
        {
            string value = CarboParameterPicker.ValueOf(cbb_RCImportValue);

            if (CarboModelNames.Contains(RCParameterKind(), value) == true)
            {
                cbb_RCImportValue.BorderBrush =
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0x97, 0x97, 0x97));
                cbb_RCImportValue.BorderThickness = new Thickness(1);

                txt_RCParamWarning.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            cbb_RCImportValue.BorderBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xC0, 0x50, 0x00));
            cbb_RCImportValue.BorderThickness = new Thickness(2);

            txt_RCParamWarning.Text =
                "This model has no " + CarboModelNames.DescriptionOf(RCParameterKind()) +
                " called \"" + value + "\", so the table below applies to every group.";

            txt_RCParamWarning.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Shows and fills the material and category boxes, when the mapper was opened on a project.
        /// The categories offered are the ones the project's own groups use: a category no group
        /// uses can only ever reinforce nothing, which is the outcome this is here to prevent.
        /// </summary>
        private void FillMaterialBoxes()
        {
            if (project == null)
                return;

            List<string> materialNames = new List<string>();

            if (project.CarboDatabase != null && project.CarboDatabase.CarboMaterialList != null)
            {
                foreach (CarboMaterial cm in project.CarboDatabase.CarboMaterialList)
                {
                    if (cm != null && string.IsNullOrEmpty(cm.Name) == false)
                        materialNames.Add(cm.Name);
                }
            }

            categoryCounts.Clear();

            if (project.getGroupList != null)
            {
                foreach (CarboGroup grp in project.getGroupList)
                {
                    //A generated group is an allowance itself and never gets one of its own.
                    if (grp == null || grp.IsAutoGenerated() == true)
                        continue;

                    if (grp.Material == null || string.IsNullOrEmpty(grp.Material.Category))
                        continue;

                    if (categoryCounts.ContainsKey(grp.Material.Category) == false)
                        categoryCounts.Add(grp.Material.Category, 0);

                    categoryCounts[grp.Material.Category] = categoryCounts[grp.Material.Category] + 1;
                }
            }

            //Nothing to choose from either way, so the boxes would only be in the way.
            if (materialNames.Count == 0 || categoryCounts.Count == 0)
                return;

            materialNames.Sort();

            List<string> categories = new List<string>(categoryCounts.Keys);
            categories.Sort();

            foreach (string name in materialNames)
                cbb_RCMaterial.Items.Add(name);

            foreach (string category in categories)
                cbb_RCMaterialCategory.Items.Add(category);

            cbb_RCMaterial.SelectedItem = FindExact(materialNames, materialName);

            if (cbb_RCMaterial.SelectedItem == null)
                cbb_RCMaterial.SelectedItem = FindByKeyword(materialNames, new string[] { "reinforcement", "rebar", "steel" });

            cbb_RCMaterialCategory.SelectedItem = FindExact(categories, materialCategory);

            if (cbb_RCMaterialCategory.SelectedItem == null)
                cbb_RCMaterialCategory.SelectedItem = FindByKeyword(categories, new string[] { "concrete" });

            txt_Note.Text = "The parameter below defines a local override to reinforcement density (kg/m³).";
            pnl_Materials.Visibility = System.Windows.Visibility.Visible;

            //The two extra rows need the room, the mapping table keeps the height it had.
            this.MinHeight = 660;
            this.Height = 660;

            uiReady = true;
            RefreshWarning();
        }

        /// <summary>
        /// Says what would stop these choices from reinforcing anything, while it can still be changed.
        /// </summary>
        private void RefreshWarning()
        {
            if (uiReady == false)
                return;

            string category = cbb_RCMaterialCategory.SelectedItem as string;
            string material = cbb_RCMaterial.SelectedItem as string;
            string message = "";

            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(material))
            {
                message = "Pick both a concrete material category and a reinforcement material.";
            }
            else if (categoryCounts.ContainsKey(category) == false || categoryCounts[category] == 0)
            {
                message = "No groups use a material in this category, so nothing would be reinforced.";
            }
            else
            {
                CarboMaterial selected = project.CarboDatabase == null ? null : project.CarboDatabase.GetExcactMatch(material);

                //The reinforcement quantities are kg per m³ and have to be turned into a volume of
                //this material, which divides by its density. See CarboProject.CreateReinforcementGroup.
                if (selected != null && selected.Density <= 0)
                    message = "\"" + selected.Name + "\" has no density, so reinforcement quantities cannot be " +
                              "converted into a volume. Pick another material, or give this one a density in the database.";
            }

            txt_Warning.Text = message;
            txt_Warning.Visibility = message == "" ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// The entry that matches the wanted value, ignoring case and surrounding spaces.
        /// </summary>
        private static string FindExact(IList<string> candidates, string wanted)
        {
            if (string.IsNullOrWhiteSpace(wanted) || candidates == null)
                return null;

            foreach (string candidate in candidates)
            {
                if (candidate != null &&
                    string.Equals(candidate.Trim(), wanted.Trim(), StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }

            return null;
        }

        /// <summary>
        /// The first entry containing one of the keywords, the keywords in order of preference.
        /// This only decides what the boxes open on, the user still chooses.
        /// </summary>
        private static string FindByKeyword(IList<string> candidates, string[] keywords)
        {
            if (candidates == null)
                return null;

            foreach (string keyword in keywords)
            {
                foreach (string candidate in candidates)
                {
                    if (candidate != null && candidate.ToLower().Contains(keyword))
                        return candidate;
                }
            }

            return null;
        }

        private void Cbb_Material_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshWarning();
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            if (pnl_Materials.Visibility == System.Windows.Visibility.Visible)
            {
                string category = cbb_RCMaterialCategory.SelectedItem as string;
                string material = cbb_RCMaterial.SelectedItem as string;

                if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(material))
                {
                    System.Windows.MessageBox.Show(
                        "Pick both a concrete material category and a reinforcement material.",
                        this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (categoryCounts.ContainsKey(category) == false || categoryCounts[category] == 0)
                {
                    System.Windows.MessageBox.Show(
                        "No groups use a material in category \"" + category + "\", so nothing would be reinforced.",
                        this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                materialName = material;
                materialCategory = category;
            }

            isAccepted = true;

            categoryName = CarboParameterPicker.ValueOf(cbb_RCImportValue);
            categoryType = cbb_RCImportType.Text;

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

 
    }
}
