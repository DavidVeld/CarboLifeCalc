using CarboLifeAPI.Data;
using System;
using System.Collections;
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
        public List<carboCirclePair> carboCircleMatchedPairs { get; set; }
        public carboCircleSettings settings { get; set; }

        public carboCircleProject() 
        {
            ProjectName = "New Project";
            ProjectNumber = "000";
            ProjectCategory = "";
            ProjectDescription = "";

            minedData = new List<carboCircleElement>();
            minedVolumes = new List<carboCircleElement>();
            requiredData = new List<carboCircleElement>();
            requiredVolumes = new List<carboCircleElement>();
            carboCircleMatchedPairs = new List<carboCirclePair>();

            settings = new carboCircleSettings();
        }

        /// <summary>
        /// Pursed a raw datalist to a sorted list, massobjcts separated from data
        /// </summary>
        /// <param name="collectedElements"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void ParseMinedData(List<carboCircleElement> collectedElements)
        {
            minedData.Clear();
            minedVolumes.Clear();
            List<carboCircleElement> minedVolumeBuffer = new List<carboCircleElement>();


            foreach (carboCircleElement element in collectedElements)
            {
                try
                {
                    if(element.isVolumeElement)
                    {
                        minedVolumeBuffer.Add(element.Copy());
                    }
                    else
                    {
                        minedData.Add(element.Copy());
                    }
                }
                catch
                { }
            }

            //combine all volumdata to single;
            minedVolumes = combineByMaterialName(minedVolumeBuffer);

            correctMinedValues();
        }

        private void correctMinedValues()
        {
            if (settings.cutoffbeamLength < 0)
                settings.cutoffbeamLength = 500;

            if (settings.VolumeLoss <= 0)
                settings.VolumeLoss = 25;


            foreach (carboCircleElement cCE in minedData)
            {
                double percentageCut = 0;
                double length  = cCE.length;
                double lengthNet = cCE.length - (settings.cutoffbeamLength / 1000); ;

                if (lengthNet < 0)
                    lengthNet = 0;

                percentageCut = lengthNet / length;
                if (percentageCut < 0)
                    percentageCut = 0;

                cCE.netLength = lengthNet;
                cCE.netVolume = cCE.volume * percentageCut;

            }

            double factor = 1 - (Convert.ToDouble(settings.VolumeLoss) / 100);

            foreach (carboCircleElement cCE in minedVolumes)
            {
                cCE.netVolume = cCE.volume * factor;
            }
        }

        private List<carboCircleElement> combineByMaterialName(List<carboCircleElement> minedVolumeBuffer)
        {
            List<carboCircleElement> result = new List<carboCircleElement>();

            foreach(carboCircleElement vE in minedVolumeBuffer)
            {
                if(vE.isVolumeElement)
                {
                    try
                    {
                        bool existingElement = false;
                        foreach (carboCircleElement vRE in result)
                        {
                            if (vE.materialName == vRE.materialName)
                            {
                                vRE.id = 0;
                                vRE.GUID += ";" + vE.GUID;
                                vRE.humanId += ";" + vE.humanId;

                                vRE.category += ";" + vE.category;
                                vRE.volume += vE.volume;
                                vRE.netVolume += vE.netVolume;
                                existingElement = true;
                                continue;
                            }
                        }
                        if (existingElement == false)
                        {
                            vE.name = vE.materialName;
                            result.Add(vE.Copy());
                        }
                    }
                    catch 
                    { 
                    }
                }
            }

            return result;


        }

        internal void ParseRequiredData(List<carboCircleElement> collectedElements)
        {
            requiredData.Clear();
            requiredVolumes.Clear();
            List<carboCircleElement> requiredVolumeBuffer = new List<carboCircleElement>();


            foreach (carboCircleElement element in collectedElements)
            {
                try
                {
                    if (element.isVolumeElement)
                    {
                        requiredVolumeBuffer.Add(element.Copy());
                    }
                    else
                    {
                        requiredData.Add(element.Copy());
                    }
                }
                catch
                { }
            }

            //combine all volumdata to single;
            requiredVolumes = combineByMaterialName(requiredVolumeBuffer);

            correctMinedValues();

        }

        internal void FindOpportunities()
        {
            carboCircleProject result = new carboCircleProject();

            List<carboCirclePair> pairs = carboCircleMatchCore.findOpportunitiesV2(this);

            if(pairs != null)
                carboCircleMatchedPairs = pairs;
        }

        public List<carboCircleMatchElement> getCarboMatchesListSimplified()
        {
            List<carboCircleMatchElement> result = new List<carboCircleMatchElement>();

            result = carboCircleUtils.getCarboMatchListSimplified(this.carboCircleMatchedPairs);

            return result;
        }


    }
}
