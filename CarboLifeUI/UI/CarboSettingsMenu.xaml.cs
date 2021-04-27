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
            CheckTemplateFile();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            settings.Save();
            this.Close();
        }

        private void btn_Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Carbo Material Data Files (*.cxml)|*.cxml";

            var path = openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "")
            {
                FileInfo finfo = new FileInfo(openFileDialog.FileName);

                if (openFileDialog.FileName != "")
                {
                    settings.templatePath = openFileDialog.FileName;
                    txt_Path.Text = settings.templatePath;
                    CheckTemplateFile();
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
    }
}
