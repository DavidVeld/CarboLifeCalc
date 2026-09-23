using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// The heat map's carbon per m³ view. Revit takes one colour per element id, so an element
    /// split over several groups (a layered wall, a reinforced slab) needs ONE value: the carbon
    /// of all its parts over the volume of all its parts - EC_Cumulative / Volume_Cumulative.
    /// </summary>
    public class HeatMapIntensityTests
    {
        private const double Precision = 1e-6;

        private static CarboMaterial Material(string name, double density, double eciA1A3)
        {
            CarboMaterial material = TestProjects.Material(name);
            material.Density = density;
            material.ECI_A1A3 = eciA1A3;
            return material;
        }

        private static CarboGroup Group(CarboMaterial material, params CarboElement[] elements)
        {
            CarboGroup group = new CarboGroup();
            group.Material = material;
            group.MaterialName = material.Name;
            group.AllElements = elements.ToList();
            return group;
        }

        private static CarboProject Project(params CarboGroup[] groups)
        {
            CarboProject project = TestProjects.ThreeElementProject();
            project.getGroupList.Clear();
            foreach (CarboGroup group in groups)
                project.AddGroup(group);
            project.CalculateProject();
            return project;
        }

        [Fact]
        public void Layered_wall_gets_one_value_its_total_carbon_over_its_total_volume()
        {
            //Wall 1: 0.2 m³ concrete (2400 kg/m³, 0.1 kgCO₂e/kg) = 48 kgCO₂e
            //      + 0.1 m³ insulation (30 kg/m³, 1.5 kgCO₂e/kg)  = 4.5 kgCO₂e
            //      = 52.5 kgCO₂e over 0.3 m³ = 175 kgCO₂e/m³
            CarboProject project = Project(
                Group(Material("Concrete", 2400, 0.1), TestProjects.Element(1, 0.2)),
                Group(Material("Insulation", 30, 1.5), TestProjects.Element(1, 0.1)));

            List<CarboValues> values = HeatMapCollector.GetMaterialVolumeData(project)
                .entireProjectData.Where(v => v.Id == 1).ToList();

            //One colour per id: every entry for the element must carry the same value.
            Assert.All(values, v => Assert.Equal(175, v.Value, Precision));
        }

        [Fact]
        public void Reinforcement_counts_its_own_volume_not_the_concrete_volume_again()
        {
            //Slab 1: 1 m³ concrete (2400 kg/m³, 0.1) = 240 kgCO₂e
            //      + rebar at 120 kg/m³ (7850 kg/m³, 1.2) = 144 kgCO₂e, 0.0153 m³ of steel
            //The rebar part is a copy of the concrete element, so its Volume is the concrete's
            //1 m³; only its Volume_Total, after the correction, is the steel.
            CarboElement rebar = TestProjects.Element(1, 1);
            CarboGroup rebarGroup = Group(Material("Rebar", 7850, 1.2), rebar);
            rebarGroup.Correction = "*(120/7850)";

            CarboProject project = Project(
                Group(Material("Concrete", 2400, 0.1), TestProjects.Element(1, 1)),
                rebarGroup);

            double expected = (240 + 144) / (1 + 120.0 / 7850);   //378.2 kgCO₂e/m³

            List<CarboValues> values = HeatMapCollector.GetMaterialVolumeData(project)
                .entireProjectData.Where(v => v.Id == 1).ToList();

            Assert.All(values, v => Assert.Equal(expected, v.Value, Precision));
        }
    }
}
