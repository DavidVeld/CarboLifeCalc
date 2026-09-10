using CarboLifeAPI.Data;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Application = Microsoft.Office.Interop.Excel.Application;
using Excel = Microsoft.Office.Interop.Excel;


namespace CarboLifeAPI
{
    public static class DataExportUtils
    {
        public static int i;

        static string reportpath;
        public static string GetSaveAsLocation()
        {
            //Create a File and save it as a HTML File
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save CSV file";
            saveDialog.Filter = "csv file |*.csv";
            saveDialog.FilterIndex = 2;
            saveDialog.RestoreDirectory = true;

            saveDialog.ShowDialog();

            string path = saveDialog.FileName;

            //Check if the file can be read and written to.
            if (File.Exists(path))
            {
                //FileInfo fileInfo = new FileInfo(path);
                bool isInUse = IsFileLocked(path);

                if (isInUse == true)
                    return null;
            }
            else
            {
                if (path != "")
                    //This is a new file
                    return path;
                else
                    return null;
            }


            //If this part is reached; return the valid path;
            return path;

        }

        public static string GetOpenCSVLocation()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Open CSV File";
            openDialog.Filter = "csv file|*.csv";
            openDialog.FilterIndex = 1;
            openDialog.RestoreDirectory = true;

            if (openDialog.ShowDialog() != true)
                return null;

            string path = openDialog.FileName;

            if (string.IsNullOrWhiteSpace(path) || File.Exists(path) == false)
                return null;

            //IsFileReadable, not IsFileLocked. IsFileLocked asks for write access, so it
            //answers "can I save over this" - and a file the user still has open in Excel
            //fails it. Opening one to read is exactly what this dialog is for, and the
            //material import dialog's own instructions tell the user to edit the file in
            //Excel first, so refusing an open file made the documented workflow a no-op.
            if (IsFileReadable(path) == false)
                return null;

            //If this part is reached; return the valid path;
            return path;

        }
        /// <summary>
        /// True when a file cannot be opened for writing right now, either because something else
        /// holds it or because this user is not allowed to write it.
        ///
        /// This asks for ReadWrite access, so it answers "can I save over this", not "can I read
        /// this". A caller that only intends to read should use <see cref="IsFileReadable"/>.
        /// </summary>
        public static bool IsFileLocked(string file)
        {
            try
            {
                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //The file is open
                return true;
            }
            catch (Exception)
            {
                //A read-only file, or one on a share the user may read but not write, raises
                //UnauthorizedAccessException rather than IOException. That escaped this method
                //entirely and was caught by whatever called it, which turned a plain permission
                //problem into whatever generic message that caller happened to carry. Unwritable
                //and locked are the same answer to everyone who asks this - none of them can
                //proceed with the write - so answer it here rather than throwing past them.
                //Mirrors the copy in CarboCircle. See also IsFileReadable below.
                return true;
            }

            //All is ok
            return false;
        }

        /// <summary>
        /// True when a file can actually be read right now.
        ///
        /// The pickers used to gate on <see cref="IsFileLocked"/>, which asks for write access.
        /// A company material database or mapping file on a share the user may read but not
        /// write failed that test, and the user was told their file could not be found or was of
        /// the wrong format. Reading a database does not need write access, so this is what the
        /// pickers ask instead. FileShare.ReadWrite because a colleague having the same shared
        /// file open is no reason we cannot read it.
        /// </summary>
        public static bool IsFileReadable(string file)
        {
            try
            {
                using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    stream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        /*
        [Obsolete("Not Used")]
        public static void ExportToExcel(CarboProject carboProject, string Path, bool exportResults, bool exportElements, bool exportMaterials)
        {

            reportpath = Path;

            //Check if user has excel
            Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            if (xlApp == null)
            {
                System.Windows.MessageBox.Show("You need to have excel installed to continue", "Computer says no", MessageBoxButton.OK);
                return;
            }

            CreateExcelFile(carboProject, xlApp, exportResults, exportElements, exportMaterials);

        }

        [Obsolete]
        private static void CreateExcelFile(CarboProject carboProject, Excel.Application xlApp, bool exportResults, bool exportElements, bool exportMaterials)
        {
            int row = 1;
            int col = 1;

            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Worksheet xlWorkSheet2;
            Excel.Worksheet xlWorkSheet3;

            xlApp.DisplayAlerts = false;

            object misValue = System.Reflection.Missing.Value;

            xlWorkBook = xlApp.Workbooks.Add(misValue);
            /////////////////////
            ///GroupTable
            if (exportResults == true)
            {
                xlWorkSheet = (Excel.Worksheet)xlApp.Worksheets.Add();
                if (xlWorkSheet != null)
                {
                    xlWorkSheet.Name = "GroupData";

                    row = 1;
                    col = 1;

                    xlWorkSheet.Cells[row, col] = "Category";
                    xlWorkSheet.Cells[row + 1, col] = "";

                    xlWorkSheet.Cells[row, col + 1] = "Material";
                    xlWorkSheet.Cells[row + 1, col + 1] = "";

                    xlWorkSheet.Cells[row, col + 2] = "Description";
                    xlWorkSheet.Cells[row + 1, col + 2] = "";

                    xlWorkSheet.Cells[row, col + 3] = "Total Volume";
                    xlWorkSheet.Cells[row + 1, col + 3] = "m³";

                    xlWorkSheet.Cells[row, col + 4] = "Density";
                    xlWorkSheet.Cells[row + 1, col + 4] = "kg/m³";

                    xlWorkSheet.Cells[row, col + 5] = "Mass";
                    xlWorkSheet.Cells[row + 1, col + 5] = "kg";

                    xlWorkSheet.Cells[row, col + 6] = "ECI";
                    xlWorkSheet.Cells[row + 1, col + 6] = "kgCO2e/kg";

                    xlWorkSheet.Cells[row, col + 7] = "EC";
                    xlWorkSheet.Cells[row + 1, col + 7] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 8] = "Total";
                    xlWorkSheet.Cells[row + 1, col + 8] = "%";

                    xlWorkSheet.Cells[row, col + 9] = "A1-A3";
                    xlWorkSheet.Cells[row + 1, col + 9] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 10] = "A4";
                    xlWorkSheet.Cells[row + 1, col + 10] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 11] = "A5";
                    xlWorkSheet.Cells[row + 1, col + 11] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 12] = "B1-B5";
                    xlWorkSheet.Cells[row + 1, col + 12] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 13] = "C1-C4";
                    xlWorkSheet.Cells[row + 1, col + 13] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 14] = "D";
                    xlWorkSheet.Cells[row + 1, col + 14] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 15] = "Mix";
                    xlWorkSheet.Cells[row + 1, col + 15] = "tCO2e";

                    //Advanced
                    xlWorkSheet.Cells[row, col + 16] = "[Formula]";
                    xlWorkSheet.Cells[row + 1, col + 16] = "";

                    xlWorkSheet.Cells[row, col + 17] = "[Waste]";
                    xlWorkSheet.Cells[row + 1, col + 17] = "%";

                    xlWorkSheet.Cells[row, col + 18] = "[B4]";
                    xlWorkSheet.Cells[row + 1, col + 18] = "x";

                    xlWorkSheet.Cells[row, col + 19] = "[Additional]";
                    xlWorkSheet.Cells[row + 1, col + 19] = "kgCO2e/kg";

                    xlWorkSheet.Cells[row, col + 20] = "Base Volume";
                    xlWorkSheet.Cells[row + 1, col + 20] = "m³";
                    row++;
                    i++;

                    foreach (CarboGroup grp in carboProject.getGroupList)
                    {
                        row++;

                        xlWorkSheet.Cells[row, col] = grp.Category;
                        xlWorkSheet.Cells[row, col + 1] = grp.MaterialName;
                        xlWorkSheet.Cells[row, col + 2] = grp.Description;
                        xlWorkSheet.Cells[row, col + 3] = grp.TotalVolume;
                        xlWorkSheet.Cells[row, col + 4] = grp.Density;
                        xlWorkSheet.Cells[row, col + 5] = grp.Mass;
                        xlWorkSheet.Cells[row, col + 6] = grp.ECI;
                        xlWorkSheet.Cells[row, col + 7] = grp.EC;
                        xlWorkSheet.Cells[row, col + 8] = grp.PerCent;
                        xlWorkSheet.Cells[row, col + 9] = (grp.inUseProperties.B4 * (grp.Material.ECI_A1A3 * grp.Mass)) / 1000;
                        xlWorkSheet.Cells[row, col + 10] = (grp.inUseProperties.B4 * (grp.Material.ECI_A4 * grp.Mass)) / 1000;
                        xlWorkSheet.Cells[row, col + 11] = (grp.inUseProperties.B4 * (grp.Material.ECI_A5 * grp.Mass)) / 1000;
                        xlWorkSheet.Cells[row, col + 12] = (grp.inUseProperties.B4 * (grp.Material.ECI_B1B5)) / 1000;
                        xlWorkSheet.Cells[row, col + 13] = (grp.inUseProperties.B4 * (grp.Material.ECI_C1C4 * grp.Mass)) / 1000;
                        xlWorkSheet.Cells[row, col + 14] = (grp.inUseProperties.B4 * (grp.Material.ECI_D * grp.Mass)) / 1000;
                        xlWorkSheet.Cells[row, col + 15] = (grp.inUseProperties.B4 * (grp.Material.ECI_Mix * grp.Mass)) / 1000;

                        xlWorkSheet.Cells[row, col + 16] = grp.Correction;
                        xlWorkSheet.Cells[row, col + 17] = grp.Waste;
                        xlWorkSheet.Cells[row, col + 18] = grp.inUseProperties.B4;
                        xlWorkSheet.Cells[row, col + 19] = grp.Additional;
                        xlWorkSheet.Cells[row, col + 20] = grp.Volume;


                        i++;
                    }
                    //Totals
                    row++;
                    xlWorkSheet.Cells[row, col + 7].Formula = string.Format("=SUM(H3:H{0})", (row - 1));
                    xlWorkSheet.Cells[row, col + 8].Formula = string.Format("=SUM(I3:I{0})", (row - 1));
                    xlWorkSheet.Cells[row, col + 9].Formula = string.Format("=SUM(J3:J{0})", (row - 1));
                    xlWorkSheet.Cells[row, col + 10].Formula = string.Format("=SUM(K3:K{0})", (row - 1));
                    xlWorkSheet.Cells[row, col + 11].Formula = string.Format("=SUM(L3:L{0})", (row - 1));
                    xlWorkSheet.Cells[row, col + 12].Formula = string.Format("=SUM(M3:M{0})", (row - 1));
                    xlWorkSheet.Cells[row, col + 13].Formula = string.Format("=SUM(N3:N{0})", (row - 1));
                    xlWorkSheet.Cells[row, col + 14].Formula = string.Format("=SUM(O3:O{0})", (row - 1));
                    xlWorkSheet.Cells[row, col + 15].Formula = string.Format("=SUM(P3:P{0})", (row - 1));
                    // xlWorkSheet.Cells[row, col + 15].Formula = string.Format("=SUM(U3:P{0})", (row - 1));


                    //Format the table
                    xlWorkSheet.Columns[1].ColumnWidth = 20;
                    xlWorkSheet.Columns[2].ColumnWidth = 40;
                    xlWorkSheet.Columns[3].ColumnWidth = 40;

                    Marshal.ReleaseComObject(xlWorkSheet);

                }
            }
            ////////////////////
            ///Element Table
            if (exportElements == true)
            {
                xlWorkSheet2 = (Excel.Worksheet)xlApp.Worksheets.Add();
                //newWorksheet = Excel.Worksheet)excelApp.Worksheets.Add(Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                if (xlWorkSheet2 != null)
                {
                    xlWorkSheet2.Name = "ElementData";

                    row = 1;
                    col = 1;

                    xlWorkSheet2.Cells[row, col] = "Id";
                    xlWorkSheet2.Cells[row + 1, col] = "";

                    xlWorkSheet2.Cells[row, col + 1] = "Name";
                    xlWorkSheet2.Cells[row + 1, col + 1] = "";

                    xlWorkSheet2.Cells[row, col + 2] = "Material Name";
                    xlWorkSheet2.Cells[row + 1, col + 2] = "";

                    xlWorkSheet2.Cells[row, col + 3] = "Category";
                    xlWorkSheet2.Cells[row + 1, col + 3] = "";

                    xlWorkSheet2.Cells[row, col + 4] = "Sub Category";
                    xlWorkSheet2.Cells[row + 1, col + 4] = "";

                    xlWorkSheet2.Cells[row, col + 5] = "Volume";
                    xlWorkSheet2.Cells[row + 1, col + 5] = "m³";

                    xlWorkSheet2.Cells[row, col + 6] = "Mass";
                    xlWorkSheet2.Cells[row + 1, col + 6] = "kg";

                    xlWorkSheet2.Cells[row, col + 7] = "Level";
                    xlWorkSheet2.Cells[row + 1, col + 7] = "mm";

                    xlWorkSheet2.Cells[row, col + 8] = "IsDemolished";
                    xlWorkSheet2.Cells[row + 1, col + 8] = "";

                    xlWorkSheet2.Cells[row, col + 9] = "IsExisting";
                    xlWorkSheet2.Cells[row + 1, col + 9] = "";

                    xlWorkSheet2.Cells[row, col + 10] = "IsSubstructure";
                    xlWorkSheet2.Cells[row + 1, col + 10] = "";

                    xlWorkSheet2.Cells[row, col + 11] = "ECI";
                    xlWorkSheet2.Cells[row + 1, col + 11] = "kgCO2e/kg";

                    xlWorkSheet2.Cells[row, col + 12] = "EC";
                    xlWorkSheet2.Cells[row + 1, col + 12] = "kgCO2e";

                    xlWorkSheet2.Cells[row, col + 13] = "ECI Cumulative";
                    xlWorkSheet2.Cells[row + 1, col + 13] = "kgCO2e/kg";

                    xlWorkSheet2.Cells[row, col + 14] = "EC Cumulative";
                    xlWorkSheet2.Cells[row + 1, col + 14] = "kgCO2e";

                    xlWorkSheet2.Cells[row, col + 15] = "Volume Cumulative";
                    xlWorkSheet2.Cells[row + 1, col + 15] = "m³";

                    row++;
                    i++;

                    foreach (CarboElement el in carboProject.getElementsFromGroups())
                    {
                        row++;

                        xlWorkSheet2.Cells[row, col] = el.Id;
                        xlWorkSheet2.Cells[row, col + 1] = el.Name;
                        xlWorkSheet2.Cells[row, col + 2] = el.MaterialName;
                        xlWorkSheet2.Cells[row, col + 3] = el.Category;
                        xlWorkSheet2.Cells[row, col + 4] = el.SubCategory;

                        xlWorkSheet2.Cells[row, col + 5] = el.Volume;
                        xlWorkSheet2.Cells[row, col + 6] = el.Mass;
                        xlWorkSheet2.Cells[row, col + 7] = el.Level;
                        xlWorkSheet2.Cells[row, col + 8] = el.isDemolished;

                        xlWorkSheet2.Cells[row, col + 9] = el.isExisting;
                        xlWorkSheet2.Cells[row, col + 10] = el.isSubstructure;
                        xlWorkSheet2.Cells[row, col + 11] = el.ECI;
                        xlWorkSheet2.Cells[row, col + 12] = el.EC;

                        xlWorkSheet2.Cells[row, col + 13] = el.ECI_Cumulative;
                        xlWorkSheet2.Cells[row, col + 14] = el.EC_Cumulative;
                        xlWorkSheet2.Cells[row, col + 15] = el.Volume_Cumulative;
                        i++;
                    }
                    //Totals

                    //Format the table
                    xlWorkSheet2.Columns[1].ColumnWidth = 15;
                    xlWorkSheet2.Columns[2].ColumnWidth = 40;
                    xlWorkSheet2.Columns[3].ColumnWidth = 40;
                    xlWorkSheet2.Columns[4].ColumnWidth = 20;

                    Marshal.ReleaseComObject(xlWorkSheet2);

                }
            }
            /////////////////////
            ///MaterialTable
            if (exportMaterials == true)
            {
                xlWorkSheet3 = (Excel.Worksheet)xlApp.Worksheets.Add();
                if (xlWorkSheet3 != null)
                {
                    xlWorkSheet3.Name = "MaterialData";

                    row = 1;
                    col = 1;

                    xlWorkSheet3.Cells[row, col] = "Id";
                    xlWorkSheet3.Cells[row + 1, col] = "";

                    xlWorkSheet3.Cells[row, col + 1] = "Name";
                    xlWorkSheet3.Cells[row + 1, col + 1] = "";

                    xlWorkSheet3.Cells[row, col + 2] = "Category";
                    xlWorkSheet3.Cells[row + 1, col + 2] = "";

                    xlWorkSheet3.Cells[row, col + 3] = "Description";
                    xlWorkSheet3.Cells[row + 1, col + 3] = "";

                    xlWorkSheet3.Cells[row, col + 4] = "Density";
                    xlWorkSheet3.Cells[row + 1, col + 4] = "kg/m³";

                    xlWorkSheet3.Cells[row, col + 5] = "ECI";
                    xlWorkSheet3.Cells[row + 1, col + 5] = "kgCO2e/kg";

                    xlWorkSheet3.Cells[row, col + 6] = "ECI Volume";
                    xlWorkSheet3.Cells[row + 1, col + 6] = "kgCO2e/n³";

                    xlWorkSheet3.Cells[row, col + 7] = "A1-A3";
                    xlWorkSheet3.Cells[row + 1, col + 7] = "kgCO2e/kg";

                    xlWorkSheet3.Cells[row, col + 8] = "A4";
                    xlWorkSheet3.Cells[row + 1, col + 8] = "kgCO2e/kg";

                    xlWorkSheet3.Cells[row, col + 9] = "A5";
                    xlWorkSheet3.Cells[row + 1, col + 9] = "kgCO2e/kg";

                    xlWorkSheet3.Cells[row, col + 10] = "B1-B7";
                    xlWorkSheet3.Cells[row + 1, col + 10] = "kgCO2e/kg";

                    xlWorkSheet3.Cells[row, col + 11] = "C1-C4";
                    xlWorkSheet3.Cells[row + 1, col + 11] = "kgCO2e/kg";

                    xlWorkSheet3.Cells[row, col + 12] = "D";
                    xlWorkSheet3.Cells[row + 1, col + 12] = "kgCO2e/kg";

                    xlWorkSheet3.Cells[row, col + 13] = "Mix";
                    xlWorkSheet3.Cells[row + 1, col + 13] = "kgCO2e/kg";

                    xlWorkSheet3.Cells[row, col + 14] = "B4";
                    xlWorkSheet3.Cells[row + 1, col + 14] = "factor";

                    row++;

                    foreach (CarboMaterial material in carboProject.CarboDatabase.CarboMaterialList)
                    {
                        row++;

                        xlWorkSheet3.Cells[row, col] = material.Id;
                        xlWorkSheet3.Cells[row, col + 1] = material.Name;
                        xlWorkSheet3.Cells[row, col + 2] = material.Category;
                        xlWorkSheet3.Cells[row, col + 3] = material.Description;
                        xlWorkSheet3.Cells[row, col + 4] = material.Density;

                        xlWorkSheet3.Cells[row, col + 5] = material.ECI;
                        xlWorkSheet3.Cells[row, col + 6] = material.getVolumeECI;

                        xlWorkSheet3.Cells[row, col + 7] = material.ECI_A1A3;
                        xlWorkSheet3.Cells[row, col + 8] = material.ECI_A4;
                        xlWorkSheet3.Cells[row, col + 9] = material.ECI_A5;
                        xlWorkSheet3.Cells[row, col + 10] = material.ECI_B1B5;
                        xlWorkSheet3.Cells[row, col + 11] = material.ECI_C1C4;
                        xlWorkSheet3.Cells[row, col + 12] = material.ECI_D;
                        xlWorkSheet3.Cells[row, col + 13] = material.ECI_Mix;
                        xlWorkSheet3.Cells[row, col + 14] = 0;

                    }

                    xlWorkSheet3.Columns[1].ColumnWidth = 15;
                    xlWorkSheet3.Columns[2].ColumnWidth = 30;
                    xlWorkSheet3.Columns[3].ColumnWidth = 20;
                    xlWorkSheet3.Columns[4].ColumnWidth = 50;

                    Marshal.ReleaseComObject(xlWorkSheet3);

                }
            }

            ////////////////////
            ///Save File
            xlWorkBook.SaveAs(reportpath, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

        }
        */

        public static void ExportComaringGraphs(CarboProject carboLifeProject, List<CarboProject> projectListToCompareTo, bool exportCurrentProject)
        {

            //Check if user has excel
            string path = GetSaveAsLocation();

            if (path != null)
            {
                try
                {
                    //Export a csv of all the projects with all the totals
                    List<CarboProject> listToExport = new List<CarboProject>();
                    foreach (CarboProject project in projectListToCompareTo)
                    {
                        listToExport.Add(project);
                    }

                    if (exportCurrentProject == true)
                    { 
                        listToExport.Add(carboLifeProject); 
                    }
                    CreateProjectCombinedExportCSV(listToExport, path, carboLifeProject);
                    //CreateProjectTotalsCVSFile(listToExport, path, carboLifeProject);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /*
        [Obsolete]
        private static void CreateTotalsExcelFile(List<CarboProject> projectList, Excel.Application xlApp, string path)
        {
            int row = 1;
            int col = 1;

            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;

            xlApp.DisplayAlerts = false;

            object misValue = System.Reflection.Missing.Value;

            xlWorkBook = xlApp.Workbooks.Add(misValue);

            //Add the durrent 

            try
            {

                xlWorkSheet = (Excel.Worksheet)xlApp.Worksheets.Add();
                if (xlWorkSheet != null)
                {
                    xlWorkSheet.Name = "Comparing Table";

                    row = 1;
                    col = 1;

                    /////////////////////
                    ///Headers
                    ///
                    xlWorkSheet.Cells[row, col] = "Project Nr";
                    xlWorkSheet.Cells[row + 1, col] = "";

                    xlWorkSheet.Cells[row, col + 1] = "Project Name";
                    xlWorkSheet.Cells[row + 1, col + 1] = "";

                    xlWorkSheet.Cells[row, col + 2] = "Total EC";
                    xlWorkSheet.Cells[row + 1, col + 2] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 3] = "A1-A3";
                    xlWorkSheet.Cells[row + 1, col + 3] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 4] = "A4";
                    xlWorkSheet.Cells[row + 1, col + 4] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 5] = "A5 (Material)";
                    xlWorkSheet.Cells[row + 1, col + 5] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 6] = "A5 (Global)";
                    xlWorkSheet.Cells[row + 1, col + 6] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 7] = "B1-B7";
                    xlWorkSheet.Cells[row + 1, col + 7] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 8] = "C1-C4";
                    xlWorkSheet.Cells[row + 1, col + 8] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 9] = "C1 (Global)";
                    xlWorkSheet.Cells[row + 1, col + 9] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 10] = "D";
                    xlWorkSheet.Cells[row + 1, col + 10] = "tCO2e";

                    xlWorkSheet.Cells[row, col + 11] = "Additional";
                    xlWorkSheet.Cells[row + 1, col + 11] = "tCO2e";

                    //Advanced

                    row++;
                    i++;

                    foreach (CarboProject cp in projectList)
                    {

                        List<CarboDataPoint> listofPoints = cp.getPhaseTotals();
                        //pointList.Add(listofPoints);

                        row++;

                        xlWorkSheet.Cells[row, col] = cp.Number;
                        xlWorkSheet.Cells[row, col + 1] = cp.Name;
                        xlWorkSheet.Cells[row, col + 2] = cp.getTotalEC();
                        xlWorkSheet.Cells[row, col + 3] = listofPoints[0].Value / 1000;
                        xlWorkSheet.Cells[row, col + 4] = listofPoints[1].Value / 1000;
                        xlWorkSheet.Cells[row, col + 5] = listofPoints[2].Value / 1000;
                        xlWorkSheet.Cells[row, col + 6] = listofPoints[3].Value / 1000;
                        xlWorkSheet.Cells[row, col + 7] = listofPoints[4].Value / 1000;
                        xlWorkSheet.Cells[row, col + 8] = listofPoints[5].Value / 1000;
                        xlWorkSheet.Cells[row, col + 9] = listofPoints[6].Value / 1000;
                        xlWorkSheet.Cells[row, col + 10] = listofPoints[7].Value / 1000;
                        xlWorkSheet.Cells[row, col + 11] = listofPoints[8].Value / 1000;

                        i++;
                    }


                    //Format the table
                    xlWorkSheet.Columns[1].ColumnWidth = 30;
                    xlWorkSheet.Columns[2].ColumnWidth = 40;
                    xlWorkSheet.Columns[3].ColumnWidth = 30;

                    Marshal.ReleaseComObject(xlWorkSheet);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            ////////////////////
            ///Save File
            xlWorkBook.SaveAs(path, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

        }
        */

        public static void ExportToCSV(CarboProject carboLifeProject, string cvsExportPath, bool exportResult, bool exportElements, bool exportMaterials, bool exportProject)
        {
            string exportpath = Path.GetDirectoryName(cvsExportPath);
            string prefix = Path.GetFileNameWithoutExtension(cvsExportPath);

            //Check if user has excel
            if (!Directory.Exists(exportpath) == true)
            {
                return;
            }

            if (prefix != "")
                prefix = prefix + "_";

            string exportpathResult = exportpath + "\\" + prefix + "Results.csv";
            string exportpathElements = exportpath + "\\" + prefix + "Elements.csv";
            string exportpathMaterials = exportpath + "\\" + prefix + "Materials.csv";
            string exportpathProject = exportpath + "\\" + prefix + "Project.csv";


            if (File.Exists(exportpathResult) == true ||
                File.Exists(exportpathElements) == true ||
                File.Exists(exportpathMaterials) == true ||
                File.Exists(exportpathProject) == true)
            {
                var result = System.Windows.MessageBox.Show("Do you want to override the existing files?", "Question", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) //Anything but YES should cancel the command
                    return;
            }

            //if no files were to be overwritten or no exsting files were found, then you woll arrive here:
            if (exportResult == true)
                CreateResultsCVSFile(carboLifeProject, exportpathResult);

            if (exportElements == true)
                CreateElementsCVSFile(carboLifeProject, exportpathElements);

            if (exportMaterials == true)
                CreateMaterialDatabaseCVSFile(carboLifeProject.CarboDatabase, exportpathMaterials);

            if (exportProject == true)
                CreateProjectDatabaseCVSFile(carboLifeProject, exportpathProject);

        }

            // 3. PROCESS ROWS
private static void CreateProjectCombinedExportCSV(List<CarboProject> projectListToCompareTo, string exportPath, CarboProject baseProject)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath))
                return;

            // 1. PRE-SCAN: Identify all unique Categories across all projects
            SortedSet<string> allCategories = new SortedSet<string>();
            foreach (CarboProject project in projectListToCompareTo)
            {
                List<CarboElement> projectElements = project.getElementsFromGroups().ToList();

                var dataPoints = CarboCalcTextUtils.ConvertResultTableToDataPointsMergedPlus(projectElements);
                if (dataPoints != null)
                {
                    foreach (var dp in dataPoints)
                        allCategories.Add(dp.Name);
                }
            }

            StringBuilder csvBuilder = new StringBuilder();

            // 2. CREATE HEADERS
            string headers = "Name,Number,Category,Description,SocialCost,Area,AreaNew,A0Global,A5Global,b675Global,C1Global," +
                             "TotalEC,Total_A1A3,Total_A4,Total_A5,Total_B,Total_C1C4,Total_D,Total_Mix,Total_Seq," +
                             "Total_A1_A5,Total_A1_C_Seq," + // New Columns
                             "valueUnit,designLife,story,uncertainty";

            foreach (string cat in allCategories)
            {
                headers += "," + CVSFormat(cat);
            }
            csvBuilder.AppendLine(headers);

            // 3. PROCESS ROWS
            foreach (CarboProject carboLifeProject in projectListToCompareTo)
            {
                try
                {
                    SyncProjectSettings(carboLifeProject, baseProject);
                    carboLifeProject.CalculateProject();

                    // Calculate Totals
                    double tA1 = 0, tEC = 0, tA4 = 0, tA5 = 0, tB = 0, tC = 0, tD = 0, tM = 0, tS = 0;
                    foreach (CarboGroup cbg in carboLifeProject.getGroupList)
                    {
                        if (cbg.Material == null)
                            continue;

                        //The group getters, so the B4 replacement count and the group's
                        //Additional allowance are in these totals. ECI * Mass left both out.
                        tEC += cbg.EC;
                        tA1 += cbg.getTotalA1A3;
                        tA4 += cbg.getTotalA4;
                        tA5 += cbg.getTotalA5;
                        tB += cbg.getTotalB1B7;
                        tC += cbg.getTotalC1C4;
                        tD += cbg.getTotalD;
                        tM += cbg.getTotalMix;
                        tS += cbg.getTotalSeq;
                    }

                    // Perform Sums for new columns
                    // Convert to matching units (dividing by 1000 where appropriate)
                    double sumA1A3 = tA1 / 1000.0;
                    double sumA4 = tA4 / 1000.0;
                    double sumA5 = tA5 / 1000.0;
                    double sumSeq = tS / 1000.0;
                    double sumMix = tM / 1000.0;

                    // Total A1-A5: A0Global + A5Global + Total_A1A3 + Total_A4 + Total_A5
                    double totalA1_A5 = (carboLifeProject.A0GlobalUncert/1000) + carboLifeProject.A5Global + sumA1A3 + sumA4 + sumA5;

                    // Total A1-C+Seq: Total A1-A5 + C1Global + Total_Seq + Total_Mix
                    double totalA1_C_Seq = totalA1_A5 + carboLifeProject.C1Global + (tC / 1000) + sumSeq + sumMix;

                    // Build Row
                    //String interpolation formats in the current culture, so these numbers used
                    //to arrive with a comma for a decimal point on half of Europe's machines.
                    CsvLine row = new CsvLine();

                    row.Add(carboLifeProject.Name);
                    row.Add(carboLifeProject.Number);
                    row.Add(carboLifeProject.Category);
                    row.Add(carboLifeProject.Description);
                    row.Add(carboLifeProject.SocialCost);
                    row.Add(carboLifeProject.Area);
                    row.Add(carboLifeProject.AreaNew);
                    row.Add(carboLifeProject.A0GlobalUncert / 1000, 3);
                    row.Add(carboLifeProject.A5Global);
                    row.Add(carboLifeProject.b675Global);
                    row.Add(carboLifeProject.C1Global);
                    row.Add(tEC);
                    row.Add(sumA1A3, 3);
                    row.Add(sumA4, 3);
                    row.Add(sumA5, 3);
                    row.Add(tB / 1000, 3);
                    row.Add(tC / 1000, 3);
                    row.Add(tD / 1000, 3);
                    row.Add(sumMix, 3);
                    row.Add(sumSeq, 3);
                    row.Add(totalA1_A5, 3);         // New Col 1
                    row.Add(totalA1_C_Seq, 3);      // New Col 2
                    row.Add(carboLifeProject.valueUnit);
                    row.Add(carboLifeProject.designLife);
                    row.Add(carboLifeProject.getGeneralText());
                    row.Add(carboLifeProject.UncertFact, 3);

                    // Attach Category Columns
                    List<CarboElement> pElements = carboLifeProject.getElementsFromGroups().ToList();

                    List<CarboDataPoint> catPoints = CarboCalcTextUtils.ConvertResultTableToDataPointsMergedPlus(pElements);
                    foreach (string catName in allCategories)
                    {
                        var match = catPoints?.FirstOrDefault(p => p.Name == catName);
                        double val = (match != null) ? match.Value : 0.0;
                        row.Add(val, 3);
                    }

                    csvBuilder.Append(row.ToLine());
                }
                catch { /* Skip */ }
            }

            WriteCVSFile(csvBuilder.ToString(), exportPath);
            MessageBox.Show("Export Complete.");
        }


        private static void CreateProjectDatabaseCVSFile(CarboProject carboLifeProject, string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            StringBuilder fileString = new StringBuilder();

            //Create Headers;
            fileString.Append(new CsvLine().AddRange(
                "Name",         //0
                "Number",       //1
                "Category",     //2
                "Description",  //3
                "SocialCost",   //4
                "Area",         //5
                "AreaNew",      //6

                "A0Global",     //7
                "A5Global",     //8
                "b675Global",   //9
                "C1Global",     //10

                "valueUnit",    //11
                "designLife",   //12
                "story"         //13
                ).ToLine());

            //Advanced
            try
            {
                CsvLine row = new CsvLine();

                //Name, Number, Category and Description are free text and used to go in raw.
                //A project called "Job 123, Phase 2" shifted every column to its right.
                row.Add(carboLifeProject.Name);                     //0
                row.Add(carboLifeProject.Number);                   //1
                row.Add(carboLifeProject.Category);                 //2
                row.Add(carboLifeProject.Description);              //3
                row.Add(carboLifeProject.SocialCost);               //4
                row.Add(carboLifeProject.Area);                     //5
                row.Add(carboLifeProject.AreaNew);                  //6
                //A0GlobalUncert is in kgCO2e while A5Global, b675Global and C1Global beside it are

                //all tCO2e. This column used to carry the kg figure, a thousand times its neighbours.

                row.Add(carboLifeProject.A0GlobalUncert / 1000);           //7
                row.Add(carboLifeProject.A5Global);                 //8
                row.Add(carboLifeProject.b675Global);               //9
                row.Add(carboLifeProject.C1Global);                 //10

                row.Add(carboLifeProject.valueUnit);                //11
                row.Add(carboLifeProject.designLife);               //12
                row.Add(carboLifeProject.getGeneralText());         //13

                fileString.Append(row.ToLine());
            }
            catch (IOException ex)
            {
                Console.WriteLine("An error occurred while writing the file: " + ex.Message);
            }

            WriteCVSFile(fileString.ToString(), exportPath);

        }
        private static void CreateProjectTotalsCVSFile(List<CarboProject> projectListToCompareTo, string exportPath, CarboProject baseProject)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            StringBuilder fileString = new StringBuilder();

            //Create Headers;
            fileString.Append(new CsvLine().AddRange(
                "Name",         //0
                "Number",       //1
                "Category",     //2
                "Description",  //3
                "SocialCost",   //4
                "Area",         //5
                "AreaNew",      //6

                "A0Global",     //7
                "A5Global",     //8
                "b675Global",   //9
                "C1Global",     //10

                "TotalEC",      //11
                "Total_A1A3",   //12
                "Total_A4",     //13
                "Total_A5",     //14
                "Total_B",      //15
                "Total_C1C4",   //16
                "Total_D",      //17
                "Total_Mix",    //18
                "Total_Seq",    //19

                "valueUnit",    //20
                "designLife",   //21
                "story"         //22
                ).ToLine());

            //Advanced


            foreach (CarboProject carboLifeProject in projectListToCompareTo)
            {
                try
                {
                    //Apply Baseproject Settings For Totals;
                    carboLifeProject.calculateA0 = baseProject.calculateA0;
                    carboLifeProject.calculateA13 = baseProject.calculateA13;
                    carboLifeProject.calculateA4 = baseProject.calculateA4;
                    carboLifeProject.calculateA5 = baseProject.calculateA5;
                    carboLifeProject.calculateB = baseProject.calculateB;
                    carboLifeProject.calculateC = baseProject.calculateC;
                    carboLifeProject.calculateD = baseProject.calculateD;
                    carboLifeProject.calculateAdd = baseProject.calculateAdd;
                    carboLifeProject.calculateSeq = baseProject.calculateSeq;
                    carboLifeProject.calculateSubStructure = baseProject.calculateSubStructure;

                    //Recalculate the Project
                    carboLifeProject.CalculateProject();

                    //Get all the Groups In the Project:
                    ObservableCollection<CarboGroup> cglist = carboLifeProject.getGroupList;
                    cglist = new ObservableCollection<CarboGroup>(cglist.OrderBy(i => i.MaterialName));

                    string material = "";

                    double totalA1 = 0;

                    double totalEC = 0;
                    double totalA4 = 0;
                    double totalA5 = 0;
                    double totalB = 0;
                    double totalC = 0;
                    double totalD = 0;
                    double totalM = 0;
                    double totalS = 0;

                    foreach (CarboGroup cbg in cglist)
                    {
                        if (cbg.Material == null)
                            continue;

                        //The group getters, so the B4 replacement count and the group's
                        //Additional allowance are in these totals. ECI * Mass left both out.
                        totalEC += cbg.EC;
                        totalA1 += cbg.getTotalA1A3;
                        totalA4 += cbg.getTotalA4;
                        totalA5 += cbg.getTotalA5;
                        totalB += cbg.getTotalB1B7;
                        totalC += cbg.getTotalC1C4;
                        totalD += cbg.getTotalD;
                        totalM += cbg.getTotalMix;
                        totalS += cbg.getTotalSeq;
                    }


                    CsvLine row = new CsvLine();

                    row.Add(carboLifeProject.Name);             //0
                    row.Add(carboLifeProject.Number);           //1
                    row.Add(carboLifeProject.Category);         //2
                    row.Add(carboLifeProject.Description);      //3
                    row.Add(carboLifeProject.SocialCost);       //4
                    row.Add(carboLifeProject.Area);             //5
                    row.Add(carboLifeProject.AreaNew);          //6

                    //A0GlobalUncert is in kgCO2e while A5Global, b675Global and C1Global beside it are


                    //all tCO2e. This column used to carry the kg figure, a thousand times its neighbours.


                    row.Add(carboLifeProject.A0GlobalUncert / 1000);   //7
                    row.Add(carboLifeProject.A5Global);         //8
                    row.Add(carboLifeProject.b675Global);       //9
                    row.Add(carboLifeProject.C1Global);         //10

                    row.Add(totalEC);                           //11
                    row.Add(totalA1 / 1000, 3);                 //12
                    row.Add(totalA4 / 1000, 3);                 //13
                    row.Add(totalA5 / 1000, 3);                 //14
                    row.Add(totalB / 1000, 3);                  //15
                    row.Add(totalC / 1000, 3);                  //16
                    row.Add(totalD / 1000, 3);                  //17
                    row.Add(totalM / 1000, 3);                  //18
                    row.Add(totalS / 1000, 3);                  //19

                    row.Add(carboLifeProject.valueUnit);        //20
                    row.Add(carboLifeProject.designLife);       //21
                    row.Add(carboLifeProject.getGeneralText()); //22

                    fileString.Append(row.ToLine());
                }
                catch (IOException ex)
                {

                }
            }

            WriteCVSFile(fileString.ToString(), exportPath);
            MessageBox.Show("Export File Created at: " + exportPath);
        }

        private static void CreateProjectCategoryExportCSV(List<CarboProject> projectListToCompareTo, string exportPath, CarboProject baseProject)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath))
                return;

            // 1. Identify all unique Categories/Substructures across ALL projects to create headers
            // We use a SortedSet to keep them in alphabetical order
            SortedSet<string> allCategories = new SortedSet<string>();

            foreach (CarboProject project in projectListToCompareTo)
            {
                List<CarboElement> projectElements = project.getElementsFromGroups().ToList();

                var dataPoints = CarboCalcTextUtils.ConvertResultTableToDataPointsMergedPlus(projectElements);
                if (dataPoints != null)
                {
                    foreach (var dp in dataPoints)
                    {
                        allCategories.Add(dp.Name);
                    }
                }
            }

            StringBuilder csvBuilder = new StringBuilder();

            // 2. Create Headers
            csvBuilder.Append("Number,Name");
            foreach (string cat in allCategories)
            {
                csvBuilder.Append("," + CVSFormat(cat));
            }
            csvBuilder.AppendLine();

            // 3. Process each project
            foreach (CarboProject carboLifeProject in projectListToCompareTo)
            {
                try
                {
                    // Sync settings and Recalculate (as per your original logic)
                    SyncProjectSettings(carboLifeProject, baseProject);
                    carboLifeProject.CalculateProject();

                    // Get the specific data points for this project
                    List<CarboElement> projectElements = carboLifeProject.getElementsFromGroups().ToList();

                    List<CarboDataPoint> projectPoints = CarboCalcTextUtils.ConvertResultTableToDataPointsMergedPlus(projectElements);

                    // Start the row
                    CsvLine row = new CsvLine();
                    row.Add(carboLifeProject.Number);
                    row.Add(carboLifeProject.Name);

                    // Map values to the global category list
                    foreach (string cat in allCategories)
                    {
                        var match = projectPoints?.FirstOrDefault(p => p.Name == cat);
                        double value = (match != null) ? match.Value : 0.0;

                        row.Add(value, 3);
                    }

                    csvBuilder.Append(row.ToLine());
                }
                catch (Exception ex)
                {
                    // Log error or skip project
                }
            }

            // 4. Export
            WriteCVSFile(csvBuilder.ToString(), exportPath);
            MessageBox.Show("Category Batch Export Created at: " + exportPath);
        }

        /// <summary>
        /// Helper to sync settings from a base project to a target project
        /// </summary>
        private static void SyncProjectSettings(CarboProject target, CarboProject baseProject)
        {
            target.calculateA0 = baseProject.calculateA0;
            target.calculateA13 = baseProject.calculateA13;
            target.calculateA4 = baseProject.calculateA4;
            target.calculateA5 = baseProject.calculateA5;
            target.calculateB = baseProject.calculateB;
            target.calculateC = baseProject.calculateC;
            target.calculateD = baseProject.calculateD;
            target.calculateAdd = baseProject.calculateAdd;
            target.calculateSeq = baseProject.calculateSeq;
            target.calculateSubStructure = baseProject.calculateSubStructure;
        }
        private static void CreateResultsCVSFile(CarboProject carboLifeProject, string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            StringBuilder fileString = new StringBuilder();

            //Create Headers;
            fileString.Append(new CsvLine().AddRange(
                "Category",                 //0
                "Material",                 //1
                "Description",              //2
                "Base Volume",              //3
                "[Formula]",                //4
                "[Waste] (%)",              //5
                "[B4] (x)",                 //6
                //This column carries grp.Additional, which is an intensity per kg, not a tonnage.
                "[Additional] (kgCO2e/kg)", //7
                "Total Volume",             //8
                "Density (kg/m³)",          //9
                "Mass (kg)",                //10

                "ECI (kgCO2e/kg)",          //11
                "EC (tCO2e)",               //12
                "Total (%)",                //13

                "A1-A3 (tCO2e)",            //14
                "A4 (tCO2e)",               //15
                "A5 (tCO2e)",               //16
                "B1-B5 (tCO2e)",            //17
                "C1-C4 (tCO2e)",            //18
                "D (tCO2e)",                //19
                "Sequestration (tCO2e)",    //20
                "Additional (tCO2e)"        //21
                ).ToLine());

            //Advanced
            foreach (CarboGroup grp in carboLifeProject.getGroupList)
            {
                try
                {
                    CsvLine row = new CsvLine();

                    row.Add(grp.Category);              //0
                    row.Add(grp.MaterialName);          //1
                    row.Add(grp.Description);           //2
                    row.Add(grp.Volume);                //3
                    row.Add(grp.Correction);            //4
                    row.Add(grp.Waste);                 //5
                    row.Add(grp.inUseProperties.B4);    //6
                    row.Add(grp.Additional);            //7
                    row.Add(grp.TotalVolume);           //8
                    row.Add(grp.Density);               //9
                    row.Add(grp.Mass);                  //10

                    row.Add(grp.ECI);                   //11
                    row.Add(grp.EC);                    //12
                    row.Add(grp.PerCent);               //13

                    //The group's own getters rather than a hand rolled ECI * Mass, which had
                    //drifted from them in three ways. They were missing the B4 replacement
                    //count, which the Excel exporter this was adapted from did apply, so a
                    //group on a 20 year element life in a 60 year building reported a third of
                    //its carbon. B1-B5 had lost its "* grp.Mass" altogether and was writing an
                    //intensity divided by a thousand into a tonnage column. And the Additional
                    //allowance never reached the last column, where getTotalMix folds it in.
                    //These getters are what the application itself displays.
                    if (grp.Material != null)
                    {
                        row.Add(grp.getTotalA1A3 / 1000);   //14
                        row.Add(grp.getTotalA4 / 1000);     //15
                        row.Add(grp.getTotalA5 / 1000);     //16
                        row.Add(grp.getTotalB1B7 / 1000);   //17
                        row.Add(grp.getTotalC1C4 / 1000);   //18
                        row.Add(grp.getTotalD / 1000);      //19
                        row.Add(grp.getTotalSeq / 1000);    //20
                        row.Add(grp.getTotalMix / 1000);    //21
                    }
                    else
                    {
                        //Nothing to price the group with, and the getters would throw on it.
                        for (int i = 14; i <= 21; i++)
                            row.Add(0d);
                    }

                    fileString.Append(row.ToLine());
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            WriteCVSFile(fileString.ToString(), exportPath);

        }
        private static void CreateElementsCVSFile(CarboProject carboLifeProject, string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            StringBuilder fileString = new StringBuilder();

            //Create Headers;
            fileString.Append(getElementCsvHeader().ToLine());

            //One calculation feeds this file, the JSON export and the LCAx export.
            //
            //This used to look the material up again per element through getClosestMatch, which
            //goes back to the database and so ignores any edit made to the material inside the
            //project, and it then priced the element with a bare ECI * mass: no B4 replacement
            //count and no group Additional. The same file's group rows, a hundred lines further
            //down, went through the group getters and did include both, so one file answered the
            //same question two different ways.
            //
            //converToJsProject reads the material off the group the element actually sits in,
            //applies B4 and Additional exactly as CarboGroup.getTotalXX does, sorts by Id and
            //appends one row per element-less group, which is why the second loop that used to
            //be here has gone.
            JsCarboProject jsProject = JsonExportUtils.converToJsProject(carboLifeProject);

            foreach (JsCarboElement el in jsProject.elementList)
            {
                CsvLine row = new CsvLine();

                row.Add(el.Id);                     //0
                row.Add(el.Category);               //1
                row.Add(el.Name);                   //2
                row.Add(el.SubCategory);            //3
                row.Add(el.MaterialName);           //4
                row.Add(el.CarboMaterialName);      //5
                row.Add(el.Level);                  //6
                row.Add(el.LevelName);              //7

                row.Add(el.Volume);                 //8
                row.Add(el.Volume_Total);           //9
                row.Add(el.Volume_Cumulative);      //10

                row.Add(el.Density);                //11
                row.Add(el.Mass);                   //12
                row.Add(el.Grade);                  //13

                row.Add(el.ECI);                    //14
                row.Add(el.ECI_Cumulative);         //15
                row.Add(el.EC);                     //16
                row.Add(el.EC_Cumulative);          //17

                row.Add(el.isExisting);             //18
                row.Add(el.isDemolished);           //19
                row.Add(el.isSubstructure);         //20
                row.Add(el.includeInCalc);          //21

                row.Add(el.AdditionalData);         //22

                row.Add(el.EC_A1A3_Total);          //23
                row.Add(el.EC_A4_Total);            //24
                row.Add(el.EC_A5_Total);            //25
                row.Add(el.EC_B1B7_Total);          //26
                row.Add(el.EC_C1C4_Total);          //27
                row.Add(el.EC_D_Total);             //28
                row.Add(el.EC_Mix_Total);           //29
                row.Add(el.EC_Sequestration_Total); //30

                row.Add(el.Correction);             //31
                row.Add(el.RCDensity);              //32
                row.Add(el.Area);                   //33
                row.Add(el.GUID);                   //34
                //Appended: the Revit material class, which the matcher uses beside the name.
                row.Add(el.MaterialCategoryName);   //35

                fileString.Append(row.ToLine());
            }

            WriteCVSFile(fileString.ToString(), exportPath);

}
        public static string CVSFormat(string str)
        {
            if (str == null)
                str = "";

            str = str.Replace("\"", "\"\"");
            //Flatten
            str = Regex.Replace(str, @"\t|\n|\r", "");

            if (str.Contains(",") | str.Contains("\n") | str.Contains("\r") | str.Contains("\""))
            {
                str = "\"" + str + "\"";
            }

            return str;
        }

        /// <summary>
        /// Builds one line of a CSV file, one field at a time.
        /// </summary>
        /// <remarks>
        /// Everything written to a CSV goes through here, for two reasons.
        ///
        /// Numbers are formatted with the invariant culture. The rows used to be built by
        /// concatenating values straight into a string, which formats them in the user's own
        /// culture, so on any comma decimal locale 12.5 was written as "12,5" and one column
        /// silently became two. A four column row came out of a Dutch, German or Italian
        /// machine with seven fields in it.
        ///
        /// Text goes through CVSFormat exactly once, so a comma in a project name or an
        /// element level no longer shifts every column to its right.
        ///
        /// The separator is written between fields rather than after each one, which is what
        /// keeps the field count honest: several of these files used to end every data row
        /// with a trailing comma while the header did not, giving the rows one phantom column
        /// more than the header. Utils.LoadCSV throws on that when the file is read back.
        /// </remarks>
        public sealed class CsvLine
        {
            private readonly StringBuilder builder = new StringBuilder();
            private int fieldCount;

            /// <summary>
            /// How many fields the line holds, so a header and its rows can be checked against
            /// each other.
            /// </summary>
            public int FieldCount { get { return fieldCount; } }

            private CsvLine AppendField(string preparedText)
            {
                if (fieldCount > 0)
                    builder.Append(',');

                builder.Append(preparedText);
                fieldCount++;

                return this;
            }

            /// <summary>Text, escaped and quoted only where the content needs it.</summary>
            public CsvLine Add(string value)
            {
                return AppendField(CVSFormat(value));
            }

            /// <summary>
            /// A number. NaN and the infinities are written as 0: they would otherwise arrive as
            /// the words "NaN" and "Infinity" in a numeric column, which Excel shows as text and
            /// the reader cannot make a number of.
            /// </summary>
            public CsvLine Add(double value)
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                    return AppendField("0");

                return AppendField(value.ToString(CultureInfo.InvariantCulture));
            }

            public CsvLine Add(double value, int decimals)
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                    return AppendField("0");

                return AppendField(Math.Round(value, decimals).ToString(CultureInfo.InvariantCulture));
            }

            public CsvLine Add(int value)
            {
                return AppendField(value.ToString(CultureInfo.InvariantCulture));
            }

            public CsvLine Add(long value)
            {
                return AppendField(value.ToString(CultureInfo.InvariantCulture));
            }

            /// <summary>Written as True/False, the same as before this class existed.</summary>
            public CsvLine Add(bool value)
            {
                return AppendField(value ? "True" : "False");
            }

            public CsvLine AddEmpty()
            {
                return AppendField(string.Empty);
            }

            /// <summary>Adds every string in order, for building a header.</summary>
            public CsvLine AddRange(params string[] values)
            {
                foreach (string value in values)
                    Add(value);

                return this;
            }

            /// <summary>The finished line, with its line break.</summary>
            public string ToLine()
            {
                return builder.ToString() + Environment.NewLine;
            }
        }

        /// <summary>
        /// The element csv header, used by the Elements export and by the import template so the
        /// two cannot drift apart. GetElementsFromCVSFile reads a file in this shape back, so an
        /// exported Elements.csv is a valid import file: edit it and bring it straight back in.
        /// </summary>
        /// <remarks>
        /// The columns after Grade are computed - mass, densities, the ECI and EC figures - and
        /// are written for reading, not for reading back. An import takes the geometry, the
        /// names and the flags, and the calculation works the rest out again from the material.
        ///
        /// A column may only ever be APPENDED. Material Class went on the end for that reason:
        /// it is what the matcher uses beside the material name, and without it a round trip
        /// came back matching on the name alone.
        /// </remarks>
        private static CsvLine getElementCsvHeader()
        {
            return new CsvLine().AddRange(
                "Id",                           //0
                "Category",                     //1
                "Name",                         //2
                "SubCategory",                  //3

                "Material Name",                //4
                "Carbo Material Name",          //5
                "Level",                        //6
                "Level Name",                   //7

                "Volume (m3)",                  //8
                "Volume Total (m3)",            //9
                "Volume Cumulative (m3)",       //10

                "Density (kg/m3)",              //11
                "Mass (kg)",                    //12
                "Grade",                        //13

                "ECI (kgCO2e/kg)",              //14
                "ECI Cumulative (kgCO2e/kg)",   //15
                "EC (kgCO2e)",                  //16
                "EC Cumulative (kgCO2e)",       //17

                "isExisting",                   //18
                "isDemolished",                 //19
                "isSubstructure",               //20
                "includeInCalc",                //21
                "Additional",                   //22

                "EC A1A3 (kgCO2e)",             //23
                "EC A4 (kgCO2e)",               //24
                "EC A5 (kgCO2e)",               //25
                "EC B1B7 (kgCO2e)",             //26
                "EC C1C4 (kgCO2e)",             //27
                "EC D (kgCO2e)",                //28
                "EC Misc (kgCO2e)",             //29
                "EC Sequestration (kgCO2e)",    //30

                "Correction",                   //31
                "RC Density (kg/m3)",           //32
                "Area (m2)",                    //33
                "GUID",                         //34
                "Material Class"                //35
                );
        }

        /// <summary>
        /// Reads a number out of a field of one of these CSV files.
        /// </summary>
        /// <remarks>
        /// The counterpart to CsvLine, and it has to be: these files are written in the
        /// invariant culture, so that is the reading tried first, with NumberStyles.Float so
        /// that no separator can be swallowed as a thousands group.
        ///
        /// Utils.ConvertMeToDouble is the wrong tool here and was quietly corrupting the
        /// material round trip. It resolves what a user typed into a text box, and it treats a
        /// lone separator with exactly three digits behind it as genuinely ambiguous, deferring
        /// to the machine's own culture. An exported "0.225" read back on a Dutch or German
        /// machine therefore came in as 225, a factor of a thousand, with nothing to show for it.
        ///
        /// It stays as the fallback, for a file somebody has opened in Excel and saved back out
        /// with their own decimal comma in it.
        /// </remarks>
        public static double ReadCsvDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            double result;

            if (double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;

            Utils.TryConvertToDouble(value, out result);

            return result;
        }

        [Obsolete("Replaced by CreateMaterialDatabaseCSVFile")]
        private static void CreateMaterialsCVSFile(CarboProject carboLifeProject, string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            StringBuilder fileString = new StringBuilder();

            //Create Headers;
            fileString.Append(new CsvLine().AddRange(
                "Id",                           //0
                "Name",                         //1
                "Category",                     //2
                "Description",                  //3
                "Density",                      //4
                "ECI (kgCO2e/kg)",              //5
                "ECI Volume (kgCO2e/m³)",       //6

                "A1-A3 (kgCO2e/kg)",            //7
                "A4 (kgCO2e/kg)",               //8
                "A5 (kgCO2e/kg)",               //9
                "B1-B7 (kgCO2e/kg)",            //10
                "C1-C4 (kgCO2e/kg)",            //11
                "D (tCO2e)",                    //12
                "Sequestration (kgCO2e/kg)",    //13
                "Additional (kgCO2e/kg)",       //14

                "Default Waste (%)"             //15
                ).ToLine());

            ObservableCollection<CarboGroup> cglist = carboLifeProject.getGroupList;
            cglist = new ObservableCollection<CarboGroup>(cglist.OrderBy(i => i.MaterialName));

            string material = "";

            foreach (CarboGroup cbg in cglist)
            {
                if (cbg.MaterialName != material)
                {
                    CsvLine row = new CsvLine();

                    row.Add(cbg.Material.Id);               //0
                    row.Add(cbg.Material.Name);             //1
                    row.Add(cbg.Material.Category);         //2
                    row.Add(cbg.Material.Description);      //3
                    row.Add(cbg.Material.Density);          //4
                    row.Add(cbg.Material.ECI);              //5
                    row.Add(cbg.Material.getVolumeECI);     //6

                    row.Add(cbg.Material.ECI_A1A3);         //7
                    row.Add(cbg.Material.ECI_A4);           //8
                    row.Add(cbg.Material.ECI_A5);           //9
                    row.Add(cbg.Material.ECI_B1B5);         //10
                    row.Add(cbg.Material.ECI_C1C4);         //11
                    row.Add(cbg.Material.ECI_D);            //12
                    row.Add(cbg.Material.ECI_Seq);          //13
                    row.Add(cbg.Material.ECI_Mix);          //14

                    row.Add(cbg.Material.WasteFactor);      //15

                    fileString.Append(row.ToLine());
                }
            }

            WriteCVSFile(fileString.ToString(), exportPath);
        }

        public static void WriteCVSFile(string fileString, string exportPath)
        {
            try
            {
                //UTF-8 with a byte order mark. Without one Excel opens the file as ANSI, so the
                //kg/m³ in the headers and the £ in the project file arrive as mojibake. Reading
                //is unaffected: Utils.LoadCSV goes through StreamReader, which detects the mark
                //and strips it, so the first header still reads as "Id" and not "﻿Id".
                using (StreamWriter writer = new StreamWriter(exportPath, false, new UTF8Encoding(true)))
                {
                    //Write, not WriteLine: every row already carries its own line break, so
                    //WriteLine left a blank line on the end of every file.
                    writer.Write(fileString);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("An error occurred while writing the file: " + ex.Message);
            }
        }

        //MaterialDatabase
        public static bool CreateMaterialDatabaseCVSFile(CarboDatabase materialDataBase, string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return false;

           /* string fileString = "DO NOT REMOVE THIS LINE: " +
                            "edit the data below and use as import template, do not change column layout. " +
                            "materials with identical ID's will be overwritten. " +
                            "New Ids will be imported as a new material" + Environment.NewLine; */

            //This file is an import template as well as an export: GetMaterialDatabaseFromCVSFile
            //reads it back by column index, 0 to 16, so the order below is a contract. The
            //values it parses go through Utils.ConvertMeToDouble, which resolves a lone
            //separator before handing the text to a culture, so the invariant "12.5" written
            //here still reads back as 12.5 on a comma decimal machine.
            StringBuilder fileString = new StringBuilder();

            //Create Headers;
            fileString.Append(new CsvLine().AddRange(
                "Id",                       //0
                "Name",                     //1
                "Category",                 //2
                "Description",              //3
                "Density",                  //4
                "WasteFactor",              //5
                "Grade",                    //6
                "EPDURL",                   //7

                "ECI (kgCO2e/kg)",          //8
                "ECI_A1A3 (kgCO2e/kg)",     //9
                "ECI_A4 (kgCO2e/kg)",       //10
                "ECI_A5 (kgCO2e/kg)",       //11
                "ECI_B1B5 (kgCO2e/kg)",     //12
                "ECI_C1C4 (kgCO2e/kg)",     //13
                "ECI_D (kgCO2e/kg)",        //14
                "ECI_Seq (kgCO2e/kg)",      //15
                "ECI_Mix (kgCO2e/kg)"       //16
                ).ToLine());

            //Advanced
            foreach (CarboMaterial cm in materialDataBase.CarboMaterialList)
            {
                try
                {
                    CsvLine row = new CsvLine();

                    row.Add(cm.Id);             //0
                    row.Add(cm.Name);           //1
                    row.Add(cm.Category);       //2
                    row.Add(cm.Description);    //3
                    row.Add(cm.Density);        //4
                    row.Add(cm.WasteFactor);    //5
                    row.Add(cm.Grade);          //6
                    //A URL can hold a comma, and this one used to go in raw.
                    row.Add(cm.EPDurl);         //7

                    row.Add(cm.ECI);            //8
                    row.Add(cm.ECI_A1A3);       //9
                    row.Add(cm.ECI_A4);         //10
                    row.Add(cm.ECI_A5);         //11
                    row.Add(cm.ECI_B1B5);       //12
                    row.Add(cm.ECI_C1C4);       //13
                    row.Add(cm.ECI_D);          //14
                    row.Add(cm.ECI_Seq);        //15
                    row.Add(cm.ECI_Mix);        //16

                    fileString.Append(row.ToLine());
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            try
            {
                WriteCVSFile(fileString.ToString(), exportPath);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static List<CarboMaterial> GetMaterialDatabaseFromCVSFile(string importPath)
        {
            int rowsRead;
            int rowsSkipped;
            return GetMaterialDatabaseFromCVSFile(importPath, out rowsRead, out rowsSkipped);
        }

        /// <summary>
        /// Reads a material csv, and reports how many rows made it and how many were skipped.
        /// </summary>
        /// <remarks>
        /// The counts exist so a caller can tell "this file held no materials" from "this file
        /// could not be read". Every row is parsed inside its own try, so a file with the wrong
        /// column order, or one Excel has re-saved in a shape this cannot read, used to come
        /// back as an empty list with nothing said about it - and an empty list handed to
        /// CarboDatabase.SyncCSVMaterials with "delete materials not in list" ticked emptied
        /// the user's library.
        /// </remarks>
        public static List<CarboMaterial> GetMaterialDatabaseFromCVSFile(string importPath, out int rowsRead, out int rowsSkipped)
        {
            List<CarboMaterial> cmList = new List<CarboMaterial>();
            rowsRead = 0;
            rowsSkipped = 0;

            //IsFileReadable, not IsFileLocked: this only reads the file, and the dialog that
            //calls it asks the user to edit the csv in Excel first.
            if (File.Exists(importPath) && IsFileReadable(importPath))
            {
                System.Data.DataTable profileTable = Utils.LoadCSV(importPath);

                foreach (DataRow dr in profileTable.Rows)
                {
                    try
                    {
                        CarboMaterial cm = new CarboMaterial();
                        cm.Id = Convert.ToInt32(ReadCsvDouble(dr[0].ToString()));
                        cm.Name = dr[1].ToString();
                        cm.Category = dr[2].ToString();
                        cm.Description = dr[3].ToString();

                        cm.Density = Convert.ToInt32(ReadCsvDouble(dr[4].ToString()));
                        cm.WasteFactor = ReadCsvDouble(dr[5].ToString());
                        cm.Grade = dr[6].ToString();
                        cm.EPDurl = dr[7].ToString();

                        cm.ECI = ReadCsvDouble(dr[8].ToString());

                        cm.ECI_A1A3_Override = true;
                        cm.ECI_A4_Override = true;
                        cm.ECI_A5_Override = true;
                        cm.ECI_C1C4_Override = true;
                        cm.ECI_D_Override = true;
                        cm.ECI_Seq_Override = true;


                        cm.ECI_A1A3 = ReadCsvDouble(dr[9].ToString());
                        cm.ECI_A4 = ReadCsvDouble(dr[10].ToString());
                        cm.ECI_A5 = ReadCsvDouble(dr[11].ToString());
                        cm.ECI_B1B5 = ReadCsvDouble(dr[12].ToString());
                        cm.ECI_C1C4 = ReadCsvDouble(dr[13].ToString());
                        cm.ECI_D = ReadCsvDouble(dr[14].ToString());
                        cm.ECI_Seq = ReadCsvDouble(dr[15].ToString());
                        cm.ECI_Mix = ReadCsvDouble(dr[16].ToString());



                        cmList.Add(cm);
                        rowsRead++;
                    }
                    catch(Exception ex)
                    {
                        //A row in the wrong shape, most often a file whose columns have been
                        //reordered or which was saved in a layout this cannot read. Counted so
                        //the caller can say so rather than presenting an empty list as success.
                        rowsSkipped++;
                    }
                }
            }

            return cmList;
        }

        public static bool CreateElementImportTemplate(string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return false;

            //The same layout the Elements export writes, so the template and an exported
            //Elements.csv are one format: whichever of the two a user has in front of them
            //behaves the same way coming back in.
            StringBuilder fileString = new StringBuilder();

            //Create Headers;
            fileString.Append(getElementCsvHeader().ToLine());

            //One example row. The computed columns are left empty on purpose: mass, density and
            //the ECI and EC figures are worked out from the material and the volume on import,
            //so anything typed into them is ignored. A non round volume, because the importer
            //used to round every one of them to a whole number.
            CsvLine example = new CsvLine();

            example.Add(999999L);           //0  Id
            example.Add("Floors");          //1  Category
            example.Add("Example Element"); //2  Name
            example.AddEmpty();             //3  SubCategory
            example.Add("Concrete");        //4  Material Name
            example.AddEmpty();             //5  Carbo Material Name, filled in by the matcher
            example.Add(0d);                //6  Level, the elevation
            example.Add("Level 01");        //7  Level Name
            example.Add(12.5);              //8  Volume (m3)
            example.AddEmpty();             //9  Volume Total, computed
            example.AddEmpty();             //10 Volume Cumulative, computed
            example.AddEmpty();             //11 Density, computed
            example.AddEmpty();             //12 Mass, computed
            example.Add("C32/40");          //13 Grade
            example.AddEmpty();             //14 ECI, computed
            example.AddEmpty();             //15 ECI Cumulative, computed
            example.AddEmpty();             //16 EC, computed
            example.AddEmpty();             //17 EC Cumulative, computed
            example.Add(false);             //18 isExisting
            example.Add(false);             //19 isDemolished
            example.Add(false);             //20 isSubstructure
            example.Add(true);              //21 includeInCalc
            example.Add("Transfer slab");   //22 Additional
            example.AddEmpty();             //23 EC A1A3, computed
            example.AddEmpty();             //24 EC A4, computed
            example.AddEmpty();             //25 EC A5, computed
            example.AddEmpty();             //26 EC B1B7, computed
            example.AddEmpty();             //27 EC C1C4, computed
            example.AddEmpty();             //28 EC D, computed
            example.AddEmpty();             //29 EC Misc, computed
            example.AddEmpty();             //30 EC Sequestration, computed
            example.AddEmpty();             //31 Correction
            example.AddEmpty();             //32 RC Density
            example.AddEmpty();             //33 Area
            example.AddEmpty();             //34 GUID
            example.Add("Concrete");        //35 Material Class

            fileString.Append(example.ToLine());

            try
            {
                WriteCVSFile(fileString.ToString(), exportPath);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static List<CarboElement> GetElementsFromCVSFile(string importPath)
        {
            int rowsRead;
            int rowsSkipped;
            return GetElementsFromCVSFile(importPath, out rowsRead, out rowsSkipped);
        }

        /// <summary>
        /// Reads elements out of a csv, and reports how many rows made it and how many did not.
        /// </summary>
        /// <remarks>
        /// Columns are matched by HEADER NAME, not by position, which is what lets one file
        /// serve both directions: an exported Elements.csv can be edited and brought straight
        /// back in, so a problem can be chased through the same file on the way out and the way
        /// back. Reading by position meant the export and the import were two different layouts
        /// that happened to share a first column, and feeding an exported Elements.csv to the
        /// importer read Category as the name, a material name as a volume, and rejected every
        /// row.
        ///
        /// The narrow old template still works: names are compared with spaces, case and any
        /// trailing "(m3)" style unit ignored, and where the two files call the same thing by
        /// different names both spellings are accepted. "Level" is the one genuine collision -
        /// the old template used it for the level's NAME, the export uses it for the elevation -
        /// so it is read as an elevation only when a separate "Level Name" column is present.
        ///
        /// Mass, density and every ECI or EC column are deliberately not read. They are results,
        /// and the calculation works them out again from the material and the volume; taking
        /// them from the file would let a stale number outlive the thing it was computed from.
        /// </remarks>
        public static List<CarboElement> GetElementsFromCVSFile(string importPath, out int rowsRead, out int rowsSkipped)
        {
            List<CarboElement> elementList = new List<CarboElement>();
            rowsRead = 0;
            rowsSkipped = 0;

            //IsFileReadable, not IsFileLocked: reading a file the user still has open in Excel
            //is fine, and refusing it made the import silently do nothing.
            if (File.Exists(importPath) == false || IsFileReadable(importPath) == false)
                return elementList;

            System.Data.DataTable profileTable = Utils.LoadCSV(importPath);

            if (profileTable == null || profileTable.Columns.Count == 0)
                return elementList;

            Dictionary<string, int> columns = buildColumnMap(profileTable);

            //Nothing recognisable in the header: report every row as unread rather than hand
            //back a list of blank elements that would look like a successful import.
            if (findColumn(columns, "volume") < 0 && findColumn(columns, "materialname") < 0)
            {
                rowsSkipped = profileTable.Rows.Count;
                return elementList;
            }

            //The old template's "Level" is a name; the export's is an elevation beside a
            //separate "Level Name".
            bool levelIsElevation = findColumn(columns, "levelname") >= 0;

            foreach (DataRow dr in profileTable.Rows)
            {
                try
                {
                    CarboElement element = new CarboElement();

                    //Int64: Revit has used 64 bit element ids since 2024, and Convert.ToInt32
                    //threw an overflow on them, which dropped the row without a word.
                    element.Id = (long)Math.Round(readCsvDouble(dr, columns, "id"));

                    element.Name = readCsvText(dr, columns, "name");
                    element.Category = readCsvText(dr, columns, "category");
                    element.SubCategory = readCsvText(dr, columns, "subcategory");

                    element.MaterialName = readCsvText(dr, columns, "materialname");
                    element.CarboMaterialName = readCsvText(dr, columns, "carbomaterialname");
                    element.MaterialCategoryName = readCsvText(dr, columns, "materialclass");

                    if (levelIsElevation)
                    {
                        element.Level = readCsvDouble(dr, columns, "level");
                        element.LevelName = readCsvText(dr, columns, "levelname");
                    }
                    else
                    {
                        element.LevelName = readCsvText(dr, columns, "level");
                    }

                    //A double. Convert.ToInt32 here rounded every volume to a whole number, so
                    //2.5 m3 came in as 2 and 0.8 as 1.
                    element.Volume = readCsvDouble(dr, columns, "volume");
                    element.Volume_Total = element.Volume;

                    element.Grade = readCsvText(dr, columns, "grade");
                    element.Correction = readCsvText(dr, columns, "correction");
                    element.AdditionalData = readCsvText(dr, columns, "additionaldata", "additional");
                    element.GUID = readCsvText(dr, columns, "guid");

                    element.Area = readCsvDouble(dr, columns, "area");
                    element.rcDensity = readCsvDouble(dr, columns, "rcdensity");

                    element.isExisting = readCsvBool(dr, columns, "isexisting", false);
                    element.isDemolished = readCsvBool(dr, columns, "isdemolished", false);
                    element.isSubstructure = readCsvBool(dr, columns, "issubstructure", false);
                    element.includeInCalc = readCsvBool(dr, columns, "includeincalc", true);

                    elementList.Add(element);
                    rowsRead++;
                }
                catch (Exception ex)
                {
                    rowsSkipped++;
                }
            }

            return elementList;

        }

        /// <summary>
        /// Header name to column index, normalised so "Volume (m3)", "Volume" and "volume" are
        /// the same column.
        /// </summary>
        //Qualified: Excel interop puts a DataTable of its own in scope in this file.
        private static Dictionary<string, int> buildColumnMap(System.Data.DataTable table)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();

            for (int i = 0; i < table.Columns.Count; i++)
            {
                string key = normaliseColumnName(table.Columns[i].ColumnName);

                if (key.Length > 0 && map.ContainsKey(key) == false)
                    map.Add(key, i);
            }

            return map;
        }

        /// <summary>
        /// Lowercased, with any bracketed unit and every non alphanumeric character removed, so
        /// "Volume (m3)", "Volume" and "RC Density (kg/m3)" reduce to volume, volume, rcdensity.
        /// </summary>
        private static string normaliseColumnName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            string text = name;

            int bracket = text.IndexOf('(');
            if (bracket >= 0)
                text = text.Substring(0, bracket);

            StringBuilder builder = new StringBuilder();

            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(char.ToLowerInvariant(c));
            }

            return builder.ToString();
        }

        /// <summary>First of the given names that the file actually has, or -1.</summary>
        private static int findColumn(Dictionary<string, int> columns, params string[] names)
        {
            foreach (string name in names)
            {
                int index;
                if (columns.TryGetValue(name, out index))
                    return index;
            }

            return -1;
        }

        private static string readCsvText(DataRow dr, Dictionary<string, int> columns, params string[] names)
        {
            int index = findColumn(columns, names);

            if (index < 0 || index >= dr.Table.Columns.Count)
                return "";

            return dr[index] == null ? "" : dr[index].ToString();
        }

        private static double readCsvDouble(DataRow dr, Dictionary<string, int> columns, params string[] names)
        {
            return ReadCsvDouble(readCsvText(dr, columns, names));
        }

        /// <summary>
        /// A flag out of a csv. bool.Parse takes only "true" and "false", so a column filled in
        /// as Yes, No, 1 or 0 - which is what a spreadsheet invites - threw, and the row was
        /// dropped without a word.
        /// </summary>
        private static bool readCsvBool(DataRow dr, Dictionary<string, int> columns, string name, bool fallback)
        {
            string value = readCsvText(dr, columns, name);

            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            string text = value.Trim().ToLowerInvariant();

            if (text == "true" || text == "yes" || text == "y" || text == "1" || text == "x")
                return true;

            if (text == "false" || text == "no" || text == "n" || text == "0")
                return false;

            return fallback;
        }

        /// <summary>
        /// Basic export to oneclick
        /// </summary>
        /// <param name="carboLifeProject"></param>
        /// <param name="savePath"></param>
        /// <summary>
        /// Turns a csv this application has just written into an xlsx, through Excel, and offers
        /// to open it. Returns false and leaves the csv in place when it cannot.
        /// </summary>
        /// <remarks>
        /// One copy for the OneClick and IStructE exports, which each had their own and neither
        /// of which was safe:
        ///
        /// DisplayAlerts was left on, so saving onto an existing xlsx raised Excel's overwrite
        /// prompt inside an instance with no window - the export simply stopped, waiting on a
        /// dialog nobody could see.
        ///
        /// Nothing was released. Quit() on its own routinely leaves EXCEL.EXE resident, so every
        /// export leaked another one.
        ///
        /// Workbooks.Open reads a text file with the machine's own decimal separator, and these
        /// csv files are written invariant, so on a comma decimal machine Excel took "5.555" for
        /// text or a date. OpenText takes the separators as arguments, which is the whole reason
        /// for using it here.
        ///
        /// And the csv was deleted before anyone knew whether the conversion had worked.
        /// </remarks>
        private static bool ConvertCsvToXlsx(string csvPath, out string xlsxPath, out string message)
        {
            xlsxPath = Path.Combine(Path.GetDirectoryName(csvPath),
                                    Path.GetFileNameWithoutExtension(csvPath) + ".xlsx");
            message = "";

            if (File.Exists(xlsxPath) && IsFileLocked(xlsxPath) == true)
            {
                message = "The file " + Path.GetFileName(xlsxPath) + " is open in another program. "
                    + "Close it and export again." + Environment.NewLine + Environment.NewLine
                    + "The csv has been kept at:" + Environment.NewLine + csvPath;
                return false;
            }

            Application app = null;
            Workbooks books = null;
            Workbook wb = null;

            try
            {
                try
                {
                    app = new Application();
                }
                catch (Exception)
                {
                    //Not an IOException, which is all the old code caught, so this used to
                    //escape into the caller.
                    message = "Excel is needed to write the xlsx file and it could not be started."
                        + Environment.NewLine + Environment.NewLine
                        + "The data has been written as a csv instead:" + Environment.NewLine + csvPath;
                    return false;
                }

                app.Visible = false;
                //Without this an existing xlsx brings up the overwrite prompt in a hidden Excel.
                app.DisplayAlerts = false;
                app.ScreenUpdating = false;

                books = app.Workbooks;

                //Origin 65001 is UTF-8, matching the byte order mark WriteCVSFile writes, and the
                //separators are stated rather than taken from the machine.
                books.OpenText(csvPath,
                    65001,                          //Origin, UTF-8
                    1,                              //StartRow
                    XlTextParsingType.xlDelimited,  //DataType
                    XlTextQualifier.xlTextQualifierDoubleQuote,
                    false,                          //ConsecutiveDelimiter
                    false,                          //Tab
                    false,                          //Semicolon
                    true,                           //Comma
                    false,                          //Space
                    false,                          //Other
                    Type.Missing,                   //OtherChar
                    Type.Missing,                   //FieldInfo
                    Type.Missing,                   //TextVisualLayout
                    ".",                            //DecimalSeparator
                    ",",                            //ThousandsSeparator
                    Type.Missing,                   //TrailingMinusNumbers
                    false);                         //Local

                wb = app.ActiveWorkbook;

                if (wb == null)
                {
                    message = "Excel did not open the exported csv." + Environment.NewLine + Environment.NewLine
                        + "The data has been kept as a csv:" + Environment.NewLine + csvPath;
                    return false;
                }

                wb.SaveAs(xlsxPath, XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, Type.Missing,
                          Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing,
                          Type.Missing, Type.Missing, Type.Missing);

                return true;
            }
            catch (Exception ex)
            {
                message = "The xlsx file could not be written: " + ex.Message + Environment.NewLine + Environment.NewLine
                    + "The data has been kept as a csv:" + Environment.NewLine + csvPath;
                return false;
            }
            finally
            {
                //Closed and released in reverse order, each guarded: a throw part way through
                //must not leave Excel running with the file open.
                try { if (wb != null) wb.Close(false, Type.Missing, Type.Missing); }
                catch (Exception) { }

                try { if (app != null) app.Quit(); }
                catch (Exception) { }

                //FinalReleaseComObject, not ReleaseComObject. The latter drops one reference,
                //and a wrapper can hold more than one, which is why Excel was measured still
                //running after this block had finished. Then the collections: the wrappers are
                //finalizable, so until they have actually been finalised Excel still has a
                //client and stays resident. Two passes, because the first can queue more.
                releaseComObject(wb);
                releaseComObject(books);
                releaseComObject(app);

                wb = null;
                books = null;
                app = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static void releaseComObject(object comObject)
        {
            if (comObject == null)
                return;

            try
            {
                if (Marshal.IsComObject(comObject))
                    Marshal.FinalReleaseComObject(comObject);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Reports the outcome of a csv to xlsx conversion and offers to open the result.
        /// </summary>
        private static void ReportXlsxExport(bool converted, string csvPath, string xlsxPath, string message)
        {
            if (converted == false)
            {
                //The csv is deliberately left where it is, so the export is never a total loss.
                MessageBox.Show(message, "Exported as csv", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Only now that the xlsx exists.
            if (File.Exists(csvPath) && IsFileLocked(csvPath) == false)
            {
                try { File.Delete(csvPath); }
                catch (Exception) { }
            }

            if (File.Exists(xlsxPath) == false)
                return;

            MessageBoxResult result = MessageBox.Show("Exported Data Successfully! Press ok to open the file.",
                "Success", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = xlsxPath;
                startInfo.UseShellExecute = true;

                Process.Start(startInfo);
            }
        }

        /// <summary>
        /// One collated line of generated allowance in the OneClick export: the reinforcement,
        /// steel connection and timber connection groups are built one per parent group, so a
        /// model of any size makes dozens of near identical rows out of them.
        /// </summary>
        private sealed class CollatedAllowance
        {
            public string Category { get; set; }
            public string MaterialName { get; set; }

            /// <summary>Steel, Concrete, Timber and so on, off the assigned material.</summary>
            public string MaterialType { get; set; }
            public string Grade { get; set; }

            public double Quantity { get; set; }


            public int GroupCount { get; set; }
            public long ServiceLifeYears { get; set; }

            /// <summary>False once two of the collated groups disagree on their service life.</summary>
            public bool ServiceLifeAgrees { get; set; }

            public CollatedAllowance()
            {
                Category = "";
                MaterialName = "";
                MaterialType = "";
                Grade = "";
                Quantity = 0;
                GroupCount = 0;
                ServiceLifeYears = 0;
                ServiceLifeAgrees = true;
            }
        }

        /// <summary>
        /// Steel, Concrete, Timber and so on: the assigned material's own category, which is the
        /// vocabulary the IStructE tables shipped with this application use for material type.
        /// </summary>
        private static string getMaterialType(CarboGroup grp)
        {
            if (grp == null || grp.Material == null)
                return "";

            return grp.Material.Category;
        }

        /// <summary>
        /// The service life to declare for a group, in years.
        ///
        /// OneClick reads this as the component's own service life and works replacements out
        /// from it, so a group carrying a shorter element design life - what the calculation
        /// calls B4 - has to send that rather than the building's life, which is what every row
        /// used to carry.
        /// </summary>
        private static long getServiceLifeYears(CarboGroup grp, CarboProject carboLifeProject)
        {
            if (grp == null || grp.inUseProperties == null)
                return carboLifeProject.designLife;

            //designLifeToEnd is the "lasts as long as the building" switch and it is on by
            //default, while elementdesignlife sits at its own default of 50 until something
            //calls CarboB1B7Properties.calculate. Reading the number without checking the
            //switch therefore reported 50 years for every untouched group, whatever the
            //building's design life was.
            if (grp.inUseProperties.designLifeToEnd == true)
                return carboLifeProject.designLife;

            if (grp.inUseProperties.elementdesignlife > 0)
                return (long)Math.Round(grp.inUseProperties.elementdesignlife);

            return carboLifeProject.designLife;
        }

        public static void ExportToOneClick(CarboProject carboLifeProject, string savePath)
        {
            if (File.Exists(savePath) && IsFileLocked(savePath) == true)
                return;

            WriteCVSFile(BuildOneClickCsv(carboLifeProject), savePath);

            string xlsxPath;
            string message;
            bool converted = ConvertCsvToXlsx(savePath, out xlsxPath, out message);

            ReportXlsxExport(converted, savePath, xlsxPath, message);
        }

        /// <summary>
        /// The OneClick export as csv text, separated from the file and Excel handling so the
        /// content can be read and checked without either.
        /// </summary>
        public static string BuildOneClickCsv(CarboProject carboLifeProject)
        {
            StringBuilder fileString = new StringBuilder();

            //CLASS	IFCMATERIAL	QUANTITY	QTY_TYPE	THICKNESS_MM	TRANSPORT_KM	TRANSPORTDISTANCE_KMLEG2	YM_TRANSPORTATION_KM	COMMENT	SERVICELIFE	WASTAGE	MATERIAL REUSED	COSTPERUNIT	TOTALCOST	BREEAM Int'l Mat 01 classification (use to choose)	MAT01CLASS	BREEAM UK / RICS Classification (use to choose)	LEVEL	BYGNINGSDEL (use to choose)	BYGNINGSDEL	Talo2000 Rakennusosa (use to choose)	TALO2000	KG DIN 276 (use to choose)	KGDIN276	SFB (use to choose)	SFB	NS 3454 (use to choose)	NS3454	Level(s) - Language	Level(s) (use to choose)	CLASSIFICATION_LEVELS
            //Create Headers;
            fileString.Append(new CsvLine().AddRange(
                "CLASS",        //0
                "IFCMATERIAL",  //1
                "QUANTITY",     //2
                "QTY_TYPE",     //3
                "COMMENT",      //4
                "SERVICELIFE",  //5
                "WASTAGE"       //6
                ).ToLine());

            //The generated allowances - reinforcement, steel connections, timber connections -
            //are built one group per parent group, so a model of any size produces dozens of
            //near identical rows. They are collated into one line each here.
            //
            //Collated per allowance rather than into one line for all of them, because the three
            //generators take their material from three separate settings: merging them would put
            //rebar, connection steel and timber fixings under a single material name, and
            //OneClick prices what that name says. Keying on category and material name gives one
            //line per allowance, which is one line where there were dozens.
            Dictionary<string, CollatedAllowance> collated = new Dictionary<string, CollatedAllowance>();
            List<string> collatedOrder = new List<string>();

            //Advanced
            foreach (CarboGroup grp in carboLifeProject.getGroupList)
            {
                try
                {
                    //A group can compute to nothing - every element in it excluded, or a group
                    //of substructure with the substructure switch off - and a zero quantity row
                    //is only noise in the receiving tool.
                    if (grp.TotalVolume <= 0)
                        continue;

                    if (grp.IsAutoGenerated() == true)
                    {
                        //Keyed on category and material together. Both hold spaces, so they are
                        //carried on the accumulator rather than parsed back out of the key.
                        string key = (grp.Category ?? "") + "|" + (grp.MaterialName ?? "");

                        CollatedAllowance allowance;

                        if (collated.TryGetValue(key, out allowance) == false)
                        {
                            allowance = new CollatedAllowance();
                            allowance.Category = grp.Category;
                            allowance.MaterialName = grp.MaterialName;
                            allowance.ServiceLifeYears = getServiceLifeYears(grp, carboLifeProject);

                            collated.Add(key, allowance);
                            collatedOrder.Add(key);
                        }

                        allowance.Quantity += grp.TotalVolume;
                        allowance.GroupCount++;

                        //The collated groups need not share a service life. Where they all agree
                        //that value is reported; where they do not there is no single honest
                        //answer, so it falls back to the building's life.
                        if (allowance.ServiceLifeYears != getServiceLifeYears(grp, carboLifeProject))
                            allowance.ServiceLifeAgrees = false;

                        continue;
                    }

                    CsvLine row = new CsvLine();

                    row.Add(grp.Category);                                      //0
                    row.Add(grp.MaterialName);                                  //1
                    row.Add(grp.TotalVolume);                                   //2
                    row.Add("M3");                                              //3
                    //The plain description: the matcher's review note is an internal working
                    //annotation and this file goes to somebody else.
                    row.Add(grp.GetPlainDescription());                         //4
                    row.Add(getServiceLifeYears(grp, carboLifeProject));        //5
                    row.Add(0d);                                                //6

                    fileString.Append(row.ToLine());
                }
                catch (Exception ex)
                {
                    //Nothing in here touches a file, so the IOException this used to catch could
                    //not fire, while a null material on a group would escape the whole export.
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            //The collated allowance lines, after the groups that stand for real elements.
            foreach (string key in collatedOrder)
            {
                CollatedAllowance allowance = collated[key];

                if (allowance.Quantity <= 0)
                    continue;

                CsvLine row = new CsvLine();

                row.Add(allowance.Category);                                    //0 CLASS
                row.Add(allowance.MaterialName);                                //1 IFCMATERIAL
                row.Add(allowance.Quantity);                                    //2 QUANTITY
                row.Add("M3");                                                  //3 QTY_TYPE
                row.Add("Generated allowance, collated from "
                        + allowance.GroupCount.ToString(CultureInfo.InvariantCulture)
                        + " group(s)");                                         //4 COMMENT
                row.Add(allowance.ServiceLifeAgrees
                        ? allowance.ServiceLifeYears
                        : (long)carboLifeProject.designLife);                   //5 SERVICELIFE
                row.Add(0d);                                                    //6 WASTAGE

                fileString.Append(row.ToLine());
            }

            return fileString.ToString();
        }

        public static void ExportToIstructEClick(CarboProject carboLifeProject, string savePath)
        {
            if (File.Exists(savePath) && IsFileLocked(savePath) == true)
                return;

            carboLifeProject.CalculateProject();

            WriteCVSFile(BuildIstructECsv(carboLifeProject), savePath);

            string xlsxPath;
            string message;
            bool converted = ConvertCsvToXlsx(savePath, out xlsxPath, out message);

            ReportXlsxExport(converted, savePath, xlsxPath, message);
        }

        /// <summary>
        /// The IStructE export as csv text, separated from the file and Excel handling so the
        /// content can be read and checked without either.
        /// </summary>
        public static string BuildIstructECsv(CarboProject carboLifeProject)
        {
            StringBuilder fileString = new StringBuilder();

            //Material	Material Type	Material Specification	Structural Element	Description	Component Lifespan [years]	Aspect of Structure	Significant Temporary Works?	Number of Times Temp Works Used before EOL	Volume [m3] or Mass [kg]?	"Material Quantity

            //This file is pasted into the IStructE spreadsheet, so it is positional: the columns
            //have to be the template's columns, in the template's order, and there may not be an
            //extra one on the end. The names below are the template's own, copied from the row
            //recorded above, so a reader can line the two up before pasting.
            //
            //It used to carry a twelfth column, "Quantity Clean", holding the volume before
            //waste and the uncertainty factor. There is no twelfth column in the template, so it
            //ran into whatever sits beside it. The same figure is in the Results csv, as
            //"Base Volume" beside "Total Volume".
            //
            //Blank columns are deliberate. The ones this cannot know are left empty for the user
            //to fill in once the data is in the sheet.
            fileString.Append(new CsvLine().AddRange(
                "Material",                                     //0
                "Material Type",                                //1
                "Material Specification",                       //2
                "Structural Element",                           //3
                "Description",                                  //4
                "Component Lifespan [years]",                   //5
                "Aspect of Structure",                          //6
                "Significant Temporary Works?",                 //7
                "Number of Times Temp Works Used before EOL",   //8
                "Volume [m3] or Mass [kg]?",                    //9
                "Material Quantity"                             //10
                ).ToLine());

            //The generated allowances - reinforcement, steel connections, timber connections -
            //are built one group per parent group, so they arrive as dozens of near identical
            //rows. Collated to one line each, keyed on category and material so reinforcement,
            //the metal connection allowance and the timber connection allowance stay apart:
            //they take their material from three separate settings and the tool prices what the
            //material says.
            Dictionary<string, CollatedAllowance> collated = new Dictionary<string, CollatedAllowance>();
            List<string> collatedOrder = new List<string>();

            //Advanced
            foreach (CarboGroup grp in carboLifeProject.getGroupList)
            {
                try
                {
                    grp.CalculateTotals();

                    //A group can compute to nothing, and a zero quantity row is only noise in
                    //the receiving spreadsheet.
                    if (grp.TotalVolume <= 0)
                        continue;

                    if (grp.IsAutoGenerated() == true)
                    {
                        string key = (grp.Category ?? "") + "|" + (grp.MaterialName ?? "");

                        CollatedAllowance allowance;

                        if (collated.TryGetValue(key, out allowance) == false)
                        {
                            allowance = new CollatedAllowance();
                            allowance.Category = grp.Category;
                            allowance.MaterialName = grp.MaterialName;
                            allowance.MaterialType = getMaterialType(grp);
                            allowance.Grade = grp.Grade;
                            allowance.ServiceLifeYears = getServiceLifeYears(grp, carboLifeProject);

                            collated.Add(key, allowance);
                            collatedOrder.Add(key);
                        }

                        allowance.Quantity += grp.TotalVolume;
                        allowance.GroupCount++;

                        if (allowance.ServiceLifeYears != getServiceLifeYears(grp, carboLifeProject))
                            allowance.ServiceLifeAgrees = false;

                        continue;
                    }

                    CsvLine row = new CsvLine();

                    row.Add(grp.MaterialName);                              //0  Material
                    //Material Type and Material Specification were both left empty while the
                    //group has always carried them.
                    row.Add(getMaterialType(grp));                          //1  Material Type
                    row.Add(grp.Grade);                                     //2  Material Specification
                    row.AddEmpty();                                         //3  Structural Element
                    //The plain description: the matcher's review note is an internal working
                    //annotation and this file goes to somebody else.
                    row.Add((grp.MaterialName + " " + grp.GetPlainDescription()).Trim()); //4 Description
                    //Was the literal "60" on every row, whatever the project's design life or
                    //the group's own element design life said.
                    row.Add(getServiceLifeYears(grp, carboLifeProject));    //5  Component Lifespan
                    row.Add("New Build");                                   //6  Aspect
                    row.Add("No");                                          //7  Significant
                    row.AddEmpty();                                         //8  Number of Times Temp Works
                    row.Add("Volume [m3]");                                 //9  Volume or Mass
                    row.Add(grp.TotalVolume);                               //10 Material Quantity

                    fileString.Append(row.ToLine());
                }
                catch (Exception ex)
                {
                    //Nothing in here touches a file, so the IOException this used to catch could
                    //not fire, while a null material on a group would escape the whole export.
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            //The collated allowance lines, after the groups that stand for real elements.
            foreach (string key in collatedOrder)
            {
                CollatedAllowance allowance = collated[key];

                if (allowance.Quantity <= 0)
                    continue;

                CsvLine row = new CsvLine();

                row.Add(allowance.MaterialName);                            //0  Material
                row.Add(allowance.MaterialType);                            //1  Material Type
                row.Add(allowance.Grade);                                   //2  Material Specification
                row.AddEmpty();                                             //3  Structural Element
                row.Add(allowance.MaterialName + " " + allowance.Category
                        + ", generated allowance collated from "
                        + allowance.GroupCount.ToString(CultureInfo.InvariantCulture)
                        + " group(s)");                                     //4  Description
                row.Add(allowance.ServiceLifeAgrees
                        ? allowance.ServiceLifeYears
                        : (long)carboLifeProject.designLife);               //5  Component Lifespan
                row.Add("New Build");                                       //6  Aspect
                row.Add("No");                                              //7  Significant
                row.AddEmpty();                                             //8  Number of Times Temp Works
                row.Add("Volume [m3]");                                     //9  Volume or Mass
                row.Add(allowance.Quantity);                                //10 Material Quantity

                fileString.Append(row.ToLine());
            }

            return fileString.ToString();
        }

        public class LookupItem
        {
            public string name { get; set; }
            public double value { get; set; }

            public LookupItem()
            {
                name = "name";
                value = 0;
            }
        }

    }
}

