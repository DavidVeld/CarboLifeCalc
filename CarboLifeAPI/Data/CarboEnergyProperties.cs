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
        public double value { get; set; }
        public double electricityPerYear { get; set; }
        public double waterPerYear { get; set; }
        public double gasPerYear { get; set; }
        public string comment { get; set; }

        public CarboEnergyProperties()
        {
            propertyName = "CarboEnergyProperties";
            value = 0;
            electricityPerYear = 0;
            waterPerYear = 0;
            gasPerYear = 0;
            comment = "";
        }

        public void calculate()
        {

        }
    }
}
