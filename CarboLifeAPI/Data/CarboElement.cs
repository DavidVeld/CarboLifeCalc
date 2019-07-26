using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    public class CarboElement
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public string Category { get; set; }
        public string CarboCategory { get; set; }
        public double Volume { get; set; }
        public double Level { get; set; }

        public CarboMaterial material {get;set; }

        public CarboElement()
        {
            Id = -999;
            MaterialName = "Other";
            Category = "Other";
            CarboCategory = "";
            Volume = 0;
            Level = 0;
            material = new CarboMaterial();
        }
    }
}
