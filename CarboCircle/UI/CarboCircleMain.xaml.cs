using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CarboCircle.data;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using CarboLifeUI.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
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
    /// Interaction logic for CarboCircleMain.xaml
    /// </summary>
    public partial class CarboCircleMain : Window
    {
        //Used for Revit handlers
        private CarboCircleHandler m_Handler;
        private ExternalEvent m_ExEvent;

        /// <summary>
        /// This window's project.
        ///
        /// An instance field, not a static one. It was static, and shared by every window the
        /// session ever created - so a stale window answering the same import event wrote its
        /// own interpretation of that import into the one project everybody was looking at.
        /// Per-window state means a second window, however it came about, cannot corrupt the
        /// first.
        /// </summary>
        private carboCircleProject activeProject;

        private List<carboCircleElement> collectedElements;

        public CarboCircleMain(ExternalEvent exEvent, CarboCircleHandler handler)
        {
            InitializeComponent();

            this.m_ExEvent = exEvent;
            this.m_Handler = handler;

            try
            {
                //initiate new projece
                activeProject = new carboCircleProject();
                //Load() always hands back a usable instance, so no Copy() is needed here.
                activeProject.settings = new carboCircleSettings().Load();

                // Subscribe to the DataReady event
                m_Handler.DataReady += OnDataReady;

                //And let go of it when this window really goes away. The handler outlives every
                //window - it is created once per session - so a window that closed while still
                //subscribed would keep being handed imports it can no longer show, and would
                //keep itself alive through the handler's reference for as long as Revit runs.
                this.Closed += (s, args) =>
                {
                    if (m_Handler != null)
                        m_Handler.DataReady -= OnDataReady;

                    FormStatusChecker.isWindowOpen = false;
                };
            }
            catch (Exception ex)
            {
                this.Close();
            }

        }


        private void OnDataReady(object sender, carboCircleImportResult result)
        {
            //A failed import leaves the previous results on screen rather than wiping them; the
            //handler has already said what went wrong.
            if (result == null || result.Elements == null)
                return;

            if (!result.ForProject)
            {
                collectedElements = result.Elements;
                activeProject.ParseMinedData(collectedElements);

                liv_MinedData.ItemsSource = null;
                liv_MinedData.ItemsSource = activeProject.minedData;

                liv_MinedMassObjects.ItemsSource = null;
                liv_MinedMassObjects.ItemsSource = activeProject.minedVolumes;

                setMineOk();
            }
            else
            {
                collectedElements = result.Elements;
                activeProject.ParseRequiredData(collectedElements);

                liv_requiredMaterialList.ItemsSource = null;
                liv_requiredMaterialList.ItemsSource = activeProject.requiredData;

                liv_RequiredMassObjects.ItemsSource = null;
                liv_RequiredMassObjects.ItemsSource = activeProject.requiredVolumes;

                setRequiredOk();
            }
        }

        private void setRequiredOk()
        {
            setStepState(btn_GotoProject, "Nothing loaded yet", "required",
                count(activeProject.requiredData), count(activeProject.requiredVolumes));
        }

        private void setMineOk()
        {
            setStepState(btn_GotoMine, "Nothing recovered yet", "recovered",
                count(activeProject.minedData), count(activeProject.minedVolumes));
        }

        /// <summary>
        /// Puts one of the two set-up step buttons into the state its side is actually in, and
        /// writes the totals onto it.
        ///
        /// Red while the step is outstanding, green once elements have arrived. These buttons
        /// are the only thing on the set-up tab that reports whether a side has data, and an
        /// empty side is the usual reason Find opportunities comes back with nothing - so the
        /// state is worth the strongest signal the window has.
        ///
        /// Zero counts as outstanding. An import that ran and returned nothing is the case most
        /// worth flagging, and going green on an empty list would say the opposite.
        ///
        /// Style is swapped rather than Background, so the border and the text colour move with
        /// the fill: on a solid button, changing one of the three and not the others reads as a
        /// rendering fault rather than as a state.
        /// </summary>
        private void setStepState(System.Windows.Controls.Button button, string waitingText,
            string noun, int members, int volumes)
        {
            if (button == null)
                return;

            if (members + volumes <= 0)
            {
                button.Style = (Style)FindResource("CircleStepButton");
                button.Content = waitingText;
                return;
            }

            //Members and volumes are two different things - one is cut to length, the other is
            //taken by volume - and the tab below shows them in two separate grids, so a single
            //summed figure would not match anything the user can count.
            string text = "Review " + members.ToString("N0") + " " + noun + " element"
                + (members == 1 ? "" : "s");

            if (volumes > 0)
                text += " and " + volumes.ToString("N0") + " volume" + (volumes == 1 ? "" : "s");

            button.Style = (Style)FindResource("CircleStepButtonReady");
            button.Content = text;
        }

        private static int count<T>(List<T> list)
        {
            return list == null ? 0 : list.Count;
        }

        private void btn_ImportmaterialsRevit_Click(object sender, RoutedEventArgs e)
        {
            //Remember the choice as the mine side preference, then hand it to the import
            //that is about to run. The import is told the method directly: it is a property
            //of this one request, and the settings file only remembers preferences.
            string method = chosenMethod(cbb_MineSetting, activeProject.settings.MineExtractionMethod);

            activeProject.settings.MineExtractionMethod = method;
            activeProject.settings.Save();

            if (m_ExEvent != null)
            {
                m_Handler.SetSwitch(1);
                m_Handler.SetSettings(activeProject);
                m_Handler.SetExtractionMethod(method);
                m_Handler.SetImportSide(false);

                m_ExEvent.Raise();
            }
        }

        private void btn_ImportProjectRevit_Click(object sender, RoutedEventArgs e)
        {
            //Remember the choice as the project side preference, then hand it to the
            //import that is about to run. See the mine side above.
            string method = chosenMethod(cbb_ImportProjectSetting, activeProject.settings.RequiredExtractionMethod);

            activeProject.settings.RequiredExtractionMethod = method;
            activeProject.settings.Save();

            if (m_ExEvent != null)
            {
                m_Handler.SetSwitch(1);
                m_Handler.SetSettings(activeProject);
                m_Handler.SetExtractionMethod(method);
                m_Handler.SetImportSide(true);

                m_ExEvent.Raise();
            }
        }

        private void btn_Visualise_Click(object sender, RoutedEventArgs e)
        {

            if (m_ExEvent != null)
            {
                m_Handler.SetSwitch(2);
                m_Handler.SetSettings(activeProject);

                m_ExEvent.Raise();
            }
        }

        /// <summary>
        /// Stamps the reuse id onto both ends of every matched pair.
        ///
        /// Confirmed first, because it is the only thing on this window that changes the
        /// model - and it is one undo step, which is worth saying while the user still has
        /// the chance not to press it.
        /// </summary>
        private void btn_WriteReuseIds_Click(object sender, RoutedEventArgs e)
        {
            if (m_ExEvent == null)
                return;

            //Checked here rather than left to the handler: nothing to write is a question
            //about what is on screen, and answering it from the window means the user is
            //not made to wait for an external event to tell them no.
            if (activeProject.carboCircleMatchedPairs == null ||
                activeProject.carboCircleMatchedPairs.Count == 0)
            {
                //Fully qualified: this file uses both WinForms and WPF, and both have a
                //MessageBox.
                System.Windows.MessageBox.Show(
                    "There are no matched pairs to write yet. Find the opportunities first.",
                    "Nothing to write", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string parameterName = activeProject.settings.reuseIdParameterOrDefault();

            MessageBoxResult go = System.Windows.MessageBox.Show(
                "Write the reuse ID of every matched pair into the instance parameter \"" +
                parameterName + "\", on the proposed member and on the existing member it " +
                "comes from?" + Environment.NewLine + Environment.NewLine +
                "This changes the model. It is a single undo step, and anything already in " +
                "that parameter on those members is overwritten.",
                "Write reuse IDs", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (go != MessageBoxResult.OK)
                return;

            storeSettings();

            m_Handler.SetSwitch(5);
            m_Handler.SetSettings(activeProject);

            m_ExEvent.Raise();
        }

        private void btn_Select_Click(object sender, RoutedEventArgs e)
        {
            if (liv_MatchedFraming.SelectedItem != null)
            {
                try
                {
                    carboCircleMatchElement selectedMatch = liv_MatchedFraming.SelectedItem as carboCircleMatchElement;

                    //A no-match row has no member behind it, so there is nothing in the model to
                    //go and look at. Asking Revit to select it would send it an id that resolves
                    //to nothing.
                    if (selectedMatch != null && selectedMatch.matchRank == carboCircleMatchRules.ClassNoMatch)
                    {
                        System.Windows.MessageBox.Show(
                            "This requirement has no match yet, so there is no existing member to show." +
                            Environment.NewLine + Environment.NewLine + selectedMatch.description,
                            "Nothing to select", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    if (selectedMatch != null)
                    {
                        if (m_ExEvent != null)
                        {
                            m_Handler.SetSwitch(3);
                            m_Handler.SetSettings(selectedMatch);

                            m_ExEvent.Raise();
                        }
                    }
                }
                catch { }
            }
        }

        /*
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //Before the form is closed, everything must be disposed properly
            try
            {
                activeProject.settings.Save();

                m_ExEvent.Dispose();
                m_ExEvent = null;

                //clear the handler
                m_Handler._revitEvent.Dispose();
                m_Handler._revitEvent = null;
                m_Handler = null;

                FormStatusChecker.isWindowOpen = false;
                //You have to call the base class
                base.OnClosing(e);
            }
            catch 
            {
            }

        }
        */
        private void btn_GotoMine_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => tab_Main.SelectedIndex = 1));
        }
        private void btn_GotoProject_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => tab_Main.SelectedIndex = 2));
        }
        private void btn_ImportProjectSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            LoadInterFaceFromSettings();

        }

        private void LoadInterFaceFromSettings()
        {


            cbb_ImportProjectSetting.Items.Clear();
            cbb_MineSetting.Items.Clear();

            //Filled from the same constants the collector switches on, so a label and the
            //branch it is meant to select cannot drift apart.
            foreach (string method in carboCircleExtractionMethod.RequiredMethods())
                cbb_ImportProjectSetting.Items.Add(method);

            foreach (string method in carboCircleExtractionMethod.MineMethods())
                cbb_MineSetting.Items.Add(method);

            //The two sides offer different methods, so each remembers its own choice.
            selectRemembered(cbb_ImportProjectSetting, activeProject.settings.RequiredExtractionMethod,
                carboCircleExtractionMethod.AllNewInView);
            selectRemembered(cbb_MineSetting, activeProject.settings.MineExtractionMethod,
                carboCircleExtractionMethod.AllDemolishedInView);

            txt_BeamStrengthTolerance.Text = activeProject.settings.strengthRange.ToString();
            txt_SteelBeamDepthTolerance.Text = activeProject.settings.depthRange.ToString();

            txt_SteelCutoffLength.Text = activeProject.settings.cutoffbeamLength.ToString();
            txt_TimberCutoffLength.Text = activeProject.settings.timberCutoffLength.ToString();

            //The name beside the write button, read off the same settings the write uses -
            //so what the button says it will do and what it does cannot disagree.
            txt_ReuseIdParameter.Text = activeProject.settings.reuseIdParameterOrDefault();

            //Setting .Text above raises TextChanged, which refreshes these anyway. Called
            //explicitly so the state does not depend on that: if either box already held the
            //same string, WPF raises nothing and the notes would keep a previous verdict.
            refreshCutoffWarnings();

            //load colours
            btn_ColourMinedNotReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                activeProject.settings.colour_NotReused.r, activeProject.settings.colour_NotReused.g, activeProject.settings.colour_NotReused.b));
            btn_ColourMinedReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                activeProject.settings.colour_ReusedMinedData.r, activeProject.settings.colour_ReusedMinedData.g, activeProject.settings.colour_ReusedMinedData.b));

            btn_ColourRequiredNotReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                activeProject.settings.colour_NotFromReused.r, activeProject.settings.colour_NotFromReused.g, activeProject.settings.colour_NotFromReused.b));
            btn_ColourRequiredReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                activeProject.settings.colour_FromReusedData.r, activeProject.settings.colour_FromReusedData.g, activeProject.settings.colour_FromReusedData.b));
            btn_ColourMassReusable.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                activeProject.settings.colour_ReusedMinedVolumes.r, activeProject.settings.colour_ReusedMinedVolumes.g, activeProject.settings.colour_ReusedMinedVolumes.b));

            //Both step buttons read their own state rather than being left on whatever the
            //markup happened to say. activeProject is static and outlives the window, so on a
            //second open the two sides may well already hold data - the buttons come up green,
            //with the totals, instead of claiming nothing has been read.
            setMineOk();
            setRequiredOk();

        }

        /// <summary>
        /// Selects the remembered entry, falling back to <paramref name="fallback"/> when the
        /// setting is empty or is not one of the methods this side offers. Named rather than
        /// positional: the fallback used to be "index 1", which only happened to be the
        /// intended default.
        /// </summary>
        private static void selectRemembered(System.Windows.Controls.ComboBox combo, string remembered, string fallback)
        {
            int index = string.IsNullOrEmpty(remembered) ? -1 : combo.Items.IndexOf(remembered);

            if (index < 0)
                index = combo.Items.IndexOf(fallback);

            combo.SelectedIndex = index >= 0 ? index : 0;
        }

        /// <summary>
        /// The extraction method chosen in a combo, falling back to the remembered
        /// preference when nothing is selected.
        ///
        /// Never returns empty. This value now decides which branch the collector takes, so
        /// an empty one would silently mean "everything visible" - which is how the import
        /// used to ignore the choice altogether.
        /// </summary>
        private static string chosenMethod(System.Windows.Controls.ComboBox combo, string remembered)
        {
            string chosen = combo.SelectedItem as string;

            if (!string.IsNullOrEmpty(chosen))
                return chosen;

            return string.IsNullOrEmpty(remembered)
                ? carboCircleExtractionMethod.AllVisibleInView
                : remembered;
        }

        private void btn_Go_Click(object sender, RoutedEventArgs e)
        {
            // Get active settings. The two tolerances are separate values - the depth box
            // used to be written into both, silently discarding the strength tolerance.
            storeSettings();

            // Main script:
            //
            //Volumes need only a mine: crushing demolished concrete for aggregate does not
            //depend on there being a proposed frame. Requiring both lists meant a
            //concrete-only or masonry-only mine produced nothing at all, said nothing, and
            //left the previous run on screen.
            if (activeProject.minedData.Count > 0 || activeProject.minedVolumes.Count > 0)
            {
                activeProject.FindOpportunities();

                liv_MatchedFraming.ItemsSource = null;
                liv_MatchedFraming.ItemsSource = activeProject.getCarboMatchesListSimplified();
                applyMatchGrouping(liv_MatchedFraming);

                liv_MatchedVolumes.ItemsSource = null;
                liv_MatchedVolumes.ItemsSource = activeProject.getCarboVolumeOpportunities();

                liv_LeftOverData.ItemsSource = null;
                liv_LeftOverData.ItemsSource = activeProject.getLeftOverData();
                applyLeftOverGrouping(liv_LeftOverData);
            }
        }

        /// <summary>
        /// Groups the matched-framing grid by match category, in priority order.
        ///
        /// Applied here rather than in the markup because the list is rebuilt from scratch on
        /// every run: the getter hands back a fresh List each call, so its default view is new
        /// each time and anything configured on the previous one is gone. It also has to run
        /// AFTER the real ItemsSource assignment - there is no view to configure while the
        /// source is still null.
        ///
        /// Sorted on matchRank rather than on the label, so the groups come out in the order
        /// that matters - exact matches first, the work still to do last - instead of
        /// alphabetically.
        /// </summary>
        private static void applyMatchGrouping(System.Windows.Controls.ListView list)
        {
            System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(list.ItemsSource);

            if (view == null)
                return;

            using (view.DeferRefresh())
            {
                //Cleared before adding. If the source list is ever cached rather than rebuilt,
                //adding without clearing would nest the same grouping inside itself on the
                //second run.
                view.GroupDescriptions.Clear();
                view.SortDescriptions.Clear();

                view.SortDescriptions.Add(new System.ComponentModel.SortDescription(
                    "matchRank", System.ComponentModel.ListSortDirection.Ascending));
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription(
                    "required_length", System.ComponentModel.ListSortDirection.Descending));

                view.GroupDescriptions.Add(new PropertyGroupDescription("matchCategory"));
            }
        }

        /// <summary>
        /// Groups the leftovers grid into remnants and whole members nobody wanted - two
        /// different kinds of leftover that call for different action.
        /// </summary>
        private static void applyLeftOverGrouping(System.Windows.Controls.ListView list)
        {
            System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(list.ItemsSource);

            if (view == null)
                return;

            using (view.DeferRefresh())
            {
                view.GroupDescriptions.Clear();
                view.SortDescriptions.Clear();

                view.SortDescriptions.Add(new System.ComponentModel.SortDescription(
                    "sourceLabel", System.ComponentModel.ListSortDirection.Ascending));
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription(
                    "netLength", System.ComponentModel.ListSortDirection.Descending));

                view.GroupDescriptions.Add(new PropertyGroupDescription("sourceLabel"));
            }
        }

        private void btn_MineSettings_Click(object sender, RoutedEventArgs e)
        {
            //Carry what the main window owns into the settings object first, so the dialog
            //opens showing the tolerances currently on screen.
            storeSettings();

            CarboCircleSettings settingsWindow = new CarboCircleSettings(activeProject);
            settingsWindow.ShowDialog();

            if (settingsWindow.isAccepted)
            {
                //The dialog edited and saved its own snapshot; adopt it and redraw.
                activeProject.settings = settingsWindow.settings;
                LoadInterFaceFromSettings();
            }
        }

        /// <summary>
        /// Pushes the settings the main window edits directly into the settings object.
        /// Colours are written straight through by their own click handlers.
        /// </summary>
        private void storeSettings()
        {
            if (!string.IsNullOrWhiteSpace(txt_BeamStrengthTolerance.Text))
                activeProject.settings.strengthRange = Utils.ConvertMeToDouble(txt_BeamStrengthTolerance.Text);

            if (!string.IsNullOrWhiteSpace(txt_SteelBeamDepthTolerance.Text))
                activeProject.settings.depthRange = Utils.ConvertMeToDouble(txt_SteelBeamDepthTolerance.Text);

            //Blank is left alone rather than read as zero. An empty box means the user cleared
            //it on the way to typing something else, and taking that as "no allowance at all"
            //would quietly hand every mined member its full length back.
            if (!string.IsNullOrWhiteSpace(txt_SteelCutoffLength.Text))
                activeProject.settings.cutoffbeamLength = Utils.ConvertMeToDouble(txt_SteelCutoffLength.Text);

            if (!string.IsNullOrWhiteSpace(txt_TimberCutoffLength.Text))
                activeProject.settings.timberCutoffLength = Utils.ConvertMeToDouble(txt_TimberCutoffLength.Text);
        }

        private void txt_ParseTextSettings_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshCutoffWarnings();
        }

        /// <summary>
        /// Shows or hides the note under each cut-off box as the value is typed.
        ///
        /// Advisory only - nothing is clamped or rejected. A shorter allowance is a legitimate
        /// thing to model; it is precisely what makes the case for a specialist deconstruction
        /// method, and the note is there so that assumption is stated rather than buried in a
        /// number two tabs away from the results it changes.
        /// </summary>
        private void refreshCutoffWarnings()
        {
            setCutoffWarning(txt_SteelCutoffLength, wrn_SteelCutoff, txt_SteelCutoffWarning,
                carboCircleSettings.SteelCutoffAdvisoryMin);

            setCutoffWarning(txt_TimberCutoffLength, wrn_TimberCutoff, txt_TimberCutoffWarning,
                carboCircleSettings.TimberCutoffAdvisoryMin);
        }

        private static void setCutoffWarning(System.Windows.Controls.TextBox box,
            System.Windows.Controls.Border note, System.Windows.Controls.TextBlock text, double advisoryMin)
        {
            //Called from TextChanged, which can fire while the tree is still being built.
            if (box == null || note == null || text == null)
                return;

            double value;

            //Nothing to say about a box that is empty or half-typed. Only a value that parses
            //and comes out low is a decision worth flagging - "1e" on the way to "1500" is not.
            bool parsed = double.TryParse((box.Text ?? "").Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out value);

            if (parsed && value < advisoryMin)
            {
                text.Text = carboCircleSettings.CutoffAdvisoryMessage
                    + " (below " + advisoryMin.ToString("N0") + " mm).";
                note.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                note.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void btn_ExportMinedToCSV(object sender, RoutedEventArgs e)
        {
            List<carboCircleElement> dataCombined = new List<carboCircleElement>();

            string path = DataExportUtils.GetSaveAsLocation();


            List<carboCircleElement> dataToExport = activeProject.minedData;
            List<carboCircleElement> volumesToExport = activeProject.minedVolumes;

            foreach (carboCircleElement dat in dataToExport)
            {
                dataCombined.Add(dat.Copy());
            }

            foreach (carboCircleElement vol in volumesToExport)
            {
                dataCombined.Add(vol.Copy());
            }

            if (path != null)
            {
                carboCircleUtils.ExportDataToCSV(dataCombined, path);

            }


            if (File.Exists(path))
            {
                System.Windows.MessageBox.Show("CSV export successful. Click OK to open export directory.", "Success!", MessageBoxButton.OK);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }

        }

        private void btn_ExportProjectData_Click(object sender, RoutedEventArgs e)
        {
            List<carboCircleElement> dataCombined = new List<carboCircleElement>();

            string path = DataExportUtils.GetSaveAsLocation();


            List<carboCircleElement> dataToExport = activeProject.requiredData;
            List<carboCircleElement> volumesToExport = activeProject.requiredVolumes;

            foreach (carboCircleElement dat in dataToExport)
            {
                dataCombined.Add(dat.Copy());
            }

            foreach (carboCircleElement vol in volumesToExport)
            {
                dataCombined.Add(vol.Copy());
            }

            if (path != null)
            {
                carboCircleUtils.ExportDataToCSV(dataCombined, path);

            }


            if (File.Exists(path))
            {
                System.Windows.MessageBox.Show("CSV export successful. Click OK to open export directory.", "Success!", MessageBoxButton.OK);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
        }

        private void btn_ColourMinedReused_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourMinedReused.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_ReusedMinedData = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourMinedReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_ColourMinedNotReused_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourMinedNotReused.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_NotReused = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourMinedNotReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        private void btn_ColourRequiredReused_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourRequiredReused.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_FromReusedData = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourRequiredReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_ColourRequiredNotReused_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourRequiredNotReused.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_NotFromReused = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourRequiredNotReused.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_ColourMassReusable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_ColourMassReusable.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                activeProject.settings.colour_ReusedMinedVolumes = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_ColourMassReusable.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        private System.Drawing.Color GetColor(System.Windows.Media.Brush startColour)
        {
            //System.Windows.Media.Color color = ((SolidColorBrush)startColour).Color;
            //System.Drawing.Color oldC = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            try
            {
                System.Drawing.Color oldC = ConvertToColor(startColour);

                System.Windows.Forms.ColorDialog MyDialog = new System.Windows.Forms.ColorDialog();
                // Keeps the user from selecting a custom color.
                MyDialog.AllowFullOpen = true;
                MyDialog.FullOpen = true;
                // Allows the user to get help. (The default is false.)
                MyDialog.ShowHelp = true;
                // Sets the initial color select to the current text color.
                MyDialog.Color = oldC;

                // Update the text box color if the user clicks OK 
                if (MyDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    return MyDialog.Color;
                else
                    return oldC;
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
                return System.Drawing.Color.FromArgb(255, 0, 0, 0);
            }
        }

        private System.Drawing.Color ConvertToColor(System.Windows.Media.Brush brush)
        {
            try
            {
                System.Windows.Media.Color color = ((SolidColorBrush)brush).Color;
                System.Drawing.Color oldC = System.Drawing.Color.FromArgb(color.R, color.G, color.B);

                return oldC;
            }
            catch (Exception ex)
            {
                return System.Drawing.Color.FromArgb(255, 0, 0, 0);
            }
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            storeSettings();
            activeProject.settings.Save();

            //Hidden, not closed, so a mine and a project survive putting the window away and
            //bringing it back. isWindowOpen is deliberately NOT cleared here: it used to be,
            //and the application read that as "there is no window" and built a second one on
            //the next click - leaving the first alive, still subscribed to every import, and
            //still writing into the project the user was looking at.
            this.Hide();
        }

        private void btn_Report_Click(object sender, RoutedEventArgs e)
        {
            //
            bool createReport = true;
            //Create a File and save it as a HTML File
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Specify report directory";
            saveDialog.Filter = "HTML Files|*.html";
            saveDialog.FilterIndex = 2;
            saveDialog.RestoreDirectory = true;

            saveDialog.ShowDialog();

            string Path = saveDialog.FileName;

            if (File.Exists(Path))
            {
                MessageBoxResult msgResult = System.Windows.MessageBox.Show("This file already exists, do you want to overwrite this file ?", "", MessageBoxButton.YesNo);

                if (msgResult == MessageBoxResult.Yes)
                {
                    using (var fs = File.Open(Path, FileMode.Open))
                    {
                        var canRead = fs.CanRead;
                        var canWrite = fs.CanWrite;

                        if (canWrite == false)
                        {
                            System.Windows.MessageBox.Show("This file cannot be opened, please close the file and try again", "Warning", MessageBoxButton.OK);
                            createReport = false;
                        }
                    }
                    createReport = true;
                }
                else
                {
                    createReport = false;
                }
            }
            else if (Path == "")
            {
                //The dialog box was canceled;
                createReport = false;
            }


            if (createReport == true && Path != "")
            {
                if (m_ExEvent != null)
                {
                    m_Handler.SetSwitch(4);
                    m_Handler.SetSettings(activeProject);
                    //The path travels with the request. It used to be stored on this window
                    //and read back when the handler raised ImageReady - which every window
                    //ever opened answered, stale ones included, whose path was still empty.
                    m_Handler.SetReportPath(Path);

                    m_ExEvent.Raise();
                }
            }

        }

        private void btn_ImportProjectCSV_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Select a csv containing elements for import,", "Message for You!");
            string openPath = DataExportUtils.GetOpenCSVLocation();

            if (!string.IsNullOrWhiteSpace(openPath))
            {
                List<carboCircleElement> importedElements = carboCircleUtils.GetElementsFromCVSFile(openPath);
                if (importedElements != null && importedElements.Count > 0)
                {
                    activeProject.ParseRequiredData(importedElements);

                    liv_requiredMaterialList.ItemsSource = null;
                    liv_requiredMaterialList.ItemsSource = activeProject.requiredData;

                    liv_RequiredMassObjects.ItemsSource = null;
                    liv_RequiredMassObjects.ItemsSource = activeProject.requiredVolumes;

                    setRequiredOk();
                }
            }
        }

        private void btn_ImportmaterialsCSV_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Select a csv file containing elements that can be reused,", "Message for You!");
            string openPath = DataExportUtils.GetOpenCSVLocation();

            if (!string.IsNullOrWhiteSpace(openPath))
            {
                List<carboCircleElement> importedElements = carboCircleUtils.GetElementsFromCVSFile(openPath);
                if (importedElements != null && importedElements.Count > 0)
                {
                    activeProject.ParseMinedData(importedElements);

                    liv_MinedData.ItemsSource = null;
                    liv_MinedData.ItemsSource = activeProject.minedData;

                    liv_MinedMassObjects.ItemsSource = null;
                    liv_MinedMassObjects.ItemsSource = activeProject.minedVolumes;

                    setMineOk();
                }
            }
        }

        private void btn_GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                CarboProject myProject = carboCircleUtils.convertToCarboLifeProject(activeProject);


                if (myProject != null)
                {
                    try
                    {
                        CarboLifeUI.UI.CarboLifeMainWindow CarboApp = new CarboLifeMainWindow(myProject);
                        CarboApp.ShowDialog();
                    }
                    catch { }
                }
            }
            catch
            { }



        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            storeSettings();
            activeProject.settings.Save();

            //The title bar X is a real close, and is allowed to be one - the Closed handler set
            //up in the constructor unsubscribes from the handler and clears the open flag. The
            //Hide() that used to be here ran a moment before the window closed anyway and only
            //made the sequence harder to follow.
        }
    }
}