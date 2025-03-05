using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace CarboCircle.data
{
    internal class carboCircleReportUtils
    {
        internal static void ExportReport(carboCircleProject project, string reportImage, string reportPath)
        {
            string report = "";

            //Create a File and save it as a HTML File

            //EXPORT IMAGES HERE:
            string ImgTag1 = ReportBuilder.getImageTag(reportImage,0,640,"Image");

            //HTML WRITING;
            try
            {
                //Project Info
                report = writeHeader(project);

                //Calculation Results
                //report += writeCalculation(carboProject);

                //Images
                report += "<H2><B>" + "Project Image:" + "</B></H2><BR>" + System.Environment.NewLine;
                report += "<TABLE border=0 cellpadding=0 cellspacing=0 width=1600>";
                report += "<TR><TD></TD></TR>";
                report += "<TR><TD>" + ImgTag1 + "</TD></TR>";
                report += "</TABLE>";


                report += writeMatchTable(project);

                report += writeVolumesTable(project);

                report += writeLeftOverTable(project);


                report += ReportBuilder.closeHTML();


                if (report != "")
                {
                    using (StreamWriter sw = new StreamWriter(reportPath, false, Encoding.GetEncoding("Windows-1252")))
                    {
                        sw.WriteLine(report);
                        sw.Close();
                    }
                }

                //open if the file exists
                if (File.Exists(reportPath))
                {
                    System.Windows.MessageBox.Show("Report successfully created!", "Success!", MessageBoxButton.OK);
                    System.Diagnostics.Process.Start(reportPath);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }


        }

        private static string writeLeftOverTable(carboCircleProject project)
        {
            string html = "";

            html = "<H1><B>" + "Left Over Elements Table:" + "</B></H1><BR>" + System.Environment.NewLine;

            html += "<TABLE border=0 cellpadding=0 cellspacing=0 width=1600>";
            html += "<TR><TD></TD></TR>";
            html += "<TR><TD><H2>" + "The following elements were removed from the site, however they cannot be matched with a new element for this project." + "</H2></TD></TR>";
            html += "</TABLE>";

            html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=1600>";

            html += "<TR></TR>";
            //ResultTable in a table
            try
            {

                //Write Headers:

                html += "<TR>" + System.Environment.NewLine;

                html += "<TD width=" + 75 + "><B>" + "Material " + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 150 + "><B>" + "Name" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Category " + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Human Id" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Revit Id" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 150 + "><B>" + "Standard Name " + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Model Length " + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Usable Length " + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 75 + "><B>" + "Condition " + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 325 + "><B>" + "GUID  " + "</B></TD>" + System.Environment.NewLine;

                //Advanced settings
                html += "</TR>" + System.Environment.NewLine;

                //UNITS
                html += "<TR>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "m" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "m" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;

                html += "</TR>" + System.Environment.NewLine;

                //Write Data:
                List<carboCircleElement> cceList = project.getLeftOverData();
                List<carboCircleElement>  cceListSorted = new List<carboCircleElement>(cceList.OrderBy(i => i.category));


                foreach (carboCircleElement ccme in cceListSorted)
                {
                    html += "<TR>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + ccme.grade + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + ccme.category + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + ccme.name + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + ccme.humanId.ToString() + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + ccme.id.ToString() + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + ccme.standardName + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + ccme.length + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + ccme.netLength + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + ccme.quality.ToString() + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + ccme.GUID + "</td>" + System.Environment.NewLine;

                    html += "</TR>" + System.Environment.NewLine;

                }
                //html += getTotalsRow(carboProject.getTotalsGroup());

                html += "</TABLE>";
            }
            catch
            {
            }

            return html;
        }

        private static string writeVolumesTable(carboCircleProject project)
        {
            string html = "";

            html = "<H1><B>" + "Volume material Reuse Table:" + "</B></H1><BR>" + System.Environment.NewLine;

            html += "<TABLE border=0 cellpadding=0 cellspacing=0 width=1600>";
            html += "<TR><TD></TD></TR>";
            html += "<TR><TD><H2>" + "The following materials are identified as demolished, however, due to their nature they cannot be re-used directly in a new structure.<BR>" +
                "They can be processed in a way to substitute a new material with a more sustainable alternative:" + "</H2></TD></TR>";
            html += "</TABLE>";



            html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=1600>";

            html += "<TR></TR>";
            //ResultTable in a table
            try
            {

                //Write Headers:

                html += "<TR>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Material" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Volume" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Re-Used Volume" + "</B></TD>" + System.Environment.NewLine;

                //Advanced settings
                html += "</TR>" + System.Environment.NewLine;
                //UNITS
                html += "<TR>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "m3" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "m3" + "</B></TD>" + System.Environment.NewLine;

                html += "</TR>" + System.Environment.NewLine;

                //Write Data:
                List<carboCircleElement> cceVList = project.getCarboVolumeOpportunities();

                foreach (carboCircleElement cceV in cceVList)
                {
                    html += "<TR>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + cceV.materialName + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cceV.volume + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + cceV.netVolume + "</td>" + System.Environment.NewLine;

                    html += "</TR>" + System.Environment.NewLine;

                }
                //html += getTotalsRow(carboProject.getTotalsGroup());

                html += "</TABLE>";
            }
            catch
            {
            }

            return html;
        }

        private static string writeHeader(carboCircleProject project)
        {

                string html = "";

                try
                {
                    html += "<HTML><HEAD><TITLE>Carbon Circle Report: " + project.ProjectName + " </TITLE>" + System.Environment.NewLine;
                    //add header row

                    html += ReportBuilder.getCSS();

                    html += "</HEAD><BODY>";

                    html += "<H1><B>" + "Project Info" + "</B></H1><BR>" + System.Environment.NewLine;

                    html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=800>";
                    html += "<TR></TR>";

                    html += "<TR><TD width=" + 150 + "><B>" + "Name:" + "</B></TD>" + System.Environment.NewLine;
                    html += "<TD width=" + 175 + ">" + project.ProjectName + "</TD></TR>" + System.Environment.NewLine;

                    html += "<TR><TD width=" + 150 + "><B>" + "Project Number:" + "</B></TD>" + System.Environment.NewLine;
                    html += "<TD width=" + 175 + ">" + project.ProjectNumber + "</TD></TR>" + System.Environment.NewLine;

                    html += "<TR><TD width=" + 150 + "><B>" + "Description:" + "</B></TD>" + System.Environment.NewLine;
                    html += "<TD width=" + 175 + "" + project.ProjectDescription + "</TD></TR>" + System.Environment.NewLine;


                    html += "<TR><TD width=" + 150 + "><B>" + "Export Date:" + "</B></TD>" + System.Environment.NewLine;
                    html += "<TD width=" + 175 + ">" + DateTime.Today.ToShortDateString() + "</TD></TR>" + System.Environment.NewLine;

                    html += "</TABLE>";


                }
                catch
                {
                }

                return html;
            
        }

        private static string writeMatchTable(carboCircleProject project)
        {
            string html = "";

            html = "<H1><B>" + "Material Reuse Table:" + "</B></H1><BR>" + System.Environment.NewLine;

            html += "<TABLE border=0 cellpadding=0 cellspacing=0 width=1600>";
            html += "<TR><TD></TD></TR>";
            html += "<TR><TD><H2>" + "The following elements in the databases could be matched. Available materials are shown in the table below with a matching element in the designed building" + "</H2></TD></TR>";
            html += "</TABLE>";



            html += "<TABLE border=1 cellpadding=0 cellspacing=0 width=1600>";

            html += "<TR></TR>";
            //ResultTable in a table
            try
            {

                //Write Headers:

                html += "<TR>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Proposed Element" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 100 + "><B>" + "Proposed ElementId" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 75 + "><B>" + "Proposed Length" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 750 + "><B>" + "Mined to Element  " + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 150 + "><B>" + "Mined ElementId  " + "</B></TD>" + System.Environment.NewLine;
                html += "<TD width=" + 75 + "><B>" + "Mined Length " + "</B></TD>" + System.Environment.NewLine;

                html += "<TD width=" + 75 + "><B>" + "Offcut Length " + "</B></TD>" + System.Environment.NewLine;

                //Advanced settings
                html += "</TR>" + System.Environment.NewLine;

                //UNITS
                html += "<TR>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "m" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "" + "</B></TD>" + System.Environment.NewLine;
                html += "<TD align='left'><B>" + "m" + "</B></TD>" + System.Environment.NewLine;

                html += "<TD align='left'><B>" + "m" + "</B></TD>" + System.Environment.NewLine;

                html += "</TR>" + System.Environment.NewLine;

                //Write Data:
                List<carboCircleMatchElement> ccmelist = project.getCarboMatchesListSimplified();
                ccmelist = new List<carboCircleMatchElement>(ccmelist.OrderBy(i => i.required_Name));


                foreach (carboCircleMatchElement ccme in ccmelist)
                {
                    html += "<TR>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + ccme.required_standardName + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + ccme.required_id + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + ccme.required_length.ToString() + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + ccme.mined_standardName + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + ccme.mined_id + "</td>" + System.Environment.NewLine;
                    html += "<TD align='left' valign='middle'>" + ccme.mined_netLength + "</td>" + System.Environment.NewLine;

                    html += "<TD align='left' valign='middle'>" + Math.Round((ccme.mined_netLength - ccme.required_length),2).ToString() + "</td>" + System.Environment.NewLine;

                    html += "</TR>" + System.Environment.NewLine;

                }
                //html += getTotalsRow(carboProject.getTotalsGroup());

                html += "</TABLE>";
            }
            catch
            {
            }

            return html;


        }

        public static void WriteReportFile(string fileString, string exportPath)
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
                System.Windows.Forms.MessageBox.Show("An error occurred while writing the file: " + ex.Message);
            }
        }

        internal static string getImageAsString(string path)
        {
            //get temp Filepath
            //string MyAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //string MyAssemblyDir = Path.GetDirectoryName(MyAssemblyPath);
            //string tempImgpath = MyAssemblyPath + "\\tempCircleImg.jpg";

            try
            {
                if (File.Exists(path))
                {
                    using (Image image = Image.FromFile(path))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, image.RawFormat);
                            byte[] imageBytes = m.ToArray();

                            // Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);
                            return base64String;
                        }
                    }
                }
            }
            catch
            {
                return "";
            }

            return "";

        }
    }
}