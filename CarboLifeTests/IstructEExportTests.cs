using CarboLifeAPI;
using CarboLifeAPI.Data;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// The IStructE csv recalculated every group with CalculateTotals() and no arguments: every
    /// module on, no uncertainty, substructure in. It exported the wrong volume and left the
    /// open project holding it.
    /// </summary>
    public class IstructEExportTests
    {
        [Fact]
        public void Export_keeps_the_projects_own_calculation()
        {
            CarboProject project = TestProjects.ThreeElementProject();
            project.UncertFact = 0.1;
            project.CalculateProject();

            string csv = DataExportUtils.BuildIstructECsv(project);

            //3 m³ plus 10% uncertainty, in the project and in the file.
            Assert.Equal(3.3, project.getGroupList[0].TotalVolume, 9);
            Assert.Contains(",3.3", csv);
            Assert.Equal(6.6, project.ECTotal, 9);
        }
    }
}
