using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using CarboLifeAPI;
using CarboLifeAPI.Data;

namespace CarboLifeAPI.Data
{
    /// <summary>
    /// Transport carbon per kg of material, used for A4 (to site) and C2 (away from site).
    ///
    /// Two ways to calculate, chosen by the vehicle:
    /// - A vehicle with a load AND a volume capacity is calculated trip by trip: the fuel and a
    ///   share of the vehicle's construction carbon for one trip, divided by the load it carries.
    /// - A vehicle without either capacity (rail and shipping in Transport.csv) is calculated from
    ///   its tonne-km factor. Those factors are averages per tonne of goods moved, so the empty
    ///   return and the load factor are already in them and are not applied again.
    /// </summary>
    [Serializable]
    public class CarboA4Properties
    {
        public string name { get; set; }
        public string description { get; set; }

        public double distanceToSite { get; set; }
        public double materialDensity { get; set; }
        /// <summary>
        /// The load factor in %: how full the vehicle is on average. Named emptyRun for the
        /// projects and templates already saved with it.
        /// </summary>
        public double emptyRun { get; set; }
        public bool emptyReturn { get; set; }

        //User Variables
        public Vehicle vehicleSettings { get; set; }
        public Fuel vehicleFuel { get; set; }

        //Results
        public double value { get; set; }

        [NonSerialized]
        public string calcResult;

        /// <summary>
        /// False when the inputs cannot give a transport value. value is then 0, never an
        /// Infinity or NaN that would carry into every total using the material.
        /// </summary>
        [XmlIgnore]
        public bool isValid { get; set; }

        /// <summary>Why the inputs cannot give a value, empty when isValid.</summary>
        [XmlIgnore]
        public string error { get; set; }

        public CarboA4Properties()
        {
            name = "";
            description = "";

            distanceToSite = 50;
            materialDensity = 500;
            emptyRun = 80;
            emptyReturn = true;

            vehicleSettings = new Vehicle();
            vehicleFuel = new Fuel();

            calcResult = "";
            value = 0;

            calculate();
        }

        /// <summary>
        /// A copy with its own vehicle and fuel, for a dialog to edit without touching the
        /// original until the edit is accepted.
        /// </summary>
        public CarboA4Properties Copy()
        {
            CarboA4Properties copy = (CarboA4Properties)this.MemberwiseClone();

            copy.vehicleSettings = vehicleSettings != null ? vehicleSettings.Copy() : new Vehicle();
            copy.vehicleFuel = vehicleFuel != null ? vehicleFuel.Copy() : new Fuel();

            return copy;
        }

        /// <summary>
        /// True when the vehicle is calculated from its tonne-km factor, see the class summary.
        /// </summary>
        public bool usesTonneKm
        {
            get
            {
                return vehicleSettings != null &&
                    (vehicleSettings.maxLoad <= 0 || vehicleSettings.maxVolume <= 0);
            }
        }

        public void calculate()
        {
            if (vehicleSettings == null)
                vehicleSettings = new Vehicle();
            if (vehicleFuel == null)
                vehicleFuel = new Fuel();

            value = 0;
            isValid = false;
            error = "";

            StringBuilder text = new StringBuilder();

            if (distanceToSite < 0)
                error = "The distance to site cannot be negative.";
            else if (usesTonneKm)
                calculateByTonneKm(text);
            else
                calculateByTrip(text);

            if (error == "" && (double.IsNaN(value) || double.IsInfinity(value) || value < 0))
                error = "The inputs do not give a valid transport value.";

            if (error != "")
            {
                value = 0;
                text.Insert(0, "NO VALUE: " + error + Environment.NewLine + Environment.NewLine);
            }
            else
            {
                isValid = true;
            }

            this.calcResult = text.ToString();
        }

        private void calculateByTonneKm(StringBuilder text)
        {
            string nl = Environment.NewLine;
            double factor = vehicleSettings.gCO2PerTonneKm;

            text.Append("Calculation by tonne-km" + nl);
            text.Append("A " + vehicleSettings.name + " has no load and volume capacity set, so it is calculated from its average emission per tonne of goods moved per km." + nl);
            text.Append("That average already includes empty running and part loads, so the load factor and the empty return are not applied." + nl + nl);

            if (factor <= 0)
            {
                error = "This vehicle has no load and volume capacity and no tonne-km factor. Set either the capacities, or a tonne-km factor.";
                return;
            }

            value = factor * distanceToSite / 1000000;

            text.Append("Distance to site = " + Show(distanceToSite) + " km" + nl);
            text.Append("Emission = " + Show(factor) + " gCO₂e/tonne-km" + nl + nl);
            text.Append(Show(factor) + " gCO₂e/tonne-km x " + Show(distanceToSite) + " km = " + Show(factor * distanceToSite) + " gCO₂e/tonne" + nl);
            text.Append(Show(factor * distanceToSite) + " gCO₂e/tonne / 1,000,000 = " + Show(value) + " kgCO₂e/kg" + nl);
        }

        private void calculateByTrip(StringBuilder text)
        {
            string nl = Environment.NewLine;

            double maxLoad = vehicleSettings.maxLoad;
            double maxVolume = vehicleSettings.maxVolume;
            double kmPer = vehicleSettings.kmPer;
            double loadFactor = emptyRun / 100;

            if (emptyRun <= 0 || emptyRun > 100)
            {
                error = "The load factor has to be more than 0% and at most 100%.";
                return;
            }

            if (kmPer <= 0)
            {
                error = "The fuel consumption has to be more than 0 km per " + vehicleFuel.unit + ".";
                return;
            }

            double totalDistance = emptyReturn ? distanceToSite * 2 : distanceToSite;

            //The load is the load factor applied to whichever limit the vehicle reaches first,
            //its weight or its volume. Without a density the volume cannot be checked.
            double fullByVolume = materialDensity > 0 ? maxVolume * materialDensity : double.PositiveInfinity;
            bool limitedByVolume = fullByVolume < maxLoad;
            double fullLoad = limitedByVolume ? fullByVolume : maxLoad;
            double loadPerTrip = fullLoad * loadFactor;

            //Fuel: fuel carbon is per unit of fuel, consumption is in km per unit of fuel.
            double fuelPerUnit = vehicleFuel.emission + vehicleFuel.production;
            double fuelPerKm = fuelPerUnit / kmPer;
            double fuelPerTrip = fuelPerKm * totalDistance;
            double fuelPerKg = fuelPerTrip / loadPerTrip;

            //Construction: the vehicle's own carbon spread over the distance it covers in its life.
            bool includeBuild = vehicleSettings.carboCostNew > 0 && vehicleSettings.totalDistance > 0;
            double buildPerKm = includeBuild ? vehicleSettings.carboCostNew / vehicleSettings.totalDistance : 0;
            double buildPerTrip = buildPerKm * totalDistance;
            double buildPerKg = buildPerTrip / loadPerTrip;

            value = fuelPerKg + buildPerKg;

            text.Append("Calculation per trip" + nl);
            text.Append("Distance to site = " + Show(distanceToSite) + " km" + nl);
            if (emptyReturn)
                text.Append("The vehicle returns empty, so the distance per trip = 2 x " + Show(distanceToSite) + " = " + Show(totalDistance) + " km" + nl);
            else
                text.Append("The vehicle does not return empty, so the distance per trip = " + Show(totalDistance) + " km" + nl);
            text.Append(nl);

            text.Append("Load" + nl);
            if (materialDensity > 0)
            {
                text.Append("A full " + vehicleSettings.name + " carries " + Show(maxLoad) + " kg, or " + Show(maxVolume) + " m³ x " + Show(materialDensity) + " kg/m³ = " + Show(fullByVolume) + " kg by volume" + nl);
                text.Append(limitedByVolume ? "Volume is the limit: " : "Weight is the limit: ");
                text.Append(Show(fullLoad) + " kg" + nl);
            }
            else
            {
                text.Append("No material density, so the volume cannot be checked: the weight limit of " + Show(maxLoad) + " kg is used" + nl);
            }
            text.Append("At a load factor of " + Show(emptyRun) + "%: " + Show(fullLoad) + " x " + Show(loadFactor) + " = " + Show(loadPerTrip) + " kg per trip" + nl);
            text.Append(nl);

            text.Append("Fuel (" + vehicleFuel.name + ")" + nl);
            text.Append("Fuel carbon = " + Show(vehicleFuel.emission) + " + " + Show(vehicleFuel.production) + " (production) = " + Show(fuelPerUnit) + " kgCO₂e/" + vehicleFuel.unit + nl);
            text.Append("The vehicle travels " + Show(kmPer) + " km/" + vehicleFuel.unit + ": " + Show(fuelPerUnit) + " / " + Show(kmPer) + " = " + Show(fuelPerKm) + " kgCO₂e/km" + nl);
            text.Append(Show(fuelPerKm) + " kgCO₂e/km x " + Show(totalDistance) + " km = " + Show(fuelPerTrip) + " kgCO₂e/trip" + nl);
            text.Append(Show(fuelPerTrip) + " kgCO₂e/trip / " + Show(loadPerTrip) + " kg = " + Show(fuelPerKg) + " kgCO₂e/kg" + nl);
            text.Append(nl);

            text.Append("Vehicle construction" + nl);
            if (includeBuild)
            {
                text.Append("Building the vehicle = " + Show(vehicleSettings.carboCostNew) + " kgCO₂e over a life of " + Show(vehicleSettings.totalDistance) + " km" + nl);
                text.Append(Show(vehicleSettings.carboCostNew) + " / " + Show(vehicleSettings.totalDistance) + " = " + Show(buildPerKm) + " kgCO₂e/km" + nl);
                text.Append(Show(buildPerKm) + " kgCO₂e/km x " + Show(totalDistance) + " km = " + Show(buildPerTrip) + " kgCO₂e/trip" + nl);
                text.Append(Show(buildPerTrip) + " kgCO₂e/trip / " + Show(loadPerTrip) + " kg = " + Show(buildPerKg) + " kgCO₂e/kg" + nl);
            }
            else
            {
                text.Append("No construction carbon or lifetime distance set: not included" + nl);
            }
            text.Append(nl);

            text.Append("Total = " + Show(fuelPerKg) + " (fuel) + " + Show(buildPerKg) + " (construction) = " + Show(value) + " kgCO₂e/kg" + nl);
        }

        /// <summary>
        /// A number rounded for reading: small values keep enough decimals to show their size.
        /// </summary>
        private static string Show(double number)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
                return "-";

            double size = Math.Abs(number);

            if (size >= 100)
                return number.ToString("#,0.#");
            if (size >= 1)
                return number.ToString("0.###");

            return number.ToString("0.######");
        }
    }
}
