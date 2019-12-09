using CarboLifeAPI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

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
            using (CsvFileReader reader = new CsvFileReader(strFilePath))
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
            string _path = Assembly.GetExecutingAssembly().Location;
            string myPath = Path.GetDirectoryName(_path);
            return myPath;
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
                if (property.PropertyType != typeof(List<CarboProperty>))
                {
                    table.Rows.Add(property.Name, property.GetValue(material));
                }

            }
            if (material.Properties.Count > 0)
            {
                foreach (CarboProperty cp in material.Properties)
                {
                    table.Rows.Add(cp.PropertyName, cp.Value);
                }
            }

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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}