using CarboLifeAPI.Data;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CarboLifeAPI
{
    public static class ReportBuilder
    {
        static string report;
        static string reportpath;
        //static string imgPath;

        public static void CreateReport(CarboProject carboProject, Bitmap chart1, Bitmap chart2, Bitmap ratingChart)
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

            if (File.Exists(Path))
            {
                MessageBoxResult msgResult = System.Windows.MessageBox.Show("This file already exists, do you want to overwrite this file ?", "", MessageBoxButton.YesNo);

                if (msgResult == MessageBoxResult.Yes)
                {
                    using (var fs = File.Open(Path, FileMode.Open))
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

            chart1 = CleanBlack(chart1);
            chart2 = CleanBlack(chart2);
            ratingChart = CleanBlack(ratingChart);

            if (chart1 != null)
            {
                string piechart1_64 = ToBase64String(chart1);
                ImgTag1 = getImageTag(piechart1_64, 320 , 325, "PieChart1");
            }

            if (chart2 != null)
            {
                string piechart2_64 = ToBase64String(chart2);
                ImgTag2 = getImageTag(piechart2_64, 320, 325, "PieChart2");
            }

            if (ratingChart != null)
            {
                string ratingChart64 = ToBase64String(ratingChart);
                ImgTag3 = getImageTag(ratingChart64, 320, 300, "Rating");
            }

            //HTML WRITING;
            try
            {
                //Project Info
                report = writeHeader(carboProject);

                //Calculation Results
                report += writeCalculation(carboProject);

                //Images
                report += "<H2><B>" + "Graphs:" + "</B></H2><BR>" + System.Environment.NewLine;
                report += "<TABLE border=0 cellpadding=0 cellspacing=0 width=800>";
                report += "<TR><TD></TD></TR>";
                report += "<TR><TD>" + ImgTag1 + "</TD></TR>";
                report += "<TR><TD>" + ImgTag2 + "</TD></TR>";
                report += "<TR><TD>" + ImgTag3 + "</TD></TR>";
                report += "</TABLE>";

                //Material Quanaities
                report += writeQuantitiesTable(carboProject);

                //Project Information and base info
                report += writeReportTable(carboProject);

                //Calculation values
                report += writeMaterialTable(carboProject);

                report += closeHTML();


                if (report != "")
                {
                    using (StreamWriter sw = new StreamWriter(reportpath, false, Encoding.GetEncoding("Windows-1252")))
                    {
                        sw.WriteLine(report);
                        sw.Close();
                    }
                }

                if (File.Exists(reportpath))
                {
                    System.Windows.MessageBox.Show("Report successfully created!", "Success!", MessageBoxButton.OK);
                    System.Diagnostics.Process.Start(reportpath);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //VOID

        }

        private static string writeCalculation(CarboProject carboProject)
        {
            string html = "";

            try
            {

                List<CarboDataPoint> list = carboProject.getPhaseTotals();

                html += "<H1><B>" + "Calculation:" + "</B></H1><BR>" + System.Environment.NewLine;
                html += "<H2><B>" + "Material Based Values:" + "</B></H2><BR>" + System.Environment.NewLine;

                html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=800>";
                html += "<TR><TD width=175></TD><TD> tCO2</TD></TR>";

                foreach (CarboDataPoint cdp in list)
                {
                    //Write Material Dependent Properties:
                    if (!(cdp.Name.Contains("Global")))
                    {
                        html += "<TR><TD width=" + 150 + "><B>" + cdp.Name + "</B></TD>" + System.Environment.NewLine;
                        html += "<TD>" + Math.Round(cdp.Value / 1000,2) + " </TD></TR>" + System.Environment.NewLine;
                    }
                }

                html += "</TABLE>";

                ///Globl Values

                html += "<H2><B>" + "Global Values:" + "</B></H2><BR>" + System.Environment.NewLine;

                html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=800>";

                html += "<TR><TD width=175></TD><TD> tCO2</TD><TD></TD></TR>";

                html += "<TR><TD width=" + 150 + "><B>" + "A0:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD>" + carboProject.A0Global + " </TD><TD></TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "A5:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD>" + carboProject.A5Global + " </TD><TD></TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "C1:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD>" + carboProject.C1Global + " </TD><TD></TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "B6-B7:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD>" + carboProject.b675Global + " </TD><TD></TD></TR>" + System.Environment.NewLine;

                html += "</TABLE>";


                string Summarytext = carboProject.getGeneralText();
                Summarytext = Summarytext.Replace(System.Environment.NewLine, "<BR>"); //add a line terminating ;


                html += "<H3>" + Summarytext + "</H3>" + System.Environment.NewLine;

            }
            catch
            {
            }

            return html;
        }

        private static Bitmap CleanBlack(Bitmap BtmImg)
        {
            Bitmap result = BtmImg.Clone() as Bitmap;
            System.Drawing.Color white = System.Drawing.Color.FromArgb(255,255,255);
            System.Drawing.Color black = System.Drawing.Color.FromArgb(255, 255, 255);

            for (int x=1;x<BtmImg.Width; x++)
            {
                for (int y = 1; y < BtmImg.Height; y++)
                {
                    try
                    {
                        System.Drawing.Color clr = BtmImg.GetPixel(x, y);
                        if (clr.R == 0 && clr.G == 0 & clr.B == 0)
                            result.SetPixel(x, y, white);
                    }
                    catch(Exception ex)
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
            imgTag += " height=\"" + height + (char)34 + "/>" + System.Environment.NewLine;

            return imgTag;
        }

        private static string writeMaterialTable(CarboProject carboProject)
        {
            string html = "<H1><B>" + "Material Properties" + "</B></H1><BR>" + System.Environment.NewLine;

            html += "<TABLE border=1 cellpadding=0 cellspacing=0 >";

            html += "<TR></TR>";
            //ResultTable in a table
            try
            {

                //Write Headers:

                html += "<TR>" + System.Environment.NewLine;
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
                html += "<TR>" + System.Environment.NewLine;
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
                        html += "<TD align='left' valign='middle'>" + cbg.Material.Description + "</td>" + System.Environment.NewLine;

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
                html += "</TABLE>";
            }
            catch
            {
            }

            return html;
        }

        private static string writeQuantitiesTable(CarboProject carboProject)
        {
            string html = "<H1><B>" + "Material Quantities:" + "</B></H1><BR>" + System.Environment.NewLine;

            html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=1600>";
            html += "<TR></TR>";
            //ResultTable in a table
            try
            {

                //Write 10 Headers:
                html += "<TR>" + System.Environment.NewLine;

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
                html += "<TR>" + System.Environment.NewLine;

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


                html += "</TABLE>";


            }
            catch
            {
            }

            return html;

        }

        private static string writeReportTable(CarboProject carboProject)
        {

            string html = "<H1><B>" + "Embodied Carbon Calculation Groups:" + "</B></H1><BR>" + System.Environment.NewLine;
            
            html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=1600>";
            
            html += "<TR></TR>";
            //ResultTable in a table
            try
            {

                //Write Headers:

                html += "<TR>" + System.Environment.NewLine;
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
                html += "<TD width=" + 73 + "><B>" + "Sequestration" + "</B></TD>" + System.Environment.NewLine;


                html += "</TR>" + System.Environment.NewLine;
                //UNITS
                html += "<TR>" + System.Environment.NewLine;
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

                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Mass,2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.ECI,2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round((cbg.getVolumeECI), 2) + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.EC,2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.PerCent,2) + "</td>" + System.Environment.NewLine;

                    //Per Group
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.ECI_A1A3 * cbg.Mass,3), 2) + "</td>" + System.Environment.NewLine;
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

                html += "</TABLE>";
            }
            catch
            {
            }

            return html;
            

        }

        private static string getTotalsRow(CarboGroup totalGroup)
        {
            string html = "";

            html += "<TR>" + System.Environment.NewLine;
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

            html += "<TR>" + System.Environment.NewLine;
                html += "<TD align='left' valign='middle'><B>" + material + "</B></td>" + System.Environment.NewLine;
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
                html += "<HTML><HEAD><TITLE>Carbon Life Calculation for: " + carboProject.Name + " </TITLE>" + System.Environment.NewLine;
                //add header row

                html += getCSS();

                html += "</HEAD><BODY>";

                html += "<H1><B>" + "Project Info" + "</B></H1><BR>" + System.Environment.NewLine;

                html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=800>";
                html += "<TR></TR>";

                html += "<TR><TD width=" + 150 + "><B>" + "Name:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + carboProject.Name + "</TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Project Number:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + carboProject.Number + "</TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Description:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + "" + carboProject.Description + "</TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Category:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + carboProject.Category + "</TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Area (GIA):" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + carboProject.Area + "m<SUP>2</SUP></TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Value:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + carboProject.valueUnit + " " + carboProject.Value + "</TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Design Life:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + carboProject.designLife + "</TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Total Upfront Carbon (A0-A5):" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + Math.Round(carboProject.getUpfrontTotals() / 1000, 2) + "</TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Total Embodied Carbon (A0-C):" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + Math.Round(carboProject.getEmbodiedTotals() / 1000, 2) + "</TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Total Footprint:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + Math.Round(carboProject.getTotalEC(), 2) + "</TD></TR>" + System.Environment.NewLine;


                html += "<TR><TD width=" + 150 + "><B>" + "Total Embodied Carbon / m<SUP>2</SUP>:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + Math.Round((carboProject.getEmbodiedTotals() / carboProject.Area)) + "</TD></TR>" + System.Environment.NewLine;

                html += "<TR><TD width=" + 150 + "><B>" + "Export Date:" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 175 + ">" + DateTime.Today.ToShortDateString() + "</TD></TR>" + System.Environment.NewLine;

                html += "</TABLE>";


            }
            catch
            {
            }

            return html;
        }

        public static string getCSS()
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
            string title = "<H3>Date: " + DateTime.Today.ToShortDateString() + "</H3><BR>" + System.Environment.NewLine;

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
            catch(Exception ex)
            {
                MessageBox.Show("There was an error while creating an embedded image: " + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK);
                return "";
            }
        }

        public static string getFlattenedCalText(CarboProject carboLifeProject)
        {
            string result = "";

            result += "Total Carbon Footprint: " + carboLifeProject.getTotalEC().ToString() + " tCO₂e" + Environment.NewLine;

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
                        for (int i = 0; i < a; i++)
                        {
                            result += list1[i] + "\t" + list2[i] + Environment.NewLine;
                        }
                    }

                    //result += textGroups[2] + Environment.NewLine;

                }
            }
            catch (Exception ex)
            {
                result = "";
            }

            return result;
        }


    }


}

