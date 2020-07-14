using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data.Superseded
{
    [Serializable]
    [Obsolete]
    public class CarboProperty
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }

        public CarboProperty()
        {
            PropertyName = "";
            Value = "";
        }
    }
}
