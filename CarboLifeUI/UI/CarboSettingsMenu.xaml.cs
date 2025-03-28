using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
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

            CheckTemplateFile();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            settings.defaultDesignLife = Convert.ToInt16(Convert.ToDouble(txt_DesignLife.Text));
            settings.secretMessage = txt_SecretMessage.Text;

            settings.Save();
            this.Close();
        }

        private void btn_Browse_Click(object sender, RoutedEventArgs e)
        {
            string currentDir = System.IO.Path.GetDirectoryName(settings.templatePath);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Carbo Material Data Files (*.cxml)|*.cxml";
            if(Directory.Exists(currentDir))
                openFileDialog.InitialDirectory = currentDir;

            var path = openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "")
            {
                FileInfo finfo = new FileInfo(openFileDialog.FileName);

                if (openFileDialog.FileName != "")
                {
                    settings.templatePath = openFileDialog.FileName;
                    txt_Path.Text = settings.templatePath;
                    CheckTemplateFile();
                    System.Windows.MessageBox.Show("You have changed your template path, this will be used next time you start a new calculation.");
                }
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
            System.Diagnostics.Process.Start("https://buymeacoffee.com/davidveld");
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
