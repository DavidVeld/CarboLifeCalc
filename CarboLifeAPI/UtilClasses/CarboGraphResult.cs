using CarboLifeAPI.Data;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarboLifeAPI
{
    /// <summary>
    /// This class is used to store the information to colour in the model in Revit.
    /// </summary>
    public class CarboGraphResult
    {
        public string ValueName { get; set; }
        public string Unit { get; set; }
        public string ColourLegendName { get; set; }

        /// <summary>
        /// This paramater owns all the data for import to Revit
        /// </summary>
        public IList<CarboValues> entireProjectData;

        /// <summary>
        /// This paramater owns all the visible elements
        /// </summary>
        public IList<CarboValues> selectedData;
        /// <summary>
        /// This paramater owns all the data which lower than the given max
        /// </summary>
        public IList<CarboValues> notSelectedData;


        /// <summary>
        /// This paramater owns all the data thatFits the filter criteria
        /// </summary>
        public IList<CarboValues> validData;

        /// <summary>
        /// This paramater owns all the data which higher than the given max
        /// </summary>
        public IList<CarboValues> outOfBoundsMaxData;

        /// <summary>
        /// This paramater owns all the data which lower than the given max
        /// </summary>
        public IList<CarboValues> outOfBoundsMinData;



        public double max;

        public double min;

        public CarboGraphResult()
        {
            entireProjectData = new List<CarboValues>();
            validData = new List<CarboValues>();
            outOfBoundsMaxData = new List<CarboValues>();
            outOfBoundsMinData = new List<CarboValues>();
            notSelectedData = new List<CarboValues>();
            selectedData = new List<CarboValues>();
            ColourLegendName = "CLC_ColourLegend";
            Unit = "";
            max = double.PositiveInfinity;
            min = double.NegativeInfinity;
        }

        /// <summary>
        /// This method will filter the non-usable elements from the project;
        /// This is highest level filter and should always create a full filter sequence.
        /// </summary>
        /// <param name="applicablelements">This is a list of elements in the project that needs to be considered, if no elements are selected, the entire project is looked at</param>
        public void FilterNonVisible(List<int> applicablelements)
        {
            try
            {
                if (entireProjectData == null || entireProjectData.Count <= 0)
                    throw new Exception("The project has no data, we cannot filter");

                //If the list send is not present or empty then the entire range needs to be considered
                if (applicablelements == null || applicablelements.Count == 0)
                {
                    List<int> listOfAllElementIds = entireProjectData.Select(i => i.Id).Distinct().ToList();
                    if (listOfAllElementIds.Count > 0)
                    {
                        applicablelements = listOfAllElementIds;
                    }
                }

                if (Utils.IsEmpty(applicablelements))
                    throw new Exception("Could not collect visible or selected elements from the given dataset");

                //We will now collect the elements with said id:
                validData.Clear();
                validData = new List<CarboValues>();
                notSelectedData = new List<CarboValues>();

                foreach (CarboValues cv in entireProjectData)
                {
                    bool elementIsVisible = false;
                    //Check if element is visible:
                    elementIsVisible = applicablelements.Contains(cv.Id);

                    if (elementIsVisible == true)
                    {
                        selectedData.Add(cv);
                    }
                    else
                    {
                        notSelectedData.Add(cv);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// This is the second level of data where we filter the min and max out of the list
        /// </summary>
        /// <param name="_minCutoff">The min value</param>
        /// <param name="_maxCutoff">The Maximum Value</param>
        public void FilterMinMax(double minCutoff, double maxCutoff)
        {
            List<CarboValues> bufferListofValidData = new List<CarboValues>();
            outOfBoundsMinData = new List<CarboValues>();
            outOfBoundsMaxData = new List<CarboValues>();

            min = minCutoff;
            max = maxCutoff;

            try
            {
                if (selectedData == null || selectedData.Count <= 0)
                    throw new Exception("The project has no data, we cannot filter");


                foreach(CarboValues cv in selectedData)
                {
                    if (cv.Value < min) // too low
                        outOfBoundsMinData.Add(cv);
                    else if (cv.Value > max) // too high
                        outOfBoundsMaxData.Add(cv);
                    else // Value falls within range: (cv.Value > minCutoff && cv.Value < maxCutoff)
                    {
                        bufferListofValidData.Add(cv);
                    }
                }

                //We can now use the bufferList for the valid Values.
                validData.Clear();
                validData = bufferListofValidData;

            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        public double getMaxValue()
        {
            //First we need to make sure the list is sorted;
            //If the project was just loaded the valid data has no data, the selected dataset will then be used;
            List<CarboValues> SortedList = new List<CarboValues>();

            try
            {
                if (validData.Count > 0 && selectedData.Count > 0)
                {
                    //
                    SortedList = validData.OrderBy(o => o.Value).ToList();
                }
                else if (validData.Count < 0 && selectedData.Count > 0)
                {
                    SortedList = selectedData.OrderBy(o => o.Value).ToList();
                }
                else
                {
                    SortedList = entireProjectData.OrderBy(o => o.Value).ToList();
                }

                if (SortedList.Count > 0)
                    return (SortedList[SortedList.Count - 1].Value);
                else
                    return 9999;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return 9999;
            }
        }

        public double getMinValue()
        {
            //First we need to make sure the list is sorted;
            //If the project was just loaded the valid data has no data, the selected dataset will then be used;
            List<CarboValues> SortedList = new List<CarboValues>();
            try
            {
                if (validData.Count > 0)
                {
                    SortedList = validData.OrderBy(o => o.Value).ToList();
                }
                else if (validData.Count > 0 && selectedData.Count > 0)
                {
                    SortedList = selectedData.OrderBy(o => o.Value).ToList();
                }
                else
                {
                    SortedList = entireProjectData.OrderBy(o => o.Value).ToList();
                }

                if (SortedList.Count > 0)
                    return (SortedList[0].Value);
                else
                    return -99999;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -99999;
            }

        }

        public List<double> GetUniqueValues()
        {
            List<double> thisResult = new List<double>();

            try
            {
                if (validData != null)
                    thisResult = validData.Select(x => Math.Round(x.Value, 3)).Distinct().ToList();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

            return thisResult;

        }

    }

}
