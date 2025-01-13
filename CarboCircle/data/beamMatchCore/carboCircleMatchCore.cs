using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace CarboCircle.data
{
    internal class carboCircleMatchCore
    {
        /// <summary>
        /// Main entry point, this gets a list of matches based on scoring system and returns a pairing list.
        /// </summary>
        /// <param name="carboCircleProject"></param>
        /// <returns></returns>
        internal static List<carboCirclePair> findOpportunitiesV2(carboCircleProject carboCircleProject)
        {
            //Rest the matched
            foreach(carboCircleElement ccE in carboCircleProject.minedData)
            {
                ccE.matchGUID = "";
            }

            foreach (carboCircleElement ccE in carboCircleProject.requiredData)
            {
                ccE.matchGUID = "";
            }

            //Get All Data From Project
            List<carboCircleElement> existingBeamList = new List<carboCircleElement>();
            List<carboCircleElement> requiredBeamList = new List<carboCircleElement>();
            List<carboCircleElement> offcuts = new List<carboCircleElement>();
            List<carboCircleElement> finalOffcuts = new List<carboCircleElement>();

            List<carboCirclePair> matchedpairs = new List<carboCirclePair>();

            //get valid steel beams:
            foreach(carboCircleElement ccE in carboCircleProject.minedData)
            {
                if(ccE.materialClass == "Steel")
                {
                    existingBeamList.Add(ccE); 
                }
            }

            foreach (carboCircleElement ccE in carboCircleProject.requiredData)
            {
                if (ccE.materialClass == "Steel")
                {
                    requiredBeamList.Add(ccE);
                }
            }

            //valid Data Wasnt collected
            if (existingBeamList.Count <= 0 || requiredBeamList.Count <= 0)
                return null;

            //Iterate through required Data and find matches.

            foreach (carboCircleElement req_el in requiredBeamList)
            {
                carboCirclePair matchedPairSeries = new carboCirclePair();
                try
                {
                    matchedPairSeries = getBestMatchingPair(req_el, existingBeamList, carboCircleProject.settings);
                    //create offcut from pair:
                    if (matchedPairSeries != null)
                    {
                        req_el.matchGUID = matchedPairSeries.mined_Element.GUID;
                        
                        //reove from the list as it cannot be considered twice:
                        for(int i = 0; i < existingBeamList.Count; i++)
                        {
                            carboCircleElement mined_el = existingBeamList[i];
                            if (mined_el.GUID == matchedPairSeries.mined_Element.GUID)
                                existingBeamList.RemoveAt(i);
                            break;
                        }
                        /*
                        foreach (carboCircleElement mined_el in existingBeamList)
                        {

                            break;
                        }
                        */

                        matchedpairs.Add(matchedPairSeries);
                        carboCircleElement offcut = matchedPairSeries.getOffcut();
                        
                        if(offcut != null)
                            finalOffcuts.Add(offcut);
                    }
                }
                catch
                {                 
                }


            }

            //At the end go through all the offcuts, 
            foreach (carboCircleElement req_el in requiredBeamList)
            {
                //a matched existing element needs to be skipped
                if (req_el.matchGUID != "")
                    continue;

                carboCirclePair matchedPairSeries = new carboCirclePair();

                try
                {
                    matchedPairSeries = getBestMatchingPair(req_el, offcuts, carboCircleProject.settings);
                    //create offcut from pair:
                    if (matchedPairSeries != null)
                    {
                        req_el.matchGUID = matchedPairSeries.mined_Element.GUID;

                        //reove from the list as it cannot be considered twice:
                        for (int i = 0; i < offcuts.Count; i++)
                        {
                            carboCircleElement mined_el = offcuts[i];
                            if (mined_el.GUID == matchedPairSeries.mined_Element.GUID)
                                offcuts.RemoveAt(i);
                            break;
                        }

                        matchedpairs.Add(matchedPairSeries);
                        carboCircleElement offcut = matchedPairSeries.getOffcut();

                        if (offcut != null)
                            offcuts.Add(offcut);
                    }
                }
                catch
                { }
            }

            foreach (carboCircleElement offcut in finalOffcuts)
            {
                //a matched existing element needs to be skipped
                if (offcut.matchGUID != "")
                    continue;

                carboCirclePair matchedPairSeries = new carboCirclePair();

                try
                {
                    matchedPairSeries.mined_Element = offcut.Copy();
                    matchedPairSeries.required_element = new carboCircleElement();
                    matchedPairSeries.required_element.length = 0;
                    matchedPairSeries.description = "Offcut Unused";
                    matchedPairSeries.match_Score = 0;

                    matchedpairs.Add(matchedPairSeries);

                }
                catch
                { }
            }



            return matchedpairs;
        }

        /// <summary>
        /// A clever matching script to be improved that finds a match of a beam within a list, returns null if no match is found 
        /// </summary>
        /// <param name="req_el"></param>
        /// <param name="existingBeamList"></param>
        /// <param name="settings"></param>
        /// <returns>returns best matching beam within a list, null oif no suitable match is found</returns>
        private static carboCirclePair getBestMatchingPair(carboCircleElement req_el, List<carboCircleElement> existingBeamList, carboCircleSettings settings)
        {
            carboCirclePair matchedPairSeries = new carboCirclePair();
            double BestScore = 0;

            foreach (carboCircleElement ccE_mined in existingBeamList)
            {
                //if an existing beam was matched, continue.
                if (ccE_mined.matchGUID != "")
                    continue;
                //find a match
                try
                {
                    string description = "";

                    double pairScore = getScore(req_el, ccE_mined, settings,out description);
                    if (pairScore > 0)
                    {
                        if (pairScore > BestScore)
                        {
                            //new Best Pair:
                            matchedPairSeries = new carboCirclePair(req_el.Copy(), ccE_mined.Copy(), pairScore,description);
                            BestScore = pairScore;
                        }
                    }
                }
                catch
                { }
            }

            if (BestScore == 0)
                return null;
            else
                return matchedPairSeries;
        }

        private static double getScore(carboCircleElement req_el, carboCircleElement mined_el, carboCircleSettings settings, out string description)
        {
            double score = 0;
            double d_value = settings.depthRange; //mm
            double str_factor = settings.strengthRange / 100; //mm

            //If the mined beam isnt long wnough skip any other form of matching;
            double l_mined = mined_el.netLength;
            double l_required = req_el.length;

            double offcutlength = l_mined - l_required;
            double lengthFactor = 1;
            description = "";

            if (offcutlength < 0)
            {
                description = "No Match.";
                return 0;
            }
            else
            {
                //Percentage of beam used: 100% usage would return a factor 1 here:
                //The smaller the offcut the larger the factor = smaller correction of the total score.
                lengthFactor = l_required / l_mined;
            }

            //factors considered:
            //type name:
            if (req_el.standardName == mined_el.standardName)
            {
                score = 500; //This section size matched exactly the required one: 100 points
                description = "Full Match.";

            }
            else
            {
                //cutoff values:
                double d_max = req_el.standardDepth + d_value;
                double d_mined = mined_el.standardDepth;
                double d_required = req_el.standardDepth;

                double str_Iy_max = req_el.Iy + (req_el.Iy * str_factor);
                double str_Wy_max = req_el.Wy + (req_el.Wy * str_factor);
                double str_Iz_max = req_el.Iz + (req_el.Iz * str_factor);
                double str_Wz_max = req_el.Wz + (req_el.Wz * str_factor);

                double d_score = 0;
                double Iy_score = 0;
                double Wy_score = 0;
                double Iz_score = 0;
                double Wz_score = 0;

                //0 would mean too big
                if (d_mined > d_required && d_mined <= d_max)
                {
                    d_score = calcFactoredScore(d_max, d_required, d_mined);
                }

                //strength:
                if (mined_el.Iy > req_el.Iy && mined_el.Iy <= str_Iy_max)
                {
                    Iy_score = calcFactoredScore(str_Iy_max, req_el.Iy, mined_el.Iy);
                }

                if (mined_el.Wy > req_el.Wy && mined_el.Wy <= str_Wy_max)
                {
                    Wy_score = calcFactoredScore(str_Wy_max, req_el.Wy, mined_el.Wy);
                }

                if (mined_el.Iz > req_el.Iz && mined_el.Iz <= str_Iz_max)
                {
                    Iz_score = calcFactoredScore(str_Iz_max, req_el.Iz, mined_el.Iz);
                }
                if (mined_el.Wz > req_el.Wz && mined_el.Wz <= str_Wz_max)
                {
                    Wz_score = calcFactoredScore(str_Wz_max, req_el.Wz, mined_el.Wz);
                }

                //All Values need to pass, not only one.
                if(d_score <= 0 || Iy_score <= 0 || Wy_score <= 0 || Iz_score <= 0 || Wz_score <= 0)
                    score = 0;
                else
                {
                    score = d_score + Iy_score + Wy_score + Iz_score + Wz_score; //Max 500
                    description = "Partial match.";

                }
            }

            if (score > 0)
            {
                description += " Offcut length: " + offcutlength + " mm";
            }

            //returns value if within bounds, if out of bounds returns 0;
            return score * lengthFactor;

        }

        private static double calcFactoredScore(double value_max, double value_required, double value_mined)
        {
            double result = 0;
            
            //get scale:
            double v_delta = value_max - value_required;
            double v_scale = v_delta / 100;

            double value_minedDelta = value_mined - value_required;

            result = 100 - (value_minedDelta * v_scale);

            return result;
        }
    }
}
