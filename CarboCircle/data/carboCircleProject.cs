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
        public string ProjectName { get; set; }
        public string ProjectNumber { get; set; }
        public string ProjectCategory { get; set; }
        public string ProjectDescription { get; set; }

        public List<carboCircleElement> minedVolumes { get; set; }
        public List<carboCircleElement> requiredVolumes { get; set; }
        public List<carboCircleElement> minedData { get; set; }
        public List<carboCircleElement> requiredData { get; set; }
        public carboCircleSettings settings { get; set; }

        public carboCircleProject() 
        {
            ProjectName = "New Project";
            ProjectNumber = "000";
            ProjectCategory = "";
            ProjectDescription = "";



            minedData = new List<carboCircleElement>();
            minedVolumes = new List<carboCircleElement>();
            minedData = new List<carboCircleElement>();
            requiredData = new List<carboCircleElement>();

            settings = new carboCircleSettings();
        }


    }
}
