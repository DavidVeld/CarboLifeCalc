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
        public string description { get; set; }

        public double distanceToSite { get; set; }
        public double materialDensity { get; set; }
        public double emptyRun { get; set; }
        public bool emptyReturn { get; set; }
        
        //User Variables
        public Vehicle vehicleSettings { get; set; }
        public Fuel vehicleFuel { get; set; }

        //Results
        public double value { get; set; }

        [NonSerialized]
        public string calcResult;

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

        public void calculate()
        {
            //Calculate total nr of trips;
            string calcResult = "";

            //Check if volume or mass is transport limit
            // If we transport 1000000
            
            double transportDensity = materialDensity * (emptyRun / 100);

            double volumeFull = vehicleSettings.maxLoad / transportDensity;
            double weightFull = vehicleSettings.maxVolume * transportDensity;
            double volumecheck = vehicleSettings.maxVolume - volumeFull;
            double loadcheck = vehicleSettings.maxLoad - weightFull;

            double totaldistance = distanceToSite;

            if (emptyReturn == true)
                totaldistance = totaldistance * 2;

            bool useVolume = false;

            if (weightFull < vehicleSettings.maxLoad)
                useVolume = true;
  
            double loadPerTrip = 0;

            if (useVolume == true)
                loadPerTrip = transportDensity * vehicleSettings.maxVolume;
            else
                loadPerTrip = vehicleSettings.maxLoad * (emptyRun / 100);

            //Fuel Calcs
            double emmissionPerkm = (vehicleFuel.emission * 1000) / vehicleSettings.kmPer;
            double productionPerkm = (vehicleFuel.production * 1000) / vehicleSettings.kmPer;
            double totalFuelEmissionPerkm = emmissionPerkm + productionPerkm;
            double totalFuelEmission = totalFuelEmissionPerkm * totaldistance;
            double totalFuelEmissionkg = totalFuelEmission /1000;
            double totalFuelEmissionPerkg = totalFuelEmissionkg/loadPerTrip;

            //Build Cost.
            double totalBuildcost = vehicleSettings.carboCostNew;
            double totalBuildDistance = vehicleSettings.totalDistance;
            double buildCostperKm = totalBuildcost / totalBuildDistance;
            double buildCostTotal = buildCostperKm * totaldistance;
            double buildCostperKg = buildCostTotal / loadPerTrip;

            double total = buildCostperKg + totalFuelEmissionPerkg;

            try
            {
                //double conversion = 1;
              

                calcResult += "Calculation to get transport value: " + System.Environment.NewLine; ;
                calcResult += "Distance to site = " + distanceToSite + System.Environment.NewLine; ;
                calcResult += "Material Density = " + materialDensity + System.Environment.NewLine;
                calcResult += "Empty run =" + emptyRun + System.Environment.NewLine;
                calcResult += System.Environment.NewLine;
                calcResult += "Transport Density = " + materialDensity +  " x " + emptyRun + " % " + System.Environment.NewLine;
                calcResult += "Transport Load = " + vehicleSettings.maxLoad + " x " + emptyRun + " % " + System.Environment.NewLine;


                if (emptyReturn == true)
                {
                    calcResult += "The vehicle WILL return to its starting point empty" + System.Environment.NewLine;
                    calcResult += "The total distance = " + totaldistance + " km" + System.Environment.NewLine;

                }

                else
                {
                    calcResult += "The vehicle will NOT return empty to its starting point" + System.Environment.NewLine;
                    calcResult += "The total distance = " + totaldistance + " km" + System.Environment.NewLine;
                }

                //
                calcResult += "Summary: we are going to drive a " + name + " using " + vehicleFuel.name + " " + totaldistance + " km " + System.Environment.NewLine;

                calcResult += "A full " + name + " can carry " + volumeFull + " m³ (until max load is reached)" + System.Environment.NewLine;
                calcResult += "Or " + weightFull + " kg (until max volume is reached)" + System.Environment.NewLine;

                if (useVolume == true)
                {
                    calcResult += "We will use the volume capacity as a maximum threshold" + System.Environment.NewLine;
                    calcResult += "The vehicle will be fully loaded using: "  + vehicleSettings.maxVolume + " m³" +System.Environment.NewLine;
                    calcResult += "The vehicle be carrying: " + loadPerTrip + " kg/trip" + System.Environment.NewLine;

                }
                else
                {
                    calcResult += "We will use the load capacity as a maximum threshold" + System.Environment.NewLine;
                    calcResult += "The vehicle will be fully loaded using: " + loadPerTrip + " kg" + System.Environment.NewLine;
                }
                calcResult += System.Environment.NewLine;
                calcResult += "Vehicle Fuel Consumption" + System.Environment.NewLine;

                calcResult += "The vehicle consumes: " + vehicleSettings.kmPer + " " + vehicleFuel.unit + "/km" + System.Environment.NewLine;
                calcResult += "Carbon contents of " + vehicleFuel.name + " = " + vehicleFuel.emission + " CO₂e/" + vehicleFuel.unit + System.Environment.NewLine;
                // double emmissionPerkm = (vehicleFuel.emission * 1000) / vehicleSettings.kmPer;

                calcResult += "We will therefore use: (" + vehicleFuel.emission + " CO₂e/" + vehicleFuel.unit + " x 1000) /" + vehicleSettings.kmPer + " km/" + vehicleFuel.unit + " = " + emmissionPerkm + " gCO₂/km"  + System.Environment.NewLine;

                calcResult += "To Produce one " + vehicleFuel.unit  + " of " + vehicleFuel.name + " you need: " + vehicleFuel.production + " CO₂e/" + vehicleFuel.unit + System.Environment.NewLine;
                calcResult += "We will therefore use: (" + vehicleFuel.production + " CO₂e/" + vehicleFuel.unit + " x 1000) /" + vehicleSettings.kmPer + " km/" + vehicleFuel.unit + " = " + productionPerkm + " gCO₂/km" + System.Environment.NewLine;
                calcResult += emmissionPerkm + " + " + productionPerkm + " = " + totalFuelEmissionPerkm + " gCO₂/km" + System.Environment.NewLine;
                calcResult += totalFuelEmissionPerkm + " gCO₂/km x " + totaldistance + " km = " + totalFuelEmission + " gCO₂/trip" + System.Environment.NewLine;
                calcResult += totalFuelEmission + " / " + "1000" + "  = " + totalFuelEmissionkg + " kgCO₂/trip" + System.Environment.NewLine;
                calcResult += totalFuelEmissionkg + " kgCO₂/trip / " + loadPerTrip + " kg = " + totalFuelEmissionPerkg + " gCO₂/kg" + System.Environment.NewLine;

                calcResult += System.Environment.NewLine;
                calcResult += "Build cost" + System.Environment.NewLine;
                calcResult += "Each vehicle needs to be build, and therefore its carbon costs need to be included" + System.Environment.NewLine;
                calcResult += "Total carbon costs are estimated to be: " + totalBuildcost  + " kgCO₂" + System.Environment.NewLine;
                calcResult += "On average this vehicle will go for: " + totalBuildDistance + " km" + System.Environment.NewLine;
                calcResult += "Per km: " + totalBuildcost + " / " + totalBuildDistance + " = " + buildCostperKm + " kgCO₂/km" + System.Environment.NewLine;
                calcResult += "Total emission per trip: " + buildCostperKm + " x " + totaldistance + " = " + buildCostTotal + " kgCO₂/km" + System.Environment.NewLine;
                calcResult += "Total emission per kg: " + buildCostTotal + " / " + loadPerTrip + " = " + buildCostperKg + " kgCO₂/km" + System.Environment.NewLine;

                calcResult += System.Environment.NewLine;

                calcResult += "Total emission per kg: " + totalFuelEmissionkg + " / " + buildCostperKg + " = " + total + " kgCO₂/kg" + System.Environment.NewLine;

                this.value = total;
                this.calcResult = calcResult;
            }
            catch
            { 
            }
        }



    }
}
