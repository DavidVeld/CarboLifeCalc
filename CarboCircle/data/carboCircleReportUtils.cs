using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
            /*
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Specify report directory";
            saveDialog.Filter = "HTML Files|*.html";
            saveDialog.FilterIndex = 2;
            saveDialog.RestoreDirectory = true;

            saveDialog.ShowDialog();

            string Path = saveDialog.FileName;

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
            */


            //EXPORT IMAGES HERE:
            string ImgTag1 = ReportBuilder.getImageTag(reportImage,640,480,"Image");

            //HTML WRITING;
            try
            {
                //Project Info
                report = writeHeader(project);

                //Calculation Results
                //report += writeCalculation(carboProject);

                //Images
                report += "<H2><B>" + "Graphs:" + "</B></H2><BR>" + System.Environment.NewLine;
                report += "<TABLE border=1 cellpadding=0 cellspacing=0 width=800>";
                report += "<TR><TD></TD></TR>";
                report += "<TR><TD>" + ImgTag1 + "</TD></TR>";
                report += "</TABLE>";



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