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
    public partial class TemplateSelector : Window
    {
        internal bool isAccepted;
        public string selectedTemplateFile { get; private set; }

        public IDictionary<string, string> templates;


        public TemplateSelector()
        {
            selectedTemplateFile = "";
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            templates = PathUtils.getTemplateFiles();
            if (templates != null)
            {
                foreach (var template in templates)
                {
                    cbb_TemplateName.Items.Add(template.Key);
                }
                cbb_TemplateName.SelectedIndex = 0;
            }
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;

            string result;

            if (templates.TryGetValue(cbb_TemplateName.Text, out result))

                if (File.Exists(result))
                {
                    selectedTemplateFile = result;
                }
                else
                {
                    MessageBox.Show("The Selected Template could not be found");
                    isAccepted = false;
                }
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }
    }
}
