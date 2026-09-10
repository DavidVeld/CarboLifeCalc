using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for MaterialImportDialog.xaml
    /// </summary>
    public partial class MaterialImportDialog : Window
    {
        internal bool isAccepted;


        public CarboDatabase currentDb = null;
        public CarboDatabase importedDb = null;

        public bool deleteMaterials { get; set; }

        public MaterialImportDialog()
        {
            InitializeComponent();
            chx_DeleteEmpty.DataContext = this;
        }

        /// <summary>
        /// this is the default constructor
        /// </summary>
        /// <param name="db">A Material Library</param>
        public MaterialImportDialog(CarboDatabase db)
        {
            InitializeComponent();
            currentDb = db.Copy();
            importedDb = currentDb.Copy();

            dgv_Preview.ItemsSource = importedDb.CarboMaterialList;
            chx_DeleteEmpty.DataContext = this;
        }

        private void btn_SelectFile_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Select a csv to import");
            string openPath = DataExportUtils.GetOpenCSVLocation();

            if (openPath == null || openPath == "")
                return;

            int rowsRead;
            int rowsSkipped;
            List<CarboMaterial> materials =
                DataExportUtils.GetMaterialDatabaseFromCVSFile(openPath, out rowsRead, out rowsSkipped);

            //Say what happened. This used to assign the result straight into the preview, so a
            //file that yielded nothing looked exactly like a file holding no materials, and the
            //grid simply went blank.
            if (materials == null || materials.Count == 0)
            {
                string reason = rowsSkipped > 0
                    ? "None of its " + rowsSkipped + " rows could be read. The columns have to be in the "
                      + "order of the exported template, and the file has to be comma or semicolon separated."
                    : "The file holds no material rows.";

                MessageBox.Show("No materials were read from:" + Environment.NewLine
                    + System.IO.Path.GetFileName(openPath) + Environment.NewLine + Environment.NewLine
                    + reason + Environment.NewLine + Environment.NewLine
                    + "The list below has been left as it was.",
                    "Nothing imported", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            if (rowsSkipped > 0)
            {
                MessageBox.Show(rowsRead + " material(s) were read and " + rowsSkipped
                    + " row(s) were skipped because they could not be read." + Environment.NewLine
                    + "Check the list below before accepting.",
                    "Partly imported", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            importedDb.CarboMaterialList = materials;
            dgv_Preview.ItemsSource = importedDb.CarboMaterialList;
        }

        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The current material database will now be exported to use as a template for import.");
            if (currentDb != null && currentDb.CarboMaterialList.Count > 0)
            {
                string exportPath = DataExportUtils.GetSaveAsLocation();

                if (exportPath != null && exportPath != "")
                {
                    //currentDb, the database this dialog was opened on, which is what the
                    //message above promises. It used to write importedDb: the same thing until
                    //a csv has been selected, and after that the imported rows rather than the
                    //current library, so exporting a template a second time handed back the
                    //import instead of the database.
                    bool ok = DataExportUtils.CreateMaterialDatabaseCVSFile(currentDb, exportPath);

                    if (ok)
                    {
                        MessageBox.Show("CSV file created. "
                            + Environment.NewLine +
                            "Edit the file or use as a template for a new import. " + Environment.NewLine +
                            "Do not change the column order." + Environment.NewLine +
                            "Import the file back after changes");

                        string exportDir = System.IO.Path.GetDirectoryName(exportPath);
                        //Quoted: an unquoted path with a space in it sends explorer somewhere else.
                        System.Diagnostics.Process.Start("explorer.exe", "\"" + exportDir + "\"");

                    }
                }
            }

        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            int importing = importedDb == null || importedDb.CarboMaterialList == null
                ? 0
                : importedDb.CarboMaterialList.Count;

            if (importing == 0)
            {
                MessageBox.Show("There is nothing to import. Select a csv file first.",
                    "Nothing to import", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //"Delete Materials Not in List" replaces the library with the list, so the moment
            //the list is shorter than the library it is a deletion, and it is written back over
            //the user's material file when the editor is accepted. Say how many rows go, and
            //let the user stop.
            int current = currentDb == null || currentDb.CarboMaterialList == null
                ? 0
                : currentDb.CarboMaterialList.Count;

            if (deleteMaterials == true && importing < current)
            {
                MessageBoxResult answer = MessageBox.Show(
                    "\"Delete Materials Not in List\" is ticked, so the library will be replaced "
                    + "by the " + importing + " material(s) in this list." + Environment.NewLine + Environment.NewLine
                    + "That removes " + (current - importing) + " of the " + current
                    + " material(s) currently in it." + Environment.NewLine + Environment.NewLine
                    + "Continue?",
                    "Confirm replacing the material library", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (answer != MessageBoxResult.Yes)
                    return;
            }

            isAccepted = true;
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
