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
using System.Windows.Media;

namespace CarboLifeUI.UI
{
    public static class GraphBuilder
    {

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
                /*
                DataTable dt = new DataTable();

                dt.Columns.Add("A1-A3");
                dt.Columns.Add("A4");
                dt.Columns.Add("A5");
                dt.Columns.Add("B1B7");
                dt.Columns.Add("C1-C4");
                dt.Columns.Add("D");
                dt.Columns.Add("Mix");
                dt.Columns.Add("Add");
                dt.Columns.Add("B4");


                foreach (List<CarboDataPoint> pr in pointList)
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < pr.Count - 1; i++)
                    {
                        dr[i] = pr[i].Value / 1000;
                    }
                    dt.Rows.Add(dr);
                }

               0  CarboDataPoint cb_A1A3 = new CarboDataPoint("A1-A3", 0);
               1 CarboDataPoint cb_A4 = new CarboDataPoint("A4", 0);
               2 CarboDataPoint cb_A5 = new CarboDataPoint("A5(Material)",0);
               3 CarboDataPoint cb_A5Global = new CarboDataPoint("A5(Global)", this.A5Global);
               4 CarboDataPoint cb_B1B5 = new CarboDataPoint("B1-B7", 0);
               5 CarboDataPoint cb_C1C4 = new CarboDataPoint("C1-C4", 0);
               6 CarboDataPoint cb_C1Global = new CarboDataPoint("C1(Global)", this.C1Global);
               7 CarboDataPoint cb_D = new CarboDataPoint("D", 0);
               8 CarboDataPoint Added = new CarboDataPoint("Additional", 0);


                */

                //loop though
                // i is nr type of information extracted
                for (int i = 0; i < 9; i++)
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

        internal static ColorsCollection getColours()
        {

            ColorsCollection result = new ColorsCollection();

            Color c1 = new Color { A = 255, R = 100, G = 130, B = 185 }; //A1-A3 (Blue tone)
            Color c2 = new Color { A = 255, R = 180, G = 150, B = 100 }; //A4 (Brown tone)
            Color c3 = new Color { A = 255, R = 164, G = 180, B = 100 }; //A5 (0) (Yellowish)
            Color c4 = new Color { A = 255, R = 210, G = 210, B = 60 }; //A5 Global (Bright yellow)
            Color c5 = new Color { A = 255, R = 150, G = 150, B = 150 }; //B (0)
            Color c6 = new Color { A = 255, R = 190, G = 88, B = 90 }; //C (Red)
            Color c7 = new Color { A = 255, R = 200, G = 60, B = 60 }; //C Global (Red)
            Color c8 = new Color { A = 210, R = 210, G = 205, B = 60 }; //D (Blue)
            Color c9 = new Color { A = 255, R = 50, G = 50, B = 50 };  //Added
            Color c10 = new Color { A = 255, R = 160, G = 100, B = 175 };//Extra


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





            return result;
        }


        /// <summary>
        /// This returns the pie chart of the projects phases total A1-D and Mixed
        /// </summary>
        /// <param name="carboLifeProject"></param>
        /// <returns></returns>
        internal static SeriesCollection GetPieChartTotals(CarboProject carboLifeProject)
        {
            SeriesCollection result = new SeriesCollection();

            try
            {
                List<CarboDataPoint> PieceListLifePoint = new List<CarboDataPoint>();

                PieceListLifePoint = carboLifeProject.getPhaseTotals();

                //make positive if negative
                foreach (CarboDataPoint cp in PieceListLifePoint)
                {
                    if (cp.Value < 0)
                    {
                        cp.Name = "(Negative)" + cp.Name;
                        cp.Value = cp.Value * -1;
                    }
                }


                Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} tCO2", chartPoint.Y, chartPoint.Participation);


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

        internal static SeriesCollection GetPieChartMaterials(CarboProject carboLifeProject)
        {
            SeriesCollection result = new SeriesCollection();
            try
            {
                List<CarboDataPoint> PieceListMaterial = new List<CarboDataPoint>();
                Func<ChartPoint, string> labelPoint = chartPoint => string.Format("{0} tCO2", chartPoint.Y, chartPoint.Participation);

                PieceListMaterial = carboLifeProject.getMaterialTotals();

                PieceListMaterial = PieceListMaterial.OrderByDescending(o => o.Value).ToList();

                //if there are too many materials in the list combine any items over 8.
                if(PieceListMaterial.Count > 8)
                {
                    CarboDataPoint otherPoint = new CarboDataPoint();
                    otherPoint.Name = "Miscellaneous";

                    for (int i = PieceListMaterial.Count -1; i>7; i--)
                    {
                        CarboDataPoint cp = PieceListMaterial[i] as CarboDataPoint;
                        otherPoint.Value += cp.Value;
                        PieceListMaterial.RemoveAt(i);
                    }
                    PieceListMaterial.Add(otherPoint);
                }

                //make positive if negative
                foreach(CarboDataPoint cp in PieceListMaterial)
                {
                    if(cp.Value < 0)
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


    }
}
