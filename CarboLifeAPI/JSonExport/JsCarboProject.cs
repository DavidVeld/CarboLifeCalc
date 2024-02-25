using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace CarboLifeAPI.Data
{
    [Serializable]
    
    public class JsCarboProject
    {
        public List<JsCarboMaterial> materialList { get; set; }

        public List<JsCarboElement> elementList { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public double SocialCost { get; set; }

        /// <summary>
        /// m²
        /// </summary>
        public double GIA { get; set; }
        public double GIANew { get; internal set; }

        /// <summary>
        /// tCo2
        /// </summary>
        public double A0Global { get; set; }

        /// <summary>
        /// tCo2
        /// </summary>
        public double A5Global { get; set; }

        /// <summary>
        /// tCo2
        /// </summary>
        public double b675Global { get; set; }

        /// <summary>
        /// tCo2
        /// </summary>
        public double C1Global { get; set; }
        public double ECTotal { get; set; }
        public string valueUnit { get; set; }
        public int designLife { get; set; }

        /// <summary>
        /// Generates a new CarboProject
        /// </summary>
        public JsCarboProject()
        {
            elementList = new List<JsCarboElement>();
            materialList = new List<JsCarboMaterial>();

            Name = "New Project";
            Number = "000000";
            Category = "Structure";
            Description = "New Project";
            valueUnit = "£";
            GIA = 1;
            GIANew = 1;

            designLife = 0;

            A0Global = 0;
            C1Global = 0;
            A5Global = 0;
            b675Global = 0;
            SocialCost = 0;

            ECTotal = 0;
        }



    }
}
