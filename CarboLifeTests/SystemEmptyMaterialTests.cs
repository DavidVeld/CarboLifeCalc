using CarboLifeAPI.Data;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// The built in "&lt;Empty&gt;" material has to stay at zero after a recalculation. It used to
    /// pick up the default 50 km truck (A4 0.0108) and the default landfill/re-use split (C1-C4 0.009).
    /// </summary>
    public class SystemEmptyMaterialTests
    {
        private static void AssertAllZero(CarboMaterial m)
        {
            m.CalculateTotals();

            Assert.Equal(0, m.ECI_A1A3);
            Assert.Equal(0, m.ECI_A4);
            Assert.Equal(0, m.ECI_A5);
            Assert.Equal(0, m.ECI_C1C4);
            Assert.Equal(0, m.ECI_D);
            Assert.Equal(0, m.ECI_Seq);
            Assert.Equal(0, m.ECI);
        }

        [Fact]
        public void New_empty_material_stays_zero_after_recalculation()
        {
            AssertAllZero(CarboMaterial.CreateSystemEmpty());
        }

        [Fact]
        public void Empty_material_from_an_older_file_is_reset_on_load()
        {
            //An older file: zero figures, but every one of them left to be calculated.
            CarboMaterial old = CarboMaterial.CreateSystemEmpty();
            old.ECI_A4_Override = false;
            old.ECI_C1C4_Override = false;

            CarboDatabase db = new CarboDatabase();
            db.CarboMaterialList.Clear();
            db.CarboMaterialList.Add(old);

            Assert.True(db.EnsureSystemMaterials());
            AssertAllZero(old);
        }
    }
}
