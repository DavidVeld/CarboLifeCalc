using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
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

using Microsoft.Win32;
using System.IO;
using CarboLifeAPI;

using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.Drawing;
using CarboLifeRevit.Modeless;
using CarboLifeUI.UI;

namespace CarboLifeRevit
{

    /// <summary>
    /// Interaction logic for HeatMapCreator.xaml
    /// </summary>
    public partial class HeatMapCreator : Window
    {
        //Used for colour 
        private CarboProject carboProject;
        private CarboGraphResult graphData;
        private CarboColourPreset currentColourSettings;
        private CarboSettings carboSettings;

        //Used for Revit handlers
        private ColourViewerHandler m_Handler;
        private ExternalEvent m_ExEvent;
        private List<Int64> visibleElements;

        //Used for the graph
        private readonly System.Windows.Threading.DispatcherTimer graphRefreshTimer = new System.Windows.Threading.DispatcherTimer();
        private readonly HeatMapGraphOptions graphOptions = new HeatMapGraphOptions();

        /// <summary>The rounded bounds the cutoff sliders run between.</summary>
        private HeatMapNiceRange cutoffRange;
        private double cutoffMin;
        private double cutoffMax;

        /// <summary>
        /// Set while the controls are being populated, so their handlers do not filter against a half
        /// updated state. Starts true: setting Slider.Value from XAML raises ValueChanged while the rest
        /// of the window is still being built, and InitialiseGraph clears it once everything exists.
        /// </summary>
        private bool suspendGraphUpdates = true;

        public HeatMapCreator(ExternalEvent exEvent, ColourViewerHandler handler, CarboProject project, List<Int64> _visibleElements)
        {
            carboSettings = new CarboSettings();
            carboSettings.Load();

            InitializeComponent();
            InitialiseGraph();

            this.m_ExEvent = exEvent;
            this.m_Handler = handler;

 

            //set the list of elements active in the view when form was launched.
            if (_visibleElements != null && _visibleElements.Count > 0)
            {
                visibleElements = _visibleElements;
            }
            //Load the project and refresh screen
            if (project != null)
            {
                carboProject = project;

                //The heat map reads the per element results - EC_Cumulative, Volume_Cumulative -
                //which only exist after a calculation. Opening a project or a new import does
                //calculate, but this window is handed whatever the caller has, so calculate here
                //rather than rely on that: a project fresh from a file carries whatever totals
                //were last saved in it.
                carboProject.CalculateProject();

                UpdateDataSource();
            }
            else
                carboProject = new CarboProject();


        }

        //for Non-Modeless Usage;
        public HeatMapCreator(CarboProject project)
        {
            if (project != null)
                carboProject = project;
            else
                carboProject = new CarboProject();

            carboSettings = new CarboSettings();
            carboSettings.Load();

            InitializeComponent();
            InitialiseGraph();
        }

        //************************************************************************************************
        // This is the part that interacts with Revit Do not copy over
        //************************************************************************************************

        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            m_Handler.ColourTheModel(graphData, false, true,chk_Legend.IsChecked.Value);
            m_ExEvent.Raise();
        }
        private void btn_Show_Click(object sender, RoutedEventArgs e)
        {
            bool colourOutOfBounds = false;
            if(cbb_outofBounds.Text == "Colour")
            {
                colourOutOfBounds = true;
            }

            graphData.ColourLegendName = txt_LegendName.Text;

            m_Handler.ColourTheModel(graphData, true, colourOutOfBounds, chk_Legend.IsChecked.Value);
            m_ExEvent.Raise();
        }
        private void btn_Importvalues_Click(object sender, RoutedEventArgs e)
        {
            string selectedParam = txt_Parameter.Text;
            if (txt_Parameter.Text != "")
            {
                m_Handler.Importvalues(carboProject, selectedParam, false);
                m_ExEvent.Raise();
            }

        }

        private void btn_ClearValues_Click(object sender, RoutedEventArgs e)
        {
            string selectedParam = txt_Parameter.Text;

            if (selectedParam != "")
            {
                var mresult = System.Windows.MessageBox.Show("This will clear ALL values of parameter: " + selectedParam + " Do you wish to continue? ", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (mresult == MessageBoxResult.Yes)
                {
                    m_Handler.Importvalues(carboProject, selectedParam, true);
                    m_ExEvent.Raise();
                }
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //Before the form is closed, everything must be disposed properly
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


        //************************************************************************************************
        //ANYTHING BELOW THIS LINE SHOULD BE IDENTICAL TO THE NON-MODELESS FORM
        //FOR NOW BOTH CAN EXIST
        //************************************************************************************************


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CarboSettings settings = new CarboSettings();
            settings = settings.Load();
            
            //this is just to confirm the window loaded
            if (carboProject != null)
            {
                lbl_name.Text = carboProject.Name;
                lbl_total.Text = carboProject.getTotalEC().ToString("N") + " tCO2";
            }

            //load current Settings;
            carboSettings = new CarboSettings();
            carboSettings = carboSettings.Load();

            refreshColourtemplatesList();
            selectColour();

            cbb_outofBounds.Items.Add("Colour");
            cbb_outofBounds.Items.Add("No Override");
            cbb_outofBounds.SelectedIndex = 0;

            if (settings.carboLegendName != "")
                txt_LegendName.Text = settings.carboLegendName;
            else
                txt_LegendName.Text = "CLC_ColourLegend";

            if (settings.ecRevitParameter != "")
                txt_Parameter.Text = settings.ecRevitParameter;
            else
                txt_Parameter.Text = "CLC_EmbodiedCarbon";

            if (settings.carboResultLegendName != "")
                txt_DashBoardName.Text = settings.carboResultLegendName;
            else
                txt_DashBoardName.Text = "CLC_ResultsView";
            //cbb_Parameter.SelectedIndex = 0;

            //The colour preset only exists from here on, so this is the first point the graph can be drawn.
            QueueGraphRefresh();
        }


        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string projectPathToOpen = Utils.OpenCarboProject();

                if (projectPathToOpen != "")
                {
                    string projectPath = projectPathToOpen;

                    //Open the project
                    CarboProject projectToOpen = new CarboProject();

                    CarboProject projectToUpdate = new CarboProject();
                    CarboProject buffer = new CarboProject();
                    projectToUpdate = buffer.DeSerializeXML(projectPath);

                    //A damaged file comes back as null, having already reported the problem.
                    if (projectToUpdate == null)
                        return;

                    projectToUpdate.Audit();
                    projectToUpdate.CalculateProject();

                    carboProject = projectToUpdate;

                    //When Opened the entire dataset is considered;
                    if (Utils.IsEmpty(visibleElements))
                    {
                        visibleElements = carboProject.GetElementIdList();
                    }
                    else
                    {
                        //The list is not empty thus use the selected data to progress;
                    }

                    //Show the data
                    lbl_name.Text = carboProject.Name;
                    lbl_total.Text = carboProject.getTotalEC().ToString("N") + " tCO2";

                }
                //Get all the visible elements (all project)

                UpdateDataSource();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (carboProject != null)
            {
                try
                {
                    CarboLifeUI.UI.CarboLifeMainWindow CarboApp = new CarboLifeMainWindow(carboProject);
                    CarboApp.ShowDialog();

                    carboProject = CarboApp.carboLifeProject;

                    carboProject.Audit();
                    carboProject.CalculateProject();
                    this.Visibility = Visibility.Visible;
                }
                catch { }
            }

            //When Opened the entire dataset is considered;
            if (Utils.IsEmpty(visibleElements))
            {
                visibleElements = carboProject.GetElementIdList();
            }
            else
            {
                //The list is not empty thus use the selected data to progress;
            }

            //Show the data
            lbl_name.Text = carboProject.Name;
            lbl_total.Text = carboProject.getTotalEC().ToString("N") + " tCO2";


            //Get all the visible elements (all project)

            UpdateDataSource();
        }

        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {
            RefreshGraph();
        }

        //************************************************************************************************
        //GRAPH PLUMBING
        //************************************************************************************************

        /// <summary>
        /// Wires up the debounce timer. Rebuilding the graph is not free, so a slider drag or a window
        /// resize schedules one redraw instead of firing one per pixel of travel.
        /// Must be called straight after InitializeComponent, from every constructor.
        /// </summary>
        private void InitialiseGraph()
        {
            //Short enough that a drag still looks live, long enough to swallow the burst of events WPF
            //raises within a single frame.
            graphRefreshTimer.Interval = TimeSpan.FromMilliseconds(50);
            graphRefreshTimer.Tick += GraphRefreshTimer_Tick;

            //Every control exists from here on, so the handlers can safely do their work.
            suspendGraphUpdates = false;
        }

        private void GraphRefreshTimer_Tick(object sender, EventArgs e)
        {
            graphRefreshTimer.Stop();
            ApplyCutoffs();
            RefreshGraph();
        }

        /// <summary>
        /// Asks for a redraw shortly from now. Repeated calls collapse into a single rebuild.
        /// </summary>
        private void QueueGraphRefresh()
        {
            if (suspendGraphUpdates)
                return;

            graphRefreshTimer.Stop();
            graphRefreshTimer.Start();
        }

        private void FilterPerList()
        {
            if (graphData != null)
                graphData.FilterNonVisible(visibleElements);
        }

        /// <summary>
        /// Applies the current cutoffs to the dataset. This reads the slider values directly: sending them
        /// through the text boxes lost precision to rounding and failed outright on a comma decimal locale,
        /// where a formatted thousands separator parsed back as zero.
        /// </summary>
        private void ApplyCutoffs()
        {
            if (graphData == null)
                return;

            double low = Math.Min(cutoffMin, cutoffMax);
            double high = Math.Max(cutoffMin, cutoffMax);

            graphData.FilterMinMax(low, high);
        }

        /// <summary>
        /// Collects a fresh dataset from the project for the selected heatmap type.
        /// </summary>
        private CarboGraphResult CollectGraphData()
        {
            if (carboProject == null)
                return new CarboGraphResult();

            if (rad_ByDensitykg.IsChecked == true)
            {
                //This will plot each element based on their material, the value is the embodied carbon (kgCo2/kg).
                return CarboLifeAPI.HeatMapCollector.GetMaterialMassData(carboProject);
            }
            if (rad_ByDensitym.IsChecked == true)
                return CarboLifeAPI.HeatMapCollector.GetMaterialVolumeData(carboProject);
            if (rad_ByGroup.IsChecked == true)
                return CarboLifeAPI.HeatMapCollector.GetPerGroupData(carboProject);
            if (rad_ByElement.IsChecked == true)
                return CarboLifeAPI.HeatMapCollector.GetPerElementData(carboProject);
            if (rad_MaterialTotals.IsChecked == true)
                return CarboLifeAPI.HeatMapCollector.GetMaterialTotalData(carboProject);

            return new CarboGraphResult();
        }

        /// <summary>
        /// This method loads new data from the carboproject and resets the cutoff sliders to it.
        /// </summary>
        private void UpdateDataSource()
        {
            try
            {
                graphData = CollectGraphData();

                //Filter the project per visible elements, this only happends in the update source part;
                FilterPerList();

                if (graphData.entireProjectData.Count == 0)
                {
                    if (cnv_Graph != null)
                        cnv_Graph.Children.Clear();
                    UpdateStatusLine();
                    return;
                }

                //Round the raw data range out to readable bounds. The old "+1 then cast to int" threw away
                //every bit of resolution on the kgCO2/kg modes (a dataset topping out at 1.4 got a 0 to 2
                //slider) and meant nothing at all on the kgCO2/m3 ones.
                cutoffRange = HeatMapBarBuilder.GetNiceRange(graphData.getMinValue(), graphData.getMaxValue(), 20, false);

                //Setting the bounds makes WPF coerce the values, which used to re-enter the handlers and
                //filter against a half updated state.
                suspendGraphUpdates = true;
                try
                {
                    sld_Min.Minimum = cutoffRange.Min;
                    sld_Min.Maximum = cutoffRange.Max;
                    sld_Max.Minimum = cutoffRange.Min;
                    sld_Max.Maximum = cutoffRange.Max;

                    double step = cutoffRange.Step > 0 ? cutoffRange.Step : cutoffRange.Span / 20;
                    sld_Min.SmallChange = step / 10;
                    sld_Min.LargeChange = step;
                    sld_Max.SmallChange = step / 10;
                    sld_Max.LargeChange = step;

                    cutoffMin = cutoffRange.Min;
                    cutoffMax = cutoffRange.Max;
                    sld_Min.Value = cutoffMin;
                    sld_Max.Value = cutoffMax;
                }
                finally
                {
                    suspendGraphUpdates = false;
                }

                UpdateCutoffText();
                ApplyCutoffs();
                RefreshGraph();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Redraws the graph from the current data, cutoffs and colours.
        /// </summary>
        private void RefreshGraph()
        {
            try
            {
                if (cnv_Graph == null)
                    return;

                cnv_Graph.Children.Clear();

                if (graphData == null || currentColourSettings == null)
                    return;

                if (cnv_Graph.ActualWidth <= 0 || cnv_Graph.ActualHeight <= 0)
                    return;

                graphOptions.UseLogScale = chk_LogScale.IsChecked == true;

                var result = CarboLifeAPI.HeatMapBarBuilder.GetBarGraph(graphData, cnv_Graph.ActualWidth, cnv_Graph.ActualHeight, currentColourSettings, graphOptions);

                //Only ever replace the live dataset with something real.
                if (result.Item1 != null)
                    graphData = result.Item1;

                if (result.Item2 != null)
                {
                    foreach (UIElement uielement in result.Item2)
                        cnv_Graph.Children.Add(uielement);
                }

                UpdateStatusLine();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Shows the cutoffs at the precision of the current range. Display only.
        /// </summary>
        private void UpdateCutoffText()
        {
            if (cutoffRange == null)
                return;

            txt_CutoffMin.Text = cutoffRange.Format(cutoffMin);
            txt_CutoffMax.Text = cutoffRange.Format(cutoffMax);
        }

        private void UpdateStatusLine()
        {
            if (graphData == null)
            {
                lbl_debug.Text = "Status: no data";
                return;
            }

            string status = string.Format("{0:N0} in project, {1:N0} in view, {2:N0} in range, {3:N0} out of range",
                graphData.entireProjectData.Count,
                graphData.selectedData.Count,
                graphData.validData.Count,
                graphData.outOfBoundsMinData.Count + graphData.outOfBoundsMaxData.Count);

            if (graphOptions.LogScaleUnavailable)
                status += "  |  log scale needs values above zero";

            lbl_debug.Text = status;
        }

        /// <summary>
        /// Commits a typed cutoff when the user presses Enter. See the same pair in
        /// CarboLifeUI's HeatMapCreator: these boxes were disabled and display only, so an exact
        /// cutoff could not be entered at all, and the value could not even be selected to copy.
        /// Committing on Enter and on focus loss keeps a half typed "1." out of the graph.
        /// </summary>
        private void txt_Cutoff_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;

            e.Handled = true;
            CommitTypedCutoffs();
        }

        private void txt_Cutoff_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitTypedCutoffs();
        }

        /// <summary>
        /// Reads both boxes and applies them. Anything unreadable puts the boxes back to the
        /// cutoffs actually in force.
        /// </summary>
        private void CommitTypedCutoffs()
        {
            if (cutoffRange == null)
                return;

            double low, high;

            if (TryReadCutoff(txt_CutoffMin, out low) == false || TryReadCutoff(txt_CutoffMax, out high) == false)
            {
                UpdateCutoffText();
                return;
            }

            if (Math.Abs(low - cutoffMin) < double.Epsilon && Math.Abs(high - cutoffMax) < double.Epsilon)
                return;

            SetCutoffs(low, high);
        }

        private static bool TryReadCutoff(System.Windows.Controls.TextBox box, out double value)
        {
            value = 0;

            if (box == null)
                return false;

            string text = box.Text == null ? "" : box.Text.Trim();

            return double.TryParse(text, System.Globalization.NumberStyles.Any,
                                   System.Globalization.CultureInfo.CurrentCulture, out value)
                || double.TryParse(text, System.Globalization.NumberStyles.Any,
                                   System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Moves both cutoffs at once and redraws.
        /// </summary>
        private void SetCutoffs(double low, double high)
        {
            if (cutoffRange == null)
                return;

            low = Math.Max(cutoffRange.Min, Math.Min(low, cutoffRange.Max));
            high = Math.Max(cutoffRange.Min, Math.Min(high, cutoffRange.Max));

            if (high < low)
            {
                double swap = low;
                low = high;
                high = swap;
            }

            suspendGraphUpdates = true;
            try
            {
                sld_Min.Value = low;
                sld_Max.Value = high;
                cutoffMin = sld_Min.Value;
                cutoffMax = sld_Max.Value;
            }
            finally
            {
                suspendGraphUpdates = false;
            }

            UpdateCutoffText();
            ApplyCutoffs();
            RefreshGraph();
        }

        /// <summary>
        /// The canvas reports its new size once it has been laid out. The window's own SizeChanged fired
        /// while the canvas still held its previous size, which left the graph a resize behind.
        /// </summary>
        private void cnv_Graph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            QueueGraphRefresh();
        }

        private void Btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            carboSettings.carboLegendName = txt_LegendName.Text;
            carboSettings.carboResultLegendName = txt_DashBoardName.Text;
            carboSettings.ecRevitParameter = txt_Parameter.Text;

            carboSettings.Save();

            this.Close();
        }

        //Radiocontrollbuttons
        private void rad_Control_Click(object sender, RoutedEventArgs e)
        {
            UpdateDataSource();
        }

        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            //graphData = new CarboGraphResult();
            cnv_Graph.Children.Clear();
        }

        private void sld_Max_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //The XAML sets Value on this slider, which raises ValueChanged while the window is still being
            //built and the partner slider does not exist yet.
            if (suspendGraphUpdates || sld_Max == null || sld_Min == null)
                return;

            //The two cutoffs were free to cross each other, which put every element out of range and left
            //an empty canvas with nothing to explain it.
            if (sld_Max.Value < sld_Min.Value)
            {
                suspendGraphUpdates = true;
                sld_Max.Value = sld_Min.Value;
                suspendGraphUpdates = false;
            }

            cutoffMax = sld_Max.Value;
            UpdateCutoffText();
            QueueGraphRefresh();
        }

        private void sld_Min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (suspendGraphUpdates || sld_Min == null || sld_Max == null)
                return;

            if (sld_Min.Value > sld_Max.Value)
            {
                suspendGraphUpdates = true;
                sld_Min.Value = sld_Max.Value;
                suspendGraphUpdates = false;
            }

            cutoffMin = sld_Min.Value;
            UpdateCutoffText();
            QueueGraphRefresh();
        }

        private void chk_LogScale_Click(object sender, RoutedEventArgs e)
        {
            RefreshGraph();
        }

        /// <summary>
        /// Pulls the cutoffs in to the 2nd and 98th percentile. An embodied carbon distribution nearly always
        /// has a long tail, and a single outlier is enough to flatten every other bar into the baseline.
        /// </summary>
        private void btn_FitRange_Click(object sender, RoutedEventArgs e)
        {
            if (graphData == null || cutoffRange == null)
                return;

            double low = graphData.GetPercentile(2);
            double high = graphData.GetPercentile(98);

            if (high <= low)
            {
                low = cutoffRange.Min;
                high = cutoffRange.Max;
            }

            SetCutoffs(low, high);
        }

        private void btn_FullRange_Click(object sender, RoutedEventArgs e)
        {
            if (cutoffRange == null)
                return;

            SetCutoffs(cutoffRange.Min, cutoffRange.Max);
        }

        private void btn_TestFunction(object sender, RoutedEventArgs e)
        {
            List<Int64> listOfIds = new List<Int64>();
            listOfIds = carboProject.GetElementIdList();

            Random random = new Random();

            for (int i = listOfIds.Count - 1; i >= 0; i--)
            {
                int val = random.Next(1, 6);
                if (!(val == 3))
                    listOfIds.RemoveAt(i);
            }
            graphData.FilterNonVisible(listOfIds);
            visibleElements = listOfIds;

            UpdateDataSource();
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

        private System.Windows.Media.Color GetColor(System.Drawing.Color drawingColour)
        {
            try
            {
                System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(drawingColour.A, drawingColour.R, drawingColour.G, drawingColour.B);
                return color;
            }
            catch (Exception ex)
            {
                return System.Windows.Media.Color.FromArgb(255, 0, 0, 0);
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

        private void btn_SaveColours_Click(object sender, RoutedEventArgs e)
        {
            bool found = false;
            bool refreshLookup = false;
            try
            {
                //Saves the preset
                //set the preset in the settings, then save the settings;
                if (carboSettings.colourPresets != null && carboSettings.colourPresets.Count > 0)
                {
                    for (int i = 0; i < carboSettings.colourPresets.Count; i++)
                    {
                        if (carboSettings.colourPresets[i].name == cbb_colours.Text)
                        {
                            //Write back into the list. Assigning the loop variable only rebound the local
                            //copy, so saving over an existing preset silently did nothing.
                            currentColourSettings.name = cbb_colours.Text;
                            carboSettings.colourPresets[i] = currentColourSettings;
                            found = true;
                        }
                    }
                }

                if (found == false)
                {
                    //the name of the template could not be found, save as a new value.
                    carboSettings.colourPresets.Add(currentColourSettings);
                    refreshLookup = true;
                }

                carboSettings.Save();

                if (refreshLookup == true)
                    refreshColourtemplatesList();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void refreshColourtemplatesList()
        {
            try
            {
                if (carboSettings != null)
                {
                    if (carboSettings.colourPresets.Count == 0)
                    {
                        carboSettings.colourPresets.Add(new CarboColourPreset());
                    }

                    //Clear first: the list used to be appended to, so every call duplicated every preset.
                    string previous = cbb_colours.Text;
                    cbb_colours.Items.Clear();

                    foreach (CarboColourPreset ccp in carboSettings.colourPresets)
                    {
                        cbb_colours.Items.Add(ccp.name);
                    }

                    int index = cbb_colours.Items.IndexOf(previous);
                    cbb_colours.SelectedIndex = index >= 0 ? index : 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void selectColour()
        {
            try
            {
                string selectedColourname = cbb_colours.Text;

                foreach (CarboColourPreset ccp in carboSettings.colourPresets)
                {
                    if (selectedColourname == ccp.name)
                    {
                        currentColourSettings = ccp;

                        btn_Low.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(currentColourSettings.min.a, currentColourSettings.min.r, currentColourSettings.min.g, currentColourSettings.min.b));
                        btn_Mid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(currentColourSettings.mid.a, currentColourSettings.mid.r, currentColourSettings.mid.g, currentColourSettings.mid.b));
                        btn_High.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(currentColourSettings.max.a, currentColourSettings.max.r, currentColourSettings.max.g, currentColourSettings.max.b));

                        btn_MaxOut.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(currentColourSettings.outmax.a, currentColourSettings.outmax.r, currentColourSettings.outmax.g, currentColourSettings.outmax.b));
                        btn_MinOut.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(currentColourSettings.outmin.a, currentColourSettings.outmin.r, currentColourSettings.outmin.g, currentColourSettings.outmin.b));


                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_Low_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_Low.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                currentColourSettings.min = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_Low.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));
                RefreshGraph();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_Mid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_Mid.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                currentColourSettings.mid = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_Mid.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

                RefreshGraph();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_High_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_High.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                currentColourSettings.max = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_High.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

                RefreshGraph();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_MinOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_MinOut.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                currentColourSettings.outmin = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_MinOut.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

                RefreshGraph();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_MaxOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get a new colour
                System.Windows.Media.Brush startColour = btn_MaxOut.Background;
                System.Drawing.Color pickedColour = GetColor(startColour);

                //apply in the colour settings
                currentColourSettings.outmax = new CarboColour(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B);

                //Refresh the graph
                btn_MaxOut.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(pickedColour.A, pickedColour.R, pickedColour.G, pickedColour.B));

                RefreshGraph();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        private void cbb_colours_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (carboSettings.colourPresets != null && carboSettings.colourPresets.Count > 0)
                {
                    for (int i = 0; i < carboSettings.colourPresets.Count; i++)
                    {
                        CarboColourPreset cps = carboSettings.colourPresets[i];

                        if (cps.name == cbb_colours.Text)
                        {
                            currentColourSettings = cps;

                        }
                    }
                }
                selectColour();
                RefreshGraph();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void chk_Legend_Click(object sender, RoutedEventArgs e)
        {
        }

        private void txt_LegendName_TextChanged(object sender, TextChangedEventArgs e)
        {
            carboSettings.carboLegendName = txt_LegendName.Text;
        }

        private void btn_createDashBoard_Click(object sender, RoutedEventArgs e)
        {
            string viewName = txt_DashBoardName.Text;
            bool advancedTable = chk_Detailed.IsChecked.Value;
            if (viewName != "")
            {
                m_Handler.drawResultView(carboProject, viewName, advancedTable);
                m_ExEvent.Raise();
            }
        }

        private void txt_DashBoardName_TextChanged(object sender, TextChangedEventArgs e)
        {
            carboSettings.carboResultLegendName = txt_DashBoardName.Text;
        }

        private void txt_Parameter_TextChanged(object sender, TextChangedEventArgs e)
        {
            carboSettings.ecRevitParameter = txt_Parameter.Text;

        }
    }
}
