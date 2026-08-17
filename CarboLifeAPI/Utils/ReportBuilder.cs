using CarboLifeAPI.Data;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static System.Net.WebRequestMethods;

namespace CarboLifeAPI
{
    public static class ReportBuilder
    {
        static string report;
        static string reportpath;
        //static string imgPath;

        public static void CreateReport(CarboProject carboProject, string chart1, string chart2, Bitmap ratingChart)
        {
            //Create a File and save it as a HTML File
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Specify report directory";
            saveDialog.Filter = "HTML Files|*.html";
            saveDialog.FilterIndex = 2;
            saveDialog.RestoreDirectory = true;

            saveDialog.ShowDialog();

            string Path = saveDialog.FileName;
            reportpath = Path;

            if (System.IO.File.Exists(Path))
            {
                MessageBoxResult msgResult = System.Windows.MessageBox.Show("This file already exists, do you want to overwrite this file ?", "", MessageBoxButton.YesNo);

                if (msgResult == MessageBoxResult.Yes)
                {
                    using (var fs = System.IO.File.Open(Path, FileMode.Open))
                    {
                        var canRead = fs.CanRead;
                        var canWrite = fs.CanWrite;

                        if (canWrite == false)
                        {
                            System.Windows.MessageBox.Show("This file cannot be opened, please close the file and try again", "Warning", MessageBoxButton.OK);
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            else if (Path == "")
            {
                //The dialog box was canceled;
                return;
            }

            //EXPORT IMAGES HERE:
            string ImgTag1 = "";
            string ImgTag2 = "";
            string ImgTag3 = "";

            //chart1 = CleanBlack(chart1);
            //chart2 = CleanBlack(chart2);
            ratingChart = CleanBlack(ratingChart);
            ratingChart = RemoveBlackLineLeftTop(ratingChart);

            if (chart1 != null)
            {
                //string piechart1_64 = ToBase64String(chart1);
                string piechart1_64 = chart1;
                ImgTag1 = getImageTag(piechart1_64, 0, 300, "PieChart1");
            }

            if (chart2 != null)
            {
                //string piechart2_64 = ToBase64String(chart2);
                string piechart2_64 = chart2;

                ImgTag2 = getImageTag(piechart2_64, 0, 300, "PieChart2");
            }

            if (ratingChart != null)
            {
                string ratingChart64 = ToBase64String(ratingChart);
                ImgTag3 = getImageTag(ratingChart64, 850, 0, "Rating");
            }

            //HTML WRITING;
            try
            {
                //Project Info
                report = writeHeader(carboProject);

                //Images
                report += "<H1><B>" + "Graphs" + "</B></H1>" + System.Environment.NewLine;
                report += "<DIV class=\"charts\">";
                report += "<DIV class=\"chart chart-score\"><H2><B>" + "Score" + "</B></H2>" + ImgTag3 + "</DIV>";
                report += "<DIV class=\"chart-row\">";
                report += "<DIV class=\"chart\"><H2><B>" + "By Material" + "</B></H2>" + ImgTag1 + "</DIV>";
                report += "<DIV class=\"chart\"><H2><B>" + "By Phase" + "</B></H2>" + ImgTag2 + "</DIV>";
                report += "</DIV>";
                report += "</DIV>";

                //Calculation Results
                report += writeCalculation(carboProject);

                //Material Quanaities
                report += writeQuantitiesTable(carboProject);

                //Project Information and base info
                report += writeReportTable(carboProject);

                //Calculation values
                report += writeMaterialTable(carboProject);

                report += closeHTML();


                if (report != "")
                {
                    using (StreamWriter sw = new StreamWriter(reportpath, false, new UTF8Encoding(false)))
                    {
                        sw.WriteLine(report);
                        sw.Close();
                    }
                }

                if (System.IO.File.Exists(reportpath))
                {
                    var result = System.Windows.MessageBox.Show("Report successfully created! Press OK to open the report", "Success!", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = reportpath, // Path to your HTML file
                            UseShellExecute = true // This is the key part that allows it to open with the default application
                        };

                        Process.Start(startInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static string writeCalculation(CarboProject carboProject)
        {
            string html = "";

            try
            {

                List<CarboDataPoint> list = carboProject.getPhaseTotals();

                html += "<H1><B>" + "Calculation" + "</B></H1>" + System.Environment.NewLine;

                html += "<DIV class=\"values-row\">" + System.Environment.NewLine;

                //Material based values
                html += "<DIV class=\"values-col\"><H2><B>" + "Material Based Values" + "</B></H2>" + System.Environment.NewLine;

                html += "<TABLE class=\"values-table\" cellpadding=0 cellspacing=0>";
                html += "<TR class=\"hrow\"><TD>Phase</TD><TD>tCO<SUB>2</SUB>e</TD></TR>";

                foreach (CarboDataPoint cdp in list)
                {
                    //Write Material Dependent Properties:
                    if (!(cdp.Name.Contains("Global")))
                    {
                        html += "<TR><TD width=" + 150 + "><B>" + cdp.Name + "</B></TD>" + System.Environment.NewLine;
                        html += "<TD>" + Math.Round(cdp.Value / 1000, 2) + " </TD></TR>" + System.Environment.NewLine;
                    }
                }

                html += "</TABLE></DIV>" + System.Environment.NewLine;

                ///Globl Values

                html += "<DIV class=\"values-col\"><H2><B>" + "Global Values" + "</B></H2>" + System.Environment.NewLine;

                html += "<TABLE class=\"values-table\" cellpadding=0 cellspacing=0>";

                html += "<TR class=\"hrow\"><TD>Item</TD><TD>tCO<SUB>2</SUB>e</TD></TR>";

                html += "<TR><TD width=" + 150 + "><B>" + "A0:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD>" + carboProject.A0GlobalUncert + " </TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "A5:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD>" + carboProject.A5Global + " </TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "C1:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD>" + carboProject.C1Global + " </TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "B6-B7:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD>" + carboProject.b675Global + " </TD></TR>" + System.Environment.NewLine;

                html += "</TABLE></DIV>" + System.Environment.NewLine;

                html += "</DIV>" + System.Environment.NewLine;


                string Summarytext = carboProject.getGeneralText();
                Summarytext = Summarytext.Replace(System.Environment.NewLine, "<BR>"); //add a line terminating ;


                html += "<H3 class=\"summary\">" + Summarytext + "</H3>" + System.Environment.NewLine;

            }
            catch
            {
            }

            return html;
        }

        public static Bitmap CleanBlack(Bitmap BtmImg)
        {
            Bitmap result = BtmImg.Clone() as Bitmap;
            System.Drawing.Color white = System.Drawing.Color.FromArgb(255, 255, 255);
            //System.Drawing.Color black = System.Drawing.Color.FromArgb(255, 255, 255);

            for (int x = 1; x < BtmImg.Width; x++)
            {
                for (int y = 1; y < BtmImg.Height; y++)
                {
                    try
                    {
                        System.Drawing.Color clr = BtmImg.GetPixel(x, y);
                        if (clr.R == 0 && clr.G == 0 & clr.B == 0)
                            result.SetPixel(x, y, white);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return null;
                    }
                }
            }

            return result;
        }

        public static string getImageTag(string imageAsString, int width, int height, string toolText)
        {
            string imgTag = string.Empty;

            imgTag = "<img src=\"data:image/png;base64,";
            imgTag += imageAsString + "\" ";
            //imgTag += " width=\"" + width.ToString() + (char)34;
            //imgTag += " height=\"" + height.ToString() + (char)34 + "/>" + System.Environment.NewLine;

            if (height > 0)
            {
                imgTag += " height=\"" + height + (char)34 + "/>" + System.Environment.NewLine;
            }
            else
            {
                imgTag += " width=\"" + width + (char)34 + "/>" + System.Environment.NewLine;

            }
            return imgTag;
        }

        private static string writeMaterialTable(CarboProject carboProject)
        {
            string html = "<H1><B>" + "Material Properties" + "</B></H1>" + System.Environment.NewLine;

            html += "<DIV class=\"table-wrap\"><TABLE class=\"data-table\" cellpadding=0 cellspacing=0>";

            //ResultTable in a table
            try
            {

                //Write Headers:

                html += "<TR class=\"hrow\">" + System.Environment.NewLine;
                html += "<TD width=" + 175 + "><B>" + "Material" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 100 + "><B>" + "Category" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 200 + "><B>" + "Description" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 93.75 + "><B>" + "Density" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 93.75 + "><B>" + "ECI" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 93.75 + "><B>" + "ECI" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 93.75 + "><B>" + "A1-A3" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 93.75 + "><B>" + "A4" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 93.75 + "><B>" + "A5" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 93.75 + "><B>" + "B1-B7" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 93.75 + "><B>" + "C1-C4" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 93.75 + "><B>" + "D" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 93.75 + "><B>" + "Mix" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 93.75 + "><B>" + "Sequestration" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 93.75 + "><B>" + "B4" + "</B></TD>" + System.Environment.NewLine;



                html += "</TR>" + System.Environment.NewLine;
                //UNITS
                html += "<TR class=\"urow\">" + System.Environment.NewLine;
                html += "<TD><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD><B>" + "kg/m<SUP>3</SUP>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "kgCo<SUB>2</SUB>/m<SUP>3</SUP>" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD><B>" + "" + "</B></TD>" + System.Environment.NewLine;


                html += "</TR>" + System.Environment.NewLine;

                //Write Data:

                ObservableCollection<CarboGroup> cglist = carboProject.getGroupList;
                cglist = new ObservableCollection<CarboGroup>(cglist.OrderBy(i => i.MaterialName));

                string material = "";

                foreach (CarboGroup cbg in cglist)
                {
                    if (cbg.MaterialName != material)
                    {
                        material = cbg.MaterialName;

                        html += "<TR>" + System.Environment.NewLine;

                        html += "<TD align='left' valign='middle'>" + cbg.Material.Name + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + cbg.Material.Category + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + cbg.Material.Description + Environment.NewLine + cbg.Material.EPDurl + "</td>" + System.Environment.NewLine;

                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Density, 2) + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.ECI, 2) + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.getVolumeECI, 2) + "</td>" + System.Environment.NewLine;

                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Material.ECI_A1A3, 2) + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Material.ECI_A4, 2) + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Material.ECI_A5, 2) + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Material.ECI_B1B5, 2) + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Material.ECI_C1C4, 2) + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Material.ECI_D, 2) + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Material.ECI_Mix, 2) + "</td>" + System.Environment.NewLine;
                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Material.ECI_Seq, 2) + "</td>" + System.Environment.NewLine;

                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.inUseProperties.B4, 2) + "</td>" + System.Environment.NewLine;


                        html += "</TR>" + System.Environment.NewLine;
                    }
                }
                html += "</TABLE></DIV>";
            }
            catch
            {
            }

            return html;
        }

        private static string writeQuantitiesTable(CarboProject carboProject)
        {
            string html = "<H1><B>" + "Material Quantities" + "</B></H1>" + System.Environment.NewLine;

            html += "<DIV class=\"table-wrap\"><TABLE class=\"data-table\" cellpadding=0 cellspacing=0>";
            //ResultTable in a table
            try
            {

                //Write 10 Headers:
                html += "<TR class=\"hrow\">" + System.Environment.NewLine;

                html += "<TD width=" + 150 + "><B>" + "Category" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + "><B>" + "Material" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + "><B>" + "Description" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 40 + "><B>" + "Volume" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 40 + "><B>" + "Correction Formula" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 40 + "><B>" + "Waste" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 40 + "><B>" + "[B4]" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 40 + "><B>" + "Total Volume" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 40 + "><B>" + "Density" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 40 + "><B>" + "Mass" + "</B></TD>" + System.Environment.NewLine;

                html += "</TR>" + System.Environment.NewLine;

                //Write 10 units
                html += "<TR class=\"urow\">" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "m<SUP>3</SUP>" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "%" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "x" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "m<SUP>3</SUP>" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "kg/m<SUP>3</SUP>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "kg" + "</B></TD>" + System.Environment.NewLine;

                html += "</TR>" + System.Environment.NewLine;

                ObservableCollection<CarboGroup> cglist = carboProject.getGroupList;
                cglist = new ObservableCollection<CarboGroup>(cglist.OrderBy(i => i.MaterialName));

                string material = "";

                foreach (CarboGroup cbg in cglist)
                {
                    //If this is the first instance of a group, then write the title of the material
                    if (cbg.MaterialName != material)
                    {
                        material = cbg.MaterialName;
                        html += getTitleRow(material);
                    }

                    html += "<TR>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + cbg.Category + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cbg.Material.Name + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cbg.Description + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Volume, 2) + "</td>" + System.Environment.NewLine;

                    //Advanced settings
                    html += "<TD align='left' valign='middle'>" + cbg.Correction + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cbg.Waste + "%" + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.inUseProperties.B4, 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.TotalVolume, 2) + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + cbg.Density + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Mass, 2) + "</td>" + System.Environment.NewLine;

                    html += "</TR>" + System.Environment.NewLine;
                }


                html += "</TABLE></DIV>";


            }
            catch
            {
            }

            return html;

        }

        private static string writeReportTable(CarboProject carboProject)
        {

            string html = "<H1><B>" + "Embodied Carbon Calculation Groups" + "</B></H1>" + System.Environment.NewLine;

            html += "<DIV class=\"table-wrap\"><TABLE class=\"data-table\" cellpadding=0 cellspacing=0>";

            //ResultTable in a table
            try
            {

                //Write Headers:

                html += "<TR class=\"hrow\">" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Category" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 250 + "><B>" + "Material" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 225 + "><B>" + "Description" + "</B></TD>" + System.Environment.NewLine;

                //Advanced settings
                html += "<TD width=" + 73 + "><B>" + "Correction Formula" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "Waste" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "Added" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "[B4]" + "</B></TD>" + System.Environment.NewLine;


                html += "<TD width=" + 73 + "><B>" + "Total Volume" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "Density" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "Mass" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 73 + "><B>" + "ECI" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "ECI" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 73 + "><B>" + "EC" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 73 + "><B>" + "Total" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 73 + "><B>" + "A1-A3" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "A4" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "A5" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "B1-B7" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 73 + "><B>" + "C1-C4" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "D" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "Mix" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 73 + "><B>" + "Seqstr." + "</B></TD>" + System.Environment.NewLine;


                html += "</TR>" + System.Environment.NewLine;
                //UNITS
                html += "<TR class=\"urow\">" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;

                //Advanced settings
                html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "%" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "kgCO<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "m<SUP>3</SUP>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "kg/m<SUP>3</SUP>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "kg" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "kgCO<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "kgCO<SUB>2</SUB>/m<SUP>3</SUP>" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "tCO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "%" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "CO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "CO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "CO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "CO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "CO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "CO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "CO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "CO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;

                html += "</TR>" + System.Environment.NewLine;

                //Write Data:

                ObservableCollection<CarboGroup> cglist = carboProject.getGroupList;
                cglist = new ObservableCollection<CarboGroup>(cglist.OrderBy(i => i.MaterialName));

                string material = "";

                foreach (CarboGroup cbg in cglist)
                {
                    if (cbg.MaterialName != material)
                    {
                        material = cbg.MaterialName;
                        html += getTitleRow(material);
                    }

                    html += "<TR>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + cbg.Category + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cbg.Material.Name + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cbg.Description + "</td>" + System.Environment.NewLine;

                    //Advanced settings
                    html += "<TD align='left' valign='middle'>" + cbg.Correction + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cbg.Waste + "%" + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Additional, 2) + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.inUseProperties.B4, 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.TotalVolume, 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cbg.Density + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Mass, 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.ECI, 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round((cbg.getVolumeECI), 2) + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.EC, 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.PerCent, 2) + "</td>" + System.Environment.NewLine;

                    //Per Group
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.ECI_A1A3 * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.ECI_A4 * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.ECI_A5 * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.ECI_B1B5 * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.ECI_C1C4 * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.ECI_D * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.ECI_Mix * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.ECI_Seq * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;

                    html += "</TR>" + System.Environment.NewLine;

                }
                html += getTotalsRow(carboProject.getTotalsGroup());

                html += "</TABLE></DIV>";
            }
            catch
            {
            }

            return html;


        }

        private static string getTotalsRow(CarboGroup totalGroup)
        {
            string html = "";

            html += "<TR class=\"totals\">" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "TOTAL" + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;

            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;

            html += "</TR>" + System.Environment.NewLine;

            return html;
        }

        private static string getTitleRow(string material)
        {
            string html = "";

            html += "<TR class=\"group\">" + System.Environment.NewLine;
            html += "<TD colspan=\"24\"><B>" + material + "</B></TD>" + System.Environment.NewLine;
            html += "</TR>" + System.Environment.NewLine;
            return html;

        }

        private static string errorReport(Exception ex)
        {
            string html = "";
            html = "error" + ex.Message;
            return html;
        }

        internal static string writeHeader(CarboProject carboProject)
        {
            string html = "";

            try
            {
                string exportDate = DateTime.Today.ToShortDateString();

                html += "<HTML><HEAD><META charset=\"utf-8\">";
                html += "<TITLE>Carbo Life Calc : Embodied Carbon Calculation for: " + carboProject.Name + " </TITLE>" + System.Environment.NewLine;

                html += getCSS();

                html += "</HEAD><BODY>";

                //Document header: eyebrow, title and one-line meta strip
                html += "<DIV class=\"doc-header\">" + System.Environment.NewLine;
                html += "<DIV class=\"eyebrow\">Embodied Carbon Calculation</DIV>" + System.Environment.NewLine;
                html += "<H1 class=\"doc-title\"><B>" + carboProject.Name + "</B></H1>" + System.Environment.NewLine;

                html += "<DIV class=\"doc-meta\">";
                if (!string.IsNullOrWhiteSpace(carboProject.Number))
                    html += "<span>" + carboProject.Number + "</span>";
                if (!string.IsNullOrWhiteSpace(carboProject.Category))
                    html += "<span>" + carboProject.Category + "</span>";
                html += "<span>GIA " + Fmt(carboProject.Area, 2) + " m<SUP>2</SUP></span>";
                html += "<span>" + exportDate + "</span>";
                html += "</DIV>" + System.Environment.NewLine;
                html += "</DIV>" + System.Environment.NewLine;

                //Headline figures
                double upfront = carboProject.getUpfrontTotals();
                double embodied = carboProject.getEmbodiedTotals();
                double area = carboProject.Area;

                html += "<DIV class=\"kpi-band\">";
                html += getKpi("Upfront Carbon&nbsp;A0-A5", Fmt(upfront / 1000, 2), "tCO<SUB>2</SUB>e");
                html += getKpi("Embodied Carbon&nbsp;A0-C", Fmt(embodied / 1000, 2), "tCO<SUB>2</SUB>e");
                html += getKpi("Upfront Intensity", area > 0 ? Fmt(upfront / area, 0) : "-", "kgCO<SUB>2</SUB>e/m<SUP>2</SUP>");
                html += getKpi("Embodied Intensity", area > 0 ? Fmt(embodied / area, 0) : "-", "kgCO<SUB>2</SUB>e/m<SUP>2</SUP>");
                html += "</DIV>" + System.Environment.NewLine;

                //Project information
                html += "<H1><B>" + "Project Info" + "</B></H1>" + System.Environment.NewLine;

                html += "<DIV class=\"info-grid\">" + System.Environment.NewLine;

                html += getInfoItem("Name", carboProject.Name);
                html += getInfoItem("Project Number", carboProject.Number);
                html += getInfoItem("Description", carboProject.Description);
                html += getInfoItem("Category", carboProject.Category);
                html += getInfoItem("Area (GIA)", Fmt(carboProject.Area, 2) + " m<SUP>2</SUP>");
                html += getInfoItem("Design Life", carboProject.designLife.ToString());

                if (carboProject.CarboDatabase.templateName != "")
                    html += getInfoItem("Template", carboProject.CarboDatabase.templateName);

                html += getInfoItem("Total Footprint", Fmt(carboProject.getTotalEC(), 2) + " tCO<SUB>2</SUB>e");
                html += getInfoItem("Export Date", exportDate);

                html += "</DIV>" + System.Environment.NewLine;
            }
            catch
            {
            }

            return html;
        }

        private static string getKpi(string label, string value, string unit)
        {
            string html = "<DIV class=\"kpi\">";
            html += "<DIV class=\"kpi-label\">" + label + "</DIV>";
            html += "<DIV class=\"kpi-value\">" + value + "<span class=\"kpi-unit\">" + unit + "</span></DIV>";
            html += "</DIV>";

            return html;
        }

        private static string getInfoItem(string label, string value)
        {
            string html = "<DIV class=\"info-item\">";
            html += "<DIV class=\"info-label\">" + label + "</DIV>";
            html += "<DIV class=\"info-value\">" + value + "</DIV>";
            html += "</DIV>" + System.Environment.NewLine;

            return html;
        }

        /// <summary>
        /// Formats a number with thousand separators so the headline figures stay readable.
        /// </summary>
        private static string Fmt(double value, int decimals)
        {
            return value.ToString("N" + decimals);
        }

        public static string getCSS()
        {
            string html = @"
<style type=""text/css"">

:root { --coral: #B21616; }

* { box-sizing: border-box; }

body {
  font-family: ""Segoe UI"", ""Helvetica Neue"", Arial, sans-serif;
  background-color: #ffffff;
  color: #1a1a1a;
  margin: 0;
  padding: 44px 52px 64px 52px;
  font-size: 13px;
  line-height: 1.5;
  -webkit-font-smoothing: antialiased;
  font-variant-numeric: tabular-nums;
}

a { color: var(--coral); text-decoration: none; }
a:hover { text-decoration: underline; }

/* ---------- document header ---------- */

.doc-header { margin: 0 0 32px 0; }

.eyebrow {
  font-size: 10.5px;
  font-weight: 600;
  letter-spacing: 0.2em;
  text-transform: uppercase;
  color: #6a6a6a;
  margin: 0 0 12px 0;
}

h1.doc-title {
  font-size: 34px;
  font-weight: 300;
  color: var(--coral);
  letter-spacing: -0.005em;
  line-height: 1.15;
  text-transform: none;
  margin: 0;
  padding: 0;
  border: none;
}

.doc-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 6px 16px;
  margin: 14px 0 0 0;
  padding: 13px 0 0 0;
  border-top: 1px solid #e6e6e6;
  font-size: 12px;
  color: #707070;
}

.doc-meta span { white-space: nowrap; }
.doc-meta span + span::before { content: ""\00b7""; color: #c8c8c8; margin-right: 16px; }

/* ---------- headline figures ---------- */

.kpi-band {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  border-top: 2px solid var(--coral);
  border-bottom: 1px solid #e6e6e6;
  margin: 0 0 44px 0;
}

.kpi {
  display: flex;
  flex-direction: column;
  padding: 18px 26px 20px 26px;
  border-left: 1px solid #ededed;
}
.kpi:first-child { border-left: none; padding-left: 0; }

.kpi-label {
  min-height: 2.5em;
  font-size: 10.5px;
  font-weight: 600;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: #6a6a6a;
}

.kpi-value {
  font-size: 29px;
  font-weight: 600;
  color: #111111;
  line-height: 1;
  margin: auto 0 0 0;
  white-space: nowrap;
}

.kpi-unit { font-size: 11.5px; font-weight: 400; color: #6a6a6a; margin-left: 6px; }

/* ---------- section headings ---------- */

h1 {
  font-size: 16px;
  font-weight: 300;
  color: var(--coral);
  letter-spacing: 0.06em;
  text-transform: uppercase;
  margin: 54px 0 20px 0;
  padding: 0 0 9px 0;
  border-bottom: 1px solid #e6e6e6;
}

h2 {
  font-size: 11px;
  font-weight: 600;
  color: var(--coral);
  letter-spacing: 0.13em;
  text-transform: uppercase;
  margin: 28px 0 10px 0;
}

h1 b, h2 b { font-weight: inherit; }

h3 {
  font-size: 13px;
  font-weight: 400;
  line-height: 1.85;
  color: #3d3d3d;
  max-width: 800px;
}

/* ---------- project information ---------- */

.info-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 18px 44px;
  margin: 0 0 8px 0;
  max-width: 1180px;
}

.info-item { padding: 0 0 13px 0; border-bottom: 1px solid #f0f0f0; }

.info-label {
  font-size: 10.5px;
  font-weight: 600;
  letter-spacing: 0.13em;
  text-transform: uppercase;
  color: #6a6a6a;
  margin: 0 0 6px 0;
}

.info-value { font-size: 14.5px; color: #111111; }

/* ---------- charts ---------- */

.charts { display: flex; flex-direction: column; gap: 30px; margin: 0 0 8px 0; }

.chart-score { max-width: 50%; }

.chart-row { display: flex; gap: 44px; align-items: flex-start; max-width: 62.5%; }
.chart-row .chart { flex: 1 1 0; min-width: 0; }

.charts h2 { margin: 0 0 10px 0; }
.charts img { display: block; width: 100%; height: auto; max-width: 100%; margin: 0; }

img { display: block; max-width: 100%; height: auto; }

/* ---------- tables ---------- */

table { border-collapse: collapse; background-color: transparent; }

td {
  padding: 6px 11px;
  border: none;
  border-bottom: 1px solid #f2f2f2;
  vertical-align: middle;
  color: #1a1a1a;
}

/* ---------- phase / global value tables ---------- */

.values-row { display: flex; flex-wrap: wrap; gap: 24px 64px; margin: 0; }
.values-col { flex: 0 1 330px; }

table.values-table { width: 100%; margin: 0 0 20px 0; font-size: 13px; }

.values-table td {
  padding: 7px 0;
  border: none;
  border-bottom: 1px solid #f0f0f0;
  white-space: nowrap;
  color: #1a1a1a;
}

.values-table td b { font-weight: 400; color: #333333; }
.values-table td:last-child { text-align: right; }

.values-table tr.hrow td {
  font-size: 10.5px;
  font-weight: 600;
  letter-spacing: 0.13em;
  text-transform: uppercase;
  color: #6a6a6a;
  padding-bottom: 9px;
  border-bottom: 1px solid #d4d4d4;
}

/* ---------- summary narrative ---------- */

h3.summary {
  font-size: 13px;
  font-weight: 400;
  line-height: 1.95;
  color: #3d3d3d;
  max-width: 800px;
  margin: 30px 0 8px 0;
  padding: 22px 0 24px 0;
  border-top: 1px solid #e6e6e6;
  border-bottom: 1px solid #e6e6e6;
}

/* ---------- data tables ---------- */

.table-wrap { overflow-x: auto; margin: 0 0 8px 0; padding: 0 0 4px 0; }

table.data-table { width: 100%; font-size: 11px; }

.data-table td {
  padding: 6px 11px;
  border: none;
  border-bottom: 1px solid #f2f2f2;
  vertical-align: middle;
  white-space: nowrap;
  color: #1a1a1a;
}

.data-table td:first-child { padding-left: 0; }
.data-table td:nth-child(n+4) { text-align: right; }
.data-table td:nth-child(3) { white-space: normal; min-width: 165px; color: #5a5a5a; }

.data-table tr:hover td { background-color: #fafafa; }

.data-table tr.hrow td,
.data-table tr.urow td { background-color: #ffffff; }

.data-table tr.hrow td {
  font-size: 10.5px;
  font-weight: 600;
  letter-spacing: 0.07em;
  text-transform: uppercase;
  color: #5f5f5f;
  white-space: normal;
  padding: 0 11px 6px 11px;
  border-bottom: 1px solid #cfcfcf;
}

.data-table tr.urow td {
  font-size: 10.5px;
  font-weight: 400;
  color: #6e6e6e;
  padding: 0 11px 8px 11px;
  border-bottom: 1px solid #dcdcdc;
}

.data-table tr.hrow td:first-child,
.data-table tr.urow td:first-child { padding-left: 0; }

.data-table tr.urow td b { font-weight: 400; }

.data-table tr.group td {
  font-size: 11.5px;
  font-weight: 600;
  color: var(--coral);
  text-align: left;
  white-space: nowrap;
  padding: 20px 0 7px 0;
  border-bottom: 1px solid #e6e6e6;
  background-color: #ffffff;
}

.data-table tr.totals td {
  font-size: 11.5px;
  font-weight: 600;
  color: #111111;
  padding-top: 10px;
  border-top: 1px solid #cfcfcf;
  border-bottom: none;
  background-color: #ffffff;
}

/* ---------- footer ---------- */

.doc-footer {
  margin: 56px 0 0 0;
  padding: 16px 0 0 0;
  border-top: 1px solid #e6e6e6;
  font-size: 11px;
  line-height: 1.85;
  color: #6a6a6a;
}

.doc-footer a { color: #6a6a6a; border-bottom: 1px solid #d8d8d8; }
.doc-footer a:hover { color: var(--coral); border-bottom-color: var(--coral); text-decoration: none; }

/* ---------- print ---------- */

@media print {
  @page { size: A4 landscape; margin: 12mm; }
  body { padding: 0; font-size: 11px; }
  .table-wrap { overflow: visible; }
  table.data-table { font-size: 8px; }
  .data-table td { padding: 3px 6px; }
  .data-table tr:hover td { background-color: transparent; }
  h1 { margin-top: 30px; page-break-after: avoid; }
  h2 { page-break-after: avoid; }
  .doc-header, .kpi-band { page-break-after: avoid; }
  tr, .kpi-band, .charts, .chart-row, .info-item { page-break-inside: avoid; }
  .doc-footer { page-break-before: avoid; }
}

</style>
";


            return html;


        }

        [Obsolete]
        public static string getCSS_old()
        {
            string html = "";

            html += "<STYLE type=\"text/css\">" + System.Environment.NewLine;

            html += "table {" + System.Environment.NewLine +
            "font-family:Segoe UI;" + System.Environment.NewLine +
            "font-size:14px; " + System.Environment.NewLine +
            "margin-left:20px;" + System.Environment.NewLine +
            "border-bottom: 1px solid #D3D3D3;" + System.Environment.NewLine +
            "border-top: 1px solid #D3D3D3; " + System.Environment.NewLine +
            "border-left: none;" + System.Environment.NewLine +
            "border-right: none;" + System.Environment.NewLine +
            "border-collapse: collapse;" + System.Environment.NewLine +
            "}" + System.Environment.NewLine;

            html += "td {font-family:Segoe UI;" + System.Environment.NewLine +
                        "color:#000;" + System.Environment.NewLine +
                        "font-size:14px;" + System.Environment.NewLine +
                        "background:#fff;" + System.Environment.NewLine +
                        "margin:12px;}" + System.Environment.NewLine +
                         System.Environment.NewLine;

            html += "h1 {font-family:Segoe UI;" + System.Environment.NewLine +
                        "color:#000;" + System.Environment.NewLine +
                        "font-size:36px;" + System.Environment.NewLine +
                        "text-shadow: 1px 1px 0px #fff;" + System.Environment.NewLine +
                        "margin-left:20px;" + System.Environment.NewLine +
                        "border:#000 0px solid; }" + System.Environment.NewLine +
                        System.Environment.NewLine;

            html += "h2 {font-family:Segoe UI;" + System.Environment.NewLine +
                        "color:#000;" + System.Environment.NewLine +
                        "font-size:14px;" + System.Environment.NewLine +
                        "margin-left:16px;" + System.Environment.NewLine +
                        "border:#000 0px solid; }" + System.Environment.NewLine +
                        System.Environment.NewLine;

            html += "h3 {font-family:Segoe UI;" +
                        "color:#000;" +
                        "font-size:16px;" +
                        "text-shadow: 1px 1px 0px #fff;" +
                        "margin-left:20px;" +
                        "border:#000 0px solid; }" +
                        System.Environment.NewLine;

            html += "</STYLE>" + System.Environment.NewLine;


            return html;


        }

        public static string closeHTML()
        {
            string html = "";
            html += "<DIV class=\"doc-footer\">Report Generated on: " + DateTime.Today.ToShortDateString() + "<BR>" + System.Environment.NewLine;
            html += "Report Generated by: " + "Carbo Life Calculator Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "<BR>" + System.Environment.NewLine;
            html += @"<A href=""https://github.com/DavidVeld/CarboLifeCalc"">https://github.com/DavidVeld/CarboLifeCalc</A></DIV>" + System.Environment.NewLine;

            try
            {
                //End HTML File
                html += "</BODY></HTML>";
            }
            catch
            {
            }

            return html;
        }
        //Helpers:

        public static string ToBase64String(this Bitmap bmp)
        {
            try
            {
                string base64String = string.Empty;

                MemoryStream memoryStream = new MemoryStream();
                bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                memoryStream.Position = 0;
                byte[] byteBuffer = memoryStream.ToArray();

                memoryStream.Close();

                base64String = Convert.ToBase64String(byteBuffer);
                byteBuffer = null;

                return base64String;
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error while creating an embedded image: " + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK);
                return "";
            }
        }

        public static string getFlattenedCalText(CarboProject carboLifeProject)
        {
            string result = "";

            result += "Total Carbon Footprint: " + Math.Round(carboLifeProject.getTotalEC(), 0).ToString() + " tCO₂e" + Environment.NewLine;

            List<string> textGroups = carboLifeProject.getCalcText();

            //Merge first string with second;
            try
            {
                if (textGroups.Count == 2)
                {
                    string[] list1 = textGroups[0].Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    string[] list2 = textGroups[1].Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    int a = list1.Length;
                    int b = list2.Length;

                    if (a == b)
                    {
                        // Find the longest heading to determine padding width
                        int maxLength = list1.Max(s => s.Length);

                        for (int i = 0; i < a; i++)
                        {
                            // Pad each heading so all values start in the same column
                            result += list1[i].PadRight(maxLength + 4) + list2[i] + Environment.NewLine;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                result = "";
            }

            return result;
        }

        public static System.Drawing.Image base64ToImage(string base64CartesianChart1)
        {

            //data:image/gif;base64,
            //this image is a single pixel (black)
            byte[] bytes = Convert.FromBase64String(base64CartesianChart1);

            System.Drawing.Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = System.Drawing.Image.FromStream(ms);
            }

            return image;

        }

        public static Bitmap RemoveBlackLineLeftTop(Bitmap letiChart)
        {
            if (letiChart == null)
                throw new ArgumentNullException(nameof(letiChart));

            System.Drawing.Color white = System.Drawing.Color.White;

            // Top 3 rows
            for (int y = 0; y < 3 && y < letiChart.Height; y++)
            {
                for (int x = 0; x < letiChart.Width; x++)
                {
                    letiChart.SetPixel(x, y, white);
                }
            }

            // Left 3 columns
            for (int x = 0; x < 3 && x < letiChart.Width; x++)
            {
                for (int y = 0; y < letiChart.Height; y++)
                {
                    letiChart.SetPixel(x, y, white);
                }
            }

            return letiChart;
        }

    }


}

