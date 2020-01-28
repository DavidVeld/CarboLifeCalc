using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]

    public class CarboGroup
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Description { get; set; }
        public double Volume { get; set; }
        public double TotalVolume { get; set; }
        public string Correction { get; set; }
        //Calculated Values;
        public double Density { get; set; }
        public double Mass { get; set; }

        //Calculated Values
        //public double EEI { get; set; }
        public double ECI { get; set; }
        //public double EE { get; set; }
        public double EC { get; set; }
        public double PerCent { get; set; }

        public CarboMaterial Material {get;set;}

        public List<CarboElement> AllElements { get; set; }

        public bool isDemolished { get; set; }
        public bool isSubstructure { get; set; }


        public CarboGroup()
        {
            Id = -999;
            MaterialName = "";
            Category = "";
            SubCategory = "";
            Description = "";
            Correction = "";

            Volume = 0;
            Density = 0;
            Mass = 0;

            ECI = 0;
            EC = 0;

            PerCent = 0;
            Material = new CarboMaterial();
            AllElements = new List<CarboElement>();

            isDemolished = false;
            isSubstructure = false;
        }

        internal void RefreshValuesFromElements()
        {
            //Reset Material Values;

            Density = 0;
            Mass = 0;
            ECI = 0;
            EC = 0;

            PerCent = 0;

            //Get total Volumes;
            if (AllElements.Count > 0)
            {
                foreach (CarboElement ce in AllElements)
                {
                    //MaterialName = ce.MaterialName;
                    //Category = ce.Category;

                    Volume += ce.Volume;

                    //EE = 0;
                    EC = 0;
                }
            }

            //Set Material Properties
            MaterialName = Material.Name;
            Density = Material.Density;
            //EEI = Material.EEI;
            ECI = Material.ECI;

            Mass = Material.Density * Volume;

        }

        public CarboGroup(int id, string materialName, string category, string description, double volume, double density, double mass, double eei, double eci, double ee, double ec)
        {
            Id = id;
            MaterialName = materialName;
            Category = category;
            Description = description; 

            Volume = volume;
            Density = density;
            Mass = mass;

            //EEI = eei;
            ECI = eci;
            //EE = ee;
            EC = ec;

            PerCent = 0;
            Material = new CarboMaterial();
            AllElements = new List<CarboElement>();
        }

        internal void setMaterial(CarboMaterial material)
        {
            this.MaterialName = material.Name;
            this.Material = material;
            CalculateTotals();
        }

        public CarboGroup(CarboElement carboElement)
        {
            Id = -999;
            MaterialName = carboElement.MaterialName;
            Category = carboElement.Category;
            SubCategory = carboElement.SubCategory;

            Description = "";

            Volume = carboElement.Volume;
            Density = 0;
            Mass = 0;

            //EEI = 0;
            ECI = 0;
            //EE = 0;
            EC = 0;

            PerCent = 0;
            Material = new CarboMaterial();
            Material = carboElement.Material;

            AllElements = new List<CarboElement>();

            AllElements.Add(carboElement);
            isDemolished = carboElement.isDemolished;
            isSubstructure = carboElement.isSubstructure;
        }

        internal void SetPercentageOf(double eCTotal)
        {
            if (EC > 0)
            {
                PerCent = (EC / eCTotal) * 100;
                PerCent = Math.Round(PerCent, 2);
            }
            else
            {
                PerCent = 0;
            }
        }

        public void CalculateTotals()
        {
            Material.CalculateTotals();
            //Clear Values
            MaterialName = Material.Name;
            //EEI = Material.EEI;
            ECI = Material.ECI;
            Density = Material.Density;
            
            if (AllElements != null)
            {
                if (AllElements.Count > 0)
                {
                    Volume = 0;
                    foreach (CarboElement ce in AllElements)
                    {
                        Volume += ce.Volume;
                    }
                }
            }

            //Calculate the real volume based on a correction if required. 
            if (Utils.isValidExpression(Correction) == true)
            {
                string volumeStr = Volume.ToString();
                StringToFormula stf = new StringToFormula();
                double result = stf.Eval(volumeStr + Correction);
                TotalVolume = result;
            }
            else
            {
                TotalVolume = Volume;
            }
            

            //Calculate Valuesl
            Mass = TotalVolume * Density;
            //EE = Mass * EEI;
            EC = (Mass * ECI) / 1000;

        }

        internal void TrucateElements()
        {
            if(AllElements.Count > 0)
            {
                this.Description += " Removed: " + this.AllElements[0].Name;
                AllElements.Clear();
            }
        }

        public CarboGroup Copy()
        {
            CarboGroup result = new CarboGroup();
            result.Category = this.Category;
            result.SubCategory = this.SubCategory;
            result.Material = this.Material;
            result.MaterialName = this.MaterialName;
            result.AllElements = this.AllElements;
            result.Volume = this.Volume;

            result.RefreshValuesFromElements();

            return result;

        }
    }
}
