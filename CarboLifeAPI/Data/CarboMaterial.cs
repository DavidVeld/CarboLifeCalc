using System;
using System.Xml.Serialization;

namespace CarboLifeAPI.Data
{
    [Serializable]
    [XmlRoot("CarboMaterial")]

    public class CarboMaterial : ICloneable
    {

        /// <summary>
        /// Unique Identifier
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Preferably Unique name for a material
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Values: "Timber", "Steel", "Concrete", "Brick", "Plastic", "Other" Accepted
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Description for a material
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// URL Link to a EPD.
        /// </summary>
        public string EPDurl { get; set; }

        /// <summary>
        /// The material Grade can be used for grouping
        /// </summary>
        public string Grade { get; set; }

        //Technical Properties 

        /// <summary>
        /// kg/m³
        /// </summary>
        public double Density { get; set; }
        /// <summary>
        /// Total ECI kgCO₂e/kg
        /// </summary>
        public double ECI { get; set; }

        /// <summary>
        /// Total ECI kgCO₂e/m³
        /// </summary>
        public double getVolumeECI { get { return Density * ECI; } set { } }

        /// <summary>
        /// Fabrication kgCO₂e/kg
        /// </summary>
        public double ECI_A1A3 { get; set; }
        /// <summary>
        /// Transport kgCO₂e/kg
        /// </summary>
        public double ECI_A4 { get; set; }
        /// <summary>
        /// Construction kgCO₂e/kg
        /// </summary>
        public double ECI_A5 { get; set; }
        /// <summary>
        /// Life kgCO₂e/kg
        /// </summary>
        public double ECI_B1B5 { get; set; }
        /// <summary>
        /// End of Life kgCO₂e/kg
        /// </summary>
        public double ECI_C1C4 { get; set; }
        /// <summary>
        /// SUPPLEMENTARY INFORMATION BEYOND THE PROJECT LIFE CYCLE kgCO₂e/kg
        /// </summary>
        public double ECI_D { get; set; }

        /// <summary>
        /// Carbon Sequestration
        /// </summary>        
        public double ECI_Seq { get; set; }

        /// <summary>
        /// This is a value you can add to mix another materials totals into this.
        /// </summary>
        public double ECI_Mix { get; set; }

        /// <summary>
        /// When a group gets this material assigned, it get this percentage assigned for waste as a default.
        /// </summary>
        public double WasteFactor { get; set; }

        /// <summary>
        /// Used to protect data
        /// </summary>
        public bool isLocked { get; set; }

        /*
        [XmlArray("Property"), XmlArrayItem(typeof(CarboProperty), ElementName = "Property")]
        public List<CarboProperty> Properties { get; set; }
        */

        //ReplaceMaterialProperties:
        //Fabrication
        public A1A3Element materialA1A3Properties { get; set; }
        //Transport
        public CarboA4Properties materiaA4Properties { get; set; }
        //Construction
        public CarboA5Properties materialA5Properties { get; set; }
        //Life
        //public CarboB1B7Properties materialB1B5Properties { get; set; }
        //EndofLift
        public CarboC1C4Properties materialC1C4Properties { get; set; }
        //Sequestration
        public CarboSeqProperties materialSeqProperties { get; set; }
        //Suplemental
        public CarboDProperties materialDProperties { get; set; }

        public bool ECI_A1A3_Override { get; set; }
        public bool ECI_A4_Override { get; set; }
        public bool ECI_A5_Override { get; set; }
        public bool ECI_C1C4_Override { get; set; }
        public bool ECI_D_Override { get; set; }
        public bool ECI_Seq_Override { get; set; }
        public string ECI_Mix_Info { get; set; }

        public CarboMaterial()
        {
            Id = -1;
            Name = "";
            Category = "";
            Description = "";
            Density = 500;
            Grade = "";

            ECI = 0;
            ECI_A1A3 = 0;
            ECI_A4 = 0;
            ECI_A5 = 0;
            ECI_B1B5 = 0;
            ECI_C1C4 = 0;
            ECI_D = 0;
            ECI_Seq = 0;
            ECI_Mix = 0;

            ECI_Mix_Info = "";

            WasteFactor = 0;
            isLocked = false;
            //Properties = new List<CarboProperty>();

            materialA1A3Properties = new A1A3Element();
            materiaA4Properties = new CarboA4Properties();
            materiaA4Properties.calculate();
            materialA5Properties = new CarboA5Properties();
            materialC1C4Properties = new CarboC1C4Properties();
            materialSeqProperties = new CarboSeqProperties();
            materialDProperties = new CarboDProperties();

            ECI_A1A3_Override = false;
            ECI_A4_Override = false;
            ECI_A5_Override = false;
            ECI_C1C4_Override = false;
            ECI_Seq_Override = false;
            ECI_D_Override = false;

        }

        public CarboMaterial(string materialName)
        {
            Id = -1;
            Name = materialName;
            Category = "New Carbo material";
            Description = "Carbon material and properties not set.";
            Density = 1;
            Grade = "";
            //EEI = 1;
            ECI = 0;
            ECI_A1A3 = 0;
            ECI_A4 = 0;
            ECI_A5 = 0;
            ECI_B1B5 = 0;
            ECI_C1C4 = 0;
            ECI_D = 0;
            WasteFactor = 0;
            isLocked = false;
            //Properties = new List<CarboProperty>();

            //Calculated Values;
            materialA1A3Properties = new A1A3Element();
            materiaA4Properties = new CarboA4Properties();
            materialA5Properties = new CarboA5Properties();
            materialC1C4Properties = new CarboC1C4Properties();
            materialSeqProperties = new CarboSeqProperties();
            materialDProperties = new CarboDProperties();

            ECI_A1A3_Override = false;
            ECI_A4_Override = false;
            ECI_A5_Override = false;
            ECI_Seq_Override = false;
            ECI_C1C4_Override = false;
            ECI_D_Override = false;
        }

        public void CalculateTotals()
        {
            //Set All calculated Values:
            if (ECI_A1A3_Override == false)
            {
                materialA1A3Properties.Calculate();
                ECI_A1A3 = materialA1A3Properties.ECI_A1A3;
            }

            if (ECI_A4_Override == false)
            {
                materiaA4Properties.calculate();
                ECI_A4 = materiaA4Properties.value;
            }

            if (ECI_A5_Override == false)
            {
                materialA5Properties.calculate();
                ECI_A5 = materialA5Properties.value;
            }


            if (ECI_C1C4_Override == false)
            {
                materialC1C4Properties.calculate();
                ECI_C1C4 = materialC1C4Properties.value;
            }

            if (ECI_D_Override == false)
            {
                materialDProperties.calculate();
                ECI_D = materialDProperties.value;
            }

            if (ECI_Seq_Override == false)
            {
                materialSeqProperties.calculate();
                ECI_Seq = materialSeqProperties.value;
            }

            //The full Calculation
            ECI = ECI_A1A3 + ECI_A4 + ECI_A5 + ECI_B1B5 + ECI_C1C4 + ECI_D + ECI_Seq + ECI_Mix;
        }

        internal void Copy(CarboMaterial cmNew)
        {
            var type = typeof(CarboMaterial);
            foreach (var sourceProperty in type.GetProperties())
            {
                var targetProperty = type.GetProperty(sourceProperty.Name);
                targetProperty.SetValue(this, sourceProperty.GetValue(cmNew, null), null);
            }
            foreach (var sourceField in type.GetFields())
            {
                var targetField = type.GetField(sourceField.Name);
                targetField.SetValue(this, sourceField.GetValue(cmNew));
            }
        }

        public object Clone()
        {
            CarboMaterial clone = new CarboMaterial();

            clone.Id = this.Id;
            clone.Name = this.Name;
            clone.Description = this.Description;
            clone.Category = this.Category;
            clone.Density = this.Density;
            clone.Grade = this.Grade;
            clone.WasteFactor = this.WasteFactor;
            clone.isLocked = this.isLocked;
            clone.EPDurl = this.EPDurl;

            //results
            clone.ECI = this.ECI;

            clone.ECI_A1A3 = this.ECI_A1A3;
            clone.ECI_A4 = this.ECI_A4;
            clone.ECI_A5 = this.ECI_A5;
            clone.ECI_B1B5 = this.ECI_B1B5;
            clone.ECI_C1C4 = this.ECI_C1C4;
            clone.ECI_D = this.ECI_D;
            clone.ECI_Mix = this.ECI_Mix;
            clone.ECI_Seq = this.ECI_Seq;
            clone.ECI = this.ECI;

            clone.ECI_A1A3_Override = this.ECI_A1A3_Override;
            clone.ECI_A4_Override = this.ECI_A4_Override;
            clone.ECI_A5_Override = this.ECI_A5_Override;
            clone.ECI_C1C4_Override = this.ECI_C1C4_Override;
            clone.ECI_D_Override = this.ECI_D_Override;
            clone.ECI_Seq_Override = this.ECI_Seq_Override;

            clone.materialA1A3Properties = this.materialA1A3Properties;
            clone.materiaA4Properties = this.materiaA4Properties;
            clone.materialA5Properties = this.materialA5Properties;
            clone.materialC1C4Properties = this.materialC1C4Properties;
            clone.materialDProperties = this.materialDProperties;
            clone.materialSeqProperties = this.materialSeqProperties;

            return clone;
        }
    }
}
