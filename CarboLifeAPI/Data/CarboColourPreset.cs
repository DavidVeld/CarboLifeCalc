using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboColourPreset
    {


        public string name { get; set; }
        public CarboColour outmin { get; set;}
        public CarboColour min { get; set; }
        public CarboColour mid { get; set; }
        public CarboColour max { get; set; }
        public CarboColour outmax { get; set; }

        /// <summary>
        /// This is the location of the midpoint if needed.
        /// </summary>
        int mid_Position { get; set; }

        /// <summary>
        /// The Carbo Colour Prest Stores the user defined colour range for model painting
        /// System.Drawing.Color
        /// </summary>
        public CarboColourPreset()
        {
            name = "New Preset";
            outmin = new CarboColour(255, 0, 0, 255);
            min = new CarboColour(255, 141, 241, 41);
            mid = new CarboColour(255, 242, 116, 40);
            max = new CarboColour(255, 240, 40, 9); 
            outmax = new CarboColour(255, 250, 0, 0);
            mid_Position = 50;
        }
    }
}
