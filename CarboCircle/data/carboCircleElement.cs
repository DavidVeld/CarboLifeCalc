using Autodesk.Revit.DB;
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
        public string category { get; set; }
        public string name { get; set; }
        //Imported Material Name
        public string materialName { get; set; }
        //Matched To Material Name
        public double length { get; set; }
        public double netLength { get; set; }
        public string grade { get; set; }
        public int quality { get; set; }
        public string GUID { get; set; }


        //The Below are taken from standardized Database
        public string standardName { get; set; }
        public double standardDepth { get; set; }
        public string standardCategory { get; set; }
        public double Iy { get; set; }
        public double Wy { get; set; }
        public string matchGUID { get; set; }



        public carboCircleElement()
        {
            id = -999;
            name = "";
            materialName = "Other";
            grade = "";
            length = 0;
            quality = 1;
            category = "";

            standardName = "";
            standardCategory = "";
            standardDepth = 0;
            Iy = 0;
            Wy = 0;
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
            clone.length = this.length;
            clone.quality = this.quality;
            clone.category = this.category;

            clone.standardName = this.standardName;
            clone.standardCategory = this.standardCategory;
            clone.standardDepth = this.standardDepth;

            clone.Iy = this.Iy;
            clone.Wy = this.Wy;
            clone.GUID = this.GUID;
            clone.matchGUID = this.matchGUID;

            return clone;
        }
    }
}

