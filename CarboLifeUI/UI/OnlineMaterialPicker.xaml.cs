using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
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
    public partial class OnlineMaterialPicker : Window
    {
        internal bool isAccepted;
        public string description;
        public List<string> selectionList { get; set; }
        public OnlineMaterialPicker()
        {
            description = "";
            selectionList = new List<string>();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = "https://www.davidveld.nl/ping.php";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response != null)
                {
                    string line = "";

                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        while ((line = stream.ReadLine()) != null)
                        {
                            if (line != "")
                                lb_Selection.Items.Add(line);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No Online Database found, please make sure you are connected to the internet");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message) ;
            }

        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            //selectionList = lb_Selection.SelectedItems as List<string>;
            selectionList = new List<string>();
            foreach (var selection in lb_Selection.SelectedItems)
            {
                selectionList.Add(selection as string);
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
