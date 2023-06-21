using System;

namespace CarboLifeUI
{
    internal class LetiScore
    {
        internal string BuildingType { get; set; }
        internal bool sequestration { get; set; }
        internal int AAA { get; set; }
        internal int AA { get; set; }
        internal int A { get; set; }
        internal int B { get; set; }
        internal int C { get; set; }
        internal int D { get; set; }
        internal int E { get; set; }
        internal int F { get; set; }
        internal int G { get; set; }


        internal LetiScore()
        {
            BuildingType = "";
            sequestration = true;
            AAA = 0;
            AA = 0;
            A = 0;
        }

        internal int getValue(string searchValue)
        {
            switch (searchValue)
            {
                case "A++": 
                    return AAA;
                case "A+": 
                    return AA;
                case "A": 
                    return A;
                case "B": 
                    return B;
                case "C": 
                    return C;
                case "D": 
                    return D;
                case "E": 
                    return E;
                case "F": 
                    return F;
                case "G": 
                    return G;
                default: 
                    return 0;
            }
        }

        //this Will retun the value Above the one next to the given
        internal int getValueOneUp(string searchValue)
        {
            switch (searchValue)
            {
                case "A++":
                    return AAA;
                case "A+":
                    return AAA;
                case "A":
                    return AA;
                case "B":
                    return A;
                case "C":
                    return B;
                case "D":
                    return C;
                case "E":
                    return D;
                case "F":
                    return E;
                case "G":
                    return F;
                default:
                    return 0;
            }
        }
    }
}