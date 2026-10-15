using CarboLifeAPI;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// The group correction evaluator. It used to rank - below + and / below *, so a rebar group
    /// under a concrete group corrected by "/2" came out as 100/(2*(150/7850)) = 2617 m³, not 0.96.
    /// </summary>
    public class StringToFormulaTests
    {
        [Theory]
        [InlineData("100/2*(150/7850)", 100.0 / 2 * (150.0 / 7850))]
        [InlineData("100/2*3", 150)]
        [InlineData("10-0.1+0.05", 9.95)]
        [InlineData("1-2+3", 2)]
        [InlineData("2+3*4", 14)]
        [InlineData("2^3^2", 512)]
        //How .NET writes a volume below 0.0001 m³, followed by a generated rebar correction.
        [InlineData("3.6E-05*(150/7850)", 3.6e-5 * (150.0 / 7850))]
        [InlineData("2E+3-1", 1999)]
        public void Evaluates_with_standard_operator_order(string expression, double expected)
        {
            Assert.Equal(expected, new StringToFormula().Eval(expression), 9);
        }

        [Fact]
        public void A_corrected_group_with_a_tiny_element_calculates()
        {
            //Every rebar and connection group carries a correction, so one connection plate below
            //0.0001 m³ used to abort the whole calculation.
            CarboLifeAPI.Data.CarboProject project = TestProjects.ThreeElementProject();
            project.getGroupList[0].Correction = "*2";
            project.getGroupList[0].AllElements.Add(TestProjects.Element(4, 3.6e-5));

            project.CalculateProject();

            //(3 + 0.000036) m³ x 2, at 2 tCO₂e per m³.
            Assert.Equal((3 + 3.6e-5) * 2 * 2, project.ECTotal, 9);
        }
    }
}
