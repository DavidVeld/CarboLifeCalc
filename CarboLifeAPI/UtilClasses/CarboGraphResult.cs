using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarboLifeAPI
{
    /// <summary>
    /// This class is used to store the information to colour in the model in Revit.
    /// </summary>
    public class CarboGraphResult
    {
        public string xName { get; set; }
        public string yName { get; set; }

        /// <summary>
        /// This paramater owns all the data for import to Revit
        /// </summary>
        public IList<CarboValues> elementData;
        /// <summary>
        /// This is the Graph Showing the Data
        /// </summary>
        public IList<UIElement> UIData;

        public CarboGraphResult()
        {
            elementData = new List<CarboValues>();
            UIData = new List<UIElement>();
        }

        internal void Add(IList<UIElement> listOfUiData)
        {
            if (listOfUiData.Count > 0)
            {
                foreach (UIElement uie in listOfUiData)
                {
                    this.UIData.Add(uie);
                }
            }
        }
    }

}
