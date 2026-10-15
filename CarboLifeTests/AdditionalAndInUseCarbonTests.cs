using CarboLifeAPI.Data;
using System.Linq;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// A group's own Additional and its in-use (B1-B7) carbon. Both used to go into the total
    /// whatever their switch said, and the in-use part into no phase at all, so the phases did
    /// not add up to the total.
    ///
    /// The project: 3000 kg at 2 kgCO₂e/kg, 6 tCO₂e of A1-A3 and nothing else.
    /// </summary>
    public class AdditionalAndInUseCarbonTests
    {
        private const double Precision = 1e-9;

        [Theory]
        [InlineData(false, 6)]
        [InlineData(true, 9)]
        public void Additional_counts_only_under_its_switch(bool calculateAdd, double expectedTonnes)
        {
            CarboProject project = TestProjects.ThreeElementProject();
            project.getGroupList[0].Additional = 1;
            project.calculateAdd = calculateAdd;

            project.CalculateProject();

            Assert.Equal(expectedTonnes, project.ECTotal, Precision);
            Assert.Equal(expectedTonnes, PhaseSumTonnes(project), Precision);
        }

        [Theory]
        [InlineData(false, 6)]
        [InlineData(true, 7.5)]
        public void In_use_carbon_counts_only_under_B_and_shows_in_B1_B7(bool calculateB, double expectedTonnes)
        {
            CarboProject project = TestProjects.ThreeElementProject();
            CarboB1B7Properties inUse = project.getGroupList[0].inUseProperties;
            inUse.B2 = 0.5;
            inUse.calculate(project.designLife);
            project.calculateB = calculateB;

            project.CalculateProject();

            Assert.Equal(expectedTonnes, project.ECTotal, Precision);
            Assert.Equal(expectedTonnes, PhaseSumTonnes(project), Precision);

            //And the elements still add up to the group.
            Assert.Equal(expectedTonnes * 1000, project.getGroupList[0].AllElements.Sum(e => e.EC), 1e-6);
        }

        [Fact]
        public void A_copied_group_keeps_its_in_use_carbon()
        {
            CarboGroup group = TestProjects.ThreeElementProject().getGroupList[0];
            group.inUseProperties.B2 = 0.5;
            group.inUseProperties.calculate(60);

            Assert.Equal(0.5, group.Copy().inUseProperties.totalECI, Precision);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-5, 1)]
        [InlineData(30, 2)]
        [InlineData(80, 1)]
        public void Replacements_are_finite_and_at_least_one(double elementLife, double expectedB4)
        {
            CarboB1B7Properties inUse = new CarboB1B7Properties();
            inUse.designLifeToEnd = false;
            inUse.elementdesignlife = elementLife;

            inUse.calculate(60);

            Assert.Equal(expectedB4, inUse.B4);
        }

        private static double PhaseSumTonnes(CarboProject project)
        {
            return project.getPhaseTotals().Sum(p => p.Value) / 1000;
        }
    }
}
