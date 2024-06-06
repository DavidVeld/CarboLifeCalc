namespace CarboLifeAPI.Data
{
    public class JsCarboMaterial
    {
        /// <summary>
        /// Unique Identifier
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; set; }
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
        public double VolumeECI { get; set; }

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

        public JsCarboMaterial()
        {
            Id = 0;
            GUID = "";
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

            WasteFactor = 0;

            isLocked = false;

        }



    }
}