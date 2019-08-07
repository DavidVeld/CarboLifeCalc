using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    public class CarboElement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MaterialName { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public double Volume { get; set; }
        public double Level { get; set; }
        public bool isDemolished { get; set; }

        public CarboMaterial Material {get;set; }

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
            Material = new CarboMaterial();
        }

        public void setMaterial(CarboMaterial carboMaterial)
        {
            this.Material = carboMaterial;
            MaterialName = carboMaterial.Name;
        }
    }
}
