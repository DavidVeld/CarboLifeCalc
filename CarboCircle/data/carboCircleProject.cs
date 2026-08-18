using CarboLifeAPI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        //Result Data
        public List<carboCirclePair> carboCircleMatchedPairs { get; set; }
        public List<carboCircleElement> volumeOpportunities { get; set; }

        public List<carboCircleElement> leftOverData { get; set; }

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
            volumeOpportunities = new List<carboCircleElement>();

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

        /// <summary>
        /// Fills in the net quantities on the mined lists: what is left of an existing
        /// element once deconstruction has taken its share.
        /// </summary>
        private void correctMinedValues()
        {
            //Read into locals, not written back. These used to be clamped into the settings
            //object itself, which is then saved - so a deliberate 0% loss was silently
            //replaced by 25% and the figure the user typed was overwritten on disk. A
            //negative entry now simply means no loss, rather than a number nobody chose.
            double beamCutoff = settings.cutoffbeamLength > 0 ? settings.cutoffbeamLength : 0;
            double timberCutoff = settings.timberCutoffLength > 0 ? settings.timberCutoffLength : 0;
            double volumeLoss = settings.VolumeLoss > 0 ? settings.VolumeLoss : 0;

            foreach (carboCircleElement cCE in minedData)
            {
                double length = cCE.length;
                double lengthNet = length;

                if (cCE.materialClass == "Steel")
                {
                    lengthNet = length - 2 * (beamCutoff / 1000); //value cut off each side
                }
                else if(cCE.materialClass == "Wood")
                {
                    lengthNet = length - 2 * (timberCutoff / 1000); //value cut off each side
                }

                if (lengthNet < 0)
                    lengthNet = 0;

                //The allowance is worked out from the length, so with no length there is
                //nothing to work it out from and none is applied. Dividing here regardless
                //meant 0/0 for every element Revit reports no length for - columns, most of
                //the time - which put NaN into netVolume, and nothing downstream tests for
                //NaN. It reached the interface, the csv export and the report.
                double percentageCut = 1;

                if (length > 0)
                {
                    percentageCut = lengthNet / length;

                    if (percentageCut < 0)
                        percentageCut = 0;
                }

                cCE.netLength = lengthNet;
                cCE.netVolume = cCE.volume * percentageCut;

            }

            double factor = 1 - (volumeLoss / 100);

            foreach (carboCircleElement cCE in minedVolumes)
            {
                cCE.netVolume = cCE.volume * factor;
            }
        }

        /// <summary>
        /// Fills in the net quantities on the required lists.
        ///
        /// Net and gross are the same here, deliberately. A deconstruction allowance
        /// describes what survives being taken out of an existing building; a proposed
        /// element is not being taken out of anything, and the design needs the whole
        /// member and the whole volume.
        ///
        /// Both lists used to be left at zero, because importing the project side called
        /// correctMinedValues - which only ever walks the mined lists.
        /// </summary>
        private void correctRequiredValues()
        {
            foreach (carboCircleElement cCE in requiredData)
            {
                cCE.netLength = cCE.length;
                cCE.netVolume = cCE.volume;
            }

            foreach (carboCircleElement cCE in requiredVolumes)
            {
                cCE.netVolume = cCE.volume;
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
                                vRE.idList.Add(vE.id);

                                existingElement = true;

                                continue;
                            }
                        }
                        if (existingElement == false)
                        {
                            //new item
                            vE.name = vE.materialName;
                            vE.idList.Add(vE.id);

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

            correctRequiredValues();

        }

        internal void FindOpportunities()
        {
            //The cutoff lengths and the volume losses are all edited after the data has been
            //imported, so the net quantities are worked out again here rather than trusted
            //from import time. Changing an allowance and pressing Go used to match against
            //the figures from the previous settings, and the mined side only happened to
            //stay current because importing the project side recalculated it as a side
            //effect.
            correctMinedValues();
            correctRequiredValues();

            List<carboCircleElement> leftOvers = new List<carboCircleElement>();

            List<carboCirclePair> pairs = carboCircleMatchCore.findOpportunities(this, out leftOvers);

            if (pairs != null)
            {
                carboCircleMatchedPairs = pairs;
                if(leftOvers != null)
                {
                    leftOverData = leftOvers;
                }
            }

            //Asses Volumes

            List<carboCircleElement> volumeData = carboCircleMatchCore.findVolumeOpportunities(this);
            if(volumeData != null)
                volumeOpportunities = volumeData;
        }


        //collectors
        public List<carboCircleMatchElement> getCarboMatchesListSimplified()
        {
            List<carboCircleMatchElement> result = new List<carboCircleMatchElement>();

            result = carboCircleUtils.getCarboMatchListSimplified(this.carboCircleMatchedPairs);

            return result;
        }

        /// <summary>
        /// Returns the materials that could potentially be reused as a volume.
        /// </summary>
        /// <returns></returns>
        public List<carboCircleElement> getCarboVolumeOpportunities()
        {
            if (volumeOpportunities != null)
            {
                return volumeOpportunities;
            }
            else
            {
                return new List<carboCircleElement>();
            }
        }

        public List<carboCircleElement> getLeftOverData()
        {
            if(leftOverData != null)
                return leftOverData;
            else
                return new List<carboCircleElement>();
        }

    }
}
