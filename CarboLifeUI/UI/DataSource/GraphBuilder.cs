using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
//using ScottPlot;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Color = System.Windows.Media.Color;

namespace CarboLifeUI.UI
{
    public static class GraphBuilder
    {

        public static double min;
        public static double max;

        /// <summary>
        /// Builds an overview A1-Mix graph for the user to add to a graph
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static List<ISeries> BuildBarGraph(CarboProject project)
        {
            List<CarboDataPoint> pointCollection = new List<CarboDataPoint>();
            // get the current Project
            pointCollection = project.getPhaseTotals();

            List<ISeries> result = new List<ISeries>();

            //Build series

            foreach (CarboDataPoint pont in pointCollection)
            {
                /*
                StackedColumnSeries series = new StackedColumnSeries
                {
                    Values = new ChartValues<double>
                    {
                        Math.Round((pont.Value / 1000), 2),
                    },
                    //StackMode = StackMode.Values,
                    DataLabels = true,
                    Title = pont.Name
                };

                result.Add(series);
                */  

            }

            return result;

        }
        /// <summary>
        /// Returns a comaring bar chart showing the total values of the project
        /// </summary>
        /// <param name="carboLifeProject"></param>
        /// <param name="projectListToCompareTo"></param>
        /// <returns></returns>
        internal static List<ISeries> BuildComparingTotalsBarGraph(CarboProject project, List<CarboProject> projectListToCompareTo)
        {
            List<ISeries> result = new List<ISeries>();

            //Full list to compare to:
            List<CarboProject> fullProjectListToCompareTo = new List<CarboProject>();
            //bool hasCurrent = false;

            if (project != null)
            {
                fullProjectListToCompareTo.Add(project);
                //hasCurrent = true;
            }
            foreach (CarboProject projectToCompare in projectListToCompareTo)
            {
                if (projectToCompare != null)
                {
                    fullProjectListToCompareTo.Add(projectToCompare);
                }
            }
            //The list has been created
            List<List<CarboDataPoint>> pointList = new List<List<CarboDataPoint>>();

            foreach (CarboProject dp in fullProjectListToCompareTo)
            {
                List<CarboDataPoint> listofPoints = dp.getPhaseTotals();
                //to tonnes
                foreach (CarboDataPoint cdp in listofPoints)
                    cdp.Value = cdp.Value / 1000;


                pointList.Add(listofPoints);
            }

            try
            {

                /*
                //loop though
                // i is nr type of information extracted
                for (int i = 0; i < pointList[0].Count; i++)
                {
                    /*
                    StackedColumnSeries newSeries = new StackedColumnSeries();

                    ChartValues<double> Values = new ChartValues<double>();

                    //get all Values from all loaded projects
                    foreach (List<CarboDataPoint> dataPointList in pointList)
                    {
                        Values.Add(Math.Round((dataPointList[i].Value / 1000),1));
                    }

                    newSeries.Title = pointList[0][i].Name;
                    newSeries.Values = Values;
                    newSeries.StackMode = StackMode.Values;
                    newSeries.DataLabels = true;
                    newSeries.Foreground = Brushes.Black;
                    //newSeries.Width = 100;
                    newSeries.MaxColumnWidth = 50;
                    newSeries.LabelsPosition = BarLabelPosition.Perpendicular;
                    result.Add(newSeries);
                    
                }
            */
                var allPhases = pointList
    .SelectMany(project => project.Select(p => p.Name))
    .Distinct()
    .ToList(); // All unique phase names

            var series = new List<ISeries>();
                int j = 0;
                foreach (var phase in allPhases)
                {
                    var phaseValues = pointList.Select(project =>
                        project.FirstOrDefault(p => p.Name == phase)?.Value ?? 0).ToList();
                    if (phaseValues[0] > 0)
                    {
                        series.Add(new StackedColumnSeries<double>
                        {
                            Name = phase,
                            Values = phaseValues,
                            Stroke = null, // optional: cleaner look
                            Fill = new SolidColorPaint(getSKColour(j)),
                            YToolTipLabelFormatter = point => $"{point.Model:0.00} tCO₂e",
                            DataLabelsSize = 12,
                            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                            DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                            //DataLabelsFormatter = (point) => $"{point.Model:0} tCO₂e",
                            DataLabelsFormatter = (point) => $"{point.Model:0.0}",

                        });
                    }
                    else
                    {
                        series.Add(new StackedColumnSeries<double>
                        {
                            Name = phase,
                            Values = phaseValues,
                            Stroke = null, // optional: cleaner look
                            Fill = new SolidColorPaint(getSKColour(j)),
                            YToolTipLabelFormatter = point => $"{point.Model:0.00} tCO₂e",
                            DataLabelsSize = 12,
                            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                            //DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                            //DataLabelsFormatter = (point) => $"{point.Model:0} tCO₂e",
                            //DataLabelsFormatter = (point) => $"{point.Model:0}",

                        });
                    }
                        j++;
                }

                result = series;


            }
            catch
            {
                return null;
            }
            return result;

        }

        //
        internal static List<ISeries> BuildLifeLine(CarboProject project, List<CarboProject> projectListToCompareTo, bool sequestration, bool energy, bool demolition)
        {
            min = double.PositiveInfinity;
            max = double.NegativeInfinity;

            List<CarboProject> projectList = new List<CarboProject>();

            if(project != null)
                projectList.Add(project);

            if (projectListToCompareTo != null && projectListToCompareTo.Count > 0)
            {
                projectList.AddRange(projectListToCompareTo);
            }

            List<ISeries> result = new List<ISeries>();

            if (projectList.Count > 0)
            {

                //List<ISeries> SerieList = new List<ISeries>();
                //List<double> values = new List<double>();
                List<string> labels = new List<string>();
                int j = 0;
                foreach (CarboProject prct in projectList)
                {
                    IList<CarboDataPoint> data = CarboTimeLine.GetTimeLineDataPoints(prct, sequestration, energy, demolition);


                    // Create a new list of values and labels for this series
                    List<double> values = new List<double>();
                    foreach (CarboDataPoint dataPoint in data)
                    {
                        values.Add(dataPoint.Value);
                    }

                    var lc = getSKColour(j);
                    var color = new SKColor(lc.Red, lc.Green, lc.Blue);

                    var paint = new SolidColorPaint
                    {
                        Color = new SKColor(lc.Red, lc.Green, lc.Blue),
                        StrokeThickness = 2,
                        IsAntialias = true
                    };

                    // Add a LineSeries for this project
                    result.Add(new LineSeries<double>
                    {
                     

                        Values = values,
                        Name = prct.Name,
                        Fill = null,
                        GeometrySize = 0,
                        LineSmoothness = 0,
                        Stroke = paint

                    });



                    //result.Add(seriesList);

                    /*
                    LineSeries lineSeries = new LineSeries();
                    ChartValues<double> Values = new ChartValues<double>();

                    foreach (CarboDataPoint point in data)
                    {
                        Values.Add(Math.Round((point.Value / 1000), 1));
                    }

                    lineSeries.Values = Values;
                    lineSeries.Title = prct.Name;
                    lineSeries.DataLabels = false;
                    lineSeries.Foreground = Brushes.Black;
                    lineSeries.PointGeometrySize = 5;
                    lineSeries.Width = 1;

                    //check min max
                    if (Values.Max() > max)
                        max = Values.Max();
                    if(Values.Min() < min) 
                        min = Values.Min();

                    result.Add(lineSeries);
                    */
                j++;
                }
            }

            return result;
            
        }


        internal static List<ISeries> getLevelChartMaterial(List<CarboElement> projectElements, string graphType, out List<string> labels)
        {
            List<ISeries> result = new List<ISeries>();
            SolidColorBrush almostBlack = (SolidColorBrush)(new BrushConverter().ConvertFrom("#050505"));
            string groupType = graphType;
            labels = new List<string>();

            CarboByLevelDataGroup carboByLevelDataGroup = new CarboByLevelDataGroup();

            carboByLevelDataGroup.AddLevel("Substructure", -9999);

            //add all the levels in the table:
            foreach (CarboElement ce in projectElements)
            {
                string levelName = ce.LevelName.ToString(); //levelname
                double elevation = Utils.ConvertMeToDouble(ce.Level.ToString()); //elevation
                bool isSubStructure = ce.isSubstructure;
                bool existingLevel = false;

                if (isSubStructure == true)
                    levelName = "Substructure";

                foreach (CarboByLevelData cld in carboByLevelDataGroup.levelList)
                {
                    if (cld.LevelName == levelName)
                    {
                        existingLevel = true;
                        break;
                    }
                }
                if (existingLevel == false)
                {
                    carboByLevelDataGroup.AddLevel(levelName, elevation);
                }
            }

            //loop through each row and see if the data needs to be added to an item or a new row needs adding.
            foreach (CarboElement ce in projectElements)
            {
                string levelName = ce.LevelName.ToString(); //levelname
                double elevation = ce.Level; //elevation
                string groupName = "";
                double value = ce.EC;

                bool isSubStructure = ce.isSubstructure;
                if (isSubStructure == true)
                    levelName = "Substructure";


                if (groupType == "Category")
                {
                    groupName = ce.Category; //Category
                }
                else if (groupType == "Material")
                {
                    groupName = ce.MaterialName; //material
                }
                else
                {
                    groupName = "Total EC"; //Totals
                }

                if (levelName != null && value != 0)
                {
                    carboByLevelDataGroup.AddItem(levelName, elevation, groupName, value);
                }
            }

            //List<string> categoryList = new List<string>();
            List<string> levelList = new List<string>();
            carboByLevelDataGroup.SortedList();

            //remove empty levels
            /*
            for(int i = carboByLevelDataGroup.levelList.Count -1; i>= 0;i--)
            {
               CarboByLevelData cbld = carboByLevelDataGroup.levelList[i] as CarboByLevelData;
                if(cbld != null)
                {
                    double total = cbld.DataPoints.Sum(item => item.Value);

                    if (cbld.DataPoints.Count == 0 || total == 0)
                        carboByLevelDataGroup.levelList.RemoveAt(i);
                }
            }
            */

            foreach (CarboByLevelData level in carboByLevelDataGroup.levelList)
            {
                levelList.Add(level.LevelName);
                labels.Add(level.LevelName);
            }

            //build the grapicsTable
            if (carboByLevelDataGroup.levelList.Count > 0 && carboByLevelDataGroup.levelList[0].DataPoints.Count > 0)
            {
                int levelCount = carboByLevelDataGroup.levelList.Count;
                int categoryCount = carboByLevelDataGroup.levelList[0].DataPoints.Count;


                //loop though
                // i is nr type of information extracted
                int j = 0;
                for (int i = 0; i < categoryCount; i++)
                {
                    //points.Add(Math.Round(ppin.Value / 1000, 2));

                    double ThisCategoryValue = 0;
                    List<double> values = new List<double>();
                    List<string> names = new List<string>();
                    string name = "value";

                    foreach (CarboByLevelData level in carboByLevelDataGroup.levelList)
                    {
                        double value = Math.Round((level.DataPoints[i].Value / 1000), 1);

                        values.Add(value);
                        name = level.DataPoints[i].Name;


                    }
                    //Values = new ChartValues<double> { Math.Round(ppin.Value / 1000, 2) },
                    j++;

                    result.Add(new StackedRowSeries<double>
                    {
                        Values = values.ToArray(),
                        Name = name,
                        Fill = new SolidColorPaint(getSKColour(j)),
                        YToolTipLabelFormatter = point => $"{point.Model:0.00} tCO₂e",
                        DataLabelsSize = 11
                        //string.Format("{0} tCO₂e", chartPoint.Y

                    });
              }
            }


            
            return result;

        }

        public static ICartesianAxis[] XAxis(List<string> levelList)
{
            List<ICartesianAxis> resultList = new List<ICartesianAxis>();

            resultList.Add(            
                new Axis
                {
                    LabelsRotation = 0,
                    Labels = levelList,
                    TextSize = 11
                });

            return resultList.ToArray();

        }

        /// <summary>
        /// This returns the pie chart of the projects phases total A1-D and Mixed
        /// </summary>
        /// <param name="carboLifeProject"></param>
        /// <returns></returns>
        internal static List<ISeries> GetPhasePieChartTotals(CarboProject carboLifeProject)
        {
            List<ISeries> result = null;
            SolidColorBrush almostBlack = (SolidColorBrush)(new BrushConverter().ConvertFrom("#050505"));

            try
            {
                List<CarboDataPoint> PieceListLifePoint = new List<CarboDataPoint>();

                PieceListLifePoint = carboLifeProject.getPhaseTotals();

                double totalEC = carboLifeProject.ECTotal;

                //Remove the zero's
                for (int i = PieceListLifePoint.Count - 1; i >= 0; i--)
                {
                    CarboDataPoint cp = PieceListLifePoint[i] as CarboDataPoint;
                    double valueRound = Math.Round(cp.Value, 1);
                    if (Math.Round(valueRound, 2) == 0)
                        PieceListLifePoint.RemoveAt(i);

                }

                if (PieceListLifePoint.Count == 0)
                    PieceListLifePoint.Add(new CarboDataPoint { Name = "Error", Value = 1 });

                //New Code: Trim all values below 5%, 
                List<int> counter = new List<int>();

                CarboDataPoint combinedPoint = new CarboDataPoint();
                combinedPoint.Name = "Combined:" + Environment.NewLine;

                for (int i = 0; i <= PieceListLifePoint.Count - 1; i++)
                {
                    CarboDataPoint cp = PieceListLifePoint[i];
                    double pecent = cp.Value / (totalEC * 1000);
                    if (pecent <= 0.05 && pecent >= -0.05)
                    {
                        //The item is too small for the graph:
                        combinedPoint.Value += cp.Value;
                        combinedPoint.Name += cp.Name + " (" + (Math.Round(cp.Value / 1000, 2)) + " tCO₂e )" + Environment.NewLine;

                        counter.Add(i);
                    }
                }
                
                //the items need to be deleted in the opposite order
                counter.Reverse();

                //If used, add to the Combined list, only if more than one materials / elements were merged.
                if (combinedPoint.Value > 0 && counter.Count > 1)
                {
                    PieceListLifePoint.Add(combinedPoint);
                    //Remove the items from the list
                    foreach (int i in counter)
                    {
                        PieceListLifePoint.RemoveAt(i);
                    }
                }


                //make positive if negative
                foreach (CarboDataPoint cp in PieceListLifePoint)
                {
                    if (cp.Value < 0)
                    {
                        cp.Name = cp.Name  + Environment.NewLine + "(Negative)";
                        cp.Value = cp.Value * -1;
                    }
                }



                //Location of Label;
                //Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} tCO₂e", chartPoint.Y, chartPoint.Participation);

                //Convert the CarboDataPoints to Pieseries
                List<ISeries> seriesList = new List<ISeries>();

                int j = 0;
                foreach (CarboDataPoint ppin in PieceListLifePoint)
                {
                    //points.Add(Math.Round(ppin.Value / 1000, 2));

                    double value = Math.Round(ppin.Value / 1000, 2);
                    string name = ppin.Name;

                    seriesList.Add(new PieSeries<double>
                    {
                        Values = new double[] { value },
                        Name = name,
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                        Fill = new SolidColorPaint(getSKColour(j)),
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsFormatter = point => $"{point.Model:0} tCO₂e",
                        ToolTipLabelFormatter = point => $"{point.Model:0} tCO₂e",
                        
                        DataLabelsSize = 12,
                        InnerRadius = 15,
                    });

                    //Values = new ChartValues<double> { Math.Round(ppin.Value / 1000, 2) },
                    j++;
                }
                result = seriesList;

            }
            catch
            {
                return null;
            }
            return result;

        }


        /// <summary>
        /// Returns the overview of Material Properties using the DataTable as per CarboCalcTextUtils.getResultTable(CarboLifeProject)
        /// </summary>
        /// <param name="resultsTable">DataTable as per CarboCalcTextUtils.getResultTable(CarboLifeProject)</param>
        /// <param name="Type">Material or Category</param>
        /// <returns></returns>
        internal static List<ISeries> GetPieChart(DataTable resultsTable, string Type = "Material", List<CarboElement> projectElements = null)
        {
            List<ISeries> result = new List<ISeries>();
            SolidColorBrush almostBlack = (SolidColorBrush)(new BrushConverter().ConvertFrom("#050505"));

            try
            {
                List<CarboDataPoint> PieceListMaterial = new List<CarboDataPoint>();
                //Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} tCO₂e", chartPoint.Y, chartPoint.Participation);

                //Get the DataPint
                PieceListMaterial = CarboCalcTextUtils.ConvertResultTableToDataPoints(resultsTable, Type, projectElements);

                PieceListMaterial = PieceListMaterial.OrderByDescending(o => o.Value).ToList();

                double total = PieceListMaterial.Sum(x => x.Value);


                //New Code: Trim all values below 5%, 
                List<int> counter = new List<int>();

                CarboDataPoint otherPoint = new CarboDataPoint();
                otherPoint.Name = "Combined:" + Environment.NewLine;

                for (int i = PieceListMaterial.Count - 1; i >= 0; i--)
                {
                    CarboDataPoint cp = PieceListMaterial[i] as CarboDataPoint;
                    double pecent = cp.Value / total;
                    if (pecent <= 0.05 && pecent >= -0.05)
                    {
                        //The item is too small for the graph:
                        otherPoint.Value += cp.Value;
                        otherPoint.Name += cp.Name + " (" + (Math.Round(cp.Value, 1)) + " tCO₂e )" + Environment.NewLine;

                        counter.Add(i);
                    }
                }

                //If used, add to the list, only if more than one materials / elements were merged.
                if (otherPoint.Value > 0 && counter.Count > 1)
                {
                    PieceListMaterial.Add(otherPoint);
                    //Remove the items from the list
                    foreach (int i in counter)
                    {
                        PieceListMaterial.RemoveAt(i);
                    }
                }



                //make positive if negative
                foreach (CarboDataPoint cp in PieceListMaterial)
                {
                    if (cp.Value < 0)
                    {
                        cp.Name = "(Negative)" + cp.Name;
                        cp.Value = cp.Value * -1;
                    }
                }

                int j = 0;
                foreach (CarboDataPoint ppin in PieceListMaterial)
                {
                    double value = Math.Round(ppin.Value / 1, 2);
                    string name = ppin.Name;

#pragma warning disable CA1416 // Validate platform compatibility
                    result.Add(new PieSeries<double>
                    {
                        Values = new double[] { value },
                        Name = name,
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                        DataLabelsFormatter = point => $"{point.Model:0} tCO₂e",
                        ToolTipLabelFormatter = point => $"{point.Model:0} tCO₂e",
                        InnerRadius = 0,
                        DataLabelsSize = 12,
                        Fill = new SolidColorPaint(getSKColour(j)),

                    });
#pragma warning restore CA1416 // Validate platform compatibility
                    j++;
                    /* Framework 4.8 Code:
                    PieSeries newSeries = new PieSeries
                    {
                        Title = ppin.Name,
                        Values = new ChartValues<double> { Math.Round(ppin.Value, 2) },
                        PushOut = 1,
                        DataLabels = true,
                        LabelPoint = labelPoint

                    };
                    newSeries.Foreground = Brushes.Black;
                    newSeries.FontWeight = FontWeights.Normal;
                    newSeries.FontStyle = FontStyles.Normal;
                    newSeries.FontSize = 12;

                    result.Add(newSeries);
                                            YToolTipLabelFormatter = point => $"{point.Model:0.00} tCO₂e",
                        DataLabelsSize = 11
                    */
                }


            }
            catch
            {
                return null;
            }
            return result;

        }

        /// <summary>
        /// Returns the overview of Material Properties using the DataTable as per CarboCalcTextUtils.getResultTable(CarboLifeProject)
        /// </summary>
        /// <param name="resultsTable">DataTable as per CarboCalcTextUtils.getResultTable(CarboLifeProject)</param>
        /// <param name="Type">Material or Category</param>
        /// <returns></returns>
        /*
        internal static List<PieSlice> GetScottPieChart(DataTable resultsTable, string Type = "Material", List<CarboElement> projectElements = null)
        {
            List<PieSlice> result = new List<PieSlice>();
            SolidColorBrush almostBlack = (SolidColorBrush)(new BrushConverter().ConvertFrom("#050505"));

            try
            {
                List<CarboDataPoint> PieceListMaterial = new List<CarboDataPoint>();
                //Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} tCO₂e", chartPoint.Y, chartPoint.Participation);

                //Get the DataPint
                PieceListMaterial = CarboCalcTextUtils.ConvertResultTableToDataPoints(resultsTable, Type, projectElements);

                PieceListMaterial = PieceListMaterial.OrderByDescending(o => o.Value).ToList();

                double total = PieceListMaterial.Sum(x => x.Value);


                //New Code: Trim all values below 5%, 
                List<int> counter = new List<int>();

                CarboDataPoint otherPoint = new CarboDataPoint();
                otherPoint.Name = "Combined:" + Environment.NewLine;

                for (int i = PieceListMaterial.Count - 1; i >= 0; i--)
                {
                    CarboDataPoint cp = PieceListMaterial[i] as CarboDataPoint;
                    double pecent = cp.Value / total;
                    if (pecent <= 0.05 && pecent >= -0.05)
                    {
                        //The item is too small for the graph:
                        otherPoint.Value += cp.Value;
                        otherPoint.Name += cp.Name + " (" + (Math.Round(cp.Value, 1)) + " tCO₂e )" + Environment.NewLine;

                        counter.Add(i);
                    }
                }

                //If used, add to the list, only if more than one materials / elements were merged.
                if (otherPoint.Value > 0 && counter.Count > 1)
                {
                    PieceListMaterial.Add(otherPoint);
                    //Remove the items from the list
                    foreach (int i in counter)
                    {
                        PieceListMaterial.RemoveAt(i);
                    }
                }



                //make positive if negative
                foreach (CarboDataPoint cp in PieceListMaterial)
                {
                    if (cp.Value < 0)
                    {
                        cp.Name = "(Negative)" + cp.Name;
                        cp.Value = cp.Value * -1;
                    }
                }

                int j = 0;
                foreach (CarboDataPoint ppin in PieceListMaterial)
                {
                    double value = Math.Round(ppin.Value / 1, 2);
                    string name = ppin.Name;

                    PieSlice newSlice = new PieSlice();
                    newSlice.Value = value;
                    newSlice.Label = name;
                    newSlice.LegendText = name;

                    FillStyle fill = new FillStyle();
                    fill.Color = GetScottPlotColor(j);
                    newSlice.Fill = fill;

                    result.Add(newSlice);
                    j++;
                }


            }
            catch
            {
                return null;
            }
            return result;

        }
        */

        
        /// <summary>
        /// Returns a colour value based on i value;
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        /// 
        internal static SKColor getSKColour(int i)
        {
            SKColor result = new SKColor();
               
            SKColor[] Colors = new SKColor[]
                {
        new SKColor(147, 123, 131, 255),
        new SKColor(87, 164, 177, 255),
        new SKColor(210, 210, 60, 255),
        new SKColor(90, 119, 135, 255),
        new SKColor(185, 220, 160, 255),
        new SKColor(156, 206, 212, 255),
        new SKColor(251, 226, 150, 255),
        new SKColor(253, 240, 203, 210),
        new SKColor(247, 203, 145, 255),
        new SKColor(252, 179, 179, 255),
        new SKColor(118, 73, 197, 255)
                };

            if (i < Colors.Length)
            {
                result = Colors[i];
                return result;
            }
            else
            {
                SKColor randomColor = new SKColor(
    (byte)Random.Shared.Next(256),
    (byte)Random.Shared.Next(256),
    (byte)Random.Shared.Next(256),
    255); // optional alpha

                result = randomColor;
                return result;

            }
        }

        /// <summary>
        /// Returns a ScottPlot.Color based on the input index.
        /// </summary>
        /// <param name="i">The index to select or generate a color.</param>
        /// <returns>A ScottPlot.Color</returns>
         /*
        internal static ScottPlot.Color GetScottPlotColor(int i)
        {
            ScottPlot.Color[] colors = new ScottPlot.Color[]
            {
            new ScottPlot.Color(147, 123, 131),
            new ScottPlot.Color(87, 164, 177),
            new ScottPlot.Color(90, 119, 135),
            new ScottPlot.Color(210, 210, 60),
            new ScottPlot.Color(185, 220, 160),
            new ScottPlot.Color(156, 206, 212),
            new ScottPlot.Color(251, 226, 150),
            new ScottPlot.Color(253, 240, 203, 210), // with alpha
            new ScottPlot.Color(247, 203, 145),
            new ScottPlot.Color(252, 179, 179),
            new ScottPlot.Color(118, 73, 197)
            };

            if (i < colors.Length)
            {
                return colors[i];
            }
            else
            {
                // Generate a random color
                return new ScottPlot.Color(
                    (byte)Random.Shared.Next(256),
                    (byte)Random.Shared.Next(256),
                    (byte)Random.Shared.Next(256),
                    255 // optional alpha
                );
            }
        }
         */

        /// <summary>
        /// Returns a datapoint list based on per category
        /// </summary>
        /// <param name="carboLifeProject"></param>
        /// <returns></returns>
        [Obsolete]
        internal static List<ISeries> GetPieChartCategoryTotals(CarboProject carboLifeProject)
        {
            List<ISeries> result = new List<ISeries>();
            SolidColorBrush almostBlack = (SolidColorBrush)(new BrushConverter().ConvertFrom("#050505"));

            try
            {
                List<CarboDataPoint> PieceListCategory = new List<CarboDataPoint>();
                //Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} tCO₂e", chartPoint.Y, chartPoint.Participation);

                PieceListCategory = carboLifeProject.getCategoryTotals();

                PieceListCategory = PieceListCategory.OrderByDescending(o => o.Value).ToList();

                //int total = monValues.Sum(x => Convert.ToInt32(x));

                double total = PieceListCategory.Sum(x => x.Value);

                //New Code: Trim all values below 5%, 
                List<int> counter = new List<int>();

                CarboDataPoint otherPoint = new CarboDataPoint();
                otherPoint.Name = "Combined: " + Environment.NewLine;

                for (int i = PieceListCategory.Count - 1; i >= 0; i--)
                {
                    CarboDataPoint cp = PieceListCategory[i] as CarboDataPoint;
                    double pecent = cp.Value / total;
                    if (pecent <= 0.05 && pecent > 0)
                    {
                        //The item is too small for the graph:
                        otherPoint.Value += cp.Value;
                        counter.Add(i);
                    }
                }

                //If used, add to the Miscellaneous list, only if more than one materials / elements were merged.
                if (otherPoint.Value > 0 && counter.Count > 1)
                {
                    PieceListCategory.Add(otherPoint);
                    //Remove the items from the list
                    foreach (int i in counter)
                    {
                        PieceListCategory.RemoveAt(i);
                    }
                }



                //make positive if negative
                foreach (CarboDataPoint cp in PieceListCategory)
                {
                    if (cp.Value < 0)
                    {
                        cp.Name = "(Negative)" + cp.Name;
                        cp.Value = cp.Value * -1;
                    }
                }


                foreach (CarboDataPoint ppin in PieceListCategory)
                {
                    /*
                    PieSeries newSeries = new PieSeries
                    {
                        Title = ppin.Name,
                        Values = new ChartValues<double> { Math.Round(ppin.Value, 2) },
                        PushOut = 1,
                        DataLabels = true,
                        LabelPoint = labelPoint

                    };
                    newSeries.Foreground = almostBlack;
                    newSeries.FontWeight = FontWeights.Normal;
                    newSeries.FontStyle = FontStyles.Normal;
                    newSeries.FontSize = 12;

                    result.Add(newSeries);
                    */
                }
            }
            catch
            {
                return null;
            }
            return result;

        }

        internal static IEnumerable<ICartesianAxis> getYearAxis()
        {
            Axis[] XAxes = new Axis[]
            {
    new Axis
    {
        Name = "Year",
        TextSize = 12
        

    }
            };

            return XAxes;
        }

        internal static IEnumerable<ICartesianAxis> getProjectAxis(List<string> levelList)
        {
            List<ICartesianAxis> resultList = new List<ICartesianAxis>();

            resultList.Add(
                new Axis
                {
                    Name = "Projects",
                    LabelsRotation = 0,
                    Labels = levelList,
                    TextSize = 12
                });

            return resultList.ToArray();
        }


        internal static IEnumerable<ICartesianAxis> getCarbonAxis()
        {
            Axis[] YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Emissions",
                    Labeler = value => $"{value:N0} tCO₂e",
                    TextSize = 12

                }
            };

            return YAxes;

        }
    }
}
