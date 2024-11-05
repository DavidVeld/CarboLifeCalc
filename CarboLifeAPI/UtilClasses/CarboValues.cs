using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI
{
    /// <summary>
    /// A class to hold the value of a single datapoint in a graph to analyse and colour in the model.
    /// </summary>
    public class CarboValues
    {
        /// <summary>
        /// Red
        /// </summary>
        public byte r { get; set; }
        /// <summary>
        /// Green
        /// </summary>
        public byte g { get; set; }
        /// <summary>
        /// Blue
        /// </summary>
        public byte b { get; set; }
        /// <summary>
        /// (Revit) Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// (Revit) GUID
        /// </summary>
        public string GUID { get; set; }
        /// <summary>
        /// The xValue
        /// </summary>
        //public double xValue { get; set; }

        /// <summary>
        /// The yValue
        /// </summary>
        //public double yValue { get; set; }


        /// <summary>
        /// This is usefull to group data when needed
        /// </summary>
        public string ValueName { get; set; }
        /// <summary>
        /// The Element's Category
        /// </summary>
        public string ValueCategory { get; set; }
        /// <summary>
        /// The Element's Value
        /// </summary>
        public double Value { get; set; }


    }

}
