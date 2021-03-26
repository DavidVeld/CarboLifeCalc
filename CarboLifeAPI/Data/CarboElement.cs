using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboElement
    {
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
        public bool isDemolished { get; set; }
        public bool isExisting { get; set; }
        public bool isSubstructure { get; set; }

        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
        public double ECI { get; set; }
        public double EC { get; set; }
        public double ECI_Total { get; set; }
        public double EC_Total { get; set; }
        public double Volume_Total { get; set; }
       

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
            isDemolished = false;
            isExisting = false;
            //Material = new CarboMaterial(); removed 23.03.21
            r = 0;
            g = 0;
            b = 0;
            ECI = 0;
            EC = 0;
            ECI_Total = 0;
            EC_Total = 0;
            Volume_Total = 0;
            isUpdated = false;
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
        
    }
}
