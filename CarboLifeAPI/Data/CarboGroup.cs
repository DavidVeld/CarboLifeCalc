using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

        /// <summary>
        /// How well the material on this group was matched at import, 0.000 to 1.000.
        /// 1 means it was never auto matched (a manual or generated group), so nothing to review.
        /// Persisted, so a weak mapping is still visible after a save and reload.
        /// </summary>
        public double MatchConfidence { get; set; }

        /// <summary>
        /// Why the match needs a look, empty when it does not. Written by the importer from
        /// CarboMatchResult.Explanation, so the reason survives outside the Description text.
        /// </summary>
        public string MatchNote { get; set; }

        /// <summary>
        /// Where the material on this group came from. Outranks MatchConfidence: see
        /// CarboMaterialSource and SetMaterialProvenance.
        /// </summary>
        public CarboMaterialSource MaterialSource { get; set; }

        /// <summary>
        /// True when the material on this group was auto matched and the matcher was not
        /// confident about it. This is what a review list or a row highlight should test.
        /// </summary>
        public bool NeedsMaterialReview()
        {
            //A generated allowance gets its material from the import settings, so there was never
            //a guess on it to review. It is built by copying its parent group, which is where the
            //note it can arrive wearing comes from: that note describes the match made against the
            //parent's Revit material name, the concrete or the steelwork, and says nothing about
            //the rebar or plate material the generator then put on. The generators clear it now,
            //but a project saved before they did still carries the copy, so it is tested here too.
            if (IsAutoGenerated() == true)
                return false;

            return string.IsNullOrEmpty(MatchNote) == false;
        }

        //The description carries a short provenance note about the material, appended after
        //whatever the import wrote. These are the markers it can start with. They are matched as
        //text when a note is replaced, so a group re-mapped twice does not collect two of them.
        private const string reviewMarker = "[CHECK MATERIAL";
        private const string userAssignedMarker = "[USER ASSIGNED]";
        private const string mappingFileMarker = "[FROM MAPPING FILE]";

        /// <summary>
        /// Records where this group's material came from and rewrites the provenance note on the
        /// end of the description to match.
        ///
        /// Anything the user typed before the note is kept; the note itself, from its marker to
        /// the end of the string, is replaced. A material chosen by a person or named by a saved
        /// mapping clears the review flag whatever the original match scored, which is the whole
        /// point: the score describes a guess that has since been settled.
        /// </summary>
        /// <param name="source">Who decided this material.</param>
        /// <param name="confidence">The matcher's score, only meaningful for an auto match.</param>
        /// <param name="reviewNote">Why it needs a look. Ignored unless the source is AutoMatched.</param>
        public void SetMaterialProvenance(CarboMaterialSource source, double confidence, string reviewNote)
        {
            MaterialSource = source;

            string baseText = StripProvenanceNote(Description);

            if (source == CarboMaterialSource.AutoMatched)
            {
                MatchConfidence = confidence;
                MatchNote = reviewNote == null ? "" : reviewNote;

                if (string.IsNullOrEmpty(MatchNote))
                {
                    //A confident automatic match says nothing extra.
                    Description = baseText;
                    return;
                }

                string note = reviewMarker + " "
                            + confidence.ToString("0.00", CultureInfo.InvariantCulture) + "] " + MatchNote;

                Description = Join(baseText, note);
                return;
            }

            //Decided by a person, by a mapping they saved earlier, or by the import settings a
            //generator read: nothing to review.
            MatchConfidence = 1;
            MatchNote = "";

            if (source == CarboMaterialSource.Generated)
            {
                //A generated allowance already says what it is in its own description, and its
                //material can only have come from the settings, so it gets no marker of its own.
                Description = baseText;
                return;
            }

            Description = Join(baseText,
                source == CarboMaterialSource.UserAssigned ? userAssignedMarker : mappingFileMarker);
        }

        /// <summary>
        /// Removes a provenance note previously written by SetMaterialProvenance, leaving the rest
        /// of the description alone.
        /// </summary>
        private static string StripProvenanceNote(string description)
        {
            if (string.IsNullOrEmpty(description))
                return "";

            int cut = -1;

            foreach (string marker in new[] { reviewMarker, userAssignedMarker, mappingFileMarker })
            {
                int at = description.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

                if (at >= 0 && (cut < 0 || at < cut))
                    cut = at;
            }

            if (cut < 0)
                return description.Trim();

            return description.Substring(0, cut).Trim();
        }

        private static string Join(string baseText, string note)
        {
            return string.IsNullOrEmpty(baseText) ? note : baseText + " " + note;
        }

        public double RcDensity { get; set; }

        /// <summary>
        /// Where this group came from, so the generated groups can be told apart from the ones
        /// holding real elements. Groups read from a file written before this property existed
        /// arrive as Other and are repaired from the two legacy flags below by
        /// UpgradeOriginFromLegacyFlags, called on load.
        /// </summary>
        public CarboGroupOrigin Origin { get; set; }

        /// <summary>
        /// What the legacy flags said in the file that was read, used by
        /// UpgradeOriginFromLegacyFlags and nothing else.
        /// </summary>
        private bool legacyReinforcementFlag;
        private bool legacyConnectionFlag;

        /// <summary>
        /// Superseded by Origin, kept so files written here stay readable by versions that predate
        /// it. The getter reports Origin, the setter only remembers what an older file said.
        /// </summary>
        public bool isAutoReinforcementGroup
        {
            get { return Origin == CarboGroupOrigin.Reinforcement; }
            set { legacyReinforcementFlag = value; }
        }

        /// <summary>
        /// Superseded by Origin, which also says whether it is a steel or a timber allowance.
        /// Kept for the same reason as isAutoReinforcementGroup.
        /// </summary>
        public bool isAutoConnectionGroup
        {
            get
            {
                return Origin == CarboGroupOrigin.SteelConnection
                    || Origin == CarboGroupOrigin.TimberConnection;
            }
            set { legacyConnectionFlag = value; }
        }

        /// <summary>
        /// True for any group the app generated itself. Those are rebuilt from the settings on
        /// every run, so they must not be pruned or reinforced like a group holding real elements.
        /// </summary>
        public bool IsAutoGenerated()
        {
            return Origin == CarboGroupOrigin.Reinforcement
                || Origin == CarboGroupOrigin.SteelConnection
                || Origin == CarboGroupOrigin.TimberConnection;
        }

        /// <summary>
        /// Recovers Origin for a group read from a file written before it existed. Such a group
        /// carries only the two legacy flags, and those cannot tell a steel allowance from a timber
        /// one, so the category the generator wrote is used to split them.
        /// </summary>
        internal void UpgradeOriginFromLegacyFlags()
        {
            //Anything other than Other came from the file itself and is already right.
            if (Origin != CarboGroupOrigin.Other)
                return;

            if (legacyReinforcementFlag == true)
            {
                Origin = CarboGroupOrigin.Reinforcement;
            }
            else if (legacyConnectionFlag == true)
            {
                if (Category == "Timber Connections")
                    Origin = CarboGroupOrigin.TimberConnection;
                else
                    Origin = CarboGroupOrigin.SteelConnection;
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

            //Other, not Manual: this constructor also serves the deserialiser and Copy(),
            //which both set the real origin afterwards.
            Origin = CarboGroupOrigin.Other;

            RcDensity = 0;

            //Nothing was auto matched until the importer says so, and a group read from a file
            //written before these existed reads back the same way: no review needed.
            MatchConfidence = 1;
            MaterialSource = CarboMaterialSource.AutoMatched;
            MatchNote = "";
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
            Origin = CarboGroupOrigin.Other;
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

            //A group built around an element always comes from an import.
            Origin = CarboGroupOrigin.Import;


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
        /// <summary>
        /// The result of evaluating the correction expression, or the uncorrected volume when
        /// that result is not a usable number.
        ///
        /// A correction is free text that reaches StringToFormula, and a division by zero there
        /// returns Infinity rather than throwing - the reinforcement generator builds
        /// "*(172/0)" whenever the reinforcement material has no density, and 108 of the 605
        /// Okobaudat rows have exactly that. Infinity then flowed into TotalVolume, Mass, EC and
        /// on into the project total, silently, with nothing on screen to say why every number
        /// had become infinite. Falling back to the uncorrected volume keeps the group wrong in
        /// a bounded, visible way instead.
        /// </summary>
        private static double SafeCorrectedVolume(double evaluated, double fallback)
        {
            if (double.IsNaN(evaluated) || double.IsInfinity(evaluated))
                return fallback;

            return evaluated;
        }

        public void CalculateTotals(bool cA13 = true, bool cA4 = true, bool cA5 = true, bool cB = true, bool cC = true, bool cD = true, bool cSeq = true, bool cAdd = true, bool calcSubstructrue = true, double uncertFact = 0)
        {
            //Recalculate The materials
            Material.CalculateTotals();
            //Clear Values
            MaterialName = Material.Name;
            //EEI = Material.EEI;
            double totalECI = 0;
            totalECI += Additional;

            double uncertaintyFactor = 1 + uncertFact;

            if (uncertaintyFactor < 1)
                uncertaintyFactor = 1;

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
                                Volume += ce.Volume;

                                //Calculate the Volume Totals;
                                //Calculate the real volume based on a correction if required.
                                double ElWasteFact = 1 + (Waste / 100);

                                double elCorrected = ce.Volume;

                                if (Utils.isValidExpression(Correction) == true)
                                {
                                    string ElVolumeStr = ce.Volume.ToString(CultureInfo.InvariantCulture);
                                    StringToFormula stf = new StringToFormula();
                                    elCorrected = SafeCorrectedVolume(stf.Eval(ElVolumeStr + Correction), ce.Volume);
                                }

                                //B4 is deliberately NOT applied here, see the group volume below.
                                //The uncertainty factor is, so the element volumes add up to the
                                //group volume: without it every element sat 1/(1+uncert) below its
                                //share of the group, which is what the heat map, the Revit
                                //write-back and the per element exports all read.
                                ce.Volume_Total = elCorrected * ElWasteFact * uncertaintyFactor;

                                //Calculate last: it derives the element's mass and EC from Volume_Total,
                                //so it has to run after the waste, correction and B4 factors are in.
                                //Called before them it used the value left over from the previous
                                //calculation, which left every element at zero on a fresh import and one
                                //pass behind after any change to the group.
                                ce.Calculate(Material);
                            }
                        }
                    }
                }
            }

            //Round the volume;
            //Volume = Math.Round(Volume, 3);

            //Convert to Total Volume waste and conversion factors:

            double wasteFact = 1 + (Waste / 100);

            double corrected = Volume;

            //Calculate the real volume based on a correction if required.
            if (Utils.isValidExpression(Correction) == true)
            {
                string volumeStr = Volume.ToString(CultureInfo.InvariantCulture);
                StringToFormula stf = new StringToFormula();
                corrected = SafeCorrectedVolume(stf.Eval(volumeStr + Correction), Volume);
            }

            //B4, the replacement count, is deliberately NOT applied to the volume.
            //TotalVolume and Mass describe the material standing in the building, which is what
            //the volume and mass columns of every report and export mean. B4 belongs to the
            //carbon, and it is applied once in the EC line below and once in each getTotalXX
            //getter. Folding it in here as well made both of those B4 squared: a group with an
            //element design life of 30 years in a 60 year building reported twice the carbon it
            //should, four times instead of two.
            TotalVolume = corrected * wasteFact * uncertaintyFactor;

            //Use Correct ECi to write into Elements based on Chosen switches (A-D)
            if (this.AllElements.Count > 0)
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
            EC = ((Mass * (ECI + inuseECI) * inUseReplacementFactor)) / 1000;

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

            result.Origin = this.Origin;
            result.MatchConfidence = this.MatchConfidence;
            result.MaterialSource = this.MaterialSource;
            result.MatchNote = this.MatchNote;
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

                Origin = this.Origin,
                MatchConfidence = this.MatchConfidence,
                MaterialSource = this.MaterialSource,
                MatchNote = this.MatchNote,
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
            Origin = carboGroup.Origin;
            MatchConfidence = carboGroup.MatchConfidence;
            MaterialSource = carboGroup.MaterialSource;
            MatchNote = carboGroup.MatchNote;
            RcDensity = carboGroup.RcDensity;

        }

    }
}
