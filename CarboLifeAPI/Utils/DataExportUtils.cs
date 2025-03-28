using CarboLifeAPI.Data;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

            //All is ok
            return false;
        }
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

                    CreateProjectTotalsCVSFile(listToExport, path, carboLifeProject);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

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
        private static void CreateProjectDatabaseCVSFile(CarboProject carboLifeProject, string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            string fileString = "";



            //Create Headers;
            fileString =
                "Name" + "," + //0
                "Number" + "," + //1
                "Category" + "," + //2
                "Description" + "," + //3
                "SocialCost" + "," + //4
                "Area" + "," + //5
                "AreaNew" + "," + //6

                "A0Global" + "," + //7
                "A5Global" + "," + //8
                "b675Global" + "," + //9
                "C1Global" + "," + //10

                "valueUnit" + "," + //11
                "designLife" + "," + //12
                "story" + "," + //13

                Environment.NewLine;
            //Advanced

                try
                {
                    string resultString = "";

                    resultString += carboLifeProject.Name + ","; //1
                    resultString += carboLifeProject.Number + ","; //2
                    resultString += carboLifeProject.Category + ","; //3
                    resultString += carboLifeProject.Description + ","; //3
                    resultString += carboLifeProject.SocialCost + ","; //4
                    resultString += carboLifeProject.Area + ","; //5
                    resultString += carboLifeProject.AreaNew + ","; //6
                    resultString += carboLifeProject.A0Global + ","; //7
                    resultString += carboLifeProject.A5Global + ","; //8
                    resultString += carboLifeProject.b675Global + ","; //9
                    resultString += carboLifeProject.C1Global + ","; //10

                    resultString += carboLifeProject.valueUnit + ","; //11
                    resultString += carboLifeProject.designLife + ","; //12

                resultString += CVSFormat(carboLifeProject.getGeneralText()) + ","; //13


                resultString += Environment.NewLine;

                    fileString += resultString;
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            

            WriteCVSFile(fileString, exportPath);

        }
        private static void CreateProjectTotalsCVSFile(List<CarboProject> projectListToCompareTo, string exportPath, CarboProject baseProject)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            string fileString = "";

            //Create Headers;
            fileString =
                "Name" + "," + //0
                "Number" + "," + //1
                "Category" + "," + //2
                "Description" + "," + //3
                "SocialCost" + "," + //4
                "Area" + "," + //5
                "AreaNew" + "," + //6

                "A0Global" + "," + //7
                "A5Global" + "," + //8
                "b675Global" + "," + //9
                "C1Global" + "," + //10

                "TotalEC" + "," + //11
                "Total_A1A3" + "," + //12
                "Total_A4" + "," + //13
                "Total_A5" + "," + //14
                "Total_B" + "," + //15
                "Total_C1C4" + "," + //16
                "Total_D" + "," + //17
                "Total_Mix" + "," + //18
                "Total_Seq" + "," + //19

                "valueUnit" + "," + //20
                "designLife" + "," + //21
                "story" + "," + //22

                Environment.NewLine;
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


                    string resultString = "";

                    resultString += CVSFormat(carboLifeProject.Name) + ","; //1
                    resultString += carboLifeProject.Number + ","; //2
                    resultString += CVSFormat(carboLifeProject.Category) + ","; //3
                    resultString += CVSFormat(carboLifeProject.Description) + ","; //3
                    resultString += carboLifeProject.SocialCost + ","; //4
                    resultString += carboLifeProject.Area + ","; //5
                    resultString += carboLifeProject.AreaNew + ","; //6

                    resultString += carboLifeProject.A0Global + ","; //7
                    resultString += carboLifeProject.A5Global + ","; //8
                    resultString += carboLifeProject.b675Global + ","; //9
                    resultString += carboLifeProject.C1Global + ","; //10

                    resultString += totalEC.ToString() + ","; //11
                    resultString += Math.Round(totalA1/1000, 3).ToString() + ","; //12
                    resultString += Math.Round(totalA4 / 1000, 3).ToString() + ","; //13
                    resultString += Math.Round(totalA5 / 1000, 3).ToString() + ","; //14
                    resultString += Math.Round(totalB / 1000, 3).ToString() + ","; //15
                    resultString += Math.Round(totalC / 1000, 3).ToString() + ","; //16
                    resultString += Math.Round(totalD / 1000, 3).ToString() + ","; //17
                    resultString += Math.Round(totalM / 1000, 3).ToString() + ","; //18
                    resultString += Math.Round(totalS / 1000, 3).ToString() + ","; //19

                    resultString += carboLifeProject.valueUnit + ","; //20
                    resultString += carboLifeProject.designLife + ","; //21
                    resultString += CVSFormat(carboLifeProject.getGeneralText()) + ","; //22

                    resultString += Environment.NewLine;

                    fileString += resultString;
                }
                catch (IOException ex)
                {
                    
                }
            }

            WriteCVSFile(fileString, exportPath);
            MessageBox.Show("Export File Created at: " + exportPath);
        }
        private static void CreateResultsCVSFile(CarboProject carboLifeProject, string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            string fileString = "";

            /*
             * 
             */


            //Create Headers;
            fileString =
                "Category" + "," + //0
                "Material" + "," + //1
                "Description" + "," + //2
                "Base Volume" + "," + //3
                "[Formula]" + "," + //4
                "[Waste] (%)" + "," + //5
                "[B4] (x)" + "," + //6
                "[Additional] (tCO2e/kg)" + "," + //7
                "Total Volume" + "," + //8
                "Density (kg/m³)" + "," + //9
                "Mass (kg)" + "," + //10

                "ECI (kgCO2e/kg)" + "," + //11
                "EC (tCO2e)" + "," + //12
                "Total (%)" + "," + //13

                "A1-A3 (tCO2e)" + "," + //14
                "A4 (tCO2e)" + "," + //15
                "A5 (tCO2e)" + "," + //16
                "B1-B5 (tCO2e)" + "," + //17
                "C1-C4 (tCO2e)" + "," + //18
                "D (tCO2e)" + "," + //19
                "Sequestration (tCO2e)" + "," + //20
                "Additional (tCO2e)" + //21
                Environment.NewLine;
            //Advanced
            foreach (CarboGroup grp in carboLifeProject.getGroupList)
            {
                try
                {
                    string resultString = "";

                    resultString += CVSFormat(grp.Category) + ","; //1
                    resultString += CVSFormat(grp.MaterialName) + ","; //2
                    resultString += CVSFormat(grp.Description) + ","; //3
                    resultString += grp.Volume + ","; //3
                    resultString += CVSFormat(grp.Correction) + ","; //4
                    resultString += grp.Waste + ","; //5
                    resultString += grp.inUseProperties.B4 + ","; //6
                    resultString += grp.Additional + ","; //7
                    resultString += grp.TotalVolume + ","; //8
                    resultString += grp.Density + ","; //9
                    resultString += grp.Mass + ","; //10

                    resultString += grp.ECI + ","; //11
                    resultString += grp.EC + ","; //12
                    resultString += grp.PerCent + ","; //13

                    resultString += ((grp.Material.ECI_A1A3 * grp.Mass)) / 1000 + ","; //14
                    resultString += ((grp.Material.ECI_A4 * grp.Mass)) / 1000 + ","; //15
                    resultString += ((grp.Material.ECI_A5 * grp.Mass)) / 1000 + ","; //16
                    resultString += ((grp.Material.ECI_B1B5)) / 1000 + ","; //17
                    resultString += ((grp.Material.ECI_C1C4 * grp.Mass)) / 1000 + ","; //18
                    resultString += ((grp.Material.ECI_D * grp.Mass)) / 1000 + ","; //19
                    resultString += ((grp.Material.ECI_Seq * grp.Mass)) / 1000 + ","; //20
                    resultString += ((grp.Material.ECI_Mix * grp.Mass)) / 1000 + ","; //21

                    resultString += Environment.NewLine;

                    fileString += resultString;
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            WriteCVSFile(fileString, exportPath);

        }
        private static void CreateElementsCVSFile(CarboProject carboLifeProject, string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            string fileString = "";


            //Create Headers;
            fileString =
                "Id" + "," + //0
                "Category" + "," + //1
                "Name" + "," + //2
                "SubCategory" + "," + //3

                "Material Name" + "," + //4
                "Carbo Material Name" + "," + //5
                "Level" + "," + //6
                "Level Name" + "," + //7

                "Volume (m3)" + "," + //7.1
                "Volume Total (m3)" + "," + //7.2
                "Volume Cumulative (m3)" + "," + //7.3

                "Density (kg/m3)" + "," + //7.4
                "Mass (kg)" + "," + //8
                "Grade" + ","+  //8.1

                "ECI (kgCO2e/kg)" + "," + //9
                "ECI Cumulative (kgCO2e/kg)" + "," + //10
                "EC (kgCO2e)" + "," + //11
                "EC Cumulative (kgCO2e)" + "," + //12

                "isExisting" + "," + //13
                "isDemolished" + "," + //14
                "isSubstructure" + "," + //15
                "includeInCalc" + "," + //16
                "Additional" + "," + //17
                
                "EC A1A3 (kgCO2e)" + "," + //18
                "EC A4 (kgCO2e)" + "," + //19
                "EC A5 (kgCO2e)" + "," + //20
                "EC B1B7 (kgCO2e)" + "," + //21
                "EC C1C4 (kgCO2e)" + "," + //22
                "EC D (kgCO2e)" + "," + //23
                "EC Misc (kgCO2e)" + "," + //24
                "EC Sequestration (kgCO2e)" + "," + //25

                "Correction" + "," + //26
                "RC Density (kg/m3)" + "," + //27
                "Area (m2)" + "," + //28
                "GUID" + //29


        Environment.NewLine;

        IList<CarboElement> elementList = carboLifeProject.getElementsFromGroups().ToList();


            foreach (CarboElement el in elementList)
            {
                string resultString = "";
                CarboMaterial material = carboLifeProject.CarboDatabase.getClosestMatch(el.CarboMaterialName, el.Category);

                resultString += el.Id + ","; //0
                resultString += CVSFormat(el.Category) + ","; //1
                resultString += CVSFormat(el.Name) + ","; //2
                resultString += CVSFormat(el.SubCategory) + ","; //3
                resultString += CVSFormat(el.MaterialName) + ","; //4
                resultString += CVSFormat(el.CarboMaterialName) + ","; //5
                resultString += el.Level + ","; //6
                resultString += el.LevelName + ","; //7

                resultString += el.Volume + ","; //7.1
                resultString += el.Volume_Total + ","; //7.2
                resultString += el.Volume_Cumulative + ","; //7.3

                resultString += material.Density + ","; //7.4
                resultString += el.Mass + ","; //8
                resultString += material.Grade + ","; //8

                resultString += el.ECI + ","; //9
                resultString += el.ECI_Cumulative + ","; //10
                resultString += el.EC + ","; //11
                resultString += el.EC_Cumulative + ","; //12

                resultString += el.isExisting + ","; //13
                resultString += el.isDemolished + ","; //14
                resultString += el.isSubstructure + ","; //15
                resultString += el.includeInCalc + ","; //16

                resultString += CVSFormat(el.AdditionalData) + ","; //17

                //Individual Totals Elements
                double mass = el.Mass;
                if (mass == 0)
                    mass = el.Volume_Total * el.Density;

                resultString += mass * material.ECI_A1A3 + ","; //18
                resultString += mass * material.ECI_A4 + ","; //19
                resultString += mass * material.ECI_A5 + ","; //20
                resultString += mass * material.ECI_B1B5 + ","; //21
                resultString += mass * material.ECI_C1C4 + ","; //22
                resultString += mass * material.ECI_D + ","; //23
                resultString += mass * material.ECI_Mix + ","; //24
                resultString += mass * material.ECI_Seq + ","; //25

                resultString += el.Correction + ","; //26
                resultString += el.rcDensity + ","; //27
                resultString += el.Area + ","; //28
                resultString += el.GUID + ","; //28

                resultString += Environment.NewLine; //enter

                fileString += resultString;

            }

            foreach (CarboGroup grp in carboLifeProject.getGroupList)
            {
                if (grp.AllElements.Count == 0)
                {
                    string resultString = "";

                    resultString += grp.Id + ","; //0
                    resultString += CVSFormat(grp.Category) + ","; //1
                    resultString += CVSFormat(grp.Description) + ","; //2
                    resultString += CVSFormat(grp.SubCategory) + ","; //3
                    resultString += CVSFormat(grp.Material.Name) + ","; //4
                    resultString += CVSFormat(grp.Material.Name) + ","; //5
                    resultString += "" + ","; //6
                    resultString += "" + ","; //7

                    resultString += grp.Volume + ","; //7.1
                    resultString += grp.TotalVolume + ","; //7.2
                    resultString += grp.TotalVolume + ","; //7.3

                    resultString += grp.Density + ","; //7.4
                    resultString += grp.Mass + ","; //8
                    resultString += grp.Grade + ","; //8.1

                    resultString += grp.ECI + ","; //9
                    resultString += grp.ECI + ","; //10
                    resultString += grp.EC + ","; //11
                    resultString += grp.EC + ","; //12

                    resultString += grp.isExisting + ","; //13
                    resultString += grp.isDemolished + ","; //14
                    resultString += grp.isSubstructure + ","; //15
                    resultString += "True" + ","; //16

                    resultString += CVSFormat(grp.additionalData) + ","; //17

                    //Individual Totals Elements

                    resultString += grp.getTotalA1A3 + ","; //18
                    resultString += grp.getTotalA4 + ","; //19
                    resultString += grp.getTotalA5 + ","; //20
                    resultString += grp.getTotalB1B7 + ","; //21
                    resultString += grp.getTotalC1C4 + ","; //22
                    resultString += grp.getTotalD + ","; //23
                    resultString += grp.getTotalMix + ","; //24
                    resultString += grp.getTotalSeq + ","; //25

                    resultString += grp.Correction + ","; //26
                    resultString += grp.RcDensity + ","; //27
                    resultString += "0" + ","; //28 (Area)

                    resultString += Environment.NewLine; //enter

                    fileString += resultString;
                }
            }

            WriteCVSFile(fileString, exportPath);

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

        [Obsolete("Replaced by CreateMaterialDatabaseCSVFile")]
        private static void CreateMaterialsCVSFile(CarboProject carboLifeProject, string exportPath)
        {
            if (File.Exists(exportPath) && IsFileLocked(exportPath) == true)
                return;

            string fileString = "";

            //Create Headers;
            fileString =
                "Id" + "," + //0
                "Name" + "," + //1
                "Category" + "," + //2
                "Description" + "," + //3
                "Density" + "," + //4
                "ECI (kgCO2e/kg)" + "," + //5
                "ECI Volume (kgCO2e/m³)" + "," + //6

                "A1-A3 (kgCO2e/kg)" + "," + //7
                "A4 (kgCO2e/kg)" + "," + //8
                "A5 (kgCO2e/kg)" + "," + //9
                "B1-B7 (kgCO2e/kg)" + "," + //10
                "C1-C4 (kgCO2e/kg)" + "," + //11
                "D (tCO2e)" + "," + //12
                "Sequestration (kgCO2e/kg)" + "," + //13
                "Additional (kgCO2e/kg)" + "," + //14

                "Default Waste (%)" + "," + //15

                Environment.NewLine;

            ObservableCollection<CarboGroup> cglist = carboLifeProject.getGroupList;
            cglist = new ObservableCollection<CarboGroup>(cglist.OrderBy(i => i.MaterialName));

            string material = "";

            foreach (CarboGroup cbg in cglist)
            {
                if (cbg.MaterialName != material)
                {
                    string resultString = "";

                    resultString += cbg.Material.Id + ","; //1
                    resultString += CVSFormat(cbg.Material.Name) + ","; //2
                    resultString += CVSFormat(cbg.Material.Category) + ","; //3
                    resultString += CVSFormat(cbg.Material.Description) + ","; //3
                    resultString += cbg.Material.Density + ","; //4
                    resultString += cbg.Material.ECI + ","; //5
                    resultString += cbg.Material.getVolumeECI + ","; //6

                    resultString += cbg.Material.ECI_A1A3 + ","; //7
                    resultString += cbg.Material.ECI_A4 + ","; //8
                    resultString += cbg.Material.ECI_A5 + ","; //9
                    resultString += cbg.Material.ECI_B1B5 + ","; //10
                    resultString += cbg.Material.ECI_C1C4 + ","; //11
                    resultString += cbg.Material.ECI_D + ","; //12
                    resultString += cbg.Material.ECI_Seq + ","; //13
                    resultString += cbg.Material.ECI_Mix + ","; //14

                    resultString += cbg.Material.WasteFactor + ","; //14

                    resultString += Environment.NewLine; //enter

                    fileString += resultString;
                }
            }

            WriteCVSFile(fileString, exportPath);
        }

        public static void WriteCVSFile(string fileString, string exportPath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(exportPath))
                {
                    // Write header row
                    writer.WriteLine(fileString);
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
                            "materials with identical ID's will be ovewritten. " +
                            "New Ids will be imported as a new material" + Environment.NewLine; */

            string fileString = "";
//Create Headers;
fileString =
                "Id" + "," + //0
                "Name" + "," + //1
                "Category" + "," + //2
                "Description" + "," + //3
                "Density" + "," + //4
                "WasteFactor" + "," + //5
                "Grade" + "," + //5.1
                "EPDURL" + "," + //5.2

                "ECI (kgCO2e/kg)" + "," + //6
                "ECI_A1A3 (kgCO2e/kg)" + "," + //7
                "ECI_A4 (kgCO2e/kg)" + "," + //8
                "ECI_A5 (kgCO2e/kg)" + "," + //9
                "ECI_B1B5 (kgCO2e/kg)" + "," + //10
                "ECI_C1C4 (kgCO2e/kg)" + "," + //11
                "ECI_D (kgCO2e/kg)" + "," + //12
                "ECI_Seq (kgCO2e/kg)" + "," + //13
                "ECI_Mix (kgCO2e/kg)" + "," + //14

                Environment.NewLine;
            //Advanced
            foreach (CarboMaterial cm in materialDataBase.CarboMaterialList)
            {
                try
                {
                    string resultString = "";

                    resultString += cm.Id + ","; //1
                    resultString += CVSFormat(cm.Name) + ","; //2
                    resultString += CVSFormat(cm.Category) + ","; //3
                    resultString += CVSFormat(cm.Description) + ","; //3
                    resultString += cm.Density + ","; //4
                    resultString += cm.WasteFactor + ","; //5
                    resultString += cm.Grade + ","; //5.1
                    resultString += cm.EPDurl + ","; //5.2

                    resultString += cm.ECI + ","; //6
                    resultString += cm.ECI_A1A3 + ","; //7
                    resultString += cm.ECI_A4 + ","; //8
                    resultString += cm.ECI_A5 + ","; //9
                    resultString += cm.ECI_B1B5 + ","; //10
                    resultString += cm.ECI_C1C4 + ","; //11
                    resultString += cm.ECI_D + ","; //12
                    resultString += cm.ECI_Seq + ","; //13
                    resultString += cm.ECI_Mix + ","; //14

                    resultString += Environment.NewLine;

                    fileString += resultString;
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            try
            {
                WriteCVSFile(fileString, exportPath);
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
                        cm.Id = Convert.ToInt32(Utils.ConvertMeToDouble(dr[0].ToString()));
                        cm.Name = dr[1].ToString();
                        cm.Category = dr[2].ToString();
                        cm.Description = dr[3].ToString();

                        cm.Density = Convert.ToInt32(Utils.ConvertMeToDouble(dr[4].ToString()));
                        cm.WasteFactor = Utils.ConvertMeToDouble(dr[5].ToString());
                        cm.Grade = dr[6].ToString();
                        cm.EPDurl = dr[7].ToString();

                        cm.ECI = Utils.ConvertMeToDouble(dr[8].ToString());

                        cm.ECI_A1A3_Override = true;
                        cm.ECI_A4_Override = true;
                        cm.ECI_A5_Override = true;
                        cm.ECI_C1C4_Override = true;
                        cm.ECI_D_Override = true;
                        cm.ECI_Seq_Override = true;


                        cm.ECI_A1A3 = Utils.ConvertMeToDouble(dr[9].ToString());
                        cm.ECI_A4 = Utils.ConvertMeToDouble(dr[10].ToString());
                        cm.ECI_A5 = Utils.ConvertMeToDouble(dr[11].ToString());
                        cm.ECI_B1B5 = Utils.ConvertMeToDouble(dr[12].ToString());
                        cm.ECI_C1C4 = Utils.ConvertMeToDouble(dr[13].ToString());
                        cm.ECI_D = Utils.ConvertMeToDouble(dr[14].ToString());
                        cm.ECI_Seq = Utils.ConvertMeToDouble(dr[15].ToString());
                        cm.ECI_Mix = Utils.ConvertMeToDouble(dr[16].ToString());



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

            string fileString = "";
            //Create Headers;
            fileString =
                            "Id" + "," + //0
                            "Name" + "," + //1
                            "Category" + "," + //2
                            "MaterialName" + "," + //3
                            "Volume" + "," + //4
                            "IsSubstructure" + "," + //5
                            "Level" + "," + //6
                            "AdditionalData" + "," + //7
                            Environment.NewLine;
                    string resultString =
                                    "999999" + "," + //0
                                    "Example Element" + "," + //1
                                    "Floor" + "," + //2
                                    "Concrete" + "," + //3
                                    "100" + "," + //4
                                    "FALSE" + "," + //5
                                    "Level 01" + "," + //6
                                    "Transfer slab" + "," + //7
                                    Environment.NewLine;

                    fileString += resultString;

            try
            {
                WriteCVSFile(fileString, exportPath);
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

                        element.Id = Convert.ToInt32(Utils.ConvertMeToDouble(dr[0].ToString()));
                        element.Name = dr[1].ToString();
                        element.Category = dr[2].ToString();
                        element.MaterialName = dr[3].ToString();
                        element.Volume = Convert.ToInt32(Utils.ConvertMeToDouble(dr[4].ToString()));
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

            string fileString = "";

            //CLASS	IFCMATERIAL	QUANTITY	QTY_TYPE	THICKNESS_MM	TRANSPORT_KM	TRANSPORTDISTANCE_KMLEG2	YM_TRANSPORTATION_KM	COMMENT	SERVICELIFE	WASTAGE	MATERIAL REUSED	COSTPERUNIT	TOTALCOST	BREEAM Int'l Mat 01 classification (use to choose)	MAT01CLASS	BREEAM UK / RICS Classification (use to choose)	LEVEL	BYGNINGSDEL (use to choose)	BYGNINGSDEL	Talo2000 Rakennusosa (use to choose)	TALO2000	KG DIN 276 (use to choose)	KGDIN276	SFB (use to choose)	SFB	NS 3454 (use to choose)	NS3454	Level(s) - Language	Level(s) (use to choose)	CLASSIFICATION_LEVELS
            //Create Headers;
            fileString =
                "CLASS" + "," + //0
                "IFCMATERIAL" + "," + //1
                "QUANTITY" + "," + //2
                "QTY_TYPE" + "," + //3
                "COMMENT" + "," + //4
                "SERVICELIFE" + "," + //5
                "WASTAGE" + "," + //6
                Environment.NewLine;
            //Advanced
            foreach (CarboGroup grp in carboLifeProject.getGroupList)
            {
                try
                {
                    string resultString = "";
                    resultString += CVSFormat(grp.Category) + ","; //0
                    resultString += CVSFormat(grp.MaterialName) + ","; //1
                    resultString += grp.TotalVolume + ","; //2
                    resultString += CVSFormat("M3") + ","; //3
                    resultString += CVSFormat(grp.Description) + ","; //4
                    resultString += carboLifeProject.designLife + ","; //5
                    resultString += 0; //6
                    resultString += Environment.NewLine;

                    fileString += resultString;
                }
                catch (IOException ex)
                {
                    Console.WriteLine("An error occurred while writing the file: " + ex.Message);
                }
            }

            WriteCVSFile(fileString, savePath);
            string targetPath = Path.GetDirectoryName(savePath);
            string filename = Path.GetFileNameWithoutExtension(savePath);

            try
            {
                Application app = new Application();
                Workbook wb = app.Workbooks.Open(savePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                wb.SaveAs(targetPath + "\\" + filename + ".xlsx", XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                wb.Close();
                app.Quit();
            }
            catch (IOException ex)
            {
                Console.WriteLine("An error occurred while writing the file: " + ex.Message);
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

