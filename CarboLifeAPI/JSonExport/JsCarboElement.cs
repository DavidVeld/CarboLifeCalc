using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class JsCarboElement
    {
        /// <summary>
        /// revit id
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; }
        //Imported Material Name
        public string MaterialName { get; set; }
        //Matched To Material Name
        public string CarboMaterialName { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string AdditionalData { get; set; }
        public string Grade { get; set; }
        public double Volume { get; set; }
        public double Area { get; set; }

        /// <summary>
        /// This is the main property to be used for calculations
        /// </summary>
        public double Volume_Total { get; set; }
        public double Mass { get; set; }
        public double Density { get; set; }
        public double Level { get; set; }
        public string LevelName { get; set; }
        public double ECI { get; set; }
        public double EC { get; set; }
        public double RCDensity { get; set; }
        public string Correction { get; set; }

        public double EC_A1A3_Total { get; set; }
        public double EC_A4_Total { get; set; }
        public double EC_A5_Total { get; set; }
        public double EC_B1B7_Total { get; set; }
        public double EC_C1C4_Total { get; set; }
        public double EC_D_Total { get; set; }
        public double EC_Sequestration_Total { get; set; }
        public double EC_Mix_Total { get; set; }
        public bool isDemolished { get; set; }
        public bool isExisting { get; set; }
        public bool isSubstructure { get; set; }
        public bool includeInCalc { get; set; }

        //Cumulative
        public double Volume_Cumulative { get; set; }
        public double EC_Cumulative { get; set; }
        public double ECI_Cumulative { get; set; }
        public string GUID { get; set; }



        public JsCarboElement()
        {
            Id = -999;
            Name = "";
            MaterialName = "Other";
            Category = "Other";
            SubCategory = "";
            AdditionalData = "";
            Grade = "";
            LevelName = "";
            RCDensity = 0;
            Correction = "";

            GUID = "";

            Volume = 0;
            Area = 0;
            Volume_Total = 0;
            Level = 0;
            Density = 0;

            ECI = 0;
            EC = 0;

            Volume_Cumulative = 0;
            ECI_Cumulative = 0;
            EC_Cumulative = 0;

            isDemolished = false;
            isExisting = false;
            isSubstructure = false;
            includeInCalc = true;
        }

    }
}
