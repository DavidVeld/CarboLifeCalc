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
        public Color outmin { get; set; }
        public Color min { get; set; }
        public Color mid { get; set; }
        public Color max { get; set; }
        public Color outmax { get; set; }
        /// <summary>
        /// This is the location of the midpoint if needed.
        /// </summary>
        int mid_Position { get; set; }

        /// <summary>
        /// The Carbo Colour Prest Stores the user defined colour range for model painting
        /// </summary>
        public CarboColourPreset()
        {
            name = "New Preset";
            outmin = System.Drawing.Color.FromArgb(255, 0, 0, 255);
            min = System.Drawing.Color.FromArgb(255, 240, 40, 9);
            mid = System.Drawing.Color.FromArgb(255, 242, 116, 40);
            max = System.Drawing.Color.FromArgb(255, 141, 241, 41);
            outmax = System.Drawing.Color.FromArgb(255, 250, 0, 0);

            /*
            Session_minOutColour = System.Drawing.Color.FromArgb(255, 0, 0, 255);
            Session_maxOutColour = System.Drawing.Color.FromArgb(255, 250, 0, 0);

            Session_minRangeColour = System.Drawing.Color.FromArgb(255, 141, 241, 41);
            Session_midRangeColour = System.Drawing.Color.FromArgb(255, 242, 116, 40);
            Session_maxRangeColour = System.Drawing.Color.FromArgb(255, 240, 40, 9);
             */
        }
    }
}
