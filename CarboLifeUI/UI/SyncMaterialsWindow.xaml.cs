using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            isAccepted = false;
            sourcePath = "";

            //CarboProject templateProject = new CarboProject();
            //templateDatabase = templateProject.CarboDatabase;

            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            UpdateCombobox();
            cbb_DataBases.Text = "Template";
            setPath();

            refreshProjectMaterials();
            loadTemplateMaterials();

           // UpdateCombobox();
        }

        private void UpdateCombobox()
        {
            //CLS
            cbb_DataBases.ItemsSource = null;
            cbb_DataBases.Items.Clear();

            //Get all file sin online folder:
            string onlinePath = PathUtils.getDownloadedPath();
            string[] files = Directory.GetFiles(onlinePath);

            cbb_DataBases.Items.Add("Template");
            foreach(string file in files)
                cbb_DataBases.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));

         }

        private void loadTemplateMaterials()
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    templateDatabase = new CarboDatabase();
                    CarboDatabase buffer = new CarboDatabase();
                    templateDatabase = buffer.DeSerializeXML(sourcePath).Copy();
                }
                refreshTemplateMaterials();
            }
            catch
            {

            }
        }

        private void refreshTemplateMaterials()
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

        private void refreshProjectMaterials()
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

            refreshTemplateMaterials();
            refreshProjectMaterials();
        }

        private void btn_OpenFrom_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            try
            { 
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Carbo Material Data Files (*.cxml)|*.cxml";

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

                    cbb_DataBases.Items.Add(name);
                    cbb_DataBases.Text = name;
                    setPath();
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
            refreshTemplateMaterials();
            //txt_path.Text = sourcePath;
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

            refreshTemplateMaterials();
            refreshProjectMaterials();
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
                bool deletedSucces = false;
            int count = 0;

            if (liv_TemplateMaterials.SelectedItems.Count == 1)
            {
                try
                {
                    CarboMaterial cm = liv_TemplateMaterials.SelectedItems[0] as CarboMaterial;

                    if (cm != null)
                    {
                        deletedSucces = templateDatabase.deleteMaterial(cm.Name);

                        if (deletedSucces == true)
                            count++;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString()); 
                    deletedSucces = false;
                }
            }
            else
            {
                for(int i = liv_TemplateMaterials.SelectedItems.Count; i > 0; i--)
                {
                    try
                    {
                        CarboMaterial cm = liv_TemplateMaterials.SelectedItems[i-1] as CarboMaterial;
                        deletedSucces = templateDatabase.deleteMaterial(cm.Name);

                        if (deletedSucces == true)
                            count++;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.ToString());
                        deletedSucces = false;
                    }
                }


               // MessageBoxResult result = System.Windows.MessageBox.Show("Please select one material to delete", "Computer says no", MessageBoxButton.YesNo);
            }
            if (count == 1)
            {
                System.Windows.MessageBox.Show(count + " element deleted.", "Success", MessageBoxButton.OK);
            }
            else if(count > 1)
            {
                System.Windows.MessageBox.Show(count + " elements deleted.", "Success", MessageBoxButton.OK);
            }
            else
            {
                System.Windows.MessageBox.Show(0 + "elements deleted.", "Not a success", MessageBoxButton.OK);
            }

            refreshTemplateMaterials();
            refreshProjectMaterials();
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

            refreshTemplateMaterials();
            refreshProjectMaterials();
        }

        private void btn_OpenOnline_Click(object sender, RoutedEventArgs e)
        {
            OnlineMaterialPicker onlinePicker = new OnlineMaterialPicker();
            onlinePicker.ShowDialog();
            if (onlinePicker.isAccepted == true && onlinePicker.selectionList != null && onlinePicker.selectionList.Count > 0)
            {

                //Download location
                foreach (string file in onlinePicker.selectionList)
                {
                    //download each selected item
                    string filetarget = PathUtils.getDownloadedPath() + file;
                    string filename = System.IO.Path.GetFileNameWithoutExtension(filetarget);
                    try
                    {
                        if (File.Exists(filetarget))
                            File.Delete(filetarget);

                        using (WebClient wc = new WebClient())
                        {
                            //request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
                            wc.Headers.Add("user-agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36");
                            
                            //wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                            wc.DownloadFileAsync(
                                // Param1 = Link of file
                                new System.Uri("https://davidveld.nl/data/" + file),
                                // Param2 = Path to save
                                filetarget
                            );
                            //lastdownloadedfile = System.IO.Path.GetFileNameWithoutExtension(filetarget);
                            wc.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Was not able to download file!" + Environment.NewLine + ex.Message);
                    }
                }

                UpdateCombobox();

            }
        }
        private void cbb_DataBases_DropDownClosed(object sender, EventArgs e)
        {
            setPath();
            loadTemplateMaterials();
        }

        private void setPath()
        {
            string selectedItem = cbb_DataBases.Text;
            sourcePath = "";

            if (File.Exists(selectedItem))
            {
                //This is a project file
                sourcePath = selectedItem;
            }
            else if(selectedItem == "Template")
            {
                // This is the template use build-in path:
                //sourcePath = Utils.getAssemblyPath() + "\\db\\UserMaterials.cxml";
                sourcePath = PathUtils.getTemplateFolder();
            }
            else
            {
                //this is an online material:
                sourcePath = PathUtils.getDownloadedPath() + selectedItem + ".cxml";
            }

            if(!(File.Exists(sourcePath)))
            {
                System.Windows.MessageBox.Show("There was an error loading file: " + Environment.NewLine + sourcePath);
            }
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if(File.Exists(sourcePath))
                templateDatabase.SerializeXML(sourcePath);
            else
                System.Windows.MessageBox.Show("There was an error saving to file: " + Environment.NewLine + sourcePath);
        }

        private void btn_ReplaceProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to replace all materials in the PROJECT with template materials?", "Warning", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    CarboDatabase buffer = new CarboDatabase();

                    buffer = templateDatabase.Copy();

                    if (buffer.CarboMaterialList.Count == templateDatabase.CarboMaterialList.Count)
                    {
                        projectDatabase.CarboMaterialList.Clear();
                        projectDatabase.CarboMaterialList = new List<CarboMaterial>();
                        projectDatabase = buffer;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

            refreshTemplateMaterials();
            refreshProjectMaterials();
        }

        private void btn_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Carbo Material Data Files (*.cxml)|*.cxml";

            var path = saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                templateDatabase.SerializeXML(saveFileDialog.FileName);
                cbb_DataBases.Items.Add(saveFileDialog.FileName);
                cbb_DataBases.Text = saveFileDialog.FileName;
                setPath();
            }
        }
    }
}
