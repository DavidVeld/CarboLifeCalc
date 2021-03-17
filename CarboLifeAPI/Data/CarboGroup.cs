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
        //Calculated Values;
        public double Density { get; set; }
        public double Mass { get; set; }

        //Calculated Values
        //public double EEI { get; set; }
        public double ECI { get; set; }
        //public double EE { get; set; }
        public double EC { get; set; }
        public double PerCent { get; set; }

        // Advanced Values
        /// <summary>
        /// A volume correcting value
        /// </summary>
        public string Correction { get; set; }
        public string CorrectionDescription { get; set; }
        /// <summary>
        /// The faste factor
        /// </summary>
        public double Waste { get; set; }
        public string WasteDescription { get; set; }
        /// <summary>
        /// Any additional embodied carbon in this group
        /// </summary>
        public double Additional { get; set; }
        public string AdditionalDescription { get; set; }
        /// <summary>
        /// The replacement value factor
        /// </summary>
        public double B4Factor { get; set; }
        public string B4Description { get; set; }

        public double ComponentLifePeriod { get; set; }
        public double AssetLifePeriod { get; set; }

        /// <summary>
        /// The group material
        ///</summary>
        public CarboMaterial Material {get;set;}

        public List<CarboElement> AllElements { get; set; }

        public bool isDemolished { get; set; }
        public bool isSubstructure { get; set; }
        public double getVolumeECI
        {
            get
            {
                return Density * ECI;
            }
        }

        public CarboGroup()
        {
            Id = -999;
            MaterialName = "";
            Category = "";
            SubCategory = "";
            Description = "";

            Volume = 0;
            Density = 0;
            Mass = 0;

            ECI = 0;
            EC = 0;

            //Correction Formula
            Correction = "";
            CorrectionDescription = "";
            //Waste
            Waste = 0;
            WasteDescription = "";
            //Additional
            Additional = 0;
            AdditionalDescription = "";
            //B4
            B4Factor = 1;
            ComponentLifePeriod = 50;
            AssetLifePeriod = 50;
            B4Description = "";

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
            Volume = 0;
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

            //Correction Formula
            Correction = "";
            CorrectionDescription = "";
            //Waste
            Waste = 0;
            WasteDescription = "";
            //Additional
            Additional = 0;
            AdditionalDescription = "";
            //B4
            B4Factor = 1;
            ComponentLifePeriod = 50;
            AssetLifePeriod = 50;
            B4Description = "";

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

            //Correction Formula
            Correction = "";
            CorrectionDescription = "";
            //Waste
            Waste = 0;
            WasteDescription = "";
            //Additional
            Additional = 0;
            AdditionalDescription = "";
            //B4
            B4Factor = 1;
            ComponentLifePeriod = 50;
            AssetLifePeriod = 50;
            B4Description = "";

            PerCent = 0;

            //get the material

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
            ECI = Material.ECI + Additional;
            Density = Material.Density;
            
            if (AllElements != null)
            {
                if (AllElements.Count > 0)
                {
                    Volume = 0;
                    foreach (CarboElement ce in AllElements)
                    {
                        ce.Calculate(Material);
                        Volume += ce.Volume;
                    }
                }
            }

            double wasteFact = 1 + (Waste / 100);
            Volume = Math.Round(Volume, 3);

            //Calculate the real volume based on a correction if required. 
            if (Utils.isValidExpression(Correction) == true)
            {
                string volumeStr = Volume.ToString();
                StringToFormula stf = new StringToFormula();
                double result = stf.Eval(volumeStr + Correction);
                TotalVolume = Math.Round((B4Factor *(result * wasteFact)), 3);
            }
            else
            {
                TotalVolume = Math.Round(B4Factor * (Volume * wasteFact),3);
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
            result.TotalVolume = this.TotalVolume;
            result.Additional = this.Additional;
            result.Correction = this.Correction;
            result.Density = this.Density;
            result.Description = this.Description;
            result.EC = this.EC;
            result.ECI = result.ECI;
            result.Id = this.Id;
            result.Mass = this.Mass;

            //Correction Formula
            result.Correction = this.Correction;
            result.CorrectionDescription = this.CorrectionDescription;
            //Waste
            result.Waste = this.Waste;
            result.WasteDescription = this.WasteDescription;
            //Additional
            result.Additional = this.Additional;
            result.AdditionalDescription = this.AdditionalDescription;
            //B4
            result.B4Factor = this.B4Factor;
            result.ComponentLifePeriod = this.ComponentLifePeriod;
            result.AssetLifePeriod = this.AssetLifePeriod;
            result.B4Description = this.B4Description;

            result.isDemolished = this.isDemolished;
            result.isSubstructure = this.isSubstructure;

            result.PerCent = this.PerCent;

            return result;

        }

        internal void copyValues(CarboGroup carboGroup)
        {
            Category = carboGroup.Category;
            SubCategory = carboGroup.SubCategory;

            Material = carboGroup.Material;
            MaterialName = carboGroup.MaterialName;

            AllElements = carboGroup.AllElements ;

            Volume = carboGroup.Volume;
            TotalVolume = carboGroup.TotalVolume;
            Additional = carboGroup.Additional;
            Correction = carboGroup.Correction;
            Density = carboGroup.Density;
            Description = carboGroup.Description;
            EC = carboGroup.EC;
            ECI = carboGroup.ECI;
            Id = carboGroup.Id;
            Mass = carboGroup.Mass;

            isDemolished = carboGroup.isDemolished;
            isSubstructure = carboGroup.isSubstructure;

            PerCent = carboGroup.PerCent;

        }
    }
}
