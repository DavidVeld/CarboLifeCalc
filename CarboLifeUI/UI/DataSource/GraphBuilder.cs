using Autodesk.Revit.DB;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
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
        public static SeriesCollection BuildBarGraph(CarboProject project)
        {
            List<CarboDataPoint> pointCollection = new List<CarboDataPoint>();
            // get the current Project
            pointCollection = project.getPhaseTotals();

            SeriesCollection result = new SeriesCollection();

            //Build series

            foreach (CarboDataPoint pont in pointCollection)
            {
                StackedColumnSeries series = new StackedColumnSeries
                {
                    Values = new ChartValues<double>
                    {
                        Math.Round((pont.Value / 1000), 2),
                    },
                    StackMode = StackMode.Values,
                    DataLabels = true,
                    Title = pont.Name
                };

                result.Add(series);

            }

            return result;

        }
        /// <summary>
        /// Returns a comaring bar chart showing the total values of the project
        /// </summary>
        /// <param name="carboLifeProject"></param>
        /// <param name="projectListToCompareTo"></param>
        /// <returns></returns>
        internal static SeriesCollection BuildComparingTotalsBarGraph(CarboProject project, List<CarboProject> projectListToCompareTo)
        {
            SeriesCollection result = new SeriesCollection();

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
                pointList.Add(listofPoints);
            }

            try
            {

                //loop though
                // i is nr type of information extracted
                for (int i = 0; i < pointList[0].Count; i++)
                {
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

            }
            catch
            {
                return null;
            }
            return result;

        }

        //
        internal static SeriesCollection BuildLifeLine(CarboProject project, List<CarboProject> projectListToCompareTo, bool sequestration, bool energy, bool demolition)
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

            SeriesCollection result = new SeriesCollection();

            if (projectList.Count > 0)
            {
                foreach (CarboProject prct in projectList)
                {
                    IList<CarboDataPoint> data = CarboTimeLine.GetTimeLineDataPoints(prct, sequestration, energy, demolition);

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
                }
            }

            return result;
            
        }

        internal static ColorsCollection getColours()
        {

            ColorsCollection result = new ColorsCollection();

            Color c1 = new Color { A = 255, R = 147, G = 123, B = 131 }; 
            Color c2 = new Color { A = 255, R = 87, G = 164, B = 177 }; 
            Color c3 = new Color { A = 255, R = 90, G = 119, B = 135 }; 
            Color c4 = new Color { A = 255, R = 210, G = 210, B = 60 };
            Color c5 = new Color { A = 255, R = 185, G = 220, B = 160 }; 
            Color c6 = new Color { A = 255, R = 156, G = 206, B = 212 }; 
            Color c7 = new Color { A = 255, R = 251, G = 226, B = 150 }; 
            Color c8 = new Color { A = 210, R = 253, G = 240, B = 203 }; 
            Color c9 = new Color { A = 255, R = 247, G = 203, B = 145 };  
            Color c10 = new Color { A = 255, R = 252, G = 179, B = 179 }; 
            Color c11 = new Color { A = 255, R = 118, G = 73, B = 197 };


            result.Add(c1);
            result.Add(c2);
            result.Add(c3);
            result.Add(c4);
            result.Add(c5);
            result.Add(c6);
            result.Add(c7);
            result.Add(c8);
            result.Add(c9);
            result.Add(c10);
            result.Add(c11);





            return result;
        }

        [Obsolete]
        internal static SeriesCollection getLevelChartMaterial(DataTable currentProjectResult, string graphType, out List<string> labels)
        {
            SeriesCollection result = new SeriesCollection();
            SolidColorBrush almostBlack = (SolidColorBrush)(new BrushConverter().ConvertFrom("#050505"));
            string groupType = graphType;
            labels = new List<string>();

            CarboByLevelDataGroup carboByLevelDataGroup = new CarboByLevelDataGroup();

            carboByLevelDataGroup.AddLevel("Substructure", -9999);

            //add all the levels in the table:
            foreach (DataRow dr in currentProjectResult.Rows)
            {
                string levelName = dr[0].ToString(); //levelname
                double elevation = Utils.ConvertMeToDouble(dr[1].ToString()); //elevation
                bool isSubStructure = Convert.ToBoolean(dr[5]);
                bool existingLevel = false;
                 
                if(isSubStructure == true)
                    levelName = "Substructure";

                foreach (CarboByLevelData cld in carboByLevelDataGroup.levelList)
                {
                    if(cld.LevelName == levelName)
                    {
                        existingLevel = true;
                        break;
                    }
                }
                if(existingLevel == false)
                {
                    carboByLevelDataGroup.AddLevel(levelName, elevation);
                }
            }

            //loop through each row and see if the data needs to be added to an item or a new row needs adding.
            foreach (DataRow dr in currentProjectResult.Rows)
            {
                string levelName = dr[0].ToString(); //levelname
                double elevation = Utils.ConvertMeToDouble(dr[1].ToString()); //elevation
                string groupName = "";
                double value = Utils.ConvertMeToDouble(dr[6].ToString());

                bool isSubStructure = Convert.ToBoolean(dr[5]);
                if (isSubStructure == true)
                    levelName = "Substructure";


                if (groupType == "Category")
                {
                    groupName = dr[2].ToString(); //Category
                }
                else if (groupType == "Material")
                {
                    groupName = dr[4].ToString(); //material
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
                for (int i = 0; i < categoryCount; i++)
                {
                    StackedRowSeries newSeries = new StackedRowSeries();
                    
                    ChartValues<double> Values = new ChartValues<double>();

                    //get all Values from all loaded projects
                    foreach (CarboByLevelData level in carboByLevelDataGroup.levelList)
                    {
                        Values.Add(Math.Round((level.DataPoints[i].Value / 1000), 1));
                    }

                    newSeries.Title = carboByLevelDataGroup.levelList[0].DataPoints[i].Name;
                    newSeries.Values = Values;
                    newSeries.StackMode = StackMode.Values;
                    newSeries.DataLabels = false;
                    newSeries.Foreground = Brushes.Black;
                    
                    //newSeries.Width = 100;
                    //newSeries.MaxColumnWidth = 50;
                    //newSeries.LabelsPosition = BarLabelPosition.Merged;

                    result.Add(newSeries);

                }
            }
           
            return result;

        }

        internal static SeriesCollection getLevelChartMaterial(List<CarboElement> projectElements, string graphType, out List<string> labels)
        {
            SeriesCollection result = new SeriesCollection();
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
                for (int i = 0; i < categoryCount; i++)
                {
                    StackedRowSeries newSeries = new StackedRowSeries();

                    ChartValues<double> Values = new ChartValues<double>();

                    //get all Values from all loaded projects
                    foreach (CarboByLevelData level in carboByLevelDataGroup.levelList)
                    {
                        Values.Add(Math.Round((level.DataPoints[i].Value / 1000), 1));
                    }

                    newSeries.Title = carboByLevelDataGroup.levelList[0].DataPoints[i].Name;
                    newSeries.Values = Values;
                    newSeries.StackMode = StackMode.Values;
                    newSeries.DataLabels = false;
                    newSeries.Foreground = Brushes.Black;

                    //newSeries.Width = 100;
                    //newSeries.MaxColumnWidth = 50;
                    //newSeries.LabelsPosition = BarLabelPosition.Merged;

                    result.Add(newSeries);

                }
            }

            return result;

        }


        /// <summary>
        /// This returns the pie chart of the projects phases total A1-D and Mixed
        /// </summary>
        /// <param name="carboLifeProject"></param>
        /// <returns></returns>
        internal static SeriesCollection GetPhasePieChartTotals(CarboProject carboLifeProject)
        {
            SeriesCollection result = new SeriesCollection();
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
                Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} tCO₂e", chartPoint.Y, chartPoint.Participation);

                //Convert the CarboDataPoints to Pieseries
                foreach (CarboDataPoint ppin in PieceListLifePoint)
                {
                    PieSeries newSeries = new PieSeries
                    {
                        Title = ppin.Name,
                        Values = new ChartValues<double> { Math.Round(ppin.Value / 1000, 2) },
                        PushOut = 1,
                        DataLabels = true,
                        LabelPoint = labelPoint
                    };

                    newSeries.Foreground = almostBlack;
                    newSeries.FontWeight = FontWeights.Normal;
                    newSeries.FontStyle = FontStyles.Normal;
                    newSeries.FontSize = 12;

                    result.Add(newSeries);

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
        internal static SeriesCollection GetPieChart(DataTable resultsTable, string Type = "Material", List<CarboElement> projectElements = null)
        {
            SeriesCollection result = new SeriesCollection();
            SolidColorBrush almostBlack = (SolidColorBrush)(new BrushConverter().ConvertFrom("#050505"));

            try
            {
                List<CarboDataPoint> PieceListMaterial = new List<CarboDataPoint>();
                Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} tCO₂e", chartPoint.Y, chartPoint.Participation);

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


                foreach (CarboDataPoint ppin in PieceListMaterial)
                {
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

                }


            }
            catch
            {
                return null;
            }
            return result;

        }


        /// <summary>
        /// Returns a datapoint list based on per category
        /// </summary>
        /// <param name="carboLifeProject"></param>
        /// <returns></returns>
        [Obsolete]
        internal static SeriesCollection GetPieChartCategoryTotals(CarboProject carboLifeProject)
        {
            SeriesCollection result = new SeriesCollection();
            SolidColorBrush almostBlack = (SolidColorBrush)(new BrushConverter().ConvertFrom("#050505"));

            try
            {
                List<CarboDataPoint> PieceListCategory = new List<CarboDataPoint>();
                Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} tCO₂e", chartPoint.Y, chartPoint.Participation);

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
                }
            }
            catch
            {
                return null;
            }
            return result;

        }


    }
}
