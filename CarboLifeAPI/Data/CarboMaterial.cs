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
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Values: "Timber", "Steel", "Concrete", "Brick", "Plastic", "Other" Accepted
        /// </summary>
        public string Category { get; set; }
        public string Description { get; set; }

        //Technical Properties
        public double Density { get; set; }
        public double EEI { get; set; }
        public double ECI { get; set; }

        public double ECI_A1A3 { get; set; }
        public double ECI_A4A5 { get; set; }

        public double ECI_B1B7 { get; set; }
        public double ECI_C1C4 { get; set; }
        public double ECI_D { get; set; }

        public bool isLocked { get; set; }

        [XmlArray("Property"), XmlArrayItem(typeof(CarboProperty), ElementName = "Property")]
        public List<CarboProperty> Properties { get; set; }


        public CarboMaterial()
        {
            Id = -1;
            Name = "Not specified";
            Category = "Not specified";
            Description = "Carbon material and properties not set.";
            Density = 1;
            EEI = 1;
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
            EEI = 1;
            ECI = 1;
            ECI_A1A3 = 1;
            ECI_A4A5 = 1;
            ECI_B1B7 = 1;
            ECI_C1C4 = 1;
            ECI_D = 1;
            isLocked = false;
            Properties = new List<CarboProperty>();
        }

    }
}
