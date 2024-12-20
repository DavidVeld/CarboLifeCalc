using System;

namespace CarboCircle.data
{

    [Serializable]
    public class carboCirclePair
    {

        public carboCircleElement required_element { get; set; }
        public carboCircleElement mined_Element { get; set; }
        public double match_Score { get; set; }

        public carboCirclePair()
        {
            required_element = new carboCircleElement();
            mined_Element = new carboCircleElement();
            match_Score = 0;
        }

        public carboCirclePair(carboCircleElement requiredElement, carboCircleElement minedElement, double matchScore = 0)
        {
            this.required_element = requiredElement;
            this.mined_Element = minedElement;
            this.match_Score = matchScore;
        }

        public carboCirclePair Copy()
        {
            carboCirclePair clone = new carboCirclePair();
            clone = new carboCirclePair
            {
                required_element = required_element.Copy(),
                mined_Element = this.mined_Element.Copy(),
                match_Score = this.match_Score
                
            };
            return clone;
        }
    }
}