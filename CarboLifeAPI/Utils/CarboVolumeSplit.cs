using System.Collections.Generic;
using System.Linq;

namespace CarboLifeAPI
{
    /// <summary>
    /// Shares a Revit element's volume out over its materials.
    ///
    /// Railings, ramps, curtain panels, mullions and connection plates are read from the
    /// element's single Volume value, the volume of the whole element. That volume used to be
    /// handed to every material the element has, so a two material element came in at twice its
    /// volume: a glazed panel of 0.108 m³ glass and 0.010 m³ aluminium frame imported 0.118 m³ of
    /// each, and the aluminium - the material with the high carbon per kilogram - twelve times
    /// over. Each material has to get its own share, and the shares have to add up to the element.
    ///
    /// Kept free of Revit types so the rule can be tested without Revit: the add-in collects
    /// the numbers, this decides what to do with them.
    /// </summary>
    public static class CarboVolumeSplit
    {
        /// <summary>
        /// How far the materials' own volumes may add up to more than the element before they
        /// are scaled back to it. Revit rounds each material separately, so a small excess is
        /// rounding and not a reason to distrust them.
        /// </summary>
        private const double OverRunTolerance = 0.001;

        /// <summary>
        /// The volume each material gets, in the units the volumes were given in.
        /// </summary>
        /// <param name="elementVolume">The volume of the whole element.</param>
        /// <param name="materialVolumes">
        /// Every material of the element with the volume Revit reports for it
        /// (Element.GetMaterialVolume), zero where it reports none.
        /// </param>
        /// <returns>
        /// Material name to volume. Null when the element has several materials and Revit gives
        /// none of them a volume: the element volume cannot be shared out on this information,
        /// and the caller has to measure the solids instead.
        /// </returns>
        public static Dictionary<string, double> Split(double elementVolume, IDictionary<string, double> materialVolumes)
        {
            if (materialVolumes == null || materialVolumes.Count == 0)
                return null;

            //Revit's own figure per material wins whenever it has one. A material with none is
            //left out rather than guessed at: that is a paint or other surface material, which
            //has an area and no volume.
            Dictionary<string, double> measured = materialVolumes
                .Where(m => m.Value > 0)
                .ToDictionary(m => m.Key, m => m.Value);

            if (measured.Count > 0)
            {
                double total = measured.Values.Sum();

                //Never hand out more than the element holds.
                if (elementVolume > 0 && total > elementVolume * (1 + OverRunTolerance))
                {
                    double scale = elementVolume / total;
                    foreach (string name in measured.Keys.ToList())
                        measured[name] *= scale;
                }

                return measured;
            }

            //No volume per material. With one material the whole element is that material.
            if (materialVolumes.Count == 1)
            {
                if (elementVolume <= 0)
                    return null;

                return new Dictionary<string, double> { { materialVolumes.Keys.First(), elementVolume } };
            }

            //Several materials and nothing to divide the volume by.
            return null;
        }
    }
}
