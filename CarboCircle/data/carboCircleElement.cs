using Autodesk.Revit.DB;
using CarboLifeAPI;
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
        public string humanId { get; set; }
        public string category { get; set; }
        public string name { get; set; }
        //Imported Material Name
        public string materialName { get; set; }
        public string materialClass { get; set; }

        //Matched To Material Name
        public double length { get; set; }
        public double volume { get; set; }
        public double netLength { get; set; }
        public double netVolume { get; set; }

        public string grade { get; set; }
        public int quality { get; set; }
        public string GUID { get; set; }

        public bool isVolumeElement { get; set; }

        //The Below are taken from standardized Database
        public string standardName { get; set; }
        public double standardDepth { get; set; }
        public string standardCategory { get; set; }
        public double Iy { get; set; }
        public double Wy { get; set; }
        public double Iz { get; set; }
        public double Wz { get; set; }
        public string matchGUID { get; set; }
       


        public carboCircleElement()
        {
            id = -999;
            humanId = "";
            name = "";
            materialName = "Other";
            grade = "";
            length = 0;
            quality = 1;
            category = "";
            materialClass = "";

            isVolumeElement = false;
            volume = 0;

            standardName = "";
            standardCategory = "";
            standardDepth = 0;
            Iy = 0;
            Wy = 0;
            Iz = 0;
            Wz = 0;
            GUID = "";
            matchGUID = "";
         }


    public carboCircleElement Copy()
        {
            carboCircleElement clone = new carboCircleElement
            {
                id = this.id,
                humanId = this.humanId,
                name = this.name,
                materialName = this.name,
                grade = this.name,
                length = this.length,
                netLength = this.netLength,
                netVolume = this.netVolume,

                quality = this.quality,
                category = this.category,
                materialClass = this.materialClass,

                isVolumeElement = this.isVolumeElement,
                volume = this.volume,
                
                standardName = this.standardName,
                standardCategory = this.standardCategory,
                standardDepth = this.standardDepth,

                Iy = this.Iy,
                Wy = this.Wy,
                Iz = this.Iz,
                Wz = this.Wz,
                GUID = this.GUID,
                matchGUID = this.matchGUID
            };

            return clone;
        }
    }
}

