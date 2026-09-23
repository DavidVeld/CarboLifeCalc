using CarboLifeAPI.Data;
using System.Collections.Generic;

namespace CarboLifeTests
{
    /// <summary>
    /// Small projects built in code, with numbers simple enough to check by hand.
    ///
    /// Every module is switched off except A1-A3, and every global (A0, A5, C1, energy) is zero,
    /// so a test switches on only what it is about and the expected value stays a one line sum.
    /// </summary>
    internal static class TestProjects
    {
        /// <summary>1000 kg/m³ at 2 kgCO₂e/kg A1-A3 and nothing else: 2 tCO₂e per m³.</summary>
        public static CarboMaterial Material(string name = "Test material")
        {
            CarboMaterial material = new CarboMaterial(name);
            material.Density = 1000;

            //Overridden, so CalculateTotals keeps these instead of rebuilding them from the
            //sub-property objects.
            material.ECI_A1A3_Override = true;
            material.ECI_A4_Override = true;
            material.ECI_A5_Override = true;
            material.ECI_C1C4_Override = true;
            material.ECI_D_Override = true;
            material.ECI_Seq_Override = true;

            material.ECI_A1A3 = 2;
            material.ECI_A4 = 0;
            material.ECI_A5 = 0;
            material.ECI_B1B5 = 0;
            material.ECI_C1C4 = 0;
            material.ECI_D = 0;
            material.ECI_Seq = 0;
            material.ECI_Mix = 0;

            return material;
        }

        public static CarboElement Element(long id, double volume)
        {
            CarboElement element = new CarboElement();
            element.Id = id;
            element.Name = "Element " + id;
            element.Volume = volume;
            element.includeInCalc = true;
            return element;
        }

        /// <summary>One group of three 1 m³ elements, ids 1-3: 6 tCO₂e with everything counted.</summary>
        public static CarboProject ThreeElementProject()
        {
            CarboProject project = new CarboProject();
            project.getGroupList.Clear();

            project.calculateA0 = false;
            project.calculateA13 = true;
            project.calculateA4 = false;
            project.calculateA5 = false;
            project.calculateB = false;
            project.calculateB67 = false;
            project.calculateC = false;
            project.calculateD = false;
            project.calculateSeq = false;
            project.calculateAdd = false;
            project.calculateSubStructure = true;

            project.UncertFact = 0;
            project.A0Global = 0;
            project.AreaNew = 0;
            project.demoArea = 0;
            project.designLife = 10;
            project.energyProperties = new CarboEnergyProperties();

            CarboGroup group = new CarboGroup();
            group.Material = Material();
            group.MaterialName = group.Material.Name;
            group.Category = "Test";
            group.Waste = 0;
            group.Correction = "";
            group.AllElements = new List<CarboElement>
            {
                Element(1, 1),
                Element(2, 1),
                Element(3, 1),
            };

            project.AddGroup(group);

            return project;
        }

        /// <summary>
        /// 1000 kWh a year at 0.1 kgCO₂e/kWh for the 10 year design life, no decarbonisation:
        /// 1000 kgCO₂e, 1 tCO₂e, before uncertainty.
        /// </summary>
        public static void SetOneTonneOfEnergy(CarboProject project)
        {
            project.energyProperties.ElectricityUsedPerYear = 1000;
            project.energyProperties.CO2CostPerkWh = 0.1;
            project.energyProperties.WaterUsedPerYear = 0;
            project.energyProperties.CO2CostPerm3 = 0;
            project.energyProperties.ElectricitygeneratedPerYear = 0;
            project.energyProperties.decabornisationFactor = 0;
        }
    }
}
