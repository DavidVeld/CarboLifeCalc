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
    }

}

