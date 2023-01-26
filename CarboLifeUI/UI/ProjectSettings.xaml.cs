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
using LiveCharts;
using LiveCharts.Wpf;
using CarboLifeAPI;

namespace CarboLifeUI.UI
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class ProjectSettings : UserControl
    {
        public CarboProject CarboLifeProject;
        string summaryTextMemory = "";
        public ProjectSettings()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();
        }

        public ProjectSettings(CarboProject carboLifeProject)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();

            CarboLifeProject = carboLifeProject;

        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                DependencyObject parent = VisualTreeHelper.GetParent(this);
                Window parentWindow = Window.GetWindow(parent);
                CarboLifeMainWindow mainViewer = parentWindow as CarboLifeMainWindow;

                if (mainViewer != null)
                    CarboLifeProject = mainViewer.getCarbonLifeProject();

                if (CarboLifeProject != null)
                {
                    //A project Is loaded, Proceed to next
                    RefreshInterFace();
                }

                //CategoryList:
                if (cbb_BuildingType.Items.Count == 0)
                {
                    cbb_BuildingType.Items.Clear();

                    IList<LetiScore> letiList = new List<LetiScore>();

                    letiList = ScorsIndicator.getLetiList();
                    IList<string> typelist = letiList.Select(x => x.BuildingType).Distinct().ToList();

                    foreach (string name in typelist)
                    {
                        cbb_BuildingType.Items.Add(name);
                    }

                    cbb_BuildingType.SelectedItem = CarboLifeProject.Category;

                }

                if (cbb_Currency.Items.Count == 0)
                {
                    cbb_Currency.Items.Add("£");
                    cbb_Currency.Items.Add("$");
                    cbb_Currency.Items.Add("€");
                    cbb_Currency.Items.Add("¥");
                    cbb_Currency.Items.Add("A$");
                    cbb_Currency.Items.Add("C$");

                    cbb_Currency.SelectedItem = CarboLifeProject.valueUnit;

                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshInterFace()
        {
            try
            {
                if (CarboLifeProject != null)
                {

                    txt_ProjectName.Text = CarboLifeProject.Name;
                    txt_Number.Text = CarboLifeProject.Number;
                    cbb_BuildingType.Text = CarboLifeProject.Category;
                    cbb_Currency.Text = CarboLifeProject.valueUnit;
                    txt_Desctiption.Text = CarboLifeProject.Description;

                    txt_Area.Text = CarboLifeProject.Area.ToString();
                    
                    //A5
                    txt_Value.Text = CarboLifeProject.Value.ToString();
                    txt_ValueA5Fact.Text = CarboLifeProject.A5Factor.ToString();

                    txt_SocialCost.Text = CarboLifeProject.SocialCost.ToString();

                    //C1
                    txt_DemoArea.Text = CarboLifeProject.demoArea.ToString();
                    txt_DemoC1Fact.Text = CarboLifeProject.C1Factor.ToString();

                    //energy
                    txt_DesignLife.Text = CarboLifeProject.designLife.ToString();
                    txt_EnergyPerYear.Text = CarboLifeProject.energyPerYear.ToString();

                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Cnv_Summary_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            this.Visibility = Visibility.Visible;

        }
      
        private void SaveSettings()
        {
            /*
            CarboLifeProject.Name = txt_ProjectName.Text;
            CarboLifeProject.Number = txt_Number.Text;
            CarboLifeProject.Category = cbb_BuildingType.Text;
            CarboLifeProject.Description = txt_Desctiption.Text;

            CarboLifeProject.SocialCost = CarboLifeAPI.Utils.ConvertMeToDouble(txt_SocialCost.Text);
            CarboLifeProject.Area = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Area.Text);
            CarboLifeProject.SocialCost = CarboLifeAPI.Utils.ConvertMeToDouble(txt_SocialCost.Text);
            //A5
            CarboLifeProject.Value = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Value.Text);
            CarboLifeProject.A5Factor = CarboLifeAPI.Utils.ConvertMeToDouble(txt_ValueA5Fact.Text);

            //C1
            CarboLifeProject.demoArea = CarboLifeAPI.Utils.ConvertMeToDouble(txt_DemoArea.Text);
            CarboLifeProject.C1Factor = CarboLifeAPI.Utils.ConvertMeToDouble(txt_DemoC1Fact.Text);
            */

            CarboLifeProject.CalculateProject();

        }

        /*
        private void Btn_SaveInfo_Click(object sender, RoutedEventArgs e)
        {
          //  SaveSettings();
         //   RefreshInterFace();
        }
        */

        //When assembly cant be find bind to current
        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            System.Reflection.Assembly ayResult = null;
            string sShortAssemblyName = args.Name.Split(',')[0];
            System.Reflection.Assembly[] ayAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly ayAssembly in ayAssemblies)
            {
                if (sShortAssemblyName == ayAssembly.FullName.Split(',')[0])
                {
                    ayResult = ayAssembly;
                    break;
                }
            }
            return ayResult;
        }


        private void btn_EditDescription_Click(object sender, RoutedEventArgs e)
        {
            DescriptionEditor editor = new DescriptionEditor(txt_Desctiption.Text);
            editor.ShowDialog();
            if (editor.isAccepted == true)
            {
                txt_Desctiption.Text = editor.description;
            }
        }

        private void btn_EditDescription_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CarboLifeProject != null && summaryTextMemory != "")
                {
                    Clipboard.SetText(summaryTextMemory);
                    MessageBox.Show("Text copied to clipboard", "Friendly Message", MessageBoxButton.OK);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Message", MessageBoxButton.OK);
            }
        }

        private void cbb_BuildingType_DropDownClosed(object sender, EventArgs e)
        {
            CarboLifeProject.Category = cbb_BuildingType.Text;
        }

        private async void txt_ProjectName_TextChanged(object sender, TextChangedEventArgs e)
        {

            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.Name = tb.Text;
            }
        }

        private async void txt_Number_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.Number = tb.Text;
            }
        }

        private async void txt_Desctiption_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.Description = tb.Text;
            }
        }

        private async void txt_Area_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.Area = Utils.ConvertMeToDouble(tb.Text);
                SaveSettings();
            }
        }

        private async void txt_DemoArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.demoArea = Utils.ConvertMeToDouble(tb.Text);
                SaveSettings();
            }
        }

        private async void txt_DemoC1Fact_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.C1Factor = Utils.ConvertMeToDouble(tb.Text);
                SaveSettings();
            }
        }

        private async void txt_Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.Value = Utils.ConvertMeToDouble(tb.Text);
                SaveSettings();
            }
        }

        private async void txt_ValueA5Fact_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.A5Factor = Utils.ConvertMeToDouble(tb.Text);
                SaveSettings();
            }
        }

        private async void txt_SocialCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.SocialCost = Utils.ConvertMeToDouble(tb.Text);
                SaveSettings();
            }
        }




        private void cbb_Currency_DropDownClosed(object sender, EventArgs e)
        {
            CarboLifeProject.valueUnit = cbb_Currency.Text;
        }

        private async void txt_DesignLife_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.designLife = Convert.ToInt32(Utils.ConvertMeToDouble(tb.Text));
                SaveSettings();
            }
        }

        private async void txt_EnergyPerYear_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                CarboLifeProject.energyPerYear = Utils.ConvertMeToDouble(tb.Text);
                SaveSettings();
            }
        }
    }
}
