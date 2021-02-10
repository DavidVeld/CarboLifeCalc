using CarboLifeAPI;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialConstructionPicker.xaml
    /// </summary>
    public partial class RevitActivator : Window
    {
        internal bool isAccepted;

        bool ok2019;
        bool ok2020;
        bool ok2021;

        bool has2019;
        bool has2020;
        bool has2021;


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

            ok2019 = false;
            ok2020 = false;
            ok2021 = false;

            has2019 = false;
            has2020 = false;
            has2021 = false;




            if (Directory.Exists(path))
            {
                string[] dirlist = Directory.GetDirectories(path);

                if (dirlist.Length > 0)
                {
                    foreach(string str in dirlist)
                    {
                        bool exists = false;

                        string filepath = str + "\\" + "CarboLifeCalc.addin";
                        if (File.Exists(filepath))
                            exists = true;

                        if (str.Contains("2019"))
                            has2019 = true;

                        if (str.Contains("2020"))
                            has2020 = true;

                        if (str.Contains("2021"))
                            has2021 = true;


                        if(has2019 == true && exists == true)
                        {
                            ok2019 = true;
                        }

                        if (has2020 == true && exists == true)
                        {
                            ok2020 = true;
                        }

                        if (has2021 == true && exists == true)
                        {
                            ok2021 = true;
                        }
                    }
                }

            }
            else
            {
                MessageBox.Show("The intalltion folder for the addins cannot be found in: " + path + Environment.NewLine + "Please make sure you have Revit and the required versions installed", "Computer sais no", MessageBoxButton.OK);
            }
            
            ///2019
            if (has2019 == true)
            {
                //Folder is present
                if (ok2019 == true)
                {
                    //addin is installed
                    chx_2019.IsChecked = true;
                    //chx_2019.IsEnabled = false;
                    lbl_2019.Foreground = Brushes.Green;
                    lbl_2019.Content = "2019 Activated";


                }
                else
                {
                    //addin is not installed
                    chx_2019.IsChecked = false;
                    chx_2019.IsEnabled = true;
                    lbl_2019.Foreground = Brushes.Black;
                    lbl_2019.Content = "2019 Not Installed";

                }
            }
            else
            {
                //revit version not found
                chx_2019.IsEnabled = false;
                lbl_2019.Foreground = Brushes.Gray;
                lbl_2019.Content = "2019 Not Found";


            }

            ///2020
            if (has2020 == true)
            {
                //Folder is present
                if (ok2020 == true)
                {
                    //addin is installed
                    chx_2020.IsChecked = true;
                    //chx_2020.IsEnabled = false;
                    lbl_2020.Foreground = Brushes.Green;
                    lbl_2020.Content = "2020 Activated";
                }
                else
                {
                    //addin is not installed
                    chx_2020.IsChecked = false;
                    chx_2020.IsEnabled = true;
                    lbl_2020.Foreground = Brushes.Black;
                    lbl_2020.Content = "2020 Not installed";

                }
            }
            else
            {
                //revit version not found
                chx_2020.IsEnabled = false;
                lbl_2020.Foreground = Brushes.Gray;
                lbl_2019.Content = "2020 Not found";

            }

            ///2021
            if (has2021 == true)
            {
                //Folder is present
                if (ok2021 == true)
                {
                    //addin is installed
                    chx_2021.IsChecked = true;
                    //chx_2021.IsEnabled = false;
                    lbl_2021.Foreground = Brushes.Green;
                    lbl_2021.Content = "2021 Activated";

                }
                else
                {
                    //addin is not installed
                    chx_2021.IsChecked = false;
                    chx_2021.IsEnabled = true;
                    lbl_2021.Foreground = Brushes.Black;
                    lbl_2021.Content = "2021 Not Installed";

                }
            }
            else
            {
                //revit version not found
                chx_2021.IsEnabled = false;
                lbl_2021.Foreground = Brushes.Gray;
                lbl_2021.Content = "2021 Not Found";

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
