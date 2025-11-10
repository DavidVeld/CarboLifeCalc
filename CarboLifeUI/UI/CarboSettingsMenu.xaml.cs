using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for MaterialConstructionPicker.xaml
    /// </summary>
    public partial class CarboSettingsMenu : Window
    {
        internal bool isAccepted;
        //public string templatePath;
        public CarboSettings settings;

        public CarboSettingsMenu()
        {
           // templatePath = "";

            settings = new CarboSettings().Load();


            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt_Path.Text = settings.templatePath;
            txt_DesignLife.Text = settings.defaultDesignLife.ToString();
            txt_SecretMessage.Text = settings.secretMessage;

            chx_Cars.IsChecked = settings.showCars;
            chx_Trees.IsChecked = settings.showTrees;
            chx_Plane.IsChecked = settings.showPlanes;
            chx_SCC.IsChecked = settings.showSCC;
            chx_Deaths.IsChecked = settings.showDeaths;

            chx_Experimental.IsChecked = settings.launchCircle;

            CheckTemplateFile();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            settings.defaultDesignLife = Convert.ToInt16(Convert.ToDouble(txt_DesignLife.Text));
            settings.secretMessage = txt_SecretMessage.Text;

            settings.showCars = chx_Cars.IsChecked.Value;
            settings.showTrees = chx_Trees.IsChecked.Value;
            settings.showPlanes = chx_Plane.IsChecked.Value;
            settings.showSCC = chx_SCC.IsChecked.Value;
            settings.showDeaths = chx_Deaths.IsChecked.Value;

            settings.launchCircle = chx_Experimental.IsChecked.Value;

            settings.Save();
            this.Close();
        }

        private void btn_Browse_Click(object sender, RoutedEventArgs e)
        {
            string currentDir = System.IO.Path.GetDirectoryName(settings.templatePath);

            string MaterialPathToOpen = Utils.OpenCarboMaterialLibrary(currentDir);

            if (MaterialPathToOpen != "")
            {
                FileInfo finfo = new FileInfo(MaterialPathToOpen);

                settings.templatePath = MaterialPathToOpen;
                txt_Path.Text = settings.templatePath;
                CheckTemplateFile();
                System.Windows.MessageBox.Show("You have changed your template path, this will be used next time you start a new calculation.");
                
            }
        }

        private void CheckTemplateFile()
        {
            if (File.Exists(settings.templatePath))
                lbl_CheckTemplatePath.Content = "Template Found";
            else
                lbl_CheckTemplatePath.Content = "Template NOT Found";
        }

        private void btn_Coffee_Click(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "https://buymeacoffee.com/davidveld", // Path to file
                UseShellExecute = true // This is the key part that allows it to open with the default application
            };

            Process.Start(startInfo);
        }

        private void btn_Check_Click(object sender, RoutedEventArgs e)
        {
            string secretmessage = txt_SecretMessage.Text;
            string secretEnCrypted = Utils.Crypt(secretmessage);

            if (secretEnCrypted == "LOmaRc4Q9HO7UU18crErdwvOQNXv8Hf+")
            {
                System.Windows.MessageBox.Show("This code is correct, enjoy no more pop-ups!");
            }
            else
            {
                System.Windows.MessageBox.Show("You guessed wrong, or made a typo, please try again");
                System.Windows.Clipboard.SetText(secretEnCrypted);
            }
        }
    }
}
