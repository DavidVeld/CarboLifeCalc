using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboMapElement
    {
        public string revitName { get; set; }
        public string category { get; set; }
        public string carboNAME { get; set; }

        public CarboMapElement()
        {
            revitName = "";
            category = "";
            carboNAME = "";
        }

    }
}
