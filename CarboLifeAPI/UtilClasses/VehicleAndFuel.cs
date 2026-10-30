using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class Vehicle
    {
        public string name { get; set; }
        public double maxVolume { get; set; }
        public double maxLoad { get; set; }
        public double kmPer { get; set; }
        public double carboCostNew { get; set; }
        public double totalDistance { get; set; }
        public string defaultFuel { get; set; }
        public string description { get; set; }
        /// <summary>
        /// Average gCO₂e per tonne of goods per km, used for a vehicle without a load or volume
        /// capacity (rail, shipping). See CarboA4Properties.
        /// </summary>
        public double gCO2PerTonneKm { get; set; }

        public Vehicle()
        {
            name = "Truck";
            maxVolume = 96;
            maxLoad = 22000;
            kmPer = 3.85;
            carboCostNew = 280000;
            totalDistance = 250000;
            defaultFuel = "Diesel";
            description = "An unspecified Vehicle";
            gCO2PerTonneKm = 0;
        }

        public Vehicle Copy()
        {
            return (Vehicle)this.MemberwiseClone();
        }
    }

    [Serializable]
    public class Fuel
    {
        public string name { get; set; }
        public double emission { get; set; }
        public double production { get; set; }
        public string unit { get; set; }

        public Fuel()
        {
            name = "Diesel";
            emission = 2.62;
            production = 0.4;
            unit = "l";
        }

        public Fuel Copy()
        {
            return (Fuel)this.MemberwiseClone();
        }
    }

}

