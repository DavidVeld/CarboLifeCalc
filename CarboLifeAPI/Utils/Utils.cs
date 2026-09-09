using CarboLifeAPI.Data;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

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
            return PathUtils.GetAssemblyDir();
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

        /// <summary>
        /// Reads a number the user typed, and says whether it could be read at all.
        ///
        /// Prefer this over ConvertMeToDouble wherever the difference between "the user cleared
        /// the box" and "the user typed zero" matters, which is most places: a cleared area or
        /// factor should keep its previous value, not silently become 0.
        ///
        /// The old implementation replaced every comma with a dot and then parsed as invariant,
        /// so an en-GB user typing "1,234" got 1.234 - out by a factor of a thousand, with no
        /// warning. The user's own culture is tried first, then invariant for pasted or stored
        /// values, and only a single lone comma is treated as a decimal point.
        /// </summary>
        /// <returns>True when the text held a usable number.</returns>
        public static bool TryConvertToDouble(string value, out double result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            string text = value.Trim();

            //A single separator has to be resolved BEFORE handing the text to a culture, because
            //.NET is lenient about group sizes: on en-GB "3,4" parses happily as 34, and on de-DE
            //"3.4" parses as 34, either of which is a silent factor of ten.
            //
            //A real thousands group always has exactly three digits after it. One separator with
            //anything other than three digits behind it therefore cannot be a group separator, so
            //it is a decimal point whatever the local convention says. Exactly three digits
            //("1,234") is genuinely ambiguous - 1234 in Britain, 1.234 in Germany - and there the
            //user's own culture is the best answer available.
            char decimalPoint;

            if (LooksLikeLoneDecimalSeparator(text, out decimalPoint))
            {
                string normalised = text.Replace(decimalPoint, '.');

                //Float, not Any: no thousands separators are permitted in this reading.
                if (double.TryParse(normalised, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                    return true;
            }

            //What the user's keyboard and locale produce.
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
                return true;

            //Values that came from a file, a paste or an invariant ToString.
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return true;

            result = 0;
            return false;
        }

        /// <summary>
        /// True when the text holds exactly one '.' or ',' that cannot be a thousands separator,
        /// and is therefore the decimal point regardless of culture. See TryConvertToDouble.
        /// </summary>
        private static bool LooksLikeLoneDecimalSeparator(string text, out char separator)
        {
            separator = '.';

            int dots = 0;
            int commas = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '.') dots++;
                else if (text[i] == ',') commas++;
            }

            //Both present, or more than one of either: leave it to the cultures.
            if (dots + commas != 1)
                return false;

            separator = dots == 1 ? '.' : ',';

            int at = text.IndexOf(separator);

            //Count the digits after it; anything non-digit and this is not a plain number anyway.
            int digitsAfter = 0;

            for (int i = at + 1; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]) == false)
                    return false;

                digitsAfter++;
            }

            //Exactly three digits is a legitimate thousands group, so it stays ambiguous.
            return digitsAfter != 3;
        }

        /// <summary>
        /// Reads a number the user typed, returning 0 when it cannot be read.
        ///
        /// Kept because around 200 call sites use it. It cannot tell a failure from a real zero,
        /// so anywhere that distinction matters should call TryConvertToDouble instead.
        /// </summary>
        public static double ConvertMeToDouble(string value)
        {
            double result;
            TryConvertToDouble(value, out result);
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

        /// <summary>
        /// Clears the buffered match log. The old implementation deleted a file inside the
        /// install directory, the buffer lives in memory now so there is nothing to delete.
        /// </summary>
        public static void MatchLogDelete()
        {
            CarboMatchDiagnostics.Reset();
        }

        /// <summary>
        /// Appends one line to the buffered match log.
        /// This used to open, append and close a StreamWriter once per database material per
        /// lookup, with no try/catch, which is roughly 121,000 file operations for a 200 group
        /// import against Okobaudat and an uncaught UnauthorizedAccessException on a read only
        /// install. It is now off by default (CarboMatchDiagnostics.Enabled), buffered in
        /// memory, and written exactly once by MatchLogFlush inside a try/catch.
        /// </summary>
        public static void MatchLogWrite(string text)
        {
            CarboMatchDiagnostics.Record(text);
        }

        /// <summary>
        /// Writes the buffered match log in one operation and empties the buffer.
        /// Safe to call always, a no-op when logging is disabled, never throws.
        /// </summary>
        public static void MatchLogFlush()
        {
            CarboMatchDiagnostics.Flush();
        }
        [Obsolete]
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
            catch 
            {
                return "";
            }

        }

        public static List<CarboMapElement> GenerateMappinglist(CarboProject carboProject)
        {
            var mappingList = new List<CarboMapElement>();

            // Defensive check
            if (carboProject == null)
            {
                //Console.WriteLine("Error: carboProject is null.");
                return mappingList;
            }

            try
            {
                string templateName = carboProject?.CarboDatabase?.templateName ?? string.Empty;

                // Track unique combinations of revitName, carboNAME, and category
                var uniqueKeys = new HashSet<(string revitName, string carboNAME, string category, string templateName)>();

                foreach (CarboGroup cg in carboProject.getGroupList)
                {
                    if (cg.AllElements == null || cg.AllElements.Count == 0)
                        continue;

                    string revitName = cg.AllElements[0].MaterialName;
                    string carboNAME = cg.MaterialName;
                    string category = cg.Category;

                    var key = (revitName, carboNAME, category, templateName);

                    if (!uniqueKeys.Contains(key))
                    {
                        var mapElement = new CarboMapElement
                        {
                            revitName = revitName,
                            carboNAME = carboNAME,
                            category = category,
                            templateName = templateName

                        };

                        mappingList.Add(mapElement);
                        uniqueKeys.Add(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while generating mapping list: {ex.Message}");
            }

            return mappingList;
        }

        /// <summary>
        /// Opends a file dialog to select a Carbo Life Project file (.clcx)
        /// </summary>
        /// <returns>The filepath if valid or "" if not</returns>
        public static string OpenCarboProject(string pathForViewing = "")
        {
            string path = "";
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Project File (*.clcx)|*.clcx";

                if(Directory.Exists(pathForViewing))
                    openFileDialog.InitialDirectory = pathForViewing;

                var ok = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName) && openFileDialog.FileName.EndsWith("clcx", StringComparison.OrdinalIgnoreCase))
                {
                    //Readable, not writable: opening a project only reads it, and asking for write
                    //access turned a file on a read-only share into "it could not be found".
                    if(DataExportUtils.IsFileReadable(openFileDialog.FileName) == true)
                    {
                        path = openFileDialog.FileName;
                        return path;
                    }
                    else
                    {
                        MessageBox.Show("The selected Carbo Life Project file could not be read. It may be open in another program, or you may not have permission to read it.", "File Could Not Be Read", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return "";
                    }
                }
            }
            catch
            {
                MessageBox.Show("There was an error opening the file, it could not be found, or is of the wrong format", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return "";
            }

            return "";
        }

        /// <summary>
        /// Opends a file dialog to select a Carbo Life Material Library file (.clcx)
        /// </summary>
        /// <returns>The filepath if valid or "" if not</returns>
        /// <param name="pathForViewing">The folder the dialog opens in</param>
        /// <param name="includeCsv">
        /// Offer .csv material tables as well. The template lists build from both extensions and
        /// CarboDatabase.LoadTemplate reads both, so a caller that lists a .csv database should
        /// let the user pick one. See IsMaterialLibraryFile below.
        /// </param>
        public static string OpenCarboMaterialLibrary(string pathForViewing = "", bool includeCsv = false)
        {
            string path = "";
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = includeCsv
                    ? "Carbo Life Material File (*.cxml;*.csv)|*.cxml;*.csv"
                    : "Carbo Life Material File (*.cxml)|*.cxml";

                if (Directory.Exists(pathForViewing))
                    openFileDialog.InitialDirectory = pathForViewing;

                var ok = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName) && IsMaterialLibraryFile(openFileDialog.FileName, includeCsv))
                {
                    //Readable, not writable: a material database is only ever read, and a company
                    //database on a share the user may read but not write - the whole point of the
                    //Change button in the import settings - failed the write test and was reported
                    //as missing or malformed.
                    if (DataExportUtils.IsFileReadable(openFileDialog.FileName) == true)
                    {
                        path = openFileDialog.FileName;
                        return path;
                    }
                    else
                    {
                        MessageBox.Show("The selected Carbo Life Material file could not be read. It may be open in another program, or you may not have permission to read it.", "File Could Not Be Read", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return "";
                    }
                }
            }
            catch
            {
                MessageBox.Show("There was an error opening the file, it could not be found, or is of the wrong format", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return "";
            }

            return "";
        }

        /// <summary>
        /// True when a picked file is one the material databases are read from.
        /// Case insensitive: a .CXML file off a share was silently rejected before.
        /// </summary>
        private static bool IsMaterialLibraryFile(string fileName, bool includeCsv)
        {
            if (fileName.EndsWith("cxml", StringComparison.OrdinalIgnoreCase))
                return true;

            return includeCsv && fileName.EndsWith("csv", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Opens a file dialog to select a Carbo Life material mapping file (.xml)
        /// </summary>
        /// <returns>The filepath if valid or "" if not</returns>
        public static string OpenCarboMappingLibrary(string pathForViewing = "")
        {
            string path = "";
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Carbo Life Mapping File (*.xml)|*.xml";

                if (Directory.Exists(pathForViewing))
                    openFileDialog.InitialDirectory = pathForViewing;

                var ok = openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "" && File.Exists(openFileDialog.FileName) && openFileDialog.FileName.EndsWith("xml", StringComparison.OrdinalIgnoreCase))
                {
                    //Readable, not writable: the mapping file is read to match materials with. It
                    //is written back to as well, but a read-only shared mapping file is still worth
                    //importing against, and SaveToXml says clearly when it cannot write.
                    if (DataExportUtils.IsFileReadable(openFileDialog.FileName) == false)
                    {
                        MessageBox.Show("The selected Carbo Life Mapping file could not be read. It may be open in another program, or you may not have permission to read it.", "File Could Not Be Read", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return "";
                    }

                    //.xml is not enough to go on. The mapping file lives in db\settings next to
                    //CarboSettings.xml, and Export Settings writes more .xml files of its own, so
                    //the folder this dialog opens in is full of xml that is not a mapping table.
                    //Picking one of those pointed the import at a file with no matches in it, and
                    //nothing said so - the material mapping simply stopped working.
                    if (IsCarboMappingFile(openFileDialog.FileName) == false)
                    {
                        MessageBox.Show("The selected file is not a Carbo Life material mapping file." +
                                        Environment.NewLine + Environment.NewLine +
                                        openFileDialog.FileName + Environment.NewLine + Environment.NewLine +
                                        "A mapping file holds the list of Revit material names and the Carbo Life materials they were matched to. Settings files and other xml files cannot be used here.",
                                        "Not A Mapping File", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return "";
                    }

                    path = openFileDialog.FileName;
                    return path;
                }
            }
            catch
            {
                MessageBox.Show("There was an error opening the file, it could not be found, or is of the wrong format", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return "";
            }

            return "";
        }

        /// <summary>
        /// True when a file really is a material mapping file, judged by what is in it rather
        /// than by its extension.
        ///
        /// The root element is what tells them apart: a mapping file is a serialised
        /// CarboMapFile, a settings file a serialised CarboSettings, and both are .xml sitting in
        /// the same folder. Read as a stream and stopped at the root element, so a large mapping
        /// file costs nothing to check and a file that is not xml at all fails quietly.
        ///
        /// An empty mapping table is accepted: a mapping file that has not matched anything yet
        /// is a legitimate starting point, and the import fills it in.
        /// </summary>
        private static bool IsCarboMappingFile(string fileName)
        {
            try
            {
                System.Xml.XmlReaderSettings readerSettings = new System.Xml.XmlReaderSettings();

                //A mapping file needs neither a DTD nor an external entity, and this one may have
                //come off a share or out of an email. Both are a way for such a file to reach
                //back into the local file system, so neither is allowed here.
                readerSettings.DtdProcessing = System.Xml.DtdProcessing.Prohibit;
                readerSettings.XmlResolver = null;

                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stream, readerSettings))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType != System.Xml.XmlNodeType.Element)
                            continue;

                        //The first element is the root; whatever it is, that settles it.
                        return string.Equals(reader.Name, "CarboMapFile", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception)
            {
                //Unreadable or not xml. Either way it is not a mapping file we can use.
            }

            return false;
        }
    }
}