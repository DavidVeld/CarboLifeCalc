using CarboLifeAPI;
using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialConstructionPicker.xaml
    /// </summary>
    public partial class RevitActivator : Window
    {
        internal bool isAccepted;


        bool has2025;
        bool has2026;

        public RevitActivator()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckRevitVersions();
        }

        private void CheckRevitVersions()
        {
            string path = @"C:\ProgramData\Autodesk\Revit\Addins";


            has2025 = false;
            has2026 = false;


            if (Directory.Exists(path))
            {
                string[] dirlist = Directory.GetDirectories(path);

                if (dirlist.Length > 0)
                {
                    //2025
                    has2025 = false;
                    foreach (string str in dirlist)
                    {
                        string filepath = str + "\\" + "CarboLifeCalc.addin";

                        if (str.Contains("2025"))
                        {
                            //Check if user has addin in the folder
                            if (File.Exists(filepath))
                            {
                                //addin is installed
                                chx_2025.IsChecked = true;
                                chx_2025.IsEnabled = true;
                                lbl_2025.Foreground = Brushes.Green;
                                lbl_2025.Content = "2025 Addin Installed";
                            }
                            else
                            {
                                //User has revit version but not addin installed
                                chx_2025.IsChecked = false;
                                chx_2025.IsEnabled = true;
                                lbl_2025.Foreground = Brushes.Black;
                                lbl_2025.Content = "2025 Addin Not Installed";
                            }
                            has2025 = true;
                            break;
                        }
                    }

                    if (has2025 == false)
                    {
                        //revit version not found
                        chx_2025.IsEnabled = false;
                        lbl_2025.Foreground = Brushes.Gray;
                        lbl_2025.Content = "Revit 2025 Not Found";
                    }
                    //End 2025
                    //2026
                    has2026 = false;
                    foreach (string str in dirlist)
                    {
                        string filepath = str + "\\" + "CarboLifeCalc.addin";

                        if (str.Contains("2026"))
                        {
                            //Check if user has addin in the folder
                            if (File.Exists(filepath))
                            {
                                //addin is installed
                                chx_2026.IsChecked = true;
                                chx_2026.IsEnabled = true;
                                lbl_2026.Foreground = Brushes.Green;
                                lbl_2026.Content = "2026 Addin Installed";
                            }
                            else
                            {
                                //User has revit version but not addin installed
                                chx_2026.IsChecked = false;
                                chx_2026.IsEnabled = true;
                                lbl_2026.Foreground = Brushes.Black;
                                lbl_2026.Content = "2026 Addin Not Installed";
                            }
                            has2026 = true;
                            break;
                        }
                    }

                    if (has2026 == false)
                    {
                        //revit version not found
                        chx_2026.IsEnabled = false;
                        lbl_2026.Foreground = Brushes.Gray;
                        lbl_2026.Content = "Revit 2026 Not Found";
                    }
                    //End 2026
                }
                else
                {
                    MessageBox.Show("The installation folder for the addins cannot be found in: " + path + Environment.NewLine + "Please make sure you have Revit and the required versions installed", "Computer says no", MessageBoxButton.OK);
                }
            }
           
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;

            this.Close();
        }

        private void Btn_Apply(object sender, RoutedEventArgs e)
        {
            //Get the addin file
            string dirPath = Utils.getAssemblyPath();
            string filePathRaw = dirPath + "\\" + "CarboLifeCalcRaw.addin";
            string filePath = dirPath + "\\" + "CarboLifeCalc.addin";
            try
            {
                if (File.Exists(filePathRaw))
                {
                    MessageBox.Show("Copying addin files");
                    //Edit the addin file

                    UpdateAddinfile(dirPath);
                    if (File.Exists(filePath))
                    {
                        //Copy the addin file

                        if (chx_2025.IsChecked == true)
                            CopyFile(filePath, "2025");
                        else
                        {
                            if (File.Exists(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2025 + "\\CarboLifeCalc.addin"))
                                File.Delete(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2025 + "\\CarboLifeCalc.addin");
                        }
                        if (chx_2026.IsChecked == true)
                            CopyFile(filePath, "2026");
                        else
                        {
                            if (File.Exists(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2026 + "\\CarboLifeCalc.addin"))
                                File.Delete(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2026 + "\\CarboLifeCalc.addin");
                        }
                        //deletebuffer
                        File.Delete(filePath);

                    }
                }
                CheckRevitVersions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //Get the target folder
        }

        private void CopyFile(string filePath, string v)
        {

            string path = @"C:\ProgramData\Autodesk\Revit\Addins\" + v + "\\CarboLifeCalc.addin";
            try
            {
                File.Copy(filePath, path,true);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void UpdateAddinfile(string dirPath)
        {
            try
            {
                string filePathRaw = dirPath + "\\" + "CarboLifeCalcRaw.addin";
                string filePath = dirPath + "\\" + "CarboLifeCalc.addin";

                string text = File.ReadAllText(filePathRaw);
                text = text.Replace("[PATH]", dirPath);
                File.WriteAllText(filePath, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
