using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboCircle
{
    [Serializable]
    public class carboCircleElement
    {

        public int id { get; set; }
        public string name { get; set; }
        //Imported Material Name
        public string materialName { get; set; }
        //Matched To Material Name
        public string grade { get; set; }
        public double length { get; set; }
        public int quality { get; set; }

        public string standardName { get; set; }
        //Imported Material Name
        public double depth { get; set; }
        public string standardCategory { get; set; }
        public double Iz { get; set; }
        public double Wz { get; set; }
        public string GUID { get; set; }
        public string matchGUID { get; set; }



        public carboCircleElement()
        {
            id = -999;
            name = "";
            materialName = "Other";
            grade = "";
            length = 0;
            quality = 1;

            standardName = "";
            standardCategory = "";
            depth = 0;
            Iz = 0;
            Wz = 0;
            GUID = "";
            matchGUID = "";
         }


    public carboCircleElement Copy()
        {
            carboCircleElement clone = new carboCircleElement();

            clone.id = this.id;
            clone.name = this.name;
            clone.materialName = this.name;
            clone.grade = this.name;
            clone.depth = this.depth;
            clone.length = this.length;
            clone.quality = this.quality;
            clone.Iz = this.Iz;
            clone.Wz = this.Wz;
            clone.GUID = this.GUID;
            clone.matchGUID = this.matchGUID;

            return clone;
        }
    }
}

