using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// The shipped sample project, calculated and compared with the totals it gave when this
    /// test was written (September 2026, after the B6/B7, excluded element and material id fixes).
    ///
    /// Not a statement that these numbers are right: a tripwire. Any change to the calculation
    /// that moves them fails here, and then either the change is a mistake or it is a deliberate
    /// correction, in which case the values below are updated in the same commit and the change
    /// in the total goes in the release notes.
    ///
    /// The sample has 26 groups and 537 elements, uncertainty 10%, and B6/B7 switched off.
    /// </summary>
    public class SampleProjectSnapshotTests
    {
        /// <summary>tCO₂e. Well below anything the report shows, well above floating point noise.</summary>
        private const double TonnePrecision = 1e-6;

        /// <summary>kgCO₂e, for the phase totals.</summary>
        private const double KilogramPrecision = 1e-3;

        private static CarboProject LoadSample()
        {
            string path = Path.Combine(PathUtils.GetAssemblyDir(), "samples", "CarboLifeCalc_sample_project_R23.clcx");
            Assert.True(File.Exists(path), "Sample project not copied to the test output: " + path);

            CarboProject project = new CarboProject().DeSerializeXML(path);
            Assert.NotNull(project);

            project.CalculateProject();
            return project;
        }

        [Fact]
        public void Sample_project_totals_are_unchanged()
        {
            CarboProject project = LoadSample();

            Assert.Equal(26, project.getGroupList.Count);
            Assert.Equal(537, project.getGroupList.Sum(g => g.AllElements.Count));

            Assert.Equal(175.61293058453552, project.EC, TonnePrecision);
            Assert.Equal(197.61293058453552, project.ECTotal, TonnePrecision);

            //getTotalEC rounds the material part to two decimals (getTotalsGroup), so it is
            //compared to the same two decimals.
            Assert.Equal(197.61, project.getTotalEC(), 1e-9);
        }

        [Fact]
        public void Sample_project_phase_totals_are_unchanged()
        {
            CarboProject project = LoadSample();

            Dictionary<string, double> expected = new Dictionary<string, double>
            {
                { "A0(Global)", 11000 },
                { "A1-A3", 149109.13589877627 },
                { "A4", 19136.545528963667 },
                { "A5(Material)", 0 },
                { "A5(Global)", 11000 },
                { "B1-B7", 0 },
                { "Energy B6-B7 + D2 (Global)", 804.1 },
                { "C1-C4", 37761.566377685158 },
                { "C1(Global)", 0 },
                { "D", -10758.456108679187 },
                { "Sequestration", -30394.317220889592 },
                { "Additional", 0 },
            };

            Dictionary<string, double> actual = project.getPhaseTotals(true).ToDictionary(p => p.Name, p => p.Value);

            Assert.Equal(expected.Keys.OrderBy(k => k), actual.Keys.OrderBy(k => k));

            foreach (KeyValuePair<string, double> phase in expected)
                Assert.True(System.Math.Abs(phase.Value - actual[phase.Key]) < KilogramPrecision,
                    phase.Key + ": expected " + phase.Value + ", got " + actual[phase.Key]);
        }
    }
}
