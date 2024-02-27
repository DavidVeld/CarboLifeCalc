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

            if (openPath != null && openPath != "")
            {
                importedDb.CarboMaterialList = DataExportUtils.GetMaterialDatabaseFromCVSFile(openPath);
                dgv_Preview.ItemsSource = importedDb.CarboMaterialList;
            }
        }

        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The current material database will now be exported to use as a template for import.");
            if (currentDb != null && currentDb.CarboMaterialList.Count > 0)
            {
                string exportPath = DataExportUtils.GetSaveAsLocation();

                if (exportPath != null && exportPath != "")
                {
                    bool ok = DataExportUtils.CreateMaterialDatabaseCVSFile(importedDb, exportPath);

                    if (ok)
                    {
                        MessageBox.Show("CSV file created. "
                            + Environment.NewLine +
                            "Edit the file or use as a template for a new import. " + Environment.NewLine +
                            "Do not change the column order." + Environment.NewLine +
                            "Import the file back after changes");

                        string exportDir = System.IO.Path.GetDirectoryName(exportPath);
                        System.Diagnostics.Process.Start("explorer.exe", exportDir); 

                    }
                }
            }

        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
