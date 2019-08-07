using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    public class CarboLevel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Level { get; set; }
        
        public CarboLevel()
        {
            Id = -999;
            Name = "";
            Level = 0;
        }

    }
}
