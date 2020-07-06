using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    public class EoLElement
    {
        public string Material { get; set; }

        public double Incineration { get; set; }
        public double IncinerationP { get; set; }
        public double Landfill { get; set; }
        public double LandfillP { get; set; }

        public EoLElement()
        {
            Material = "";
            Incineration = 0;
            IncinerationP = 0;
            Landfill = 0;
            LandfillP = 0;
        }

        public void Calculate()
        {
            //
        }
    }
}
