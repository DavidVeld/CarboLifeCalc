using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public double Volume { get; set; }
        public double Mass { get; set; }
        public double Density { get; set; }
        public double Level { get; set; }
        public double ECI { get; set; }
        public double EC { get; set; }
        public double ECI_Total { get; set; }
        public double EC_Total { get; set; }
        public double Volume_Total { get; set; }
        public bool isDemolished { get; set; }
        public bool isExisting { get; set; }
        public bool isSubstructure { get; set; }
        public bool includeInCalc { get; set; }       



        //public CarboMaterial Material {get;set; }

        [NonSerialized]
        ///This field is used when revit updates a model, improvements for multibuildps are required
        public bool isUpdated;
        public CarboElement()
        {
            Id = -999;
            Name = "";
            MaterialName = "Other";
            Category = "Other";
            SubCategory = "";
            Volume = 0;
            Level = 0;
            Density = 0;

            ECI = 0;
            EC = 0;
            ECI_Total = 0;
            EC_Total = 0;
            Volume_Total = 0;

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
            Density = carboMaterial.Density;
            ECI = carboMaterial.ECI;
        }
        
        internal void Calculate(CarboMaterial material)
        {
            if(material != null)
            {
                ///This calculation can be made;
                ECI = material.ECI;
                EC = material.ECI * (material.Density * Volume);
                Mass = material.Density * Volume;
            }

        }
        
        internal CarboElement CopyMe()
        {
            CarboElement clone = new CarboElement();

            clone.Id = this.Id;
            clone.Name = this.Name;
            //Imported Material Name
            clone.MaterialName = this.MaterialName;
            //Matched To Material Name
            clone.CarboMaterialName = this.CarboMaterialName;

            clone.Category = this.Category;
            clone.SubCategory = this.SubCategory;
            clone.Volume = this.Volume;
            clone.Mass = this.Mass;
            clone.Density = this.Density;
            clone.Level = this.Level;
            clone.isDemolished = this.isDemolished;
            clone.isExisting = this.isExisting;
            clone.isSubstructure = this.isSubstructure;

            clone.ECI = this.ECI;
            clone.EC = this.EC;
            clone.ECI_Total = this.ECI_Total;
            clone.EC_Total = this.EC_Total;
            clone.Volume_Total = this.Volume_Total;

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

            clone.Category = this.Category;
            clone.SubCategory = this.SubCategory;
            clone.Volume = this.Volume;
            clone.Mass = this.Mass;
            clone.Density = this.Density;
            clone.Level = this.Level;
            clone.isDemolished = this.isDemolished;
            clone.isExisting = this.isExisting;
            clone.isSubstructure = this.isSubstructure;

            clone.ECI = this.ECI;
            clone.EC = this.EC;
            clone.ECI_Total = this.ECI_Total;
            clone.EC_Total = this.EC_Total;
            clone.Volume_Total = this.Volume_Total;

            clone.includeInCalc = this.includeInCalc;

            return clone;
        }
    }
}
