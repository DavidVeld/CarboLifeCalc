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
            //Create a File and save it as a HTML File
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Open CSV File";
            openDialog.Filter = "csv file |*.csv";
            openDialog.FilterIndex = 2;
            openDialog.RestoreDirectory = true;

            openDialog.ShowDialog();

            string path = openDialog.FileName;

            //Check if the file can be read and written to.
            if (File.Exists(path))
            {
                //File Should exist
                bool isInUse = IsFileLocked(path);

                if (isInUse == true)
                    return null;
            }
            else
            {
                //File doesnt Exist
                if (path != "")
                    return null;
            }


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
                        tEC += cbg.EC;
                        tA1 += (cbg.Material.ECI_A1A3 * cbg.Mass);
                        tA4 += (cbg.Material.ECI_A4 * cbg.Mass);
                        tA5 += (cbg.Material.ECI_A5 * cbg.Mass);
                        tB += (cbg.Material.ECI_B1B5 * cbg.Mass);
                        tC += (cbg.Material.ECI_C1C4 * cbg.Mass);
                        tD += (cbg.Material.ECI_D * cbg.Mass);
                        tM += (cbg.Material.ECI_Mix * cbg.Mass);
                        tS += (cbg.Material.ECI_Seq * cbg.Mass);
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
                row.Add(carboLifeProject.A0GlobalUncert);           //7
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
                        totalEC += cbg.EC;
                        totalA1 += (cbg.Material.ECI_A1A3 * cbg.Mass);
                        totalA4 += (cbg.Material.ECI_A4 * cbg.Mass);
                        totalA5 += (cbg.Material.ECI_A5 * cbg.Mass);
                        totalB += (cbg.Material.ECI_B1B5 * cbg.Mass);
                        totalC += (cbg.Material.ECI_C1C4 * cbg.Mass);
                        totalD += (cbg.Material.ECI_D * cbg.Mass);
                        totalM += (cbg.Material.ECI_Mix * cbg.Mass);
                        totalS += (cbg.Material.ECI_Seq * cbg.Mass);
                    }


                    CsvLine row = new CsvLine();

                    row.Add(carboLifeProject.Name);             //0
                    row.Add(carboLifeProject.Number);           //1
                    row.Add(carboLifeProject.Category);         //2
                    row.Add(carboLifeProject.Description);      //3
                    row.Add(carboLifeProject.SocialCost);       //4
                    row.Add(carboLifeProject.Area);             //5
                    row.Add(carboLifeProject.AreaNew);          //6

                    row.Add(carboLifeProject.A0GlobalUncert);   //7
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
                "[Additional] (tCO2e/kg)",  //7
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

                    row.Add((grp.Material.ECI_A1A3 * grp.Mass) / 1000);   //14
                    row.Add((grp.Material.ECI_A4 * grp.Mass) / 1000);     //15
                    row.Add((grp.Material.ECI_A5 * grp.Mass) / 1000);     //16
                    row.Add((grp.Material.ECI_B1B5) / 1000);              //17
                    row.Add((grp.Material.ECI_C1C4 * grp.Mass) / 1000);   //18
                    row.Add((grp.Material.ECI_D * grp.Mass) / 1000);      //19
                    row.Add((grp.Material.ECI_Seq * grp.Mass) / 1000);    //20
                    row.Add((grp.Material.ECI_Mix * grp.Mass) / 1000);    //21

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
            fileString.Append(new CsvLine().AddRange(
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
                "GUID"                          //34
                ).ToLine());

        IList<CarboElement> elementList = carboLifeProject.getElementsFromGroups().ToList();


            foreach (CarboElement el in elementList)
            {
                //Argument 2 is the Revit MaterialClass. el.Category is the Revit ELEMENT category
                //("Walls", "Structural Framing"), which is a different concept and used to poison
                //the match here.
                CarboMaterial material = carboLifeProject.CarboDatabase.getClosestMatch(el.CarboMaterialName, el.MaterialCategoryName, el.Grade);

                //Individual Totals Elements
                double mass = el.Mass;
                if (mass == 0)
                    mass = el.Volume_Total * el.Density;

                //The matcher can come back empty. Every impact figure below reads off it, so it
                //used to take the whole export down with a null reference, on a background
                //thread where nothing was watching. The element keeps its row and its geometry,
                //and the figures that need a material are left at zero.
                double density = el.Density;
                string grade = el.Grade;
                double ecA1A3 = 0, ecA4 = 0, ecA5 = 0, ecB1B5 = 0, ecC1C4 = 0, ecD = 0, ecMix = 0, ecSeq = 0;

                if (material != null)
                {
                    density = material.Density;
                    grade = material.Grade;

                    ecA1A3 = mass * material.ECI_A1A3;
                    ecA4 = mass * material.ECI_A4;
                    ecA5 = mass * material.ECI_A5;
                    ecB1B5 = mass * material.ECI_B1B5;
                    ecC1C4 = mass * material.ECI_C1C4;
                    ecD = mass * material.ECI_D;
                    ecMix = mass * material.ECI_Mix;
                    ecSeq = mass * material.ECI_Seq;
                }

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

                row.Add(density);                   //11
                row.Add(el.Mass);                   //12
                row.Add(grade);                     //13

                row.Add(el.ECI);                    //14
                row.Add(el.ECI_Cumulative);         //15
                row.Add(el.EC);                     //16
                row.Add(el.EC_Cumulative);          //17

                row.Add(el.isExisting);             //18
                row.Add(el.isDemolished);           //19
                row.Add(el.isSubstructure);         //20
                row.Add(el.includeInCalc);          //21

                row.Add(el.AdditionalData);         //22

                row.Add(ecA1A3);                    //23
                row.Add(ecA4);                      //24
                row.Add(ecA5);                      //25
                row.Add(ecB1B5);                    //26
                row.Add(ecC1C4);                    //27
                row.Add(ecD);                       //28
                row.Add(ecMix);                     //29
                row.Add(ecSeq);                     //30

                row.Add(el.Correction);             //31
                row.Add(el.rcDensity);              //32
                row.Add(el.Area);                   //33
                row.Add(el.GUID);                   //34

                fileString.Append(row.ToLine());
            }

            foreach (CarboGroup grp in carboLifeProject.getGroupList)
            {
                if (grp.AllElements.Count == 0)
                {
                    string materialName = grp.Material != null ? grp.Material.Name : "";

                    CsvLine row = new CsvLine();

                    row.Add(grp.Id);                //0
                    row.Add(grp.Category);          //1
                    row.Add(grp.Description);       //2
                    row.Add(grp.SubCategory);       //3
                    row.Add(materialName);          //4
                    row.Add(materialName);          //5
                    row.AddEmpty();                 //6  Level
                    row.AddEmpty();                 //7  Level Name

                    row.Add(grp.Volume);            //8
                    row.Add(grp.TotalVolume);       //9
                    row.Add(grp.TotalVolume);       //10

                    row.Add(grp.Density);           //11
                    row.Add(grp.Mass);              //12
                    row.Add(grp.Grade);             //13

                    row.Add(grp.ECI);               //14
                    row.Add(grp.ECI);               //15
                    row.Add(grp.EC);                //16
                    row.Add(grp.EC);                //17

                    row.Add(grp.isExisting);        //18
                    row.Add(grp.isDemolished);      //19
                    row.Add(grp.isSubstructure);    //20
                    row.Add(true);                  //21 includeInCalc

                    row.Add(grp.additionalData);    //22

                    //Individual Totals Elements
                    row.Add(grp.getTotalA1A3);      //23
                    row.Add(grp.getTotalA4);        //24
                    row.Add(grp.getTotalA5);        //25
                    row.Add(grp.getTotalB1B7);      //26
                    row.Add(grp.getTotalC1C4);      //27
                    row.Add(grp.getTotalD);         //28
                    row.Add(grp.getTotalMix);       //29
                    row.Add(grp.getTotalSeq);       //30

                    row.Add(grp.Correction);        //31
                    row.Add(grp.RcDensity);         //32
                    row.Add(0d);                    //33 Area
                    row.AddEmpty();                 //34 GUID, these rows are a group not an element

                    fileString.Append(row.ToLine());
                }
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
        internal sealed class CsvLine
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

            public CsvLine Add(double value)
            {
                return AppendField(value.ToString(CultureInfo.InvariantCulture));
            }

            public CsvLine Add(double value, int decimals)
            {
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
            List<CarboMaterial> cmList = new List<CarboMaterial>();

            if (File.Exists(importPath) && IsFileLocked(importPath) == false)
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
                    }
                    catch(Exception ex)
                    { }
                }
            }

            return cmList;
        }

        public static bool CreateElementImportTemplate(string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return false;

            StringBuilder fileString = new StringBuilder();

            //Create Headers;
            fileString.Append(new CsvLine().AddRange(
                "Id",               //0
                "Name",             //1
                "Category",         //2
                "MaterialName",     //3
                "Volume",           //4
                "IsSubstructure",   //5
                "Level",            //6
                "AdditionalData"    //7
                ).ToLine());

            fileString.Append(new CsvLine().AddRange(
                "999999",           //0
                "Example Element",  //1
                "Floor",            //2
                "Concrete",         //3
                "100",              //4
                "FALSE",            //5
                "Level 01",         //6
                "Transfer slab"     //7
                ).ToLine());

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
            List<CarboElement> elementList = new List<CarboElement>();

            if (File.Exists(importPath) && IsFileLocked(importPath) == false)
            {
                System.Data.DataTable profileTable = Utils.LoadCSV(importPath);

                foreach (DataRow dr in profileTable.Rows)
                {
                    try
                    {
                        CarboElement element = new CarboElement();

                        element.Id = Convert.ToInt32(ReadCsvDouble(dr[0].ToString()));
                        element.Name = dr[1].ToString();
                        element.Category = dr[2].ToString();
                        element.MaterialName = dr[3].ToString();
                        element.Volume = Convert.ToInt32(ReadCsvDouble(dr[4].ToString()));
                        element.isSubstructure = bool.Parse((dr[5].ToString()));
                        element.LevelName = dr[6].ToString();
                        element.AdditionalData = dr[7].ToString();
                        elementList.Add(element);
                    }
                    catch(Exception ex)
                    {

                    }
                }
            }
            return elementList;

        }

        /// <summary>
        /// Basic export to oneclick
        /// </summary>
        /// <param name="carboLifeProject"></param>
        /// <param name="savePath"></param>
        public static void ExportToOneClick(CarboProject carboLifeProject, string savePath)
        {
            if (File.Exists(savePath) && IsFileLocked(savePath) == true)
                return;

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

            //Advanced
            foreach (CarboGroup grp in carboLifeProject.getGroupList)
            {
                try
                {
                    CsvLine row = new CsvLine();

                    row.Add(grp.Category);                  //0
                    row.Add(grp.MaterialName);              //1
                    row.Add(grp.TotalVolume);               //2
                    row.Add("M3");                          //3
                    row.Add(grp.Description);               //4
                    row.Add(carboLifeProject.designLife);   //5
                    row.Add(0d);                            //6

                    fileString.Append(row.ToLine());
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            WriteCVSFile(fileString.ToString(), savePath);
            string targetPath = Path.GetDirectoryName(savePath);
            string filename = Path.GetFileNameWithoutExtension(savePath);

            try
            {
                string targetPathFull = targetPath + "\\" + filename + ".xlsx";

                Application app = new Application();
                Workbook wb = app.Workbooks.Open(savePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                wb.SaveAs(targetPath + "\\" + filename + ".xlsx", XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                wb.Close();
                app.Quit();
                if(File.Exists(savePath) && DataExportUtils.IsFileLocked(savePath) == false)
                    File.Delete(savePath);

                if (File.Exists(targetPathFull))
                {
                    var result = MessageBox.Show("Exported Data Successfully! Press ok to open the file.", "Success", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = targetPathFull, 
                            UseShellExecute = true 
                        };
                        Process.Start(startInfo);
                    }
                }


            }
            catch (IOException ex)
            {
                Console.WriteLine("An error occurred while writing the file: " + ex.Message);
            }
        }

        public static void ExportToIstructEClick(CarboProject carboLifeProject, string savePath)
        {
            if (File.Exists(savePath) && IsFileLocked(savePath) == true)
                return;

            StringBuilder fileString = new StringBuilder();

            //Material	Material Type	Material Specification	Structural Element	Description	Component Lifespan [years]	Aspect of Structure	Significant Temporary Works?	Number of Times Temp Works Used before EOL	Volume [m3] or Mass [kg]?	"Material Quantity
            carboLifeProject.CalculateProject();

            //Create Headers;
            fileString.Append(new CsvLine().AddRange(
                "Material",                     //0
                "Material Type",                //1
                "Material Specification",       //2
                "Structural Element",           //3
                "Description",                  //4
                "Component Lifespan",           //5
                "Aspect",                       //6
                "Significant",                  //7
                "Number of Times Temp Works",   //8
                "Volume or Mass",               //9
                "Quantity",                     //10
                "Quantity Clean"                //11
                ).ToLine());

            //Advanced
            foreach (CarboGroup grp in carboLifeProject.getGroupList)
            {
                try
                {
                    grp.CalculateTotals();

                    CsvLine row = new CsvLine();

                    row.Add(grp.MaterialName);                              //0
                    row.AddEmpty();                                         //1
                    row.AddEmpty();                                         //2
                    row.AddEmpty();                                         //3
                    row.Add(grp.MaterialName + " " + grp.Description);      //4
                    row.Add("60");                                          //5
                    row.Add("New Build");                                   //6
                    row.Add("No");                                          //7
                    row.AddEmpty();                                         //8
                    row.Add("Volume [m3]");                                 //9
                    row.Add(grp.TotalVolume);                               //10
                    row.Add(grp.Volume);                                    //11

                    fileString.Append(row.ToLine());
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            WriteCVSFile(fileString.ToString(), savePath);
            string targetPath = Path.GetDirectoryName(savePath);
            string filename = Path.GetFileNameWithoutExtension(savePath);

            try
            {
                string targetPathFull = targetPath + "\\" + filename + ".xlsx";
                Application app = new Application();
                Workbook wb = app.Workbooks.Open(savePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                wb.SaveAs(targetPathFull, XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                wb.Close();
                app.Quit();

                if (File.Exists(savePath) && DataExportUtils.IsFileLocked(savePath) == false)
                    File.Delete(savePath);

                if (File.Exists(targetPathFull))
                {
                    var result = MessageBox.Show("Exported Successfully! Press ok to open the file.","Success",MessageBoxButton.OKCancel);
                    if(result == MessageBoxResult.OK)
                    {
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = targetPathFull, // Path to your HTML file
                                UseShellExecute = true // This is the key part that allows it to open with the default application
                            };
                        Process.Start(startInfo);
                    }
                }

            }
            catch (IOException ex)
            {
                MessageBox.Show("An error occurred while writing the file: " + ex.Message);
            }
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

