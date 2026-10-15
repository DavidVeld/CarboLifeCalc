using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            txt_Mapping.Text = settings.mappingPath;

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

        /// <summary>
        /// The shortest and longest design life the box will take, in years.
        /// The upper bound also keeps the value inside an Int16, which is what it is stored as.
        /// </summary>
        private const int minDesignLife = 1;
        private const int maxDesignLife = 500;

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            //Convert.ToInt16(Convert.ToDouble(...)) threw on an empty box, on "60 years" and on
            //anything non numeric, and overflowed above 32767. Inside Revit that is an unhandled
            //exception, not a validation message, so the box is checked before anything is saved.
            int designLife;

            if (TryReadDesignLife(out designLife) == false)
                return;

            isAccepted = true;
            settings.defaultDesignLife = designLife;
            settings.secretMessage = txt_SecretMessage.Text;

            settings.showCars = chx_Cars.IsChecked == true;
            settings.showTrees = chx_Trees.IsChecked == true;
            settings.showPlanes = chx_Plane.IsChecked == true;
            settings.showSCC = chx_SCC.IsChecked == true;
            settings.showDeaths = chx_Deaths.IsChecked == true;

            settings.launchCircle = chx_Experimental.IsChecked == true;

            settings.templatePath = txt_Path.Text;
            settings.mappingPath = txt_Mapping.Text;

            settings.Save();
            this.Close();
        }

        /// <summary>
        /// Reads the design life box. Tells the user what is wrong and puts the caret back in the
        /// box rather than letting the dialog close on a bad value.
        /// </summary>
        private bool TryReadDesignLife(out int designLife)
        {
            designLife = settings.defaultDesignLife;

            string text = txt_DesignLife.Text == null ? "" : txt_DesignLife.Text.Trim();

            double parsed;

            //Accept what the user's keyboard produces and what a pasted invariant value looks like.
            bool ok = Utils.TryConvertToDouble(text, out parsed);

            if (ok == false)
            {
                System.Windows.MessageBox.Show(
                    "The design life must be a number of years, for example 60." + Environment.NewLine +
                    "\"" + text + "\" could not be read.",
                    "Design life", MessageBoxButton.OK, MessageBoxImage.Warning);

                txt_DesignLife.Focus();
                txt_DesignLife.SelectAll();
                return false;
            }

            double rounded = Math.Round(parsed);

            if (rounded < minDesignLife || rounded > maxDesignLife)
            {
                System.Windows.MessageBox.Show(
                    "The design life must be between " + minDesignLife + " and " + maxDesignLife + " years.",
                    "Design life", MessageBoxButton.OK, MessageBoxImage.Warning);

                txt_DesignLife.Focus();
                txt_DesignLife.SelectAll();
                return false;
            }

            designLife = (int)rounded;
            return true;
        }



        private void CheckTemplateFile()
        {
            //if (File.Exists(settings.templatePath))
                //lbl_CheckTemplatePath.Text = "Template Found";
            //else
                //lbl_CheckTemplatePath.Text = "Template NOT Found";
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

        private void btn_BrowseMap_Click(object sender, RoutedEventArgs e)
        {
            string currentDir = System.IO.Path.GetDirectoryName(settings.mappingPath);

            string MappingPathToOpen = Utils.OpenCarboMappingLibrary(currentDir);

            if (MappingPathToOpen != "")
            {
                FileInfo finfo = new FileInfo(MappingPathToOpen);

                settings.mappingPath = MappingPathToOpen;
                txt_Mapping.Text = settings.mappingPath;
                //CheckTemplateFile();
                System.Windows.MessageBox.Show("You have changed your mapping file path, this will be used next time you start a new calculation.");

            }
        }
    }
}
