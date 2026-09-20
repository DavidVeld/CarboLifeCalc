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

            //Through toSetting rather than straight off the text: it maps the "Type name"
            //the user sees back onto the empty string the settings file has always stored,
            //so a file written here still loads in an older build and vice versa.
            settings.MineParameterName = carboCircleParameterNames.toSetting(
                carboCircleParameterPicker.textOf(cmb_MinedParameter));

            settings.RequiredParameterName = carboCircleParameterNames.toSetting(
                carboCircleParameterPicker.textOf(cmb_RequiredParameter));

            //Stored as typed. An emptied box comes back as the default on the next read
            //rather than being stored as a default here, so clearing it stays a way of
            //saying "whatever the default is" instead of freezing today's default in.
            settings.reuseIdParameter = carboCircleParameterPicker.textOf(cmb_ReuseIdParameter);

            //gradeParameter, timberWidthParameter and timberDepthParameter are deliberately
            //not written. The import works those out for itself, so the window shows where
            //they come from rather than offering a setting - and writing the displayed text
            //back would overwrite whatever an older version left in the file with a
            //sentence of prose.

            //Utils.ConvertMeToDouble rather than double.Parse: the old code threw a
            //FormatException on an empty or comma-decimal entry and took the dialog with it.
            settings.cutoffbeamLength = readDouble(txt_CutoffValue, settings.cutoffbeamLength);
            settings.timberCutoffLength = readDouble(txt_WoodCutoff, settings.timberCutoffLength);

            settings.MasonryLoss = readInt(txt_MasonryLoss, settings.MasonryLoss);
            settings.VolumeLoss = readInt(txt_ConcreteLoss, settings.VolumeLoss);

            settings.depthRange = readDouble(txt_SteelBeamDepthTolerance, settings.depthRange); //in mm
            settings.strengthRange = readDouble(txt_BeamStrengthTolerance, settings.strengthRange); //in percent
            settings.widthRange = readDouble(txt_WidthTolerance, settings.widthRange); //in mm
            settings.minOffcutLength = readDouble(txt_MinOffcutLength, settings.minOffcutLength); //in mm
            settings.allowCrossFamilySubstitution = chk_AllowCrossFamily.IsChecked == true;

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

                //Each picker gets its own list. Sharing one would have them filter each
                //other - see carboCircleParameterPicker.
                //
                //The two section-name pickers offer TYPE parameters, because the override is
                //applied with ElementType.LookupParameter. The reuse id picker offers
                //INSTANCE parameters, because the id is written to the element.
                carboCircleParameterPicker.attach(cmb_MinedParameter,
                    carboCircleParameterNames.forSectionPicker(settings.MineParameterName),
                    carboCircleParameterNames.toDisplay(settings.MineParameterName));

                carboCircleParameterPicker.attach(cmb_RequiredParameter,
                    carboCircleParameterNames.forSectionPicker(settings.RequiredParameterName),
                    carboCircleParameterNames.toDisplay(settings.RequiredParameterName));

                string reuseId = settings.reuseIdParameterOrDefault();

                carboCircleParameterPicker.attach(cmb_ReuseIdParameter,
                    carboCircleParameterNames.forInstancePicker(reuseId),
                    reuseId);

                //The dropdowns are filled from the last model read. Say so when that has
                //not happened, rather than leaving the user to wonder why they are empty.
                txt_NoParametersNote.Visibility = carboCircleParameterNames.hasNames()
                    ? System.Windows.Visibility.Collapsed
                    : System.Windows.Visibility.Visible;

                //The three boxes under "Worked out automatically" carry their text from the
                //XAML. Nothing is loaded into them, because nothing in the settings file
                //controls what they describe.

                txt_CutoffValue.Text = settings.cutoffbeamLength.ToString();
                txt_WoodCutoff.Text = settings.timberCutoffLength.ToString();

                txt_MasonryLoss.Text = settings.MasonryLoss.ToString();
                txt_ConcreteLoss.Text = settings.VolumeLoss.ToString();

                showDatabasePath(txt_SteelDataBasePath, settings.dataBasePath);
                showDatabasePath(txt_MaterialDataBasePath, settings.materialDataBasePath);

                txt_SteelBeamDepthTolerance.Text = settings.depthRange.ToString(); //in mm
                txt_BeamStrengthTolerance.Text = settings.strengthRange.ToString(); //in percent
                txt_WidthTolerance.Text = settings.widthRange.ToString(); //in mm
                txt_MinOffcutLength.Text = settings.minOffcutLength.ToString(); //in mm
                chk_AllowCrossFamily.IsChecked = settings.allowCrossFamilySubstitution;

                //Setting .Text above raises TextChanged, which refreshes these anyway. Called
                //explicitly so the state does not depend on that: if a box already held the
                //same string, WPF raises nothing and the notes would keep a previous verdict.
                refreshCutoffWarnings();
            }
        }

        private void txt_Cutoff_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshCutoffWarnings();
        }

        /// <summary>
        /// Shows or hides the note under each cut-off row as the value is typed.
        ///
        /// Advisory only - nothing is clamped or rejected. The same note, off the same
        /// constants, appears on the main window, so a value lowered here cannot slip past a
        /// warning the other window would have raised.
        /// </summary>
        private void refreshCutoffWarnings()
        {
            setCutoffWarning(txt_CutoffValue, txt_CutoffValueWarning,
                carboCircleSettings.SteelCutoffAdvisoryMin);

            setCutoffWarning(txt_WoodCutoff, txt_WoodCutoffWarning,
                carboCircleSettings.TimberCutoffAdvisoryMin);
        }

        private static void setCutoffWarning(System.Windows.Controls.TextBox box,
            System.Windows.Controls.TextBlock note, double advisoryMin)
        {
            //Called from TextChanged, which can fire while the tree is still being built.
            if (box == null || note == null)
                return;

            double value;

            //Nothing to say about a box that is empty or half-typed. Only a value that parses
            //and comes out low is a decision worth flagging.
            bool parsed = double.TryParse((box.Text ?? "").Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out value);

            if (parsed && value < advisoryMin)
            {
                note.Text = carboCircleSettings.CutoffAdvisoryMessage
                    + " (below " + advisoryMin.ToString("N0") + " mm).";
                note.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                note.Visibility = System.Windows.Visibility.Collapsed;
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