using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboCircle.data
{
    public class carboCircleSettings
    {
        //Project Parameters


        //generalImportSettings
        public string TypeParameterName { get; set; } //Always By Type, or type name if empty

        //existingImportSettings
        public double cutoffbeamLength { get; set; } //600 defaiult
        public double cutoffColumnLength { get; set; } //600 defaiult

        //proposedImportSettings


        //matchSettings
        public double depthRange { get; set; }
        public double strengthRange { get; set; }
        public int VolumeLoss { get; internal set; }
        public bool MineWalls { get; internal set; }

        public carboCircleSettings()
        {

            TypeParameterName = string.Empty;
            cutoffbeamLength = 600;
            cutoffColumnLength = 600;

            depthRange = 50;
            strengthRange = .10;
            VolumeLoss = 25;
        }
    }
}
