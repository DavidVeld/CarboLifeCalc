using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarboLifeAPI.Data
{
    [Serializable]
    [XmlRoot("CarboMaterial")]

    public class CarboMaterial
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


        //Technical Properties 

        /// <summary>
        /// kg/m³
        /// </summary>
        public double Density { get; set; }
        /// <summary>
        /// Total ECI kgCO2e/kg
        /// </summary>
        public double ECI { get; set; }

        /// <summary>
        /// Total ECI kgCO2e/m³
        /// </summary>
        public double getVolumeECI { get { return Density * ECI; } set { } }

        /// <summary>
        /// Fabrication kgCO2e/kg
        /// </summary>
        public double ECI_A1A3 { get; set; }
        /// <summary>
        /// Transport kgCO2e/kg
        /// </summary>
        public double ECI_A4 { get; set; }
        /// <summary>
        /// Construction kgCO2e/kg
        /// </summary>
        public double ECI_A5 { get; set; }
        /// <summary>
        /// Life kgCO2e/kg
        /// </summary>
        public double ECI_B1B5 { get; set; }
        /// <summary>
        /// End of Life kgCO2e/kg
        /// </summary>
        public double ECI_C1C4 { get; set; }
        /// <summary>
        /// SUPPLEMENTARY INFORMATION BEYOND THE PROJECT LIFE CYCLE kgCO2e/kg
        /// </summary>
        public double ECI_D { get; set; }
        /// <summary>
        /// This is a value you can add to mix another materials totals into this.
        /// </summary>
        public double ECI_Mix { get; set; }

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
        public CarboB1B5Properties materialB1B5Properties { get; set; }
        //EndofLift
        public CarboC1C4Properties materialC1C4Properties { get;set;}
        //Suplemental
        public CarboDProperties materialDProperties { get; set; }

        public bool ECI_A1A3_Override { get; set; }
        public bool ECI_A4_Override { get; set; }
        public bool ECI_A5_Override { get; set; }
        public bool ECI_B1B5_Override { get; set; }
        public bool ECI_C1C4_Override { get; set; }
        public bool ECI_D_Override { get; set; }

        public string ECI_Mix_Info { get; set; }

        public CarboMaterial()
        {
            Id = -1;
            Name = "";
            Category = "";
            Description = "";
            Density = 1;
            ECI = 1;
            ECI_A1A3 = 1;
            ECI_A4 = 1;
            ECI_A5 = 1;
            ECI_B1B5 = 1;
            ECI_C1C4 = 1;
            ECI_D = 1;

            ECI_Mix = 0;
            ECI_Mix_Info = "";

            isLocked = false;
            //Properties = new List<CarboProperty>();

            materialA1A3Properties = new A1A3Element();
            materiaA4Properties = new CarboA4Properties();
            materiaA4Properties.calculate();
            materialA5Properties = new CarboA5Properties();
            materialB1B5Properties = new CarboB1B5Properties();
            materialC1C4Properties = new CarboC1C4Properties();
            materialDProperties = new CarboDProperties();

            ECI_A1A3_Override = false;
            ECI_A4_Override = false;
            ECI_A5_Override = false;
            ECI_B1B5_Override = false;
            ECI_C1C4_Override = false;
            ECI_D_Override = false;

        }

        public CarboMaterial(string materialName)
        {
            Id = -1;
            Name = materialName;
            Category = "Not specified";
            Description = "Carbon material and properties not set.";
            Density = 1;
            //EEI = 1;
            ECI = 0;
            ECI_A1A3 = 0;
            ECI_A4 = 0;
            ECI_A5 = 0;
            ECI_B1B5 = 1;
            ECI_C1C4 = 0;
            ECI_D = 0;
            isLocked = false;
            //Properties = new List<CarboProperty>();

            //Calculated Values;
            materialA1A3Properties = new A1A3Element();
            materiaA4Properties = new CarboA4Properties();
            materialA5Properties = new CarboA5Properties();
            materialB1B5Properties = new CarboB1B5Properties();
            materialC1C4Properties = new CarboC1C4Properties();
            materialDProperties = new CarboDProperties();

            ECI_A1A3_Override = false;
            ECI_A4_Override = false;
            ECI_A5_Override = false;
            ECI_B1B5_Override = false;
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

            if (ECI_B1B5_Override == false)
            {
                materialB1B5Properties.calculate();
                ECI_B1B5 = materialB1B5Properties.value;
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


            ECI = ECI_B1B5 * (ECI_A1A3 + ECI_A4 + ECI_A5 + ECI_C1C4 + ECI_D + ECI_Mix);
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
    }
}
