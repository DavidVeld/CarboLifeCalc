using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.Linq;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// Elements left out of the calculation, by being unticked or by being substructure while
    /// substructure is off. They used to keep the carbon of the last pass they were counted in,
    /// so the element figures - heat map, Revit write-back, per element exports - did not add up
    /// to the group total.
    /// </summary>
    public class ExcludedElementTests
    {
        private const double Precision = 1e-9;

        [Fact]
        public void Excluded_element_carries_no_carbon_and_elements_add_up_to_the_group()
        {
            CarboProject project = TestProjects.ThreeElementProject();
            project.CalculateProject();

            CarboGroup group = project.getGroupList.Single();
            Assert.Equal(6, group.EC, Precision);

            //Counted once, then left out: the stale value is exactly what used to survive.
            CarboElement excluded = group.AllElements.Single(e => e.Id == 2);
            excluded.includeInCalc = false;
            project.CalculateProject();

            Assert.Equal(4, group.EC, Precision);
            Assert.Equal(0, excluded.EC, Precision);
            Assert.Equal(0, excluded.Mass, Precision);
            Assert.Equal(0, excluded.Volume_Total, Precision);

            //Element carbon is in kg, group carbon in tonnes.
            Assert.Equal(group.EC, group.AllElements.Sum(e => e.EC) / 1000, Precision);
        }

        [Fact]
        public void Excluded_element_keeps_its_model_volume_and_comes_back_when_ticked_in()
        {
            CarboProject project = TestProjects.ThreeElementProject();
            CarboGroup group = project.getGroupList.Single();
            CarboElement element = group.AllElements.Single(e => e.Id == 2);

            element.includeInCalc = false;
            project.CalculateProject();
            Assert.Equal(1, element.Volume, Precision);

            element.includeInCalc = true;
            project.CalculateProject();

            Assert.Equal(6, group.EC, Precision);
            Assert.Equal(2000, element.EC, Precision);
        }

        [Fact]
        public void Substructure_is_excluded_the_same_way_when_substructure_is_off()
        {
            CarboProject project = TestProjects.ThreeElementProject();
            CarboGroup group = project.getGroupList.Single();
            CarboElement substructure = group.AllElements.Single(e => e.Id == 3);
            substructure.isSubstructure = true;

            project.CalculateProject();
            Assert.Equal(2000, substructure.EC, Precision);

            project.calculateSubStructure = false;
            project.CalculateProject();

            Assert.Equal(4, group.EC, Precision);
            Assert.Equal(0, substructure.EC, Precision);
            Assert.Equal(group.EC, group.AllElements.Sum(e => e.EC) / 1000, Precision);
        }

        [Fact]
        public void Element_excluded_from_every_group_it_is_in_has_no_NaN_in_its_totals()
        {
            //An element split over two groups (a layered wall) and left out of both: the
            //cumulative ECI is carbon over mass, and both are zero.
            CarboProject project = TestProjects.ThreeElementProject();
            CarboGroup first = project.getGroupList.Single();

            CarboGroup second = new CarboGroup();
            second.Material = TestProjects.Material("Second material");
            second.AllElements.Add(TestProjects.Element(1, 0.5));
            project.AddGroup(second);

            first.AllElements.Single(e => e.Id == 1).includeInCalc = false;
            second.AllElements.Single().includeInCalc = false;

            project.CalculateProject();

            foreach (CarboElement element in project.getTemporaryElementListWithTotals())
            {
                Assert.False(double.IsNaN(element.ECI_Cumulative), "ECI_Cumulative of element " + element.Id);
                Assert.False(double.IsNaN(element.EC_Cumulative), "EC_Cumulative of element " + element.Id);
            }
        }

        [Fact]
        public void Heat_map_leaves_excluded_elements_out_and_lists_them_for_resetting()
        {
            CarboProject project = TestProjects.ThreeElementProject();
            project.getGroupList.Single().AllElements.Single(e => e.Id == 2).includeInCalc = false;
            project.CalculateProject();

            CarboGraphResult perElement = HeatMapCollector.GetPerElementData(project);
            CarboGraphResult perGroup = HeatMapCollector.GetPerGroupData(project);

            Assert.DoesNotContain(perElement.entireProjectData, v => v.Id == 2);
            Assert.DoesNotContain(perGroup.entireProjectData, v => v.Id == 2);
            Assert.Equal(new long[] { 2 }, perElement.excludedIds.ToArray());

            Assert.Equal(new long[] { 1, 3 }, perElement.entireProjectData.Select(v => v.Id).OrderBy(i => i).ToArray());
        }

        [Fact]
        public void Heat_map_keeps_an_element_that_is_still_counted_in_another_group()
        {
            CarboProject project = TestProjects.ThreeElementProject();

            CarboGroup second = new CarboGroup();
            second.Material = TestProjects.Material("Second material");
            second.AllElements.Add(TestProjects.Element(1, 0.5));
            project.AddGroup(second);

            //Element 1 is left out of the first group only.
            project.getGroupList.First().AllElements.Single(e => e.Id == 1).includeInCalc = false;
            project.CalculateProject();

            CarboGraphResult perElement = HeatMapCollector.GetPerElementData(project);

            Assert.Contains(perElement.entireProjectData, v => v.Id == 1);
            Assert.DoesNotContain(1L, perElement.excludedIds);
        }
    }
}
