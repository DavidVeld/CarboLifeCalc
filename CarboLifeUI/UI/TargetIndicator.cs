using Autodesk.Revit.DB.Electrical;
using CarboLifeAPI;
using CarboLifeAPI.Data.Superseded;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;


namespace CarboLifeUI
{
    internal class TargetIndicator
    {
        static List<int> labelYvalues = new List<int>();
        static readonly SolidColorBrush almostBlack = (SolidColorBrush)(new BrushConverter().ConvertFrom("#050505"));

        internal static IEnumerable<UIElement> generateImage(Canvas myCanvas, double CarbonA15, double CarbonA1C, double GIA, string buildingType)
        {
            IList<UIElement> result = new List<UIElement>();
            labelYvalues = new List<int>();

            double canvasWidth = myCanvas.ActualWidth;
            double canvasHeight = myCanvas.ActualHeight;

            double XOffset = canvasWidth / 10;
            double YOffset = 0;
            double YCurrent = YOffset;

            //Check if the SCO2RES rating needs to be calculater for two values.
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

            //try to find matching score, if not found only one arrow is used.
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

            //Get Target Line
            IEnumerable<UIElement> targetLine = getTarget(scoreTypeA15.Target, scoreTypeA15);



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
            foreach (UIElement uielement in targetLine)
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
                    string Benchmark = dr[1].ToString();

                    string sequestrationText = dr[2].ToString();
                    string isStructureText = dr[3].ToString();


                    Int32 AAA = (int)Utils.ConvertMeToDouble(dr[4].ToString());
                    Int32 AA = (int)Utils.ConvertMeToDouble(dr[5].ToString());
                    Int32 A = (int)Utils.ConvertMeToDouble(dr[6].ToString());
                    Int32 B = (int)Utils.ConvertMeToDouble(dr[7].ToString());
                    Int32 C = (int)Utils.ConvertMeToDouble(dr[8].ToString());
                    Int32 D = (int)Utils.ConvertMeToDouble(dr[9].ToString());
                    Int32 E = (int)Utils.ConvertMeToDouble(dr[10].ToString());
                    Int32 F = (int)Utils.ConvertMeToDouble(dr[11].ToString());
                    Int32 G = (int)Utils.ConvertMeToDouble(dr[12].ToString());

                    Int32 T = (int)Utils.ConvertMeToDouble(dr[13].ToString());


                    scors.BuildingType = BuildingType;
                    scors.TargetType = Benchmark;

                    if (sequestrationText == "TRUE")
                        scors.sequestration = true;
                    else
                        scors.sequestration = false;

                    if (isStructureText == "TRUE")
                        scors.isStructure = true;
                    else
                        scors.isStructure = false;

                    scors.AAA = AAA;
                    scors.AA = AA;
                    scors.A = A;
                    scors.B = B;
                    scors.C = C;
                    scors.D = D;
                    scors.E = E;
                    scors.F = F;
                    scors.G = G;
                    scors.Target = T;

                    result.Add(scors);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("File: " + myPath + " could not be found, make sure you have the profile list located in indicated folder");
            }

            return result;
        }

        private static IEnumerable<UIElement> getTarget(int Target, LetiScore scoreRange)
        {

            double height = 25;
            double spacing = 3;

            double yStart = 75;
            double xStart = 75;
            //string rating = "";

            double yValue = getYOnCanvas(scoreRange, Target);
            yStart = yValue + (height / 2);

            IList<UIElement> result = new List<UIElement>();

            //Add dashed Leti line
            Line line = new Line();
            line.Stroke = Brushes.Gray;

            line.X1 = xStart;
            line.Y1 = yStart;

            line.X2 = xStart + 200;
            //line.Y2 = yStart - (spacing / 2);
            line.Y2 = yStart;

            line.StrokeThickness = 1;
            line.StrokeDashArray.Add(10);
            line.StrokeDashArray.Add(10);
            line.StrokeDashArray.Add(10);
            line.StrokeDashArray.Add(10);
            result.Add(line);

            //Add the rating text
            TextBlock ratingBlock = new TextBlock();
            ratingBlock.Text = "Target: " + Target + " kgCo2e/m2";
            ratingBlock.FontStyle = FontStyles.Normal;
            ratingBlock.FontWeight = FontWeights.Normal;
            ratingBlock.Foreground = almostBlack;
            ratingBlock.TextWrapping = TextWrapping.WrapWithOverflow;
            ratingBlock.VerticalAlignment = VerticalAlignment.Top;
            ratingBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

            ratingBlock.FontSize = 10;

            Canvas.SetLeft(ratingBlock, xStart + 25);
            Canvas.SetTop(ratingBlock, yStart); // + (height / 4) - 5);//

            result.Add(ratingBlock);
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
            //string rating = "";

            switch (letter)
            {
                case "A++":
                    xScale = 1;
                    colour = "#048337";
                    yStart = (1 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));
                    break;
                case "A+":
                    xScale = 1.1;
                    colour = "#45AA50";
                    yStart = (2 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));

                    break;
                case "A":
                    xScale = 1.2;
                    colour = "#A8C812";
                    yStart = (3 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));

                    break;
                case "B":
                    xScale = 1.3;
                    colour = "#F3E72C";
                    yStart = (4 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));

                    //rating = "2030 Design Target";
                    break;
                case "C":
                    xScale = 1.4;
                    colour = "#FECC46";
                    yStart = (5 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));

                    break;
                case "D":
                    xScale = 1.5;
                    colour = "#EF7C1A";
                    yStart = (6 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));

                    //rating = "Good";
                    break;
                case "E":
                    xScale = 1.6;
                    colour = "#E52320";
                    yStart = (7 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));

                    break;
                case "F":
                    xScale = 1.7;
                    colour = "#D10913";
                    yStart = (8 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));

                    //rating = "Average";
                    break;
                case "G":
                    xScale = 1.8;
                    colour = "#A81916";
                    yStart = (9 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));

                    break;
                default:
                    xScale = 1.9;
                    colour = "#A81916";
                    yStart = (10 * (height + spacing));
                    labelYvalues.Add(Convert.ToInt16(yStart + (height / 2)));

                    break;
            }

            width = xScale * width;


            IList<UIElement> result = new List<UIElement>();

            SolidColorBrush mySolidColorBrushSupport = (SolidColorBrush)(new BrushConverter().ConvertFrom(colour));

            Polyline arrow = new Polyline();
            arrow.Visibility = System.Windows.Visibility.Visible;
            arrow.StrokeThickness = 1;
            arrow.Stroke = almostBlack;
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
            label.Foreground = almostBlack;
            label.TextWrapping = TextWrapping.WrapWithOverflow;
            label.VerticalAlignment = VerticalAlignment.Top;
            label.FontSize = 16;

            Canvas.SetLeft(label, xStart + 5);
            Canvas.SetTop(label, yStart + (height / 4) - 5);
            result.Add(label);

            /*
            if (letter == "F" || letter == "D" || letter == "B")
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
                ratingBlock.Foreground = almostBlack;
                ratingBlock.TextWrapping = TextWrapping.WrapWithOverflow;
                ratingBlock.VerticalAlignment = VerticalAlignment.Top;
                ratingBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                ratingBlock.FontSize = 10;

                Canvas.SetLeft(ratingBlock, xStart + 25);
                Canvas.SetTop(ratingBlock, yStart + (height / 4) - 5);
                result.Add(ratingBlock);

            }
            */

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
            //this below corrects the legend in case a scores rating is drawn where scoreTypeA15 and A1C are equal
            if (scoreTypeA15 == scoreTypeA1C)
            {
                label.Text = scoreTypeA15.BuildingType + " " + scoreTypeA15.TargetType  + Environment.NewLine + "A1-A5 " + "kgCO₂e/m²" ;
            }
            else
            { 
                label.Text = scoreTypeA15.BuildingType + " " + scoreTypeA15.TargetType + Environment.NewLine + "A1-A5 / A1-C " + "kgCO₂e/m²";
            }

            label.FontStyle = FontStyles.Normal;
            label.FontWeight = FontWeights.Normal;
            label.Foreground = almostBlack;
            label.TextWrapping = TextWrapping.WrapWithOverflow;
            label.VerticalAlignment = VerticalAlignment.Top;
            label.FontSize = 12;

            Canvas.SetLeft(label, xStart);
            Canvas.SetTop(label, -5);
            result.Add(label);

            //LOWEST:"A++"
            double y = getYOnCanvas(scoreTypeA15, scoreTypeA15.AAA);

            TextBlock labelAAA = new TextBlock();
            labelAAA.Text = scoreTypeA15.AAA.ToString() + "/" + scoreTypeA1C.AAA.ToString();
            labelAAA.FontStyle = FontStyles.Normal;
            labelAAA.FontWeight = FontWeights.Normal;
            labelAAA.Foreground = almostBlack;
            labelAAA.TextWrapping = TextWrapping.WrapWithOverflow;
            labelAAA.VerticalAlignment = VerticalAlignment.Top;
            labelAAA.FontSize = 12;
            Canvas.SetLeft(labelAAA, xStart);
            //Canvas.SetTop(labelAAA, (scoreTypeA15.AAA * valueper1) + (height / 2));
            Canvas.SetTop(labelAAA, 40);

            result.Add(labelAAA);


            //"A"
            y = getYOnCanvas(scoreTypeA15, scoreTypeA15.A);

            TextBlock labelA = new TextBlock();
            labelA.Text = scoreTypeA15.A.ToString() + "/" + scoreTypeA1C.A.ToString();
            labelA.FontStyle = FontStyles.Normal;
            labelA.FontWeight = FontWeights.Normal;
            labelA.Foreground = almostBlack;
            labelA.TextWrapping = TextWrapping.WrapWithOverflow;
            labelA.VerticalAlignment = VerticalAlignment.Top;
            labelA.FontSize = 12;
            Canvas.SetLeft(labelA, xStart);
            //Canvas.SetTop(labelA, (scoreTypeA15.A * valueper1) + (height / 2));
            Canvas.SetTop(labelA, y);

            result.Add(labelA);

            y = getYOnCanvas(scoreTypeA15, scoreTypeA15.C);

            //"C"
            TextBlock labelC = new TextBlock();
            labelC.Text = scoreTypeA15.C.ToString() + "/" + scoreTypeA1C.C.ToString();
            labelC.FontStyle = FontStyles.Normal;
            labelC.FontWeight = FontWeights.Normal;
            labelC.Foreground = almostBlack;
            labelC.TextWrapping = TextWrapping.WrapWithOverflow;
            labelC.VerticalAlignment = VerticalAlignment.Top;
            labelC.FontSize = 12;
            Canvas.SetLeft(labelC, xStart);
            //Canvas.SetTop(labelC, (scoreTypeA15.C * valueper1) + (height / 2));
            Canvas.SetTop(labelC, y);

            result.Add(labelC);

            //"E"
            y = getYOnCanvas(scoreTypeA15, scoreTypeA15.E);

            TextBlock labelE = new TextBlock();
            labelE.Text = scoreTypeA15.E.ToString() + "/" + scoreTypeA1C.E.ToString();
            labelE.FontStyle = FontStyles.Normal;
            labelE.FontWeight = FontWeights.Normal;
            labelE.Foreground = almostBlack;
            labelE.TextWrapping = TextWrapping.WrapWithOverflow;
            labelE.VerticalAlignment = VerticalAlignment.Top;
            labelE.FontSize = 12;
            labelE.Foreground = almostBlack;
            Canvas.SetLeft(labelE, xStart);
            //Canvas.SetTop(labelE, (scoreTypeA15.E * valueper1) + (height / 2));
            Canvas.SetTop(labelE, y);

            result.Add(labelE);

            //"G"
            y = getYOnCanvas(scoreTypeA15, scoreTypeA15.G);

            TextBlock labelG = new TextBlock();
            labelG.Text = ">" + scoreTypeA15.G.ToString() + "/" + scoreTypeA1C.G.ToString();
            labelG.FontStyle = FontStyles.Normal;
            labelG.FontWeight = FontWeights.Normal;
            labelG.TextWrapping = TextWrapping.WrapWithOverflow;
            labelG.VerticalAlignment = VerticalAlignment.Top;
            labelG.FontSize = 12;
            labelG.Foreground = almostBlack;
            Canvas.SetLeft(labelG, xStart);
            //Canvas.SetTop(labelG, (scoreTypeA15.G * valueper1) + (height / 2));
            Canvas.SetTop(labelG, y);

            result.Add(labelG);

            return result;

        }

        private static double getYOnCanvas(LetiScore scoreType, double value)
        {
            double result = 0;
            //Get the score value;
            string labelText = getLabelV2(scoreType, value);

            //Get the y locations on the image:
            List<int> twoValues = getTopAndBottomYs(labelText);

            //add the absolute values to index 2 and 3 from the scores list
            twoValues.Add(scoreType.getValue(labelText));
            twoValues.Add(scoreType.getValueOneUp(labelText));

            //this is a check to find all the points
            if (twoValues != null && twoValues.Count == 4)
            {
                //Get the Centre location of the arrow based on the two value:

                double canvasdelta = (twoValues[0] - twoValues[1]);
                double letiDelta = (twoValues[2] - twoValues[3]);
                double yscale = 0;

                try
                {
                    yscale = canvasdelta / letiDelta;
                    if (Double.IsNaN(yscale))
                        yscale = 0;
                }
                catch
                {
                    yscale = 0;
                }

                //now the 
                double valueToGoUp = twoValues[2] - value;
                double canvasValue = valueToGoUp * yscale;
                result = twoValues[0] - canvasValue;

            }
            return result;
        }

        private static IEnumerable<UIElement> generateIndicatorArrow(LetiScore scoreType, double value, double xoffset)
        {
            IList<UIElement> result = new List<UIElement>();


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

            double yStart = (value * valueper1) - (height / 2);

            //Get the score value;
            string labelText = getLabelV2(scoreType, value);

            //Get the y locations on the image:
            List<int> twoValues = getTopAndBottomYs(labelText);

            //add the absolute values to index 2 and 3 from the scores list
            twoValues.Add(scoreType.getValue(labelText));
            twoValues.Add(scoreType.getValueOneUp(labelText));


            //this is a check to find all the points
            if (twoValues != null && twoValues.Count == 4)
            {
                //Get the Centre location of the arrow based on the two value:

                double canvasdelta = (twoValues[0] - twoValues[1]);
                double letiDelta = (twoValues[2] - twoValues[3]);
                double yscale = 0;

                try
                {
                    yscale = canvasdelta / letiDelta;
                }
                catch
                {
                    yscale = 1;
                }

                //now the 
                double valueToGoUp = twoValues[2] - value;
                double canvasValue = valueToGoUp * yscale;
                yStart = twoValues[0] - canvasValue;

            }

            //Check results this sets the boundaries for the graph;
            if (Double.IsNaN(yStart))
                yStart = getYOnCanvas(scoreType, scoreType.AAA) - (height / 2);

            if (value < 0)
                yStart = 0;
            if (value > scoreType.G)
                yStart = getYOnCanvas(scoreType, scoreType.G);


            Polyline arrow = new Polyline();
            arrow.Visibility = System.Windows.Visibility.Visible;
            arrow.StrokeThickness = 1;
            arrow.Stroke = almostBlack;
            arrow.Fill = almostBlack;

            PointCollection connecitonPoints = new PointCollection();

            yStart = yStart + (height /2 );

            Point pnt1 = new Point(xStart, yStart + height / 2); //Top Right
            Point pnt2 = new Point(xStart - width, yStart + height / 2);
            Point pnt3 = new Point(xStart - width - 10, yStart); // the point
            Point pnt4 = new Point(xStart - width, yStart - height / 2);

            Point pnt5 = new Point(xStart, yStart - height / 2);
            Point pnt6 = new Point(xStart, yStart + height / 2);

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
            valuelabel.Foreground = almostBlack;
            valuelabel.TextWrapping = TextWrapping.WrapWithOverflow;
            valuelabel.VerticalAlignment = VerticalAlignment.Top;
            valuelabel.FontSize = 14;
            Canvas.SetLeft(valuelabel, xStart - 80);
            Canvas.SetTop(valuelabel, yStart + height /2);
            result.Add(valuelabel);

            if (labelText == "AAA")
                labelText = "A++";
            else if (labelText == "AA")
                labelText = "A+";

            //Add the label
            TextBlock label = new TextBlock();
            label.Text = labelText;
            label.FontStyle = FontStyles.Normal;
            label.FontWeight = FontWeights.Bold;
            label.Foreground = Brushes.White;
            label.TextWrapping = TextWrapping.WrapWithOverflow;
            label.VerticalAlignment = VerticalAlignment.Top;
            label.FontSize = 16;
            Canvas.SetLeft(label, xStart - 50);
            Canvas.SetTop(label, yStart - height / 2);
            result.Add(label);

            return result;

        }

        private static List<int> getTopAndBottomYs(string labelText)
        {
            List<int> result = new List<int>();
            if (labelYvalues == null & labelYvalues.Count != 9)
                return null;

            switch (labelText)
            {
                case "AAA": // index 0
                    result.Add(labelYvalues[0]);
                    result.Add(labelYvalues[0]);
                    //The Values
                    break;
                case "AA": // index 1
                    result.Add(labelYvalues[1]);
                    result.Add(labelYvalues[0]);
                    break;
                case "A": // index 2
                    result.Add(labelYvalues[2]);
                    result.Add(labelYvalues[1]);
                    break;
                case "B": // index 3
                    result.Add(labelYvalues[3]);
                    result.Add(labelYvalues[2]);
                    break;
                case "C": // index 4
                    result.Add(labelYvalues[4]);
                    result.Add(labelYvalues[3]);
                    break;
                case "D": // index 5
                    result.Add(labelYvalues[5]);
                    result.Add(labelYvalues[4]);
                    break;
                case "E": // index 6
                    result.Add(labelYvalues[6]);
                    result.Add(labelYvalues[5]);
                    break;
                case "F": // index 7
                    result.Add(labelYvalues[7]);
                    result.Add(labelYvalues[6]);
                    break;
                case "G": // index 8
                    result.Add(labelYvalues[8]);
                    result.Add(labelYvalues[7]);
                    break;
                default: // index 9
                    result.Add(labelYvalues[8]);
                    result.Add(labelYvalues[8]);
                    break;


            }
            return result;
        }

        private static string getLabelV2(LetiScore scoreType, double value)
        {
            if (value <= scoreType.AAA) return "AAA";
            if (value <= scoreType.AA) return "AA";
            if (value <= scoreType.A) return "A";
            if (value <= scoreType.B) return "B";
            if (value <= scoreType.C) return "C";
            if (value <= scoreType.D) return "D";
            if (value <= scoreType.E) return "E";
            if (value <= scoreType.F) return "F";
            return "G";
        }
    }


}
