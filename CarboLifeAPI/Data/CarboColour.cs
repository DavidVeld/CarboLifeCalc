using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboColour
    {
        public byte a { get; set; }
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }


        public CarboColour()
        {
            a = 128;
            r = 128;
            g = 128;
            b = 128;
        }

        public CarboColour(byte a, byte r, byte g, byte b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public CarboColour Copy()
        {
            CarboColour clone = new CarboColour
            {
                a = this.a,
                r = this.r,
                g = this.g,
                b = this.b
            };

            return clone;
        }
    }
}
