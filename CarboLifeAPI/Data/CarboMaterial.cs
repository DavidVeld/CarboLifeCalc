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

        //Technical Properties 

        /// <summary>
        /// kg/m³
        /// </summary>
        public double Density { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double ECI { get; set; }

        /// <summary>
        /// kgCO2e/kg
        /// </summary>
        public double ECI_A1A3 { get; set; }
        /// <summary>
        /// kgCO2e/kg
        /// </summary>
        public double ECI_A4A5 { get; set; }
        /// <summary>
        /// kgCO2e/kg
        /// </summary>
        public double ECI_B1B7 { get; set; }
        /// <summary>
        /// kgCO2e/kg
        /// </summary>
        public double ECI_C1C4 { get; set; }
        /// <summary>
        /// kgCO2e/kg
        /// </summary>
        public double ECI_D { get; set; }

        /// <summary>
        /// Used to protect data
        /// </summary>
        public bool isLocked { get; set; }

        [XmlArray("Property"), XmlArrayItem(typeof(CarboProperty), ElementName = "Property")]
        public List<CarboProperty> Properties { get; set; }

        public CarboMaterial()
        {
            Id = -1;
            Name = "";
            Category = "";
            Description = "";
            Density = 1;
            //EEI = 1;
            ECI = 1;
            ECI_A1A3 = 1;
            ECI_A4A5 = 1;
            ECI_B1B7 = 1;
            ECI_C1C4 = 1;
            ECI_D = 1;
            isLocked = false;
            Properties = new List<CarboProperty>();
        }

        public CarboMaterial(string materialName)
        {
            Id = -1;
            Name = materialName;
            Category = "Not specified";
            Description = "Carbon material and properties not set.";
            Density = 1;
            //EEI = 1;
            ECI = 1;
            ECI_A1A3 = 1;
            ECI_A4A5 = 1;
            ECI_B1B7 = 1;
            ECI_C1C4 = 1;
            ECI_D = 1;
            isLocked = false;
            Properties = new List<CarboProperty>();
        }

        public void CalculateTotals()
        {
            ECI = ECI_A1A3 + ECI_A4A5 + ECI_B1B7 + ECI_C1C4 + ECI_D;

        }

        public void SetProperty(string properyName, string propertyValue)
        {
            CarboProperty cpnew = new CarboProperty
            {
                PropertyName = properyName,
                Value = propertyValue
            };
            bool isUnique = true;

            foreach(CarboProperty cp in Properties)
            {
                if(cp.PropertyName == cpnew.PropertyName)
                {
                    cp.Value = cpnew.Value;
                    isUnique = false;
                    break;
                }
            }
            if(isUnique == true)
            {
                Properties.Add(cpnew);
            }
        }
        public CarboProperty GetCarboProperty(string propertyName)
        {
            CarboProperty result = null;

            foreach (CarboProperty cp in Properties)
            {
                if (cp.PropertyName == propertyName)
                {
                    result = cp;
                    break;
                }
            }
            if(result == null)
            {
                result = new CarboProperty();
            }
            return result;
        }
    }
}
