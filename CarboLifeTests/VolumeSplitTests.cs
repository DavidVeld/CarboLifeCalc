using CarboLifeAPI;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CarboLifeTests
{
    /// <summary>
    /// Sharing the volume of a railing, ramp, curtain panel, mullion or connection plate out
    /// over its materials. The whole element volume used to go to every material, so a two
    /// material element came in at twice its volume.
    ///
    /// The Revit side of this - that the add-in hands Split the right numbers - is checked on
    /// the test model, samples\2024-\Unit Test CLC R2023.rvt.
    /// </summary>
    public class VolumeSplitTests
    {
        private const double Precision = 1e-9;

        [Fact]
        public void Uses_Revits_own_volume_per_material_when_it_has_one()
        {
            //A glazed curtain panel: Revit's Volume is 0.118 m³, glass and frame reported apart.
            Dictionary<string, double> parts = CarboVolumeSplit.Split(0.118, new Dictionary<string, double>
            {
                { "Glass", 0.108 },
                { "Aluminium", 0.010 },
            });

            Assert.Equal(0.108, parts["Glass"], Precision);
            Assert.Equal(0.010, parts["Aluminium"], Precision);

            //The old import gave 0.236: each material the whole element.
            Assert.Equal(0.118, parts.Values.Sum(), Precision);
        }

        [Fact]
        public void A_single_material_takes_the_whole_volume()
        {
            Dictionary<string, double> parts = CarboVolumeSplit.Split(0.05, new Dictionary<string, double>
            {
                { "Steel", 0 },
            });

            Assert.Equal(0.05, parts["Steel"], Precision);
        }

        [Fact]
        public void Several_materials_without_their_own_volume_go_to_the_geometry_route()
        {
            Dictionary<string, double> parts = CarboVolumeSplit.Split(0.118, new Dictionary<string, double>
            {
                { "Glass", 0 },
                { "Aluminium", 0 },
            });

            Assert.Null(parts);
        }

        [Fact]
        public void A_surface_material_with_no_volume_gets_none()
        {
            //Painted steel plate: the paint has an area and no volume. It must not take a share
            //of the steel, and it must not send the element to the geometry route either.
            Dictionary<string, double> parts = CarboVolumeSplit.Split(0.02, new Dictionary<string, double>
            {
                { "Steel", 0.02 },
                { "Paint", 0 },
            });

            Assert.Equal(new[] { "Steel" }, parts.Keys.ToArray());
            Assert.Equal(0.02, parts["Steel"], Precision);
        }

        [Fact]
        public void Never_hands_out_more_volume_than_the_element_has()
        {
            //Materials adding up to 10% more than the element: scaled back, in proportion.
            Dictionary<string, double> parts = CarboVolumeSplit.Split(0.1, new Dictionary<string, double>
            {
                { "Glass", 0.088 },
                { "Aluminium", 0.022 },
            });

            Assert.Equal(0.1, parts.Values.Sum(), Precision);
            Assert.Equal(0.08, parts["Glass"], Precision);
            Assert.Equal(0.02, parts["Aluminium"], Precision);
        }

        [Fact]
        public void Revit_rounding_is_not_scaled()
        {
            //A few hundredths of a percent over is each material being rounded, not an error.
            Dictionary<string, double> parts = CarboVolumeSplit.Split(0.118, new Dictionary<string, double>
            {
                { "Glass", 0.10802 },
                { "Aluminium", 0.01001 },
            });

            Assert.Equal(0.10802, parts["Glass"], Precision);
            Assert.Equal(0.01001, parts["Aluminium"], Precision);
        }

        [Fact]
        public void No_materials_goes_to_the_geometry_route()
        {
            Assert.Null(CarboVolumeSplit.Split(0.05, new Dictionary<string, double>()));
        }
    }
}
