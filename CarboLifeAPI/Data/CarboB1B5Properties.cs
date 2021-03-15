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
    public class CarboB1B5Properties
    {
        public double elementdesignlife;
        public double buildingdesignlife;

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
        public double totalValue { get; set; }


        [NonSerialized]
        public string calcResult;
        public string assetType { get; set; }

        public CarboB1B5Properties()
        {
            elementdesignlife = 50;
            buildingdesignlife = 50;
            B4 = 1;

            name = "";

            B1 = 0;
            B2 = 0;
            B3 = 0;
            B5 = 0;
            B6 = 0;
            B7 = 0;
            totalValue = 0;

            calcResult = "";
            assetType = "";

            calculate();
        }

        public void calculate()
        {
            //Calculate total nr of trips;
            string calcResult = "";
            //materialB1B5Properties.totalValue = CarboLifeAPI.Utils.ConvertMeToDouble(txt_.Text);
            // materialB1B5Properties.value = CarboLifeAPI.Utils.ConvertMeToDouble(txt_Value.Text);

            try
            {
                this.totalValue = B1 + B2 + B3 + B5 + B6 + B7;
                this.B4 = Math.Ceiling(buildingdesignlife / elementdesignlife);

                calcResult += "B1-B5 calculation:" + System.Environment.NewLine;
                calcResult += "One element of this material will last: " + elementdesignlife + " year" + System.Environment.NewLine;
                calcResult += "The building is intended to last: " + buildingdesignlife + "year" + System.Environment.NewLine;
                calcResult += "This element will therefor be : " + B4 + " time(s) created." + System.Environment.NewLine;
                
                this.calcResult = calcResult;
                this.name = elementdesignlife + " years design life , " + B4 + " time(s) created";

            }
            catch (Exception ex)
            {
                this.calcResult = ex.Message;
            }
        }
    }
}
