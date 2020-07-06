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
    public class CarboB1B5Properties
    {
        public double elementdesignlife;
        public double buildingdesignlife;

        public string name { get; set; }

        //User Variables
       
        //Results
        public double value { get; set; }
        public string calcResult { get; set; }

        public CarboB1B5Properties()
        {
            elementdesignlife = 1;
            buildingdesignlife = 1;

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
                value = buildingdesignlife / elementdesignlife;

                calcResult += "B1-B5 calculation:" + System.Environment.NewLine;
                calcResult += "One element of this material will last: " + elementdesignlife + " year" + System.Environment.NewLine;
                calcResult += "The building is intended to last: " + buildingdesignlife + "year" + System.Environment.NewLine;
                calcResult += "This element will therefor be : " + value + "times replaced." + System.Environment.NewLine;
                
                this.calcResult = calcResult;
            }
            catch (Exception ex)
            {
                this.calcResult = ex.Message;
            }
        }
    }
}
