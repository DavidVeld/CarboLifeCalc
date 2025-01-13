using Autodesk.Revit.DB;
using CarboLifeAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboCircle
{
    [Serializable]
    public class carboCircleMatchElement
    {

        public int required_id { get; set; }
        public string required_humanId { get; set; }

        public int mined_id { get; set; }
        public string mined_humanId { get; set; }
        public string required_Name { get; set; }
        public string mined_Name { get; set; }

        //Matched To Material Name
        public double required_length { get; set; }
        public double required_volume { get; set; }
        public double mined_netLength { get; set; }
        public double mined_netVolume { get; set; }

        public bool isVolumeElement { get; set; }
        public bool isOffcut { get; set; }

        //The Below are taken from standardized Database
        public string required_standardName { get; set; }
        public string mined_standardName { get; set; }
        public double match_Score { get; set; }

        public string description { get; set; }


        public carboCircleMatchElement(
        int requiredId = 0,
        string requiredHumanId = null,
        int minedId = 0,
        string minedHumanId = null,
        string requiredName = null,
        string minedName = null,
        double requiredLength = 0.0,
        double requiredVolume = 0.0,
        double minedNetLength = 0.0,
        double minedNetVolume = 0.0,
        double match_Score = 0.0,
        bool isOffcut = false,
        bool isVolumeElement = false,
        string requiredStandardName = null,
        string minedStandardName = null,         
        string description = null)
        {
            required_id = requiredId;
            required_humanId = requiredHumanId;
            mined_id = minedId;
            mined_humanId = minedHumanId;
            required_Name = requiredName;
            mined_Name = minedName;
            required_length = requiredLength;
            required_volume = requiredVolume;
            mined_netLength = minedNetLength;
            mined_netVolume = minedNetVolume;
            this.isOffcut = isOffcut;
            this.isVolumeElement = isVolumeElement;
            required_standardName = requiredStandardName;
            mined_standardName = minedStandardName;
            this.description = description;
        }

        public carboCircleMatchElement Copy()
        {
            carboCircleMatchElement clone = new carboCircleMatchElement
            {
                required_id = this.required_id,
                required_humanId = this.required_humanId,
                mined_id = this.mined_id,
                mined_humanId = this.mined_humanId,
                required_Name = this.required_Name,
                mined_Name = this.mined_Name,
                required_length = this.required_length,
                required_volume = this.required_volume,
                mined_netLength = this.mined_netLength,
                mined_netVolume = this.mined_netVolume,
                isVolumeElement = this.isVolumeElement,
                isOffcut = this.isOffcut,
                required_standardName = this.required_standardName,
                mined_standardName = this.mined_standardName,
                match_Score = this.match_Score,
                description = this.description
            };

            return clone;
        }
    }
}

