using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for MaterialBasePicker.xaml
    /// </summary>
    public partial class ProfileWindow : Window
    {
        public CarboDatabase materials;
        public CarboGroup concreteGroup;
        public CarboGroup profileGroup;

        List<Profile> profileList;

        public bool isAccepted;
        public double convertionFact;

        public ProfileWindow(CarboDatabase materialDatabase, CarboGroup myConcreteGroup)
        {
            isAccepted = false;
            materials = materialDatabase;
            concreteGroup = myConcreteGroup;
            profileGroup = new CarboGroup();
            profileGroup.Category = "Floor";
            profileGroup.Description = "Metal deck / Profile";
            InitializeComponent();
            convertionFact = 1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            profileList = LoadProfiles();
            //CarboLifeAPI.Utils.LoadCSV("");

            foreach (Profile pf in profileList)
            {
                    cbb_Profile.Items.Add(pf.name);
            }

            foreach (CarboMaterial cm in materials.CarboMaterialList)
            {
                if (cm.Category == "Steel" ||
                    cm.Name.Contains("metaldeck") ||
                    cm.Name.Contains("comflor"))
                {
                    cbb_ProfileMaterial.Items.Add(cm.Name);
                }
            }
            if(cbb_ProfileMaterial.Items.Count <= 0)
            {
                MessageBox.Show("No rebar or steel materials found in database, please create one and try again");
            }

            cbb_Profile.SelectedItem = cbb_Profile.Items[0];
            cbb_ProfileMaterial.SelectedItem = cbb_ProfileMaterial.Items[0];
            txt_Volume.Text = concreteGroup.Volume.ToString();

            refreshInterface();
        }

        private List<Profile> LoadProfiles()
        {
            List<Profile> result = new List<Profile>();

            //Find Profilelist;
            string myPath = Utils.getAssemblyPath() + "\\data\\" + "Profiles.csv";

            if(File.Exists(myPath))
            {
                DataTable profileTable = Utils.LoadCSV(myPath);
                foreach(DataRow dr in profileTable.Rows)
                {
                    Profile newProfile = new Profile();

                    string name = dr[0].ToString() + " - " + dr[1].ToString() + " - " +  dr[2].ToString();
                    double steel = Utils.ConvertMeToDouble(dr[3].ToString());
                    double constant = Utils.ConvertMeToDouble(dr[4].ToString());
                    double profileHeight = Utils.ConvertMeToDouble(dr[5].ToString());

                    newProfile.name = name;
                    newProfile.steel = steel;
                    newProfile.constant = constant;
                    newProfile.profileHeight = profileHeight;

                    result.Add(newProfile);
                }
            }
            else
            {
                MessageBox.Show("File: " + myPath + " could not be found, make sure you have the profile list located in indicated folder");
            }

            return result;
        }

        private void refreshInterface()
        {
            double volume = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Volume.Text);
            double thickness = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Thickness.Text);

            if (cbb_ProfileMaterial != null &&  cbb_Profile != null)
            {

                CarboMaterial material = materials.GetExcactMatch(cbb_ProfileMaterial.Text);
                Profile selectedProfile = profileList.Find(x=> x.name.Contains(cbb_Profile.Text));


                if (material != null && txt_Volume.Text != "" && thickness != 0 && selectedProfile != null)
                {
                    //=J13+((D13-K13)/1000)
                    //=Constsnt+((Thickness-ProfileHeight)/1000)
                    double area = volume / (thickness / 1000);
                    double constant = selectedProfile.constant;
                    double profileHeight = selectedProfile.profileHeight;

                    double conVolPerM2 = constant+((thickness - profileHeight)/1000);
                    double stlWeightPerM2 = selectedProfile.steel * 7850;


                    txt_ConcreteVolume.Text = Convert.ToString(Math.Round((conVolPerM2 * area),2));
                    txt_SteelVolume.Text = Convert.ToString(Math.Round(((stlWeightPerM2 * area) / 7850), 2));

                    convertionFact = (conVolPerM2 * area) / volume;
                    lbl_CalcCon.Content = Math.Round(conVolPerM2,2) + " m³/m² x " + Math.Round(area,2) + " m² Convertion = *" + Math.Round(convertionFact,2);
                    lbl_CalcSteel.Content = Math.Round(stlWeightPerM2,2) + " kg/m² x " + Math.Round(area,2) + " m²";

                }
            }
        }

        private void Btn_Accept_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = true;
            CarboMaterial material = materials.GetExcactMatch(cbb_ProfileMaterial.Text);

            //concreteGroup.Volume = Utils.ConvertMeToDouble(txt_ConcreteVolume.Text);
            concreteGroup.Correction = "*" + Math.Round(convertionFact, 3).ToString();
            concreteGroup.Description += " - Corrected volume";

            profileGroup.Volume = Utils.ConvertMeToDouble(txt_SteelVolume.Text);
            profileGroup.Description = cbb_Profile.Text;
            profileGroup.Material = material;
            profileGroup.CalculateTotals();

            this.Close();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void Cbb_Profile_DropDownClosed(object sender, EventArgs e)
        {
            refreshInterface();
        }

        private void Cbb_ProfileMaterial_DropDownClosed(object sender, EventArgs e)
        {
            refreshInterface();

        }

        private void Txt_Thickness_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshInterface();
        }
    }


    public class Profile
    {
        public string name { get; set; }

        public double steel { get; set; }
        public double constant { get; set; }
        public double profileHeight { get; set; }

        public Profile()
        {
            name = "";
            steel = 0;
            constant = 0;
            profileHeight = 0;
        }
    }
}
