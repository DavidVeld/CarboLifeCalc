using Autodesk.Revit.DB;
using CarboCircle.data;
using CarboLifeAPI;
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

namespace CarboCircle.UI
{
    /// <summary>
    /// Interaction logic for CarboCircleSettings.xaml
    /// </summary>
    public partial class CarboCircleSettings : Window
    {
        public carboCircleSettings settings;
        public bool isAccepted { get; internal set; }

        public CarboCircleSettings()
        {
            InitializeComponent();
        }

        public CarboCircleSettings(carboCircleProject activeProject)
        {
            this.settings = activeProject.settings.Copy();
            InitializeComponent();

        }

        private void btn_Accept_Click(object sender, RoutedEventArgs e)
        {

            settings.ConsiderColumnBeams = chk_MineSteelBeams.IsChecked.Value;
            settings.ConsiderSlabs = chk_MineFloors.IsChecked.Value;
            settings.ConsiderWalls = chk_MineWalls.IsChecked.Value;

            settings.MineParameterName = txt_MinedParameter.Text;
            settings.RequiredParameterName = txt_RequiredParameter.Text;
            settings.gradeParameter = txt_SteelGradeParameter.Text;

            settings.cutoffbeamLength = double.Parse(txt_CutoffValue.Text);
            settings.MasonryLoss = int.Parse(txt_MasonryLoss.Text);
            settings.VolumeLoss = int.Parse(txt_ConcreteLoss.Text);

            settings.depthRange = double.Parse(txt_SteelBeamDepthTolerance.Text); //in mm
            settings.strengthRange = double.Parse(txt_BeamStrengthTolerance.Text); //in percent

            settings.Save();

            isAccepted = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(settings != null)
            {
                chk_MineSteelBeams.IsChecked = settings.ConsiderColumnBeams;
                chk_MineFloors.IsChecked = settings.ConsiderSlabs;
                chk_MineWalls.IsChecked = settings.ConsiderWalls;

                txt_MinedParameter.Text = settings.MineParameterName;
                txt_RequiredParameter.Text = settings.RequiredParameterName;
                txt_SteelGradeParameter.Text = settings.gradeParameter;

                txt_CutoffValue.Text = settings.cutoffbeamLength.ToString();
                txt_MasonryLoss.Text = settings.MasonryLoss.ToString();
                txt_ConcreteLoss.Text = settings.VolumeLoss.ToString();

                if (settings.dataBasePath == "")
                    txt_SteelDataBasePath.Text = "Local";
                else
                    txt_SteelDataBasePath.Text += settings.dataBasePath;

                txt_SteelBeamDepthTolerance.Text = settings.depthRange.ToString(); //in mm
                txt_BeamStrengthTolerance.Text = settings.strengthRange.ToString(); //in percent

            }
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void btn_Browse_Click(object sender, RoutedEventArgs e)
        {
            string currentDir = Utils.getAssemblyPath() + "\\db\\";

            if (!Directory.Exists(currentDir))
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = "Carbo Circle Section Database (*.csv)|*.csv";
                if (Directory.Exists(currentDir))
                    openFileDialog.InitialDirectory = currentDir;

                var path = openFileDialog.ShowDialog();
                if (openFileDialog.FileName != "")
                {
                    FileInfo finfo = new FileInfo(openFileDialog.FileName);

                    if (openFileDialog.FileName != "")
                    {
                        settings.dataBasePath = openFileDialog.FileName;
                        txt_SteelDataBasePath.Text = settings.dataBasePath;
                    }
                }
            }
        }
    }
}
