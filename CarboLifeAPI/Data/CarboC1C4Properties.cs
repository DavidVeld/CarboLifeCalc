using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI.Data
{
    [Serializable]
    public class CarboC1C4Properties
    {
        public string propertyName { get; set; }
        //C1
        public double c1density { get; set; }
        public double c1BaseValue { get; set; }
        public double c1Value { get; set; }

        //C2
        public CarboA4Properties c2Properties { get; set; }
        public double c2Value { get; set; }

        //C3
        public double c3Value { get; set; }

        //C4
        public string c4DisposalName { get; set; }

        public double c4landfP { get; set; }
        public double c4landfV { get; set; }
        public double c4lanfResult { get; set; }

        public double c4incfP { get; set; }
        public double c4incfV { get; set; }
        public double c4incResult { get; set; }

        public double c4reUseP { get; set; }
        public double c4reUseV { get; set; }
        public double c4reUseResult { get; set; }

        public double other { get; set; }

        //Total
        [NonSerialized]
        public string calcResult;
        public double value { get; set; }

        public CarboC1C4Properties()
        {
            propertyName = "";

            c1density = 0;
            c1BaseValue = 3.40;
            c1Value = 0;

            c2Properties = new CarboA4Properties();
            c2Value = 0;

            c3Value = 0;

            c4DisposalName = "Generic";
            c4landfP = 50;
            c4landfV = 0.013;

            c4incfP = 0;
            c4incfV = 0;

            c4reUseP = 50;
            c4reUseV = 0.005;

            value = 0;
            calcResult = "";

        }

        public void calculate()
        {
            string calcResult = "";
           
            //Calculate total nr of trips;
            //C1
            c1Value = c1BaseValue * c1density;

            double c4incResult = c4incfV * (c4incfP / 100);
            double c4lanfResult = c4landfV * (c4landfP / 100);
            double c4reUseResult = c4reUseV * (c4reUseP / 100);
            double c4value = c4incResult + c4lanfResult + c4reUseResult;


            double costTotal = c1Value + c2Value + c3Value + c4value + other;

            try
            {
                calcResult += "This calculation create a CO₂e per kg value based on the given parameters." + System.Environment.NewLine + System.Environment.NewLine;
                calcResult += System.Environment.NewLine;

                if (c1Value != 0)
                {
                    calcResult += "[C1] Demolition carbon cost: " + System.Environment.NewLine;
                   // calcResult += System.Environment.NewLine;
                    calcResult += c1BaseValue + "kgCO₂e/m2  x " + c1density + "kg/m2 = " + c4incResult + " kgCO₂/kg " + System.Environment.NewLine;
                    calcResult += System.Environment.NewLine;
                }


                if (c2Value != 0)
                {
                    calcResult += "[C2] Transport carbon cost: " + System.Environment.NewLine;
                    //calcResult += System.Environment.NewLine;
                    calcResult += c2Value + " kgCO₂e/m2 " + System.Environment.NewLine;
                    calcResult += System.Environment.NewLine;
                }

                if (c3Value != 0)
                {
                    calcResult += "[C3]  Waste Processing carbon costs: " + System.Environment.NewLine;
                    //calcResult += System.Environment.NewLine;
                    calcResult += "Waste Processing cost are set as: " + c3Value + " kgCO₂/kg " + System.Environment.NewLine;
                    calcResult += System.Environment.NewLine;
                }

                if (c4value != 0)
                {
                    calcResult += "[C4] Disposal carbon costs: " + System.Environment.NewLine;
                    //calcResult += System.Environment.NewLine;
                    calcResult += "[C4] Incinerator costs is: " + c4incfV + " x " + c4incfP + " % = " + c4incResult + " kgCO₂/kg " + System.Environment.NewLine;
                    calcResult += "[C4] Landfill costs is: " + c4landfV + " x " + c4landfP + " % = " + c4lanfResult + " kgCO₂/kg " + System.Environment.NewLine;
                    calcResult += "[C4] Re-use costs is: " + c4reUseV + " x " + c4reUseP + " % = " + c4reUseResult + " kgCO₂/kg " + System.Environment.NewLine;
                    calcResult += System.Environment.NewLine;
                    calcResult += "[C4] total cost is: " + c4incResult + " + " + c4lanfResult + " + " + c4reUseResult + " = " + c4value + " kgCO₂/kg " + System.Environment.NewLine;

                    calcResult += System.Environment.NewLine;
                }

                if (c3Value != 0)
                {
                    calcResult += "Additional costs are (please specify in description otherwise leave as 0): " + other + " kgCO₂/kg " + System.Environment.NewLine;
                    calcResult += System.Environment.NewLine;
                }

                calcResult += "[C4] Disposal carbon costs: " + System.Environment.NewLine;
                calcResult += "Total costs are: " + "[C1]" + " + " + "[C2]" + " + " + "[C3]" + " + " + "[C4]" + " + " + "[Additional]" + System.Environment.NewLine;

                calcResult += "Total costs are: " + c1Value + " + " + c2Value + " + " + c3Value + " + " + c4value + " + " + other + System.Environment.NewLine;
                calcResult += "= " + costTotal + " kgCO₂/kg " + System.Environment.NewLine;
                
                if(c1Value > 1 && c4value > 1)
                {
                    calcResult += System.Environment.NewLine;
                    calcResult += "WARNING, YOUR EOL CALCULATION SHOWS A C1 AND A C4 VALUE, ONLY ONE CAN BE USED OTHERWISE DEMOLITION WILL BE COUNTED DOUBLE" + System.Environment.NewLine;
                    calcResult += System.Environment.NewLine;

                }


                this.calcResult = calcResult;
                this.value = costTotal;
            }
            catch
            { }
        }
    }
}
