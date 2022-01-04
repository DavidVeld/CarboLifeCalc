using CarboLifeAPI.Data;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

            if (chart1 != null)
            {
                string piechart1_64 = ToBase64String(chart1);
                ImgTag1 = getImageTag(piechart1_64, chart1.Width, chart1.Height, "PieChart1");
            }

            if (chart2 != null)
            {
                string piechart2_64 = ToBase64String(chart2);
                ImgTag2 = getImageTag(piechart2_64, chart2.Width, chart2.Height, "PieChart1");
            }

            if (ratingChart != null)
            {
                string ratingChart64 = ToBase64String(ratingChart);
                ImgTag3 = getImageTag(ratingChart64, ratingChart.Width, ratingChart.Height, "PieChart1");
            }

            //HTML WRITING;
            try
            {
                report = writeHeader(carboProject);

                report += writeReportTable(carboProject);

                report += writeMaterialTable(carboProject);

                report += ImgTag1;
                report += ImgTag2;
                report += ImgTag3;

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

        private static string getImageTag(string imageAsString, int width, int height, string toolText)
        {
            string imgTag = string.Empty;

            imgTag = "<img src=\"data:image/png;base64,";
             imgTag += imageAsString + "\" ";
            //imgTag += " width=\"" + width.ToString() + (char)34;
            imgTag += " height=\"" + height.ToString() + (char)34 + "/>" + System.Environment.NewLine; 

            return imgTag;
        }

        private static string writeMaterialTable(CarboProject carboProject)
        {
            string html = "<H1><B>" + "Material Properties" + "</B></H1><BR>" + System.Environment.NewLine;

            html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=1250>";

            html += "<TR></TR>";
            //ResultTable in a table
            try
            {

                //Write Headers:

                html += "<TR>" + System.Environment.NewLine;
                html += "<TD width=" + 200 + "><B>" + "Material" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 200 + "><B>" + "Category" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "Description" + "</B></TD>" + System.Environment.NewLine;
                
                html += "<TD width=" + 50 + "><B>" + "Density" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "ECI" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "ECI" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 50 + "><B>" + "EA1-A3" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "A4" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "A5" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "B1-B7" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "C1-C4" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "D" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "Mix" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 50 + "><B>" + "B4" + "</B></TD>" + System.Environment.NewLine;



                html += "</TR>" + System.Environment.NewLine;
                //UNITS
                html += "<TR>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='middle'><B>" + "kg/m³" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "kgCo<SUB>2</SUB>/m³" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='middle'><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='middle'><B>" + "kgCo<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='middle'><B>" + "" + "</B></TD>" + System.Environment.NewLine;


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

                        html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Material.materialB1B5Properties.B4, 2) + "</td>" + System.Environment.NewLine;


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

        private static string writeReportTable(CarboProject carboProject)
        {

            string html = "<H1><B>" + "Embodied Carbon Calculation Groups:" + "</B></H1><BR>" + System.Environment.NewLine;
            
            html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=1250>";
            
            html += "<TR></TR>";
            //ResultTable in a table
            try
            {

                //Write Headers:

                html += "<TR>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Category" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 200 + "><B>" + "Material" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 200 + "><B>" + "Description" + "</B></TD>" + System.Environment.NewLine;

                //Advanced settings
                html += "<TD width=" + 50 + "><B>" + "Correction Formula" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "Waste" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "Added" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "[B4]" + "</B></TD>" + System.Environment.NewLine;


                html += "<TD width=" + 50 + "><B>" + "Total Volume" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "Density" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "Mass" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 50 + "><B>" + "ECI" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "ECI" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 50 + "><B>" + "EC" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 50 + "><B>" + "Total" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 50 + "><B>" + "A1-A3" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "A4" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "A5" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "B1-B7" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 50 + "><B>" + "C1-C4" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "D" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 50 + "><B>" + "Mix" + "</B></TD>" + System.Environment.NewLine;


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

                html += "<TD align='left'><B>" + "m³" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "kg/m³" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "kg" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "kgCO<SUB>2</SUB>/kg" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "kgCO<SUB>2</SUB>/m³" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "tCO<SUB>2</SUB>" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "%" + "</B></TD>" + System.Environment.NewLine;

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
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.B4Factor, 2) + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.TotalVolume, 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cbg.Density + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.Mass,2) + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.ECI,2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round((cbg.getVolumeECI), 2) + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.EC,2) + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + Math.Round(cbg.PerCent,2) + "</td>" + System.Environment.NewLine;

                    //Per Group
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.materialB1B5Properties.B4 * cbg.Material.ECI_A1A3 * cbg.Mass,3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.materialB1B5Properties.B4 * cbg.Material.ECI_A4 * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.materialB1B5Properties.B4 * cbg.Material.ECI_A5 * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.materialB1B5Properties.B4 * cbg.Material.ECI_B1B5 * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.materialB1B5Properties.B4 * cbg.Material.ECI_C1C4 * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.materialB1B5Properties.B4 * cbg.Material.ECI_D * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + Math.Round(Math.Round(cbg.Material.materialB1B5Properties.B4 * cbg.Material.ECI_Mix * cbg.Mass, 3), 2) + "</td>" + System.Environment.NewLine;

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
            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;

            html += "<TD width=" + 50 + "><B>" + "" + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + totalGroup.EC + "</B></TD>" + System.Environment.NewLine;
            html += "<TD width=" + 50 + "><B>" + "100 %" + "</B></TD>" + System.Environment.NewLine;

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
                html = "<HTML><HEAD><TITLE>Carbon Life Calculation for: " + carboProject.Name + " </TITLE>" + System.Environment.NewLine;
                //add header row
                html += "<LINK href = 'https://fonts.googleapis.com/css?family=Oswald' rel='stylesheet'>" + System.Environment.NewLine;

                html += getCSS();
                html += "</HEAD><BODY>";
                html += "<H1>Project Info:</H1>" + System.Environment.NewLine;
                html += "<H2>Name: " + carboProject.Name + "</H2>" + System.Environment.NewLine;
                html += "<H2>Description: " + carboProject.Description + "</H2>" + System.Environment.NewLine;
                html += "<H2>Value: £ " + carboProject.Value + " </H2>" + System.Environment.NewLine;
                html += "<H2>Area: " + carboProject.Area + " m²</H2>" + System.Environment.NewLine;
                html += "<H2>Total Embodied Carbon: " + Math.Round(carboProject.getTotalEC(), 2) + " tCO<SUB>2</SUB></H2>" + System.Environment.NewLine;
                html += "<H2>Total Embodied Carbon / m<SUP>2</SUP>: " + Math.Round((carboProject.getTotalEC()/carboProject.Area), 3) + " tCO<SUB>2</SUB>/m²</H2>" + System.Environment.NewLine;

                html += "<H2>Category: " + carboProject.Category + "</H2>" + System.Environment.NewLine;
                html += "<H2>Export Date: " + DateTime.Today.ToShortDateString() + "</H2>" + System.Environment.NewLine;

                string Summarytext = carboProject.getSummaryText(true, true, true, true);
                Summarytext = Summarytext.Replace(System.Environment.NewLine, "<BR>"); //add a line terminating ;


                html += "<H3>" + Summarytext + "</H3>" + System.Environment.NewLine;

            }
            catch
            {
            }

            return html;
        }

        private static string getCSS()
        {
            string html = "";

            html += "<STYLE type=\"text/css\">" + System.Environment.NewLine;

            html += "table {font-family:Artifakt Element, Quattrocento Sans, Oswald, Sergoe UI, Calabri, Arial, Helvetica, sans-serif;" +
                        "margin-left:20px;" +
                        "border:#000 1px solid; }" +
                        System.Environment.NewLine;

            html += "td {font-family:Artifakt Element, Quattrocento Sans, Oswald, Sergoe UI, Calabri, Arial, Helvetica, sans-serif;" +
                        "color:#000;" +
                        "font-size:16px;" +
                        "background:#fff;" +
                        "margin:12px;" +
                        "border:#000 0px solid; }" +
                        System.Environment.NewLine;

            html += "h1 {font-family:Artifakt Element, Ubuntu, Quattrocento Sans, Oswald, Sergoe UI, Calabri, Arial, Helvetica, sans-serif;" +
                        "color:#000;" +
                        "font-size:36px;" +
                        "text-shadow: 1px 1px 0px #fff;" +
                        "margin-left:20px;" +
                        "border:#000 0px solid; }" +
                        System.Environment.NewLine;

            html += "h2 {font-family:Artifakt Element, Quattrocento Sans, Oswald, Sergoe UI, Calabri, Arial, Helvetica, sans-serif;" +
                        "color:#000;" +
                        "font-size:16px;" +
                        "text-shadow: 1px 1px 0px #fff;" +
                        "margin-left:20px;" +
                        "border:#000 0px solid; }" +
                        System.Environment.NewLine;

            html += "h3 {font-family:Artifakt Element, Quattrocento Sans, Oswald, Sergoe UI, Calabri, Arial, Helvetica, sans-serif;" +
                        "color:#000;" +
                        "font-size:16px;" +
                        "text-shadow: 1px 1px 0px #fff;" +
                        "margin-left:20px;" +
                        "border:#000 0px solid; }" +
                        System.Environment.NewLine;

            html += "</STYLE>" + System.Environment.NewLine;


            return html;


        }

        internal static string closeHTML()
        {
            string html = "";
            string title = "<H3>Carbo Calculation: " + DateTime.Today.ToShortDateString() + "</H3><BR>" + System.Environment.NewLine;

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

    }


}

