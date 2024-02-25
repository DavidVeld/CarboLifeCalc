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
                    CarboLifeProject.CalculateProject();

                    txt_ProjectName.Text = CarboLifeProject.Name;
                    txt_Number.Text = CarboLifeProject.Number;
                    cbb_BuildingType.Text = CarboLifeProject.Category;
                    cbb_Currency.Text = CarboLifeProject.valueUnit;
                    txt_Desctiption.Text = CarboLifeProject.Description;



                    chx_IsTotalBuilding.IsChecked = CarboLifeProject.totalAreaIsNew;
                    txt_Area.Text = CarboLifeProject.Area.ToString();
                    txt_AreaNew.Text = CarboLifeProject.AreaNew.ToString();

                    /*
                    if (CarboLifeProject.totalAreaIsNew == true)
                    {
                        //these two need to be equal if above is true;
                        CarboLifeProject.AreaNew = CarboLifeProject.Area;
                        txt_AreaNew.IsEnabled = false;
                        txt_AreaNew.Foreground = new SolidColorBrush(Colors.DarkGray);
                    }
                    else
                    {
                        txt_AreaNew.IsEnabled = true;
                        txt_AreaNew.Foreground = new SolidColorBrush(Colors.Black);

                    }
                    */







                    txt_DesignLife.Text = CarboLifeProject.designLife.ToString();
                    
                    //A5
                    txt_Value.Text = CarboLifeProject.Value.ToString();
                    txt_ValueA5Fact.Text = CarboLifeProject.A5Factor.ToString();

                    txt_SocialCost.Text = CarboLifeProject.SocialCost.ToString();

                    //C1
                    txt_DemoArea.Text = CarboLifeProject.demoArea.ToString();
                    txt_DemoC1Fact.Text = CarboLifeProject.C1Factor.ToString();

                    //energy
                    txt_EnergyPerYear.Text = CarboLifeProject.energyProperties.value.ToString();

                    //Totals
                    txt_A0Total.Text = ((CarboLifeProject.A0Global) / 1000).ToString();
                    txt_A5Total.Text = CarboLifeProject.A5Global.ToString();
                    txt_EnergyTotal.Text = (CarboLifeProject.energyProperties.value / 1000).ToString();
                    txt_C1Total.Text = CarboLifeProject.C1Global.ToString();

                    lbl_Currency.Content = CarboLifeProject.valueUnit.ToString();
                    lbl_Currencyunit.Content = "kgCO₂e/" + CarboLifeProject.valueUnit.ToString();
                   

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
            string text = txt_Area.Text;
            try
            {
                TextBox tb = (TextBox)sender;
                int startLength = tb.Text.Length;

                await Task.Delay(1200);

                    double convertedText = Utils.ConvertMeToDouble(tb.Text);
                    if (convertedText != 0)
                    {
                        CarboLifeProject.Area = convertedText;
                        txt_Area.Text = convertedText.ToString();
                        /*
                        if (chx_IsTotalBuilding.IsChecked == true)
                        {
                            CarboLifeProject.AreaNew = convertedText;
                            txt_AreaNew.Text = convertedText.ToString();
                        }*/
                    }
                
                RefreshInterFace();

            }
            catch (Exception ex)
            {
                //Resume async error.
            }
        }

        private async void txt_AreaNew_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = txt_AreaNew.Text;
            try
            {
                TextBox tb = (TextBox)sender;
                int startLength = tb.Text.Length;

                await Task.Delay(1000);

                    double convertedText = Utils.ConvertMeToDouble(tb.Text);
                    if (convertedText != 0)
                    {
                        CarboLifeProject.AreaNew = convertedText;
                        txt_AreaNew.Text = convertedText.ToString();
                        /*
                        if(chx_IsTotalBuilding.IsChecked == true)
                        {
                            CarboLifeProject.Area = convertedText;
                            txt_Area.Text = convertedText.ToString();
                        }*/
                    }
                
                RefreshInterFace();

            }
            catch (Exception ex)
            {
                //Resume async error.
            }
        }
        private void chx_IsTotalBuilding_Checked(object sender, RoutedEventArgs e)
        {
            if (chx_IsTotalBuilding != null && CarboLifeProject != null)
            {
                CarboLifeProject.totalAreaIsNew = chx_IsTotalBuilding.IsChecked.Value;
                RefreshInterFace();
            }
        }
        private async void txt_DemoArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
            CarboLifeProject.demoArea = Utils.ConvertMeToDouble(tb.Text);
            RefreshInterFace();

        }

        private async void txt_DemoC1Fact_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
            CarboLifeProject.C1Factor = Utils.ConvertMeToDouble(tb.Text);
            RefreshInterFace();

        }

        private async void txt_Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
            CarboLifeProject.Value = Utils.ConvertMeToDouble(tb.Text);
            RefreshInterFace();

        }

        private async void txt_ValueA5Fact_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(1000);
            CarboLifeProject.A5Factor = Utils.ConvertMeToDouble(tb.Text);
            RefreshInterFace();

        }

        private async void txt_SocialCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            await Task.Delay(1000);
            CarboLifeProject.SocialCost = Utils.ConvertMeToDouble(tb.Text);
            RefreshInterFace();

        }


        private void cbb_Currency_DropDownClosed(object sender, EventArgs e)
        {
            CarboLifeProject.valueUnit = cbb_Currency.Text;
            RefreshInterFace();
        }

        private async void txt_DesignLife_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            int designLifeNew = 50;

            await Task.Delay(1000);
            designLifeNew = Convert.ToInt32(Utils.ConvertMeToDouble(tb.Text));
            CarboLifeProject.SetDesignLife(designLifeNew);
            RefreshInterFace();
        }

        private void btn_CalcEnergy_Click(object sender, RoutedEventArgs e)
        {
            ProjectEnergyUsage energyusageWindow = new ProjectEnergyUsage(CarboLifeProject.energyProperties, CarboLifeProject.designLife);
            energyusageWindow.ShowDialog();

            if (energyusageWindow.isAccepted)
            {

                CarboLifeProject.energyProperties = energyusageWindow.projectEnergyProperties;
                CarboLifeProject.energyProperties.calculate(CarboLifeProject.designLife);
            }
            RefreshInterFace();

        }

        private void txt_EnergyPerYear_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void txt_A0Value_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            await Task.Delay(1000);
            CarboLifeProject.A0Global = Utils.ConvertMeToDouble(tb.Text) * 1000;
            RefreshInterFace();
        }


    }
}
