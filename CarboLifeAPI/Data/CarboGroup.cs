using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]

    public class CarboGroup : ICloneable
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Description { get; set; }
        public double Volume { get; set; }

        /// <summary>
        /// This is a reserved value to get volumes from another group to calculate current. Useful for Reinforce,emt/
        /// </summary>
        public string VolumeLink { get; set; }

        public string Grade {  get; set; }
        public double TotalVolume { get; set; }
        //Calculated Values;
        public double Density { get; set; }
        public double Mass { get; set; }

/// <summary>
/// Total kgCO2/kg per kg.
/// </summary>
        public double ECI { get; set; }

        /// <summary>
        /// Total CO2e in Ton
        /// </summary>
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
        /// In Use Values
        /// </summary>
        ///       
        public CarboB1B7Properties inUseProperties { get; set; }

        /// <summary>
        /// The group material
        ///</summary>
        public CarboMaterial Material {get;set;}
        public List<CarboElement> AllElements { get; set; }

  
        /// The below values are required to corectly group elements to their home group:

        public bool isExisting { get; set; }
        public bool isDemolished { get; set; }
        public bool isSubstructure { get; set; }
        public string additionalData { get; set; }
        public double getVolumeECI
        {
            get
            {
                return Density * ECI;
            }
        }
        public double getTotalA1A3
        {
            get
            {
                double result = 0;
                result = (Mass * Material.ECI_A1A3 * inUseProperties.B4);
                return result;

            }
        }
        public double getTotalA4
        {
            get
            {
                return (Mass * Material.ECI_A4 * inUseProperties.B4);
            }
        }
        public double getTotalA5
        {
            get
            {
                return (Mass * Material.ECI_A5 * inUseProperties.B4);
            }
        }
        public double getTotalB1B7
        {
            get
            {
                return (Mass * Material.ECI_B1B5 * inUseProperties.B4);
            }
        }
        public double getTotalC1C4
        {
            get
            {
                return (Mass * Material.ECI_C1C4 * inUseProperties.B4);
            }
        }
        public double getTotalD
        {
            get
            {
                return (Mass * Material.ECI_D * inUseProperties.B4);
            }
        }
        public double getTotalSeq
        {
            get
            {
                return (Mass * Material.ECI_Seq * inUseProperties.B4);
            }
        }
        public double getTotalMix
        {
            get
            {
                double materialAdded = Mass * Material.ECI_Mix * inUseProperties.B4;
                double addedManual = Mass * this.Additional * inUseProperties.B4;
                return (materialAdded + addedManual);
            }
        }

        public double RcDensity { get; set; }
        public bool isAutoReinforcementGroup { get; set; }

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
            Grade = "";

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
            //Life
            inUseProperties = new CarboB1B7Properties();

            PerCent = 0;
            Material = new CarboMaterial();
            AllElements = new List<CarboElement>();

            isDemolished = false;
            isSubstructure = false;
            isExisting = false;

            isAutoReinforcementGroup = false;

            RcDensity = 0;
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
        public CarboGroup(int id, string materialName, string category, string description, double volume, double density, double mass, double eei, double eci, double ee, double ec, string grade)
        {
            Id = id;
            MaterialName = materialName;
            Category = category;
            Description = description; 

            Volume = volume;
            Density = density;
            Mass = mass;
            Grade = grade;

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
            
            //Life / In use
            inUseProperties = new CarboB1B7Properties();


            PerCent = 0;

            Material = new CarboMaterial();
            AllElements = new List<CarboElement>();
            isAutoReinforcementGroup = false;
        }
        internal void setMaterial(CarboMaterial material)
        {
            this.MaterialName = material.Name;
            this.Material = material;
            this.Density = material.Density;
            this.Waste = material.WasteFactor;
            this.Grade = material.Grade;

            CalculateTotals();
        }
        public CarboGroup(CarboElement carboElement)
        {
            Id = -999;
            MaterialName = carboElement.MaterialName;
            Category = carboElement.Category;
            SubCategory = carboElement.SubCategory;

            Volume = carboElement.Volume;
            Density = carboElement.Density;
            Mass = 0;
            Grade = carboElement.Grade;

            Description = "New Group";

            //EEI = 0;
            ECI = 0;
            //EE = 0;
            EC = 0;

            //Correction Formula
            Correction = carboElement.Correction;
            CorrectionDescription = "";
            //Waste
            Waste = 0;
            WasteDescription = "";
            //Additional
            Additional = 0;
            AdditionalDescription = "";
            
            //Life / in use
            inUseProperties = new CarboB1B7Properties();

            PerCent = 0;

            //get the material

            Material = new CarboMaterial();
            //Material = carboElement.Material; removed from CarboElement

            AllElements = new List<CarboElement>();

            AllElements.Add(carboElement);

            isExisting = carboElement.isExisting;
            isDemolished = carboElement.isDemolished;
            isSubstructure = carboElement.isSubstructure;
            additionalData = carboElement.AdditionalData;

            VolumeLink = "";
            

        }
        internal void getDescription(CarboGroupSettings importSettings)
        {
            string description = "";

            //check if substructure note is required.
            if (importSettings.IncludeSubStructure == true && isSubstructure == true)
            {
                description += "(Substructure)";
            }

            description += this.Category;


            if (this.isExisting == true && this.isDemolished == true)
            {
                description += "Existing & Demolished ";
            }

            if (this.isExisting == true && this.isDemolished == false)
            {
                description = "Existing ";
            }

            if (importSettings.IncludeAdditionalParameter == true && additionalData != "")
            {
                description += additionalData;
            }

            if (importSettings.IncludeGradeParameter == true && Grade != "")
            {
                description += " Grade: " + Grade;
            }

            if (importSettings.IncludeCorrectionParameter == true && Correction != "")
            {
                description += " Corrected";
            }

            if (importSettings.mapReinforcement == true && RcDensity != 0)
            {
                description += " RC Override";
            }

            //Apply description
            Description = description;

        }

        /*
        [Obsolete]
        
        private string GetDescription(CarboElement carboElement)
        {
            string result = "";
            string prefix = "";
            string suffix = "";

            if (carboElement.isExisting == true && carboElement.isDemolished == true)
                prefix = "Existing & Demolished ";

            if (carboElement.isExisting == true && carboElement.isDemolished == false)
                prefix = "Existing ";

//            if (carboElement.isSubstructure == true)
//               suffix = " (Substructure)";

            result = prefix + carboElement.Category + suffix;

            return result;
        }
        */
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
        public void CalculateTotals(bool cA13 = true, bool cA4 = true, bool cA5 = true, bool cB = true, bool cC = true, bool cD = true, bool cSeq = true, bool cAdd = true, bool calcSubstructrue = true)
        {
            //Recalculate The materials
            Material.CalculateTotals();
            //Clear Values
            MaterialName = Material.Name;
            //EEI = Material.EEI;
            double totalECI = 0;
            totalECI += Additional;

            //Calculate the total ECI for each group, using only the parameters that are set
            if (cA13 == true)
                totalECI += Material.ECI_A1A3;
            if (cA4 == true)
                totalECI += Material.ECI_A4;
            if (cA5 == true)
                totalECI += Material.ECI_A5;
            if (cB == true)
                totalECI += Material.ECI_B1B5;
            if (cC == true)
                totalECI += Material.ECI_C1C4;
            if (cD == true)
                totalECI += Material.ECI_D;
            if (cSeq == true)
                totalECI += Material.ECI_Seq;
            if (cAdd == true)
                totalECI += Material.ECI_Mix;

            ECI = totalECI;

            //Get the density from the material.
            Density = Material.Density;
               
            //Recalculate All the Element's Volumes
            if (AllElements != null)
            {
                if (AllElements.Count > 0)
                {
                    Volume = 0;
                    foreach (CarboElement ce in AllElements)
                    {
                        if (ce.includeInCalc == true)
                        {
                            if (calcSubstructrue == false && ce.isSubstructure == true) {
                                int flag = 0; //No Action just a flag for debug
                            }
                            else
                            {
                                ce.Calculate(Material);

                                Volume += ce.Volume;

                                //Calculate the Volume Totals;
                                //Calculate the real volume based on a correction if required. 
                                double ElWasteFact = 1 + (Waste / 100);

                                if (Utils.isValidExpression(Correction) == true)
                                {
                                    string ElVolumeStr = ce.Volume.ToString();
                                    StringToFormula stf = new StringToFormula();
                                    double result = stf.Eval(ElVolumeStr + Correction);

                                    //TotalVolume = Math.Round((inUseProperties.B4 * (result * wasteFact)), 3);
                                    ce.Volume_Total = inUseProperties.B4 * (result * ElWasteFact);

                                }
                                else
                                {
                                    //TotalVolume = Math.Round(inUseProperties.B4 * (Volume * wasteFact), 3);
                                    ce.Volume_Total = inUseProperties.B4 * (ce.Volume * ElWasteFact);

                                }





                            }
                        }
                    }
                }
            }

            //Round the volume;
            //Volume = Math.Round(Volume, 3);

            //Convert to Total Volume waste, convertion and B4 factors:

            double wasteFact = 1 + (Waste / 100);

            //Calculate the real volume based on a correction if required. 
            if (Utils.isValidExpression(Correction) == true)
            {
                string volumeStr = Volume.ToString();
                StringToFormula stf = new StringToFormula();
                double result = stf.Eval(volumeStr + Correction);

                //TotalVolume = Math.Round((inUseProperties.B4 * (result * wasteFact)), 3);
                TotalVolume = inUseProperties.B4 * (result * wasteFact);

            }
            else
            {
                //TotalVolume = Math.Round(inUseProperties.B4 * (Volume * wasteFact), 3);
                TotalVolume = inUseProperties.B4 * (Volume * wasteFact);

            }

            //Use Correct ECi to write into Elements based on Chosen switches (A-D)
            if(this.AllElements.Count > 0)
            {
                foreach(CarboElement el in  this.AllElements)
                {
                    el.ECI = this.ECI;
                }

            }


            //Calculate corrected mass
            Mass = TotalVolume * Density;
            //EC = Total corrected Mass * EE;I

            double inUseReplacementFactor = inUseProperties.B4;

            //Get all the B1-B7 per group
            double inuseECI = inUseProperties.totalECI;
            double ECB1B7 = Mass * inuseECI;
            inUseProperties.totalValue = ECB1B7;

            //The final calc:
            EC = (Mass * (ECI + inuseECI) * inUseReplacementFactor) / 1000;

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

            result.AllElements = new List<CarboElement>();
            foreach( CarboElement element in this.AllElements )
            {
                result.AllElements.Add((CarboElement)element.Clone());
            }

            result.Volume = this.Volume;
            result.TotalVolume = this.TotalVolume;
            result.Additional = this.Additional;
            result.Correction = this.Correction;
            result.Density = this.Density;
            result.Description = this.Description;
            result.EC = this.EC;
            result.ECI = this.ECI;
            result.Id = this.Id;
            result.Mass = this.Mass;
            result.Grade = this.Grade;
            result.VolumeLink = this.VolumeLink;

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
            result.inUseProperties = this.inUseProperties.Copy();

            result.isDemolished = this.isDemolished;
            result.isSubstructure = this.isSubstructure;

            result.PerCent = this.PerCent;

            result.isAutoReinforcementGroup = this.isAutoReinforcementGroup;
            result.Grade = this.Grade;
            result.VolumeLink = this.VolumeLink;
            result.RcDensity = this.RcDensity;
            return result;

        }

        /// <summary>
        /// Clones Without Elements
        /// </summary>
        /// <returns>Clone Without Elements</returns>
        public object Clone()
        {
            return new CarboGroup

            {
                Category = this.Category,
                SubCategory = this.SubCategory,

                Material = this.Material,
                MaterialName = this.MaterialName,

                Volume = this.Volume,
                TotalVolume = this.TotalVolume,
                Correction = this.Correction,
                Density = this.Density,
                Description = this.Description,
                EC = this.EC,
                ECI = this.ECI,
                Id = this.Id,
                Mass = this.Mass,
                Grade = this.Grade,
                VolumeLink = this.VolumeLink,

                AllElements = new List<CarboElement>(),

                //Correction Formula
                CorrectionDescription = this.CorrectionDescription,
                //Waste
                Waste = this.Waste,
                WasteDescription = this.WasteDescription,
                //Additional
                Additional = this.Additional,
                AdditionalDescription = this.AdditionalDescription,
                //B4
                inUseProperties = this.inUseProperties,

                isDemolished = this.isDemolished,
                isSubstructure = this.isSubstructure,

                PerCent = this.PerCent,

                isAutoReinforcementGroup = this.isAutoReinforcementGroup,
                RcDensity = this.RcDensity
        };

        }

        [Obsolete("this should be embedded into a copy method.")]
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
            isAutoReinforcementGroup = carboGroup.isAutoReinforcementGroup;
            RcDensity = carboGroup.RcDensity;

        }

    }
}
