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
    public class CarboDProperties
    {
        public string name { get; set; }

        //User Variables
       
        //Results
        public double value { get; set; }
        public string calcResult { get; set; }

        public CarboDProperties()
        {
            name = "";
            value = 0;
            calcResult = "";
        }

        public void calculate()
        {
            try
            {
            }
            catch
            { 
            }
        }
    }
}
