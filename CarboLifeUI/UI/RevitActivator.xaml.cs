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

        bool has2019;
        bool has2020;
        bool has2021;
        bool has2022;
        bool has2023;
        bool has2024;
        bool has2025;


        bool checked2019;

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

            has2020 = false;
            has2021 = false;
            has2022 = false;
            has2023 = false;
            has2024 = false;
            has2025 = false;


            if (Directory.Exists(path))
            {
                string[] dirlist = Directory.GetDirectories(path);

                if (dirlist.Length > 0)
                {
                    //2019
                    has2019 = false;
                    foreach (string str in dirlist)
                    {
                        string filepath = str + "\\" + "CarboLifeCalc.addin";

                        if (str.Contains("2019"))
                        {
                            //Check if user has addin in the folder
                            if (File.Exists(filepath))
                            {
                                //addin is installed
                                chx_2019.IsChecked = true;
                                chx_2019.IsEnabled = true;
                                lbl_2019.Foreground = Brushes.Green;
                                lbl_2019.Content = "2019 Addin Installed";
                            }
                            else
                            {
                                //User has revit version but not addin installed
                                chx_2019.IsChecked = false;
                                chx_2019.IsEnabled = true;
                                lbl_2019.Foreground = Brushes.Black;
                                lbl_2019.Content = "2019 Addin Not Installed";
                            }
                            has2019 = true;
                            break;
                        }
                    }

                    if (has2019 == false)
                    {
                        //revit version not found
                        chx_2019.IsEnabled = false;
                        lbl_2019.Foreground = Brushes.Gray;
                        lbl_2019.Content = "Revit 2019 Not Found";
                    }
                    //End 2019
                    //2020
                    has2020 = false;
                    foreach (string str in dirlist)
                    {
                        string filepath = str + "\\" + "CarboLifeCalc.addin";

                        if (str.Contains("2020"))
                        {
                            //Check if user has addin in the folder
                            if (File.Exists(filepath))
                            {
                                //addin is installed
                                chx_2020.IsChecked = true;
                                chx_2020.IsEnabled = true;
                                lbl_2020.Foreground = Brushes.Green;
                                lbl_2020.Content = "2020 Addin Installed";
                            }
                            else
                            {
                                //User has revit version but not addin installed
                                chx_2020.IsChecked = false;
                                chx_2020.IsEnabled = true;
                                lbl_2020.Foreground = Brushes.Black;
                                lbl_2020.Content = "2020 Addin Not Installed";
                            }
                            has2020 = true;
                            break;
                        }
                    }

                    if (has2020 == false)
                    {
                        //revit version not found
                        chx_2020.IsEnabled = false;
                        lbl_2020.Foreground = Brushes.Gray;
                        lbl_2020.Content = "Revit 2020 Not Found";
                    }
                    //End 2020
                    //2021
                    has2021 = false;
                    foreach (string str in dirlist)
                    {
                        string filepath = str + "\\" + "CarboLifeCalc.addin";

                        if (str.Contains("2021"))
                        {
                            //Check if user has addin in the folder
                            if (File.Exists(filepath))
                            {
                                //addin is installed
                                chx_2021.IsChecked = true;
                                chx_2021.IsEnabled = true;
                                lbl_2021.Foreground = Brushes.Green;
                                lbl_2021.Content = "2021 Addin Installed";
                            }
                            else
                            {
                                //User has revit version but not addin installed
                                chx_2021.IsChecked = false;
                                chx_2021.IsEnabled = true;
                                lbl_2021.Foreground = Brushes.Black;
                                lbl_2021.Content = "2021 Addin Not Installed";
                            }
                            has2021 = true;
                            break;
                        }
                    }

                    if (has2021 == false)
                    {
                        //revit version not found
                        chx_2021.IsEnabled = false;
                        lbl_2021.Foreground = Brushes.Gray;
                        lbl_2021.Content = "Revit 2021 Not Found";
                    }
                    //End 2021
                    //2022
                    has2022 = false;
                    foreach (string str in dirlist)
                    {
                        string filepath = str + "\\" + "CarboLifeCalc.addin";

                        if (str.Contains("2022"))
                        {
                            //Check if user has addin in the folder
                            if (File.Exists(filepath))
                            {
                                //addin is installed
                                chx_2022.IsChecked = true;
                                chx_2022.IsEnabled = true;
                                lbl_2022.Foreground = Brushes.Green;
                                lbl_2022.Content = "2022 Addin Installed";
                            }
                            else
                            {
                                //User has revit version but not addin installed
                                chx_2022.IsChecked = false;
                                chx_2022.IsEnabled = true;
                                lbl_2022.Foreground = Brushes.Black;
                                lbl_2022.Content = "2022 Addin Not Installed";
                            }
                            has2022 = true;
                            break;
                        }
                    }

                    if (has2022 == false)
                    {
                        //revit version not found
                        chx_2022.IsEnabled = false;
                        lbl_2022.Foreground = Brushes.Gray;
                        lbl_2022.Content = "Revit 2022 Not Found";
                    }
                    //End 2022
                    //2023
                    has2023 = false;
                    foreach (string str in dirlist)
                    {
                        string filepath = str + "\\" + "CarboLifeCalc.addin";

                        if (str.Contains("2023"))
                        {
                            //Check if user has addin in the folder
                            if (File.Exists(filepath))
                            {
                                //addin is installed
                                chx_2023.IsChecked = true;
                                chx_2023.IsEnabled = true;
                                lbl_2023.Foreground = Brushes.Green;
                                lbl_2023.Content = "2023 Addin Installed";
                            }
                            else
                            {
                                //User has revit version but not addin installed
                                chx_2023.IsChecked = false;
                                chx_2023.IsEnabled = true;
                                lbl_2023.Foreground = Brushes.Black;
                                lbl_2023.Content = "2023 Addin Not Installed";
                            }
                            has2023 = true;
                            break;
                        }
                    }

                    if (has2023 == false)
                    {
                        //revit version not found
                        chx_2023.IsEnabled = false;
                        lbl_2023.Foreground = Brushes.Gray;
                        lbl_2023.Content = "Revit 2023 Not Found";
                    }
                    //End 2023
                    //2024
                    has2024 = false;
                    foreach (string str in dirlist)
                    {
                        string filepath = str + "\\" + "CarboLifeCalc.addin";

                        if (str.Contains("2024"))
                        {
                            //Check if user has addin in the folder
                            if (File.Exists(filepath))
                            {
                                //addin is installed
                                chx_2024.IsChecked = true;
                                chx_2024.IsEnabled = true;
                                lbl_2024.Foreground = Brushes.Green;
                                lbl_2024.Content = "2024 Addin Installed";
                            }
                            else
                            {
                                //User has revit version but not addin installed
                                chx_2024.IsChecked = false;
                                chx_2024.IsEnabled = true;
                                lbl_2024.Foreground = Brushes.Black;
                                lbl_2024.Content = "2024 Addin Not Installed";
                            }
                            has2024 = true;
                            break;
                        }
                    }

                    if (has2024 == false)
                    {
                        //revit version not found
                        chx_2024.IsEnabled = false;
                        lbl_2024.Foreground = Brushes.Gray;
                        lbl_2024.Content = "Revit 2024 Not Found";
                    }
                    //End 2024
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
                        if (chx_2019.IsChecked == true)
                            CopyFile(filePath, "2019");
                        else
                        {
                            if(File.Exists(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2019 + "\\CarboLifeCalc.addin"))
                                File.Delete(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2019 + "\\CarboLifeCalc.addin");
                        }

                        if (chx_2020.IsChecked == true)
                            CopyFile(filePath, "2020");
                        else
                        {
                            if (File.Exists(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2020 + "\\CarboLifeCalc.addin"))
                                File.Delete(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2020 + "\\CarboLifeCalc.addin");

                        }

                        if (chx_2021.IsChecked == true)
                            CopyFile(filePath, "2021");
                        else
                        {
                            if (File.Exists(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2021 + "\\CarboLifeCalc.addin"))
                                File.Delete(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2021 + "\\CarboLifeCalc.addin");
                        }

                        if (chx_2022.IsChecked == true)
                            CopyFile(filePath, "2022");
                        else
                        {
                            if (File.Exists(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2022 + "\\CarboLifeCalc.addin"))
                                File.Delete(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2022 + "\\CarboLifeCalc.addin");
                        }

                        if (chx_2023.IsChecked == true)
                            CopyFile(filePath, "2023");
                        else
                        {
                            if (File.Exists(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2023 + "\\CarboLifeCalc.addin"))
                                File.Delete(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2023 + "\\CarboLifeCalc.addin");
                        }

                        if (chx_2024.IsChecked == true)
                            CopyFile(filePath, "2024");
                        else
                        {
                            if (File.Exists(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2024 + "\\CarboLifeCalc.addin"))
                                File.Delete(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2024 + "\\CarboLifeCalc.addin");
                        }
                        if (chx_2025.IsChecked == true)
                            CopyFile(filePath, "2025");
                        else
                        {
                            if (File.Exists(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2025 + "\\CarboLifeCalc.addin"))
                                File.Delete(@"C:\ProgramData\Autodesk\Revit\Addins\" + 2025 + "\\CarboLifeCalc.addin");
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
