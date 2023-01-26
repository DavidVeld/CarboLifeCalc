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
        public int sequestrationPeriod { get; set; }
        public double value { get; set; }

        //Total
        [NonSerialized]
        public string calcResult;

        public CarboSeqProperties()
        {
            propertyName = "";

            value = 0;
            calcResult = "";
            sequestrationPeriod = 40;
        }

        public void calculate()
        {

        }
    }
}
