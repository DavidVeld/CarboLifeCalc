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
        public string calcResult { get; set; }
        public double value { get; set; }


        public CarboC1C4Properties()
        {
            propertyName = "";

            c1density = 1;
            c1BaseValue = 3.40;
            c1Value = 0;

            c2Properties = new CarboA4Properties();
            c2Value = 0;

            c3Value = 0;

            c4DisposalName = "";
            c4landfP = 0;
            c4landfV = 0;

            c4incfP = 0;
            c4incfV = 0;

            c4reUseP = 0;
            c4reUseV = 0;

            value = 0;
            calcResult = "";

        }

        public void calculate()
        {
            string calcResult = "";
            double value = 0;

            //C1V = txt1BaseV * txtC1Fact;

            //Calculate re-usage:
            double c4reUseP = 100 - (c4landfP + c4incfP);
            
            //Recalibrate percentages
            if (c4reUseP < 0)
            {
                c4reUseP = 0;

                if (c4landfP > 100)
                    c4landfP = 100;

                c4incfP = 100 - c4landfP;
            }

            //Calculate total nr of trips;
            //C1
            c1Value = Math.Round((c1BaseValue * c1density), 3);

            double c4incResult = Math.Round(c4incfV * (c4incfP / 100), 3);
            double c4lanfResult = Math.Round(c4landfV * (c4landfP / 100), 3);
            double c4reUseResult = Math.Round(c4reUseV * (c4reUseP / 100), 3);

            double costTotal = Math.Round((c4incResult + c4lanfResult + c4reUseResult + other), 3);
            try
            {
                calcResult += "This calculation will try to create a CO2 per kg value based on the given parameters." + System.Environment.NewLine + System.Environment.NewLine;
                calcResult += "Incinerator costs are: " + c4incfV + " x " + c4incfP + " % = " + c4incResult + " kgCO2/kg " + System.Environment.NewLine;
                calcResult += "Landfill costs are: " + c4landfV + " x " + c4landfP + " % = " + c4lanfResult + " kgCO2/kg " + System.Environment.NewLine;
                calcResult += "Re-use costs are: " + c4reUseV + " x " + c4reUseP + " % = " + c4reUseResult + " kgCO2/kg " + System.Environment.NewLine;
                calcResult += "Additional costs are: " + other + " kgCO2/kg " + System.Environment.NewLine;

                calcResult += "Total costs are: " + c4incResult + " + " + c4lanfResult + " + " + c4reUseResult + " + " + other + System.Environment.NewLine;
                calcResult += "= " + costTotal + " kgCO2/kg " + System.Environment.NewLine;

                this.calcResult = calcResult;
                this.value = costTotal;
            }
            catch (Exception ex)
            { }
        }
    }
}
