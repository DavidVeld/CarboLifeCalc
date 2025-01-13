using System;

namespace CarboCircle.data
{

    [Serializable]
    public class carboCirclePair
    {

        public carboCircleElement required_element { get; set; }
        public carboCircleElement mined_Element { get; set; }
        public double match_Score { get; set; }
        public string description { get; set; }

        public carboCirclePair()
        {
            required_element = new carboCircleElement();
            mined_Element = new carboCircleElement();
            match_Score = 0;
            description = string.Empty;
        }

        public carboCirclePair(carboCircleElement requiredElement, carboCircleElement minedElement, double matchScore = 0, string description = "")
        {
            this.required_element = requiredElement.Copy();
            this.mined_Element = minedElement.Copy();
            this.match_Score = matchScore;
            this.description = description;
        }

        /// <summary>
        /// Returns the remaining part as an offcut
        /// </summary>
        /// <returns></returns>
        internal carboCircleElement getOffcut()
        {
            double delta = mined_Element.netLength - required_element.length;
            if (delta <= 0)
                return null;

            carboCircleElement offcut = mined_Element.Copy();
            offcut.isOffcut = true;

            offcut.name += "_Offcut";
            offcut.GUID += "_OC";
            //offcut.id = "_OC";
            offcut.humanId += "_OC";
            offcut.netLength = mined_Element.netLength - required_element.length;
            offcut.length = mined_Element.length - required_element.length;

            //linear correction
            offcut.volume = offcut.volume * (required_element.length / mined_Element.netLength);

            return offcut;
        }
        public carboCirclePair Copy()
        {
            carboCirclePair clone = new carboCirclePair();
            clone = new carboCirclePair
            {
                required_element = required_element.Copy(),
                mined_Element = this.mined_Element.Copy(),
                match_Score = this.match_Score,
                description = this.description 
                
            };
            return clone;
        }


    }
}