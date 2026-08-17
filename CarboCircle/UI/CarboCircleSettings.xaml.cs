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
            settings.ConsiderColumnBeams = chk_MineSteelBeams.IsChecked == true;
            settings.ConsiderSlabs = chk_MineFloors.IsChecked == true;
            settings.ConsiderWalls = chk_MineWalls.IsChecked == true;

            settings.MineParameterName = txt_MinedParameter.Text;
            settings.RequiredParameterName = txt_RequiredParameter.Text;
            settings.gradeParameter = txt_SteelGradeParameter.Text;

            settings.timberWidthParameter = txt_ParameterWidth.Text;
            settings.timberDepthParameter = txt_ParameterDepth.Text;

            //Utils.ConvertMeToDouble rather than double.Parse: the old code threw a
            //FormatException on an empty or comma-decimal entry and took the dialog with it.
            settings.cutoffbeamLength = readDouble(txt_CutoffValue, settings.cutoffbeamLength);
            settings.timberCutoffLength = readDouble(txt_WoodCutoff, settings.timberCutoffLength);

            settings.MasonryLoss = readInt(txt_MasonryLoss, settings.MasonryLoss);
            settings.VolumeLoss = readInt(txt_ConcreteLoss, settings.VolumeLoss);

            settings.depthRange = readDouble(txt_SteelBeamDepthTolerance, settings.depthRange); //in mm
            settings.strengthRange = readDouble(txt_BeamStrengthTolerance, settings.strengthRange); //in percent

            settings.Save();

            isAccepted = true;
            this.Close();
        }

        /// <summary>
        /// Reads a number from a textbox, keeping the previous value if the box does not
        /// hold one.
        /// </summary>
        private static double readDouble(System.Windows.Controls.TextBox box, double fallback)
        {
            if (box == null || string.IsNullOrWhiteSpace(box.Text))
                return fallback;

            return Utils.ConvertMeToDouble(box.Text);
        }

        private static int readInt(System.Windows.Controls.TextBox box, int fallback)
        {
            if (box == null || string.IsNullOrWhiteSpace(box.Text))
                return fallback;

            return Convert.ToInt32(Math.Round(Utils.ConvertMeToDouble(box.Text)));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (settings != null)
            {
                chk_MineSteelBeams.IsChecked = settings.ConsiderColumnBeams;
                chk_MineFloors.IsChecked = settings.ConsiderSlabs;
                chk_MineWalls.IsChecked = settings.ConsiderWalls;

                txt_MinedParameter.Text = settings.MineParameterName;
                txt_RequiredParameter.Text = settings.RequiredParameterName;
                txt_SteelGradeParameter.Text = settings.gradeParameter;

                txt_ParameterWidth.Text = settings.timberWidthParameter;
                txt_ParameterDepth.Text = settings.timberDepthParameter;

                txt_CutoffValue.Text = settings.cutoffbeamLength.ToString();
                txt_WoodCutoff.Text = settings.timberCutoffLength.ToString();

                txt_MasonryLoss.Text = settings.MasonryLoss.ToString();
                txt_ConcreteLoss.Text = settings.VolumeLoss.ToString();

                showDatabasePath(txt_SteelDataBasePath, settings.dataBasePath);
                showDatabasePath(txt_MaterialDataBasePath, settings.materialDataBasePath);

                txt_SteelBeamDepthTolerance.Text = settings.depthRange.ToString(); //in mm
                txt_BeamStrengthTolerance.Text = settings.strengthRange.ToString(); //in percent
            }
        }

        /// <summary>
        /// An empty setting means "use the copy shipped in circledb".
        /// </summary>
        private static void showDatabasePath(System.Windows.Controls.TextBox box, string configuredPath)
        {
            box.Text = string.IsNullOrEmpty(configuredPath) ? "Local" : configuredPath;
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            isAccepted = false;
            this.Close();
        }

        private void btn_Browse_Click(object sender, RoutedEventArgs e)
        {
            string picked = browseForDatabase("Carbo Circle Section Database (*.csv)|*.csv");

            if (picked == null)
                return;

            settings.dataBasePath = picked;
            showDatabasePath(txt_SteelDataBasePath, picked);
        }

        private void btn_BrowseMaterials_Click(object sender, RoutedEventArgs e)
        {
            string picked = browseForDatabase("Carbo Life Material Database (*.cxml)|*.cxml");

            if (picked == null)
                return;

            settings.materialDataBasePath = picked;
            showDatabasePath(txt_MaterialDataBasePath, picked);
        }

        /// <summary>
        /// Asks for a database file, starting in circledb. Returns null when the user
        /// cancels.
        ///
        /// The previous version guarded the whole dialog with !Directory.Exists(...), so the
        /// button did nothing whenever the folder was actually there.
        /// </summary>
        private static string browseForDatabase(string filter)
        {
            string startDir = System.IO.Path.Combine(Utils.getAssemblyPath(), "circledb");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;

            if (Directory.Exists(startDir))
                openFileDialog.InitialDirectory = startDir;

            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return null;

            if (string.IsNullOrEmpty(openFileDialog.FileName) || !File.Exists(openFileDialog.FileName))
                return null;

            return openFileDialog.FileName;
        }
    }
}