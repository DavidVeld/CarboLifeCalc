using CarboLifeAPI.Data;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
            saveDialog.Title = "Specify report file location";
            saveDialog.Filter = "Excel Files|*.xls";
            saveDialog.FilterIndex = 2;
            saveDialog.RestoreDirectory = true;

            saveDialog.ShowDialog();

            string path = saveDialog.FileName;

            //Check if the file can be read and written to.
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                bool isInUse = IsFileLocked(fileInfo);

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
        private static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
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
            if(exportResults == true)
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
                    xlWorkSheet.Cells[row + 1, col + 6] = "kgCO₂e/kg";

                    xlWorkSheet.Cells[row, col + 7] = "EC";
                    xlWorkSheet.Cells[row + 1, col + 7] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 8] = "Total";
                    xlWorkSheet.Cells[row + 1, col + 8] = "%";

                    xlWorkSheet.Cells[row, col + 9] = "A1-A3";
                    xlWorkSheet.Cells[row + 1, col + 9] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 10] = "A4";
                    xlWorkSheet.Cells[row + 1, col + 10] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 11] = "A5";
                    xlWorkSheet.Cells[row + 1, col + 11] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 12] = "B1-B5";
                    xlWorkSheet.Cells[row + 1, col + 12] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 13] = "C1-C4";
                    xlWorkSheet.Cells[row + 1, col + 13] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 14] = "D";
                    xlWorkSheet.Cells[row + 1, col + 14] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 15] = "Mix";
                    xlWorkSheet.Cells[row + 1, col + 15] = "tCO₂e";

                    //Advanced
                    xlWorkSheet.Cells[row, col + 16] = "[Formula]";
                    xlWorkSheet.Cells[row + 1, col + 16] = "";

                    xlWorkSheet.Cells[row, col + 17] = "[Waste]";
                    xlWorkSheet.Cells[row + 1, col + 17] = "%";

                    xlWorkSheet.Cells[row, col + 18] = "[B4]";
                    xlWorkSheet.Cells[row + 1, col + 18] = "x";

                    xlWorkSheet.Cells[row, col + 19] = "[Additional]";
                    xlWorkSheet.Cells[row + 1, col + 19] = "kgCO₂e/kg";

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
                    xlWorkSheet2.Cells[row + 1, col + 11] = "kgCO₂e/kg";

                    xlWorkSheet2.Cells[row, col + 12] = "EC";
                    xlWorkSheet2.Cells[row + 1, col + 12] = "kgCO₂e";

                    xlWorkSheet2.Cells[row, col + 13] = "ECI Cumulative";
                    xlWorkSheet2.Cells[row + 1, col + 13] = "kgCO₂e/kg";

                    xlWorkSheet2.Cells[row, col + 14] = "EC Cumulative";
                    xlWorkSheet2.Cells[row + 1, col + 14] = "kgCO₂e";

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

                        xlWorkSheet2.Cells[row, col + 13] = el.ECI_Total;
                        xlWorkSheet2.Cells[row, col + 14] = el.EC_Total;
                        xlWorkSheet2.Cells[row, col + 15] = el.Volume_Total;
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
                    xlWorkSheet3.Cells[row + 1, col + 5] = "kgCO₂e/kg";

                    xlWorkSheet3.Cells[row, col + 6] = "ECI Volume";
                    xlWorkSheet3.Cells[row + 1, col + 6] = "kgCO₂e/n³";

                    xlWorkSheet3.Cells[row, col + 7] = "A1-A3";
                    xlWorkSheet3.Cells[row + 1, col + 7] = "kgCO₂e/kg";

                    xlWorkSheet3.Cells[row, col + 8] = "A4";
                    xlWorkSheet3.Cells[row + 1, col + 8] = "kgCO₂e/kg";

                    xlWorkSheet3.Cells[row, col + 9] = "A5";
                    xlWorkSheet3.Cells[row + 1, col + 9] = "kgCO₂e/kg";

                    xlWorkSheet3.Cells[row, col + 10] = "B1-B7";
                    xlWorkSheet3.Cells[row + 1, col + 10] = "kgCO₂e/kg";

                    xlWorkSheet3.Cells[row, col + 11] = "C1-C4";
                    xlWorkSheet3.Cells[row + 1, col + 11] = "kgCO₂e/kg";

                    xlWorkSheet3.Cells[row, col + 12] = "D";
                    xlWorkSheet3.Cells[row + 1, col + 12] = "kgCO₂e/kg";

                    xlWorkSheet3.Cells[row, col + 13] = "Mix";
                    xlWorkSheet3.Cells[row + 1, col + 13] = "kgCO₂e/kg";

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

        public static void ExportComaringGraphs(CarboProject carboLifeProject, List<CarboProject> projectListToCompareTo)
        {

            //Check if user has excel
            string path = GetSaveAsLocation();

            if (path != null)
            {
                Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                if (xlApp == null)
                {
                    System.Windows.MessageBox.Show("You need to have excel installed to continue", "Computer says no", MessageBoxButton.OK);
                    return;
                }

                if (carboLifeProject != null)
                    projectListToCompareTo.Insert(0, carboLifeProject);

                CreateTotalsExcelFile(projectListToCompareTo, xlApp, path);

                if (File.Exists(path))
                {
                    System.Windows.MessageBox.Show("Excel export succesful, click OK to open!", "Success!", MessageBoxButton.OK);
                    System.Diagnostics.Process.Start(path);
                }

            }

        }

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
                    xlWorkSheet.Cells[row + 1, col + 2] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 3] = "A1-A3";
                    xlWorkSheet.Cells[row + 1, col + 3] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 4] = "A4";
                    xlWorkSheet.Cells[row + 1, col + 4] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 5] = "A5 (Material)";
                    xlWorkSheet.Cells[row + 1, col + 5] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 6] = "A5 (Global)";
                    xlWorkSheet.Cells[row + 1, col + 6] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 7] = "B1-B7";
                    xlWorkSheet.Cells[row + 1, col + 7] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 8] = "C1-C4";
                    xlWorkSheet.Cells[row + 1, col + 8] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 9] = "C1 (Global)";
                    xlWorkSheet.Cells[row + 1, col + 9] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 10] = "D";
                    xlWorkSheet.Cells[row + 1, col + 10] = "tCO₂e";

                    xlWorkSheet.Cells[row, col + 11] = "Additional";
                    xlWorkSheet.Cells[row + 1, col + 11] = "tCO₂e";

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
            catch(Exception ex)
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

        public static void ExportToCSV(CarboProject carboLifeProject, string excelExportPath, bool exportResult, bool exportElements, bool exportMaterials)
        {
            string exportpath = Path.GetDirectoryName(excelExportPath);

            //Check if user has excel
            if (!Directory.Exists(exportpath) == true)
            {
                return;
            }

            string exportpathResult = exportpath + "\\Results.csv";
            string exportpathElements = exportpath + "\\Elements.csv";
            string exportpathMaterials = exportpath + "\\Materials.csv";

            if (File.Exists(exportpathResult) == true ||
                File.Exists(exportpathElements) == true ||
                File.Exists(exportpathMaterials) == true)
            {
                var result =  System.Windows.MessageBox.Show("Do you want to override the existing files?", "Question", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) //Anything but YES should cancel the command
                    return;
            }

            if (exportResult == false)
                exportpathResult = "";
                    
            if (exportElements == false)
                exportpathElements = "";

            if (exportMaterials == false)
                exportpathMaterials = "";

            CreateCVSFile(carboLifeProject, exportpathResult, exportpathElements, exportpathMaterials);
        }

        private static void CreateCVSFile(CarboProject carboLifeProject, string exportpathResult, string exportpathElements, string exportpathMaterials)
        {

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

