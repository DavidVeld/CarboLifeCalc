using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboCircle.data
{
    [Serializable]
    public class carboCircleProject
    {
        public static List<carboCircleElement> availableElements { get; set; }
        public static List<carboCircleElement> proposedElements { get; set; }
        public carboCircleSettings settings { get; set; }

        public carboCircleProject() 
        {
            availableElements = new List<carboCircleElement>();
            proposedElements = new List<carboCircleElement>();
            settings = new carboCircleSettings();
        }


    }
}
