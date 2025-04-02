using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboEnergyProperties
    {
        public string propertyName { get; set; }

        /// <summary>
        /// Total CO2e during design life in Co2e for tCO2e / 1000
        /// </summary>
        public double value { get; set; }

        /// <summary>
        /// Electricity CO2e/year
        /// </summary>
        public double B6 { get; set; }

        /// <summary>
        /// WaterUsage CO2e/year
        /// </summary>
        public double B7 { get; set; }

        /// <summary>
        /// Electricity Generation CO2e/year
        /// </summary>
        public double D2 { get; set; }


        /// <summary>
        /// Electricity kWh/year
        /// </summary>
        public double ElectricityUsedPerYear;

        /// <summary>
        /// Electricity kWh/year
        /// </summary>
        public double WaterUsedPerYear;

        /// <summary>
        /// Electricity kWh/year
        /// </summary>
        public double ElectricitygeneratedPerYear;

        public double CO2CostPerkWh { get; set; }
        public double CO2CostPerm3 { get; set; }

        /// <summary>
        /// % of decarbonisation per year untill reaches 0;
        /// </summary>
        public double decabornisationFactor { get; set; }

        public string comment { get; set; }

        public CarboEnergyProperties()
        {
            propertyName = "CarboEnergyProperties";
            value = 0;
            B6 = 0;
            B7 = 0;
            D2 = 0;

            ElectricityUsedPerYear = 100;
            WaterUsedPerYear = 550;
            ElectricitygeneratedPerYear = 0;
            CO2CostPerkWh = 0.200;
            CO2CostPerm3 = 0.0015;

            decabornisationFactor = 2;

            comment = "";
        }

        //This sets the total energy used by the project for given year
        public void calculate(int years)
        {
            //If this factor is 0, each year will have the same embodied carbon value.
            double factorPeryear = decabornisationFactor / 100; //normalise to percent

            double Percent = 1;
            double reduction = 1;

            B6 = 0;
            B7 = 0;
            D2 = 0;

            for (int i = 0; i < years; i++)
            {
                B6 += (ElectricityUsedPerYear * CO2CostPerkWh) * Percent;
                B7 += (WaterUsedPerYear * CO2CostPerm3) * Percent;
                D2 += (ElectricitygeneratedPerYear * CO2CostPerkWh) * Percent;

                reduction = 1 - factorPeryear;
                Percent = Percent * reduction;
            }

            value = Math.Round(B6 + B7 - D2);
        }

        /// <summary>
        /// This returns the total consumes energy up to the year specified.
        /// </summary>
        /// <param name="year">the year you required the used consumed energy and water for</param>
        /// <returns></returns>
        public double getTotalValue(int year)
        {
            double result = 0;

            //If this factor is 0, each year will have the same embodied carbon value.
            double factorPeryear = decabornisationFactor / 100; //normalise to percent

            double Percent = 1;
            double reduction = 1;

            double B6_local = 0;
            double B7_local = 0;
            double D2_local = 0;

            for (int i = 0; i < year; i++)
            {
                B6_local += (ElectricityUsedPerYear * CO2CostPerkWh) * Percent;
                B7_local += (WaterUsedPerYear * CO2CostPerm3) * Percent;
                D2_local += (ElectricitygeneratedPerYear * CO2CostPerkWh) * Percent;

                reduction = 1 - factorPeryear;
                Percent = Percent * reduction;

            }

            result = Math.Round(B6_local + B7_local - D2_local);
            
            return result;
        }

    }
}
