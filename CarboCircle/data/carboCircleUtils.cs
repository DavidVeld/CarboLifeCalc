using System;
using System.Collections.Generic;

namespace CarboCircle.data
{
    internal class carboCircleUtils
    {
        [Obsolete]
        internal static carboCircleProject findOpportunities(carboCircleProject carboCircleProject)
        {
            //needs to include offcuts, basically each element is mapped to mtch 
            foreach (carboCircleElement req_el in carboCircleProject.requiredData)
            {
                try
                {
                    foreach (carboCircleElement min_el in carboCircleProject.minedData)
                    {
                        if (req_el.standardName == min_el.standardName)
                        {
                            //the profile matches
                            if (req_el.length <= min_el.netLength && min_el.matchGUID == "")
                            {
                                //match
                                req_el.matchGUID = min_el.GUID;
                                min_el.matchGUID = req_el.GUID;

                                carboCirclePair pair = new carboCirclePair();
                                pair.required_element = req_el.Copy();
                                pair.mined_Element = min_el.Copy();
                                pair.match_Score = 100;

                                carboCircleProject.carboCircleMatchedPairs.Add(pair);

                                break;//next required element;

                            }
                        }
                    }
                }
                catch
                { }
            }

            return carboCircleProject;
        }


        internal static List<carboCircleMatchElement> getCarboMatchListSimplified(List<carboCirclePair> carboCircleMatchedPairs)
        {
            List<carboCircleMatchElement> result = new List<carboCircleMatchElement>();

            if(carboCircleMatchedPairs != null )
            {
                if(carboCircleMatchedPairs.Count > 0) 
                { 
                    foreach(carboCirclePair pair in carboCircleMatchedPairs)
                    {
                        carboCircleMatchElement ccme
                            = new carboCircleMatchElement();

                        //convert the pairs to simplified data
                        //required Element
                        ccme.required_id = pair.required_element.id;
                        ccme.required_Name = pair.required_element.name;
                        ccme.required_standardName = pair.required_element.standardName;
                        ccme.required_length = pair.required_element.length;

                        //mined elemnent
                        ccme.mined_id = pair.mined_Element.id;
                        ccme.mined_Name = pair.mined_Element.name;
                        ccme.mined_standardName = pair.mined_Element.standardName;
                        ccme.mined_netLength = pair.mined_Element.netLength;

                        ccme.match_Score = pair.match_Score;

                        ccme.description = pair.description;

                        result.Add(ccme);
                    }
                }
            }

            return result;


        }
    }
}