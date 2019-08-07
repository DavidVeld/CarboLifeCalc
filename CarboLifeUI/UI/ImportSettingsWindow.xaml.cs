using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ImportSettingsWindow : Window
    {
        public bool dialogOk;
        public ObservableCollection<CarboElement> carboElementList;
        public ObservableCollection<CarboGroup> carboGroupList;
        public List<CarboLevel> carboLevelList;

        public ImportSettingsWindow(ObservableCollection<CarboElement> elementList, List<CarboLevel> levelList)
        {
            dialogOk = false;
            carboElementList = elementList;
            carboLevelList = levelList;
            carboGroupList = new ObservableCollection<CarboGroup>();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dialogOk = false;
            List<string> categorylist = new List<string>();
            categorylist.Add("(Revit) Category");
            categorylist.Add("Sub-Category");
            categorylist.Add("Type Name");
            categorylist.Add("Demolished Yes/No");
            categorylist.Add("Super/Substructure");

            categorylist.Add("");

            foreach (string str in categorylist)
            {
                cbb_MainGroup.Items.Add(str);
                cbb_SecGroup.Items.Add(str);
            }

            cbb_MainGroup.Text = "(Revit) Category";
            cbb_SecGroup.Text = "Revit Category";

            //LevelList
            if (carboLevelList.Count > 0) {
                foreach (CarboLevel cl in carboLevelList)
                {
                    cbb_Levels.Items.Add(cl.Name);
                }
            }
        }

        private double getCutoffLevel()
        {
            double result = 0;

            foreach (CarboLevel cl in carboLevelList)
            {
                if (cl.Name == cbb_Levels.Text)
                {
                    result = cl.Level;
                }
            }
            return result;
        }
        private void Btn_Group_Click(object sender, RoutedEventArgs e)
        {
            carboGroupList = new ObservableCollection<CarboGroup>();
            double levelcutoff = -999;
            if (cbb_Levels.Text != "")
            {
                levelcutoff = getCutoffLevel();
            }
            carboGroupList = CarboElementImporter.GroupElementsAdvanced(carboElementList, cbb_MainGroup.Text,cbb_SecGroup.Text, txt_SpecialTypes.Text, levelcutoff, chk_ImportDemolished.IsChecked.Value);
        }
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            dialogOk = false;
            this.Close();
        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            dialogOk = true;
            this.Close();
        }


    }
}
