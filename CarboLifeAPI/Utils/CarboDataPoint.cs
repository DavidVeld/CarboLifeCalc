using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI
{
    public class CarboDataPoint
    {
            public string Name
            {
                get;
                set;
            }
            public double Value
            {
                get;
                set;
            }


        public CarboDataPoint()
        {
            this.Name = "";
            this.Value = 0;
        }

        public CarboDataPoint(string Name, double Value )
        {
            this.Name = Name;
            this.Value = Value;
        }
    }
}
