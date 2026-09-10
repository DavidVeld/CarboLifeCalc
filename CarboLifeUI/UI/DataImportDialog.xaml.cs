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
    public partial class DataImportDialog : Window
    {
        internal bool isAccepted;

        public List<CarboElement> elementList = null;

        public bool deleteMaterials { get; set; }

        public DataImportDialog()
        {
            InitializeComponent();
            elementList = new List<CarboElement>();
        }


        private void btn_SelectFile_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Select a csv to import");
            string openPath = DataExportUtils.GetOpenCSVLocation();

            if (openPath == null || openPath == "")
                return;

            int rowsRead;
            int rowsSkipped;
            List<CarboElement> imported =
                DataExportUtils.GetElementsFromCVSFile(openPath, out rowsRead, out rowsSkipped);

            //Say what happened. The result used to go straight into the preview, so a file that
            //yielded nothing, or half of what it held, looked exactly like a short file.
            if (imported == null || imported.Count == 0)
            {
                string reason = rowsSkipped > 0
                    ? "None of its " + rowsSkipped + " rows could be read. The file needs the column "
                      + "headers of the exported Elements csv, or of the template this dialog writes."
                    : "The file holds no element rows.";

                MessageBox.Show("No elements were read from:" + Environment.NewLine
                    + System.IO.Path.GetFileName(openPath) + Environment.NewLine + Environment.NewLine
                    + reason + Environment.NewLine + Environment.NewLine
                    + "The list below has been left as it was.",
                    "Nothing imported", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            if (rowsSkipped > 0)
            {
                MessageBox.Show(rowsRead + " element(s) were read and " + rowsSkipped
                    + " row(s) were skipped because they could not be read." + Environment.NewLine
                    + "Check the list below before accepting.",
                    "Partly imported", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            elementList = imported;
            dgv_Preview.ItemsSource = elementList;
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

        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Save a CSV file to use as a template for import.");
                string exportPath = DataExportUtils.GetSaveAsLocation();

                if (exportPath != null && exportPath != "")
                {
                    bool ok = DataExportUtils.CreateElementImportTemplate(exportPath);

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
}
