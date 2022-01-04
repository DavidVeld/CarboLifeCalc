using CarboLifeAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace CarboLifeUI
{
    internal class ScorsIndicator
    {
        internal static IEnumerable<UIElement> generateImage(Canvas myCanvas, double CarbonA15, double CarbonA1C, double GIA, string buildingType)
        {
            IList<UIElement> result = new List<UIElement>();

            double canvasWidth = myCanvas.ActualWidth;
            double canvasHeight = myCanvas.ActualHeight;

            double XOffset = canvasWidth / 10;
            double YOffset = 0;
            double YCurrent = YOffset;

            double CarbonPerAreaA15 = CarbonA15 / GIA;
            double CarbonPerAreaA1C = CarbonA1C / GIA;

            //Decide on LetiScale;
            LetiScore scoreTypeA15;
            LetiScore scoreTypeA1C;

            IList<LetiScore> ScoresList = new List<LetiScore>();
            ScoresList = getLetiList();

            if (ScoresList.Count <= 0)
                return result;

            if (buildingType == "")
                buildingType = ScoresList[0].BuildingType;

            scoreTypeA15 = getLetiScore(ScoresList, buildingType, false);
            scoreTypeA1C = getLetiScore(ScoresList, buildingType, true);


            IEnumerable<UIElement> arrowListAAA = generateArrows("A++");
            IEnumerable<UIElement> arrowListAA = generateArrows("A+");
            IEnumerable<UIElement> arrowListA = generateArrows("A");
            IEnumerable<UIElement> arrowListB = generateArrows("B");
            IEnumerable<UIElement> arrowListC = generateArrows("C");
            IEnumerable<UIElement> arrowListD = generateArrows("D");
            IEnumerable<UIElement> arrowListE = generateArrows("E");
            IEnumerable<UIElement> arrowListF = generateArrows("F");
            IEnumerable<UIElement> arrowListG = generateArrows("G");

            //Title & Legend
            IEnumerable<UIElement> legend = generateLegend(scoreTypeA15, scoreTypeA1C);
            //kgCO₂e/m²
            IEnumerable<UIElement> indicatorArrow1 = new List<UIElement>();
            IEnumerable<UIElement> indicatorArrow2 = new List<UIElement>();

            if (scoreTypeA15 != null)
                indicatorArrow1 = generateIndicatorArrow(scoreTypeA15, CarbonPerAreaA15, 350);

            if(scoreTypeA1C != null)
                indicatorArrow2 = generateIndicatorArrow(scoreTypeA1C, CarbonPerAreaA1C, 460);

            foreach (UIElement uielement in arrowListAAA)
            {
                result.Add(uielement);
            }
            foreach (UIElement uielement in arrowListAA)
            {
                result.Add(uielement);
            }
            foreach (UIElement uielement in arrowListA)
            {
                result.Add(uielement);
            }
            foreach (UIElement uielement in arrowListB)
            {
                result.Add(uielement);
            }
            foreach (UIElement uielement in arrowListC)
            {
                result.Add(uielement);
            }
            foreach (UIElement uielement in arrowListD)
            {
                result.Add(uielement);
            }
            foreach (UIElement uielement in arrowListE)
            {
                result.Add(uielement);
            }
            foreach (UIElement uielement in arrowListF)
            {
                result.Add(uielement);
            }
            foreach (UIElement uielement in arrowListG)
            {
                result.Add(uielement);
            }

            foreach (UIElement uielement in legend)
            {
                result.Add(uielement);
            }

            foreach (UIElement uielement in indicatorArrow1)
            {
                result.Add(uielement);
            }
            foreach (UIElement uielement in indicatorArrow2)
            {
                result.Add(uielement);
            }

            return result;

        }

        private static LetiScore getLetiScore(IList<LetiScore> ScoresList, string buildingType, bool allowForSequestration)
        {
            LetiScore result = null;

            foreach (LetiScore ls in ScoresList)
            {
                if (ls.BuildingType == buildingType && ls.sequestration == allowForSequestration)
                { 
                result = ls;
                break;
                }
            }

            return result;
        }

        public static IList<LetiScore> getLetiList()
        {
            IList<LetiScore> result = new List<LetiScore>();


            //Find Profilelist;
            string myPath = Utils.getAssemblyPath() + "\\data\\" + "Letidata.csv";

            if (File.Exists(myPath))
            {
                DataTable profileTable = Utils.LoadCSV(myPath);
                foreach (DataRow dr in profileTable.Rows)
                {
                    LetiScore scors = new LetiScore();

                    string BuildingType = dr[0].ToString();

                    string sequestrationText = dr[1].ToString();

                    Int32 AAA = (int)Utils.ConvertMeToDouble(dr[2].ToString());
                    Int32 AA = (int)Utils.ConvertMeToDouble(dr[3].ToString());
                    Int32 A = (int)Utils.ConvertMeToDouble(dr[4].ToString());
                    Int32 B = (int)Utils.ConvertMeToDouble(dr[5].ToString());
                    Int32 C = (int)Utils.ConvertMeToDouble(dr[6].ToString());
                    Int32 D = (int)Utils.ConvertMeToDouble(dr[7].ToString());
                    Int32 E = (int)Utils.ConvertMeToDouble(dr[8].ToString());
                    Int32 F = (int)Utils.ConvertMeToDouble(dr[9].ToString());
                    Int32 G = (int)Utils.ConvertMeToDouble(dr[10].ToString());


                    scors.BuildingType = BuildingType;

                    if(sequestrationText == "TRUE")
                        scors.sequestration = true;
                    else
                        scors.sequestration = false;

                    scors.AAA = AAA;
                    scors.AA = AA;
                    scors.A = A;
                    scors.B = B;
                    scors.C = C;
                    scors.D = D;
                    scors.E = E;
                    scors.F = F;
                    scors.G = G;

                    result.Add(scors);
                }
            }
            else
            {
                MessageBox.Show("File: " + myPath + " could not be found, make sure you have the profile list located in indicated folder");
            }

            return result;
        }

        private static IEnumerable<UIElement> generateArrows(string letter)
        {
            double xScale = 1;
            string colour = "#eda75c";
            double width = 100;
            double height = 25;
            double spacing = 3;

            double yStart = 75;
            double xStart = 75;
            string rating = "";

            switch (letter)
            {
                case "A++":
                    xScale = 1;
                    colour = "#048337";
                    yStart = (1 * (height + spacing));
                    break;
                case "A+":
                    xScale = 1.1;
                    colour = "#45AA50";
                    yStart = (2 * (height + spacing));

                    break;
                case "A":
                    xScale = 1.2;
                    colour = "#A8C812";
                    yStart = (3 * (height + spacing));

                    break;
                case "B":
                    xScale = 1.3;
                    colour = "#F3E72C";
                    yStart = (4 * (height + spacing));
                    rating = "2030 Design Target";
                    break;
                case "C":
                    xScale = 1.4;
                    colour = "#FECC46";
                    yStart = (5 * (height + spacing));
                    break;
                case "D":
                    xScale = 1.5;
                    colour = "#EF7C1A";
                    yStart = (6 * (height + spacing));
                    rating = "Good";
                    break;
                case "E":
                    xScale = 1.6;
                    colour = "#E52320";
                    yStart = (7 * (height + spacing));
                    break;
                case "F":
                    xScale = 1.7;
                    colour = "#D10913";
                    yStart = (8 * (height + spacing));
                    rating = "Average";
                    break;
                case "G":
                    xScale = 1.8;
                    colour = "#A81916";
                    yStart = (9 * (height + spacing));
                    break;
                default:
                    xScale = 1.9;
                    colour = "#A81916";
                    yStart = (10 * (height + spacing));
                    break;
            }

            width = xScale * width;


            IList<UIElement> result = new List<UIElement>();

            SolidColorBrush mySolidColorBrushSupport = (SolidColorBrush)(new BrushConverter().ConvertFrom(colour));

            Polyline arrow = new Polyline();
            arrow.Visibility = System.Windows.Visibility.Visible;
            arrow.StrokeThickness = 1;
            arrow.Stroke = System.Windows.Media.Brushes.Black;
            arrow.Fill = mySolidColorBrushSupport;

            PointCollection connecitonPoints = new PointCollection();

            Point pnt1 = new Point(xStart, yStart);
            Point pnt2 = new Point(xStart + width, yStart);
            Point pnt3 = new Point(xStart + width + 10, yStart + height / 2);
            Point pnt4 = new Point(xStart + width, yStart + height);

            Point pnt5 = new Point(xStart, yStart+ height);
            Point pnt6 = new Point(xStart, yStart);

            connecitonPoints.Add(pnt1);
            connecitonPoints.Add(pnt2);
            connecitonPoints.Add(pnt3);
            connecitonPoints.Add(pnt4);
            connecitonPoints.Add(pnt5);
            connecitonPoints.Add(pnt6);

            arrow.Points = connecitonPoints;
            //

            result.Add(arrow);

            //Add the text
            TextBlock label = new TextBlock();
            label.Text = letter;
            label.FontStyle = FontStyles.Normal;
            label.FontWeight = FontWeights.Bold;
            label.Foreground = Brushes.Black;
            label.TextWrapping = TextWrapping.WrapWithOverflow;
            label.VerticalAlignment = VerticalAlignment.Top;
            label.FontSize = 16;

            Canvas.SetLeft(label, xStart + 5);
            Canvas.SetTop(label, yStart + (height / 4) - 5);
            result.Add(label);

            if(letter == "F" || letter == "D" || letter == "B")
            {
                //Add dashed Leti line
                Line line = new Line();
                line.Stroke = Brushes.Gray;

                line.X1 = xStart;
                line.Y1 = yStart - (spacing / 2);

                line.X2 = xStart + 200;
                line.Y2 = yStart - (spacing / 2);

                line.StrokeThickness = 1;
                line.StrokeDashArray.Add(10);
                line.StrokeDashArray.Add(10);
                line.StrokeDashArray.Add(10);
                line.StrokeDashArray.Add(10);
                result.Add(line);

                //Add the rating text
                TextBlock ratingBlock = new TextBlock();
                ratingBlock.Text = rating;
                ratingBlock.FontStyle = FontStyles.Normal;
                ratingBlock.FontWeight = FontWeights.Normal;
                ratingBlock.Foreground = Brushes.Black;
                ratingBlock.TextWrapping = TextWrapping.WrapWithOverflow;
                ratingBlock.VerticalAlignment = VerticalAlignment.Top;
                ratingBlock.HorizontalAlignment = HorizontalAlignment.Right;

                ratingBlock.FontSize = 10;

                Canvas.SetLeft(ratingBlock, xStart + 25);
                Canvas.SetTop(ratingBlock, yStart + (height / 4) - 5);
                result.Add(ratingBlock);

            }

            return result;

        }
        private static IEnumerable<UIElement> generateLegend(LetiScore scoreTypeA15, LetiScore scoreTypeA1C)
        {
            if (scoreTypeA15 == null)
                scoreTypeA15 = new LetiScore();

            if (scoreTypeA1C == null)
                scoreTypeA1C = new LetiScore();


            double height = 25;
            double spacing = 3;

            double xStart = 0;

            double ymin = 75 - 3 - 12.5;
            double ymax = ymin + (8 * (height + spacing));
            double distance = ymax - ymin;

            double valuedist = scoreTypeA15.G - scoreTypeA15.AAA;
            double valueper1 =   distance / valuedist;


            IList<UIElement> result = new List<UIElement>();

            //Add the text kgCO₂e/m²
            TextBlock label = new TextBlock();
            label.Text = "A1-A5 / A1-C " + "kgCO₂e/m²";
            label.FontStyle = FontStyles.Normal;
            label.FontWeight = FontWeights.Normal;
            label.Foreground = Brushes.Black;
            label.TextWrapping = TextWrapping.WrapWithOverflow;
            label.VerticalAlignment = VerticalAlignment.Top;
            label.FontSize = 12;

            Canvas.SetLeft(label, xStart);
            Canvas.SetTop(label, -5);
            result.Add(label);

            //LOWEST:"A+++"
            TextBlock labelAAA = new TextBlock();
            labelAAA.Text = scoreTypeA15.AAA.ToString() + "/" + scoreTypeA1C.AAA.ToString();
            labelAAA.FontStyle = FontStyles.Normal;
            labelAAA.FontWeight = FontWeights.Normal;
            labelAAA.Foreground = Brushes.Black;
            labelAAA.TextWrapping = TextWrapping.WrapWithOverflow;
            labelAAA.VerticalAlignment = VerticalAlignment.Top;
            labelAAA.FontSize = 12;
            Canvas.SetLeft(labelAAA, xStart);
            Canvas.SetTop(labelAAA, (scoreTypeA15.AAA * valueper1) + (height / 2));
            result.Add(labelAAA);

            //"A"
            TextBlock labelA = new TextBlock();
            labelA.Text = scoreTypeA15.A.ToString() + "/" + scoreTypeA1C.A.ToString();
            labelA.FontStyle = FontStyles.Normal;
            labelA.FontWeight = FontWeights.Normal;
            labelA.Foreground = Brushes.Black;
            labelA.TextWrapping = TextWrapping.WrapWithOverflow;
            labelA.VerticalAlignment = VerticalAlignment.Top;
            labelA.FontSize = 12;
            Canvas.SetLeft(labelA, xStart);
            Canvas.SetTop(labelA, (scoreTypeA15.A * valueper1) + (height / 2));
            result.Add(labelA);

            //"C"
            TextBlock labelC = new TextBlock();
            labelC.Text = scoreTypeA15.C.ToString() + "/" + scoreTypeA1C.C.ToString();
            labelC.FontStyle = FontStyles.Normal;
            labelC.FontWeight = FontWeights.Normal;
            labelC.Foreground = Brushes.Black;
            labelC.TextWrapping = TextWrapping.WrapWithOverflow;
            labelC.VerticalAlignment = VerticalAlignment.Top;
            labelC.FontSize = 12;
            Canvas.SetLeft(labelC, xStart);
            Canvas.SetTop(labelC, (scoreTypeA15.C * valueper1) + (height / 2));
            result.Add(labelC);

            //"E"
            TextBlock labelE = new TextBlock();
            labelE.Text = scoreTypeA15.E.ToString() + "/" + scoreTypeA1C.E.ToString();
            labelE.FontStyle = FontStyles.Normal;
            labelE.FontWeight = FontWeights.Normal;
            labelE.Foreground = Brushes.Black;
            labelE.TextWrapping = TextWrapping.WrapWithOverflow;
            labelE.VerticalAlignment = VerticalAlignment.Top;
            labelE.FontSize = 12;
            labelE.Foreground = Brushes.Black;
            Canvas.SetLeft(labelE, xStart);
            Canvas.SetTop(labelE, (scoreTypeA15.E * valueper1) + (height / 2));
            result.Add(labelE);

            //"G"
            TextBlock labelG = new TextBlock();
            labelG.Text = scoreTypeA15.G.ToString() + "/" + scoreTypeA1C.G.ToString();
            labelG.FontStyle = FontStyles.Normal;
            labelG.FontWeight = FontWeights.Normal;
            labelG.TextWrapping = TextWrapping.WrapWithOverflow;
            labelG.VerticalAlignment = VerticalAlignment.Top;
            labelG.FontSize = 12;
            labelG.Foreground = Brushes.Black;
            Canvas.SetLeft(labelG, xStart);
            Canvas.SetTop(labelG, (scoreTypeA15.G * valueper1) + (height / 2));
            result.Add(labelG);

            return result;

        }
        private static IEnumerable<UIElement> generateIndicatorArrow(LetiScore scoreType, double value, double xoffset)
        {
            double xScale = 1;
            double width = 50;
            double height = 25;
            double spacing = 3;

            double xStart = xoffset;

            double ymin = 75 - 3 - 12.5;
            double ymax = ymin + (8 * (height + spacing));
            double distance = ymax - ymin;

            double valuedist = scoreType.G - scoreType.AAA;
            double valueper1 = distance / valuedist;

            width = xScale * width;
            double yStart = (value * valueper1) + (height / 2);

            //Check results;
            if (value < 0)
                yStart = 0;
            if (value > scoreType.G)
                yStart = (scoreType.G * valueper1) + (height / 2);


            IList<UIElement> result = new List<UIElement>();


            Polyline arrow = new Polyline();
            arrow.Visibility = System.Windows.Visibility.Visible;
            arrow.StrokeThickness = 1;
            arrow.Stroke = Brushes.Black;
            arrow.Fill = Brushes.Black;

            PointCollection connecitonPoints = new PointCollection();

            Point pnt1 = new Point(xStart, yStart);
            Point pnt2 = new Point(xStart - width, yStart);
            Point pnt3 = new Point(xStart - width - 10, yStart + height / 2);
            Point pnt4 = new Point(xStart - width, yStart + height);

            Point pnt5 = new Point(xStart, yStart + height);
            Point pnt6 = new Point(xStart, yStart);

            connecitonPoints.Add(pnt1);
            connecitonPoints.Add(pnt2);
            connecitonPoints.Add(pnt3);
            connecitonPoints.Add(pnt4);
            connecitonPoints.Add(pnt5);
            connecitonPoints.Add(pnt6);

            arrow.Points = connecitonPoints;
            //

            result.Add(arrow);

            //Add the value
            TextBlock valuelabel = new TextBlock();
            valuelabel.Text = Math.Round(value,0).ToString() + " kgCO₂e/m²";
            valuelabel.FontStyle = FontStyles.Normal;
            valuelabel.FontWeight = FontWeights.Bold;
            valuelabel.Foreground = Brushes.Black;
            valuelabel.TextWrapping = TextWrapping.WrapWithOverflow;
            valuelabel.VerticalAlignment = VerticalAlignment.Top;
            valuelabel.FontSize = 14;
            Canvas.SetLeft(valuelabel, xStart - 80);
            Canvas.SetTop(valuelabel, yStart + height);
            result.Add(valuelabel);

            //Add the label
            string labelText = getLabel(scoreType, value);
            TextBlock label = new TextBlock();
            label.Text = labelText;
            label.FontStyle = FontStyles.Normal;
            label.FontWeight = FontWeights.Bold;
            label.Foreground = Brushes.White;
            label.TextWrapping = TextWrapping.WrapWithOverflow;
            label.VerticalAlignment = VerticalAlignment.Top;
            label.FontSize = 16;
            Canvas.SetLeft(label, xStart - 50);
            Canvas.SetTop(label, yStart + 0);
            result.Add(label);

            return result;

        }

        private static string getLabel(LetiScore scoreType, double value)
        {
            string result = "";
            if (value >= 0)
            {
                if (value >= scoreType.AAA)
                {
                    if (value >= scoreType.AA)
                    {
                        if (value >= scoreType.A)
                        {
                            if (value >= scoreType.B)
                            {
                                if (value >= scoreType.C)
                                {
                                    if (value >= scoreType.D)
                                    {
                                        if (value >= scoreType.E)
                                        {
                                            if (value >= scoreType.F)
                                            {
                                                if (value >= scoreType.G)
                                                {
                                                    result = "G";
                                                }
                                            }
                                            else
                                                result = "F";
                                        }
                                        else
                                            result = "E";
                                    }
                                    else
                                        result = "D";
                                }
                                else
                                    result = "C";
                            }
                            else
                                result = "B";
                        }
                        else
                            result = "A+";
                    }
                    else
                        result = "A++";
                }
                else
                    result = "A+++";
            }
            else
            result = "A+++";


            return result;
        }
    }


}
