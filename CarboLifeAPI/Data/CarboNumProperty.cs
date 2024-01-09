using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data.Superseded
{
    [Serializable]
    public class CarboNumProperty
    {
        public string PropertyName { get; set; }
        public double Value { get; set; }

        public CarboNumProperty()
        {
            PropertyName = "";
            Value = 0;
        }
    }
}
