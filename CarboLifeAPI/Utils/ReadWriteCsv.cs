using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI
{
    /// <summary>
    /// Class to store one CSV row
    /// </summary>
    public class CsvRow : List<string>
    {
        public string LineText { get; set; }
    }

    /// <summary>
    /// Class to write data to a CSV file
    /// </summary>
    public class CsvFileWriter : StreamWriter
    {
        public CsvFileWriter(Stream stream)
            : base(stream)
        {
        }

        public CsvFileWriter(string filename)
            : base(filename)
        {
        }

        /// <summary>
        /// Writes a single row to a CSV file.
        /// </summary>
        /// <param name="row">The row to be written</param>
        public void WriteRow(CsvRow row)
        {
            StringBuilder builder = new StringBuilder();
            bool firstColumn = true;
            foreach (string value in row)
            {
                // Add separator if this isn't the first value
                if (!firstColumn)
                    builder.Append(',');
                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);
                firstColumn = false;
            }
            row.LineText = builder.ToString();
            WriteLine(row.LineText);
        }
    }

    /// <summary>
    /// Class to read data from a CSV file
    /// </summary>
    public class CsvFileReader : StreamReader

    {
        /// <summary>
        /// The field separator. A comma is what everything in this application writes, but
        /// Excel saves a "csv" with the machine's list separator, which is a semicolon on every
        /// locale that uses a comma for the decimal point. A user who exports a file, edits it
        /// in Excel and saves it - the workflow the material import dialog asks for - hands
        /// back a semicolon file on half of Europe, and read as commas it is a single column
        /// per row, so every row failed and the import came back silently empty.
        /// </summary>
        public char Separator { get; set; }

        public CsvFileReader(Stream stream, Encoding encodeType, bool byteOrder)
            : base(stream)
        {
            Separator = ',';
        }

        public CsvFileReader(string filename, Encoding encodeType,bool byteOrder )
            : base(filename)
        {
            Separator = ',';
        }

        /// <summary>
        /// Picks the separator a line was written with: whichever of ',' and ';' appears more
        /// often outside quotes. A comma file with the odd semicolon in a description still
        /// reads as commas, and the other way round.
        /// </summary>
        public static char DetectSeparator(string headerLine)
        {
            if (string.IsNullOrEmpty(headerLine))
                return ',';

            int commas = 0;
            int semicolons = 0;
            bool inQuotes = false;

            for (int i = 0; i < headerLine.Length; i++)
            {
                char c = headerLine[i];

                if (c == '"')
                    inQuotes = !inQuotes;
                else if (inQuotes == false && c == ',')
                    commas++;
                else if (inQuotes == false && c == ';')
                    semicolons++;
            }

            return semicolons > commas ? ';' : ',';
        }


        /// <summary>
        /// Reads a row of data from a CSV file
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool ReadRow(CsvRow row)
        {
            

            row.LineText = ReadLine();
            if (String.IsNullOrEmpty(row.LineText))
                return false;

            int pos = 0;
            int rows = 0;

            while (pos < row.LineText.Length)
            {
                string value;

                // Special handling for quoted field
                if (row.LineText[pos] == '"')
                {
                    // Skip initial quote
                    pos++;

                    // Parse quoted value
                    int start = pos;
                    while (pos < row.LineText.Length)
                    {
                        // Test for quote character
                        if (row.LineText[pos] == '"')
                        {
                            // Found one
                            pos++;

                            // If two quotes together, keep one
                            // Otherwise, indicates end of value
                            if (pos >= row.LineText.Length || row.LineText[pos] != '"')
                            {
                                pos--;
                                break;
                            }
                        }
                        pos++;
                    }
                    value = row.LineText.Substring(start, pos - start);
                    value = value.Replace("\"\"", "\"");
                }
                else
                {
                    // Parse unquoted value
                    int start = pos;
                    while (pos < row.LineText.Length && row.LineText[pos] != Separator)
                        pos++;
                    value = row.LineText.Substring(start, pos - start);
                }

                // Add field to list
                if (rows < row.Count)
                    row[rows] = value;
                else
                    row.Add(value);
                rows++;

                // Eat up to and including next comma
                while (pos < row.LineText.Length && row.LineText[pos] != Separator)
                    pos++;
                if (pos < row.LineText.Length)
                    pos++;
            }
            // Delete any unused items
            while (row.Count > rows)
                row.RemoveAt(rows);

            // Return true if any columns read
            return (row.Count > 0);
        }
    }
}
