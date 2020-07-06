using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CarboLifeAPI;
using CarboLifeAPI.Data;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboA4Properties
    {
        public string name { get; set; }

        public double capacity { get; set; }
        public double construction { get; set; }
        public double maxDistance { get; set; }
        public double emissionPerKm { get; set; }
        public double density { get; set; }

        //User Variables
        public double distanceToSite { get; set; }
       
        //Results
        public double value { get; set; }
        public string calcResult { get; set; }

        public CarboA4Properties()
        {
            name = "";

            capacity = 0;
            construction = 0;
            maxDistance = 0;
            emissionPerKm = 0;

            distanceToSite = 0;
            value = 0;
        }

        public void calculate()
        {
            //Calculate total nr of trips;
            string calcResult = "";

            try
            {
                //double conversion = 1;

                string units = " km";

                double costPerTrip = Math.Round(distanceToSite * emissionPerKm);

                double costFromNewVehicle = construction * (distanceToSite / maxDistance);

                double costTotalPerTrip = costFromNewVehicle + costPerTrip;

                double massPerTransport = Math.Round(capacity * density);

                double co2prtkg = Math.Round(((1 / massPerTransport) * costTotalPerTrip), 5);

                calcResult += "This calculation will try to create a CO2 per kg value based on the given parameters." + System.Environment.NewLine;
                calcResult += "One trip costs: " + distanceToSite + units + " x " + emissionPerKm + "kgCo2/" + units + "= " + costPerTrip + " kgCo2" + System.Environment.NewLine;
                calcResult += System.Environment.NewLine;
                calcResult += "This will use: " + costFromNewVehicle + "kgCO2 from a new vehicle" + System.Environment.NewLine;
                calcResult += "New vehicle = " + construction + "kgCO2 x (" + distanceToSite + units + " / " + maxDistance + units + ") = " + costFromNewVehicle + "kgCO2" + System.Environment.NewLine;
                calcResult += System.Environment.NewLine;
                calcResult += "Total CEI per trip then is " + costFromNewVehicle + " kgCO2 + " + costPerTrip + " kgCO2 = " + costTotalPerTrip + " kgCO2" + System.Environment.NewLine;

                calcResult += "One " + name + " can carry " + capacity + " m³ per trip" + System.Environment.NewLine;
                calcResult += "This weighs: " + density + " kg/m³ x " + capacity + " m³ = " + massPerTransport + "kg/transport" + System.Environment.NewLine; ;
                calcResult += System.Environment.NewLine;

                calcResult += "So per kg this will cost : (1 /" + massPerTransport + ") x " + costTotalPerTrip + " = " + co2prtkg + " CO2/kg" + System.Environment.NewLine; ;

                this.value = co2prtkg;
                this.calcResult = calcResult;
            }
            catch (Exception ex)
            { }
        }
    }
}
