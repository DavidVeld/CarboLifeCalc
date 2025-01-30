using CarboLifeAPI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Windows;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Security.Cryptography;

namespace CarboLifeAPI
{
    public static class Utils
    {
        public static DataTable LoadCSV(string strFilePath)
        {
            //Call up a table
            DataTable dt = new DataTable("");

            //Variable used for headers
            int rowcount = 0;
            using (CsvFileReader reader = new CsvFileReader(strFilePath, Encoding.UTF8, true))
            {
                CsvRow row = new CsvRow();
                while (reader.ReadRow(row))
                {
                    List<string> RowData = new List<string>();

                    //Read a line to the list
                    foreach (string s in row)
                    {
                        RowData.Add(s);
                    }

                    if (rowcount == 0)
                    {
                        //this is a header
                        foreach (string header in RowData)
                        {
                            dt.Columns.Add(header);
                        }
                    }
                    else
                    {
                        //This is a DataRow
                        DataRow dr = dt.NewRow();

                        for (int i = 0; i < RowData.Count; i++)
                        {
                            dr[i] = RowData[i];
                        }

                        dt.Rows.Add(dr);
                    }
                    rowcount++;
                }
                return dt;
            }
        }

        public static double getScalingfactor(double canvasPixels, double Value)
        {
            //Based on y\ =\ 1-e^{-0.3x}
            double result = 1;

            if (Value <= 0)
                Value = 50;

            result = canvasPixels / Value;

            return result;
        }

        public static int CalcLevenshteinDistance(string a, string b)
        {
            if (String.IsNullOrEmpty(a) || String.IsNullOrEmpty(b)) return 999;

            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }

        public static string getAssemblyPath()
        {
            return PathUtils.getAssemblyPath();
            //string _path = Assembly.GetExecutingAssembly().Location;
            //string myPath = Path.GetDirectoryName(_path);
            //return myPath;
        }

        public static DataTable ToDataTables(CarboMaterial material)
        {
            DataTable table = new DataTable();
            PropertyInfo[] propertyValues = typeof(CarboMaterial).GetProperties();

            table.Columns.Add("Property");
            table.Columns.Add("Value");

            for (int i = 0; i < propertyValues.Length; i++)
            {
                PropertyInfo property = propertyValues[i];

                if (property.PropertyType != typeof(A1A3Element))
                {
                    table.Rows.Add(property.Name, property.GetValue(material));
                }

            }
            /*
            if (material.Properties.Count > 0)
            {
                foreach (CarboProperty cp in material.Properties)
                {
                    table.Rows.Add(cp.PropertyName, cp.Value);
                }
            }
            */
            return table;
        }

        public static DataTable ToDataTables<T>(List<T> data)
        {
            //FieldInfo[] fieldValues = typeof(T).GetFields();
            PropertyInfo[] propertyValues = typeof(T).GetProperties();

            DataTable table = new DataTable();
            try
            {
                for (int i = 0; i < propertyValues.Length; i++)
                {
                    PropertyInfo property = propertyValues[i];
                    table.Columns.Add(property.Name, property.PropertyType);
                }


                object[] values = new object[propertyValues.Length];
                foreach (T item in data)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = propertyValues[i].GetValue(item);
                    }
                    table.Rows.Add(values);
                }
            }
            catch
            {
                //MessageBox.Show(ex.Message);
            }
            return table;
        }

        public static double ConvertMeToDouble(string value)
        {
            double result = 0;

            try
            {
                //result = Convert.ToDouble(value);
                bool ok = double.TryParse(value, out result);
            }
            catch
            {
                result = 0;
                return result;
            }
            return result;

        }


        public static void CopyAll<T>(T source, T target)
        {
            var type = typeof(T);
            foreach (var sourceProperty in type.GetProperties())
            {
                var targetProperty = type.GetProperty(sourceProperty.Name);
                targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
            }
            foreach (var sourceField in type.GetFields())
            {
                var targetField = type.GetField(sourceField.Name);
                targetField.SetValue(target, sourceField.GetValue(source));
            }
        }

        public static double convertToCubicMtrs(double volumeCubicFt)
        {
            double result = 0;
            double factor = Math.Pow((0.3048), 3);
            result = volumeCubicFt * factor;
            return result;

        }
        public static double convertToSqreMtrs(double areaSqrFt)
        {
            double result = 0;
            double factor = Math.Pow((0.3048), 2);
            result = areaSqrFt * factor;
            return result;

        }

        public static bool isValidExpression(string correction)
        {
            string one = "1";
            try
            {
                string[] _operators = { "-", "+", "/", "*", "^" };

                bool okGo = false;

                if (correction.Length < 2)
                    return false;

                string first = correction.Substring(0, 1);
                foreach (string str in _operators)
                {
                    if (first == str)
                        okGo = true;
                }

                if (okGo == false)
                    return false;

                StringToFormula stf = new StringToFormula();
                double result = stf.Eval(one + correction);
                if (result == 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void MatchLogDelete()
        {
            string fileName = "db\\matchlog.txt";

            string myPath = Utils.getAssemblyPath() + "\\" + fileName;
            try 
            { 
               File.Delete(myPath); 
            } catch 
            { 
            }
        }

            public static void MatchLogWrite(string text)
        {
            
            string fileName = "db\\matchlog.txt";

            string myPath = Utils.getAssemblyPath() + "\\" + fileName;

            using (StreamWriter sw = File.AppendText(myPath))
            {
                sw.WriteLine(text);
            }
            
        }

        public static void WriteToLog(string text)
        {
            /*
            string fileName = "db\\log.txt";

            string myPath = Utils.getAssemblyPath() + "\\" + fileName;
            string timeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff");

            using (StreamWriter sw = File.AppendText(myPath))
            {
                sw.WriteLine(timeStamp + " :: " + text);
            }
            */
        }

        public static void Openlink(string text)
        {
            string link = text;
            if (link != "")
                if (link.StartsWith("http://") | link.StartsWith("https://"))
                    Process.Start(link);
        }

        public static System.Windows.Media.Color getColour(double max, double min, double value, string fieldName)
        {
            System.Windows.Media.Color result = new System.Windows.Media.Color();
            double range;
            double maxScale = 0;
            double minScale = 0;

            double byteRange = 254;

            byte dotcolour = 0;
            //See what Type applies:
            //Situation 1: All values are below 0
            // Situation 2: All values are above 0
            // Situation 3: Below and above, values are to be split:

            /*
             Colour scale in minusL
             RGB([0(min)-254(max)], 255, 25)

             Colour scale in plus
             RGB(255, [255(min)-0(max)], 25)
             */

            if (min < 0 && max <= 0)
            {
                //Situation 1: All values are below 0
                //min will be 0 max will be 254 (range);

                range = max - min;
                minScale = byteRange / range;
                double newColorValue = (value - max) * minScale * -1;
                newColorValue = verifyByte(newColorValue, byteRange);

                dotcolour = Convert.ToByte(newColorValue);

                //Second colour slider:
                byte secondColorValue = 255;

                //if(newColorValue < 150)
                secondColorValue = Convert.ToByte(255 - newColorValue * .5);
                dotcolour = Convert.ToByte(255 - newColorValue);

                result = System.Windows.Media.Color.FromRgb(dotcolour, secondColorValue, 25);

            }
            else if (max > 0 && min >= 0)
            {
                //Situation 2: All values are above 0
                //min will be 0 max will be 254 (range);

                range = max - min;
                maxScale = byteRange / range;
                double newColorValue = (value - min) * maxScale;

                newColorValue = verifyByte(newColorValue, byteRange);
                dotcolour = Convert.ToByte(255 - newColorValue);

                result = System.Windows.Media.Color.FromRgb(255, dotcolour, 25);

            }
            else
            {
                // Situation 3: Below and above, values are to be split:
                //Split;
                minScale = byteRange / min * -1;
                maxScale = byteRange / max;
                double newColorValue = 0;

                if (value < 0)
                {
                    newColorValue = (value * minScale) * -1;
                    newColorValue = verifyByte(newColorValue, byteRange);

                    //Second colour slider:
                    byte secondColorValue = 255;

                    //if(newColorValue < 150)
                    secondColorValue = Convert.ToByte(255 - newColorValue * .5);

                    dotcolour = Convert.ToByte(255 - newColorValue);
                    result = System.Windows.Media.Color.FromRgb(dotcolour, secondColorValue, 25);



                }
                else
                {
                    newColorValue = value * maxScale;
                    newColorValue = verifyByte(newColorValue, byteRange);

                    dotcolour = Convert.ToByte(255 - newColorValue);
                    result = System.Windows.Media.Color.FromRgb(255, dotcolour, 25);
                }

            }

            return result;
        }

        private static double verifyByte(double value, double byteRange)
        {
            if (value < 0)
                value = 0;
            else if (value > 255)
                value = byteRange;

            return value;
        }

        public static Color GetBlendedColor(double max, double min, double value, Color minRangeColour, Color midRangeColour, Color maxRangeColour)
        {
            try
            {
                //Normalize the range;
                if (min < 0)
                {
                    max = max + (min * -1);
                    value = value + (min * -1);
                    min = 0;
                }
                else if (min > 0)
                {
                    max = max - min;
                    value = value - min;
                    min = 0;
                }

                double total = max - min;

                /*
                if (total >= 0)
                    total = 1;
                */

                double x = value / total;
                x = 1 - x;
                Color myColor = GetBlendedColor(Convert.ToInt32(x * 100), minRangeColour, midRangeColour, maxRangeColour);

                //int f = 255;
                /*
                byte r = Convert.ToByte(verifyByte(2.0f * x, 254));
                byte g = Convert.ToByte(verifyByte(2.0f * (1 - x),254));
                byte b = 0;


                Color myColor = Color.FromRgb(r,g, b);
                */
                return myColor;
            }
            catch
            {
                return Color.FromArgb((int)Math.Round(0.0), (int)Math.Round(0.0), (int)Math.Round(0.0));
            }


        }

        public static System.Drawing.Color GetBlendedColor(int percentage, Color minRangeColour, Color midRangeColour, Color maxRangeColour)
        {
            
            if (percentage < 50)
                return Interpolate(maxRangeColour, midRangeColour, percentage / 50.0);
            return Interpolate(midRangeColour, minRangeColour, (percentage - 50) / 50.0);
            
            //OLD
            /*
            if (percentage < 50)
                return Interpolate(System.Drawing.Color.Red, System.Drawing.Color.Yellow, percentage / 50.0);
            return Interpolate(Color.Yellow, Color.Lime, (percentage - 50) / 50.0);
            */
        }

        private static System.Drawing.Color Interpolate(System.Drawing.Color color1, System.Drawing.Color color2, double fraction)
        {
            double r = Interpolate(color1.R, color2.R, fraction);
            double g = Interpolate(color1.G, color2.G, fraction);
            double b = Interpolate(color1.B, color2.B, fraction);
            return System.Drawing.Color.FromArgb((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
        }

        private static double Interpolate(double d1, double d2, double fraction)
        {
            return d1 + (d2 - d1) * fraction;
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// gets the value of a phase from the project totals
        /// </summary>
        /// <param name="pieceListLifePoint"></param>
        /// <param name="itemName">"A0"</param>
        /// <returns>the value as a string, or "not Calculated"</returns>
        internal static string getString(List<CarboDataPoint> pieceListLifePoint, string itemName)
        {
            string result = "";

            CarboDataPoint point = pieceListLifePoint.First(item => item.Name == itemName);

            if (point != null)
                result += Math.Round(point.Value / 1000, 2, MidpointRounding.AwayFromZero).ToString("N");
            else
                result += "Not Calculated ";

            return result;
        }

        /// <summary>
        /// Checks if a list is null or empty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(List<T> list)
        {
            if (list == null)
            {
                return true;
            }

            return !list.Any();
        }

        private static byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public static string Crypt(this string text)
        {
            try
            {
                if (text == null || text == "")
                    return "";

                SymmetricAlgorithm algorithm = DES.Create();
                ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
                byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                return Convert.ToBase64String(outputBuffer);
            }
            catch
            {
                return "";
            }
        }

        public static string Decrypt(this string text)
        {
            try
            {
                if (text == null || text == "")
                    return "";

                SymmetricAlgorithm algorithm = DES.Create();
                ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
                byte[] inputbuffer = Convert.FromBase64String(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);

                return Encoding.Unicode.GetString(outputBuffer);
            }
            catch {
                return "";
            }

        }

    }
}