using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for MaterialLifePicker.xaml
    /// </summary>
    public partial class MaterialMyEPD : Window
    {
        internal bool isAccepted;
        internal bool areaCalc;
        internal bool isloaded;
        //public CarboMaterial material;

        public double Density;
        public double Thickness;

        public string URL;

        public double m3A1, m3A2, m3A3, m3A4, m3A5, m3B17, m3C1, m3C2, m3C3, m3C4, m3D;
        public double kgA1, kgA2, kgA3, kgA4, kgA5, kgB17, kgC1, kgC2, kgC3, kgC4, kgD;

        private void btn_OpenLink_Click(object sender, RoutedEventArgs e)
        {
            string link = txt_EPDLink.Text;
            CarboLifeAPI.Utils.Openlink(link);
        }

        public MaterialMyEPD()
        {
            isloaded = false;

            Density = 0;
            Thickness = 1;
            areaCalc = false;
            URL = "";

            InitializeComponent();
        }

        public MaterialMyEPD(double density, string link)
        {
            isloaded = false;
            Density = density;
            Thickness = 1;
            areaCalc = false;
            URL = link;

            InitializeComponent();
        }

        private void rad_m3_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSettings();
            UpdateValue();
        }

        private void rad_m2_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSettings();
            UpdateValue();
        }

        private async void txt_Update_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                UpdateValue();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isloaded = true;
            txt_EPDLink.Text = URL;
            UpdateSettings();
            UpdateValue();
        }

        private void UpdateSettings()
        {
            if (isloaded == true)
            {
                txt_Thickness.Text = Thickness.ToString();
                txt_Density.Text = Density.ToString();
                if (rad_m2 != null && rad_m3 != null)
                {
                    if (rad_m2.IsChecked == true)
                    {
                        areaCalc = true;
                        txt_Thickness.Visibility = Visibility.Visible;
                        txt_Thickness.IsReadOnly = false;
                        txt_Thickness.Foreground = Brushes.Black;
                        txt_Thickness.Background = Brushes.White;

                        lbl_Thickness.Foreground = Brushes.Black;
                        lbl_ThicknessUnit.Foreground = Brushes.Black;

                        lbl_Unit.Content = "eCO₂/m²";
                    }
                    else
                    {
                        areaCalc = false;
                        txt_Thickness.Visibility = Visibility.Visible;
                        txt_Thickness.IsReadOnly = true;
                        txt_Thickness.Foreground = Brushes.LightGray;
                        txt_Thickness.Background = Brushes.LightGray;

                        lbl_Thickness.Foreground = Brushes.LightGray;
                        lbl_ThicknessUnit.Foreground = Brushes.LightGray;

                        lbl_Unit.Content = "eCO₂/m³";
                    }
                }
            }
        }

        private void UpdateValue()
        {
            if (isloaded == true)
            {
                Density = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Density.Text);
                Thickness = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Thickness.Text);

                ConvertInput();
            }
        }

        private void ConvertInput()
        {
            m3A1 = ValidateValueCallback(txt_m3A1.Text);
            m3A2 = ValidateValueCallback(txt_m3A2.Text);
            m3A3 = ValidateValueCallback(txt_m3A3.Text);
            m3A4 = ValidateValueCallback(txt_m3A4.Text);
            m3A5 = ValidateValueCallback(txt_m3A5.Text);
            m3B17 = ValidateValueCallback(txt_m3B17.Text);
            m3C1 = ValidateValueCallback(txt_m3C1.Text);
            m3C2 = ValidateValueCallback(txt_m3C2.Text);
            m3C3 = ValidateValueCallback(txt_m3C3.Text);
            m3C4 = ValidateValueCallback(txt_m3C4.Text);
            m3D = ValidateValueCallback(txt_m3D.Text);

            double perCube = 1;
            if (areaCalc == true)
            {
                perCube = 1000 / Thickness;
            }           

            kgA1 = ((m3A1 * perCube) / Density);
            kgA2 = ((m3A2 * perCube) / Density);
            kgA3 = ((m3A3 * perCube) / Density);
            kgA4 = ((m3A4 * perCube) / Density);
            kgA5 = ((m3A5 * perCube) / Density);
            kgB17 = ((m3B17 * perCube) / Density);
            kgC1 = ((m3C1 * perCube) / Density);
            kgC2 = ((m3C2 * perCube) / Density);
            kgC3 = ((m3C3 * perCube) / Density);
            kgC4 = ((m3C4 * perCube) / Density);
            kgD = ((m3D * perCube) / Density);

            txt_kgA1.Text = Math.Round(kgA1, 4).ToString();
            txt_kgA2.Text = Math.Round(kgA2, 4).ToString();
            txt_kgA3.Text = Math.Round(kgA3, 4).ToString();
            txt_kgA4.Text = Math.Round(kgA4, 4).ToString();
            txt_kgA5.Text = Math.Round(kgA5, 4).ToString();
            txt_kgB17.Text = Math.Round(kgB17, 4).ToString();
            txt_kgC1.Text = Math.Round(kgC1, 4).ToString();
            txt_kgC2.Text = Math.Round(kgC2, 4).ToString();
            txt_kgC3.Text = Math.Round(kgC3, 4).ToString();
            txt_kgC4.Text = Math.Round(kgC4, 4).ToString();
            txt_kgD.Text = Math.Round(kgD, 4).ToString();
        }

        private double ValidateValueCallback(string text)
        {
            double result = 0;

            //Check if value contains "E"
            if (text.Contains("E"))
            {
                //Validate Using "E" value
                string[] value = text.Split('E');

                if (value.Length == 2)
                {
                    double left = CarboLifeAPI.Utils.ConvertMeToDouble(value[0]);
                    double right = CarboLifeAPI.Utils.ConvertMeToDouble(value[1]);

                    double ten = Math.Pow(10,right);
                    result = left * ten;
                }                
            }
            else
            {
                result = CarboLifeAPI.Utils.ConvertMeToDouble(text);
            }

            return result;
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;

            this.Close();
        }

        private async void txt_m3A1_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int startLength = tb.Text.Length;

            await Task.Delay(250);
            if (startLength == tb.Text.Length)
            {
                UpdateValue();
            }
        }

        internal double getA1A3()
        {
            double result = kgA1 + kgA2 + kgA3;
            return result;
        }

        internal double getC14()
        {
            double result = kgC1 + kgC2 + kgC3 + kgC4;
            return result;
        }
    }
}
