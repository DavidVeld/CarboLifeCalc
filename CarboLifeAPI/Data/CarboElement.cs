using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboElement : ICloneable
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
        public string MaterialCategoryName { get; set; }
        public string Grade { get; set; }

        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string AdditionalData { get; set; }
        public double Area { get; set; }

        public string Correction { get; set; }

        public double Volume { get; set; }
        public double Volume_Total { get; set; }

        public double Mass { get; set; }
        public double Density { get; set; }
        public double Level { get; set; }
        public string LevelName { get; set; }
        public double ECI { get; set; }
        public double EC { get; set; }


        public double Volume_Cumulative { get; set; }
        public double ECI_Cumulative { get; set; }
        public double EC_Cumulative { get; set; }

        public bool isDemolished { get; set; }
        public bool isExisting { get; set; }
        public bool isSubstructure { get; set; }
        public bool includeInCalc { get; set; }
        public double rcDensity { get; set; }
        public string GUID { get; set; }


        //public CarboMaterial Material {get;set; }

        [NonSerialized]
        ///This field is used when revit updates a model, improvements for multibuildps are required
        public bool isUpdated;
        public CarboElement()
        {
            Id = -999;
            Name = "";
            MaterialName = "Other";
            MaterialCategoryName = "";
            Grade = "";

            Category = "Other";
            SubCategory = "";
            AdditionalData = "";
            Area = 0;

            Correction = "";
            rcDensity = 0;
            GUID = "";

            LevelName = "";
            
            Volume = 0;
            Volume_Total = 0;

            Level = 0;
            Density = 0;

            ECI = 0;
            EC = 0;

            ECI_Cumulative = 0;
            EC_Cumulative = 0;
            Volume_Cumulative = 0;

            isDemolished = false;
            isExisting = false;
            isUpdated = false;
            isSubstructure = false;
            includeInCalc = true;
        }

        
        public void setMaterial(CarboMaterial carboMaterial)
        {
            //this.Material = carboMaterial;
            MaterialName = carboMaterial.Name;
            MaterialCategoryName = carboMaterial.Category;
            Density = carboMaterial.Density;
            ECI = carboMaterial.ECI;
            Grade = carboMaterial.Grade;
        }
        
        internal void Calculate(CarboMaterial material)
        {
            if(material != null)
            {
                ///This calculation can be made;
                ECI = material.ECI;
                EC = material.ECI * (material.Density * Volume_Total);
                Mass = material.Density * Volume_Total;
                Density = material.Density;
                Grade = material.Grade;
            }

        }

        public CarboElement CopyMe()
        {
            CarboElement clone = new CarboElement();

            clone.Id = this.Id;
            clone.Name = this.Name;
            //Imported Material Name
            clone.MaterialName = this.MaterialName;
            //Matched To Material Name
            clone.CarboMaterialName = this.CarboMaterialName;
            clone.MaterialCategoryName = this.MaterialCategoryName;

            clone.Category = this.Category;
            clone.SubCategory = this.SubCategory;
            clone.AdditionalData = this.AdditionalData;
            clone.Volume = this.Volume;
            clone.Volume_Total = this.Volume_Total;

            clone.Area = this.Area;

            clone.Mass = this.Mass;
            clone.Density = this.Density;
            clone.Level = this.Level;
            clone.LevelName = this.LevelName;
            clone.Correction = this.Correction;
            clone.Grade = this.Grade;
            clone.rcDensity = this.rcDensity;
            clone.GUID = this.GUID;

            clone.ECI = this.ECI;
            clone.EC = this.EC;
            clone.ECI_Cumulative = this.ECI_Cumulative;
            clone.EC_Cumulative = this.EC_Cumulative;
            clone.Volume_Cumulative = this.Volume_Cumulative;

            clone.isDemolished = this.isDemolished;
            clone.isExisting = this.isExisting;
            clone.isSubstructure = this.isSubstructure;
            clone.includeInCalc = this.includeInCalc;

            return clone;
        }

        public object Clone()
        {
            CarboElement clone = new CarboElement();

            clone.Id = this.Id;
            clone.Name = this.Name;
            //Imported Material Name
            clone.MaterialName = this.MaterialName;
            //Matched To Material Name
            clone.CarboMaterialName = this.CarboMaterialName;
            clone.MaterialCategoryName = this.MaterialCategoryName;

            clone.Category = this.Category;
            clone.SubCategory = this.SubCategory;
            clone.AdditionalData = this.AdditionalData;
            clone.Volume = this.Volume;
            clone.Volume_Total = this.Volume_Total;

            clone.Area = this.Area;

            clone.Mass = this.Mass;
            clone.Density = this.Density;
            clone.Level = this.Level;
            clone.LevelName = this.LevelName;
            clone.Correction = this.Correction;
            clone.Grade = this.Grade;
            clone.rcDensity = this.rcDensity;
            clone.GUID = this.GUID;

            clone.isDemolished = this.isDemolished;
            clone.isExisting = this.isExisting;
            clone.isSubstructure = this.isSubstructure;

            clone.ECI = this.ECI;
            clone.EC = this.EC;
            clone.ECI_Cumulative = this.ECI_Cumulative;
            clone.EC_Cumulative = this.EC_Cumulative;
            clone.Volume_Cumulative = this.EC_Cumulative;

            clone.includeInCalc = this.includeInCalc;

            return clone;
        }
    }
}
