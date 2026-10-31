using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Data;
using System.IO;
using System.Linq;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// Loading the A1-A3 tables behind the material editor's A1-A3 picker. A bad row or file
    /// used to throw while the picker was being created, which crashed the editor.
    /// </summary>
    public class A1A3TableTests
    {
        private const string Header = "Id,Name,Description,Category,Density,ECI_A1A3";

        private static string TempFolder()
        {
            string folder = Path.Combine(Path.GetTempPath(), "CarboA1A3Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return folder;
        }

        [Fact]
        public void Shipped_tables_load_without_errors()
        {
            A1A3Collection tables = new A1A3Collection();
            tables.LoadAll();

            Assert.Empty(tables.LoadErrors);
            Assert.Equal(717, tables.a1a3List.Single(l => l.Name == "ECI3").Elements.Count);
            Assert.Equal(71, tables.a1a3List.Single(l => l.Name == "IstructE").Elements.Count);
            Assert.Contains(tables.a1a3List, l => l.Name == "User");
        }

        [Fact]
        public void Bad_rows_are_skipped_and_reported_not_thrown()
        {
            string folder = TempFolder();

            try
            {
                File.WriteAllLines(Path.Combine(folder, "Hand.csv"), new[]
                {
                    Header,
                    "1,Steel beam,Hot rolled,Steel,7850,1.55",
                    //An unquoted comma in the description: one field too many.
                    "2,Steel plate,Hot rolled, recycled,Steel,7850,1.4",
                    //A blank line used to end the file here.
                    "",
                    "3,Timber,Sawn,Timber,470,0.26",
                    "4,,No name,Timber,470,0.3",
                    //Too large for an int: Convert.ToInt32 used to throw.
                    "99999999999,Glass,Float,Glass,2500,1.4",
                });

                A1A3Collection tables = new A1A3Collection();
                tables.LoadFrom(folder);

                A1A3List list = tables.a1a3List.Single();
                Assert.Equal(new[] { "Steel beam", "Timber", "Glass" }, list.Elements.Select(e => e.Name));
                Assert.Equal(0, list.Elements.Single(e => e.Name == "Glass").Id);
                Assert.Equal(2, tables.LoadErrors.Count);
                Assert.Contains(tables.LoadErrors, m => m.Contains("Steel plate"));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [Fact]
        public void A_file_with_too_few_columns_is_reported()
        {
            string folder = TempFolder();

            try
            {
                File.WriteAllLines(Path.Combine(folder, "Short.csv"), new[] { "Id,Name", "1,Steel" });
                File.WriteAllLines(Path.Combine(folder, "Good.csv"), new[] { Header, "1,Steel beam,,Steel,7850,1.55" });

                A1A3Collection tables = new A1A3Collection();
                tables.LoadFrom(folder);

                Assert.Equal("Good", tables.a1a3List.Single().Name);
                Assert.Single(tables.LoadErrors);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [Fact]
        public void A_missing_folder_is_reported_not_thrown()
        {
            A1A3Collection tables = new A1A3Collection();
            tables.LoadFrom(Path.Combine(Path.GetTempPath(), "NoSuchFolder_" + Guid.NewGuid().ToString("N")));

            Assert.Empty(tables.a1a3List);
            Assert.Single(tables.LoadErrors);
        }

        [Fact]
        public void LoadCSV_keeps_rows_after_a_blank_line_and_flags_extra_fields()
        {
            string folder = TempFolder();
            string path = Path.Combine(folder, "t.csv");

            try
            {
                File.WriteAllLines(path, new[] { "A,B", "1,2", "", "   ", "3,4,5", "6,7" });

                DataTable table = Utils.LoadCSV(path);

                Assert.Equal(3, table.Rows.Count);
                Assert.Equal("6", table.Rows[2][0]);
                Assert.Equal(new[] { 1 }, Utils.GetMalformedRows(table));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
