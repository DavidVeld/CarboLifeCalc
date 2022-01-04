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
    }
}