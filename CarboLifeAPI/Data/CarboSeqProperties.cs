using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboSeqProperties
    {
        public string propertyName { get; set; }

        //Total
        [NonSerialized]
        public string calcResult;
        public double value { get; set; }

        public CarboSeqProperties()
        {
            propertyName = "";

            value = 0;
            calcResult = "";

        }

        public void calculate()
        {
        }
    }
}
