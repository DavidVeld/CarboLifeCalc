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
    public class CarboA5Properties
    {
        public string name { get; set; }

        //User Variables
       
        //Results
        public double value { get; set; }
        public string calcResult { get; set; }

        public CarboA5Properties()
        {
            name = "";

            value = 0;
            calcResult = "";

        }

        public void calculate()
        {
            //Calculate total nr of trips;
            string calcResult = "";

            try
            {
                //double conversion = 1;

                calcResult += "A5 calculation to be implemented " + System.Environment.NewLine;
                calcResult += "Set Value by user as: " + value + "kgCo2/kg" + System.Environment.NewLine; 

                this.calcResult = calcResult;
            }
            catch (Exception ex)
            { }
        }
    }
}
