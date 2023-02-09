using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CarboLifeAPI;
using CarboLifeAPI.Data;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboB1B7Properties
    {
        /// <summary>
        /// This is the asset reference period, and is used to calculate how often it will need to be replaced in a project
        /// </summary>
        public double elementdesignlife { get; set; }
        /// <summary>
        /// Setting this to true will force the element to exist untill the end of the project.
        /// </summary>
        public bool designLifeToEnd { get; set; }

        public string name { get; set; }

        //Complete list of values
        /// <summary>
        /// Usage
        /// </summary>
        public double B1 { get; set; }
        /// <summary>
        /// Maintenance
        /// </summary>
        public double B2 { get; set; }
        /// <summary>
        /// Repair
        /// </summary>
        public double B3 { get; set; }
        /// <summary>
        /// Replacement
        /// </summary>
        public double B4 { get; set; }
        /// <summary>
        /// Refurbishment
        /// </summary>
        public double B5 { get; set; }
        /// <summary>
        /// Operational energy use
        /// </summary>
        public double B6 { get; set; }
        /// <summary>
        /// Operational water use
        /// </summary>
        public double B7 { get; set; }


        //Results
        /// <summary>
        /// The total value of kgCO2/kg
        /// </summary>
        public double totalECI { get; set; }

        //Results
        /// <summary>
        /// The total value of kgCO2
        /// </summary>
        public double totalValue { get; set; }
        public string assetType { get; set; }

        [NonSerialized]
        public string calcResult;

        public CarboB1B7Properties()
        {
            elementdesignlife = 50;
            designLifeToEnd = true;

            B4 = 1;

            name = "";

            B1 = 0;
            B2 = 0;
            B3 = 0;
            B5 = 0;
            B6 = 0;
            B7 = 0;
            totalValue = 0;
            totalECI = 0;

            calcResult = "";
            assetType = "";

        }

        public void calculate(int buildingdesignlife)
        {
            //Calculate total nr of trips;
            string calcResult = "";

            try
            {
                this.totalECI = B1 + B2 + B3 + B5 + B6 + B7;


                calcResult += "B1-B5 calculation:" + System.Environment.NewLine;

                if (designLifeToEnd == false)
                {
                    this.B4 = Math.Ceiling(buildingdesignlife / elementdesignlife);

                    calcResult += "One element of this material will last: " + elementdesignlife + " year(s)" + System.Environment.NewLine;
                    calcResult += "The building is intended to last: " + buildingdesignlife + "year" + System.Environment.NewLine;
                    calcResult += "This element will therefor be : " + B4 + " time(s) replaced." + System.Environment.NewLine;
                    this.name = elementdesignlife + " years design life , " + B4 + " time(s) created";

                }
                else
                {
                    this.elementdesignlife = buildingdesignlife;
                    this.B4 = 1;

                    calcResult += "This material is intended to last as long as the building: " + buildingdesignlife + " year(s)" + System.Environment.NewLine;
                    this.name = buildingdesignlife + " years design life , " + B4 + " time created";

                }

                this.calcResult = calcResult;
                
            }
            catch (Exception ex)
            {
                this.calcResult = ex.Message;
            }
        }

        internal CarboB1B7Properties Copy()
        {
            CarboB1B7Properties result = new CarboB1B7Properties();

            result.elementdesignlife = this.elementdesignlife;
            result.designLifeToEnd = this.designLifeToEnd;
            result.name = this.name;
            result.B1 = this.B1;
            result.B2 = this.B2;
            result.B3 = this.B3;
            result.B4 = this.B4;
            result.B5 = this.B5;
            result.B6 = this.B6;
            result.B7 = this.B7;
            result.totalValue = this.totalValue;
            result.assetType = this.assetType;
            result.calcResult = this.calcResult;
            
            return result;
        }
    }
}
