using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
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
    public partial class SyncMaterialsWindow : Window
    {
        internal bool isAccepted;
        public string sourcePath;
        public CarboDatabase projectDatabase;
        private CarboDatabase templateDatabase;

        public SyncMaterialsWindow(CarboDatabase returnedDatabase)
        {
            this.projectDatabase = returnedDatabase;
            CarboProject templateProject = new CarboProject();
            sourcePath = "";
            templateDatabase = templateProject.CarboDatabase;

            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadProjectMaterials();
            loadTemplateMaterials();
        }

        private void loadTemplateMaterials()
        {
            if (templateDatabase != null)
            {
                liv_TemplateMaterials.Items.Clear();

                foreach (CarboMaterial cm in templateDatabase.CarboMaterialList)
                {
                    liv_TemplateMaterials.Items.Add(cm);
                }

                //Sort list
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(liv_TemplateMaterials.Items);

                if (view != null)
                {
                    view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Category", System.ComponentModel.ListSortDirection.Ascending));
                }
            }
        }

        private void loadProjectMaterials()
        {
            if (projectDatabase != null)
            {
                liv_CurrentMaterials.Items.Clear();

                foreach (CarboMaterial cm in projectDatabase.CarboMaterialList)
                {
                    liv_CurrentMaterials.Items.Add(cm);

                }

                //Sort list
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(liv_CurrentMaterials.Items);

                if (view != null)
                {
                    view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Category", System.ComponentModel.ListSortDirection.Ascending));
                }
            }
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;

            templateDatabase.SerializeXML(sourcePath);

            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void btn_SyncTo_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            if (liv_CurrentMaterials.SelectedItems.Count > 0)
            {
                try
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to update the material template with the current project materials?" + Environment.NewLine +
                        "Materials with excact same names will be overwritten with new values, other will be added to the template file", "Warning", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        CarboDatabase buffer = new CarboDatabase();

                        foreach (object item in liv_CurrentMaterials.SelectedItems)
                        {
                            CarboMaterial cm = item as CarboMaterial;
                            if (cm != null)
                                buffer.AddMaterial(cm);
                        }
                        templateDatabase.SyncMaterials(buffer);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Please select a material to syncronise", "Computer says no", MessageBoxButton.YesNo);
            }

            loadTemplateMaterials();
            loadProjectMaterials();
        }

        private void btn_OpenFrom_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to open a project to load or save materials to / from ?", "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";

                    var path = openFileDialog.ShowDialog();
                    if (openFileDialog.FileName != "")
                    {
                        FileInfo finfo = new FileInfo(openFileDialog.FileName);

                        if (openFileDialog.FileName != "")
                        {
                            name = openFileDialog.FileName;
                            CarboDatabase userprojects = new CarboDatabase();

                            CarboDatabase buffer = userprojects.DeSerializeXML(name);

                            if (buffer != null)
                            {
                                templateDatabase = buffer;
                            }
                        }
                        sourcePath = name;
                    }
                    else
                    {
                        //reload the template
                        sourcePath = "";
                        CarboDatabase userprojects = new CarboDatabase();
                        CarboDatabase buffer = userprojects.DeSerializeXML(sourcePath);
                        if (buffer != null)
                        {
                            templateDatabase = buffer;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
            loadTemplateMaterials();
            txt_path.Text = sourcePath;
        }

        private void btn_SyncFrom_Click(object sender, RoutedEventArgs e)
        {
            if (liv_TemplateMaterials.SelectedItems.Count > 0)
            {
                try
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to update the project materials  with the selected materials from the template?" + Environment.NewLine +
                        "Materials with excact same name will be overwritten, others will be added to the project", "Warning", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        CarboDatabase buffer = new CarboDatabase();

                        foreach (object item in liv_TemplateMaterials.SelectedItems)
                        {
                            CarboMaterial cm = item as CarboMaterial;
                            if(cm != null)
                                buffer.AddMaterial(cm);
                        }
                        projectDatabase.SyncMaterials(buffer);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Please select a material to syncronise", "Computer says no", MessageBoxButton.YesNo);
            }

            loadTemplateMaterials();
            loadProjectMaterials();
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (liv_TemplateMaterials.SelectedItems.Count == 1)
            {
                try
                {
                    CarboMaterial cm = liv_TemplateMaterials.SelectedItems[0] as CarboMaterial;

                    if (cm != null)
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to delete" + cm.Name +  " from the template?", "Warning", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            templateDatabase.deleteMaterial(cm.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Please select one material to delete", "Computer says no", MessageBoxButton.YesNo);
            }

            loadTemplateMaterials();
            loadProjectMaterials();
        }

        private void btn_Replace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to replace all materials in the template with project materials?", "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    CarboDatabase buffer = new CarboDatabase();

                    buffer = projectDatabase.Copy();

                    if(buffer.CarboMaterialList.Count == projectDatabase.CarboMaterialList.Count)
                    {
                        templateDatabase.CarboMaterialList.Clear();
                        templateDatabase.CarboMaterialList = new List<CarboMaterial>();
                        templateDatabase = buffer;
                    }    
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

            loadTemplateMaterials();
            loadProjectMaterials();
        }
    }
}
