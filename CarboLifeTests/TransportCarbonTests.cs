using CarboLifeAPI;
using CarboLifeAPI.Data;
using System.Data;
using System.IO;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// A4/C2 transport carbon (CarboA4Properties). The default is a 22t diesel truck, 50 km to
    /// site and back empty, 500 kg/m³ material at an 80% load factor.
    /// </summary>
    public class TransportCarbonTests
    {
        private const double Precision = 1e-12;

        /// <summary>The value for a truck carrying loadPerTrip kg, 100 km per trip.</summary>
        private static double TruckValue(double loadPerTrip)
        {
            double fuelPerKm = (2.62 + 0.4) / 3.85;
            double buildPerKm = 280000.0 / 250000.0;

            return (fuelPerKm + buildPerKm) * 100 / loadPerTrip;
        }

        [Fact]
        public void Default_truck_is_weight_limited_at_the_load_factor()
        {
            CarboA4Properties a4 = new CarboA4Properties();

            //500 kg/m³ x 96 m³ = 48t by volume, so the 22t weight limit applies: 22000 x 0.8.
            Assert.True(a4.isValid);
            Assert.Equal(TruckValue(17600), a4.value, Precision);
        }

        [Fact]
        public void Light_material_is_volume_limited_at_the_load_factor()
        {
            CarboA4Properties a4 = new CarboA4Properties();
            a4.materialDensity = 100;
            a4.calculate();

            //96 m³ x 100 kg/m³ = 9600 kg, under 22t: 9600 x 0.8.
            Assert.Equal(TruckValue(7680), a4.value, Precision);
        }

        [Fact]
        public void Load_factor_applies_to_the_limit_reached_first()
        {
            CarboA4Properties a4 = new CarboA4Properties();
            a4.materialDensity = 270;
            a4.calculate();

            //A full volume of 270 kg/m³ is 25.9t, over the 22t limit, so the load is 22000 x 0.8.
            //It used to compare against the reduced density and load 20736 kg.
            Assert.Equal(TruckValue(17600), a4.value, Precision);
        }

        [Fact]
        public void Vehicle_without_capacity_uses_its_tonne_km_factor()
        {
            CarboA4Properties a4 = new CarboA4Properties();
            a4.vehicleSettings.maxVolume = 0;
            a4.vehicleSettings.gCO2PerTonneKm = 22;
            a4.distanceToSite = 100;
            a4.calculate();

            //22 g per tonne-km x 100 km = 2200 g per tonne = 0.0022 kg per kg. No empty return.
            Assert.True(a4.usesTonneKm);
            Assert.True(a4.isValid);
            Assert.Equal(0.0022, a4.value, Precision);
        }

        [Theory]
        [InlineData(0, 80, 50)]      //no capacity and no tonne-km factor
        [InlineData(96, 0, 50)]      //load factor 0%
        [InlineData(96, 120, 50)]    //load factor over 100%
        [InlineData(96, 80, -10)]    //negative distance
        public void Invalid_inputs_give_no_value_rather_than_infinity(double maxVolume, double loadFactor, double distance)
        {
            CarboA4Properties a4 = new CarboA4Properties();
            a4.vehicleSettings.maxVolume = maxVolume;
            a4.vehicleSettings.gCO2PerTonneKm = 0;
            a4.emptyRun = loadFactor;
            a4.distanceToSite = distance;
            a4.calculate();

            Assert.False(a4.isValid);
            Assert.NotEqual("", a4.error);
            Assert.Equal(0.0, a4.value);
        }

        [Fact]
        public void Zero_fuel_consumption_gives_no_value()
        {
            CarboA4Properties a4 = new CarboA4Properties();
            a4.vehicleSettings.kmPer = 0;
            a4.calculate();

            Assert.False(a4.isValid);
            Assert.Equal(0.0, a4.value);
        }

        [Fact]
        public void Copy_does_not_share_vehicle_or_fuel()
        {
            CarboA4Properties original = new CarboA4Properties();
            double before = original.value;

            CarboA4Properties copy = original.Copy();
            copy.distanceToSite = 500;
            copy.vehicleSettings.maxLoad = 1000;
            copy.vehicleFuel.emission = 10;
            copy.calculate();

            original.calculate();
            Assert.Equal(50.0, original.distanceToSite);
            Assert.Equal(22000.0, original.vehicleSettings.maxLoad);
            Assert.Equal(2.62, original.vehicleFuel.emission);
            Assert.Equal(before, original.value, Precision);
        }

        /// <summary>
        /// Every vehicle the dialog offers, read the way the dialog reads Transport.csv, gives a
        /// value. Rail and shipping used to give Infinity or NaN.
        /// </summary>
        [Fact]
        public void Every_shipped_vehicle_gives_a_valid_value()
        {
            string path = Path.Combine(PathUtils.GetAssemblyDir(), "data", "Transport.csv");
            Assert.True(File.Exists(path), "Transport.csv not copied to the test output: " + path);

            DataTable table = Utils.LoadCSV(path);
            Assert.True(table.Rows.Count > 0);

            foreach (DataRow dr in table.Rows)
            {
                CarboA4Properties a4 = new CarboA4Properties();
                a4.vehicleSettings.name = dr[0].ToString();
                a4.vehicleSettings.maxVolume = DataExportUtils.ReadCsvDouble(dr[1].ToString());
                a4.vehicleSettings.maxLoad = DataExportUtils.ReadCsvDouble(dr[2].ToString());
                a4.vehicleSettings.kmPer = DataExportUtils.ReadCsvDouble(dr[3].ToString());
                a4.vehicleSettings.carboCostNew = DataExportUtils.ReadCsvDouble(dr[4].ToString());
                a4.vehicleSettings.totalDistance = DataExportUtils.ReadCsvDouble(dr[5].ToString());
                a4.vehicleSettings.gCO2PerTonneKm = DataExportUtils.ReadCsvDouble(dr[7].ToString());
                a4.calculate();

                Assert.True(a4.isValid, a4.vehicleSettings.name + ": " + a4.error);
                Assert.True(a4.value > 0, a4.vehicleSettings.name + " gave " + a4.value);
            }
        }
    }
}
