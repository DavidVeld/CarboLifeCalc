using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// Reading materials in from csv, and merging them into a library.
    /// </summary>
    public class MaterialImportTests
    {
        [Fact]
        public void Rows_without_an_id_all_get_their_own_id_and_none_is_lost()
        {
            //The generator used a new Random per call, which on .NET Framework is seeded from
            //the clock: five rows numbered in one go came out with the same id, and the merge,
            //which matches on id, wrote them over each other.
            CarboDatabase library = new CarboDatabase();
            library.AddMaterial(NamedMaterial("Existing A", 200000));
            library.AddMaterial(NamedMaterial("Existing B", 200005));

            CarboDatabase imported = new CarboDatabase();
            imported.CarboMaterialList.Clear();
            for (int i = 1; i <= 5; i++)
                imported.CarboMaterialList.Add(NamedMaterial("New " + i, 0));

            Assert.True(library.SyncCSVMaterials(imported, false));

            List<CarboMaterial> added = library.CarboMaterialList.Where(m => m.Name.StartsWith("New ")).ToList();

            Assert.Equal(5, added.Count);
            Assert.Equal(5, added.Select(m => m.Id).Distinct().Count());
            Assert.DoesNotContain(added, m => m.Id == 200000 || m.Id == 200005);
            Assert.All(added, m => Assert.InRange(m.Id, 200000, 299999));
        }

        [Fact]
        public void A_generated_id_never_repeats_an_id_already_in_the_incoming_rows()
        {
            CarboDatabase library = new CarboDatabase();

            CarboDatabase imported = new CarboDatabase();
            imported.CarboMaterialList.Clear();
            //200001 is not in the library yet, only in the file.
            imported.CarboMaterialList.Add(NamedMaterial("From file with id", 200001));
            imported.CarboMaterialList.Add(NamedMaterial("From file without id", 0));

            Assert.True(library.SyncCSVMaterials(imported, false));

            CarboMaterial withId = library.CarboMaterialList.Single(m => m.Name == "From file with id");
            CarboMaterial withoutId = library.CarboMaterialList.Single(m => m.Name == "From file without id");

            Assert.Equal(200001, withId.Id);
            Assert.NotEqual(withId.Id, withoutId.Id);
        }

        [Theory]
        [InlineData("en-GB")]
        [InlineData("nl-NL")]
        public void Density_is_read_as_a_decimal_number(string culture)
        {
            //It went through Convert.ToInt32, so 12.5 kg/m³ insulation came in as 12.
            CultureInfo original = Thread.CurrentThread.CurrentCulture;
            string path = Path.Combine(Path.GetTempPath(), "CarboLifeTests_" + Guid.NewGuid() + ".csv");

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

                //The header of the csv template the application ships, so this is the layout
                //users actually fill in.
                string header = File.ReadLines(Path.Combine(PathUtils.GetMaterialsDir(), "CSVTemplate.csv")).First();
                string row = "0,Mineral wool,Insulation,Test row,12.5,0,,,1.2,1.2,0,0,0,0,0,0,0,";
                File.WriteAllText(path, header + "\r\n" + row + "\r\n", new UTF8Encoding(true));

                int rowsRead;
                int rowsSkipped;
                List<CarboMaterial> materials = DataExportUtils.GetMaterialDatabaseFromCVSFile(path, out rowsRead, out rowsSkipped);

                Assert.Equal(1, rowsRead);
                Assert.Equal(0, rowsSkipped);
                Assert.Equal(12.5, materials.Single().Density);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private static CarboMaterial NamedMaterial(string name, int id)
        {
            CarboMaterial material = TestProjects.Material(name);
            material.Id = id;
            return material;
        }
    }
}
