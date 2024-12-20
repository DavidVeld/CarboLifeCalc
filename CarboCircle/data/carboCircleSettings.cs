using CarboLifeAPI.Data;
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
        public string MineStyle { get; set; } //Always By Type, or type name if empty
        public string RequiredStyle { get; set; } //Always By Type, or type name if empty

        //existingImportSettings
        public double cutoffbeamLength { get; set; } //600 defaiult
        public double cutoffColumnLength { get; set; } //600 defaiult
        public int VolumeLoss { get; set; }

        public bool MineWalls { get; set; }
        public bool MineSlabs { get; set; }
        public bool MineColumnBeams { get; set; }

        public bool RequireWalls { get; set; }
        public bool RequireSlabs { get; set; }
        public bool RequireColumnBeams { get; set; }


        //matchSettings
        public double depthRange { get; set; }
        public double strengthRange { get; set; }

        //colours
        public CarboColour colour_ReusedMinedData { get; set; }
        public CarboColour colour_ReusedMinedVolumes { get; set; }
        public CarboColour colour_NotReused { get; set; }
        public CarboColour colour_FromReusedData { get; set; }
        public CarboColour colour_FromReusedVolumes { get; set; }
        public CarboColour colour_NotFromReused { get; set; }

        public carboCircleSettings()
        {

            TypeParameterName = string.Empty;
            MineStyle = string.Empty;
            RequiredStyle = string.Empty;
            cutoffbeamLength = 600;
            cutoffColumnLength = 600;

            depthRange = 50;
            strengthRange = .10;
            VolumeLoss = 25;

            MineWalls = false;
            MineSlabs = false;
            MineColumnBeams = true;

            RequireWalls = false;
            RequireSlabs = false;
            RequireColumnBeams = true;

            colour_ReusedMinedData = new CarboColour();
            colour_ReusedMinedVolumes = new CarboColour();
            colour_NotReused = new CarboColour();
            colour_FromReusedData = new CarboColour();
            colour_FromReusedVolumes = new CarboColour();
            colour_NotFromReused = new CarboColour();

        }


        internal carboCircleSettings Copy()
        {
            carboCircleSettings clone = new carboCircleSettings
            {
                //Import settings
                TypeParameterName = this.TypeParameterName,
                MineStyle= this.MineStyle,
                RequiredStyle= this.RequiredStyle,
                cutoffbeamLength = this.cutoffbeamLength,
                cutoffColumnLength = this.cutoffColumnLength,

                depthRange = this.depthRange,
                strengthRange = this.strengthRange,
                VolumeLoss = this.VolumeLoss,

                MineWalls = this.MineWalls,
                MineSlabs= this.MineSlabs,
                MineColumnBeams= this.MineColumnBeams,

                RequireWalls = this.RequireWalls,
                RequireSlabs = this.RequireSlabs,
                RequireColumnBeams = this.RequireColumnBeams,

                //Colours
                colour_ReusedMinedData = this.colour_ReusedMinedData.Copy(),
                colour_ReusedMinedVolumes = this.colour_ReusedMinedVolumes.Copy(),
                colour_NotReused = this.colour_NotReused.Copy(),
                colour_FromReusedData = this.colour_FromReusedData.Copy(),
                colour_FromReusedVolumes = this.colour_FromReusedVolumes.Copy(),
                colour_NotFromReused= this.colour_NotReused.Copy()

            };

            return clone;


        }
    }
}
