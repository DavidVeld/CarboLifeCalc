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
        private List<int> visibleElements;

        public HeatMapCreator(ExternalEvent exEvent, ColourViewerHandler handler, CarboProject project, List<int> _visibleElements)
        {
            carboSettings = new CarboSettings();
            carboSettings.Load();

            InitializeComponent();

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
                lbl_name.Content = carboProject.Name;
                lbl_total.Content = carboProject.getTotalEC().ToString("N") + " tCO2";
            }

            //load current Settings;
            carboSettings = new CarboSettings();
            carboSettings = carboSettings.Load();

            refreshColourtemplatesList();
            selectColour();

            cbb_outofBounds.Items.Add("Colour");
            cbb_outofBounds.Items.Add("No Override");
            cbb_outofBounds.SelectedIndex = 0;

            if (settings.ecRevitParameter != "")
                txt_Parameter.Text = settings.ecRevitParameter;
            else
                txt_Parameter.Text = "CLC_EmbodiedCarbon";
            
            if(settings.carboLegendName != "")
                txt_LegendName.Text = settings.carboLegendName;
            else
                txt_LegendName.Text = "CLC_ColourLegend";

            if (settings.carboDashboardName != "")
                txt_DashBoardName.Text = settings.carboDashboardName;
            else
                txt_DashBoardName.Text = "ResultsView";
            //cbb_Parameter.SelectedIndex = 0;

        }


        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Project File (*.clcx)|*.clcx|All files (*.*)|*.*";

                var path = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName))
                {
                    string projectPath = openFileDialog.FileName;

                    //Open the project
                    CarboProject projectToOpen = new CarboProject();

                    CarboProject projectToUpdate = new CarboProject();
                    CarboProject buffer = new CarboProject();
                    projectToUpdate = buffer.DeSerializeXML(projectPath);

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
                    lbl_name.Content = carboProject.Name;
                    lbl_total.Content = carboProject.getTotalEC().ToString("N") + " tCO2";

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
            lbl_name.Content = carboProject.Name;
            lbl_total.Content = carboProject.getTotalEC().ToString("N") + " tCO2";


            //Get all the visible elements (all project)

            UpdateDataSource();
        }

        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {
            RefreshGraph();
        }

        private void FilterPerList()
        {
            graphData.FilterNonVisible(visibleElements);
        }

        private void FilterPerMaxMin()
        {
            try
            {
                //Values we'd need for all options:
                double xMaxCutoff = Utils.ConvertMeToDouble(txt_CutoffMax.Text);
                double xMinCutoff = Utils.ConvertMeToDouble(txt_CutoffMin.Text);

                graphData.FilterMinMax(xMinCutoff, xMaxCutoff);
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// This method loads new data from the carboproject
        /// </summary>
        private void UpdateDataSource()
        {
            CarboGraphResult thisResult = new CarboGraphResult();
            //Define the type of graph to make:
            try
            {
                if (carboProject != null)
                {
                    if (rad_ByDensitykg.IsChecked == true)
                    {
                        //This will plot each element based on their material the X axis is the embodied carbon (kgCo2/kg) the Y axis it the weight or mass.
                        thisResult = CarboLifeAPI.HeatMapCollector.GetMaterialMassData(carboProject);
                    }
                    else if (rad_ByDensitym.IsChecked == true)
                    {
                        thisResult = CarboLifeAPI.HeatMapCollector.GetMaterialVolumeData(carboProject);
                    }
                    else if (rad_ByGroup.IsChecked == true)
                    {
                        thisResult = CarboLifeAPI.HeatMapCollector.GetPerGroupData(carboProject);
                    }
                    else if (rad_ByElement.IsChecked == true)
                    {
                        thisResult = CarboLifeAPI.HeatMapCollector.GetPerElementData(carboProject);
                    }
                    else if (rad_MaterialTotals.IsChecked == true)
                    {
                        thisResult = CarboLifeAPI.HeatMapCollector.GetMaterialTotalData(carboProject);
                    }
                    else
                    {

                    }
                }

                graphData = thisResult;

                //Filter the project per visible elements, this only happends in the update source part;
                FilterPerList();

                //if data was collected make it the source and update the graph
                //clear if no data
                if (thisResult.entireProjectData.Count > 0)
                {
                    double maxValue = graphData.getMaxValue() + 1;
                    double minValue = graphData.getMinValue() - 1;

                    //Some Data checks:
                    if (minValue > 0)
                        minValue = 0;

                    maxValue = Convert.ToInt32(maxValue);

                    txt_CutoffMax.Text = maxValue.ToString();
                    txt_CutoffMin.Text = minValue.ToString();

                    sld_Max.Minimum = minValue;
                    sld_Max.Maximum = maxValue;
                    sld_Max.Value = maxValue;

                    sld_Min.Minimum = minValue;
                    sld_Min.Maximum = maxValue;
                    sld_Min.Value = minValue;

                    UpdateGraphData();
                    RefreshGraph();
                }
                else
                    cnv_Graph.Children.Clear();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }
        private void UpdateGraphData()
        {
            try
            {
                if (cnv_Graph != null)
                {
                    cnv_Graph.Visibility = Visibility.Hidden;
                    cnv_Graph.Children.Clear();
                    cnv_Graph.Visibility = Visibility.Visible;

                    if (carboProject != null && graphData != null)
                    {
                        FilterPerMaxMin();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        //this method updates the graph based on current settings.
        private void RefreshGraph()
        {
            try
            {
                if (cnv_Graph != null && currentColourSettings != null && cnv_Graph.ActualWidth != 0 && cnv_Graph.ActualHeight != 0)
                {
                    cnv_Graph.Visibility = Visibility.Hidden;
                    cnv_Graph.Children.Clear();
                    cnv_Graph.Visibility = Visibility.Visible;
                    if (carboProject != null && graphData != null)
                    {
                        if (graphData.entireProjectData.Count > 0)
                        {
                            //to be set in a ui
                            var result = CarboLifeAPI.HeatMapBarBuilder.GetBarGraph(graphData, cnv_Graph.ActualWidth, cnv_Graph.ActualHeight, currentColourSettings);

                            graphData = result.Item1 as CarboGraphResult;
                            List<UIElement> graph = result.Item2 as List<UIElement>;

                            cnv_Graph.Children.Clear();
                            if (graph != null && graph.Count > 0)
                            {
                                foreach (UIElement uielement in graph)
                                {
                                    cnv_Graph.Children.Add(uielement);
                                }
                            }
                        }
                    }
                }
                if (!Utils.IsEmpty(visibleElements))
                    lbl_debug.Content = string.Format("Elements in projects {0}, selected: {1} " + Environment.NewLine + ", valid/filtered: {2} elements in selection/view: {3}",
                        graphData.entireProjectData.Count,
                        graphData.selectedData.Count,
                        graphData.validData.Count,
                        visibleElements.Count);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshGraph();
        }

        private void Btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            carboSettings.carboLegendName = txt_LegendName.Text;
            carboSettings.carboDashboardName = txt_DashBoardName.Text;
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
            try
            {
                txt_CutoffMax.Text = Math.Round(sld_Max.Value, 3).ToString();
                UpdateGraphData();
                RefreshGraph();
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void sld_Min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                txt_CutoffMin.Text = Math.Round(sld_Min.Value, 3).ToString();
                UpdateGraphData();
                RefreshGraph();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void btn_TestFunction(object sender, RoutedEventArgs e)
        {
            List<int> listOfIds = new List<int>();
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
            UpdateGraphData();
            RefreshGraph();
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
                        CarboColourPreset cps = carboSettings.colourPresets[i];

                        if (cps.name == cbb_colours.Text)
                        {
                            cps = currentColourSettings;
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

                    foreach (CarboColourPreset ccp in carboSettings.colourPresets)
                    {
                        cbb_colours.Items.Add(ccp.name);
                    }
                    cbb_colours.SelectedIndex = 0;
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
                UpdateGraphData();
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

                UpdateGraphData();
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

                UpdateGraphData();
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

                UpdateGraphData();
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

                UpdateGraphData();
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
            if (viewName != "")
            {
                m_Handler.drawResultView(carboProject, viewName);
                m_ExEvent.Raise();
            }
        }

        private void txt_DashBoardName_TextChanged(object sender, TextChangedEventArgs e)
        {
            carboSettings.carboDashboardName = txt_DashBoardName.Text;
        }

        private void txt_Parameter_TextChanged(object sender, TextChangedEventArgs e)
        {
            carboSettings.ecRevitParameter = txt_Parameter.Text;

        }
    }
}
