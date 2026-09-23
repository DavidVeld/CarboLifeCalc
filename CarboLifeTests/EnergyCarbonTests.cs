using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.Linq;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// B6/B7 operational energy carbon. It used to be calculated in four places that disagreed
    /// about the uncertainty factor and about the B6/B7 switch; these pin them to one answer.
    ///
    /// The project: 6 tCO₂e of material and 1 tCO₂e of energy, both before uncertainty.
    /// </summary>
    public class EnergyCarbonTests
    {
        private const double Precision = 1e-9;

        [Fact]
        public void Uncertainty_applies_to_energy_in_every_total()
        {
            CarboProject project = TestProjects.ThreeElementProject();
            TestProjects.SetOneTonneOfEnergy(project);
            project.calculateB67 = true;
            project.UncertFact = 0.1;

            project.CalculateProject();

            //Material 6 x 1.1 = 6.6, energy 1 x 1.1 = 1.1.
            Assert.Equal(1.1, project.b675Global, Precision);
            Assert.Equal(7.7, project.ECTotal, Precision);
            Assert.Equal(7.7, project.getTotalEC(), Precision);

            CarboDataPoint energy = project.getPhaseTotals().Single(p => p.Name.StartsWith("Energy"));
            Assert.Equal(1100, energy.Value, Precision);
        }

        [Fact]
        public void Energy_is_left_out_everywhere_when_B6B7_is_off()
        {
            CarboProject project = TestProjects.ThreeElementProject();
            TestProjects.SetOneTonneOfEnergy(project);
            project.calculateB67 = false;
            project.UncertFact = 0.1;

            project.CalculateProject();

            Assert.Equal(6.6, project.ECTotal, Precision);
            Assert.Equal(6.6, project.getTotalEC(), Precision);
            Assert.DoesNotContain(project.getPhaseTotals(), p => p.Name.StartsWith("Energy"));
        }

        [Fact]
        public void Calculating_by_phase_follows_its_own_energy_switch()
        {
            CarboProject project = TestProjects.ThreeElementProject();
            TestProjects.SetOneTonneOfEnergy(project);
            project.UncertFact = 0.1;

            //The project's own switch is off; the argument is what counts here.
            project.calculateB67 = false;
            project.CalculateProjectByPhase(cEnergyUse: true);
            Assert.Equal(7.7, project.ECTotal, Precision);

            project.calculateB67 = true;
            project.CalculateProjectByPhase(cEnergyUse: false);
            Assert.Equal(6.6, project.ECTotal, Precision);
        }

        [Fact]
        public void Calculating_by_phase_and_by_project_agree_when_asked_for_the_same_modules()
        {
            CarboProject project = TestProjects.ThreeElementProject();
            TestProjects.SetOneTonneOfEnergy(project);
            project.calculateB67 = true;
            project.UncertFact = 0.25;

            project.CalculateProject();
            double byProject = project.ECTotal;

            project.CalculateProjectByPhase(cA13: true, cA4: false, cA5: false, cB: false, cC: false,
                cD: false, cSeq: false, cAdd: false, cEnergyUse: true, cA0: false);

            Assert.Equal(byProject, project.ECTotal, Precision);
        }
    }
}
